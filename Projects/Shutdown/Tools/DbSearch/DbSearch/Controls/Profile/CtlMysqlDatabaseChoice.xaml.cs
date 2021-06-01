using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
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
using DbSearch.Models.Profile;
using Utils;

namespace DbSearch.Controls.Profile {
    /// <summary>
    /// Interaktionslogik für CtlMysqlDatabaseChoice.xaml
    /// </summary>
    public partial class CtlMysqlDatabaseChoice : UserControl {
        public CtlMysqlDatabaseChoice() {
            InitializeComponent();
        }

        
        DbConfig DbConfig { get { return (DataContext as DatabaseModel).DbConfig; } }
        DatabaseModel Model{ get { return DataContext as DatabaseModel; } }

        private void cbDatabase_DropDownOpened(object sender, EventArgs e) {
            try {
                if (Model.IsReadOnly) return;
                DbConfig config = (DbConfig)DbConfig.Clone();               
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
            if (Model.IsReadOnly) return;
            if (cbDatabase.SelectedItem != null)
                DbConfig.DbName = cbDatabase.SelectedItem.ToString();

        }

        private void btnTestViewConnection_Click(object sender, RoutedEventArgs e) {
            try {
                using (IDatabase conn = ConnectionManager.CreateConnection(DbConfig)) {
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
