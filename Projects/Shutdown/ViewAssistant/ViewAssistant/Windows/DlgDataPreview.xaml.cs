using System.Windows;
using System.Windows.Input;

namespace ViewAssistant.Windows
{
    /// <summary>
    /// Interaction logic for DlgDataPreview.xaml
    /// </summary>
    public partial class DlgDataPreview
    {
        public DlgDataPreview()
        {
            InitializeComponent();
        }

        private void BtnCloseClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void WindowPreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                e.Handled = true;
                Close();
            }
        }
    }
}
