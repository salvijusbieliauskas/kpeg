using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace kpeg
{
    public static class SettingsManager
    {
        private static Dictionary<string, object> Settings = new Dictionary<string, object>();
        private static readonly JsonSerializer JsonSerializer = new JsonSerializer(){Formatting = Formatting.Indented};
        private static readonly string SettingsFileName = "settings.json";
        public static void Save()
        {
            try
            {
                using (StreamWriter sw = new StreamWriter(SettingsFileName))
                {
                    JsonSerializer.Serialize(sw, Settings);
                }
            }
            catch (UnauthorizedAccessException e)
            {
                new ExceptionWindow("Failed to save settings due to unauthorized access",e);
            }
            catch (PathTooLongException e)
            {
                new ExceptionWindow("Path of the settings file was too long to write",e);
            }
            catch (IOException e)
            {
                new ExceptionWindow("An IO exception has occured while saving settings",e);
            }
            catch (Exception e)
            {
                new ExceptionWindow("An unknown exception has occured while saving settings",e);
            }
        }
        public static void Set(Setting key, object value)
        {
            Settings[key.ToString()] = value;
            Save();
        }

        public static object Get(Setting key)
        {
            if (Settings.Count == 0)
                Load();
            return Settings[key.ToString()];
        }
        private static Dictionary<string,object> GetDefaults()
        {
            return new Dictionary<string, object>()
            {
                {Setting.OpenDirectoryAfterDownload.ToString(), true},
                {Setting.OpenConverterAfterDownload.ToString(), false},
                {Setting.ConvertToMp4.ToString(), true},
                {Setting.DownloadAudioOnly.ToString(), false},
                {Setting.DownloadAudioAsWav.ToString(), false},
                {Setting.SetModifiedDate.ToString(), true},
                {Setting.DownloadDirectory.ToString(), ""},
                {Setting.DownloadClip.ToString(), false}
            };
        }

        public static void SetDefaults()
        {
            Settings = GetDefaults();
        }

        public static void Load()
        {
            if (!File.Exists(SettingsFileName))
            {
                SetDefaults();
                return;
            }

            try
            {
                using (StreamReader sr = new StreamReader(SettingsFileName))
                {
                    Settings = (Dictionary<string, object>)JsonSerializer.Deserialize(sr,
                        typeof(Dictionary<string, object>));
                }

                if (Settings == null || Settings.Count != GetDefaults().Count)
                {
                    Settings = GetDefaults();
                    new ExceptionWindow("Failed loading settings, using defaults");
                }
            }
            catch (IOException e)
            {
                new ExceptionWindow("An IO exception has occured while reading the settings file",e);
            }
            catch (Exception e)
            {
                new ExceptionWindow("An unknown exception has occured while reading the settings file",e);
            }
        }
    }
    public enum Setting
    {
        OpenDirectoryAfterDownload,
        OpenConverterAfterDownload,
        ConvertToMp4,
        DownloadAudioOnly,
        DownloadAudioAsWav,
        SetModifiedDate,
        DownloadDirectory,
        DownloadClip
    }
}
