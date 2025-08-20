using System;
using System.IO;
using System.Windows;
using WinForms = System.Windows.Forms;

namespace WordForge;

public partial class PropertiesWindow : Window
{
    public PropertiesWindow()
    {
        InitializeComponent();
        GenreBox.ItemsSource = new[] { "Fantasy", "Sci-Fi", "Mystery", "Romance", "Other" };

        var project = ProjectService.CurrentProject;
        TitleBox.Text = project.Title;
        AuthorBox.Text = project.Author;
        GenreBox.SelectedItem = project.Genre;

        var location = ProjectService.CurrentPath != null
            ? Path.GetDirectoryName(ProjectService.CurrentPath)
            : Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        LocationBox.Text = location;

        TransitionsToggle.IsChecked = TransitionService.TransitionsEnabled;
        ToggleStateText.Text = TransitionService.TransitionsEnabled ? "On" : "Off";
        TransitionsToggle.Checked += ToggleChanged;
        TransitionsToggle.Unchecked += ToggleChanged;
    }

    private void ToggleChanged(object? sender, RoutedEventArgs e)
    {
        bool enabled = TransitionsToggle.IsChecked == true;
        TransitionService.TransitionsEnabled = enabled;
        ToggleStateText.Text = enabled ? "On" : "Off";
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        var project = ProjectService.CurrentProject;
        project.Title = TitleBox.Text.Trim();
        project.Author = AuthorBox.Text.Trim();
        project.Genre = GenreBox.SelectedItem?.ToString() ?? "";

        var directory = string.IsNullOrWhiteSpace(LocationBox.Text)
            ? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            : LocationBox.Text;
        Directory.CreateDirectory(directory);
        var fileName = ProjectService.SanitizeFileName(project.Title) + ".forge";
        var path = Path.Combine(directory, fileName);
        ProjectService.SaveProject(project, path);
        DialogResult = true;
    }

    private void ChooseFolder_Click(object sender, RoutedEventArgs e)
    {
        using var dialog = new WinForms.FolderBrowserDialog { SelectedPath = LocationBox.Text };
        if (dialog.ShowDialog() == WinForms.DialogResult.OK)
        {
            LocationBox.Text = dialog.SelectedPath;
        }
    }
}
