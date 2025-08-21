using System.Windows;

namespace WordForge;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var window = new NewProjectWindow();
        if (window.ShowDialog() == true)
        {
            var main = new MainWindow();
            MainWindow = main;
            if (!window.IsLoad)
            {
                var project = new Project
                {
                    Title = window.ProjectTitle,
                    Author = window.ProjectAuthor,
                    Genre = window.ProjectGenre
                };
                ProjectService.CreateNew(project, window.ProjectLocation);
            }
            main.Show();
            ProjectService.InitializeUI();
        }
        else
        {
            Shutdown();
        }
    }
}
