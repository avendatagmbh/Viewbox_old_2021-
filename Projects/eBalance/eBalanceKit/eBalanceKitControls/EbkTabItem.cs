using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace eBalanceKitControls {

    public class EbkTabItem : TabItem {
        static EbkTabItem() {            
            DefaultStyleKeyProperty.OverrideMetadata(typeof(EbkTabItem), new FrameworkPropertyMetadata(typeof(EbkTabItem)));
        }

        #region Caption
        public string Caption {
            get { return (string)GetValue(CaptionProperty); }
            set { SetValue(CaptionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Caption.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CaptionProperty =
            DependencyProperty.Register("Caption", typeof(string), typeof(EbkTabItem), new UIPropertyMetadata(string.Empty));
        #endregion Caption

        #region DetailCaption
        public string DetailCaption {
            get { return (string)GetValue(DetailCaptionProperty); }
            set { SetValue(DetailCaptionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DetailCaption.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DetailCaptionProperty =
            DependencyProperty.Register("DetailCaption", typeof(string), typeof(EbkTabItem), new UIPropertyMetadata(null));
        #endregion DetailCaption

        #region ImageSource
        public ImageSource ImageSource {
            get { return (ImageSource)GetValue(ImageSourceProperty); }
            set { SetValue(ImageSourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ImageSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ImageSourceProperty =
            DependencyProperty.Register("ImageSource", typeof(ImageSource), typeof(EbkTabItem), new UIPropertyMetadata(null));
        #endregion ImageSource

        #region HorizontalAlignment
        public HorizontalAlignment HeaderHorizontalAlignment {
            get { return (HorizontalAlignment)GetValue(HeaderHorizontalAlignmentProperty); }
            set { SetValue(HeaderHorizontalAlignmentProperty, value); }
        }

        // Using a DependencyProperty as the backing store for HorizontalAlignment.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HeaderHorizontalAlignmentProperty =
            DependencyProperty.Register("HeaderHorizontalAlignment", typeof(HorizontalAlignment), typeof(EbkTabItem), new UIPropertyMetadata(HorizontalAlignment.Center));
        #endregion HorizontalAlignment
    }
    
    public class EbkWarningTabItem : EbkTabItem{
        static EbkWarningTabItem() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(EbkWarningTabItem), new FrameworkPropertyMetadata(typeof(EbkWarningTabItem)));
        }
        
        public Visibility WarningMessageVisibility {
            get { return (Visibility)GetValue(WarningMessageVisibilityProperty); }
            set { SetValue(WarningMessageVisibilityProperty, value); }
        }

        // Using a DependencyProperty as the backing store for WarningMessageVisibility.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty WarningMessageVisibilityProperty =
            DependencyProperty.Register("WarningMessageVisibility", typeof(Visibility), typeof(EbkWarningTabItem), new UIPropertyMetadata(Visibility.Collapsed));
        
        public Visibility ErrorMessageVisibility {
            get { return (Visibility)GetValue(ErrorMessageVisibilityProperty); }
            set { SetValue(ErrorMessageVisibilityProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ErrorMessageVisiblity.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ErrorMessageVisibilityProperty =
            DependencyProperty.Register("ErrorMessageVisibility", typeof(Visibility), typeof(EbkWarningTabItem), new UIPropertyMetadata(Visibility.Collapsed));

        
    }
}
