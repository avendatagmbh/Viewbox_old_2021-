using System.Windows;
using System.Windows.Input;
using eBalanceKit.Models.Assistants;

namespace eBalanceKit.Windows.BalanceList {
    /// <summary>
    /// Interaction logic for DlgProfileName.xaml
    /// </summary>
    public partial class DlgProfileName {
        public DlgProfileName() {
            InitializeComponent();
        }

        ProfileNameModel Model {
            get { return DataContext as ProfileNameModel; }
        }

        private void YesButtonClick(object sender, RoutedEventArgs e) {
            Model.PressedOk = true;
            Close();
        }

        private void NoButtonClick(object sender, RoutedEventArgs e) {
            Close();
        }

        private void Window_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e) {
            switch (e.Key) {
                case Key.Escape:
                    NoButtonClick(sender, null);
                    e.Handled = true;
                    break;
                case Key.Enter:
                    YesButtonClick(sender, null);
                    e.Handled = true;
                    break;
            }
        }

    }
}
