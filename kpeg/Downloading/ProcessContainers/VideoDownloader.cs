using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using kpeg.Properties;

namespace kpeg.Downloading.ProcessContainers
{
    public class VideoDownloader
    {
        private static VideoDownloader VideoDownloaderInstance;
        private int currentItem, maxItems;
        private readonly string CurrentURL = string.Empty;
        private Task VideoDownloadTask;

        private VideoDownloader()
        {
            VideoDownloadTask = new Task(() => { });
        }

        public static VideoDownloader GetInstance()
        {
            if (VideoDownloaderInstance == null)
                VideoDownloaderInstance = new VideoDownloader();
            return VideoDownloaderInstance;
        }

        private async Task DownloadVideoTask(string url, int clipStart, int clipEnd)
        {
            DownloadWindow.GetInstance().DisableDownloadChildren();
            Settings.Default.Save();
            ProcessStartInfo info = new ProcessStartInfo();
            info.FileName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\Resources\\yt-dlp.exe";
            bool audioOnly = (bool)SettingsManager.Get(Setting.DownloadAudioOnly);
            bool openDirectory = (bool)SettingsManager.Get(Setting.OpenDirectoryAfterDownload);
            bool convertToMp3 = (bool)SettingsManager.Get(Setting.DownloadAudioAsWav);
            bool convertToMp4 = (bool)SettingsManager.Get(Setting.ConvertToMp4);
            bool isPlaylist = Utils.IsPlayList(url);
            bool downloadClip = (bool)SettingsManager.Get(Setting.DownloadClip);
            currentItem = 0;
            maxItems = 0;
            lastProgress = -1;
            if (audioOnly)
            {
                info.Arguments += "-f \"bestaudio\" -x";
                if (convertToMp3)
                    info.Arguments += " --audio-format wav";
            }
            else if(convertToMp4)
            {
                info.Arguments += "-S vcodec:h264,res,acodec:m4a";
            }
            if ((bool)SettingsManager.Get(Setting.SetModifiedDate))
                info.Arguments += " --no-mtime";

            if (downloadClip)
            {
                TimeSpan clipStartSpan = TimeSpan.FromSeconds(clipStart);
                TimeSpan clipEndSpan = TimeSpan.FromSeconds(clipEnd);
                info.Arguments += $" --download-sections *{clipStartSpan.Hours}:{clipStartSpan.Minutes}:{clipStartSpan.Seconds}-{clipEndSpan.Hours}:{clipEndSpan.Minutes}:{clipEndSpan.Seconds} --force-keyframes-at-cuts";
            }

            info.Arguments += " " + url;
            info.WorkingDirectory = (string)SettingsManager.Get(Setting.DownloadDirectory);
            info.CreateNoWindow = true;
            info.UseShellExecute = false;
            info.RedirectStandardOutput = true;
            info.RedirectStandardError = true;
            Application.Current.Dispatcher.Invoke(() => { TerminalWindow.GetInstance().VideoDownloaderView.TextBox.AppendText(info.WorkingDirectory+"> "+info.FileName+" "+info.Arguments + '\n'); });
            string output = "";
            using (Process p = new Process())
            {
                p.StartInfo = info;

                p.OutputDataReceived += (s, e) =>
                {
                    if (e.Data == null)
                        return;
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        TerminalWindow.GetInstance().VideoDownloaderView.TextBox.AppendText(e.Data.Trim() + '\n');
                    });
                    processDownloadProgressString(e.Data, isPlaylist, audioOnly, clipEnd - clipStart);
                    output += e.Data+'\n';
                };
                p.ErrorDataReceived += (s, e) =>
                {
                    if (e.Data == null)
                        return;
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        TerminalWindow.GetInstance().VideoDownloaderView.TextBox.AppendText(e.Data.Trim() + '\n');
                    });
                    processDownloadProgressString(e.Data, isPlaylist, audioOnly, clipEnd - clipStart);
                    output += e.Data+'\n';
                };
                p.Start();
                p.BeginErrorReadLine();
                p.BeginOutputReadLine();
                p.WaitForExit();
            }

            DownloadWindow.GetInstance().EnableDownloadChildren();
            if (openDirectory)
                Process.Start(info.WorkingDirectory);
        }

        double lastProgress = -1;
        private void processDownloadProgressString(string str, bool isPlaylist, bool audioOnly, int durationSeconds)
        {
            if (str == null)
                return;
            double progress = Utils.DownloadProgressStringToDouble(str,TimeSpan.FromSeconds(durationSeconds));
            if (progress == -1 && lastProgress > 95)
                progress = 100;
            if (progress == -1)
            {
                if (str.Contains("[download] Downloading item "))
                {
                    int ofIndex = str.IndexOf(" of ");
                    maxItems = int.Parse(str.Substring(ofIndex + 4));
                    int itemIndex = str.IndexOf("item");
                    currentItem = int.Parse(str.Substring(itemIndex + 4,
                        str.Length - itemIndex - 4 - (str.Length - ofIndex)));
                }

                return;
            }
            if (lastProgress == progress&&progress>0&&progress<100)
                return;
            lastProgress = progress;

            MainWindow.GetInstance().Dispatcher.Invoke(() =>
            {
                DownloadWindow.GetInstance().GetProgressBar().Value = progress;
                if (!isPlaylist && currentItem == maxItems)
                {
                    if (progress == 100 || str.IndexOf("100% of") > -1)
                        DownloadWindow.GetInstance().GetProgressBar().Foreground = Brushes.Green;
                    else
                        DownloadWindow.GetInstance().GetProgressBar().Foreground = MainWindow.GetInstance().mainBrush;
                }

                DownloadWindow.GetInstance().GetProgressBarLabel().Content = audioOnly?"Downloading audio":"Downloading video";
                if (isPlaylist)
                    if (currentItem > 0 && maxItems > 0)
                        DownloadWindow.GetInstance().GetProgressBarLabel().Content +=
                            string.Format(" (item {0} of {1})", currentItem,
                                maxItems); //TODO: green doesnt work, converting text does not work
            });
        }

        public async Task DownloadVideo(string url, int clipStart, int clipEnd)
        {
            if (!Utils.IsLinkValid(url)) return;
            if (CurrentURL == url)
                return;
            if (VideoDownloadTask.Status != TaskStatus.RanToCompletion &&
                VideoDownloadTask.Status != TaskStatus.Created)
            {
                VideoDownloadTask.Wait();
                await DownloadVideo(url,clipStart,clipEnd);
                return;
            }

            if (YTdlpUpdater.GetInstance().GetTask().Status != TaskStatus.RanToCompletion &&
                YTdlpUpdater.GetInstance().GetTask().Status != TaskStatus.Created)
            {
                YTdlpUpdater.GetInstance().GetTask().Wait();
                await DownloadVideo(url,clipStart,clipEnd);
                return;
            }

            VideoDownloadTask = Task.Run(() => DownloadVideoTask(url, clipStart, clipEnd));
        }
    }
}