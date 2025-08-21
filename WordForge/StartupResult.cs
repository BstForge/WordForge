namespace WordForge;

public enum StartupAction
{
    New,
    Load,
    Cancel
}

public record StartupResult
{
    public StartupAction Action { get; init; }
    public string? Title { get; init; }
    public string? Author { get; init; }
    public string? Genre { get; init; }
    public string? FolderPath { get; init; }
    public string? LoadPath { get; init; }
}
