using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using WordForge.models;

namespace WordForge.Panes
{
    public partial class ManuscriptViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<ChapterNode> chapterList;

        [ObservableProperty]
        private SceneNode selectedScene;

        public ManuscriptViewModel()
        {
            chapterList = new ObservableCollection<ChapterNode>();

            var ch1 = new ChapterNode("Chapter 1");
            ch1.Scenes.Add(new SceneNode("Scene 1.1", "Scene 1.1 text..."));
            ch1.Scenes.Add(new SceneNode("Scene 1.2", "Scene 1.2 text..."));

            var ch2 = new ChapterNode("Chapter 2");
            ch2.Scenes.Add(new SceneNode("Scene 2.1"));
            ch2.Scenes.Add(new SceneNode("Scene 2.2"));

            chapterList.Add(ch1);
            chapterList.Add(ch2);
        }
    }
}
