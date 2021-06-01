using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace AvdWpfControls
{
    public class AvdMenuExpanderItem : Button
    {
        static AvdMenuExpanderItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof (AvdMenuExpanderItem),
                                                     new FrameworkPropertyMetadata(typeof (AvdMenuExpanderItem)));
        }

        #region Caption

        public static readonly DependencyProperty CaptionProperty =
            DependencyProperty.Register("Caption", typeof (string), typeof (AvdMenuExpanderItem),
                                        new PropertyMetadata(default(string)));

        public string Caption
        {
            get { return (string) GetValue(CaptionProperty); }
            set { SetValue(CaptionProperty, value); }
        }

        #endregion // Caption

        #region Description

        public static readonly DependencyProperty DescriptionProperty =
            DependencyProperty.Register("Description", typeof (string), typeof (AvdMenuExpanderItem),
                                        new PropertyMetadata(default(string)));

        public string Description
        {
            get { return (string) GetValue(DescriptionProperty); }
            set { SetValue(DescriptionProperty, value); }
        }

        #endregion // Description

        #region ImageSource

        // Using a DependencyProperty as the backing store for ImageSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ImageSourceProperty =
            DependencyProperty.Register("ImageSource", typeof (ImageSource), typeof (AvdMenuExpanderItem),
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
            DependencyProperty.Register("ImageHeight", typeof (double), typeof (AvdMenuExpanderItem),
                                        new UIPropertyMetadata((double) 32));

        public double ImageHeight
        {
            get { return (double) GetValue(ImageHeightProperty); }
            set { SetValue(ImageHeightProperty, value); }
        }

        #endregion ImageHeight
    }
}