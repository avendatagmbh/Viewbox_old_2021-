using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AvdWpfControls
{
    /// <summary>
    ///   Base class for controls which should handle click events.
    /// </summary>
    public class ClickableControl : UserControl
    {
        private Point _startPoint;

        #region MouseClick

        public static readonly RoutedEvent MouseClickEvent = EventManager.RegisterRoutedEvent(
            "MouseClick", RoutingStrategy.Bubble, typeof (RoutedEventHandler), typeof (ClickableControl));

        public event RoutedEventHandler MouseClick
        {
            add { AddHandler(MouseClickEvent, value); }
            remove { RemoveHandler(MouseClickEvent, value); }
        }

        internal void OnMouseClick()
        {
            RaiseEvent(new RoutedEventArgs(MouseClickEvent));
        }

        #endregion // MouseClick

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            _startPoint = e.GetPosition(this);
            base.OnMouseLeftButtonDown(e);
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            Point position = e.GetPosition(this);
            if (Math.Abs(position.X - _startPoint.X) <= SystemParameters.MinimumHorizontalDragDistance &&
                Math.Abs(position.Y - _startPoint.Y) <= SystemParameters.MinimumVerticalDragDistance)
            {
                OnMouseClick();
                e.Handled = true;
            }
            base.OnMouseLeftButtonUp(e);
        }
    }
}