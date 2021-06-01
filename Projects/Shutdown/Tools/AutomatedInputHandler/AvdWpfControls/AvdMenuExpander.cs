// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-06-11
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Windows;
using System.Windows.Controls;

namespace AvdWpfControls {
    public class AvdMenuExpander : ItemsControl {
        static AvdMenuExpander() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AvdMenuExpander), new FrameworkPropertyMetadata(typeof(AvdMenuExpander)));
        }

        #region ImageSource
        public string ImageSource { get { return (string)GetValue(ImageSourceProperty); } set { SetValue(ImageSourceProperty, value); } }

        public static readonly DependencyProperty ImageSourceProperty =
            DependencyProperty.Register("ImageSource", typeof(string), typeof(AvdMenuExpander), new UIPropertyMetadata(null));

        #endregion ImageSource

        #region ImageHeight
        public static readonly DependencyProperty ImageHeightProperty =
            DependencyProperty.Register("ImageHeight", typeof(double), typeof(AvdMenuExpander),
                                        new UIPropertyMetadata((double)24));

        public double ImageHeight { get { return (double)GetValue(ImageHeightProperty); } set { SetValue(ImageHeightProperty, value); } }
        #endregion ImageHeight

        #region Text
        public static readonly DependencyProperty CaptionProperty =
            DependencyProperty.Register("Caption", typeof(string), typeof(AvdMenuExpander),
                                        new PropertyMetadata(default(string)));

        public string Caption { get { return (string)GetValue(CaptionProperty); } set { SetValue(CaptionProperty, value); } }

        #endregion Text
    }
}
