using System;
using System.IO;
using System.Text.Json;
using System.Windows;
using Microsoft.Win32;

namespace WordForge;

public record Project
{
    public string Title { get; set; } = "";
    public string Author { get; set; } = "";
    public string Genre { get; set; } = "";
    public DateTime Created { get; set; }
        = DateTime.Now;
    public DateTime Saved { get; set; }
        = DateTime.Now;
}

public static class ProjectService
{
    public static Project CurrentProject { get; private set; } = new Project
    {
        Title = "New Project",
        Created = DateTime.Now,
        Saved = DateTime.Now
    };

    public static string? CurrentPath { get; private set; }
        = null;

    public static void CreateNew(Project project)
    {
        var documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        var fileName = SanitizeFileName(project.Title) + ".forge";
        var path = Path.Combine(documents, fileName);
        project.Created = project.Saved = DateTime.Now;
        SaveToPath(project, path);
        SetCurrent(project, path);
    }

    public static void Save()
    {
        if (CurrentProject == null)
            return;

        if (CurrentPath == null)
        {
            var dialog = new SaveFileDialog
            {
                Filter = "WordForge Project (*.forge)|*.forge",
                DefaultExt = ".forge",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                FileName = SanitizeFileName(CurrentProject.Title) + ".forge"
            };
            if (dialog.ShowDialog() == true)
            {
                SaveToPath(CurrentProject, dialog.FileName);
                CurrentPath = dialog.FileName;
            }
        }
        else
        {
            SaveToPath(CurrentProject, CurrentPath);
        }
        UpdateWindowTitle();
    }

    public static void Load()
    {
        var dialog = new OpenFileDialog
        {
            Filter = "WordForge Project (*.forge)|*.forge",
            DefaultExt = ".forge",
            InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
        };
        if (dialog.ShowDialog() == true)
        {
            var json = File.ReadAllText(dialog.FileName);
            var project = JsonSerializer.Deserialize<Project>(json);
            if (project != null)
            {
                SetCurrent(project, dialog.FileName);
            }
        }
    }

    private static void SaveToPath(Project project, string path)
    {
        project.Saved = DateTime.Now;
        var json = JsonSerializer.Serialize(project, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(path, json);
    }

    private static void SetCurrent(Project project, string path)
    {
        CurrentProject = project;
        CurrentPath = path;
        UpdateWindowTitle();
    }

    private static void UpdateWindowTitle()
    {
        if (Application.Current.MainWindow is MainWindow main && CurrentProject != null)
        {
            main.Title = $"WordForge - {CurrentProject.Title}";
        }
    }

    private static string SanitizeFileName(string name)
    {
        foreach (var c in Path.GetInvalidFileNameChars())
            name = name.Replace(c, '_');
        return string.IsNullOrWhiteSpace(name) ? "project" : name;
    }
}

