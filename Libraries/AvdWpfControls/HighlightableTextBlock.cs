using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace AvdWpfControls
{
    public class HighlightableTextBlock : Control
    {
        static HighlightableTextBlock()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof (HighlightableTextBlock),
                                                     new FrameworkPropertyMetadata(typeof (HighlightableTextBlock)));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        #region HighlightFilter

        // Using a DependencyProperty as the backing store for HighlightFilter.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HighlightFilterProperty =
            DependencyProperty.Register("HighlightFilter", typeof (string), typeof (HighlightableTextBlock),
                                        new UIPropertyMetadata(null, PropertyChangedCallback));

        public string HighlightFilter
        {
            get { return (string) GetValue(HighlightFilterProperty); }
            set { SetValue(HighlightFilterProperty, value); }
        }

        #endregion // HighlightFilter

        #region Text

        // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof (string), typeof (HighlightableTextBlock),
                                        new UIPropertyMetadata(null, PropertyChangedCallback));

        public string Text
        {
            get { return (string) GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        private static void PropertyChangedCallback(DependencyObject dependencyObject,
                                                    DependencyPropertyChangedEventArgs
                                                        dependencyPropertyChangedEventArgs)
        {
            var item = (HighlightableTextBlock) dependencyObject;
            var text = item.Text;
            var filter = item.HighlightFilter;

            if (filter == null || text == null)
            {
                item.Prefix = text;
                item.HighlightedPart = null;
                item.Suffix = null;
            }
            else
            {
                var index = text.IndexOf(filter, StringComparison.InvariantCultureIgnoreCase);
                if (index < 0)
                {
                    item.Prefix = text;
                    item.HighlightedPart = null;
                    item.Suffix = null;
                }
                else
                {
                    item.Prefix = text.Substring(0, index);
                    item.HighlightedPart = text.Substring(item.Prefix.Length, filter.Length);
                    var len = item.Prefix.Length + item.HighlightedPart.Length;
                    item.Suffix = len >= text.Length ? null : text.Substring(len, text.Length - len);
                }
            }
        }

        #endregion // Text       

        #region Prefix

        // Using a DependencyProperty as the backing store for Prefix.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PrefixProperty =
            DependencyProperty.Register("Prefix", typeof (string), typeof (HighlightableTextBlock),
                                        new UIPropertyMetadata(null));

        public string Prefix
        {
            get { return (string) GetValue(PrefixProperty); }
            set { SetValue(PrefixProperty, value); }
        }

        #endregion // Prefix

        #region HighlightedPart

        // Using a DependencyProperty as the backing store for HighlightedPart.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HighlightedPartProperty =
            DependencyProperty.Register("HighlightedPart", typeof (string), typeof (HighlightableTextBlock),
                                        new UIPropertyMetadata(null));

        public string HighlightedPart
        {
            get { return (string) GetValue(HighlightedPartProperty); }
            set { SetValue(HighlightedPartProperty, value); }
        }

        #endregion // HighlightedPart

        #region Suffix

        // Using a DependencyProperty as the backing store for Suffix.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SuffixProperty =
            DependencyProperty.Register("Suffix", typeof (string), typeof (HighlightableTextBlock),
                                        new UIPropertyMetadata(null));

        public string Suffix
        {
            get { return (string) GetValue(SuffixProperty); }
            set { SetValue(SuffixProperty, value); }
        }

        #endregion // Suffix

        #region SingleLine

        // Using a DependencyProperty as the backing store for SingleLine.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SingleLineProperty =
            DependencyProperty.Register("SingleLine", typeof (bool), typeof (HighlightableTextBlock),
                                        new UIPropertyMetadata(false));

        public bool SingleLine
        {
            get { return (bool) GetValue(SingleLineProperty); }
            set { SetValue(SingleLineProperty, value); }
        }

        #endregion
    }
}