using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AvdWpfControls {
    public class AvdComboBox : ComboBox {
        static AvdComboBox() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AvdComboBox), new FrameworkPropertyMetadata(typeof(AvdComboBox)));
            Setter mySetter1 = new Setter {Property = BorderBrushProperty, Value = new SolidColorBrush(Colors.PowderBlue)};
            Setter mySetter2 = new Setter {Property = BorderThicknessProperty, Value = new Thickness(3d)};
            Trigger myTrigger = new Trigger {
                Property = IsHighlightedProperty,
                Value = true,
                Setters = {mySetter1, mySetter2}
            };
            _myStyle = new Style (typeof(AvdComboBox)) {Triggers = {myTrigger}};
        }

        private static Style _myStyle;

        public static Style AvdComboBoxStyle { get { return _myStyle; } }

        public static readonly DependencyProperty IsHighlightedProperty =
            DependencyProperty.Register("IsHighlighted", typeof (bool), typeof (AvdComboBox), new PropertyMetadata(default(bool)));

        public bool IsHighlighted { get { return (bool) GetValue(IsHighlightedProperty); } set { SetValue(IsHighlightedProperty, value); } }

        #region MakeChoiceMessage
        //--------------------------------------------------------------------------------
        public string SelectValueMessage {
            get { return (string)GetValue(SelectValueMessageProperty); }
            set { SetValue(SelectValueMessageProperty, value); }
        }

        public static readonly DependencyProperty SelectValueMessageProperty =
            DependencyProperty.Register("SelectValueMessage", typeof(string), typeof(AvdComboBox), new UIPropertyMetadata(string.Empty));
        //--------------------------------------------------------------------------------
        #endregion MakeChoiceMessage

        
    }
}
