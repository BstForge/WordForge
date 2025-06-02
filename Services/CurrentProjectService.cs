using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using WordForge.models;

namespace WordForge.Services
{
    public class CurrentProjectService
    {
        private static readonly Lazy<CurrentProjectService> instance = new(() => new CurrentProjectService());
        public static CurrentProjectService Instance => instance.Value;

        private CurrentProjectService() { }

        public ProjectData CurrentProject { get; private set; } = new();
        public string CurrentFilePath { get; private set; }

        public void Load(ProjectData project, string path)
        {
            CurrentProject = project;
            CurrentFilePath = path;
        }

        public void SetFilePath(string path)
        {
            CurrentFilePath = path;
        }

        public void Save()
        {
            if (string.IsNullOrWhiteSpace(CurrentFilePath))
                throw new InvalidOperationException("No file path set for current project.");

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };

            string json = JsonSerializer.Serialize(CurrentProject, options);
            File.WriteAllText(CurrentFilePath, json);
        }

        public void CreateNew(string title, string author, string series, string path)
        {
            CurrentProject = new ProjectData
            {
                Title = title,
                Author = author,
                Series = series
            };

            CurrentFilePath = path;
        }
    }
}
