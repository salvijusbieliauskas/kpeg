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

        private async Task DownloadVideoTask(string url)
        {
            DownloadWindow.GetInstance().DisableDownloadChildren();
            Settings.Default.Save();
            ProcessStartInfo info = new ProcessStartInfo();
            info.FileName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\Resources\\yt-dlp.exe";
            bool audioOnly = (bool)SettingsManager.Get("downloadAudioOnly");
            bool openDirectory = (bool)SettingsManager.Get("openDirectoryAfterDownload");
            bool convertToMp3 = (bool)SettingsManager.Get("downloadAudioAsMp3");
            bool convertToMp4 = (bool)SettingsManager.Get("convertToMp4");
            bool isPlaylist = Utils.isPlayList(url);
            bool downloadClip = (bool)SettingsManager.Get("downloadClip");
            if (audioOnly)
                convertToMp4 = false;
            if (audioOnly)
            {
                info.Arguments += "-f \"bestaudio\" -x";
                if (convertToMp3)
                    info.Arguments += " --audio-format wav";
            }

            if ((bool)SettingsManager.Get("setModifiedDate"))
                info.Arguments += " --no-mtime";
            info.Arguments += " " + url;
            info.WorkingDirectory = (string)SettingsManager.Get("downloadDirectory");
            info.CreateNoWindow = true;
            info.UseShellExecute = false;
            info.RedirectStandardOutput = true;
            info.RedirectStandardError = true;
            Application.Current.Dispatcher.Invoke(() => { TerminalWindow.GetInstance().VideoDownloaderView.TextBox.AppendText(info.WorkingDirectory+"> "+info.FileName+" "+info.Arguments + '\n'); });
            string output = "";
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

                p.OutputDataReceived += (s, e) =>
                {
                    if (e.Data == null)
                        return;
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        TerminalWindow.GetInstance().VideoDownloaderView.TextBox.AppendText(e.Data.Trim() + '\n');
                    });
                    (messageString, audioString) = processDownloadProgressString(e.Data, audioOnly, messageString,
                        audioString, (audioOnly && convertToMp3) || (!audioOnly && convertToMp4), isPlaylist);
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
                    (messageString, audioString) = processDownloadProgressString(e.Data, audioOnly, messageString,
                        audioString, (audioOnly && convertToMp3) || (!audioOnly && convertToMp4), isPlaylist);
                    output += e.Data+'\n';
                };
                p.Start();
                p.BeginErrorReadLine();
                p.BeginOutputReadLine();
                p.WaitForExit();
            }



            if ((!audioOnly && convertToMp4) || downloadClip)
            {
                string outputName = "";
                try
                {
                    outputName = Regex.Match(output, "\\[Merger\\] Merging formats into \"[^\"]*\"", RegexOptions.IgnoreCase).ToString().Substring(31).TrimEnd(new char[] { '"' });
                }
                catch
                {
                    try
                    {
                        outputName = output.Substring(output.IndexOf("[ExtractAudio] Destination: ") + "[ExtractAudio] Destination: ".Length, output.IndexOf("\n", output.IndexOf("[ExtractAudio] Destination:")) - output.IndexOf("[ExtractAudio] Destination:") - "[ExtractAudio] Destination:".Length-1);
                    }
                    catch
                    {
                        DownloadWindow.GetInstance().EnableDownloadChildren();
                        if (openDirectory)
                            Process.Start(info.WorkingDirectory);
                        return;
                    }
                }
                string conversionArguments = "";
                string conversionPath = Path.Combine((string)SettingsManager.Get("downloadDirectory"),outputName);
                string inputExtension = outputName.Split(new char[] { '.' }, System.StringSplitOptions.RemoveEmptyEntries).Last();
                string outputExtension = "c."+inputExtension;

                if (downloadClip)
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        conversionArguments += $"-ss {DownloadWindow.GetInstance().GetStartHourBox().Text}:{DownloadWindow.GetInstance().GetStartMinBox().Text}:{DownloadWindow.GetInstance().GetStartSecBox().Text} -to {DownloadWindow.GetInstance().GetEndHourBox().Text}:{DownloadWindow.GetInstance().GetEndMinBox().Text}:{DownloadWindow.GetInstance().GetEndSecBox().Text}";

                    });
                if (!convertToMp4)
                {
                    conversionArguments += " -c:a copy";
                    if (!audioOnly)
                        conversionArguments += " -c:v copy";
                }
                else
                    outputExtension = "c.mp4";

                MainWindow.GetInstance().Dispatcher.Invoke(() =>
                {
                    DownloadWindow.GetInstance().GetProgressBarLabel().Content = "Converting";
                });
                await DownloadConverter.GetInstance().ConvertDownloadedVideo(conversionPath, conversionArguments, conversionPath.Substring(0,conversionPath.Length-inputExtension.Length)+outputExtension);
                File.Delete(conversionPath);
                MainWindow.GetInstance().Dispatcher.Invoke(() =>
                {
                    DownloadWindow.GetInstance().GetProgressBarLabel().Content = "";
                });
            }

            DownloadWindow.GetInstance().EnableDownloadChildren();
            if (openDirectory)
                Process.Start(info.WorkingDirectory);
        }

        double lastProgress = -1;
        private (string, string) processDownloadProgressString(string str, bool audioOnly, string startingString,
            string alternativeString, bool needsConversion, bool isPlaylist)
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
                    currentItem = int.Parse(str.Substring(itemIndex + 4,
                        str.Length - itemIndex - 4 - (str.Length - ofIndex)));
                }

                return (startingString, alternativeString);
            }
            if (lastProgress == progress&&progress>0&&progress<100)
                return (startingString, alternativeString);
            lastProgress = progress;

            MainWindow.GetInstance().Dispatcher.Invoke(() =>
            {
                if (progress < DownloadWindow.GetInstance().GetProgressBar().Value)
                    (startingString, alternativeString) = (alternativeString, startingString);
                DownloadWindow.GetInstance().GetProgressBar().Value = progress;
                if (!isPlaylist && currentItem == maxItems)
                {
                    if (progress == 100)
                        DownloadWindow.GetInstance().GetProgressBar().Foreground = Brushes.Green;
                    else
                        DownloadWindow.GetInstance().GetProgressBar().Foreground = MainWindow.GetInstance().mainBrush;
                }

                if (progress == 100 && startingString == "Downloading audio" && needsConversion &&
                    currentItem == maxItems)
                    startingString = "Converting";
                DownloadWindow.GetInstance().GetProgressBarLabel().Content = startingString;
                if (isPlaylist)
                    if (currentItem > 0 && maxItems > 0)
                        DownloadWindow.GetInstance().GetProgressBarLabel().Content +=
                            string.Format(" (item {0} of {1})", currentItem,
                                maxItems); //TODO: green doesnt work, converting text does not work
            });
            return (startingString, alternativeString);
        }

        public async Task DownloadVideo(string url)
        {
            if (!Utils.isLinkValid(url)) return;
            if (CurrentURL == url)
                return;
            if (VideoDownloadTask.Status != TaskStatus.RanToCompletion &&
                VideoDownloadTask.Status != TaskStatus.Created)
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