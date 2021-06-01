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
using eBalanceKitBusiness.FederalGazette;

namespace eBalanceKit.Windows.FederalGazette {
    /// <summary>
    /// Interaktionslogik für DlgFederalGazetteLogin.xaml
    /// </summary>
    public partial class DlgFederalGazetteLogin : Window {
        public DlgFederalGazetteLogin() {
            InitializeComponent();
            DataContext = this;
        }

        public string UserName { get; set; }
        public string Password { get; set; }

        private void btnLogin_Click(object sender, RoutedEventArgs e) { DialogResult = true; }

        private void btnCancel_Click(object sender, RoutedEventArgs e) {
            DialogResult = false;
            Close();

        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            UserName = "DemoEinsender";
            Password = "tx12pr97";
        }
    }
}