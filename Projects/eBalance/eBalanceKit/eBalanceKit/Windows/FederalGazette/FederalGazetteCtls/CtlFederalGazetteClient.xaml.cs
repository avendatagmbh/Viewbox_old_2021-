using System;
using System.Windows;
using System.Windows.Controls;
using eBalanceKitBase.Windows;
using eBalanceKitBusiness.FederalGazette;
using eBalanceKitBusiness.FederalGazette.Model;
using MessageBox = System.Windows.MessageBox;
using UserControl = System.Windows.Controls.UserControl;

namespace eBalanceKit.Windows.FederalGazette.FederalGazetteCtls {
    /// <summary>
    /// Interaktionslogik für CtlFederalGazetteClient.xaml
    /// </summary>
    public partial class CtlFederalGazetteClient : UserControl {
        public CtlFederalGazetteClient(FederalGazetteMainModel model) {
            InitializeComponent();
            Model = model;
            DataContext = model;
            _progress = new DlgProgress(null);
        }

        private FederalGazetteMainModel Model { get; set; }
        private DlgProgress _progress = null;


        private void btnNewClient_Click(object sender, RoutedEventArgs e) {
            stplClient.Visibility = Visibility.Visible;
            Model.ClientNumber = null;
            Model.CompanyName = null;
            Model.CompanySign = null;
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
                btnSaveClient.Visibility = Visibility.Collapsed;
                btnNewClient.Visibility = Visibility.Visible;
            } else {
                btnSaveClient.Visibility = Visibility.Visible;
                btnNewClient.Visibility = Visibility.Collapsed;
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

        private void btnSaveClient_Click(object sender, RoutedEventArgs e) {
            if (GetLogin()) {
                try {
                    FederalGazetteClientOperations fgOperation;
                    if (Model.ClientNumber == null) {
                        fgOperation = new FederalGazetteClientOperations(Model);
                        fgOperation.CreateClient();
                        MessageBox.Show("Einsenderkonto wurde erfolgreich erstellt.");
                    } else {
                        fgOperation = new FederalGazetteClientOperations(Model);
                        fgOperation.ChangeClient();
                        MessageBox.Show("Die Daten wurde erfolgreich aktualisiert.");
                    }
                } catch (Exception ex) {
                    throw new Exception(ex.Message);
                }
            }
        }

        private void btnGetClientlist_Click(object sender, RoutedEventArgs e) { UpdateClientList(); }

        private void UpdateClientList() {
            var getLogin = new DlgFederalGazetteLogin();
            var result = getLogin.ShowDialog();
            if (result == true) {
                Model.Username = getLogin.UserName;
                Model.Password = getLogin.Password;
                _progress = new DlgProgress(null)
                {ProgressInfo = {Caption = "Clientlist wird abgerufen...", IsIndeterminate = true}};
                _progress.ExecuteModal(Model.GetClientsList);
                getLogin.Close();
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e) {
            var delete = new FederalGazetteClientOperations(Model);
            var client = lstCompanies.SelectedItem as ClientsList;
            if (client != null) {
                var getLogin = new DlgFederalGazetteLogin();
                var result = getLogin.ShowDialog();
                if (result == true) {
                    Model.Username = getLogin.UserName;
                    Model.Password = getLogin.Password;
                    try {
                        delete.DeleteClient(client.ClientId);
                        MessageBox.Show("Client wude gelöscht!");
                        getLogin.Close();
                        UpdateClientList();
                    } catch (Exception ex) {
                        throw new Exception(ex.Message);
                    }
                }
            }
        }

        private void lstCompanies_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            var client = lstCompanies.SelectedItem as ClientsList;
            if (client != null) {
                stplClient.Visibility = Visibility.Visible;
                Model.ClientNumber = client.ClientId;
                Model.CompanyName = client.CompanyName;
                Model.CompanySign = client.CompanySign;
            }
        }

        private void btnRetrieveCompanyId_Click(object sender, RoutedEventArgs e) {
            var getCompanyId = new FederalGazetteClientOperations(Model);
            Model.CompanyList = getCompanyId.GetCompanyListQueryId();
        }
    }
}
