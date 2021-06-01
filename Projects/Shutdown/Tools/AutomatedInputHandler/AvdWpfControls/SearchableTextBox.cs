using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Input;

namespace AvdWpfControls {
    public class SearchableTextBox : TextBox {

        static SearchableTextBox() {
            Setter mySetter1 = new Setter {Property = BorderBrushProperty, Value = new SolidColorBrush(Colors.PowderBlue)};
            Setter mySetter2 = new Setter {Property = BorderThicknessProperty, Value = new Thickness(3d)};
            Trigger myTrigger = new Trigger {
                Property = IsHighlightedProperty,
                Value = true,
                Setters = {mySetter1, mySetter2}
            };
            _myStyle = new Style (typeof(SearchableTextBox)) {Triggers = {myTrigger}};
        }

        public SearchableTextBox()
        {
            GotKeyboardFocus += TextBoxGotKeyboardFocus;
            LostKeyboardFocus += TextBoxLostKeyboardFocus;
        }

        private static Style _myStyle;

        public static Style TextBoxStyle { get { return _myStyle; } }

        public static readonly DependencyProperty IsHighlightedProperty =
            DependencyProperty.Register("IsHighlighted", typeof (bool), typeof (SearchableTextBox), new PropertyMetadata(default(bool)));

        public bool IsHighlighted { get { return (bool) GetValue(IsHighlightedProperty); } set { SetValue(IsHighlightedProperty, value); } }

        private void TextBoxGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            IsHighlighted = true;
        }

        private void TextBoxLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            IsHighlighted = false;
        }
    }
}
