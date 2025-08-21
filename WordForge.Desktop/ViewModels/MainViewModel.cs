using System.Collections.ObjectModel;
using System.Windows.Input;
using WordForge.Core.Models;

namespace WordForge.Desktop.ViewModels;

public class MainViewModel : ViewModelBase
{
    public Project Project { get; } = new();
    private object? _currentNode;
    public object? CurrentNode
    {
        get => _currentNode;
        set => SetProperty(ref _currentNode, value);
    }

    public RightPaneViewModel RightPane { get; } = new();

    public ICommand ShowCharactersCommand { get; }
    public ICommand ShowLocationsCommand { get; }
    public ICommand ShowItemsCommand { get; }
    public ICommand ShowTimelineCommand { get; }

    private int _wordCount;
    public int WordCount
    {
        get => _wordCount;
        set => SetProperty(ref _wordCount, value);
    }

    private int _charCount;
    public int CharCount
    {
        get => _charCount;
        set => SetProperty(ref _charCount, value);
    }

    public ObservableCollection<string> Scopes { get; } = new(new[] {"Scene", "Chapter", "Project"});

    private string _selectedScope = "Scene";
    public string SelectedScope
    {
        get => _selectedScope;
        set => SetProperty(ref _selectedScope, value);
    }

    public MainViewModel()
    {
        // sample data
        var ch1 = new Chapter { Title = "Chapter 1" };
        ch1.Scenes.Add(new Scene { Title = "Scene 1" });
        ch1.Scenes.Add(new Scene { Title = "Scene 2" });
        var ch2 = new Chapter { Title = "Chapter 2" };
        ch2.Scenes.Add(new Scene { Title = "Scene 1" });
        Project.Chapters.Add(ch1);
        Project.Chapters.Add(ch2);

        RightPane.CurrentPane = new CharacterPaneViewModel();

        ShowCharactersCommand = new RelayCommand(_ => RightPane.CurrentPane = new CharacterPaneViewModel());
        ShowLocationsCommand = new RelayCommand(_ => RightPane.CurrentPane = new LocationPaneViewModel());
        ShowItemsCommand = new RelayCommand(_ => RightPane.CurrentPane = new ItemPaneViewModel());
        ShowTimelineCommand = new RelayCommand(_ => RightPane.CurrentPane = new TimelinePaneViewModel());
    }
}
