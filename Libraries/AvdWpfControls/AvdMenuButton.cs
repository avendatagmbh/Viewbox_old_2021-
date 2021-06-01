using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace AvdWpfControls
{
    public enum TextAllignments
    {
        Left,
        Right,
        Top,
        Bottom
    }

    public class AvdMenuButton : Button
    {
        public static readonly DependencyProperty CaptionAllignmentProperty =
            DependencyProperty.Register("TextAllignment", typeof (TextAllignments), typeof (AvdMenuButton),
                                        new UIPropertyMetadata(TextAllignments.Bottom));

        static AvdMenuButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof (AvdMenuButton),
                                                     new FrameworkPropertyMetadata(typeof (AvdMenuButton)));
        }

        #region Caption

        // Using a DependencyProperty as the backing store for Caption.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CaptionProperty =
            DependencyProperty.Register("Caption", typeof (string), typeof (AvdMenuButton),
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
            DependencyProperty.Register("ImageSource", typeof (ImageSource), typeof (AvdMenuButton),
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
            DependencyProperty.Register("ImageHeight", typeof (double), typeof (AvdMenuButton),
                                        new UIPropertyMetadata((double) 24));

        public double ImageHeight
        {
            get { return (double) GetValue(ImageHeightProperty); }
            set { SetValue(ImageHeightProperty, value); }
        }

        #endregion ImageHeight

        public TextAllignments TextAllignment
        {
            get { return (TextAllignments) GetValue(CaptionAllignmentProperty); }
            set { SetValue(CaptionAllignmentProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TextAllignment.  This enables animation, styling, binding, etc...
    }
}