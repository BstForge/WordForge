using System.Windows;
using System.Windows.Controls;

namespace WordForge.Menus.TopMenu;

public partial class TimelineMenu : UserControl
{
    public TimelineMenu()
    {
        InitializeComponent();
        HamburgerButton.Click += HamburgerButton_Click;
    }

    private void HamburgerButton_Click(object sender, RoutedEventArgs e)
    {
        HamburgerPopup.IsOpen = !HamburgerPopup.IsOpen;
    }
}
