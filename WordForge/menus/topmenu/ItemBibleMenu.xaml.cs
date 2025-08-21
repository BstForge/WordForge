using System.Windows;
using System.Windows.Controls;
using WordForge;

namespace WordForge.Menus.TopMenu;

public partial class ItemBibleMenu : UserControl
{
    public ItemBibleMenu()
    {
        InitializeComponent();
        HamburgerButton.Click += HamburgerButton_Click;
        RightPaneService.BindButton(TimelineButton, RightPaneKind.Timeline);
        RightPaneService.BindButton(OutlineButton, RightPaneKind.Outline);
        RightPaneService.BindButton(CharacterButton, RightPaneKind.Character);
        RightPaneService.BindButton(LocationButton, RightPaneKind.Location);
        RightPaneService.BindButton(LoreButton, RightPaneKind.Lore);
        CustomizeButton.Click += (_, __) => new CustomizeWindow { Owner = Application.Current.MainWindow }.ShowDialog();
    }

    private void HamburgerButton_Click(object sender, RoutedEventArgs e)
    {
        HamburgerPopup.IsOpen = !HamburgerPopup.IsOpen;
    }
}
