using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace eBalanceKitConfig {
    /// <summary>
    /// Interaktionslogik für DlgProxy.xaml
    /// </summary>
    public partial class DlgProxy : Window {

        public DlgProxy(string host, string port, string username, string password) {
            InitializeComponent();
            txtHost.Text = host;
            txtPort.Text = port;
            txtUsername.Text = username;
            txtPassword.Password = password;
        }

        private void btnOK_Click(object sender, RoutedEventArgs e) {
            this.DialogResult = true;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e) {
            this.DialogResult = false;
        }


        private void txtHost_LostFocus(object sender, RoutedEventArgs e) {
            txtHost.Text = txtHost.Text.Trim();
        }

        private void txtUsername_LostFocus(object sender, RoutedEventArgs e) {
            txtUsername.Text = txtUsername.Text.Trim();
        }

        private void txtPassword_LostFocus(object sender, RoutedEventArgs e) {
            txtPassword.Password = txtPassword.Password.Trim();
        }

        private void txtPort_LostFocus(object sender, RoutedEventArgs e) {
            txtPort.Text = txtPort.Text.Trim();
        }
    }
}
