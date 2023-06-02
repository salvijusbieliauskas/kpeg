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
                throw new NotImplementedException(); //implement cases
            }
            catch (PathTooLongException e)
            {
                throw new NotImplementedException();
            }
            catch (IOException e)
            {
                throw new NotImplementedException();
            }
            catch (Exception e)
            {
                throw new NotImplementedException();
            }
        }
        public static void Set(string key, object value)
        {
            Settings[key] = value;
            Save();
        }

        public static object Get(string key)
        {
            if (Settings.Count == 0)
                Load();
            return Settings[key];
        }
        private static Dictionary<string,object> GetDefaults()
        {
            return new Dictionary<string, object>()
            {
                {"openDirectoryAfterDownload", true},
                {"openConverterAfterDownload", false},
                {"convertToMp4", true},
                {"downloadAudioOnly", false},
                {"downloadAudioAsMp3", false},
                {"setModifiedDate", true},
                {"downloadDirectory", ""},
                {"downloadClip", false}
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
                    Settings = (Dictionary<string, object>)JsonSerializer.Deserialize(sr, typeof(Dictionary<string, object>));
                }
                if (Settings == null || Settings.Count != GetDefaults().Count)
                {
                    Settings = GetDefaults();
                    throw new NotImplementedException();
                }
            }
            catch (DirectoryNotFoundException e)
            {
                throw new NotImplementedException();
            }
            catch (IOException e)
            {
                throw new NotImplementedException();
            }
            catch (Exception e)
            {
                throw new NotImplementedException();
            }
        }
    }
}
