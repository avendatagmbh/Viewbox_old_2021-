using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace eBalanceKit.Controls.RichTextEditor {
    /// <summary>
    /// Interaktionslogik für RichTextBoxEditor.xaml
    /// </summary>
    public partial class RichTextBoxEditor : UserControl
    {
        public static readonly DependencyProperty TextProperty =
          DependencyProperty.Register("HtmlContent", typeof(string), typeof(RichTextBoxEditor),
          new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty LoadCommandProperty =
          DependencyProperty.Register("LoadCommand", typeof(ICommand), typeof(RichTextBoxEditor),
          new PropertyMetadata(null));

        public RichTextBoxEditor()
        {
            InitializeComponent();
        }

        public string HtmlContent
        {
            get { return GetValue(TextProperty) as string; }
            set { 
                SetValue(TextProperty, value);
            }
        }

        public ICommand LoadCommand
        {
            get { return GetValue(LoadCommandProperty) as ICommand; }
            set {
                SetValue(LoadCommandProperty, value);
            }
        }

        private void rtbPreviewKeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                var newPointer = mainRTB.Selection.Start.InsertLineBreak();
                mainRTB.Selection.Select(newPointer, newPointer);
                e.Handled = true;
            }
        }
    }
}