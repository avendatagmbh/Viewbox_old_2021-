using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace AvdWpfControls {
    public class HighlightableTextBlock : Control {
        static HighlightableTextBlock() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(HighlightableTextBlock), new FrameworkPropertyMetadata(typeof(HighlightableTextBlock)));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName) {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        #region HighlightFilter
        public string HighlightFilter {
            get { return (string)GetValue(HighlightFilterProperty); }
            set { SetValue(HighlightFilterProperty, value); }
        }

        // Using a DependencyProperty as the backing store for HighlightFilter.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HighlightFilterProperty =
            DependencyProperty.Register("HighlightFilter", typeof(string), typeof(HighlightableTextBlock), new UIPropertyMetadata(null, PropertyChangedCallback));
        #endregion // HighlightFilter

        #region Text
        public string Text {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(HighlightableTextBlock), new UIPropertyMetadata(null, PropertyChangedCallback));

        private static void PropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs) {
            var item = (HighlightableTextBlock)dependencyObject;
            var text = item.Text;
            var filter = item.HighlightFilter;
           
            if (filter == null || text == null) {
                item.Prefix = text;
                item.HighlightedPart = null;
                item.Suffix = null;
            } else {
                var index = text.IndexOf(filter, StringComparison.OrdinalIgnoreCase);
                if (index < 0) {
                    item.Prefix = text;
                    item.HighlightedPart = null;
                    item.Suffix = null;
                } else {
                    item.Prefix = text.Substring(0, index);
                    item.HighlightedPart = text.Substring(item.Prefix.Length, filter.Length);
                    var len = item.Prefix.Length + item.HighlightedPart.Length;
                    item.Suffix = len >= text.Length ? null : text.Substring(len, text.Length - len);
                }
            }
        }
        #endregion // Text       

        #region Prefix
        public string Prefix {
            get { return (string)GetValue(PrefixProperty); }
            set { SetValue(PrefixProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Prefix.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PrefixProperty =
            DependencyProperty.Register("Prefix", typeof(string), typeof(HighlightableTextBlock), new UIPropertyMetadata(null));
        #endregion // Prefix

        #region HighlightedPart


        public string HighlightedPart {
            get { return (string)GetValue(HighlightedPartProperty); }
            set { SetValue(HighlightedPartProperty, value); }
        }

        // Using a DependencyProperty as the backing store for HighlightedPart.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HighlightedPartProperty =
            DependencyProperty.Register("HighlightedPart", typeof(string), typeof(HighlightableTextBlock), new UIPropertyMetadata(null));
        #endregion // HighlightedPart
        
        #region Suffix


        public string Suffix {
            get { return (string)GetValue(SuffixProperty); }
            set { SetValue(SuffixProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Suffix.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SuffixProperty =
            DependencyProperty.Register("Suffix", typeof(string), typeof(HighlightableTextBlock), new UIPropertyMetadata(null));
        #endregion // Suffix

        #region SingleLine

        public bool SingleLine {
            get { return (bool)GetValue(SingleLineProperty); }
            set { SetValue(SingleLineProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SingleLine.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SingleLineProperty =
            DependencyProperty.Register("SingleLine", typeof(bool), typeof(HighlightableTextBlock), new UIPropertyMetadata(false));

        #endregion
    }
}
