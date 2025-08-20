using System.Windows;
using System.Windows.Controls;
using WordForge.Menus.TopMenu;
using WordForge;

namespace WordForge.Menus;

public partial class LeftMenu : UserControl
{
    public LeftMenu()
    {
        InitializeComponent();
    }

    private static void AnimateMenus()
    {
        var main = (MainWindow)Application.Current.MainWindow;
        TransitionService.SlideVertical(main.TopMenuHostElement);
        TransitionService.SlideVertical(main.CenterHostElement);
    }

    private void TranscriptButton_Click(object sender, RoutedEventArgs e)
    {
        ((MainWindow)Application.Current.MainWindow).TopMenuContent.Content = new TranscriptMenu();
        AnimateMenus();
    }

    private void TimelineButton_Click(object sender, RoutedEventArgs e)
    {
        ((MainWindow)Application.Current.MainWindow).TopMenuContent.Content = new TimelineMenu();
        AnimateMenus();
    }

    private void OutlineButton_Click(object sender, RoutedEventArgs e)
    {
        ((MainWindow)Application.Current.MainWindow).TopMenuContent.Content = new OutlineMenu();
        AnimateMenus();
    }

    private void CharacterBibleButton_Click(object sender, RoutedEventArgs e)
    {
        ((MainWindow)Application.Current.MainWindow).TopMenuContent.Content = new CharacterBibleMenu();
        AnimateMenus();
    }

    private void LocationBibleButton_Click(object sender, RoutedEventArgs e)
    {
        ((MainWindow)Application.Current.MainWindow).TopMenuContent.Content = new LocationBibleMenu();
        AnimateMenus();
    }

    private void ItemBibleButton_Click(object sender, RoutedEventArgs e)
    {
        ((MainWindow)Application.Current.MainWindow).TopMenuContent.Content = new ItemBibleMenu();
        AnimateMenus();
    }
}
