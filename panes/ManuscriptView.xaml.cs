using System.Windows;
using System.Windows.Controls;
using WordForge.models;
using WordForge.Services;

namespace WordForge.Panes
{
    public partial class ManuscriptView : UserControl
    {
        private readonly ManuscriptViewModel viewModel;

        public ManuscriptView()
        {
            InitializeComponent();
            viewModel = new ManuscriptViewModel();
            viewModel.SelectedScene = null;
            DataContext = viewModel;
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (viewModel.SelectedScene != null)
            {
                viewModel.SelectedScene.Content = Editor.Text;
            }

            if (e.NewValue is SceneNode newScene)
            {
                viewModel.SelectedScene = newScene;
                Editor.Text = newScene.Content ?? string.Empty;
            }
            else
            {
                Editor.Text = string.Empty;
            }
        }

        private void Editor_LostFocus(object sender, RoutedEventArgs e)
        {
            if (viewModel.SelectedScene != null)
            {
                viewModel.SelectedScene.Content = Editor.Text;
            }
        }

        private void TreeView_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is TreeView tree)
            {
                ExpandAll(tree);
            }
        }

        private void ExpandAll(ItemsControl parent)
        {
            foreach (var item in parent.Items)
            {
                var container = parent.ItemContainerGenerator.ContainerFromItem(item) as TreeViewItem;
                if (container != null)
                {
                    container.IsExpanded = true;
                    ExpandAll(container);
                }
            }
        }
    }
}
