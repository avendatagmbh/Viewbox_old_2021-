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
using DbAccess;
using DbAccess.Structures;
using Microsoft.Win32;
using System.IO;
using Utils;
using eBalanceKitBase;

namespace RegKeyGenerator {
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
            cboVersion.ItemsSource = VersionInfo.Instance.DbVersionHistory;
            cboVersion.SelectedItem = VersionInfo.Instance.CurrentDbVersion;
            DataContext = new MainWindowModel();
        }

        private MainWindowModel Model { get { return DataContext as MainWindowModel; } }

        private void btnGenerateKey_Click(object sender, RoutedEventArgs e) {

            try {
                DbConfig dbConfig = new DbConfig("MySQL") { Hostname = "profiledb", Username = "ebkreg", Password = "Eng6glal" };
                using (var conn = ConnectionManager.CreateConnection(dbConfig)) {
                    try {
                        conn.Open();
                    } catch (Exception ex) {
                        MessageBox.Show("Verbindung zur Datenbank nicht möglich: " + ex.Message);
                    }

                    conn.CreateDatabaseIfNotExists("e_balance_kit_registration");
                    conn.ExecuteNonQuery("use e_balance_kit_registration");
                    conn.ExecuteNonQuery(
                    "CREATE TABLE IF NOT EXISTS `regdata` (" +
                        "`id` INTEGER UNSIGNED NOT NULL AUTO_INCREMENT," +
                        "`company` VARCHAR(128) NOT NULL," +
                        "`forename` VARCHAR(64) NOT NULL," +
                        "`surename` VARCHAR(64) NOT NULL," +
                        "`email` VARCHAR(128)," +
                        "`serial` VARCHAR(19) NOT NULL," +
                        "`key` VARCHAR(19) NOT NULL," +
                        "`timestamp` DATETIME NOT NULL," +
                        "PRIMARY KEY (`id`)" +
                    ") ENGINE = MyISAM;");

                    string company = txtCompany.Text;
                    string forename = txtForename.Text;
                    string surename = txtSurename.Text;
                    string serial = txtSerial.Text;
                    string eMail = txtEMail.Text;
                    string version = cboVersion.SelectedItem.ToString();

                    // TODO: check if serial number does exist
                    int n = Convert.ToInt32(conn.ExecuteScalar("SELECT count(*) FROM `serials` WHERE `serial` = '" + serial + "'"));
                    if (n == 0) {
                        MessageBox.Show("Die angegebene Seriennummer existiert nicht.");
                        return;
                    }

                    n = Convert.ToInt32(conn.ExecuteScalar("SELECT count(*) FROM `regdata` WHERE `serial` = '" + serial + "'"));
                    if (n > 0) {
                        MessageBox.Show("Die angegebene Seriennummer wurde bereits registriert.");
                        return;
                    }
                    
                    string key = Utils.StringUtils.CreateKey(company, serial);
                    txtKey.Text = key;

                    conn.ExecuteNonQuery(
                        "INSERT INTO regdata (company, forename, surename, `email`, `version`, `serial`, `key`, `timestamp`) VALUES(" +
                            conn.GetSqlString(company) + "," +
                            conn.GetSqlString(forename) + "," +
                            conn.GetSqlString(surename) + "," +
                            conn.GetSqlString(eMail) + "," +
                            conn.GetSqlString(version) + "," +
                            conn.GetSqlString(serial) + "," +
                            conn.GetSqlString(key) + "," +
                            conn.GetSqlString(System.DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")) +
                        ")");

                    StringBuilder sb = new StringBuilder();
                    sb.Append("Registrierungsinformationen" + Environment.NewLine);
                    sb.Append("----------------------------------------" + Environment.NewLine);
                    sb.Append("Firma:\t\t" + txtCompany.Text + Environment.NewLine);
                    sb.Append("Seriennummer:\t" + txtSerial.Text + Environment.NewLine);
                    sb.Append("Freischaltcode:\t" + txtKey.Text + Environment.NewLine);
                    txtOutput.Text = sb.ToString();                    
                }
            } catch (Exception ex) {
                MessageBox.Show("Fehler: " + ex.Message);
            }

        }

        private void btnGenerateSerialNumber_Click(object sender, RoutedEventArgs e) {
            try {
                DbConfig dbConfig = new DbConfig("MySQL") { Hostname = "profiledb", Username = "ebkreg", Password = "Eng6glal" };
                using (var conn = ConnectionManager.CreateConnection(dbConfig)) {
                    try {
                        conn.Open();
                    } catch (Exception ex) {
                        MessageBox.Show("Verbindung zur Datenbank nicht möglich: " + ex.Message);
                    }

                    conn.CreateDatabaseIfNotExists("e_balance_kit_registration");
                    conn.ExecuteNonQuery("use e_balance_kit_registration");
                    conn.ExecuteNonQuery(
                    "CREATE TABLE IF NOT EXISTS `serials` (" +
                        "`id` INTEGER UNSIGNED NOT NULL AUTO_INCREMENT," +
                        "`serial` VARCHAR(19) NOT NULL," +
                        "`timestamp` DATETIME NOT NULL," +
                        "PRIMARY KEY (`id`)" +
                    ") ENGINE = MyISAM;");

                    int count = int.Parse(txtCount.Text);

                    txtSerialnumber.Text = string.Empty;

                    for (int k = 0; k < count; k++) {
                        string tmpSerial = Guid.NewGuid().ToString();
                        System.Security.Cryptography.MD5 csp = new System.Security.Cryptography.MD5CryptoServiceProvider();
                        System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
                        string hash = Utils.StringUtils.ByteArrayToString(csp.ComputeHash(enc.GetBytes(tmpSerial)));
                        csp.Dispose();

                        string serial = string.Empty;
                        for (int i = 0; i < 32; i += 2) {
                            serial += hash[i];
                            if (i == 6) serial += "-";
                            if (i == 14) serial += "-";
                            if (i == 22) serial += "-";
                        }

                        if (txtSerialnumber.Text.Length > 0) txtSerialnumber.Text += Environment.NewLine + serial;
                        else txtSerialnumber.Text = serial;

                        conn.ExecuteNonQuery(
                        "INSERT INTO serials (`serial`, `timestamp`) VALUES(" +
                            conn.GetSqlString(serial) + "," +
                            conn.GetSqlString(System.DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")) +
                        ")");
                    }
                }
            } catch (Exception ex) {
                MessageBox.Show("Fehler: " + ex.Message);
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e) {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.FileOk += new System.ComponentModel.CancelEventHandler(dlg_FileOk);
            dlg.Filter = "*.txt|*.txt";
            dlg.ShowDialog();
        }

        void dlg_FileOk(object sender, System.ComponentModel.CancelEventArgs e) {
            SaveFileDialog dlg = sender as SaveFileDialog;
            StreamWriter writer = new StreamWriter(dlg.FileName);
            writer.WriteLine("Registrierungsinformationen");
            writer.WriteLine("----------------------------------------");
            writer.WriteLine("Firma:\t\t" + txtCompany.Text);
            writer.WriteLine("Seriennummer:\t" + txtSerial.Text);
            writer.WriteLine("Freischaltcode:\t" + txtKey.Text);
            writer.Close();
        }

        private void BtnRegistrationsClick(object sender, RoutedEventArgs e) { new RegViewer {Owner = this}.ShowDialog(); }

        private void BtnOkClick(object sender, RoutedEventArgs e) { Close(); }
    }
}
