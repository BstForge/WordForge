using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Linq;
using WordForge.Properties;

namespace WordForge.Services
{
    public static class RecentProjectsService
    {
        private const string SettingKey = "RecentProjects";
        private const string CurrentPathKey = "CurrentProjectPath";
        private const int MaxRecent = 5;

        public static List<string> Load()
        {
            StringCollection paths = Settings.Default[SettingKey] as StringCollection ?? new StringCollection();
            return paths.Cast<string>().Where(File.Exists).ToList();
        }

        public static void Add(string filePath)
        {
            var settings = Settings.Default;
            var paths = settings[SettingKey] as StringCollection ?? new StringCollection();

            if (paths.Contains(filePath))
                paths.Remove(filePath);

            paths.Insert(0, filePath);

            while (paths.Count > MaxRecent)
                paths.RemoveAt(paths.Count - 1);

            settings[SettingKey] = paths;
            settings[CurrentPathKey] = filePath;
            settings.Save();
        }

        public static void SetCurrentFile(string filePath)
        {
            Settings.Default[CurrentPathKey] = filePath;
            Settings.Default.Save();
        }

        public static string GetCurrentFile()
        {
            return Settings.Default[CurrentPathKey]?.ToString();
        }
    }
}
