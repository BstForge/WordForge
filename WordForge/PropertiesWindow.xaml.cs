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

        TransitionsToggle.IsChecked = project.Transitions;
        ToggleStateText.Text = project.Transitions ? "On" : "Off";
        TransitionsToggle.Checked += ToggleChanged;
        TransitionsToggle.Unchecked += ToggleChanged;

        AutosaveToggle.IsChecked = project.Autosave.Enabled;
        AutosaveStateText.Text = project.Autosave.Enabled ? "On" : "Off";
        AutosaveToggle.Checked += AutosaveToggleChanged;
        AutosaveToggle.Unchecked += AutosaveToggleChanged;
        AutosaveModeBox.SelectedIndex = project.Autosave.Mode == AutosaveMode.Interval ? 0 : 1;
        AutosaveMinutesBox.Text = project.Autosave.Minutes.ToString();
        AutosaveModeBox.SelectionChanged += (_, __) => UpdateAutosaveVisibility();
        UpdateAutosaveVisibility();
    }

    private void ToggleChanged(object? sender, RoutedEventArgs e)
    {
        bool enabled = TransitionsToggle.IsChecked == true;
        TransitionService.TransitionsEnabled = enabled;
        ProjectService.CurrentProject.Transitions = enabled;
        ToggleStateText.Text = enabled ? "On" : "Off";
    }

    private void AutosaveToggleChanged(object? sender, RoutedEventArgs e)
    {
        bool enabled = AutosaveToggle.IsChecked == true;
        AutosaveStateText.Text = enabled ? "On" : "Off";
        UpdateAutosaveVisibility();
    }

    private void UpdateAutosaveVisibility()
    {
        if (AutosaveToggle.IsChecked == true && AutosaveModeBox.SelectedIndex == 0)
            AutosaveMinutesPanel.Visibility = Visibility.Visible;
        else
            AutosaveMinutesPanel.Visibility = Visibility.Collapsed;
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        var project = ProjectService.CurrentProject;
        project.Title = TitleBox.Text.Trim();
        project.Author = AuthorBox.Text.Trim();
        project.Genre = GenreBox.SelectedItem?.ToString() ?? "";
        project.Transitions = TransitionsToggle.IsChecked == true;
        project.Autosave.Enabled = AutosaveToggle.IsChecked == true;
        project.Autosave.Mode = AutosaveModeBox.SelectedIndex == 0 ? AutosaveMode.Interval : AutosaveMode.OnPaneChange;
        if (int.TryParse(AutosaveMinutesBox.Text, out int mins))
            project.Autosave.Minutes = mins;

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
