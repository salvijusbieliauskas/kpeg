using kpeg.Downloading.ProcessContainers;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

namespace kpeg.Conversion.ProcessContainers
{
    public class FFProbeContainer
    {
        private static FFProbeContainer FFProbeContainerInstance = null;
        private Task FFprobeTask;
        private bool cancelled = false;
        public static FFProbeContainer GetInstance()
        {
            if (FFProbeContainerInstance == null)
                FFProbeContainerInstance = new FFProbeContainer();
            return FFProbeContainerInstance;
        }
        private FFProbeContainer()
        {
            FFprobeTask = new Task(() => { });
        }
        private Task RunFFProbeTask(string args)
        {
            using (Process p = new Process())
            {
                ProcessStartInfo info = new ProcessStartInfo();
                info.FileName = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                    "Resources/ffprobe.exe");
                info.Arguments = args;
                info.CreateNoWindow = true;
                info.UseShellExecute = false;
                info.RedirectStandardOutput = true;
                info.RedirectStandardError = true;
                info.WorkingDirectory = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                    "Resources/");
                p.StartInfo = info;
                p.OutputDataReceived += (s, e) =>
                {
                    if (e.Data == null)
                        return;
                    Application.Current.Dispatcher.Invoke(() => { TerminalWindow.GetInstance().FFProbeContainerView.TextBox.AppendText(e.Data.Trim() + '\n'); });
                };
                p.ErrorDataReceived += (s, e) =>
                {
                    if (e.Data == null)
                        return;
                    Application.Current.Dispatcher.Invoke(() => { TerminalWindow.GetInstance().FFProbeContainerView.TextBox.AppendText(e.Data.Trim() + '\n'); });
                };
                p.Start();
                p.BeginErrorReadLine();
                p.BeginOutputReadLine();
                p.WaitForExit();
            }
            return Task.CompletedTask;
        }
        public async Task GetFileInfoJson(string args)
        {
            if (FFprobeTask.Status != TaskStatus.RanToCompletion &&
                FFprobeTask.Status != TaskStatus.Created)
            {
                cancelled = true;
                FFprobeTask.Wait();
                cancelled = false;
                await GetFileInfoJson(args);
                return;
            }

            if (YTdlpUpdater.GetInstance().GetTask().Status != TaskStatus.RanToCompletion &&
                YTdlpUpdater.GetInstance().GetTask().Status != TaskStatus.Created)
            {
                YTdlpUpdater.GetInstance().GetTask().Wait();
                await GetFileInfoJson(args);
                return;
            }

            FFprobeTask = Task.Run(() => RunFFProbeTask(args));
            FFprobeTask.Wait();
        }
    }
}
