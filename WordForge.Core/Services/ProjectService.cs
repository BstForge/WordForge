using System.IO;
using System.IO.Compression;
using System.Text.Json;
using WordForge.Core.Models;

namespace WordForge.Core.Services;

public static class ProjectService
{
    public static void SaveProject(Project project, string path)
    {
        using var archive = ZipFile.Open(path, ZipArchiveMode.Create);
        var entry = archive.CreateEntry("project.json");
        using var stream = entry.Open();
        JsonSerializer.Serialize(stream, project, new JsonSerializerOptions { WriteIndented = true });
    }

    public static Project LoadProject(string path)
    {
        using var archive = ZipFile.OpenRead(path);
        var entry = archive.GetEntry("project.json")
            ?? throw new FileNotFoundException("project.json not found in archive.");
        using var stream = entry.Open();
        var project = JsonSerializer.Deserialize<Project>(stream);
        return project ?? new Project();
    }
}
