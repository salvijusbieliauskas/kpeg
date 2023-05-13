using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace kpeg.ProcessContainers
{
    public class VideoDownloader
    {
        private static VideoDownloader VideoDownloaderInstance;
        private Task VideoDownloadTask;
        private VideoDownloader()
        {
            VideoDownloadTask = new Task(() => { });
        }
        public static VideoDownloader GetInstance()
        {
            if(VideoDownloaderInstance == null)
                VideoDownloaderInstance = new VideoDownloader();
            return VideoDownloaderInstance;
        }
        private string CurrentURL = string.Empty;
        private async Task DownloadVideoTask(string url)
        {
            DownloadWindow.GetInstance().DisableDownloadChildren();
            Properties.Settings.Default.Save();
            ProcessStartInfo info = new ProcessStartInfo();
            info.FileName = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Resources\\yt-dlp.exe";
            bool audioOnly = Properties.Settings.Default.downloadAudioOnly;
            bool openDirectory = Properties.Settings.Default.openDirectoryAfterDownload;
            bool convertToMp3 = Properties.Settings.Default.downloadAudioAsMp3;
            bool convertToMp4 = Properties.Settings.Default.convertToMp4;
            bool isPlaylist = Utils.isPlayList(url);
            bool downloadClip = Properties.Settings.Default.downloadClip;
            info.Arguments = "";
            if (audioOnly)
            {
                info.Arguments += "-f \"bestaudio\" -x";
                if (convertToMp3)
                    info.Arguments += " --audio-format wav";
            }
            else if (convertToMp4)
                info.Arguments += " -f \"bestvideo[ext=mp4]+bestaudio[ext=m4a]/best[ext=mp4]/best\"";
            if (Properties.Settings.Default.setModifiedDate)
                info.Arguments += " --no-mtime";
            if (downloadClip)
            {
                MainWindow.GetInstance().Dispatcher.Invoke((Action)(() =>
                {
                    int from = int.Parse(DownloadWindow.GetInstance().GetStartSecBox().Text) + int.Parse(DownloadWindow.GetInstance().GetStartMinBox().Text) * 60 + int.Parse(DownloadWindow.GetInstance().GetStartHourBox().Text) * 3600;
                    int to = int.Parse(DownloadWindow.GetInstance().GetEndSecBox().Text) + int.Parse(DownloadWindow.GetInstance().GetEndMinBox().Text) * 60 + int.Parse(DownloadWindow.GetInstance().GetEndHourBox().Text) * 3600;

                    info.Arguments += string.Format(" --download-sections \"*{0}-{1}\"", from, to);
                }));
            }
            info.Arguments += " " + url;
            info.WorkingDirectory = Properties.Settings.Default.downloadDirectory;
            info.CreateNoWindow = true;
            info.UseShellExecute = false;
            info.RedirectStandardOutput = true;
            info.RedirectStandardError = true;
            using (Process p = new Process())
            {
                p.StartInfo = info;
                string messageString, alternativeString;
                string audioString = "Downloading audio";
                string videoString = "Downloading video";
                if (audioOnly)
                {
                    messageString = audioString;
                    alternativeString = audioString;
                }
                else
                {
                    messageString = videoString;
                    alternativeString = audioString;
                }

                p.OutputDataReceived += new DataReceivedEventHandler((s, e) =>
                {
                    (messageString, audioString) = processDownloadProgressString(e.Data, audioOnly, messageString,
                        audioString, (audioOnly && convertToMp3) || (!audioOnly && convertToMp4), isPlaylist);
                    System.Diagnostics.Debug.WriteLine(e.Data);
                });
                p.ErrorDataReceived += new DataReceivedEventHandler((s, e) =>
                {
                    (messageString, audioString) = processDownloadProgressString(e.Data, audioOnly, messageString,
                        audioString, (audioOnly && convertToMp3) || (!audioOnly && convertToMp4), isPlaylist);
                    System.Diagnostics.Debug.WriteLine(e.Data);
                });
                p.Start();
                p.BeginErrorReadLine();
                p.BeginOutputReadLine();
                p.WaitForExit();
            }

            DownloadWindow.GetInstance().EnableDownloadChildren();
            if (openDirectory)
                Process.Start(info.WorkingDirectory);
        }
        private int currentItem = 0, maxItems = 0;
        private (string, string) processDownloadProgressString(string str, bool audioOnly, string startingString, string alternativeString, bool needsConversion, bool isPlaylist)
        {
            if (str == null)
                return (startingString, alternativeString);
            double progress = Utils.downloadProgressStringToDouble(str);
            if (progress == -1)
            {
                if (str.Contains("[download] Downloading item "))
                {
                    int ofIndex = str.IndexOf(" of ");
                    maxItems = int.Parse(str.Substring(ofIndex + 4));
                    int itemIndex = str.IndexOf("item");
                    currentItem = int.Parse(str.Substring(itemIndex + 4, str.Length - itemIndex - 4 - (str.Length - ofIndex)));
                }
                return (startingString, alternativeString);
            }
            MainWindow.GetInstance().Dispatcher.Invoke((Action)(() =>
            {
                //bool reallybad = str.Contains("frag");
                //if (progress < DownloadWindow.GetInstance().GetProgressBar().Value && !reallybad)
                //    (startingString, alternativeString) = (alternativeString, startingString);
                if (progress < DownloadWindow.GetInstance().GetProgressBar().Value)
                    (startingString, alternativeString) = (alternativeString, startingString);
                DownloadWindow.GetInstance().GetProgressBar().Value = progress;
                if (!isPlaylist && currentItem == maxItems)
                {
                    if (progress == 100)
                        DownloadWindow.GetInstance().GetProgressBar().Foreground = System.Windows.Media.Brushes.Green;
                    else
                    {
                        DownloadWindow.GetInstance().GetProgressBar().Foreground = MainWindow.GetInstance().mainBrush;
                    }
                }
                if (progress == 100 && startingString == "Downloading audio" && needsConversion && currentItem == maxItems)
                    startingString = "Converting";
                //if (!reallybad)
                //{
                    DownloadWindow.GetInstance().GetProgressBarLabel().Content = startingString;
                    if (isPlaylist)
                    {
                        if (currentItem > 0 && maxItems > 0)
                            DownloadWindow.GetInstance().GetProgressBarLabel().Content += string.Format(" (item {0} of {1})", currentItem, maxItems);//TODO: green doesnt work, converting text does not work
                    }
                //}
                //else
                //{
                //    DownloadWindow.GetInstance().GetProgressBarLabel().Content = "Video is fragmented, no details can be obtained";
                //}
            }));
            return (startingString, alternativeString);
        }
        public async Task DownloadVideo(string url)
        {
            if (!Utils.isLinkValid(url))
            {
                return;
            }
            if (CurrentURL == url)
                return;
            if (VideoDownloadTask.Status != TaskStatus.RanToCompletion && VideoDownloadTask.Status != TaskStatus.Created)
            {
                VideoDownloadTask.Wait();
                await DownloadVideo(url);
                return;
            }

            if (YTdlpUpdater.GetInstance().GetTask().Status != TaskStatus.RanToCompletion &&
                YTdlpUpdater.GetInstance().GetTask().Status != TaskStatus.Created)
            {
                YTdlpUpdater.GetInstance().GetTask().Wait();
                await DownloadVideo(url);
                return;
            }
            VideoDownloadTask = Task.Run(() => DownloadVideoTask(url));
        }
    }
}
