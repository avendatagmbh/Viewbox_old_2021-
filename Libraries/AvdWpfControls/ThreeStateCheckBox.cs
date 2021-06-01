using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace AvdWpfControls
{
    public class ThreeStateCheckBox : CheckBox
    {
        private static readonly Style _myStyle;

        public static readonly DependencyProperty IsHighlightedProperty =
            DependencyProperty.Register("IsHighlighted", typeof (bool), typeof (ThreeStateCheckBox),
                                        new PropertyMetadata(default(bool)));

        static ThreeStateCheckBox()
        {
            Setter mySetter1 = new Setter
                                   {Property = BorderBrushProperty, Value = new SolidColorBrush(Colors.PowderBlue)};
            Setter mySetter2 = new Setter
                                   {Property = BackgroundProperty, Value = new SolidColorBrush(Colors.PowderBlue)};
            Trigger myTrigger = new Trigger
                                    {
                                        Property = IsHighlightedProperty,
                                        Value = true,
                                        Setters = {mySetter1, mySetter2}
                                    };
            // TODO: style is overwritten there. Even the basedOn. Add a based on property to the key "CheckBoxKey"
            _myStyle = new Style(typeof (ThreeStateCheckBox))
                           {Triggers = {myTrigger}};
        }

        public static Style ThreeStateCheckBoxStyle
        {
            get { return _myStyle; }
        }

        public bool IsHighlighted
        {
            get { return (bool) GetValue(IsHighlightedProperty); }
            set { SetValue(IsHighlightedProperty, value); }
        }

        public event RoutedEventHandler Toggled;

        private void OnToggled()
        {
            if (Toggled != null) Toggled(this, new RoutedEventArgs());
        }

        protected override void OnToggle()
        {
            if (IsChecked == false) IsChecked = true;
            else if (IsChecked == true) IsChecked = null;
            else IsChecked = false;
            OnToggled();
        }
    }
}