using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace FluentHyperV.Desktop.Helpers;

/// <summary>
/// Attached behavior that enables smooth auto-scrolling for TabControl when a tab is selected
/// </summary>
public static class TabControlScrollBehavior
{
    private static readonly TimeSpan DefaultAnimationDuration = TimeSpan.FromMilliseconds(400);
    private static readonly IEasingFunction DefaultEasingFunction = new ExponentialEase
    {
        EasingMode = EasingMode.EaseOut,
    };

    public static readonly DependencyProperty AutoScrollToSelectedProperty =
        DependencyProperty.RegisterAttached(
            "AutoScrollToSelected",
            typeof(bool),
            typeof(TabControlScrollBehavior),
            new(false, OnAutoScrollToSelectedChanged)
        );

    public static readonly DependencyProperty ScrollAnimationDurationProperty =
        DependencyProperty.RegisterAttached(
            "ScrollAnimationDuration",
            typeof(TimeSpan),
            typeof(TabControlScrollBehavior),
            new(DefaultAnimationDuration)
        );

    public static readonly DependencyProperty ScrollEasingFunctionProperty =
        DependencyProperty.RegisterAttached(
            "ScrollEasingFunction",
            typeof(IEasingFunction),
            typeof(TabControlScrollBehavior),
            new(DefaultEasingFunction)
        );

    // Getters and Setters for AutoScrollToSelected
    public static bool GetAutoScrollToSelected(DependencyObject obj)
    {
        return (bool)obj.GetValue(AutoScrollToSelectedProperty);
    }

    public static void SetAutoScrollToSelected(DependencyObject obj, bool value)
    {
        obj.SetValue(AutoScrollToSelectedProperty, value);
    }

    // Getters and Setters for ScrollAnimationDuration
    public static TimeSpan GetScrollAnimationDuration(DependencyObject obj)
    {
        return (TimeSpan)obj.GetValue(ScrollAnimationDurationProperty);
    }

    public static void SetScrollAnimationDuration(DependencyObject obj, TimeSpan value)
    {
        obj.SetValue(ScrollAnimationDurationProperty, value);
    }

    // Getters and Setters for ScrollEasingFunction
    public static IEasingFunction GetScrollEasingFunction(DependencyObject obj)
    {
        return (IEasingFunction)obj.GetValue(ScrollEasingFunctionProperty);
    }

    public static void SetScrollEasingFunction(DependencyObject obj, IEasingFunction value)
    {
        obj.SetValue(ScrollEasingFunctionProperty, value);
    }

    private static void OnAutoScrollToSelectedChanged(
        DependencyObject d,
        DependencyPropertyChangedEventArgs e
    )
    {
        if (d is TabControl tabControl)
        {
            if ((bool)e.NewValue)
            {
                tabControl.SelectionChanged += TabControl_SelectionChanged;
                tabControl.Loaded += TabControl_Loaded;
            }
            else
            {
                tabControl.SelectionChanged -= TabControl_SelectionChanged;
                tabControl.Loaded -= TabControl_Loaded;
            }
        }
    }

    private static void TabControl_Loaded(object sender, RoutedEventArgs e)
    {
        if (sender is TabControl tabControl)
        {
            // Delay the initial scroll to ensure the layout is complete
            tabControl.Dispatcher.BeginInvoke(
                DispatcherPriority.ContextIdle,
                new Action(() =>
                {
                    ScrollToSelectedTab(tabControl);
                })
            );
        }
    }

