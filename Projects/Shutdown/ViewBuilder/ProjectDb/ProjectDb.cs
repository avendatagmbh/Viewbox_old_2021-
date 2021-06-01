using System;
using System.Collections.Generic;
using System.ComponentModel;
using AV.Log;
using DbAccess;
using DbAccess.Structures;
using ProjectDb.Tables;
using Utils;
using log4net;

namespace ProjectDb
{
    /// <summary>
    ///   This class provides an interface to project databases.
    /// </summary>
    public class ProjectDb : IDisposable, INotifyPropertyChanged
    {
        /// <summary>
        ///   Suffix for project databases.
        /// </summary>
        public const string DbSuffix = "_project";

        #region events

        /// <summary>
        ///   Occurs when a property changed
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        ///   Occurs when an error occured.
        /// </summary>
        public event EventHandler<ErrorEventArgs> Error;

        #endregion events

        #region eventTrigger

        /// <summary>
        ///   Called when an error occured.
        /// </summary>
        private void OnError(string message)
        {
            if (Error != null) Error(this, new ErrorEventArgs(message));
        }

        /// <summary>
        ///   Called when a property changed.
        /// </summary>
        /// <param name="property"> The property. </param>
        private void OnPropertyChanged(string property)
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(property));
        }

        #endregion eventTrigger

        #region eventHandler

        /// <summary>
        ///   Handles the PropertyChanged event of the ConnectionManager control.
        /// </summary>
        /// <param name="sender"> The source of the event. </param>
        /// <param name="e"> The <see cref="System.ComponentModel.PropertyChangedEventArgs" /> instance containing the event data. </param>
        private void ConnectionManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (ConnectionManager != null)
            {
                switch (e.PropertyName)
                {
                    case "ConnectionState":
                        switch (ConnectionManager.ConnectionState)
                        {
                            case ConnectionStates.Online:
                                if (IsInitialized) break;
                                try
                                {
                                    using (IDatabase conn = ConnectionManager.GetConnection())
                                    {
                                        // create neccesary tables
                                        Info.CreateTable(conn);
                                        conn.DbMapping.CreateTableIfNotExists<Viewscript>();
                                        conn.DbMapping.CreateTableIfNotExists<View>();
                                        // detect db-version
                                        DetectDbVersion(conn);
                                        if (DbVersion == "1.0.0")
                                        {
                                            conn.DropTableIfExists("info");
                                            Info.CreateTable(conn);
                                        }
                                        new DbUpgrader().UpgradeDb(prjDbConfig);
                                    }
                                    IsInitialized = true;
                                }
                                catch (Exception ex)
                                {
                                    using (NDC.Push(LogHelper.GetNamespaceContext()))
                                    {
                                        LogHelper.GetLogger().Error("Error while initializing ProjectDb", ex);
                                    }
                                    IsInitialized = false;
                                    OnError("Failed to initialize the project database interface: " + ex.Message);
                                }
                                break;
                        }
                        break;
                }
            }
        }

        #endregion eventHandler

        #region fields

        /// <summary>
        ///   See property ConnectionManager.
        /// </summary>
        private ConnectionManager _connectionManager;

        /// <summary>
        ///   See property DbVersion.
        /// </summary>
        private string _dbVersion;

        /// <summary>
        ///   See property IsInitialized.
        /// </summary>
        private bool _isInitialized;

        #endregion fields

        #region properties

        /// <summary>
        ///   Gets or sets a value indicating whether this instance is initialized.
        /// </summary>
        /// <value> <c>true</c> if this instance is initialized; otherwise, <c>false</c> . </value>
        public bool IsInitialized
        {
            get { return _isInitialized; }
            set
            {
                if (_isInitialized != value)
                {
                    _isInitialized = value;
                    OnPropertyChanged("IsInitialized");
                }
            }
        }

        /// <summary>
        ///   Gets or sets the connection manager.
        /// </summary>
        /// <value> The connection manager. </value>
        public ConnectionManager ConnectionManager
        {
            get { return _connectionManager; }
            set
            {
                if (_connectionManager != value)
                {
                    _connectionManager = value;
                    OnPropertyChanged("ConnectionManager");
                }
            }
        }

        /// <summary>
        ///   Gets or sets the db version.
        /// </summary>
        /// <value> The db version. </value>
        public string DbVersion
        {
            get { return _dbVersion; }
            private set
            {
                if (_dbVersion != value)
                {
                    _dbVersion = value;
                    OnPropertyChanged("DbVersion");
                }
            }
        }

        /// <summary>
        ///   Gets or sets the name of the data db.
        /// </summary>
        /// <value> The name of the data db. </value>
        public string DataDbName { get; private set; }

        #endregion properties

        #region methods

        public string RelationsFilePath
        {
            get
            {
                using (var conn = ConnectionManager.GetConnection())
                {
                    return Info.GetValue(conn, "RelationsCsvPath");
                }
            }
            set
            {
                using (var conn = ConnectionManager.GetConnection())
                {
                    Info.SetValue(conn, "RelationsCsvPath", value);
                }
            }
        }

        /// <summary>
        ///   Disposed this instance.
        /// </summary>
        public void Dispose()
        {
            if (_connectionManager != null)
                _connectionManager.Dispose();
        }

        public void Notify()
        {
            OnPropertyChanged("ConnectionState");
        }

        /// <summary>
        ///   Inits the specified db config.
        /// </summary>
        /// <param name="dbConfig"> The db config. </param>
        public void Init(DbConfig dbConfig)
        {
            try
            {
                // create project database if it does not yet exist
                using (IDatabase conn = ConnectionManager.CreateConnection(dbConfig))
                {
                    conn.Open();
                    conn.CreateDatabaseIfNotExists(dbConfig.DbName + DbSuffix);
                }
            }
            catch (Exception ex)
            {
                OnError("Error when creating the project database: " + ex.Message);
            }
            prjDbConfig = dbConfig.Clone() as DbConfig;
            prjDbConfig.DbName = dbConfig.DbName + DbSuffix;
            DataDbName = dbConfig.DbName;
            ConnectionManager = new ConnectionManager(prjDbConfig, 2);
            ConnectionManager.PropertyChanged += ConnectionManager_PropertyChanged;
            ConnectionManager.Init();
        }

        /// <summary>
        ///   Loads the viewsscripts.
        /// </summary>
        /// <returns> </returns>
        public List<Viewscript> LoadViewscripts()
        {
            List<Viewscript> viewscripts;
            using (IDatabase conn = ConnectionManager.GetConnection())
            {
                viewscripts = Viewscript.Load(conn);
                foreach (Viewscript viewscript in viewscripts)
                {
                    viewscript.ProjectDb = this;
                }
            }
            return viewscripts;
        }

        /// <summary>
        ///   Loads the views.
        /// </summary>
        /// <returns> </returns>
        public List<View> LoadViews()
        {
            List<View> views = new List<View>();
            using (IDatabase conn = ConnectionManager.GetConnection())
            {
                //views = View.Load(conn);
                views = View.LoadLatestRecords(conn);
                foreach (View view in views)
                {
                    view.ProjectDb = this;
                }
            }
            return views;
        }

        /// <summary>
        ///   Loads the views.
        /// </summary>
        /// <returns> </returns>
        public List<View> LoadViewsHistory()
        {
            List<View> views = new List<View>();
            using (IDatabase conn = ConnectionManager.GetConnection())
            {
                views = View.Load(conn);
                foreach (View view in views)
                {
                    view.ProjectDb = this;
                }
            }
            return views;
        }

        /// <summary>
        ///   Gets the current version name.
        /// </summary>
        /// <param name="conn"> The connection. </param>
        /// <returns> </returns>
        private void DetectDbVersion(IDatabase conn)
        {
            if (!conn.GetTableList().Contains(Info.TABLENAME))
            {
                throw new Exception("Missing Table 'info' in project database.");
            }
            else
            {
                DbVersion = Info.GetValue(conn, Info.KEY_VERSION);
            }
        }

        #endregion methods

        private DbConfig prjDbConfig;

        /*****************************************************************************************************/

        /*****************************************************************************************************/

        /*****************************************************************************************************/

        /*****************************************************************************************************/

        /*****************************************************************************************************/

        /*****************************************************************************************************/
    }
}