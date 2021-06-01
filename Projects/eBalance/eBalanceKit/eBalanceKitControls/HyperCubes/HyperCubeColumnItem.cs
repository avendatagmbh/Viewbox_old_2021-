// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-01-23
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Windows;
using System.Windows.Controls;

namespace eBalanceKitControls.HyperCubes {
    public class HyperCubeColumnItem : HeaderedItemsControl {

        public HyperCubeColumnItem() {
        DataContextChanged += new DependencyPropertyChangedEventHandler(HyperCubeColumnItem_DataContextChanged);
        }

        void HyperCubeColumnItem_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            var x = DataContext;
        }

        static HyperCubeColumnItem() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof (HyperCubeColumnItem),
                                                     new FrameworkPropertyMetadata(typeof (HyperCubeColumnItem)));
        }



        public bool IsExpanded {
            get { return (bool)GetValue(IsExpandedProperty); }
            set { SetValue(IsExpandedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsExpanded.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsExpandedProperty =
            DependencyProperty.Register("IsExpanded", typeof(bool), typeof(HyperCubeColumnItem), new UIPropertyMetadata(false));


    }
}