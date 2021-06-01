using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Input;

namespace AvdWpfControls.Utils
{
    public class EnterKeyMapper
    {
        public static readonly DependencyProperty ButtonProperty =
            DependencyProperty.RegisterAttached("Button", typeof (Button), typeof (EnterKeyMapper),
                                                new PropertyMetadata(null, OnButtonPropertyChanged));

        private static void OnButtonPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            Control ctl = sender as TextBox;
            if (ctl == null)
                ctl = sender as ComboBox;
            if (ctl == null) return;
            if (e.NewValue != null)
            {
                ctl.KeyDown += OnKeyDown;
            }
            else if (e.OldValue != null)
            {
                ctl.KeyDown -= OnKeyDown;
            }
        }

        private static void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Control ctl = sender as TextBox;
                if (ctl != null)
                {
                    var exp = ctl.GetBindingExpression(TextBox.TextProperty);
                    if (exp != null)
                        exp.UpdateSource();
                }
                else if (sender is ComboBox)
                {
                    ctl = sender as ComboBox;
                    var exp = ctl.GetBindingExpression(ComboBox.TextProperty);
                    if (exp != null)
                        exp.UpdateSource();
                }
                else
                    return;

                var button = GetButton((UIElement) sender);
                if (button != null)
                    SimulateButtonClick(button);
            }
        }

        private static void SimulateButtonClick(Button button)
        {
            if (button == null || !button.IsEnabled) return;
            ButtonAutomationPeer peer = new ButtonAutomationPeer(button);
            IInvokeProvider invokeProv = peer.GetPattern(PatternInterface.Invoke) as IInvokeProvider;
            invokeProv.Invoke();
        }

        public static void SetButton(UIElement element, Button button)
        {
            element.SetValue(ButtonProperty, button);
        }

        [AttachedPropertyBrowsableForType(typeof (Control))]
        public static Button GetButton(UIElement element)
        {
            return (Button) element.GetValue(ButtonProperty);
        }
    }

    public class EnterToTabKeyMapper
    {
        public static readonly DependencyProperty FocusedControlProperty =
            DependencyProperty.RegisterAttached("FocusedControl", typeof (Control), typeof (EnterToTabKeyMapper),
                                                new PropertyMetadata(null, OnFocusedControlPropertyChanged));

        public static readonly DependencyProperty FocusNextProperty =
            DependencyProperty.RegisterAttached("FocusNext", typeof (bool), typeof (EnterToTabKeyMapper),
                                                new PropertyMetadata(false, OnFocusNextPropertyChanged));

        private static void OnFocusedControlPropertyChanged(DependencyObject sender,
                                                            DependencyPropertyChangedEventArgs e)
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
            if (e.Key == Key.Enter)
            {
                if (GetFocusNext((UIElement) sender))
                {
                    TraversalRequest tRequest = new TraversalRequest(FocusNavigationDirection.Next);
                    UIElement keyboardFocus = Keyboard.FocusedElement as UIElement;
                    if (keyboardFocus != null)
                    {
                        keyboardFocus.MoveFocus(tRequest);
                    }
                }
                else if (GetFocusedControl((UIElement) sender) != null)
                {
                    var ctl = GetFocusedControl((UIElement) sender);
                    if (ctl != null && ctl is IInputElement)
                    {
                        FocusManager.SetFocusedElement(ctl, ctl);
                    }
                }
                e.Handled = true;
            }
        }

        public static void SetFocusedControl(UIElement element, Control control)
        {
            element.SetValue(FocusedControlProperty, control);
        }

        [AttachedPropertyBrowsableForType(typeof (Control))]
        public static Control GetFocusedControl(UIElement element)
        {
            return (Control) element.GetValue(FocusedControlProperty);
        }

        private static void OnFocusNextPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
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

        public static void SetFocusNext(UIElement element, bool focusNext)
        {
            element.SetValue(FocusNextProperty, focusNext);
        }

        [AttachedPropertyBrowsableForType(typeof (Control))]
        public static bool GetFocusNext(UIElement element)
        {
            return (bool) element.GetValue(FocusNextProperty);
        }
    }
}