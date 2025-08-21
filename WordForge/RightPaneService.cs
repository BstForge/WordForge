using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using WordForge.Panes;

namespace WordForge;

public enum RightPaneKind
{
    Timeline,
    Outline,
    Character,
    Location,
    Item,
    Lore
}

public static class RightPaneService
{
    public static ContentControl? Host { get; set; }
    public static FrameworkElement? Container { get; set; }

    public static RightPaneKind CurrentPane { get; private set; } = RightPaneKind.Timeline;

    public static event Action<RightPaneKind>? PaneChanged;

    public static void Show(RightPaneKind kind)
    {
        void Update()
        {
            if (Host != null)
            {
                Host.Content = kind switch
                {
                    RightPaneKind.Timeline => new TimelinePane(),
                    RightPaneKind.Outline => new OutlinePane(),
                    RightPaneKind.Character => new CharacterBiblePane(),
                    RightPaneKind.Location => new LocationBiblePane(),
                    RightPaneKind.Item => new ItemBiblePane(),
                    RightPaneKind.Lore => new LoreBiblePane(),
                    _ => new TimelinePane()
                };
            }
            CurrentPane = kind;
            PaneChanged?.Invoke(kind);
        }

        if (Container != null && Container.RenderSize.Width > 0)
        {
            TransitionService.SlideHorizontal(Container, Container.RenderSize.Width, Update);
        }
        else
        {
            Update();
        }
    }

    public static void BindButton(Button button, RightPaneKind kind)
    {
        button.Click += (_, __) => Show(kind);
        PaneChanged += pane => SetActive(button, pane == kind);
        SetActive(button, CurrentPane == kind);
    }

    private static void SetActive(Button button, bool active)
    {
        var activeBrush = (Brush)new BrushConverter().ConvertFrom("#FF30D158")!;
        var inactiveBrush = (Brush)new BrushConverter().ConvertFrom("#FFE5E7EB")!;
        var activeForeground = Brushes.White;
        var inactiveForeground = (Brush)new BrushConverter().ConvertFrom("#FF111111")!;
        button.Background = active ? activeBrush : inactiveBrush;
        button.Foreground = active ? activeForeground : inactiveForeground;
    }
}
