using System.IO;
using System.Windows;

namespace AvdWpfControls {
    /// <summary>
    /// Interaktionslogik für AvdWebBrowser.xaml
    /// </summary>
    public partial class AvdWebBrowser {
        public AvdWebBrowser() {
            InitializeComponent();
        }

        //public static readonly DependencyProperty HtmlProperty = DependencyProperty.RegisterAttached(
        //    "Html",
        //    typeof (string),
        //    typeof (AvdWebBrowser),
        //    new FrameworkPropertyMetadata(OnHtmlChanged));

        //[AttachedPropertyBrowsableForType(typeof(WebBrowser))]
        //public static string GetHtml(WebBrowser d) {
        //    return (string)d.GetValue(HtmlProperty);
        //}

        //public static void SetHtml(WebBrowser d, string value) {
        //    d.SetValue(HtmlProperty, value);
        //}

        //static void OnHtmlChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
        //    WebBrowser wb = d as WebBrowser;
        //    if (wb != null)
        //        wb.NavigateToString(e.NewValue as string);
        //}


        public static readonly DependencyProperty HtmlTextProperty = DependencyProperty.Register("HtmlText", typeof(string), typeof(AvdWebBrowser));

        public string HtmlText {
            get { return (string)GetValue(HtmlTextProperty); }
            set { SetValue(HtmlTextProperty, value); } }

        public static readonly DependencyProperty HtmlStreamProperty = DependencyProperty.Register("HtmlStream", typeof(Stream), typeof(AvdWebBrowser));

        public Stream HtmlStream {
            get { return (Stream)GetValue(HtmlStreamProperty); }
            set { SetValue(HtmlStreamProperty, value); } }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e) {
            base.OnPropertyChanged(e);
            
            if (e.Property == HtmlTextProperty) {
                DoBrowse(HtmlText);
            }

            if (e.Property == HtmlStreamProperty) {
                DoBrowse(HtmlStream);
            }

        }

        private void DoBrowse(object destination = null) {
            if (destination is string && !string.IsNullOrEmpty(HtmlText)) {
                browser.NavigateToString(HtmlText);
                return;
            }

            if (destination is Stream && HtmlStream != null) {
                browser.NavigateToStream(HtmlStream);
            }
        }

        /// <summary>
        /// Gets the HTML text.
        /// </summary>
        /// <returns>Returns html code as string.</returns>
        public string GetHtmlText() {
            dynamic doc = browser.Document;
            if (doc == null || doc.documentElement == null || doc.documentElement.InnerHtml() == null)
                return "<!DOCTYPE html PUBLIC " + '"' + "-//W3C//DTD XHTML 1.0 Transitional//EN" + '"' +
                                   "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd" + '"' + "><html xmlns=" +
                                   '"' + "http://www.w3.org/1999/xhtml" + '"' +
                                   "><head></head><body>" + "" + "</body></html>";

            return doc.documentElement.InnerHtml().ToString();
        }
    }
}
