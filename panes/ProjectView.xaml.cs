using System.Windows.Controls;
using System.Windows;
using WordForge.Panes;

namespace WordForge.Panes
{
    public partial class ProjectView : UserControl
    {
        public ProjectView()
        {
            InitializeComponent();
            this.DataContext = new ProjectViewModel();
        }

        private void RecentProjectSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0 && DataContext is ProjectViewModel vm)
            {
                string selectedPath = e.AddedItems[0] as string;
                if (!string.IsNullOrEmpty(selectedPath))
                {
                    vm.SelectRecentCommand.Execute(selectedPath);
                }
            }

            // Deselect after triggering
            if (sender is ListBox listBox)
            {
                listBox.SelectedItem = null;
            }
        }
    }
}
