// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-07-02
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Windows;

namespace AvdWpfControls {
    public class AvdMenuCheckbox : ImageCheckBox {
        static AvdMenuCheckbox() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof (AvdMenuCheckbox),
                                                     new FrameworkPropertyMetadata(typeof (AvdMenuCheckbox)));
        }
    }
}