using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace AvdWpfControls
{
    public class NumericTextbox : TextBox
    {
        private static readonly Style _myStyle;

        public static readonly DependencyProperty IsHighlightedProperty =
            DependencyProperty.Register("IsHighlighted", typeof (bool), typeof (NumericTextbox),
                                        new PropertyMetadata(default(bool)));

        static NumericTextbox()
        {
            //DefaultStyleKeyProperty.OverrideMetadata(typeof(NumericTextbox), new FrameworkPropertyMetadata(typeof(NumericTextbox)));            
            Setter mySetter1 = new Setter
                                   {Property = BorderBrushProperty, Value = new SolidColorBrush(Colors.PowderBlue)};
            Setter mySetter2 = new Setter {Property = BorderThicknessProperty, Value = new Thickness(3d)};
            Trigger myTrigger = new Trigger
                                    {
                                        Property = IsHighlightedProperty,
                                        Value = true,
                                        Setters = {mySetter1, mySetter2}
                                    };
            _myStyle = new Style(typeof (NumericTextbox)) {Triggers = {myTrigger}};
        }

        public static Style NumericTextBoxStyle
        {
            get { return _myStyle; }
        }

        public bool IsHighlighted
        {
            get { return (bool) GetValue(IsHighlightedProperty); }
            set { SetValue(IsHighlightedProperty, value); }
        }

        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            // does not work since assignment would destroy databinding!
            // replace all matches for (no number, ".", "," or "-" with an empty string           
            //Text = System.Text.RegularExpressions.Regex.Replace(Text, @"[^-.,\d]", "");
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            // disable CTRL-V handler
            if (e.Key == Key.V && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                e.Handled = true;
            }
            else
            {
                base.OnPreviewKeyDown(e);
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            //if (e.Key == Key.OemPeriod) {
            //    if (!this.Text.Contains(",")) {
            //        int caretIndex = this.CaretIndex;
            //        string tmp1 = this.Text.Substring(0, this.CaretIndex);
            //        string tmp2 = (this.CaretIndex < this.Text.Length ? this.Text.Substring(this.CaretIndex , this.Text.Length - this.CaretIndex) : "");
            //        this.Text = tmp1 + "," + tmp2;
            //        this.CaretIndex = caretIndex + 1;
            //    }
            //    e.Handled = true;
            //    return;
            //}
            // prevent multiple decimal points
            if ((e.Key == Key.OemComma || e.Key == Key.Decimal) && (Text.Contains(",")))
            {
                e.Handled = true;
                return;
            }
            bool isNumPadNumeric = (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9) || e.Key == Key.OemComma ||
                                   e.Key == Key.Decimal;
            bool isNumeric = (e.Key >= Key.D0 && e.Key <= Key.D9);
            if ((isNumeric || isNumPadNumeric) && Keyboard.Modifiers != ModifierKeys.None)
            {
                e.Handled = true;
                return;
            }
            bool isControl = ((Keyboard.Modifiers != ModifierKeys.None && Keyboard.Modifiers != ModifierKeys.Shift)
                              || e.Key == Key.Back || e.Key == Key.Delete || e.Key == Key.Insert
                              || e.Key == Key.Down || e.Key == Key.Left || e.Key == Key.Right || e.Key == Key.Up
                              || e.Key == Key.Tab
                              || e.Key == Key.PageDown || e.Key == Key.PageUp
                              || e.Key == Key.Enter || e.Key == Key.Return || e.Key == Key.Escape
                              || e.Key == Key.Home || e.Key == Key.End);
            bool isSign =
                (e.Key == Key.OemMinus || e.Key == Key.Subtract) &&
                (CaretIndex == 0 && !Text.Contains("-")) ||
                (SelectionStart == 0 && SelectedText.Contains("-"));
            if ((isSign || isControl || isNumeric || isNumPadNumeric) && !Keyboard.IsKeyToggled(Key.CapsLock))
            {
                base.OnKeyDown(e);
            }
            else
            {
                e.Handled = true;
            }
        }
    }
}