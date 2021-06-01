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
using System.Windows.Media.Animation;

namespace AvdWpfControls {

    public class SlideControlItem : SlideControlBase {

        static SlideControlItem() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SlideControlItem), new FrameworkPropertyMetadata(typeof(SlideControlItem)));
        }

        public SlideControlItem() {
            Visibility = System.Windows.Visibility.Collapsed;
        }

    }
}
