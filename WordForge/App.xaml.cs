using System.Windows;

namespace WordForge;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;

        while (true)
        {
            var window = new NewProjectWindow();
            if (window.ShowDialog() != true)
            {
                Shutdown();
                return;
            }

            if (window.IsLoad)
            {
                if (!string.IsNullOrWhiteSpace(window.LoadedForgePath) && ProjectService.LoadFromFile(window.LoadedForgePath))
                {
                    break;
                }
                MessageBox.Show("Failed to load project.", "Load", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                var project = new Project
                {
                    Title = window.ProjectTitle,
                    Author = window.ProjectAuthor,
                    Genre = window.ProjectGenre
                };
                var chapter = new Chapter { Title = "Chapter 1" };
                chapter.Scenes.Add(new Scene { Title = "Scene 1" });
                project.Chapters.Add(chapter);
                ProjectService.StartNew(project, window.ProjectLocation);
                break;
            }
        }

        var main = new MainWindow();
        MainWindow = main;
        main.Show();
        Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
        ProjectService.InitializeUI();
    }
}
