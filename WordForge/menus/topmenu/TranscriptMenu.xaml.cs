using System.Windows;
using System.Windows.Controls;

namespace WordForge.Menus.TopMenu;

public partial class TranscriptMenu : UserControl
{
    public TranscriptMenu()
    {
        InitializeComponent();
        HamburgerButton.Click += HamburgerButton_Click;
    }

    private void HamburgerButton_Click(object sender, RoutedEventArgs e)
    {
        HamburgerPopup.IsOpen = !HamburgerPopup.IsOpen;
    }
}
