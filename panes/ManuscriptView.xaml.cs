using System.Windows;
using System.Windows.Controls;

namespace WordForge.Panes
{
    public partial class ManuscriptView : UserControl
    {
        private ManuscriptViewModel vm;

        public ManuscriptView()
        {
            InitializeComponent();
            vm = new ManuscriptViewModel();
            DataContext = vm;
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is SceneNode scene)
            {
                vm.EditorText = scene.Content;
                Editor.Text = scene.Content;
            }
            else
            {
                Editor.Text = string.Empty;
            }
        }
    }
}
