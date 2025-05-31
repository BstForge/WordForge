using System.Collections.ObjectModel;

namespace WordForge.Panes
{
    public class ChapterNode
    {
        public string Title { get; set; }
        public ObservableCollection<SceneNode> Scenes { get; set; } = new();
    }

    public class SceneNode
    {
        public string Title { get; set; }
        public string Content { get; set; }
    }
}
