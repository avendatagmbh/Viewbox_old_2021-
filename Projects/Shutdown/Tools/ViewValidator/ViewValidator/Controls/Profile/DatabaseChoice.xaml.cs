using System;
using System.Data;
using System.Data.Common;
using System.Windows;
using System.Windows.Controls;
using DbAccess;
using DbAccess.Structures;
using Microsoft.Win32;
using ViewValidator.Models.Profile;

namespace ViewValidator.Controls.Profile {
    /// <summary>
    /// Interaktionslogik für DatabaseChoise.xaml
    /// </summary>
    public partial class DatabaseChoice : UserControl {

        #region Model
        private DatabaseModel Model { get { return this.DataContext as DatabaseModel; } }
        #endregion

        public DatabaseChoice() {
            InitializeComponent();
        }

        private void tbPassword_PasswordChanged(object sender, RoutedEventArgs e) {
            Model.ConfigDest.Password = ((PasswordBox)sender).Password;
        }

        private void cbDatabase_DropDownOpened(object sender, EventArgs e) {
            try {
                DbConfig config = (DbConfig) Model.ConfigDest.Clone();
                config.DbName = "";
                IDatabase db = ConnectionManager.CreateConnection(config);
                db.Open();
                IDataReader reader = db.ExecuteReader("SHOW DATABASES");
                cbDatabase.Items.Clear();
                while (reader.Read()) cbDatabase.Items.Add(reader.GetValue(0).ToString());
                db.Close();
            } catch (Exception ex) {
                MessageBox.Show(ex.Message);
            }
        }

        private void cbDatabase_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (cbDatabase.SelectedItem != null)
                Model.ConfigDest.DbName = cbDatabase.SelectedItem.ToString();
            
        }

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            if (Model != null && Model.ConfigDest != null) tbPassword.Password = Model.ConfigDest.Password;
        }

        private void btnTestViewConnection_Click(object sender, RoutedEventArgs e) {
            //if (string.IsNullOrEmpty(Model.ConfigDest.DbName)) {
            //    MessageBox.Show(UIHelpers.TryFindParent<Window>(this), "Keine Datenbank angegeben.", "", MessageBoxButton.OK, MessageBoxImage.Error);
            //    return;
            //}
            try {
                using (IDatabase conn = ConnectionManager.CreateConnection(Model.ConfigDest)) {
                    conn.Open();
                }
            } catch (Exception ex) {
                MessageBox.Show(UIHelpers.TryFindParent<Window>(this), "Verbindungsfehler:" + Environment.NewLine + ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            MessageBox.Show(UIHelpers.TryFindParent<Window>(this), "Verbindung erfolgreich.", "", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
