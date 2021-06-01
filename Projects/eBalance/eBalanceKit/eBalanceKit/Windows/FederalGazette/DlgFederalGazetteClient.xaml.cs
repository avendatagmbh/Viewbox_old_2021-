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
using eBalanceKit.Models;
using eBalanceKitBusiness.FederalGazette;
using eBalanceKitBusiness.Manager;
using eBalanceKitBusiness.Structures.DbMapping;

namespace eBalanceKit.Windows.FederalGazette {
    /// <summary>
    /// Interaktionslogik für DlgFederalGazetteClient.xaml
    /// </summary>
    public partial class DlgFederalGazetteClient : Window {
        public DlgFederalGazetteClient(FederalGazetteModel model) {
            InitializeComponent();
            Model = model;
            DataContext = Model;
        }

        private FederalGazetteModel Model { get; set; }

        private void btnCancel_Click(object sender, RoutedEventArgs e) {
            DialogResult = false; Close(); }

        private void btnCreateClient_Click(object sender, RoutedEventArgs e) {

            if (GetLogin()) {
                try {
                    var fgOperation = new FederalGazetteOperations(Model);
                    fgOperation.CreateClient();
                    MessageBox.Show("Einsenderkonto wurde erfolgreich erstellt.");
                    Close();

                } catch (Exception ex) {

                    throw new Exception(ex.Message);
                }

            }
        }

        private bool GetLogin() {
            var getLogin = new DlgFederalGazetteLogin();
            var result = getLogin.ShowDialog();

            if (result.HasValue && result == true) {
                Model.Username = getLogin.UserName;
                Model.Password = getLogin.Password;
                getLogin.Close();
                return true;
            }
            return false;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            if (string.IsNullOrEmpty(Model.ClientNumber)) {
                btnChangeClient.Visibility = Visibility.Collapsed;
                btnCreateClient.Visibility = Visibility.Visible;
            } else {
                btnChangeClient.Visibility = Visibility.Visible;
                btnCreateClient.Visibility = Visibility.Collapsed;
            }
        }

        private void cmbCountries_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            stplState.Visibility = Model.CompanySelectedCountry.Id.ToLower().Equals("de")
                                       ? Visibility.Visible
                                       : Visibility.Collapsed;
        }

        private void cmbCompanyRefType_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (!Model.SelectedCompanyRegisterationType.Id.ToLower().Equals("german_registered")) {
                stplCompanyName.Visibility = Visibility.Visible;
                stplDomicile.Visibility = Visibility.Visible;
                stplCompanyId.Visibility = Visibility.Collapsed;
                stplLegalForm.Visibility = Visibility.Visible;
                stplCompanyId.Visibility = Visibility.Collapsed;
                stplRegisteredCourt.Visibility = Visibility.Collapsed;
                stplRegisterationType.Visibility = Visibility.Collapsed;
            } else {
                stplCompanyName.Visibility = Visibility.Collapsed;
                stplDomicile.Visibility = Visibility.Collapsed;
                stplLegalForm.Visibility = Visibility.Collapsed;

            }
            if (Model.SelectedCompanyRegisterationType.Id.ToLower().Equals("german_registered")) {
                stplCompanyId.Visibility = Visibility.Visible;
                stplRegisteredCourt.Visibility = Visibility.Visible;
                stplRegisterationType.Visibility = Visibility.Visible;

            }

        }

        private void btnChangeClient_Click(object sender, RoutedEventArgs e)
        {
            if (GetLogin()) {
                try
                {
                    var FgOperation = new FederalGazetteOperations(Model);
                    FgOperation.ChangeClient();
                    MessageBox.Show("Die Daten wurde erfolgreich aktualisiert.");
                    Close();
                }
                catch (Exception ex) {
                    throw new Exception(ex.Message);
                }
            }
        }
    }
}
