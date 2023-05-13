using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace kpeg.ProcessContainers
{
    public class ThumbnailDownloader
    {
        private static ThumbnailDownloader thumbnailDownloaderInstance;
        private Task thumbnailDownloadTask;
        private ThumbnailDownloader()
        {
            thumbnailDownloadTask = new Task(new Action(() => { }));
        }

        public static ThumbnailDownloader GetInstance()
        {
            return thumbnailDownloaderInstance ?? (thumbnailDownloaderInstance = new ThumbnailDownloader());
        }

        private string CurrentURL = string.Empty;
        private bool Cancelled = false;
        private async Task GetThumbnail(string url)
        {
            CurrentURL = url;
            using (Process p = new Process())
            {
                ProcessStartInfo info = new ProcessStartInfo();
                info.FileName = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "Resources/yt-dlp.exe");
                info.Arguments = "--skip-download --write-thumbnail --output tmpimage --convert-thumbnails png " + Utils.trimListPart(url);
                info.CreateNoWindow = true;
                info.UseShellExecute = false;
                info.RedirectStandardOutput = true;
                info.RedirectStandardError = true;
                info.WorkingDirectory = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "Resources/");
                p.StartInfo = info;
                string accumulated = "";
                p.OutputDataReceived += new DataReceivedEventHandler((s, e) =>
                {
                    accumulated += e.Data + '\n';
                });
                p.ErrorDataReceived += new DataReceivedEventHandler((s, e) =>
                {
                    accumulated += e.Data + '\n';
                });
                p.Start();
                p.BeginErrorReadLine();
                p.BeginOutputReadLine();
                p.WaitForExit();
                if (accumulated.Contains("ERROR: [youtube]"))
                {
                    Application.Current.Dispatcher.Invoke(new Action(() => { DownloadWindow.GetInstance().GetVideoThumbnail().Source = null; }));
                    CurrentURL = "";
                    return;
                }
                if (accumulated.Contains("yt-dlp -U"))
                {
                    CurrentURL = "";
                    return;
                }
                if (!Cancelled)
                    Application.Current.Dispatcher.Invoke(new Action(() => { DownloadWindow.GetInstance().GetVideoThumbnail().Source = Utils.uriToSource(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "Resources/tmpimage.png"));
                    }));
                else
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        DownloadWindow.GetInstance().GetVideoThumbnail().Source = null;
                    }));
            }
        }
        public async Task UpdateThumbnail(string url)
        {
            if (!Utils.isLinkValid(url))
            {
                Application.Current.Dispatcher.Invoke(new Action(() => { DownloadWindow.GetInstance().GetVideoThumbnail().Source = null; }));
                return;
            }
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                if (DownloadWindow.GetInstance().videoThumbnail.Source == null && DownloadWindow.GetInstance().GetVideoTitleBlock().Text == "")
                    CurrentURL = "";
            }));
            if (CurrentURL == url)
                return;
            if (thumbnailDownloadTask.Status != TaskStatus.RanToCompletion && thumbnailDownloadTask.Status != TaskStatus.Created)
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
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                DownloadWindow.GetInstance().GetVideoThumbnail().Source =
                    new BitmapImage(new Uri("pack://application:,,,/Resources/thumbnailDownloading.png"));
            }));
            thumbnailDownloadTask = Task.Run(() => GetThumbnail(url));
        }
    }
}
