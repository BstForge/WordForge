using System.Windows.Controls;

namespace WordForge.Panes
{
    public partial class ProjectView : UserControl
    {
        public ProjectView()
        {
            InitializeComponent();
        }

        private void RecentProjects_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataContext is ProjectViewModel viewModel && viewModel.SelectedRecent != null)
            {
                viewModel.SelectRecentCommand.Execute(viewModel.SelectedRecent);
            }
        }
    }
}
