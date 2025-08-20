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

namespace WordForge
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Grid TopMenuHostElement => TopMenuHost;
        public Grid CenterHostElement => CenterHost;
        public Border RightPaneHostElement => RightPaneHost;

        public MainWindow()
        {
            InitializeComponent();
            RightPaneService.Host = RightPaneContent;
            RightPaneService.Container = RightPaneHost;
            TopMenuContent.Content = new TranscriptMenu();
            RightPaneService.Show(RightPaneKind.Timeline);
        }
    }
}