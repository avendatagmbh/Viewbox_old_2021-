// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-03-09
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Windows;
using System.Windows.Controls;

namespace eBalanceKitControls {
    public class SelectionWithDetailInfo : ItemsControl {
        static SelectionWithDetailInfo() {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof (SelectionWithDetailInfo),
                new FrameworkPropertyMetadata(typeof (SelectionWithDetailInfo)));
        }

        public string DetailCaption { get { return (string) GetValue(DetailCaptionProperty); } set { SetValue(DetailCaptionProperty, value); } }
        public static readonly DependencyProperty DetailCaptionProperty =
            DependencyProperty.Register("DetailCaption", typeof (string), typeof (SelectionWithDetailInfo),
                                        new UIPropertyMetadata(null));

        public string DetailContent { get { return (string) GetValue(DetailContentProperty); } set { SetValue(DetailContentProperty, value); } }
        public static readonly DependencyProperty DetailContentProperty =
            DependencyProperty.Register("DetailContent", typeof(string), typeof(SelectionWithDetailInfo), new UIPropertyMetadata(null));

        
    }
}
