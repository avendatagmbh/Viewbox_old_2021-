using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AvdWpfControls.Utils
{
    public class NumericBehavior
    {
        public static readonly DependencyProperty AcceptOnlyNumericsProperty =
            DependencyProperty.RegisterAttached("AcceptOnlyNumerics", typeof(bool), typeof(NumericBehavior),
            new PropertyMetadata(false, OnAcceptOnlyNumericsPropertyChanged));

        private static void OnAcceptOnlyNumericsPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            Control ctl = sender as TextBox;
            if (ctl == null)
                ctl = sender as ComboBox;
            if (ctl == null) return;

            if (e.NewValue != null)
            {
                ctl.PreviewKeyDown += OnPreviewKeyDown;
            }
            else if (e.OldValue != null)
            {
                ctl.PreviewKeyDown -= OnPreviewKeyDown;
            }
        }

        private static void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.D0 || e.Key == Key.D1 || e.Key == Key.D2 || e.Key == Key.D3 ||
                e.Key == Key.D4 || e.Key == Key.D5 || e.Key == Key.D6 || e.Key == Key.D7 ||
                e.Key == Key.D8 || e.Key == Key.D9 ||
                e.Key == Key.NumPad0 || e.Key == Key.NumPad1 || e.Key == Key.NumPad2 || e.Key == Key.NumPad3 ||
                e.Key == Key.NumPad4 || e.Key == Key.NumPad5 || e.Key == Key.NumPad6 || e.Key == Key.NumPad7 ||
                e.Key == Key.NumPad8 || e.Key == Key.NumPad9 || e.Key == Key.OemPeriod || e.Key == Key.OemComma || e.Key == Key.Separator ||
                e.Key == Key.Insert || e.Key == Key.Delete || e.Key == Key.Home || e.Key == Key.End ||
                e.Key == Key.Tab || e.Key == Key.Enter || e.Key == Key.Back || e.Key == Key.OemMinus ||
                e.Key == Key.Left || e.Key == Key.Right || e.Key == Key.Up || e.Key == Key.Down ||
                e.Key == Key.LeftShift || e.Key == Key.RightShift || e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl ||
                e.Key == Key.F1 || e.Key == Key.F2 || e.Key == Key.F3 || e.Key == Key.F4 || e.Key == Key.F5 || e.Key == Key.F6 || e.Key == Key.F7 || e.Key == Key.F8 || e.Key == Key.F9 || e.Key == Key.F10 || e.Key == Key.F11 || e.Key == Key.F12 ||
                e.Key == Key.Escape || e.Key == Key.Print || e.Key == Key.PrintScreen)
            {
                return;
            }
            else
            {
                e.Handled = true;
            }
            if (sender is TextBox)
            {
                var control = sender as TextBox;
                if (string.IsNullOrEmpty(control.Text))
                {
                    control.Text = null;
                }
            }
            else if (sender is ComboBox)
            {
                var control = sender as ComboBox;
                if (string.IsNullOrEmpty(control.Text))
                {
                    control.Text = null;
                }
            }
        }

        public static void SetAcceptOnlyNumerics(UIElement element, bool acceptOnlyNumerics)
        {
            element.SetValue(AcceptOnlyNumericsProperty, acceptOnlyNumerics);
        }

        [AttachedPropertyBrowsableForType(typeof(TextBox))]
        [AttachedPropertyBrowsableForType(typeof(ComboBox))]
        public static bool GetAcceptOnlyNumerics(UIElement element)
        {
            return (bool)element.GetValue(AcceptOnlyNumericsProperty);
        }
    }
}
