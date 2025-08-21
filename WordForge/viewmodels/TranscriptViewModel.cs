using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WordForge.ViewModels;

public enum CountScope
{
    Scene,
    Chapter,
    Project
}

public class TranscriptViewModel : INotifyPropertyChanged
{
    private static CountScope _lastScope = CountScope.Chapter;
    private static bool _scopeSet = false;
    private CountScope _countScope = _lastScope;

    public ObservableCollection<Chapter> Chapters => ProjectService.CurrentProject.Chapters;

    public CountScope CountScope
    {
        get => _countScope;
        set
        {
            if (_countScope != value)
            {
                _countScope = value;
                _lastScope = value;
                _scopeSet = true;
                OnPropertyChanged();
            }
        }
    }

    public static bool ScopePersisted => _scopeSet;

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
