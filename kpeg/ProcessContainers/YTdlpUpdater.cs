using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace kpeg.ProcessContainers
{
    public class YTdlpUpdater
    {
        private static YTdlpUpdater YTdlpUpdaterInstance;
        private Task<bool> YTdlpUpdateTask;
        private YTdlpUpdater()
        {
            YTdlpUpdateTask = new Task<bool>(() => { return true; });
        }

        public static YTdlpUpdater GetInstance()
        {
            if (YTdlpUpdaterInstance == null)
                YTdlpUpdaterInstance = new YTdlpUpdater();
            return YTdlpUpdaterInstance;
        }
        private async Task<bool> RunUpdateProcess()
        {
            using (Process p = new Process())
            {
                ProcessStartInfo info = new ProcessStartInfo();
                info.FileName = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "Resources/yt-dlp.exe");
                info.Arguments = "-U";
                info.CreateNoWindow = true;
                info.RedirectStandardOutput = true;
                info.RedirectStandardError = true;
                info.UseShellExecute = false;
                info.WorkingDirectory = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "Resources/");
                p.StartInfo = info;
                string output = "";
                p.OutputDataReceived += new DataReceivedEventHandler((s, e) =>
                {
                    output += e.Data + "\n";
                });
                p.ErrorDataReceived += new DataReceivedEventHandler((s, e) =>
                {
                    output += e.Data + "\n";
                });
                p.Start();
                p.BeginErrorReadLine();
                p.BeginOutputReadLine();
                p.WaitForExit();
                if (output.Contains("Updated"))
                    return true;
                else
                    return false;
            }
        }

        public async Task updateYTDLP()
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                DownloadWindow.GetInstance().GetVideoTitleBlock().Text = "Updating yt-dlp";
            }));
            if (YTdlpUpdateTask.Status == TaskStatus.Running)
                return;
            await (YTdlpUpdateTask = Task.Run(RunUpdateProcess));
            if (YTdlpUpdateTask.Result == true)
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    DownloadWindow.GetInstance().GetVideoTitleBlock().Text = "Update success";
                }));
            else
                throw new Exception("Update failure");
        }
    }
}
