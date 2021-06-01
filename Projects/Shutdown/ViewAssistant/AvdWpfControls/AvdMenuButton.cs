// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-06-11
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace AvdWpfControls {
    public class AvdMenuButton : Button {        
        static AvdMenuButton() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AvdMenuButton), new FrameworkPropertyMetadata(typeof(AvdMenuButton)));
        }

        #region Caption
        public string Caption {
            get { return (string)GetValue(CaptionProperty); }
            set { SetValue(CaptionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Caption.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CaptionProperty =
            DependencyProperty.Register("Caption", typeof(string), typeof(AvdMenuButton), new UIPropertyMetadata(string.Empty));
        #endregion Caption


        #region ImageSource
        public ImageSource ImageSource {
            get { return (ImageSource)GetValue(ImageSourceProperty); }
            set { SetValue(ImageSourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ImageSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ImageSourceProperty =
            DependencyProperty.Register("ImageSource", typeof(ImageSource), typeof(AvdMenuButton), new UIPropertyMetadata(null));
        #endregion ImageSource


        #region ImageHeight
        public double ImageHeight {
            get { return (double)GetValue(ImageHeightProperty); }
            set { SetValue(ImageHeightProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ImageHeight.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ImageHeightProperty =
            DependencyProperty.Register("ImageHeight", typeof(double), typeof(AvdMenuButton), new UIPropertyMetadata((double)24));
        #endregion ImageHeight


        #region TextAllignment
        public TextAllignments TextAllignment {
            get { return (TextAllignments)GetValue(CaptionAllignmentProperty); }
            set { SetValue(CaptionAllignmentProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TextAllignment.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CaptionAllignmentProperty =
            DependencyProperty.Register("TextAllignment", typeof(TextAllignments), typeof(AvdMenuButton), new UIPropertyMetadata(TextAllignments.Bottom));
        #endregion TextAllignment

    }
}
