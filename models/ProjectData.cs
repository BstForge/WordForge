using System.Collections.ObjectModel;

namespace WordForge.models
{
    public class ProjectData
    {
        public string Title { get; set; }

        public string Series { get; set; }

        public string Author { get; set; }

        public ObservableCollection<ChapterNode> Chapters { get; set; } = new();
    }
}
