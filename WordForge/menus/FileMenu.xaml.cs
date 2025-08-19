using System.Windows;
using System.Windows.Controls;

namespace WordForge.Menus;

public partial class FileMenu : UserControl
{
    public FileMenu()
    {
        InitializeComponent();
    }

    private void Exit_Click(object sender, RoutedEventArgs e)
    {
        Application.Current.Shutdown();
    }
}
