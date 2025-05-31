using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace WordForge.Panes
{
    public partial class ManuscriptViewModel : ObservableObject
    {
        public ObservableCollection<ChapterNode> ChapterList { get; } = new();


        public RelayCommand<ChapterNode> RenameChapterCommand { get; }
        public RelayCommand<ChapterNode> DeleteChapterCommand { get; }
        public RelayCommand<ChapterNode> InsertChapterAboveCommand { get; }
        public RelayCommand<ChapterNode> InsertChapterBelowCommand { get; }

        [ObservableProperty]
        private string editorText;

        public RelayCommand<SceneNode> RenameSceneCommand { get; }
        public RelayCommand<SceneNode> DeleteSceneCommand { get; }
        public RelayCommand<SceneNode> InsertSceneAboveCommand { get; }
        public RelayCommand<SceneNode> InsertSceneBelowCommand { get; }

        public ManuscriptViewModel()
        {
            RenameSceneCommand = new RelayCommand<SceneNode>(OnRenameScene);
            DeleteSceneCommand = new RelayCommand<SceneNode>(OnDeleteScene);
            InsertSceneAboveCommand = new RelayCommand<SceneNode>(OnInsertSceneAbove);
            InsertSceneBelowCommand = new RelayCommand<SceneNode>(OnInsertSceneBelow);

            LoadSampleChapters();
            RenameChapterCommand = new RelayCommand<ChapterNode>(OnRenameChapter);
            DeleteChapterCommand = new RelayCommand<ChapterNode>(OnDeleteChapter);
            InsertChapterAboveCommand = new RelayCommand<ChapterNode>(OnInsertChapterAbove);
            InsertChapterBelowCommand = new RelayCommand<ChapterNode>(OnInsertChapterBelow);

        }

        private void LoadSampleChapters()
        {
            var ch1 = new ChapterNode { Title = "Chapter 1: The Awakening" };
            ch1.Scenes.Add(new SceneNode { Title = "Scene 1.1: A Whisper in the Dark", Content = "Eryndra stirred as the vision took hold..." });
            ch1.Scenes.Add(new SceneNode { Title = "Scene 1.2: Her First Flame", Content = "A spark bloomed in her palm, unbidden." });

            var ch2 = new ChapterNode { Title = "Chapter 2: The Departure" };
            ch2.Scenes.Add(new SceneNode { Title = "Scene 2.1: The Gift Refused", Content = "She turned from the gate and the men who waited." });

            ChapterList.Add(ch1);
            ChapterList.Add(ch2);
        }

        private void OnRenameScene(SceneNode scene)
        {
            System.Windows.MessageBox.Show($"[Rename] {scene?.Title}");
        }

        private void OnDeleteScene(SceneNode scene)
        {
            System.Windows.MessageBox.Show($"[Delete] {scene?.Title}");
        }

        private void OnInsertSceneAbove(SceneNode scene)
        {
            System.Windows.MessageBox.Show($"[Insert Above] {scene?.Title}");
        }

        private void OnInsertSceneBelow(SceneNode scene)
        {
            System.Windows.MessageBox.Show($"[Insert Below] {scene?.Title}");
        }

        private void OnRenameChapter(ChapterNode chapter)
        {
            System.Windows.MessageBox.Show($"[Rename Chapter] {chapter?.Title}");
        }

        private void OnDeleteChapter(ChapterNode chapter)
        {
            System.Windows.MessageBox.Show($"[Delete Chapter] {chapter?.Title}");
        }

        private void OnInsertChapterAbove(ChapterNode chapter)
        {
            System.Windows.MessageBox.Show($"[Insert Chapter Above] {chapter?.Title}");
        }

        private void OnInsertChapterBelow(ChapterNode chapter)
        {
            System.Windows.MessageBox.Show($"[Insert Chapter Below] {chapter?.Title}");
        }

    }
}