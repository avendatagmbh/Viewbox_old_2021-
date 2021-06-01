/**************************************************************************************************************
 * author               date            comment
 * ------------------------------------------------------------------------------------------------------------
 * mid                  2010-11-21      initial implementation
 *************************************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TransDATABusiness.ConfigDb.Tables;
using System.ComponentModel;
using TransDATABusiness.EventArgs;
using DbAccess;
using DbAccess.Structures;

namespace TransDATABusiness.ConfigDb {

    /// <summary>
    /// This class provides an interface to the configuration database.
    /// </summary>
    public class ConfigDb : INotifyPropertyChanged {

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigDb"/> class.
        /// </summary>
        internal ConfigDb() {           
        }

        /*****************************************************************************************************/

        #region events

        /// <summary>
        /// Occurs when an error occured.
        /// </summary>
        public event EventHandler<MessageEventArgs> Error;

        /// <summary>
        /// Occurs when a property changed
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion events

        /*****************************************************************************************************/

        #region eventTrigger

        /// <summary>
        /// Called when an error occured.
        /// </summary>
        private void OnError(string message) {
            if (Error != null) Error(this, new MessageEventArgs(message));
        }

        /// <summary>
        /// Called when a property changed.
        /// </summary>
        /// <param name="property">The property.</param>
        private void OnPropertyChanged(string property) {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(property));
        }

        #endregion eventTrigger

        /*****************************************************************************************************/

        #region eventHandler

        /// <summary>
        /// Handles the PropertyChanged event of the ConnectionManager control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.ComponentModel.PropertyChangedEventArgs"/> instance containing the event data.</param>
        void ConnectionManager_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case "ConnectionState":
                    switch (this.ConnectionManager.ConnectionState) {
                        case ConnectionStates.Online:

                            if (this.IsInitialized) break;

                            try {
                                using (IDatabase conn = this.ConnectionManager.GetConnection()) {

                                    // create database tables, if they does not yet exists             
                                    Info.CreateTable(conn);                                    
                                    conn.DbMapping.CreateTableIfNotExists<Profile>();
                                    conn.DbMapping.CreateTableIfNotExists<Table>();
                                    conn.DbMapping.CreateTableIfNotExists<TableInfo>();
                                    conn.DbMapping.CreateTableIfNotExists<Column>();
                                    conn.DbMapping.CreateTableIfNotExists<ColumnInfo>();

                                    // detect db-version
                                    DetectDbVersion(conn);
                                }

                                this.IsInitialized = true;

                            } catch (Exception ex) {
                                this.IsInitialized = false;
                                OnError("Failed to initialize the config database interface: " + ex.Message);
                            }
                            break;
                    }
                    break;
            }
        }

        #endregion eventHandler

        /*****************************************************************************************************/
        
        #region fields

        /// <summary>
        /// See property IsInitialized.
        /// </summary>
        private bool _isInitialized;

        /// <summary>
        /// See property ConnectionManager.
        /// </summary>
        private ConnectionManager _connectionManager;

        /// <summary>
        /// See property DbVersion.
        /// </summary>
        private string _dbVersion;

        #endregion fields

        /*****************************************************************************************************/

        #region properties

        /// <summary>
        /// Gets or sets a value indicating whether this instance is initialized.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is initialized; otherwise, <c>false</c>.
        /// </value>
        public bool IsInitialized {
            get { return _isInitialized; }
            set {
                if (_isInitialized != value) {
                    _isInitialized = value;
                    OnPropertyChanged("IsInitialized");
                }
            }
        }

        /// <summary>
        /// Gets or sets the connection manager.
        /// </summary>
        /// <value>The connection manager.</value>
        public ConnectionManager ConnectionManager {
            get { return _connectionManager; }
            set {
                if (_connectionManager != value) {
                    _connectionManager = value;
                    OnPropertyChanged("ConnectionManager");
                }
            }
        }

        /// <summary>
        /// Gets or sets the db version.
        /// </summary>
        /// <value>The db version.</value>
        public string DbVersion {
            get { return _dbVersion; }
            private set {
                if (_dbVersion != value) {
                    _dbVersion = value;
                    OnPropertyChanged("DbVersion");
                }
            }
        }

        #endregion properties

        /*****************************************************************************************************/

        #region methods

        /// <summary>
        /// Gets the current version name.
        /// </summary>
        /// <param name="conn">The connection.</param>
        /// <returns></returns>
        private void DetectDbVersion(IDatabase conn) {
            if (!conn.GetTableList().Contains(Info.TABLENAME)) {
                throw new Exception("Missing Table 'info' in config database.");
            } else {
                this.DbVersion = Info.GetValue(conn, Info.KEY_VERSION);
            }
        }

        /// <summary>
        /// Inits this instance.
        /// </summary>
        /// <param name="dbPath">The db path.</param>
        internal void Init(string dbPath) {
            DbConfig dbConfig = new DbConfig(ConnectionManager.GetDbName(typeof(DbAccess.DbSpecific.SQLite.Database))) { Hostname = dbPath + "\\config.db3" };

            this.ConnectionManager = new ConnectionManager(dbConfig, 12);
            this.ConnectionManager.PropertyChanged += new PropertyChangedEventHandler(ConnectionManager_PropertyChanged);
            this.ConnectionManager.Init();
            //Info.CreateTable(this.ConnectionManager.GetConnection());
        }

        /// <summary>
        /// Disposed this instance.
        /// </summary>
        public void Dispose() {
            _connectionManager.Dispose();
        }

        /// <summary>
        /// Checks the password.
        /// </summary>
        /// <param name="password">The password.</param>
        /// <returns></returns>
        internal bool CheckPassword(string password) {
            using (IDatabase conn = this.ConnectionManager.GetConnection()) {
                return Info.CheckPassword(conn, password);
            }
        }

        #endregion methods

    }
}
