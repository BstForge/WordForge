using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.IO.Compression;
using System.Windows;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Linq;

namespace WordForge;

public enum AutosaveMode
{
    Interval,
    OnPaneChange
}

public record AutosaveSettings
{
    public bool Enabled { get; set; }
    public AutosaveMode Mode { get; set; } = AutosaveMode.Interval;
    public int Minutes { get; set; } = 5;
}

public record Project
{
    public string Title { get; set; } = "";
    public string Author { get; set; } = "";
    public string Genre { get; set; } = "";
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    public DateTime ModifiedUtc { get; set; } = DateTime.UtcNow;
    public bool Transitions { get; set; } = true;
    public AutosaveSettings Autosave { get; set; } = new();
    public ObservableCollection<Chapter> Chapters { get; set; } = new();
    public bool IsDirty { get; set; } = false;
}

public static class ProjectService
{
    public static event Action? BeforeSave;
    public static Project CurrentProject { get; private set; } = new Project
    {
        Title = "New Project",
        CreatedUtc = DateTime.UtcNow,
        ModifiedUtc = DateTime.UtcNow
    };

    public static string? CurrentPath { get; private set; } = null;

    public static void CreateNew(Project project, string? directory)
    {
        directory = string.IsNullOrWhiteSpace(directory)
            ? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            : directory;
        Directory.CreateDirectory(directory);
        var fileName = SanitizeFileName(project.Title) + ".forge";
        var path = Path.Combine(directory, fileName);
        project.CreatedUtc = project.ModifiedUtc = DateTime.UtcNow;
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
                : SanitizeFileName(CurrentProject.Title) + ".forge",
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
            var path = dialog.FileName;
            var autosave = GetAutosavePath(path);
            if (File.Exists(autosave) && File.GetLastWriteTime(autosave) > File.GetLastWriteTime(path))
            {
                if (MessageBox.Show("A newer autosave exists. Recover?", "Autosave", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    try
                    {
                        var proj = LoadFromPath(autosave);
                        if (proj != null)
                        {
                            SetCurrent(proj, path);
                            CurrentProject.IsDirty = true;
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Load", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            try
            {
                var project = LoadFromPath(path);
                if (project != null)
                {
                    SetCurrent(project, path);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Load", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    internal static void SaveToPath(Project project, string path, bool markClean = true)
    {
        project.ModifiedUtc = DateTime.UtcNow;
        var temp = path + ".tmp";
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        using (var fs = new FileStream(temp, FileMode.Create, FileAccess.Write, FileShare.None))
        using (var archive = new ZipArchive(fs, ZipArchiveMode.Create))
        {
            var projectEntry = archive.CreateEntry("project.json");
            using (var writer = new StreamWriter(projectEntry.Open(), new UTF8Encoding(false)))
            {
                writer.NewLine = "\n";
                var json = JsonSerializer.Serialize(new
                {
                    schemaVersion = 1,
                    title = project.Title,
                    author = project.Author,
                    genre = project.Genre,
                    autosave = project.Autosave,
                    transitions = project.Transitions,
                    createdUtc = project.CreatedUtc,
                    modifiedUtc = project.ModifiedUtc
                }, new JsonSerializerOptions { WriteIndented = true });
                writer.Write(json);
            }

            var indexEntry = archive.CreateEntry("transcript/index.json");
            using (var writer = new StreamWriter(indexEntry.Open(), new UTF8Encoding(false)))
            {
                writer.NewLine = "\n";
                var index = project.Chapters.Select(c => new { id = c.Id, title = c.Title }).ToList();
                var json = JsonSerializer.Serialize(index, new JsonSerializerOptions { WriteIndented = true });
                writer.Write(json);
            }

            foreach (var ch in project.Chapters)
            {
                ch.UpdatedUtc = DateTime.UtcNow;
                var entry = archive.CreateEntry($"transcript/chapters/ch_{ch.Id}.json");
                using var writer = new StreamWriter(entry.Open(), new UTF8Encoding(false));
                writer.NewLine = "\n";
                var obj = new
                {
                    id = ch.Id,
                    title = ch.Title,
                    createdUtc = ch.CreatedUtc,
                    updatedUtc = ch.UpdatedUtc,
                    scenes = ch.Scenes.Select(s => new { title = s.Title, text = s.Text }).ToList()
                };
                var json = JsonSerializer.Serialize(obj, new JsonSerializerOptions { WriteIndented = true });
                writer.Write(json);
            }

            CreateEmpty(archive, "bibles/characters/index.json", "[]");
            archive.CreateEntry("bibles/characters/entries/");
            CreateEmpty(archive, "bibles/locations/index.json", "[]");
            archive.CreateEntry("bibles/locations/entries/");
            CreateEmpty(archive, "bibles/items/index.json", "[]");
            archive.CreateEntry("bibles/items/entries/");
            CreateEmpty(archive, "bibles/lore/index.json", "[]");
            archive.CreateEntry("bibles/lore/entries/");
            CreateEmpty(archive, "outline/outline.json", "{}");
            CreateEmpty(archive, "timeline/timeline.json", "{}");
            archive.CreateEntry("assets/");
        }

        if (File.Exists(path))
            File.Delete(path);
        File.Move(temp, path);

        if (markClean)
            project.IsDirty = false;
    }

    private static void CreateEmpty(ZipArchive archive, string path, string content)
    {
        var entry = archive.CreateEntry(path);
        using var writer = new StreamWriter(entry.Open(), new UTF8Encoding(false));
        writer.NewLine = "\n";
        writer.Write(content);
    }

    private static Project? LoadFromPath(string path)
    {
        using var archive = ZipFile.OpenRead(path);
        var projectEntry = archive.GetEntry("project.json") ?? throw new FileNotFoundException("project.json missing");
        Project? project;
        using (var reader = new StreamReader(projectEntry.Open()))
        {
            var json = reader.ReadToEnd();
            var doc = JsonSerializer.Deserialize<ProjectFile>(json);
            if (doc == null) return null;
            project = new Project
            {
                Title = doc.title ?? "",
                Author = doc.author ?? "",
                Genre = doc.genre ?? "",
                CreatedUtc = doc.createdUtc,
                ModifiedUtc = doc.modifiedUtc,
                Transitions = doc.transitions,
                Autosave = doc.autosave ?? new AutosaveSettings()
            };
        }

        var indexEntry = archive.GetEntry("transcript/index.json");
        List<ChapterIndex> index = new();
        if (indexEntry != null)
        {
            using var reader = new StreamReader(indexEntry.Open());
            var json = reader.ReadToEnd();
            index = JsonSerializer.Deserialize<List<ChapterIndex>>(json) ?? new();
        }
        else
        {
            index = archive.Entries.Where(e => e.FullName.StartsWith("transcript/chapters/ch_") && e.FullName.EndsWith(".json"))
                .Select(e =>
                {
                    using var r = new StreamReader(e.Open());
                    var ch = JsonSerializer.Deserialize<ChapterFile>(r.ReadToEnd());
                    return ch != null ? new ChapterIndex { id = ch.id, title = ch.title } : null;
                })
                .Where(i => i != null)
                .Cast<ChapterIndex>()
                .ToList();
        }

        foreach (var idx in index)
        {
            try
            {
                var entry = archive.GetEntry($"transcript/chapters/ch_{idx.id}.json");
                if (entry == null) continue;
                using var reader = new StreamReader(entry.Open());
                var json = reader.ReadToEnd();
                var file = JsonSerializer.Deserialize<ChapterFile>(json);
                if (file == null) continue;
                var chapter = new Chapter
                {
                    Id = file.id ?? Guid.NewGuid().ToString(),
                    Title = file.title ?? "Chapter",
                    CreatedUtc = file.createdUtc,
                    UpdatedUtc = file.updatedUtc
                };
                foreach (var s in file.scenes ?? new List<SceneFile>())
                    chapter.Scenes.Add(new Scene { Title = s.title ?? "Scene", Text = s.text ?? string.Empty });
                project.Chapters.Add(chapter);
            }
            catch
            {
                // skip malformed chapter
            }
        }

        return project;
    }

    private class ProjectFile
    {
        public int schemaVersion { get; set; }
        public string? title { get; set; }
        public string? author { get; set; }
        public string? genre { get; set; }
        public AutosaveSettings? autosave { get; set; }
        public bool transitions { get; set; }
        [JsonPropertyName("createdUtc")] public DateTime createdUtc { get; set; }
        [JsonPropertyName("modifiedUtc")] public DateTime modifiedUtc { get; set; }
    }

    private class ChapterIndex { public string id { get; set; } = ""; public string title { get; set; } = ""; }
    private class SceneFile { public string? title { get; set; } public string? text { get; set; } }
    private class ChapterFile
    {
        public string? id { get; set; }
        public string? title { get; set; }
        public DateTime createdUtc { get; set; }
        public DateTime updatedUtc { get; set; }
        public List<SceneFile>? scenes { get; set; }
    }

    private static void SetCurrent(Project project, string path)
    {
        CurrentProject = project;
        CurrentPath = path;
        CurrentProject.IsDirty = false;
        TransitionService.TransitionsEnabled = project.Transitions;
        UpdateWindowTitle();
        AutosaveService.Configure(project);
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
        AutosaveService.OnManualSave();
    }

    internal static string GetAutosavePath(string path)
    {
        var dir = Path.GetDirectoryName(path)!;
        var file = Path.GetFileNameWithoutExtension(path) + ".autosave.forge";
        return Path.Combine(dir, file);
    }

    public static void SaveCopy(Project project, string path) => SaveToPath(project, path, false);
}

