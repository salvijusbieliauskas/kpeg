using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kpeg
{
    public static class Utils
    {
        public static double downloadProgressStringToDouble(string str)
        {
            if (str == null)
                return -1;
            if (str == "")
                return -1;
            if (str.IndexOf("download") < 0 || str.IndexOf("% of") < 0)
                return -1;
            string progress = str.Substring(11, 5).Trim();
            if (double.TryParse(progress, out _))
                return double.Parse(progress);
            return -1;
        }
        public static string GetDownloadFolderPath()
        {
            if (Properties.Settings.Default.downloadDirectory != null && Properties.Settings.Default.downloadDirectory != "")
                return Properties.Settings.Default.downloadDirectory;
            if (System.Environment.OSVersion.Platform == System.PlatformID.Unix)
            {
                string pathDownload = System.IO.Path.Combine(GetHomePath(), "Downloads");
                return pathDownload;
            }

            return System.Convert.ToString(Microsoft.Win32.Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Shell Folders", "{374DE290-123F-4565-9164-39C4925E467B}", String.Empty));
        }
        public static string trimListPart(string list)
        {
            if (isPlayList(list))
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

        public static void cleanupFiles()
        {
            DirectoryInfo di = new DirectoryInfo(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "Resources/"));
            foreach (FileInfo fileInfo in di.GetFiles())
            {
                if (fileInfo.Name.ToLower().StartsWith("tmp"))
                    fileInfo.Delete();
            }
        }
        public static bool isPlayList(string url)
        {
            return url.Contains("&list");
        }
        public static bool isLinkValid(string link)
        {
            string linkType1 = "youtu.be/";
            string linkType2 = "watch?v=";
            if (link.IndexOf(linkType1) == -1 && link.IndexOf(linkType2) == -1)
                return false;
            if (link.IndexOf(linkType1) > -1)
            {
                return link.Length >= link.IndexOf(linkType1) + 11 + linkType1.Length;
            }
            else
            {
                return link.Length >= link.IndexOf(linkType2) + 11 + linkType2.Length;
            }
        }
    }
}
