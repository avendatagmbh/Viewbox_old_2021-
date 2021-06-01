// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-01-23
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Windows;
using System.Windows.Controls;

namespace eBalanceKitControls.HyperCubes {
    public class HyperCubeRow : ItemsControl {
        static HyperCubeRow() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof (HyperCubeRow),
                                                     new FrameworkPropertyMetadata(typeof (HyperCubeRow)));
        }
    }
}