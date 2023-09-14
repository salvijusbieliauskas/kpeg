using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace kpeg
{
    public static class Utils
    {
        public static double DownloadProgressStringToDouble(string str, TimeSpan duration)
        {
            if (str == null)
                return -1;
            if (str == "")
                return -1;
            if (str.IndexOf("100% of") > -1)
                return 100;
            double FFmpegParseResult = ProcessFFmpegStringToDouble(str, duration);
            if (FFmpegParseResult != -1)
                return FFmpegParseResult;

            if (str.IndexOf("download") < 0 || str.IndexOf("% of") < 0)
                return -1;
            string progress = str.Substring(11, 5).Trim();
            if (double.TryParse(progress, out _))
                return double.Parse(progress);
            return -1;
        }
        public static double ProcessFFmpegStringToDouble(string str, TimeSpan duration)
        {
            if (str == null)
                return -1;
            if (str == "")
                return -1;
            int index = str.IndexOf("time=");
            if (index < 0)
                return -1;
            try
            {
                return ((TimeSpan.Parse(str.Substring(index + 5, 8)).TotalSeconds/duration.TotalSeconds)*100);
            }
            catch
            {
                return -1;
            }
        }
        public static string GetDownloadFolderPath()
        {
            if ((string)SettingsManager.Get(Setting.DownloadDirectory) != null && (string)SettingsManager.Get(Setting.DownloadDirectory) != "")
                return (string)SettingsManager.Get(Setting.DownloadDirectory);
            if (System.Environment.OSVersion.Platform == System.PlatformID.Unix)
            {
                string pathDownload = System.IO.Path.Combine(GetHomePath(), "Downloads");
                return pathDownload;
            }

            return System.Convert.ToString(Microsoft.Win32.Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Shell Folders", "{374DE290-123F-4565-9164-39C4925E467B}", String.Empty));
        }
        public static string TrimListPart(string list)
        {
            if (IsPlayList(list))
            {
                return list.Substring(0, list.IndexOf("&list"));
            }
            return list;
        }
        public static string GetHomePath()
        {
            if (System.Environment.OSVersion.Platform == System.PlatformID.Unix)
                return System.Environment.GetEnvironmentVariable("HOME");

            return System.Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%");
        }

        public static void CleanupFiles()
        {
            DirectoryInfo di = new DirectoryInfo(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "Resources/"));
            foreach (FileInfo fileInfo in di.GetFiles())
            {
                if (fileInfo.Name.ToLower().StartsWith("tmp"))
                    fileInfo.Delete();
            }
        }
        public static bool IsPlayList(string url)
        {
            return url.Contains("&list");
        }
        public static bool IsLinkValid(string link)
        {
            string linkType1 = "youtu.be/";
            string linkType2 = "watch?v=";
            string linkType3 = "shorts/";
            if (link.IndexOf(linkType1) == -1 && link.IndexOf(linkType2) == -1 && link.IndexOf(linkType3) == -1)
                return false;
            if (link.IndexOf(linkType1) > -1)
            {
                return link.Length >= link.IndexOf(linkType1) + 11 + linkType1.Length;
            }
            else if(link.IndexOf(linkType2) > -1)
            {
                return link.Length >= link.IndexOf(linkType2) + 11 + linkType2.Length;
            }
            else
            {
                return link.Length >= link.IndexOf(linkType3) + 11 + linkType3.Length;
            }
        }
        public static BitmapImage UriToSource(string path)
        {
            using (FileStream fs = new FileStream(path, FileMode.Open))
            {
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = fs;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                return bitmapImage;
            }
        }
    }
}
