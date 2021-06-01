// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-01-23
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Windows;
using System.Windows.Controls;

namespace eBalanceKitControls.HyperCubes {
    public class HyperCubeColumnCollection : ItemsControl {
        static HyperCubeColumnCollection() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof (HyperCubeColumnCollection),
                                                     new FrameworkPropertyMetadata(typeof (HyperCubeColumnCollection)));
        }
    }
}