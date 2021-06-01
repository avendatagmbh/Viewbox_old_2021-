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
using System.Windows.Navigation;
using System.Windows.Shapes;
using eBalanceKitConfig.Models;

namespace eBalanceKitConfig.Controls {
    /// <summary>
    /// Interaktionslogik für CtlProxy.xaml
    /// </summary>
    public partial class CtlProxy : UserControl {
        public CtlProxy() {
            InitializeComponent();
            
            if (this.Model != null) {
                this.Model.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(Model_PropertyChanged);
            }
            
            this.DataContextChanged += new DependencyPropertyChangedEventHandler(CtlProxy_DataContextChanged);
        }

        void CtlProxy_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            if (this.Model != null) {
                this.Model.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(Model_PropertyChanged);
            }   
        }

        void Model_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case "Password":
                    if (this.Model == null) return;
                    txtPassword.Password = this.Model.Password;
                    break;
            }
        }

        ProxyConfig Model {
            get { return DataContext as ProxyConfig; }
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
