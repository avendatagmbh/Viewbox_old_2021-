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
using eBalanceKitBusiness.Structures;
using eBalanceKitBase;

namespace eBalanceKit.Windows {
    /// <summary>
    /// Interaktionslogik für DlgProxy.xaml
    /// </summary>
    public partial class DlgProxy : Window {
        
        public DlgProxy(ProxyConfig proxyConfig) {
            InitializeComponent();
            this.Model = proxyConfig;
            this.Model.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(Model_PropertyChanged);
            txtPassword.Password = proxyConfig.Password;
        }

        void Model_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case "Password":
                    if (this.Model == null) return;
                    txtPassword.Password = this.Model.Password;
                    break;
            }           
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

        public ProxyConfig Model { get; set; }
    }
}
