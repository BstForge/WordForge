using System.Collections.Generic;

namespace WordForge.Core.Models;

public class Chapter
{
    public string Title { get; set; } = string.Empty;
    public List<Scene> Scenes { get; set; } = new();
}
