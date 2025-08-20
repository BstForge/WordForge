using System;
using System.IO;
using System.Text.Json;
using System.Windows;
using Microsoft.Win32;
using System.Collections.ObjectModel;

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
    public ObservableCollection<Chapter> Chapters { get; set; } = new();
    public bool IsDirty { get; set; } = false;
}

public static class ProjectService
{
    public static event Action? BeforeSave;
    public static Project CurrentProject { get; private set; } = new Project
    {
        Title = "New Project",
        Created = DateTime.Now,
        Saved = DateTime.Now
    };

    public static string? CurrentPath { get; private set; }
        = null;

    public static void CreateNew(Project project, string? directory)
    {
        directory = string.IsNullOrWhiteSpace(directory)
            ? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            : directory;
        Directory.CreateDirectory(directory);
        var fileName = SanitizeFileName(project.Title) + ".forge";
        var path = Path.Combine(directory, fileName);
        project.Created = project.Saved = DateTime.Now;
        SaveProject(project, path);
    }

    public static void Save()
    {
        if (CurrentProject == null)
            return;

        BeforeSave?.Invoke();

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
                SaveProject(CurrentProject, dialog.FileName);
            }
        }
        else
        {
            SaveProject(CurrentProject, CurrentPath);
        }
    }

    public static void SaveAs()
    {
        if (CurrentProject == null)
            return;

        BeforeSave?.Invoke();

        var dialog = new SaveFileDialog
        {
            Filter = "WordForge Project (*.forge)|*.forge",
            DefaultExt = ".forge",
            InitialDirectory = CurrentPath != null
                ? Path.GetDirectoryName(CurrentPath)
                : Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            FileName = CurrentPath != null
                ? Path.GetFileName(CurrentPath)
                : SanitizeFileName(CurrentProject.Title) + ".forge"
        };
        if (dialog.ShowDialog() == true)
        {
            SaveProject(CurrentProject, dialog.FileName);
        }
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
        project.IsDirty = false;
    }

    private static void SetCurrent(Project project, string path)
    {
        CurrentProject = project;
        CurrentPath = path;
        CurrentProject.IsDirty = false;
        UpdateWindowTitle();
    }

    private static void UpdateWindowTitle()
    {
        if (Application.Current.MainWindow is MainWindow main && CurrentProject != null)
        {
            main.Title = $"WordForge - {CurrentProject.Title}";
        }
    }

    public static string SanitizeFileName(string name)
    {
        foreach (var c in Path.GetInvalidFileNameChars())
            name = name.Replace(c, '_');
        return string.IsNullOrWhiteSpace(name) ? "project" : name;
    }

    public static void SaveProject(Project project, string path)
    {
        if (project == CurrentProject)
            BeforeSave?.Invoke();

        if (!path.EndsWith(".forge", StringComparison.OrdinalIgnoreCase))
            path += ".forge";
        SaveToPath(project, path);
        SetCurrent(project, path);
    }
}

