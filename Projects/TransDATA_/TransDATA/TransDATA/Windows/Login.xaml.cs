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
using System.Reflection;

namespace TransDATA.Windows {
    /// <summary>
    /// Interaktionslogik für Login.xaml
    /// </summary>
    public partial class Login : Window {
        
        public Login() {
            InitializeComponent();

            TransDATABusiness.Global.Init();

            this.AppConfig = TransDATABusiness.Global.AppConfig;

            lblVersion.Content =
                "Version " +
                Assembly.GetExecutingAssembly().GetName().Version.Major + "." +
                Assembly.GetExecutingAssembly().GetName().Version.Minor + "." +
                Assembly.GetExecutingAssembly().GetName().Version.Build;

            this.DataContext = this;
            txtPassword.Focus();
            _keepConfig = false;
        }

        private bool _keepConfig;

        private TransDATABusiness.Structures.AppConfig AppConfig { get; set; }        

        /// <summary>
        /// Handles the Click event of the btnLogin control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void btnLogin_Click(object sender, RoutedEventArgs e) {
            CheckPassword();
        }

        /// <summary>
        /// Handles the Closed event of the Window control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void Window_Closed(object sender, EventArgs e) {
            if (!_keepConfig) AppConfig.Dispose();
        }

        /// <summary>
        /// Handles the KeyDown event of the txtPassword control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.KeyEventArgs"/> instance containing the event data.</param>
        private void txtPassword_KeyDown(object sender, KeyEventArgs e){
            if (e.Key == Key.Enter) {
                CheckPassword();
            }
        }

        private void CheckPassword() {
            try {
                if (TransDATABusiness.Global.AppConfig.CheckPassword(txtPassword.Password)) {
                    new MainWindow().Show();
                    _keepConfig = true;
                    this.Close();
                } else {
                    MessageBox.Show("Falsches Password.");
                }
            } catch (Exception ex) {
                MessageBox.Show("Fehler beim Initialisieren des Hauptformulars.");
            }
        }
    }
}
