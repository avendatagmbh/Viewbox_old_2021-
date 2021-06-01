using System.Windows.Input;

namespace ViewAssistant.Windows
{
    /// <summary>
    /// Interaction logic for DlgProfile.xaml
    /// </summary>
    public partial class DlgProfile
    {
        public DlgProfile()
        {
            InitializeComponent();
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
