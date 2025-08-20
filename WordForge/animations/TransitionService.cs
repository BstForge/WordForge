using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace WordForge;

public static class TransitionService
{
    public static bool TransitionsEnabled { get; set; } = true;

    private const double DurationScale = 2.5;

    private static TranslateTransform GetTransform(UIElement element)
    {
        if (element.RenderTransform is TranslateTransform t)
            return t;
        t = new TranslateTransform();
        element.RenderTransform = t;
        return t;
    }

    public static void SlideVertical(UIElement element)
    {
        if (!TransitionsEnabled)
        {
            ResetTransform(element);
            return;
        }

        var transform = GetTransform(element);
        transform.BeginAnimation(TranslateTransform.YProperty, null);
        transform.Y = 0;
        element.IsHitTestVisible = false;

        double offset = element.RenderSize.Height;
        var retract = new DoubleAnimation(-offset, TimeSpan.FromMilliseconds(180 * DurationScale))
        {
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
        };
        retract.Completed += (_, __) =>
        {
            var expand = new DoubleAnimation(0, TimeSpan.FromMilliseconds(220 * DurationScale))
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            expand.Completed += (_, __2) =>
            {
                element.IsHitTestVisible = true;
                transform.Y = 0;
            };
            transform.BeginAnimation(TranslateTransform.YProperty, expand, HandoffBehavior.SnapshotAndReplace);
        };
        transform.BeginAnimation(TranslateTransform.YProperty, retract, HandoffBehavior.SnapshotAndReplace);
    }

    public static void SlideHorizontal(UIElement element, double offset, Action? midwayAction = null)
    {
        if (!TransitionsEnabled)
        {
            midwayAction?.Invoke();
            ResetTransform(element);
            return;
        }

        var transform = GetTransform(element);
        transform.BeginAnimation(TranslateTransform.XProperty, null);
        transform.X = 0;
        element.IsHitTestVisible = false;

        var retract = new DoubleAnimation(offset, TimeSpan.FromMilliseconds(180 * DurationScale))
        {
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
        };
        retract.Completed += (_, __) =>
        {
            midwayAction?.Invoke();
            var expand = new DoubleAnimation(0, TimeSpan.FromMilliseconds(220 * DurationScale))
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            expand.Completed += (_, __2) =>
            {
                element.IsHitTestVisible = true;
                transform.X = 0;
            };
            transform.BeginAnimation(TranslateTransform.XProperty, expand, HandoffBehavior.SnapshotAndReplace);
        };
        transform.BeginAnimation(TranslateTransform.XProperty, retract, HandoffBehavior.SnapshotAndReplace);
    }

    public static void ResetTransform(UIElement element)
    {
        var transform = GetTransform(element);
        transform.BeginAnimation(TranslateTransform.XProperty, null);
        transform.BeginAnimation(TranslateTransform.YProperty, null);
        transform.X = 0;
        transform.Y = 0;
        element.IsHitTestVisible = true;
    }
}
