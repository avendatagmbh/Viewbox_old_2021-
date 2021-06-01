using System.Windows;
using System.Windows.Input;

namespace ViewAssistant.Windows
{
    /// <summary>
    /// Interaction logic for DlgViewProfileSettings.xaml
    /// </summary>
    public partial class DlgViewProfileSettings
    {
        public DlgViewProfileSettings()
        {
            InitializeComponent();
        }

        private void WindowPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                e.Handled = true;
                Close();
            }
        }

        private void BtnCloseClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
