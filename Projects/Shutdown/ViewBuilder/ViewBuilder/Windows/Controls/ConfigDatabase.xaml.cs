/**************************************************************************************************************
 * author               date            comment
 * ------------------------------------------------------------------------------------------------------------
 * Mirko Dibbert        2010-10-16      initial implementation
 *************************************************************************************************************/

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
using Utils;
using ViewBuilderBusiness.Structures;
using System.Collections.ObjectModel;
using ViewBuilderBusiness.Structures.Config;
using System.Text.RegularExpressions;
using DbAccess;
using DbAccess.Structures;

namespace ViewBuilder.Windows.Controls {

    /// <summary>
    /// Interaktionslogik für ConfigDatabase.xaml
    /// </summary>
    public partial class ConfigDatabase : UserControl {

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigDatabase"/> class.
        /// </summary>
        public ConfigDatabase() {
            InitializeComponent();

            _databases = new List<string>();
            _VisibleDatabases = new ObservableCollection<string>();
            lstDatabases.ItemsSource = _VisibleDatabases;
            base.DataContextChanged += new DependencyPropertyChangedEventHandler(ConfigDatabase_DataContextChanged);
            txtDatabaseType.ItemsSource = ConnectionManager.DbTypeNames;
            txtDatabaseType.SelectedItem = 0;
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
        /// Last selected database.
        /// </summary>
        private string _lastSelectedDatabase;

        #endregion member varaiables

        /*****************************************************************************************************/

        #region eventHandler

        /// <summary>
        /// Handles the DataContextChanged event of the ConfigDatabase control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        void ConfigDatabase_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            
            _databases.Clear();

            if (e.NewValue is DbConfig) {
                DbConfig cfg = (DbConfig)e.NewValue;
                this.txtPassword.Password = cfg.Password;
                //if (cfg.DbName.Length > 0) {
                //    _databases.Add(cfg.DbName);
                //}
            }

            UpdateVisibleDatabases();
        }

        /// <summary>
        /// Handles the GotFocus event of the textbox control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void textbox_GotFocus(object sender, RoutedEventArgs e) {
            ((TextBox)sender).SelectAll();
        }

