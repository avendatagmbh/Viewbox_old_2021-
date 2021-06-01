using System.Windows;
using System.Windows.Controls;

namespace AvdWpfControls
{
    public class TextboxWithDefaultText : TextBox
    {
        public static readonly DependencyProperty DefaultTextProperty =
            DependencyProperty.Register("DefaultText", typeof (string), typeof (TextboxWithDefaultText),
                                        new PropertyMetadata(default(string)));

        static TextboxWithDefaultText()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof (TextboxWithDefaultText),
                                                     new FrameworkPropertyMetadata(typeof (TextboxWithDefaultText)));
        }

        public string DefaultText
        {
            get { return (string) GetValue(DefaultTextProperty); }
            set { SetValue(DefaultTextProperty, value); }
        }
    }
}