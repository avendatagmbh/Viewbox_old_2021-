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
    public class TextboxWithDefaultText : TextBox {
        static TextboxWithDefaultText() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TextboxWithDefaultText), new FrameworkPropertyMetadata(typeof(TextboxWithDefaultText)));
            
        }

        public static readonly DependencyProperty DefaultTextProperty =
            DependencyProperty.Register("DefaultText", typeof (string), typeof (TextboxWithDefaultText), new PropertyMetadata(default(string)));

        public string DefaultText { get { return (string) GetValue(DefaultTextProperty); } set { SetValue(DefaultTextProperty, value); } }
    }
}
