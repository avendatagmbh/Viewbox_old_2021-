using System.Windows;

namespace AvdWpfControls
{
    public class AvdMenuCheckbox : ImageCheckBox
    {
        static AvdMenuCheckbox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof (AvdMenuCheckbox),
                                                     new FrameworkPropertyMetadata(typeof (AvdMenuCheckbox)));
        }
    }
}