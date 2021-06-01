using System;
using System.Windows;
using System.Windows.Input;
using eBalanceKitConfig.Models;
using eBalanceKitBase.Windows;
using System.Globalization;
using System.Threading;
using eBalanceKitBase;

namespace eBalanceKitConfig {
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
            try { this.Model = new ConfigModel(this); } catch (Exception ex) { MessageBox.Show(ex.Message); Close(); return; }

            this.DataContext = this.Model;

            this.Model.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(Model_PropertyChanged);
            System.Threading.Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("de-DE");
            System.Threading.Thread.CurrentThread.CurrentUICulture = CultureInfo.CreateSpecificCulture("de-DE");
        }

        void Model_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            if (e.PropertyName == "Password") {
                ctlDbConfig_MySQL.InitPassword();
                ctlDbConfig_SQLServer.InitPassword();
                ctlDbConfig_OracleServer.InitPassword();
            }
        }


        private ConfigModel Model { get; set; }

        private void btnOk_Click(object sender, RoutedEventArgs e) {
            if (SaveConfig()) this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }


        private void Window_PreviewKeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Escape) {
                e.Handled = true;
                this.Close();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            try {
                this.Model.LoadConfig();
            } catch (Exception ex) {
                MessageBox.Show(ex.Message);
            }
            
        }

        private void optSQLServer_Unchecked(object sender, RoutedEventArgs e) {
            ctlDbConfig_SQLServer.UpdatePassword();
        }

        private void optMySQL_Unchecked(object sender, RoutedEventArgs e) {
            ctlDbConfig_MySQL.UpdatePassword();
        }
        private void optOracle_Unchecked(object sender, RoutedEventArgs e) {
            ctlDbConfig_OracleServer.UpdatePassword();
        }

        private void optSQLite_Unchecked(object sender, RoutedEventArgs e) {
            // nothing to do
        }

        private void btnTestConnection_Click(object sender, RoutedEventArgs e) {
            if (Model.UseMySQLEngine) {
                ctlDbConfig_MySQL.UpdatePassword();
            } else if (Model.UseSQLServerEngine) {
                ctlDbConfig_SQLServer.UpdatePassword();
            } else if (Model.UseOracleServerEngine) {
                ctlDbConfig_OracleServer.UpdatePassword();
            }


            try {
                this.Model.TestConnection();
                MessageBox.Show("Verbindungstest erfolgreich.", "Verbindungtest", MessageBoxButton.OK, MessageBoxImage.Information);
            } catch (Exception ex) {
                MessageBox.Show("Verbindung nicht möglich: " + Environment.NewLine + ex.Message, "Verbindungstest", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e) {
            if (SaveConfig()) {
                MessageBox.Show("Die Konfiguration wurde gespeichert.");
            }
        }

        private bool SaveConfig() {
            try {

                if (Model.UseMySQLEngine) {
                    ctlDbConfig_MySQL.UpdatePassword();
                } else if (Model.UseSQLServerEngine) {
                    ctlDbConfig_SQLServer.UpdatePassword();
                } else if (Model.UseSQLServerEngine) {
                    ctlDbConfig_OracleServer.UpdatePassword();
                }

                this.Model.ProxyConfig.Password = ctlProxy.txtPassword.Password;

                this.Model.SaveConfig();
                return true;
            
            } catch (Exception ex) {
                MessageBox.Show("Konfiguration konnte nicht gespeichert werden: " + ex.Message);
                return false;
            }

        }
    }
}
