using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace AvdWpfControls
{
    public class SearchableDatePicker : DatePicker
    {
        private static readonly Style _myStyle;

        public static readonly DependencyProperty IsHighlightedProperty =
            DependencyProperty.Register("IsHighlighted", typeof (bool), typeof (SearchableDatePicker),
                                        new PropertyMetadata(default(bool)));

        static SearchableDatePicker()
        {
            Setter mySetter1 = new Setter
                                   {Property = BorderBrushProperty, Value = new SolidColorBrush(Colors.PowderBlue)};
            Setter mySetter2 = new Setter {Property = BorderThicknessProperty, Value = new Thickness(3d)};
            Trigger myTrigger = new Trigger
                                    {
                                        Property = IsHighlightedProperty,
                                        Value = true,
                                        Setters = {mySetter1, mySetter2}
                                    };
            _myStyle = new Style(typeof (SearchableDatePicker)) {Triggers = {myTrigger}};
        }

        public static Style DatePickerStyle
        {
            get { return _myStyle; }
        }

        public bool IsHighlighted
        {
            get { return (bool) GetValue(IsHighlightedProperty); }
            set { SetValue(IsHighlightedProperty, value); }
        }
    }
}