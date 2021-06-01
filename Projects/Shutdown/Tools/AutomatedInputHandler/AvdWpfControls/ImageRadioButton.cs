// --------------------------------------------------------------------------------
// author: Mirko Dibbert (idea by Sebastian Vetter)
// since:  2012-02-12
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace AvdWpfControls {
    public class ImageRadioButton : RadioButton {
        static ImageRadioButton() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof (ImageRadioButton),
                                                     new FrameworkPropertyMetadata(typeof (ImageRadioButton)));
        }

        #region ImageSource
        public static readonly DependencyProperty ImageSourceProperty =
            DependencyProperty.Register("ImageSource", typeof(ImageSource), typeof(ImageRadioButton),
                                        new UIPropertyMetadata(null));

        public ImageSource ImageSource { get { return (ImageSource) GetValue(ImageSourceProperty); } set { SetValue(ImageSourceProperty, value); } }
        #endregion ImageSource

        #region ImageHeight
        public static readonly DependencyProperty ImageHeightProperty =
            DependencyProperty.Register("ImageHeight", typeof(double), typeof(ImageRadioButton),
                                        new UIPropertyMetadata((double) 16));

        public double ImageHeight { get { return (double) GetValue(ImageHeightProperty); } set { SetValue(ImageHeightProperty, value); } }
        #endregion ImageHeight
    }
}