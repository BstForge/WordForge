using System.Windows;
using System.Windows.Controls;
using WordForge;

namespace WordForge.Menus;

public partial class FileMenu : UserControl
{
    public FileMenu()
    {
        InitializeComponent();
    }

    private void New_Click(object sender, RoutedEventArgs e)
    {
        var window = new NewProjectWindow { Owner = Application.Current.MainWindow };
        if (window.ShowDialog() == true)
        {
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
        }
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        ProjectService.Save();
    }

    private void SaveAs_Click(object sender, RoutedEventArgs e)
    {
        ProjectService.SaveAs();
    }

    private void Load_Click(object sender, RoutedEventArgs e)
    {
        ProjectService.Load();
    }

    private void Properties_Click(object sender, RoutedEventArgs e)
    {
        var window = new PropertiesWindow { Owner = Application.Current.MainWindow };
        window.ShowDialog();
    }

    private void Exit_Click(object sender, RoutedEventArgs e)
    {
        Application.Current.Shutdown();
    }
}
