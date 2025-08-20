using System.Windows;

namespace WordForge;

public partial class NewProjectWindow : Window
{
    public string ProjectTitle => TitleBox.Text.Trim();
    public string ProjectAuthor => AuthorBox.Text.Trim();
    public string ProjectGenre => GenreBox.SelectedItem?.ToString() ?? "";

    public NewProjectWindow()
    {
        InitializeComponent();
        GenreBox.ItemsSource = new[] { "Fantasy", "Sci-Fi", "Mystery", "Romance", "Other" };
        GenreBox.SelectedIndex = 0;
    }

    private void Ok_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(ProjectTitle))
        {
            MessageBox.Show(this, "Title is required.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        DialogResult = true;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }
}
