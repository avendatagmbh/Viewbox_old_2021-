// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-08-16
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Windows;
using System.Windows.Input;

namespace eBalanceKit.Windows.Security {
    /// <summary>
    /// Interaktionslogik für DlgEditUser.xaml
    /// </summary>
    public partial class DlgEditUser : Window {
        public DlgEditUser() { InitializeComponent(); 
        
        }
        private void CtlEditUser_Ok(object sender, RoutedEventArgs e) { this.Close(); }
        private void CtlEditUser_Cancel(object sender, RoutedEventArgs e) { this.Close(); }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Escape) { e.Handled = true; Close(); }
        }

        private void CtlEditUser_Loaded(object sender, RoutedEventArgs e) {
            
        }
    }
}
