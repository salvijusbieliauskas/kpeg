using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

namespace kpeg.Downloading.ProcessContainers
{
    public class VideoNameDownloader
    {
        private static VideoNameDownloader VideoNameDownloaderInstance;
        private bool Cancelled;

        private string CurrentURL = string.Empty;
        private Task videoNameDownloadTask;

        private VideoNameDownloader()
        {
            videoNameDownloadTask = new Task(() => { });
        }

        public static VideoNameDownloader GetInstance()
        {
            if (VideoNameDownloaderInstance == null)
                VideoNameDownloaderInstance = new VideoNameDownloader();
            return VideoNameDownloaderInstance;
        }

        private async Task GetVideoName(string url)
        {
            CurrentURL = url;
            using (Process p = new Process())
            {
                ProcessStartInfo info = new ProcessStartInfo(
                    Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                        "Resources/yt-dlp.exe"), "--skip-download --print title " + Utils.trimListPart(url));
                info.CreateNoWindow = true;
                info.UseShellExecute = false;
                info.RedirectStandardOutput = true;
                info.RedirectStandardError = true;
                info.WorkingDirectory = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                    "Resources/");
                p.StartInfo = info;
                string accumulated = "";
                p.OutputDataReceived += (s, e) =>
                {
                    if (e.Data == null)
                        return;
                    accumulated += e.Data + '\n';
                    Application.Current.Dispatcher.Invoke(() => { TerminalWindow.GetInstance().VideoNameDownloaderView.TextBox.AppendText(e.Data.Trim() + '\n'); });
                };
                p.ErrorDataReceived += (s, e) =>
                {
                    if (e.Data == null)
                        return;
                    accumulated += e.Data + '\n';
                    Application.Current.Dispatcher.Invoke(() => { TerminalWindow.GetInstance().VideoNameDownloaderView.TextBox.AppendText(e.Data.Trim() + '\n'); });
                };
                p.Start();
                p.BeginErrorReadLine();
                p.BeginOutputReadLine();
                p.WaitForExit();
                if (Cancelled) return;

                if (accumulated.Trim().EndsWith("yt-dlp -U"))
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        DownloadWindow.GetInstance().GetVideoTitleBlock().Text = "Update required. Please wait.";
                    });
                    await YTdlpUpdater.GetInstance().UpdateYTDLP();
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        DownloadWindow.GetInstance().textBox_TextChanged(null, null);
                    });
                    CurrentURL = "";
                    return;
                }

                if (accumulated.StartsWith("ERROR:"))
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        DownloadWindow.GetInstance().GetVideoTitleBlock().Text =
                            "Invalid video. Could not retrieve data.";
                    });
                    CurrentURL = "";
                    return;
                }


                //if (accumulated.StartsWith("WARNING") && accumulated.Contains(".js"))
                //    accumulated = accumulated.Substring(accumulated.IndexOf(".js") + 3);
                accumulated = accumulated.Split(new char[] { '\n' },System.StringSplitOptions.RemoveEmptyEntries).Last();
                Application.Current.Dispatcher.Invoke(() =>
                {
                    DownloadWindow.GetInstance().GetVideoTitleBlock().Text = Utils.isPlayList(url)
                        ? accumulated.Trim() + " (PLAYLIST)"
                        : accumulated.Trim();
                });
            }
        }

        public async Task UpdateVideoName(string url)
        {
            if (!Utils.isLinkValid(url))
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    DownloadWindow.GetInstance().GetVideoTitleBlock().Text = "";
                });
                return;
            }

            Application.Current.Dispatcher.Invoke(() =>
            {
                if (DownloadWindow.GetInstance().videoThumbnail.Source == null &&
                    DownloadWindow.GetInstance().GetVideoTitleBlock().Text == "")
                    CurrentURL = "";
            });
            if (CurrentURL == url)
                return;
            Application.Current.Dispatcher.Invoke(() =>
            {
                DownloadWindow.GetInstance().GetVideoTitleBlock().Text = "Fetching name...";
            });
            if (videoNameDownloadTask.Status != TaskStatus.RanToCompletion &&
                videoNameDownloadTask.Status != TaskStatus.Created)
            {
                Cancelled = true;
                videoNameDownloadTask.Wait();
                Cancelled = false;
                await UpdateVideoName(url);
                return;
            }

            if (YTdlpUpdater.GetInstance().GetTask().Status != TaskStatus.RanToCompletion &&
                YTdlpUpdater.GetInstance().GetTask().Status != TaskStatus.Created)
            {
                YTdlpUpdater.GetInstance().GetTask().Wait();
                await UpdateVideoName(url);
                return;
            }

            videoNameDownloadTask = Task.Run(() => GetVideoName(url));
        }
    }
}