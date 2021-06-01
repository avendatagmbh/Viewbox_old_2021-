using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace AvdWpfControls
{
    public class ImageCheckBox : CheckBox
    {
        static ImageCheckBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof (ImageCheckBox),
                                                     new FrameworkPropertyMetadata(typeof (ImageCheckBox)));
        }

        #region ImageSource

        // Using a DependencyProperty as the backing store for ImageSourceChecked.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ImageSourceCheckedProperty =
            DependencyProperty.Register("ImageSourceChecked", typeof (ImageSource), typeof (ImageCheckBox),
                                        new UIPropertyMetadata(null));

        public ImageSource ImageSourceChecked
        {
            get { return (ImageSource) GetValue(ImageSourceCheckedProperty); }
            set { SetValue(ImageSourceCheckedProperty, value); }
        }

        #endregion ImagCheckedeSource

        #region ImageCheckedSource

        // Using a DependencyProperty as the backing store for ImageSourceUnchecked.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ImageSourceUncheckedProperty =
            DependencyProperty.Register("ImageSourceUnchecked", typeof (ImageSource), typeof (ImageCheckBox),
                                        new UIPropertyMetadata(null));

        public ImageSource ImageSourceUnchecked
        {
            get { return (ImageSource) GetValue(ImageSourceUncheckedProperty); }
            set { SetValue(ImageSourceUncheckedProperty, value); }
        }

        #endregion ImageSourceUnchecked

        #region ImageHeight

        // Using a DependencyProperty as the backing store for ImageHeight.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ImageHeightProperty =
            DependencyProperty.Register("ImageHeight", typeof (double), typeof (ImageCheckBox),
                                        new UIPropertyMetadata((double) 24));

        public double ImageHeight
        {
            get { return (double) GetValue(ImageHeightProperty); }
            set { SetValue(ImageHeightProperty, value); }
        }

        #endregion ImageHeight
    }
}