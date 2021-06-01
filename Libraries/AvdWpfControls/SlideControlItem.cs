using System.Windows;

namespace AvdWpfControls
{
    public class SlideControlItem : SlideControlBase
    {
        static SlideControlItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof (SlideControlItem),
                                                     new FrameworkPropertyMetadata(typeof (SlideControlItem)));
        }

        public SlideControlItem()
        {
            Visibility = Visibility.Collapsed;
        }
    }
}