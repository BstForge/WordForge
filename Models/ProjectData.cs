using System.Collections.Generic;
using WordForge.Panes;

namespace WordForge.Models
{
    public class ProjectData
    {
        public string Title { get; set; }
        public string Series { get; set; }
        public string Author { get; set; }
        public List<ChapterNode> Chapters { get; set; } = new();
    }
}
