using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using Ookii.Dialogs.Wpf;
using WordForge.models;
using WordForge.Services;
using WordForge.Panes;

namespace WordForge.Panes
{
    public partial class ProjectViewModel : ObservableObject
    {
        [ObservableProperty] private string title;
        [ObservableProperty] private string series;
        [ObservableProperty] private string author;
        [ObservableProperty] private string selectedRecent;

        public ObservableCollection<string> RecentProjects { get; } = new();

        public ICommand CreateCommand { get; }
        public ICommand LoadCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand NewCommand { get; }
        public ICommand SelectRecentCommand { get; }

        public ProjectViewModel()
        {
            CreateCommand = new RelayCommand(OnCreate);
            LoadCommand = new RelayCommand(OnLoad);
            SaveCommand = new RelayCommand(OnSave);
            NewCommand = new RelayCommand(OnNew);
            SelectRecentCommand = new RelayCommand<string>(OnSelectRecent);

            LoadRecentProjects();
        }

        private void OnCreate()
        {
            if (string.IsNullOrWhiteSpace(Title) || string.IsNullOrWhiteSpace(Author))
            {
                MessageBox.Show("Title and Author are required.", "Missing Information");
                return;
            }

            var dialog = new VistaSaveFileDialog
            {
                Filter = "WordForge Project (*.forge)|*.forge",
                DefaultExt = ".forge",
                AddExtension = true,
                Title = "Save New Project As..."
            };

            if (dialog.ShowDialog() != true) return;

            try
            {
                CurrentProjectService.Instance.CreateNew(Title, Author, Series, dialog.FileName);
                CurrentProjectService.Instance.Save();
                Services.RecentProjectsService.Add(dialog.FileName);

                if (Application.Current.MainWindow is WordForge.MainWindow mw)
                    mw.SwitchToManuscriptView();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Failed to save project: {ex.Message}", "Error");
            }
        }

        private void OnLoad()
        {
            var dialog = new VistaOpenFileDialog
            {
                Filter = "WordForge Project (*.forge)|*.forge",
                Title = "Open Project"
            };

            if (dialog.ShowDialog() != true || !System.IO.File.Exists(dialog.FileName)) return;

            try
            {
                string json = System.IO.File.ReadAllText(dialog.FileName);
                var project = JsonSerializer.Deserialize<ProjectData>(json);
                CurrentProjectService.Instance.Load(project, dialog.FileName);
                Services.RecentProjectsService.Add(dialog.FileName);

                if (Application.Current.MainWindow is WordForge.MainWindow mw)
                    mw.SwitchToManuscriptView();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Failed to load project: {ex.Message}", "Load Error");
            }
        }

        private void OnSave()
        {
            try
            {
                if (Application.Current.MainWindow is WordForge.MainWindow mw &&
                    mw.MainContent.Content is ManuscriptView mv &&
                    mv.DataContext is ManuscriptViewModel vm &&
                    vm.SelectedScene != null)
                {
                    vm.SelectedScene.Content = mv.Editor.Text;
                    MessageBox.Show("Saving content: " + vm.SelectedScene.Content);
                }

                CurrentProjectService.Instance.Save();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Failed to save project: {ex.Message}", "Save Error");
            }
        }

        private void OnNew()
        {
            Title = string.Empty;
            Series = string.Empty;
            Author = string.Empty;

            if (Application.Current.MainWindow is WordForge.MainWindow mw)
                mw.ShowProjectView();
        }

        private void OnSelectRecent(string path)
        {
            if (System.IO.File.Exists(path))
            {
                Services.RecentProjectsService.Add(path);
                if (Application.Current.MainWindow is WordForge.MainWindow mw)
                    mw.SwitchToManuscriptView();
            }
        }

        private void LoadRecentProjects()
        {
            RecentProjects.Clear();
            RecentProjects.Add("Project1.forge");
            RecentProjects.Add("Project2.forge");
            RecentProjects.Add("Project3.forge");
        }
    }
}
