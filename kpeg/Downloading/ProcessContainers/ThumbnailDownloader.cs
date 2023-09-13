using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace kpeg.Downloading.ProcessContainers
{
    public class ThumbnailDownloader
    {
        private static ThumbnailDownloader thumbnailDownloaderInstance;
        private bool Cancelled;

        private string CurrentURL = string.Empty;
        private Task thumbnailDownloadTask;

        private ThumbnailDownloader()
        {
            thumbnailDownloadTask = new Task(() => { });
        }

        public static ThumbnailDownloader GetInstance()
        {
            return thumbnailDownloaderInstance ?? (thumbnailDownloaderInstance = new ThumbnailDownloader());
        }

        private async Task GetThumbnail(string url)
        {
            CurrentURL = url;
            using (Process p = new Process())
            {
                ProcessStartInfo info = new ProcessStartInfo();
                info.FileName = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                    "Resources/yt-dlp.exe");
                info.Arguments = "--skip-download --write-thumbnail --output tmpimage --convert-thumbnails png " +
                                 Utils.TrimListPart(url);
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
                    Application.Current.Dispatcher.Invoke(() => { TerminalWindow.GetInstance().ThumbnailDownloaderView.TextBox.AppendText(e.Data.Trim() + '\n'); });
                };
                p.ErrorDataReceived += (s, e) =>
                {
                    if (e.Data == null)
                        return;
                    accumulated += e.Data + '\n';
                    Application.Current.Dispatcher.Invoke(() => { TerminalWindow.GetInstance().ThumbnailDownloaderView.TextBox.AppendText(e.Data.Trim() + '\n'); });
                };
                p.Start();
                p.BeginErrorReadLine();
                p.BeginOutputReadLine();
                p.WaitForExit();
                if (accumulated.Contains("ERROR: [youtube]"))
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        DownloadWindow.GetInstance().GetVideoThumbnail().Source = null;
                    });
                    CurrentURL = "";
                    return;
                }

                if (accumulated.EndsWith("yt-dlp -U"))
                {
                    CurrentURL = "";
                    return;
                }
                
                if (!Cancelled)
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        DownloadWindow.GetInstance().GetVideoThumbnail().Source = Utils.UriToSource(
                            Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                                "Resources/tmpimage.png"));
                    });
                else
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        DownloadWindow.GetInstance().GetVideoThumbnail().Source = null;
                    });
            }
        }

        public async Task UpdateThumbnail(string url)
        {
            if (!Utils.IsLinkValid(url))
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    DownloadWindow.GetInstance().GetVideoThumbnail().Source = null;
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
            if (thumbnailDownloadTask.Status != TaskStatus.RanToCompletion &&
                thumbnailDownloadTask.Status != TaskStatus.Created)
            {
                Cancelled = true;
                thumbnailDownloadTask.Wait();
                Cancelled = false;
                await UpdateThumbnail(url);
                return;
            }

            if (YTdlpUpdater.GetInstance().GetTask().Status != TaskStatus.RanToCompletion &&
                YTdlpUpdater.GetInstance().GetTask().Status != TaskStatus.Created)
            {
                YTdlpUpdater.GetInstance().GetTask().Wait();
                await UpdateThumbnail(url);
                return;
            }

            Application.Current.Dispatcher.Invoke(() =>
            {
                DownloadWindow.GetInstance().GetVideoThumbnail().Source =
                    new BitmapImage(new Uri("pack://application:,,,/Resources/thumbnailDownloading.png"));
            });
            thumbnailDownloadTask = Task.Run(() => GetThumbnail(url));
        }
    }
}