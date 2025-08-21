using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WordForge.Menus.TopMenu;
using WordForge.Views;

namespace WordForge
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Grid TopMenuHostElement => TopMenuHost;
        public Grid CenterHostElement => CenterHost;
        public ContentControl CenterContentElement => CenterContent;
        public Border RightPaneHostElement => RightPaneHost;

        public MainWindow()
        {
            InitializeComponent();
            RightPaneService.Host = RightPaneContent;
            RightPaneService.Container = RightPaneHost;
            TopMenuContent.Content = new TranscriptMenu();
            CenterContentElement.Content = new TranscriptView();
            RightPaneService.Show(RightPaneKind.Timeline);
            StatusService.StatusChanged += message => Dispatcher.Invoke(() => StatusBarText.Text = message);
        }
    }
}