        /// <summary>
        /// Handles the GotFocus event of the txtPassword control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void txtPassword_GotFocus(object sender, RoutedEventArgs e) {
            ((PasswordBox)sender).SelectAll();
        }

        /// <summary>
        /// Handles the TextChanged event of the TextBox control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Controls.TextChangedEventArgs"/> instance containing the event data.</param>
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e) {
            UpdateVisibleDatabases();
        }

        /// <summary>
        /// Handles the Click event of the btnRefreshDatabaseList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void btnRefreshDatabaseList_Click(object sender, RoutedEventArgs e) {
            RefreshDatabaseList();
        }

        #endregion eventHandler

        /*****************************************************************************************************/

        #region properties

        public static readonly DependencyProperty FilterProperty =
            DependencyProperty.Register("Filter", typeof(string), typeof(ConfigDatabase), new PropertyMetadata("information_schema|mysql"));

        public static readonly DependencyProperty ShowOnlyOptimizesDbsProperty =
            DependencyProperty.Register("ShowOnlyOptimizesDbs", typeof(bool), typeof(ConfigDatabase), new PropertyMetadata(false));

        /// <summary>
        /// Gets or sets the filter.
        /// </summary>
        /// <value>The filter.</value>
        public string Filter {
            get { return (string)this.GetValue(FilterProperty); }
            set { this.SetValue(FilterProperty, value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [show only optimizes DBS].
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [show only optimizes DBS]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowOnlyOptimizesDbs {
            get { return (bool)this.GetValue(ShowOnlyOptimizesDbsProperty); }
            set { this.SetValue(ShowOnlyOptimizesDbsProperty, value); }
        }

        #endregion properties

        /*****************************************************************************************************/

        #region methods

        /// <summary>
        /// Updates the Visible databases.
        /// </summary>
        private void UpdateVisibleDatabases() {
            _VisibleDatabases.Clear();

            string[] filters = this.Filter.Split('|');
            for (int i = 0; i < filters.Length; i++) {
                filters[i] = filters[i].Trim().Replace("*", ".*");
            }

            List<string> dbLowerCase = new List<string>(); 
            foreach (string database in _databases) {
                dbLowerCase.Add(database.ToLower());
            }

            foreach (string database in _databases) {
                bool passedFilter = true;
                for (int i = 0; i < filters.Length; i++) {
                    if (Regex.IsMatch(database, filters[i])) {
                        passedFilter = false;
                        break;
                    }
                }

                if (!passedFilter) continue;

                if (ShowOnlyOptimizesDbs) {
                    if (!dbLowerCase.Contains(database + "_system")) continue;
                }

                if (database.Contains(txtFilter.Text)) _VisibleDatabases.Add(database);
            }
        }

        /// <summary>
        /// Updates the data bindings.
        /// </summary>
        public void UpdateBindings() {
            txtDatabaseType.GetBindingExpression(ComboBox.SelectedItemProperty).UpdateSource();
            txtHost.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            txtUsername.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            txtPort.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            txtDatabase.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            ((DbConfig)this.DataContext).Password = txtPassword.Password;
        }

        /// <summary>
        /// Creates the database if not exists.
        /// </summary>
        /// <returns></returns>
        public bool CreateDatabaseIfNotExists() {
            try {
                DbConfig cfg = CreateDbConfig();
                if (cfg == null) return false;
                using (IDatabase db = ConnectionManager.CreateConnection(cfg)) {
                    db.Open();
                    db.CreateDatabaseIfNotExists(txtDatabase.Text.Trim());
                }

            } catch (Exception ex) {
                MessageBox.Show(UIHelpers.TryFindParent<Window>(this),
                    "Verbindung zum Server nicht möglich:" + Environment.NewLine + ex.Message,
                    "Fehler",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                return false;
            }

            return true;
        }

        /// <summary>
        /// Checks if the databases exists.
        /// </summary>
        /// <returns></returns>
        public bool DatabaseExists() {
            try {
                DbConfig cfg = CreateDbConfig();
                if (cfg == null) return false;
                using (IDatabase db = ConnectionManager.CreateConnection(cfg)) {
                    db.Open();
                    return db.GetSchemaList().Contains(txtDatabase.Text.Trim());
                }

            } catch (Exception ex) {
                MessageBox.Show(UIHelpers.TryFindParent<Window>(this),
                    "Verbindung zum Server nicht möglich:" + Environment.NewLine + ex.Message,
                    "Fehler",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                return false;
            }
        }
        
        /// <summary>
        /// Tests the connection.
        /// </summary>
        /// <returns></returns>
        public bool TestConnection() {
            try {
                DbConfig cfg = CreateDbConfig();
                if (cfg == null) return false;
                using (IDatabase db = ConnectionManager.CreateConnection(cfg)) {
                    db.Open();
                }

            } catch (Exception ex) {
                MessageBox.Show(UIHelpers.TryFindParent<Window>(this),
                    "Verbindung zum Server nicht möglich:" + Environment.NewLine + ex.Message,
                    "Fehler",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                return false;
            }

            return true;
        }

        /// <summary>
        /// Creates the db config.
        /// </summary>
        /// <returns></returns>
        private DbConfig CreateDbConfig() {
            int port;
            if (!int.TryParse(txtPort.Text, out port)) {
                MessageBox.Show(UIHelpers.TryFindParent<Window>(this),
                    "Fehlerhafter Wert für 'Port'!",
                    "Fehler",
                    MessageBoxButton.OK,
                    MessageBoxImage.Exclamation);

                return null;
            }

            //var dbConfig = new DbConfig("MySQL");
            var dbConfig = new DbConfig(txtDatabaseType.SelectedItem.ToString());
            dbConfig.Hostname = txtHost.Text;
            dbConfig.Username = txtUsername.Text;
            dbConfig.Password = txtPassword.Password;
            dbConfig.Port = port;

            return dbConfig;

        }

        /// <summary>
        /// Refreshes the database list.
        /// </summary>
        private void RefreshDatabaseList() {
            _lastSelectedDatabase = txtDatabase.Text;

            try {
                DbConfig cfg = CreateDbConfig();
                if (cfg == null) return;
                using (IDatabase db = ConnectionManager.CreateConnection(cfg)) {
                    db.Open();

                    _databases = db.GetSchemaList();
                    UpdateVisibleDatabases();

                    db.Close();
                }

            } catch (Exception ex) {
                MessageBox.Show(UIHelpers.TryFindParent<Window>(this),
                    "Verbindung zum Server nicht möglich:" + Environment.NewLine + ex.Message,
                    "Fehler",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }

            txtDatabase.Text = _lastSelectedDatabase;
        }
        #endregion methods
    }
}
