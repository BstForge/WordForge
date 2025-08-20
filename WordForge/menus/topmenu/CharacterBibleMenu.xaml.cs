using System.Windows;
using System.Windows.Controls;

namespace WordForge.Menus.TopMenu;

public partial class CharacterBibleMenu : UserControl
{
    public CharacterBibleMenu()
    {
        InitializeComponent();
        HamburgerButton.Click += HamburgerButton_Click;
    }

    private void HamburgerButton_Click(object sender, RoutedEventArgs e)
    {
        HamburgerPopup.IsOpen = !HamburgerPopup.IsOpen;
    }
}
