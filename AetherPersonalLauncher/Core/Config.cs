using System;
using System.IO;
using Newtonsoft.Json.Linq;

namespace AetherPersonalLauncher.Core
{
    public static class Config
    {
        private static string _currentVersion = "无";
        private const string ConfigFolder = "APL";
        private const string ConfigFileName = "Config.json";
        public static event EventHandler<string> CurrentVersionChanged;

        public static string CurrentVersion
        {
            get => _currentVersion;
            set
            {
                _currentVersion = value;
                CurrentVersionChanged?.Invoke(null, value);
            }
        }

        public static string CurrentUser { get; set; } = string.Empty;

        static Config()
        {
            if (File.Exists(Path.Combine(Environment.CurrentDirectory, ConfigFolder, ConfigFileName)))
            {
                var text = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, ConfigFolder, ConfigFileName));
                JObject obj = JObject.Parse(text);
                CurrentVersion = (string)obj["CurrentVersion"];
                CurrentUser = (string)obj["CurrentUser"];
            }
        }

        public static void Save()
        {
            JObject obj = new JObject();
            obj.Add("CurrentVersion", CurrentVersion);
            obj.Add("CurrentUser", CurrentUser);
            if (!File.Exists(Path.Combine(Environment.CurrentDirectory, ConfigFolder, ConfigFileName)))
            {
                if (!Directory.Exists(Path.Combine(Environment.CurrentDirectory, ConfigFolder)))
                {
                    Directory.CreateDirectory(Path.Combine(Environment.CurrentDirectory, ConfigFolder));
                }

                File.Create(Path.Combine(Environment.CurrentDirectory, ConfigFolder, ConfigFileName))
                    .Close();
            }

            File.WriteAllText(Path.Combine(Environment.CurrentDirectory, ConfigFolder, ConfigFileName),
                obj.ToString());
        }
    }
}