using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace AvdWpfControls
{
    public class ScrollSynchronizer : DependencyObject
    {
        /// <summary>
        ///   Identifies the attached property ScrollGroup
        /// </summary>
        public static readonly DependencyProperty ScrollGroupProperty =
            DependencyProperty.RegisterAttached("ScrollGroup", typeof (string), typeof (ScrollSynchronizer),
                                                new PropertyMetadata(OnScrollGroupChanged));

        /// <summary>
        ///   List of all registered scroll viewers.
        /// </summary>
        private static readonly Dictionary<ScrollViewer, string> ScrollViewers = new Dictionary<ScrollViewer, string>();

        /// <summary>
        ///   Contains the latest horizontal scroll offset for each scroll group.
        /// </summary>
        private static readonly Dictionary<string, double> HorizontalScrollOffsets = new Dictionary<string, double>();

        /// <summary>
        ///   Contains the latest vertical scroll offset for each scroll group.
        /// </summary>
        private static readonly Dictionary<string, double> VerticalScrollOffsets = new Dictionary<string, double>();

        /// <summary>
        ///   Sets the value of the attached property ScrollGroup.
        /// </summary>
        /// <param name="obj"> Object on which the property should be applied. </param>
        /// <param name="scrollGroup"> Value of the property. </param>
        public static void SetScrollGroup(DependencyObject obj, string scrollGroup)
        {
            obj.SetValue(ScrollGroupProperty, scrollGroup);
        }

        /// <summary>
        ///   Gets the value of the attached property ScrollGroup.
        /// </summary>
        /// <param name="obj"> Object for which the property should be read. </param>
        /// <returns> Value of the property StartTime </returns>
        public static string GetScrollGroup(DependencyObject obj)
        {
            return (string) obj.GetValue(ScrollGroupProperty);
        }

        /// <summary>
        ///   Occurs, when the ScrollGroupProperty has changed.
        /// </summary>
        /// <param name="d"> The DependencyObject on which the property has changed value. </param>
        /// <param name="e"> Event data that is issued by any event that tracks changes to the effective value of this property. </param>
        private static void OnScrollGroupChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var scrollViewer = d as ScrollViewer;
            if (scrollViewer != null)
            {
                if (!string.IsNullOrEmpty((string) e.OldValue))
                {
                    // Remove scrollviewer
                    if (ScrollViewers.ContainsKey(scrollViewer))
                    {
                        scrollViewer.ScrollChanged -= ScrollViewer_ScrollChanged;
                        ScrollViewers.Remove(scrollViewer);
                    }
                }
                if (!string.IsNullOrEmpty((string) e.NewValue))
                {
                    // If group already exists, set scrollposition of new scrollviewer to the scrollposition of the group
                    if (HorizontalScrollOffsets.ContainsKey((string) e.NewValue))
                    {
                        scrollViewer.ScrollToHorizontalOffset(HorizontalScrollOffsets[(string) e.NewValue]);
                    }
                    else
                    {
                        HorizontalScrollOffsets.Add((string) e.NewValue, scrollViewer.HorizontalOffset);
                    }
                    if (VerticalScrollOffsets.ContainsKey((string)e.NewValue))
                    {
                        scrollViewer.ScrollToVerticalOffset(VerticalScrollOffsets[(string) e.NewValue]);
                    }
                    else
                    {
                        VerticalScrollOffsets.Add((string) e.NewValue, scrollViewer.VerticalOffset);
                    }
                    // Add scrollviewer
                    ScrollViewers.Add(scrollViewer, (string) e.NewValue);
                    scrollViewer.ScrollChanged += ScrollViewer_ScrollChanged;
                }
            }
        }

        /// <summary>
        ///   Occurs, when the scroll offset of one scrollviewer has changed.
        /// </summary>
        /// <param name="sender"> The sender of the event. </param>
        /// <param name="e"> EventArgs of the event. </param>
        private static void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (e.VerticalChange != 0)
            {
                var changedScrollViewer = sender as ScrollViewer;
                ScrollVertical(changedScrollViewer);
            }
            if (e.HorizontalChange != 0)
            {
                var changedScrollViewer = sender as ScrollViewer;
                ScrollHorizontal(changedScrollViewer);
            }
        }

        private static void ScrollVertical(ScrollViewer changedScrollViewer)
        {
            var group = ScrollViewers[changedScrollViewer];
            if (changedScrollViewer.VerticalScrollBarVisibility != ScrollBarVisibility.Disabled)
            {
                VerticalScrollOffsets[group] = changedScrollViewer.VerticalOffset;
                foreach (
                    var scrollViewer in ScrollViewers.
                        Where(s => s.Value == group && s.Key != changedScrollViewer &&
                                   s.Key.VerticalScrollBarVisibility != ScrollBarVisibility.Disabled))
                {
                    if (scrollViewer.Key.VerticalOffset != changedScrollViewer.VerticalOffset)
                    {
                        scrollViewer.Key.ScrollToVerticalOffset(changedScrollViewer.VerticalOffset);
                    }
                }
            }
        }

        private static void ScrollHorizontal(ScrollViewer changedScrollViewer)
        {
            var group = ScrollViewers[changedScrollViewer];
            if (changedScrollViewer.HorizontalScrollBarVisibility != ScrollBarVisibility.Disabled)
            {
                HorizontalScrollOffsets[group] = changedScrollViewer.HorizontalOffset;
                foreach (
                    var scrollViewer in ScrollViewers.
                        Where(s =>
                              s.Value == group && s.Key != changedScrollViewer &&
                              s.Key.HorizontalScrollBarVisibility != ScrollBarVisibility.Disabled))
                {
                    if (scrollViewer.Key.HorizontalOffset != changedScrollViewer.HorizontalOffset)
                    {
                        scrollViewer.Key.ScrollToHorizontalOffset(changedScrollViewer.HorizontalOffset);
                    }
                }
            }
        }
    }
}