using System.Collections.ObjectModel;

namespace WordForge;

public class Scene
{
    public string Title { get; set; } = "Scene";
    public string Text { get; set; } = string.Empty;
}

public class Chapter
{
    public string Title { get; set; } = "Chapter";
    public ObservableCollection<Scene> Scenes { get; set; } = new();
}
