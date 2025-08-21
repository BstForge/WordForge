using System.Windows;
using System.Windows.Controls;
using WordForge.Menus.TopMenu;
using WordForge.Views;
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
        var main = (MainWindow)Application.Current.MainWindow;
        main.TopMenuContent.Content = new TranscriptMenu();
        main.CenterContentElement.Content = new TranscriptView();
        AnimateMenus();
    }

    private void TimelineButton_Click(object sender, RoutedEventArgs e)
    {
        var main = (MainWindow)Application.Current.MainWindow;
        main.TopMenuContent.Content = new TimelineMenu();
        main.CenterContentElement.Content = new TimelineView();
        AnimateMenus();
    }

    private void OutlineButton_Click(object sender, RoutedEventArgs e)
    {
        var main = (MainWindow)Application.Current.MainWindow;
        main.TopMenuContent.Content = new OutlineMenu();
        main.CenterContentElement.Content = new OutlineView();
        AnimateMenus();
    }

    private void CharacterBibleButton_Click(object sender, RoutedEventArgs e)
    {
        var main = (MainWindow)Application.Current.MainWindow;
        main.TopMenuContent.Content = new CharacterBibleMenu();
        main.CenterContentElement.Content = new CharacterBibleView();
        AnimateMenus();
    }

    private void LocationBibleButton_Click(object sender, RoutedEventArgs e)
    {
        var main = (MainWindow)Application.Current.MainWindow;
        main.TopMenuContent.Content = new LocationBibleMenu();
        main.CenterContentElement.Content = new LocationBibleView();
        AnimateMenus();
    }

    private void ItemBibleButton_Click(object sender, RoutedEventArgs e)
    {
        var main = (MainWindow)Application.Current.MainWindow;
        main.TopMenuContent.Content = new ItemBibleMenu();
        main.CenterContentElement.Content = new ItemBibleView();
        AnimateMenus();
    }

    private void LoreBibleButton_Click(object sender, RoutedEventArgs e)
    {
        var main = (MainWindow)Application.Current.MainWindow;
        main.TopMenuContent.Content = new LoreBibleMenu();
        main.CenterContentElement.Content = new LoreBibleView();
        AnimateMenus();
    }
}
