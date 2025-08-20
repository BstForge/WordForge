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
