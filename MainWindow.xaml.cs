using System.Windows.Controls;
using System.Windows;
using Fluent;
using WordForge.Panes;
using WordForge.Models;
using System.Text.Json;
using System.IO;
using WordForge.Services;

namespace WordForge
{
    public partial class MainWindow : RibbonWindow
    {
        private ProjectView ProjectView;
        private ManuscriptView _manuscriptView;
        private ManuscriptViewModel _manuscriptViewModel;

        public MainWindow()
        {
            InitializeComponent();
            _manuscriptViewModel = new ManuscriptViewModel();
            _manuscriptView = new ManuscriptView { DataContext = _manuscriptViewModel };
            ProjectView = new ProjectView();
            ShowProjectView();
        }

        private void ShowProjectView()
        {
            MainContent.Content = ProjectView;
        }

        private void OnNewProjectClick(object sender, RoutedEventArgs e)
        {
            _manuscriptViewModel.ChapterList.Clear();
            _manuscriptView.ClearEditor();
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
            if (result == true && File.Exists(dialog.FileName))
            {
                RecentProjectsService.Add(dialog.FileName);
                LoadProjectFromFile(dialog.FileName);
            }
        }

        private void LoadProjectFromFile(string path)
        {
            try
            {
                string json = File.ReadAllText(path);
                var project = JsonSerializer.Deserialize<ProjectData>(json);

                _manuscriptViewModel.ChapterList.Clear();
                _manuscriptView.ClearEditor();

                foreach (var chapter in project.Chapters)
                {
                    var chapterNode = new ChapterNode { Title = chapter.Title };

                    foreach (var scene in chapter.Scenes)
                    {
                        chapterNode.Scenes.Add(new SceneNode
                        {
                            Title = scene.Title,
                            Content = scene.Content
                        });
                    }

                    _manuscriptViewModel.ChapterList.Add(chapterNode);
                }

                MainContent.Content = _manuscriptView;

                // Update title/author view
                ProjectView = new ProjectView();
                ProjectView.DataContext = new ProjectViewModel
                {
                    Title = project.Title,
                    Author = project.Author
                };
            }
            catch
            {
                MessageBox.Show("Failed to load project file.", "Error");
            }
        }

        private void OnSaveClick(object sender, RoutedEventArgs e)
        {
            SaveProject();
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
            MainContent.Content = _manuscriptView;
        }

        private void SaveProject()
        {
            string path = RecentProjectsService.GetCurrentFile();
            if (string.IsNullOrEmpty(path)) return;

            if (_manuscriptViewModel != null)
            {
                var project = new ProjectData
                {
                    Title = ProjectView?.ProjectTitle ?? "Untitled",
                    Author = ProjectView?.ProjectAuthor ?? "Unknown"
                };

                foreach (var chapter in _manuscriptViewModel.ChapterList)
                {
                    var chapterModel = new ChapterModel
                    {
                        Title = chapter.Title
                    };

                    foreach (var scene in chapter.Scenes)
                    {
                        chapterModel.Scenes.Add(new SceneModel
                        {
                            Title = scene.Title,
                            Content = scene.Content
                        });
                    }

                    project.Chapters.Add(chapterModel);
                }

                var json = JsonSerializer.Serialize(project, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(path, json);
                _manuscriptViewModel.IsDirty = false;
            }
        }
    }
}
