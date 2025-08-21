namespace WordForge.Desktop.ViewModels;

public class RightPaneViewModel : ViewModelBase
{
    private ViewModelBase? _currentPane;
    public ViewModelBase? CurrentPane
    {
        get => _currentPane;
        set => SetProperty(ref _currentPane, value);
    }
}
