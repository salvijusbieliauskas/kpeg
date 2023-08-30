using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace kpeg.Downloading.ProcessContainers
{
    public class VideoMetadataDownloader
    {
        private static VideoMetadataDownloader VideoMetadataDownloaderInstance;
        private bool Cancelled;

        private string CurrentURL = string.Empty;
        private Task<Dictionary<string,object>> videoMetadataDownloadTask;

        private VideoMetadataDownloader()
        {
            videoMetadataDownloadTask = new Task<Dictionary<string,object>>(() => { return null; });
        }

        public static VideoMetadataDownloader GetInstance()
        {
            if (VideoMetadataDownloaderInstance == null)
                VideoMetadataDownloaderInstance = new VideoMetadataDownloader();
            return VideoMetadataDownloaderInstance;
        }

        private async Task<Dictionary<string,object>> GetVideoMetadata(string url)
        {
            CurrentURL = url;
            if (File.Exists("Resources/metadata.info.json"))
                File.Delete("Resources/metadata.info.json");
            using (Process p = new Process())
            {
                ProcessStartInfo info = new ProcessStartInfo(
                    Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                        "Resources/yt-dlp.exe"), "--skip-download --write-info-json -o metadata " + Utils.trimListPart(url));
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
                    Application.Current.Dispatcher.Invoke(() => { TerminalWindow.GetInstance().VideoMetadataDownloaderView.TextBox.AppendText(e.Data.Trim() + '\n'); });
                };
                p.ErrorDataReceived += (s, e) =>
                {
                    if (e.Data == null)
                        return;
                    accumulated += e.Data + '\n';
                    Application.Current.Dispatcher.Invoke(() => { TerminalWindow.GetInstance().VideoMetadataDownloaderView.TextBox.AppendText(e.Data.Trim() + '\n'); });
                };
                p.Start();
                p.BeginErrorReadLine();
                p.BeginOutputReadLine();
                p.WaitForExit();
                if (Cancelled) return null;

                try
                {
                    return JsonConvert.DeserializeObject<Dictionary<string, object>>(File.ReadAllText("Resources/metadata.info.json"));
                }
                catch
                {
                    return null;
                }
            }
        }

        public async Task UpdateVideoMetadata(string url)
        {
            if (!Utils.isLinkValid(url) || Utils.isPlayList(url))
            {
                return;
            }

            if (CurrentURL == url)
                return;
            if (videoMetadataDownloadTask.Status != TaskStatus.RanToCompletion &&
                videoMetadataDownloadTask.Status != TaskStatus.Created)
            {
                Cancelled = true;
                videoMetadataDownloadTask.Wait();
                Cancelled = false;
                await UpdateVideoMetadata(url);
                return;
            }

            if (YTdlpUpdater.GetInstance().GetTask().Status != TaskStatus.RanToCompletion &&
                YTdlpUpdater.GetInstance().GetTask().Status != TaskStatus.Created)
            {
                YTdlpUpdater.GetInstance().GetTask().Wait();
                await UpdateVideoMetadata(url);
                return;
            }

            videoMetadataDownloadTask = Task.Run(() => GetVideoMetadata(url));
            Dictionary<string,object> metadata = await videoMetadataDownloadTask;
            if (metadata != null)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    DownloadWindow.GetInstance().EnableClipSelector();
                    DownloadWindow.GetInstance().clipSelector.VideoDuration = (int)(Int64)metadata["duration"];
                    DownloadWindow.GetInstance().clipSelector.UserControl_Loaded(null, null);
                });
            }
            else
            {
                CurrentURL = string.Empty;
                Application.Current.Dispatcher.Invoke(() =>
                {
                    DownloadWindow.GetInstance().DisableClipSelector();
                });
            }
        }
    }
}