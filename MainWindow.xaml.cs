using System.Windows.Controls;
using System.Windows;
using Fluent;
using WordForge.Panes;

namespace WordForge
{
    public partial class MainWindow : RibbonWindow
    {
        private ProjectView ProjectView;

        public MainWindow()
        {
            InitializeComponent();
            ProjectView = new ProjectView();
            ShowProjectView();
        }

        private void ShowProjectView()
        {
            MainContent.Content = ProjectView;
        }

        private void OnNewProjectClick(object sender, RoutedEventArgs e)
        {
            ShowProjectView();
        }

        private void OnLoadProjectClick(object sender, RoutedEventArgs e)
        {
            var dialog = new Ookii.Dialogs.Wpf.VistaOpenFileDialog
            {
                Filter = "WordForge Project (*.forge)|*.forge",
                Title = "Open Project"
            };

            bool? result = dialog.ShowDialog();
            if (result == true && System.IO.File.Exists(dialog.FileName))
            {
                Services.RecentProjectsService.Add(dialog.FileName);
                SwitchToManuscriptView();
            }
        }

        private void OnSaveClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Save functionality coming soon.", "Not Implemented");
        }

        private void OnExportClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Export functionality coming soon.", "Not Implemented");
        }

        private void OnPrintClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Print functionality coming soon.", "Not Implemented");
        }

        private void OnExitClick(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        public void SwitchToManuscriptView()
        {
            var view = new ManuscriptView();
            MainContent.Content = view;
        }
    }
}
