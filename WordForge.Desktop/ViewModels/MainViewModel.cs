using System;
using System.Collections.ObjectModel;
using System.Linq;
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

    public ObservableCollection<string> Scopes { get; } = new(new[] {"Scene", "Chapter", "Project"});

    private string _selectedScope = "Scene";
    public string SelectedScope
    {
        get => _selectedScope;
        set => SetProperty(ref _selectedScope, value);
    }

    private string _transcriptText = string.Empty;
    public string TranscriptText
    {
        get => _transcriptText;
        set
        {
            if (SetProperty(ref _transcriptText, value))
            {
                UpdateCounts();
            }
        }
    }

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

    private ActiveCenterPane _activeCenterPane = ActiveCenterPane.Transcript;
    public ActiveCenterPane ActiveCenterPane
    {
        get => _activeCenterPane;
        set
        {
            if (SetProperty(ref _activeCenterPane, value))
            {
                OnPropertyChanged(nameof(IsTranscriptActive));
                OnPropertyChanged(nameof(IsTimelineActive));
                OnPropertyChanged(nameof(IsOutlineActive));
                OnPropertyChanged(nameof(IsCharBibleActive));
                OnPropertyChanged(nameof(IsLocBibleActive));
                OnPropertyChanged(nameof(IsItemBibleActive));
            }
        }
    }

    public bool IsTranscriptActive => ActiveCenterPane == ActiveCenterPane.Transcript;
    public bool IsTimelineActive => ActiveCenterPane == ActiveCenterPane.Timeline;
    public bool IsOutlineActive => ActiveCenterPane == ActiveCenterPane.Outline;
    public bool IsCharBibleActive => ActiveCenterPane == ActiveCenterPane.CharBible;
    public bool IsLocBibleActive => ActiveCenterPane == ActiveCenterPane.LocBible;
    public bool IsItemBibleActive => ActiveCenterPane == ActiveCenterPane.ItemBible;

    private ActiveRightPane _activeRightPane = ActiveRightPane.Character;
    public ActiveRightPane ActiveRightPane
    {
        get => _activeRightPane;
        set => SetProperty(ref _activeRightPane, value);
    }

    private bool _isRightPaneVisible = true;
    public bool IsRightPaneVisible
    {
        get => _isRightPaneVisible;
        set => SetProperty(ref _isRightPaneVisible, value);
    }

    private string _statusMessage = string.Empty;
    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public ICommand ShowTranscriptCommand { get; }
    public ICommand ShowTimelineCenterCommand { get; }
    public ICommand ShowOutlineCenterCommand { get; }
    public ICommand ShowCharBibleCenterCommand { get; }
    public ICommand ShowLocBibleCenterCommand { get; }
    public ICommand ShowItemBibleCenterCommand { get; }

    public ICommand ToggleRightPaneCommand { get; }
    public ICommand ShowCharactersCommand { get; }
    public ICommand ShowLocationsCommand { get; }
    public ICommand ShowItemsCommand { get; }
    public ICommand ShowTimelineCommand { get; }

    public ICommand InsertSceneBreakCommand { get; }

    public ICommand ExportPdfCommand { get; }
    public ICommand ExportDocxCommand { get; }
    public ICommand ExportRtfCommand { get; }
    public ICommand ExportTxtCommand { get; }

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

        RightPane.CurrentPane = new TimelinePaneViewModel();

        ShowTranscriptCommand = new RelayCommand(_ => ActiveCenterPane = ActiveCenterPane.Transcript);
        ShowTimelineCenterCommand = new RelayCommand(_ =>
        {
            ActiveCenterPane = ActiveCenterPane.Timeline;
            RightPane.CurrentPane = new TimelinePaneViewModel();
        });
        ShowOutlineCenterCommand = new RelayCommand(_ => ActiveCenterPane = ActiveCenterPane.Outline);
        ShowCharBibleCenterCommand = new RelayCommand(_ => ActiveCenterPane = ActiveCenterPane.CharBible);
        ShowLocBibleCenterCommand = new RelayCommand(_ => ActiveCenterPane = ActiveCenterPane.LocBible);
        ShowItemBibleCenterCommand = new RelayCommand(_ => ActiveCenterPane = ActiveCenterPane.ItemBible);

        ToggleRightPaneCommand = new RelayCommand(_ => IsRightPaneVisible = !IsRightPaneVisible);
        ShowCharactersCommand = new RelayCommand(_ => { RightPane.CurrentPane = new CharacterPaneViewModel(); ActiveRightPane = ActiveRightPane.Character; IsRightPaneVisible = true; });
        ShowLocationsCommand = new RelayCommand(_ => { RightPane.CurrentPane = new LocationPaneViewModel(); ActiveRightPane = ActiveRightPane.Location; IsRightPaneVisible = true; });
        ShowItemsCommand = new RelayCommand(_ => { RightPane.CurrentPane = new ItemPaneViewModel(); ActiveRightPane = ActiveRightPane.Item; IsRightPaneVisible = true; });
        ShowTimelineCommand = new RelayCommand(_ => { RightPane.CurrentPane = new TimelinePaneViewModel(); ActiveRightPane = ActiveRightPane.Character; });

        InsertSceneBreakCommand = new RelayCommand(_ => TranscriptText += "\n***\n");

        ExportPdfCommand = new RelayCommand(_ => StatusMessage = "Export PDF clicked");
        ExportDocxCommand = new RelayCommand(_ => StatusMessage = "Export DOCX clicked");
        ExportRtfCommand = new RelayCommand(_ => StatusMessage = "Export RTF clicked");
        ExportTxtCommand = new RelayCommand(_ => StatusMessage = "Export TXT clicked");

        UpdateCounts();
    }

    private void UpdateCounts()
    {
        var text = TranscriptText ?? string.Empty;
        CharCount = text.Length;
        WordCount = text.Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries).Length;
    }
}
