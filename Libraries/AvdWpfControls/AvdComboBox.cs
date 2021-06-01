using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace AvdWpfControls
{
    public class AvdComboBox : ComboBox
    {
        private static readonly Style _myStyle;

        public static readonly DependencyProperty IsHighlightedProperty =
            DependencyProperty.Register("IsHighlighted", typeof (bool), typeof (AvdComboBox),
                                        new PropertyMetadata(default(bool)));

        static AvdComboBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof (AvdComboBox),
                                                     new FrameworkPropertyMetadata(typeof (AvdComboBox)));
            Setter mySetter1 = new Setter
                                   {Property = BorderBrushProperty, Value = new SolidColorBrush(Colors.PowderBlue)};
            Setter mySetter2 = new Setter {Property = BorderThicknessProperty, Value = new Thickness(3d)};
            Trigger myTrigger = new Trigger
                                    {
                                        Property = IsHighlightedProperty,
                                        Value = true,
                                        Setters = {mySetter1, mySetter2}
                                    };
            _myStyle = new Style(typeof (AvdComboBox)) {Triggers = {myTrigger}};
        }

        public static Style AvdComboBoxStyle
        {
            get { return _myStyle; }
        }

        public bool IsHighlighted
        {
            get { return (bool) GetValue(IsHighlightedProperty); }
            set { SetValue(IsHighlightedProperty, value); }
        }

        #region MakeChoiceMessage

        //--------------------------------------------------------------------------------

        public static readonly DependencyProperty SelectValueMessageProperty =
            DependencyProperty.Register("SelectValueMessage", typeof (string), typeof (AvdComboBox),
                                        new UIPropertyMetadata(string.Empty));

        public string SelectValueMessage
        {
            get { return (string) GetValue(SelectValueMessageProperty); }
            set { SetValue(SelectValueMessageProperty, value); }
        }

        //--------------------------------------------------------------------------------

        #endregion MakeChoiceMessage
    }
}