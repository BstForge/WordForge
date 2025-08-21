using System;
using System.Windows;
using Microsoft.Win32;
using WinForms = System.Windows.Forms;

namespace WordForge;

public partial class NewProjectWindow : Window
{
    public string ProjectTitle => TitleBox.Text.Trim();
    public string ProjectAuthor => AuthorBox.Text.Trim();
    public string ProjectGenre => GenreBox.SelectedItem?.ToString() ?? "";
    public string ProjectLocation => string.IsNullOrWhiteSpace(LocationBox.Text)
        ? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
        : LocationBox.Text;
    public StartupResult Result { get; private set; } = new() { Action = StartupAction.Cancel };

    public NewProjectWindow()
    {
        InitializeComponent();
        GenreBox.ItemsSource = new[] { "Fantasy", "Sci-Fi", "Mystery", "Romance", "Other" };
        GenreBox.SelectedIndex = 0;
        LocationBox.Text = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
    }

    private void Ok_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(ProjectTitle))
        {
            MessageBox.Show(this, "Title is required.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        Result = new StartupResult
        {
            Action = StartupAction.New,
            Title = ProjectTitle,
            Author = ProjectAuthor,
            Genre = ProjectGenre,
            FolderPath = ProjectLocation
        };
        DialogResult = true;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        Result = new StartupResult { Action = StartupAction.Cancel };
        DialogResult = false;
    }

    private void ChooseFolder_Click(object sender, RoutedEventArgs e)
    {
        using var dialog = new WinForms.FolderBrowserDialog { SelectedPath = LocationBox.Text };
        if (dialog.ShowDialog() == WinForms.DialogResult.OK)
        {
            LocationBox.Text = dialog.SelectedPath;
        }
    }

    private void LoadProject_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Filter = "WordForge Project (*.forge)|*.forge",
            DefaultExt = ".forge",
            InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
        };
        if (dialog.ShowDialog() == true)
        {
            Result = new StartupResult
            {
                Action = StartupAction.Load,
                LoadPath = dialog.FileName
            };
            DialogResult = true;
        }
    }
}
