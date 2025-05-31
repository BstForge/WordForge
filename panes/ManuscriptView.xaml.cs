using System.Windows;
using System.Windows.Controls;

namespace WordForge.Panes
{
    public partial class ManuscriptView : UserControl
    {
        private ManuscriptViewModel vm;
        private SceneNode currentScene;

        public ManuscriptView()
        {
            InitializeComponent();
            vm = new ManuscriptViewModel();
            DataContext = vm;

            Editor.TextChanged += Editor_TextChanged;
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            // Save edits from previous scene
            if (currentScene != null)
            {
                currentScene.Content = Editor.Text;
                vm.IsDirty = true;
            }

            // Switch to new scene
            if (e.NewValue is SceneNode scene)
            {
                currentScene = scene;
                vm.EditorText = scene.Content;
                Editor.Text = scene.Content;
            }
            else
            {
                currentScene = null;
                Editor.Text = string.Empty;
            }
        }

        private void Editor_TextChanged(object sender, System.EventArgs e)
        {
            if (currentScene != null && Editor.IsFocused)
            {
                currentScene.Content = Editor.Text;
                vm.IsDirty = true;
            }
        }

        internal void ForceSync()
        {
            if (currentScene != null)
            {
                currentScene.Content = Editor.Text;
            }
        }

        public void ClearEditor()
        {
            currentScene = null;
            Editor.Text = string.Empty;
        }
    }
}
