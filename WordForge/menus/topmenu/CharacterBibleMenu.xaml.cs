using System.Windows;
using System.Windows.Controls;
using WordForge;

namespace WordForge.Menus.TopMenu;

public partial class CharacterBibleMenu : UserControl
{
    public CharacterBibleMenu()
    {
        InitializeComponent();
        HamburgerButton.Click += HamburgerButton_Click;
        RightPaneService.BindButton(TimelineButton, RightPaneKind.Timeline);
        RightPaneService.BindButton(OutlineButton, RightPaneKind.Outline);
        RightPaneService.BindButton(LocationButton, RightPaneKind.Location);
        RightPaneService.BindButton(ItemButton, RightPaneKind.Item);
    }

    private void HamburgerButton_Click(object sender, RoutedEventArgs e)
    {
        HamburgerPopup.IsOpen = !HamburgerPopup.IsOpen;
    }
}
