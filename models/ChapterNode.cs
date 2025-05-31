using System.Collections.ObjectModel;

namespace WordForge.models
{
    public class ChapterNode
    {
        public string Title { get; set; }

        public ObservableCollection<SceneNode> Scenes { get; set; } = new();

        public ChapterNode() { }

        public ChapterNode(string title)
        {
            Title = title;
        }
    }
}
