using System.Windows;
using System.Windows.Controls;
using Fluent;
using WordForge.Panes;

namespace WordForge
{
    public partial class MainWindow : RibbonWindow
    {
        public readonly ProjectViewModel projectViewModel;
        private ProjectView ProjectView;
        private ManuscriptView manuscriptView;

        public MainWindow()
        {
            InitializeComponent();

            projectViewModel = new ProjectViewModel();
            this.DataContext = projectViewModel;

            ProjectView = new ProjectView();
            ShowProjectView();
        }

        public void ShowProjectView()
        {
            MainContent.Content = ProjectView;
        }

        public void SwitchToManuscriptView()
        {
            manuscriptView = new ManuscriptView();
            MainContent.Content = manuscriptView;
        }

        public ProjectViewModel GetProjectViewModel()
        {
            return projectViewModel;
        }
    }
}
