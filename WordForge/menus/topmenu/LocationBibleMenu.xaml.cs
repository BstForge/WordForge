using System.Windows;
using System.Windows.Controls;

namespace WordForge.Menus.TopMenu;

public partial class LocationBibleMenu : UserControl
{
    public LocationBibleMenu()
    {
        InitializeComponent();
        HamburgerButton.Click += HamburgerButton_Click;
    }

    private void HamburgerButton_Click(object sender, RoutedEventArgs e)
    {
        HamburgerPopup.IsOpen = !HamburgerPopup.IsOpen;
    }
}
