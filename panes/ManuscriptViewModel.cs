using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using WordForge.models;
using WordForge.Services;

namespace WordForge.Panes
{
    public partial class ManuscriptViewModel : ObservableObject
    {
        public ObservableCollection<ChapterNode> ChapterList => CurrentProjectService.Instance.CurrentProject.Chapters;

        [ObservableProperty]
        private SceneNode selectedScene;
    }
}
