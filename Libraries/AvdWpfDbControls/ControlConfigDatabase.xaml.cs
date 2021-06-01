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
using System.Collections.ObjectModel;
using DbAccess;
using DbAccess.Structures;

namespace AvdWpfDbControls {
    /// <summary>
    /// Interaktionslogik für ControlConfigDatabase.xaml
    /// </summary>
    public partial class ControlConfigDatabase : UserControl {
        public ControlConfigDatabase() {
            InitializeComponent();

            _databases = new List<string>();
            _VisibleDatabases = new ObservableCollection<string>();
            lstDatabases.ItemsSource = _VisibleDatabases;
            txtDatabaseType.ItemsSource = ConnectionManager.DbTypeNames;
            txtDatabaseType.SelectedItem = 0;
            _dsn = new ObservableCollection<string>();
            txtDSN.ItemsSource = this.DSNListInit();
        }


        public void Init(DbConfig configDatabase) {
            this.DataContext = configDatabase;
            txtPassword.Password = configDatabase.Password;
        }

        /*****************************************************************************************************/
        #region fields

        /// <summary>
        /// List of all databases.
        /// </summary>
        private List<string> _databases;

        /// <summary>
        /// List of visible databases.
        /// </summary>
        private ObservableCollection<string> _VisibleDatabases;
        
        /// <summary>
        /// List of all dsn.
        /// </summary>
        private ObservableCollection<string> _dsn;

        #endregion member varaiables

        /*****************************************************************************************************/
        #region properties


        #endregion properties

      
        /*******************************************************************************************************/
        #region eventHandler

        private void textbox_GotFocus(object sender, RoutedEventArgs e) {
            ((TextBox)sender).SelectAll();
        }

        private void txtPassword_GotFocus(object sender, RoutedEventArgs e) {
            ((PasswordBox)sender).SelectAll();
        }

        private void btnRefreshDatabaseList_Click(object sender, RoutedEventArgs e) {
            UpdateBindings();
            RefreshDatabaseList();
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e) {
            if (lstDatabases == null) return;
            char[] _ch = txtSearch.Text.ToCharArray();
            GetSearch(_ch, _ch.Length);
        }

        private void lstDatabases_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            txtDatabase.Text = (string)lstDatabases.SelectedItem;
            txtDatabase.GetBindingExpression(TextBox.TextProperty).UpdateSource();
        } 
        
         #endregion eventHandler

        /*******************************************************************************************************/
        #region methods

        /// <summary>
        /// Refreshes the database list.
        /// </summary>
        private void RefreshDatabaseList() {
            ((DbConfig)this.DataContext).Password = txtPassword.Password;
            try {
                using (IDatabase db = ConnectionManager.CreateConnection((DbConfig)this.DataContext)) {
                    db.Open();

                    _databases = db.GetSchemaList();
                    foreach (string database in _databases) {
                        _VisibleDatabases.Add(database);
                    }
                }

            } catch (Exception ex) {
                MessageBox.Show(
                    "Verbindung zum Server nicht möglich:" + Environment.NewLine + ex.Message,
                    "Fehler",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Gets the search.
        /// </summary>
        /// <param name="ch">The ch.</param>
        /// <param name="ch_length">The ch_length.</param>
        private void GetSearch(char[] ch, int ch_length) {
            if (ch_length == 0) {
                lstDatabases.ItemsSource = this._VisibleDatabases;
                return;
            }
            if (ch_length == 0 || (ch_length != 0 && ch_length == ch.Length)) {
                lstDatabases.ItemsSource = this._VisibleDatabases;
            }

            ObservableCollection<string> _search = new ObservableCollection<string>();

            for (int i = 0; i < this._VisibleDatabases.Count; i++) {
                _search.Add(this._VisibleDatabases.ElementAt(i));
            }

            int j = 0;
            while (j < _search.Count) {
                if (_search.ElementAt(j)[ch_length - ch.Length] != ch[0]) {
                    _search.RemoveAt(j);
                } else {
                    j++;
                }
            }

            if ((ch.Length - 1) == 0) {
                lstDatabases.ItemsSource = _search;
                return;
            }

            char[] chars = new char[ch.Length - 1];
            for (int i = 0; i < ch.Length - 1; i++) {
                chars[i] = ch[i + 1];
            }
            lstDatabases.ItemsSource = _search;
            this.GetSearch(chars, ch_length);
        }

        public bool ValidateInput() {
            return !Validation.GetHasError(txtDatabaseType) &&
                    !Validation.GetHasError(txtHost) &&
                    !Validation.GetHasError(txtPassword) &&
                    !Validation.GetHasError(txtUsername);
        }


        /// <summary>
        /// Tests the connection.
        /// </summary>
        /// <returns></returns>
        public bool TestConnection() {
            try {
                using (IDatabase db = ConnectionManager.CreateConnection((DbConfig)this.DataContext)) {
                    db.Open();;
                }

            } catch (Exception ex) {
                MessageBox.Show(
                    "Verbindung zum Server nicht möglich:" + Environment.NewLine + ex.Message,
                    "Fehler",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                txtDatabase.Focus();
                return false;
            }

            return true;
        }

        public bool LoginValidate() {
            ((DbConfig)this.DataContext).Password = txtPassword.Password;
            if (ValidateInput() && TestConnection() && txtDatabase.Text != String.Empty) {                
                return true;
            } else {
                MessageBox.Show(
                    "Datenbank wählen.",
                    "Fehler",
                    MessageBoxButton.OK,
                    MessageBoxImage.Exclamation);

                return false;
            }
        }

        /// <summary>
        /// Initialization method
        /// </summary>
        private ObservableCollection<string> DSNListInit() {

            // Add all avaliable DSNs

            this._dsn.Add(string.Empty);

            foreach (string sSystemDsn in ConnectionManager.GetSystemDsnList()) {
                this._dsn.Add(sSystemDsn);
            }

            foreach (string sUserDsn in ConnectionManager.GetUserDsnList()) {
                this._dsn.Add(sUserDsn);
            }

            return this._dsn;
        }

        public void UpdateBindings() {
            txtHost.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            txtUsername.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            txtPort.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            txtDatabase.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            txtDatabaseType.GetBindingExpression(ComboBox.SelectedItemProperty).UpdateSource();
            txtDSN.GetBindingExpression(ComboBox.SelectedItemProperty).UpdateSource();
            ((DbConfig)this.DataContext).Password = txtPassword.Password;
        }

        #endregion methods
    }
}
