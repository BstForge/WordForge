using System;
using System.IO;
using System.Timers;
using System.Windows;

namespace WordForge;

public static class AutosaveService
{
    private static Timer? _timer;
    private static DateTime _lastPaneSave = DateTime.MinValue;

    public static void Configure(Project project)
    {
        _timer?.Stop();
        _timer?.Dispose();
        _timer = null;
        RightPaneService.PaneChanged -= PaneChanged;

        if (!project.Autosave.Enabled)
            return;

        if (project.Autosave.Mode == AutosaveMode.Interval)
        {
            _timer = new Timer(Math.Max(1, project.Autosave.Minutes) * 60 * 1000);
            _timer.Elapsed += (_, __) => Application.Current.Dispatcher.Invoke(SaveAutosave);
            _timer.AutoReset = true;
            _timer.Start();
        }
        else
        {
            _lastPaneSave = DateTime.MinValue;
            RightPaneService.PaneChanged += PaneChanged;
        }
    }

    private static void PaneChanged(RightPaneKind kind)
    {
        if ((DateTime.UtcNow - _lastPaneSave).TotalSeconds < 5)
            return;
        _lastPaneSave = DateTime.UtcNow;
        SaveAutosave();
    }

    private static void SaveAutosave()
    {
        if (ProjectService.CurrentPath == null)
            return;
        var path = ProjectService.GetAutosavePath(ProjectService.CurrentPath);
        try
        {
            ProjectService.SaveCopy(ProjectService.CurrentProject, path);
        }
        catch
        {
            // ignore
        }
    }

    public static void OnManualSave()
    {
        if (ProjectService.CurrentPath == null)
            return;
        var path = ProjectService.GetAutosavePath(ProjectService.CurrentPath);
        if (File.Exists(path))
        {
            try { File.Delete(path); } catch { }
        }
    }
}

