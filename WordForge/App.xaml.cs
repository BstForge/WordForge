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
            window.ShowDialog();
            var result = window.Result;

            if (result.Action == StartupAction.Cancel)
            {
                Shutdown();
                return;
            }

            if (result.Action == StartupAction.New)
            {
                var project = new Project
                {
                    Title = result.Title ?? string.Empty,
                    Author = result.Author ?? string.Empty,
                    Genre = result.Genre ?? string.Empty
                };
                var chapter = new Chapter { Title = "Chapter 1" };
                chapter.Scenes.Add(new Scene { Title = "Scene 1" });
                project.Chapters.Add(chapter);
                ProjectService.StartNew(project, result.FolderPath);
                break;
            }

            if (result.Action == StartupAction.Load)
            {
                if (!string.IsNullOrWhiteSpace(result.LoadPath) && ProjectService.LoadFromFile(result.LoadPath))
                {
                    break;
                }
                MessageBox.Show("Failed to load project.", "Load", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        var main = new MainWindow();
        MainWindow = main;
        main.Show();
        Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
        ProjectService.InitializeUI();
    }
}
