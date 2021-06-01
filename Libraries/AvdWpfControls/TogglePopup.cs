using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace AvdWpfControls
{
    [ContentProperty("Content")]
    public class TogglePopup : Control
    {
        static TogglePopup()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof (TogglePopup),
                                                     new FrameworkPropertyMetadata(typeof (TogglePopup)));
        }

        #region Content

        public static readonly DependencyProperty ContentProperty =
            DependencyProperty.Register("Content", typeof (object), typeof (TogglePopup), new UIPropertyMetadata(null));

        public object Content
        {
            get { return GetValue(ContentProperty); }
            set { SetValue(ContentProperty, value); }
        }

        #endregion Content

        #region ImageSource

        public static readonly DependencyProperty ImageSourceProperty =
            DependencyProperty.Register("ImageSource", typeof (string), typeof (TogglePopup),
                                        new UIPropertyMetadata(null));

        public string ImageSource
        {
            get { return (string) GetValue(ImageSourceProperty); }
            set { SetValue(ImageSourceProperty, value); }
        }

        #endregion ImageSource

        #region ImageHeight

        public static readonly DependencyProperty ImageHeightProperty =
            DependencyProperty.Register("ImageHeight", typeof (double), typeof (TogglePopup),
                                        new UIPropertyMetadata((double) 12));

        public double ImageHeight
        {
            get { return (double) GetValue(ImageHeightProperty); }
            set { SetValue(ImageHeightProperty, value); }
        }

        #endregion ImageHeight

        #region Text 

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof (string), typeof (TogglePopup),
                                        new PropertyMetadata(default(string)));

        public string Text
        {
            get { return (string) GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        #endregion Text
    }
}