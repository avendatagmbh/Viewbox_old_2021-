using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using EricWrapper;
using Microsoft.Win32;
using eBalanceKitBase.Windows;
using eBalanceKitBusiness;
using eBalanceKitBusiness.Structures;
using eBalanceKitBusiness.Structures.DbMapping;
using eBalanceKitResources.Localisation;

namespace eBalanceKit.Windows {
    /// <summary>
    /// Interaktionslogik für DlgSendData.xaml
    /// </summary>
    public partial class DlgSendData : Window {

        public DlgSendData(Document document) {
            InitializeComponent();
            Document = document;
        }

        private Document Document { get; set; }
        private DlgProgress Progress { get; set; }

        private void btnCancel_Click(object sender, RoutedEventArgs e) {
            Close();
        }

        private void btnSend_Click(object sender, RoutedEventArgs e) {
            SendData(false);
        }

        private void btnTestConnection_Click(object sender, RoutedEventArgs e) {
            SendData(true);
        }

        private void SendData(bool isTest) {
            MessageBox.Show(Owner,
                            "Es kann momentan kein Testversand durch den Elster-Rich-Client (ERIC) von der Finanzverwaltung durchgeführt werden, da die aktuelle Taxonomie 5.1 nicht unterstützt wird. Im November 2012 wird es eine neue ERIC Version geben, mit der die Taxonomie 5.1 validiert und gesendet werden kann, diese wird dann umgehend in das eBilanz-Kit integriert.",
                            "Leider aktuell nicht möglich", MessageBoxButton.OK, MessageBoxImage.Information);
            return;

            string certFile = txtCert.Text.Trim();
            if (string.IsNullOrEmpty(certFile)) {
                MessageBox.Show(
                    "Es wurde noch keine Zertifikatsdatei ausgewählt.",
                    ResourcesCommon.Error, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!File.Exists(certFile)) {
                MessageBox.Show(
                    "Die angegebene Zertifikatsdatei existiert nicht.",
                    ResourcesCommon.Error, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (isTest || MessageBox.Show(
                "Soll der aktuell ausgewählte Report (" + Document.Company.Name + " / Geschäftsjahr " +
                Document.FinancialYear.FYear + " / " + Document.Name + ") gesendet werden?",
                "Daten senden?", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) ==
                MessageBoxResult.Yes) {
                var dlg = new DlgEnterPin1 {Owner = this};
                bool? result = dlg.ShowDialog();
                if (result.HasValue && result.Value) {
                    Progress = new DlgProgress(Owner);
                    Progress.ProgressInfo.Caption = "Daten werden übertragen" + (isTest ? " (Testübertragung)" : "") +
                                                    "...";
                    Progress.ProgressInfo.IsIndeterminate = true;

                    Hide();

                    var ericController = new EricController(Progress.ProgressInfo, Document, dlg.txtPin.Password, txtCert.Text, isTest);
                    ericController.Error += (sender, args) => Dispatcher.Invoke(
                        DispatcherPriority.Background,
                        new voidDelegate(() => MessageBox.Show(args.Message, args.Caption, MessageBoxButton.OK, MessageBoxImage.Error)));

                    ericController.Info += (sender, args) => Dispatcher.Invoke(
                        DispatcherPriority.Background,
                        new voidDelegate(() => MessageBox.Show(args.Message, args.Caption, MessageBoxButton.OK, MessageBoxImage.Information)));

                    Progress.ExecuteModal(ericController.Start);

                    if (isTest) Show();
                }
            }
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Escape) {
                e.Handled = true;
                Close();
            }
        }

        #region btnSelectCertificate_Click
        private void btnSelectCertificate_Click(object sender, RoutedEventArgs e) {
            var dlg = new OpenFileDialog();
            dlg.FileOk += dlg_FileOk;
            dlg.Filter = "Zertifikatdateien (*.pfx)|*.pfx";
            dlg.ShowDialog();
        }
        #endregion

        #region dlg_FileOk
        private void dlg_FileOk(object sender, CancelEventArgs e) {
            txtCert.Text = ((OpenFileDialog) sender).FileName;
        }
        #endregion

        #region btnProxy_Click
        private void btnProxy_Click(object sender, RoutedEventArgs e) {
            var dlg = new DlgProxy(AppConfig.ProxyConfig) {Owner = this};
            dlg.DataContext = AppConfig.ProxyConfig;
            bool? result = dlg.ShowDialog();
            if (result.HasValue && result.Value) {
                AppConfig.ProxyConfig.Password = dlg.txtPassword.Password;
                Eric.SetProxy(AppConfig.ProxyConfig.Host, AppConfig.ProxyConfig.Port, AppConfig.ProxyConfig.Username,
                              AppConfig.ProxyConfig.Password);
                AppConfig.Save();
            }
        }
        #endregion

        #region Nested type: voidDelegate
        private delegate void voidDelegate();
        #endregion

        private void btnInfo_Click(object sender, RoutedEventArgs e) { infoPopup.IsOpen = true; }

        private void infoPopup_PreviewKeyUp(object sender, KeyEventArgs e) {
            if (e.Key == Key.Escape) {
                infoPopup.IsOpen = false;
                e.Handled = true;
            }
        }
    }
}