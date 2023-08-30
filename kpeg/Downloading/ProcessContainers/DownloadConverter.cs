using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace kpeg.Downloading.ProcessContainers
{
    public class DownloadConverter
    {
        private static DownloadConverter DownloadConverterInstance;
        private DownloadConverter()
        {
            ConvertVideoTask = new Task(() => { });
        }
        public static DownloadConverter GetInstance()
        {
            if(DownloadConverterInstance == null)
                DownloadConverterInstance = new DownloadConverter();
            return DownloadConverterInstance;
        }
        private bool Cancelled;

        private Task ConvertVideoTask;
        private async Task ConvertVideo(string path, string args, string outputPath)
        {
            using (Process p = new Process())
            {
                ProcessStartInfo info = new ProcessStartInfo();
                info.FileName = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                    "Resources/ffmpeg.exe");
                info.Arguments = $"-i \"{path}\" {args} \"{outputPath}\"";
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
                    Application.Current.Dispatcher.Invoke(() => { TerminalWindow.GetInstance().DownloadedVideoConverterView.TextBox.AppendText(e.Data.Trim() + '\n'); });
                };
                p.ErrorDataReceived += (s, e) =>
                {
                    if (e.Data == null)
                        return;
                    accumulated += e.Data + '\n';
                    Application.Current.Dispatcher.Invoke(() => { TerminalWindow.GetInstance().DownloadedVideoConverterView.TextBox.AppendText(e.Data.Trim() + '\n'); });
                };
                p.Start();
                p.BeginErrorReadLine();
                p.BeginOutputReadLine();
                p.WaitForExit();
            }
        }

        public async Task ConvertDownloadedVideo(string path, string args, string outputPath)
        {
            if (!File.Exists(path))
                return;
            if (ConvertVideoTask.Status != TaskStatus.RanToCompletion &&
                ConvertVideoTask.Status != TaskStatus.Created)
            {
                Cancelled = true;
                ConvertVideoTask.Wait();
                Cancelled = false;
                await ConvertDownloadedVideo(path, args, outputPath);
                return;
            }
            ConvertVideoTask = Task.Run(() => ConvertVideo(path, args, outputPath));
            ConvertVideoTask.Wait();
        }
    }
}
