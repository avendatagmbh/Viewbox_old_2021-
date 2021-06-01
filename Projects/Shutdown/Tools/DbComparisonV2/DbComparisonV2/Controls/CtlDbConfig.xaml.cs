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
using System.Text.RegularExpressions;
using Utils;
using DbComparisonV2.Windows;

namespace DbComparisonV2.Models
{
    /// <summary>
    /// Interaktionslogik für CtlDbConfig.xaml
    /// </summary>
    public partial class CtlDbConfig : UserControl
    {
        

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseConfig"/> class.
        /// </summary>
        public CtlDbConfig()
        {
            InitializeComponent();

            _databases = new List<string>();
            _VisibleDatabases = new ObservableCollection<string>();
            //cboDbName.ItemsSource = _VisibleDatabases;
            base.DataContextChanged += new DependencyPropertyChangedEventHandler(ConfigDatabase_DataContextChanged);
            cboDbType.ItemsSource = ConnectionManager.DbTypeNames;
            cboDbType.SelectedItem = 0;
        }

        /*****************************************************************************************************/

        #region fields

        private DbConfig _config;
        
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
        /// Handles the DataContextChanged event of the DatabaseConfig control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        void ConfigDatabase_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            
            _databases.Clear();

            if (e.NewValue is DbConfig) {
                DbConfig cfg = (DbConfig)e.NewValue;
                this.txtPassword.Password = cfg.Password;
                cboDbName.SelectedValue = cfg.DbName;
                //if (cfg.DbName.Length > 0) {
                //    _databases.Add(cfg.DbName);
                //}
            }

            //UpdateVisibleDatabases();
        }

        #endregion eventHandler

        /*****************************************************************************************************/

        #region properties

        public static readonly DependencyProperty FilterProperty =
            DependencyProperty.Register("Filter", typeof(string), typeof(CtlDbConfig), new PropertyMetadata("information_schema|mysql"));

        public static readonly DependencyProperty ShowOnlyOptimizesDbsProperty =
            DependencyProperty.Register("ShowOnlyOptimizesDbs", typeof(bool), typeof(CtlDbConfig), new PropertyMetadata(false));

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

                //if (database.Contains(txtFilter.Text)) _VisibleDatabases.Add(database);
            }
        }

        /// <summary>
        /// Creates the database if not exists.
        /// </summary>
        /// <returns></returns>
        public bool CreateDatabaseIfNotExists() {
            try {
                DbConfig cfg = CreateDbConfig();
                if (cfg == null) return false;
                using (IDatabase db = ConnectionManager.CreateDbFromConfig(cfg)) {
                    db.Open();
                    db.CreateDatabaseIfNotExists(cboDbName.Text.Trim());
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
                using (IDatabase db = ConnectionManager.CreateDbFromConfig(cfg)) {
                    db.Open();
                    return db.GetSchemaList().Contains(cboDbName.Text.Trim());
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
                using (IDatabase db = ConnectionManager.CreateDbFromConfig(cfg)) {
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
            var dbConfig = new DbConfig(cboDbType.SelectedItem.ToString());
            dbConfig.Hostname = txtHost.Text;
            dbConfig.Username = Username.Text;
            dbConfig.Password = txtPassword.Password;
            dbConfig.Port = port;

            return dbConfig;

        }
        #endregion methods
        public void ClearDatabaseList() 
        {
            this.cboDbName.Items.Clear();
            this.cboDbName.ItemsSource = null;
        }

        public void UpdateBindings()
        {
            if(cboDbName.SelectedValue != null)
                ((DbConfig)this.DataContext).DbName = cboDbName.SelectedValue as string;
    
            if(!String.IsNullOrEmpty(txtPassword.Password))
                ((DbConfig)this.DataContext).Password = txtPassword.Password;
        }
        public void RefreshDatabaseList()
        {
            UpdateBindings();
            ClearDatabaseList();
            // Create a test connection
            //this.UpdateConfig();
            _config = ((DatabaseConfig)this.DataContext);

            string backup = _config.DbName;
            _config.DbName = "";
            BackgroundDbAccess testConnection = new BackgroundDbAccess(_config);

            // Read the given schemes
            testConnection.ReadSchemas();
            testConnection.ShowDialog();

            // Add the schemes to the drop down
            if (testConnection.DialogResult.HasValue && testConnection.DialogResult.Value)
            {

                foreach (string sSchema in testConnection.Schemas)
                {
                    this.cboDbName.Items.Add(sSchema);
                }

            }
            _config.DbName = backup;
        
        }
        private void btnUpdateDatabaseList_Click(object sender, RoutedEventArgs e)
        {
            RefreshDatabaseList();
        }

    }
}
