using System.Collections.Generic;

namespace WordForge.Core.Models;

public class Project
{
    public string Title { get; set; } = string.Empty;
    public List<Chapter> Chapters { get; set; } = new();
}
