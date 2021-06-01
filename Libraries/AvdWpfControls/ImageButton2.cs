using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace AvdWpfControls
{
    public class ImageButton2 : Button
    {
        static ImageButton2()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof (ImageButton2),
                                                     new FrameworkPropertyMetadata(typeof (ImageButton2)));
        }

        #region Caption

        // Using a DependencyProperty as the backing store for Caption.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CaptionProperty =
            DependencyProperty.Register("Caption", typeof (string), typeof (ImageButton2),
                                        new UIPropertyMetadata(string.Empty));

        public string Caption
        {
            get { return (string) GetValue(CaptionProperty); }
            set { SetValue(CaptionProperty, value); }
        }

        #endregion Caption

        #region ImageSource

        // Using a DependencyProperty as the backing store for ImageSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ImageSourceProperty =
            DependencyProperty.Register("ImageSource", typeof (ImageSource), typeof (ImageButton2),
                                        new UIPropertyMetadata(null));

        public ImageSource ImageSource
        {
            get { return (ImageSource) GetValue(ImageSourceProperty); }
            set { SetValue(ImageSourceProperty, value); }
        }

        #endregion ImageSource

        #region ImageHeight

        // Using a DependencyProperty as the backing store for ImageHeight.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ImageHeightProperty =
            DependencyProperty.Register("ImageHeight", typeof (double), typeof (ImageButton2),
                                        new UIPropertyMetadata((double) 16));

        public double ImageHeight
        {
            get { return (double) GetValue(ImageHeightProperty); }
            set { SetValue(ImageHeightProperty, value); }
        }

        #endregion ImageHeight
    }
}