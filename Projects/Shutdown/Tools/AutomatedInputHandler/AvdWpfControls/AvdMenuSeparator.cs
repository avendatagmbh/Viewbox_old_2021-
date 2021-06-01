using System.Windows;
using System.Windows.Controls;

namespace AvdWpfControls {
    public class AvdMenuSeparator : Control {
        static AvdMenuSeparator() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AvdMenuSeparator), new FrameworkPropertyMetadata(typeof(AvdMenuSeparator)));
        }
    }
}
