using System;
using System.Data.Common;
using System.Windows;
using System.Windows.Controls;
using DbAccess;
using DbAccess.Structures;
using Utils;
using ViewboxAdmin.Models.ProfileRelated;
using ViewboxAdmin.ViewModels;


// I did not have time for refactor and unit test this class

namespace ViewboxAdmin.Controls.ProfileRelated {
    /// <summary>
    /// Interaktionslogik für CtlMysqlDatabaseChoice.xaml
    /// </summary>
    public partial class CtlMysqlDatabaseChoice : UserControl {
        public CtlMysqlDatabaseChoice() {
            InitializeComponent();
        }


        IDbConfig DbConfig { get { return Model.DbConfig; } }
        //DatabaseModel Model{ get { return DataContext as DatabaseModel; } }

        private void cbDatabase_DropDownOpened(object sender, EventArgs e) {
            try {
                //if (Model.IsReadOnly) return;
                DbConfig config = (DbConfig)((ICloneable)DbConfig).Clone();
                config.DbName = "";
                IDatabase db = ConnectionManager.CreateConnection(config);
                db.Open();
                DbDataReader reader = (DbDataReader)db.ExecuteReader("SHOW DATABASES");
                cbDatabase.Items.Clear();
                while (reader.Read()) cbDatabase.Items.Add(reader.GetValue(0).ToString());
                db.Close();
            } catch (Exception ex) {
                MessageBox.Show(ex.Message);
            }
        }

        private void cbDatabase_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            //if (Model.IsReadOnly) return;
            if (cbDatabase.SelectedItem != null)
                DbConfig.DbName = cbDatabase.SelectedItem.ToString();

        }

        private void btnTestViewConnection_Click(object sender, RoutedEventArgs e) {
            try {
                DbConfig tempConfig = DbConfig as DbConfig;
                tempConfig.Password = tbPassword.Password;
                using (IDatabase conn = ConnectionManager.CreateConnection(tempConfig as DbConfig)) {
                    conn.Open();
                }
            } catch (Exception ex) {
                MessageBox.Show(UIHelpers.TryFindParent<Window>(this), "Error:" + Environment.NewLine + ex.Message+" Connectionstring: "+DbConfig.ConnectionString, "", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            MessageBox.Show(UIHelpers.TryFindParent<Window>(this), "Successfully connected:", "", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private CreateNewProfile_ViewModel Model { get; set; }

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) { Model = (DataContext as CreateNewProfile_ViewModel); }
    }
}
