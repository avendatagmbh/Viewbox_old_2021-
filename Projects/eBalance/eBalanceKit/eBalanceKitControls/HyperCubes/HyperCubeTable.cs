// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-01-19
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Windows;
using System.Windows.Controls;

namespace eBalanceKitControls.HyperCubes {

    public class HyperCubeTable : Control {
        static HyperCubeTable() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(HyperCubeTable), new FrameworkPropertyMetadata(typeof(HyperCubeTable)));
        }
    }
}
