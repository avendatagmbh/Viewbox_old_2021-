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
using System.IO;
using eBalanceKitBusiness.Structures.DbMapping;
using eBalanceKitBase;
using System.Diagnostics;
using eBalanceKitBusiness.Structures;

namespace eBalanceKit.Windows {
    /// <summary>
    /// Interaktionslogik für Register.xaml
    /// </summary>
    public partial class Register : Window {
        private bool _keepAppConfig;

        public Register() {
            InitializeComponent();
        }

        private void btnOk_Click(object sender, RoutedEventArgs e) {

            string serial = txtSerial.Text.ToUpper();
            string key = txtKey.Text.ToUpper();
            string company = txtCompany.Text;

            string key1 = Utils.StringUtils.CreateKey(company, serial);

            if (key1.Replace("-", "") != key.Replace("-", "").Replace(" ", "")) {
                MessageBox.Show("Der eingegebene Registrierungscode ist ungültig, bitte überprüfen Sie Ihre Daten.");
                return;
            } else {
                MessageBox.Show("Ihr Produkt wurde erfolgreich aktiviert.");
            }

            Info.Serial = serial;
            Info.RK = key;
            
            _keepAppConfig = true;
            new Login().Show();
            this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            Title = CustomResources.CustomStrings.ProductName + " / " + "Registrierung (Version " + VersionInfo.Instance.CurrentVersion + ")";
            registrationMail.NavigateUri = new Uri(AppConfig.GetRegistrationMailHRef());
            txtRegistrationPhone.Text =
                "Alternativ kann die Registrierung auch telefonisch unter der Telefonnummer " + CustomResources.CustomStrings.RegistrationPhone + " erfolgen.";
            registrationMail.Inlines.Clear();
            registrationMail.Inlines.Add(new Run(CustomResources.CustomStrings.RegistrationMail));        }

        private void registrationMail_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e) {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        private void Window_Closed(object sender, System.EventArgs e) {
            if (!_keepAppConfig) AppConfig.Dispose();
        }
    }
}
