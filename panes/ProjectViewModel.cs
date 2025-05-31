using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace WordForge.Panes
{
    public partial class ProjectViewModel : ObservableObject
    {
        [ObservableProperty]
        private string title;

        [ObservableProperty]
        private string series;

        [ObservableProperty]
        private string author;

        [ObservableProperty]
        private string selectedRecent;

        public ObservableCollection<string> RecentProjects { get; } = new();

        public ICommand CreateCommand { get; }
        public ICommand LoadCommand { get; }
        public ICommand NewCommand { get; }
        public ICommand SelectRecentCommand { get; }

        public ProjectViewModel()
        {
            CreateCommand = new RelayCommand(OnCreate);
            LoadCommand = new RelayCommand(OnLoad);
            NewCommand = new RelayCommand(OnNew);
            SelectRecentCommand = new RelayCommand<string>(OnSelectRecent);

            LoadRecentProjects();
        }

        private void OnCreate()
        {
            if (string.IsNullOrWhiteSpace(Title) || string.IsNullOrWhiteSpace(Author))
            {
                System.Windows.MessageBox.Show("Title and Author are required.", "Missing Information");
                return;
            }

            var dialog = new Ookii.Dialogs.Wpf.VistaSaveFileDialog
            {
                Filter = "WordForge Project (*.forge)|*.forge",
                DefaultExt = ".forge",
                AddExtension = true,
                Title = "Save New Project As..."
            };

            bool? result = dialog.ShowDialog();
            if (result != true)
                return;

            string path = dialog.FileName;

            try
            {
                System.IO.File.WriteAllText(path, $"# WordForge Project\nTitle: {Title}\nAuthor: {Author}\nSeries: {Series}");
                Services.RecentProjectsService.Add(path);
                System.Windows.Application.Current.MainWindow?.GetType()
                    .GetMethod("SwitchToManuscriptView")
                    ?.Invoke(System.Windows.Application.Current.MainWindow, null);
            }
            catch (System.Exception ex)
            {
                System.Windows.MessageBox.Show($"Failed to save project: {ex.Message}", "Error");
            }
        }

        private void OnLoad()
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
                System.Windows.Application.Current.MainWindow?.GetType()
                    .GetMethod("SwitchToManuscriptView")
                    ?.Invoke(System.Windows.Application.Current.MainWindow, null);
            }
        }

        private void OnNew()
        {
            Title = string.Empty;
            Series = string.Empty;
            Author = string.Empty;
        }

        private void OnSelectRecent(string path)
        {
            if (System.IO.File.Exists(path))
            {
                Services.RecentProjectsService.Add(path);
                System.Windows.Application.Current.MainWindow?.GetType()
                    .GetMethod("SwitchToManuscriptView")
                    ?.Invoke(System.Windows.Application.Current.MainWindow, null);
            }
        }

        private void LoadRecentProjects()
        {
            // Temporary stub
            RecentProjects.Clear();
            RecentProjects.Add("Project1.forge");
            RecentProjects.Add("Project2.forge");
            RecentProjects.Add("Project3.forge");
        }
    }
}