    private static void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is TabControl tabControl)
        {
            // Use a small delay to ensure the visual state has updated
            tabControl.Dispatcher.BeginInvoke(
                DispatcherPriority.Background,
                new Action(() =>
                {
                    ScrollToSelectedTab(tabControl);
                })
            );
        }
    }

    private static void ScrollToSelectedTab(TabControl tabControl)
    {
        if (tabControl.SelectedItem == null)
            return;

        // Find the TabItem container for the selected item
        if (
            tabControl.ItemContainerGenerator.ContainerFromItem(tabControl.SelectedItem)
            is not TabItem selectedTabItem
        )
            return;

        // Find the ScrollViewer in the TabControl template
        var scrollViewer = FindVisualChild<ScrollViewer>(tabControl);
        if (scrollViewer == null)
            return;

        // Calculate the position to scroll to
        var tabPanel = FindVisualChild<TabPanel>(tabControl);
        if (tabPanel == null)
            return;

        try
        {
            // Get the position of the selected tab relative to the TabPanel
            var transform = selectedTabItem.TransformToAncestor(tabPanel);
            var position = transform.Transform(new(0, 0));

            // For vertical scrolling (since TabStripPlacement is Left)
            var targetOffset = position.Y;
            var tabItemHeight = selectedTabItem.ActualHeight;
            var scrollViewerHeight = scrollViewer.ViewportHeight;

            // Add some padding around the tab
            const double padding = 20;

            // Check if the tab is already visible with padding
            var currentOffset = scrollViewer.VerticalOffset;
            var isVisible =
                targetOffset >= (currentOffset + padding)
                && (targetOffset + tabItemHeight) <= (currentOffset + scrollViewerHeight - padding);

            if (!isVisible)
            {
                // Calculate the best scroll position
                double newOffset;

                if (targetOffset < currentOffset)
                {
                    // Tab is above the viewport, scroll to show it at the top with padding
                    newOffset = Math.Max(0, targetOffset - padding);
                }
                else
                {
                    // Tab is below the viewport, scroll to show it at the bottom with padding
                    newOffset = targetOffset + tabItemHeight - scrollViewerHeight + padding;
                }

                // Ensure we don't scroll beyond the content bounds
                newOffset = Math.Max(0, Math.Min(newOffset, scrollViewer.ScrollableHeight));

                // Only animate if there's a significant difference
                if (Math.Abs(currentOffset - newOffset) > 1)
                {
                    // Animate scroll with smooth transition
                    AnimateScroll(scrollViewer, currentOffset, newOffset, tabControl);
                }
            }
        }
        catch
        {
            // Fallback to immediate scroll if animation fails
            selectedTabItem.BringIntoView();
        }
    }

    /// <summary>
    /// Animates the scroll position with smooth easing
    /// </summary>
    private static void AnimateScroll(
        ScrollViewer scrollViewer,
        double fromOffset,
        double toOffset,
        TabControl tabControl
    )
    {
        // Stop any existing animation
        scrollViewer.BeginAnimation(ScrollViewerOffsetProperty, null);

        var duration = GetScrollAnimationDuration(tabControl);
        var easingFunction = GetScrollEasingFunction(tabControl);

        var animation = new DoubleAnimation
        {
            From = fromOffset,
            To = toOffset,
            Duration = new(duration),
            EasingFunction = easingFunction,
            FillBehavior = FillBehavior.HoldEnd,
        };

        // Set up completion handler to ensure final position is maintained
        animation.Completed += (sender, e) =>
        {
            // Remove the animation and set the final value directly
            scrollViewer.BeginAnimation(ScrollViewerOffsetProperty, null);
            scrollViewer.SetValue(ScrollViewerOffsetProperty, toOffset);
            scrollViewer.ScrollToVerticalOffset(toOffset);
        };

        // Apply the animation directly
        scrollViewer.SetValue(ScrollViewerOffsetProperty, fromOffset);
        scrollViewer.BeginAnimation(ScrollViewerOffsetProperty, animation);
    }

    /// <summary>
    /// Custom dependency property for animating scroll offset
    /// </summary>
    private static readonly DependencyProperty ScrollViewerOffsetProperty =
        DependencyProperty.RegisterAttached(
            "ScrollViewerOffset",
            typeof(double),
            typeof(TabControlScrollBehavior),
            new(0.0, OnScrollViewerOffsetChanged)
        );

    private static void OnScrollViewerOffsetChanged(
        DependencyObject d,
        DependencyPropertyChangedEventArgs e
    )
    {
        if (d is ScrollViewer scrollViewer && e.NewValue is double newOffset)
        {
            scrollViewer.ScrollToVerticalOffset(newOffset);
        }
    }

    /// <summary>
    /// Helper method to find a visual child of a specific type
    /// </summary>
    private static T? FindVisualChild<T>(DependencyObject parent)
        where T : DependencyObject
    {
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);

            if (child is T result)
                return result;

            var childOfChild = FindVisualChild<T>(child);
            if (childOfChild != null)
                return childOfChild;
        }
        return null;
    }
}
