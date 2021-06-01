// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-08-25
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using Business;
using TransDATA.Models;

namespace TransDATA.Windows {
    /// <summary>
    /// Interaktionslogik für DlgLogin.xaml
    /// </summary>
    public partial class DlgLogin : Window {
        public DlgLogin() {
            InitializeComponent();

            ctlLogin.txtVersion.Text = "Version " +
                                       Assembly.GetEntryAssembly().GetName().Version.Major + "." +
                                       Assembly.GetEntryAssembly().GetName().Version.Minor +
                                       (Assembly.GetEntryAssembly().GetName().Version.Build > 0
                                            ? "." + Assembly.GetEntryAssembly().GetName().Version.Build
                                            : "");

            DataContext = new LoginModel(this);

        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Escape) {
                e.Handled = true;
                Close();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            if (!AppController.IsInitialized) Close();
        }
    }
}