using System;
using System.IO;
using System.Windows.Threading;

namespace WordForge;

/// <summary>
/// Centralized autosave coordinator. Uses DispatcherTimer for UI-thread ticks.
/// Modes (via Project.Autosave.Mode):
///  - Interval: saves an autosave snapshot every N minutes
///  - OnPaneChange: throttled autosave when center pane changes
/// 
/// Notes:
///  - Autosave snapshots are simple copies of the last saved project file.
///    (If the project has never been saved, autosave is skipped.)
///  - Manual Save clears any existing autosave snapshot.
/// </summary>
public static class AutosaveService
{
    private static DispatcherTimer? _timer;
    private static DateTime _lastPaneSaveUtc = DateTime.MinValue;
    private static Project? _project;

    public static void Configure(Project project)
    {
        _project = project;

        // Stop any previous timer
        if (_timer != null)
        {
            _timer.Tick -= OnTimerTick;
            _timer.Stop();
            _timer = null;
        }

        if (project?.Autosave == null || !project.Autosave.Enabled)
            return;

        // Choose behavior based on enum value (no string/?. comparisons)
        switch (project.Autosave.Mode)
        {
            case AutosaveMode.Interval:
                {
                    var minutes = Math.Max(1, project.Autosave.Minutes);
                    _timer = new DispatcherTimer
                    {
                        Interval = TimeSpan.FromMinutes(minutes)
                    };
                    _timer.Tick += OnTimerTick;
                    _timer.Start();
                    break;
                }

            case AutosaveMode.OnPaneChange:
            default:
                {
                    // No timer; pane changes will call NotifyPaneChanged()
                    _lastPaneSaveUtc = DateTime.MinValue;
                    break;
                }
        }
    }

    /// <summary>
    /// Call this when the center pane changes (only relevant in OnPaneChange mode).
    /// Saves at most once per configured interval.
    /// </summary>
    public static void NotifyPaneChanged()
    {
        if (_project?.Autosave == null || !_project.Autosave.Enabled)
            return;

        if (_project.Autosave.Mode != AutosaveMode.OnPaneChange)
            return;

        var throttle = TimeSpan.FromMinutes(Math.Max(1, _project.Autosave.Minutes));
        var now = DateTime.UtcNow;
        if (now - _lastPaneSaveUtc >= throttle)
        {
            _lastPaneSaveUtc = now;
            TryWriteAutosaveSnapshot();
        }
    }

    private static void OnTimerTick(object? sender, EventArgs e)
    {
        TryWriteAutosaveSnapshot();
    }

    /// <summary>
    /// Creates/overwrites the autosave snapshot if the project has a saved path.
    /// This copies the current .forge file to the autosave path. If the project
    /// has never been saved, this is a no-op.
    /// </summary>
    private static void TryWriteAutosaveSnapshot()
    {
        if (ProjectService.CurrentPath == null)
            return;

        try
        {
            var source = ProjectService.CurrentPath;
            var autosavePath = ProjectService.GetAutosavePath(source);

            // Ensure parent directory exists
            var dir = Path.GetDirectoryName(autosavePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            if (File.Exists(source))
            {
                File.Copy(source, autosavePath, overwrite: true);
            }
        }
        catch
        {
            // Intentionally swallow; autosave failures should not interrupt the user.
        }
    }

    /// <summary>
    /// Call after a successful manual Save to clear any autosave snapshot.
    /// </summary>
    public static void OnManualSave()
    {
        if (ProjectService.CurrentPath == null)
            return;

        try
        {
            var path = ProjectService.GetAutosavePath(ProjectService.CurrentPath);
            if (File.Exists(path))
                File.Delete(path);
        }
        catch
        {
            // Swallow; failure to delete autosave isn't critical.
        }
    }

    /// <summary>
    /// Stops any active timers and clears references. Call on project close.
    /// </summary>
    public static void Shutdown()
    {
        if (_timer != null)
        {
            _timer.Tick -= OnTimerTick;
            _timer.Stop();
            _timer = null;
        }
        _project = null;
        _lastPaneSaveUtc = DateTime.MinValue;
    }
}
