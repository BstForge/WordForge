namespace WordForge.models
{
    public class SceneNode
    {
        public string Title { get; set; }

        public string Content { get; set; }

        public SceneNode() { }

        public SceneNode(string title, string content = "")
        {
            Title = title;
            Content = content;
        }
    }
}
