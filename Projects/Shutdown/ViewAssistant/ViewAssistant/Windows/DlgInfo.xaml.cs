using System.Windows;
using System.Windows.Input;
using System.Windows.Navigation;
using System.Configuration;
using System.Diagnostics;

namespace ViewAssistant.Windows
{
    /// <summary>
    /// Interaction logic for DlgInfo.xaml
    /// </summary>
    public partial class DlgInfo : Window
    {
        public DlgInfo()
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

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            ApplicationConfigSection config = (ApplicationConfigSection)ConfigurationManager.GetSection("ApplicationConfig");
            txtVersion.Text = config.VersionString;
            txtArchitecture.Text = config.ProgramArchitecture;
        }

        private void HyperlinkRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        private void BtnCloseClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
