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
            Cancelled = false;
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
                    return;
                if (accumulated.Contains("yt-dlp -U"))
                {
                    Application.Current.Dispatcher.Invoke(new Action(() => { DownloadWindow.GetInstance().GetVideoTitleBlock().Text = "Update required. Please wait."; }));
                    await YTdlpUpdater.GetInstance().updateYTDLP();
                    await GetVideoName(url);
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
            if (CurrentURL == url)
                return;
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                DownloadWindow.GetInstance().GetVideoTitleBlock().Text = "Fetching name...";
            }));
            if (videoNameDownloadTask.Status == TaskStatus.Running)
            {
                Cancelled = true;
                System.Diagnostics.Debug.WriteLine("yea");
                videoNameDownloadTask.Wait();
            }
            videoNameDownloadTask = Task.Run(() => GetVideoName(url));
        }
    }
}
