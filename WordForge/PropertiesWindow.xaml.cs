using System.Windows;

namespace WordForge;

public partial class PropertiesWindow : Window
{
    public PropertiesWindow()
    {
        InitializeComponent();
        TransitionsToggle.IsChecked = TransitionService.TransitionsEnabled;
        ToggleStateText.Text = TransitionService.TransitionsEnabled ? "On" : "Off";
        TransitionsToggle.Checked += ToggleChanged;
        TransitionsToggle.Unchecked += ToggleChanged;
    }

    private void ToggleChanged(object? sender, RoutedEventArgs e)
    {
        bool enabled = TransitionsToggle.IsChecked == true;
        TransitionService.TransitionsEnabled = enabled;
        ToggleStateText.Text = enabled ? "On" : "Off";
    }
}
