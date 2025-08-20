using System.Windows;
using System.Windows.Controls;
using WordForge.Menus.TopMenu;

namespace WordForge.Menus;

public partial class LeftMenu : UserControl
{
    public LeftMenu()
    {
        InitializeComponent();
    }

    private void TranscriptButton_Click(object sender, RoutedEventArgs e)
    {
        ((MainWindow)Application.Current.MainWindow).TopMenuContent.Content = new TranscriptMenu();
    }

    private void TimelineButton_Click(object sender, RoutedEventArgs e)
    {
        ((MainWindow)Application.Current.MainWindow).TopMenuContent.Content = new TimelineMenu();
    }

    private void OutlineButton_Click(object sender, RoutedEventArgs e)
    {
        ((MainWindow)Application.Current.MainWindow).TopMenuContent.Content = new OutlineMenu();
    }

    private void CharacterBibleButton_Click(object sender, RoutedEventArgs e)
    {
        ((MainWindow)Application.Current.MainWindow).TopMenuContent.Content = new CharacterBibleMenu();
    }

    private void LocationBibleButton_Click(object sender, RoutedEventArgs e)
    {
        ((MainWindow)Application.Current.MainWindow).TopMenuContent.Content = new LocationBibleMenu();
    }

    private void ItemBibleButton_Click(object sender, RoutedEventArgs e)
    {
        ((MainWindow)Application.Current.MainWindow).TopMenuContent.Content = new ItemBibleMenu();
    }
}
