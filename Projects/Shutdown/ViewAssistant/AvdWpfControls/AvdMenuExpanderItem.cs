// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-06-29
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Windows;

namespace AvdWpfControls {
    public class AvdMenuExpanderItem : ImageButton {
        static AvdMenuExpanderItem() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof (AvdMenuExpanderItem),
                                                     new FrameworkPropertyMetadata(typeof (AvdMenuExpanderItem)));
        }

        #region Description
        public static readonly DependencyProperty DescriptionProperty =
            DependencyProperty.Register("Description", typeof (string), typeof (AvdMenuExpanderItem), new PropertyMetadata(default(string)));

        public string Description { get { return (string) GetValue(DescriptionProperty); } set { SetValue(DescriptionProperty, value); } }
        #endregion // Description

    }
}