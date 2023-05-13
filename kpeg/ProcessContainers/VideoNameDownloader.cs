using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media.Imaging;

namespace kpeg.ProcessContainers
{
    public class VideoNameDownloader
    {
        private static VideoNameDownloader VideoNameDownloaderInstance;
        private Task videoNameDownloadTask;
        private VideoNameDownloader()
        {
            videoNameDownloadTask = new Task(new Action(() => { }));
        }
        public static VideoNameDownloader GetInstance()
        {
            if(VideoNameDownloaderInstance == null)
                VideoNameDownloaderInstance = new VideoNameDownloader();
            return VideoNameDownloaderInstance;
        }

        private string CurrentURL = string.Empty;
        private bool Cancelled = false;
        private async Task GetVideoName(string url)
        {
            CurrentURL = url;
            using (Process p = new Process())
            {
                ProcessStartInfo info = new ProcessStartInfo(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "Resources/yt-dlp.exe"), "--skip-download --print title " + Utils.trimListPart(url));
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
                if (Cancelled)
                {
                    return;
                }

                if (accumulated.Contains("yt-dlp -U"))
                {
                    Application.Current.Dispatcher.Invoke(new Action(() => { DownloadWindow.GetInstance().GetVideoTitleBlock().Text = "Update required. Please wait."; }));
                    await YTdlpUpdater.GetInstance().UpdateYTDLP();
                    await GetVideoName(url);
                    ThumbnailDownloader.GetInstance().UpdateThumbnail(url);
                    CurrentURL = "";
                    return;
                }

                if (accumulated.StartsWith("ERROR:"))
                {
                    Application.Current.Dispatcher.Invoke(new Action(() => { DownloadWindow.GetInstance().GetVideoTitleBlock().Text = "Invalid video. Could not retrieve data."; }));
                    CurrentURL = "";
                    return;
                }


                if (accumulated.StartsWith("WARNING") && accumulated.Contains(".js"))
                    accumulated = accumulated.Substring(accumulated.IndexOf(".js") + 3);
                Application.Current.Dispatcher.Invoke(new Action(() => { DownloadWindow.GetInstance().GetVideoTitleBlock().Text = Utils.isPlayList(url) ? accumulated.Trim() + " (PLAYLIST)" : accumulated.Trim(); }));
            }
        }
        public async Task UpdateVideoName(string url)
        {
            if (!Utils.isLinkValid(url))
            {
                Application.Current.Dispatcher.Invoke(new Action(() => { DownloadWindow.GetInstance().GetVideoTitleBlock().Text = ""; }));
                return;
            }

            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                if (DownloadWindow.GetInstance().videoThumbnail.Source == null && DownloadWindow.GetInstance().GetVideoTitleBlock().Text ==  "")
                    CurrentURL = "";
            }));
            if (CurrentURL == url)
                return;
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                DownloadWindow.GetInstance().GetVideoTitleBlock().Text = "Fetching name...";
            }));
            if (videoNameDownloadTask.Status != TaskStatus.RanToCompletion && videoNameDownloadTask.Status != TaskStatus.Created)
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
