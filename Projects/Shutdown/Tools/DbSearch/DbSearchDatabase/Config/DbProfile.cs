// --------------------------------------------------------------------------------
// author: Benjamin Held
// since: 2011-09-08
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Windows.Threading;
using DbAccess;
using DbAccess.Structures;
using DbSearchBase.Interfaces;
using DbSearchDatabase.Interfaces;
using DbSearchDatabase.Results;
using DbSearchDatabase.Structures;
using DbSearchDatabase.TableRelated;
using DbSearchDatabase.Upgrader;

namespace DbSearchDatabase.Config {
    [DbTable("profiles", ForceInnoDb = true)]
    public class DbProfile : DatabaseObjectBase<int>, IDbProfile {
        #region Constructor
        
        public DbProfile() {
            DbCommonConfig = new DbCommonConfig(this);
        }

        #endregion

        #region Properties

        #region Name
        private string _name;
        [DbColumn("name", AllowDbNull = false)]
        public string Name {
            get { return _name; }
            set {
                if (_name != value) {
                    _name = value;
                    Save();
                }
            }
        }
        #endregion

        #region Description
        private string _description;
        [DbColumn("description", AllowDbNull = true)]
        public string Description {
            get { return _description; }
            set {
                if (_description != value) {
                    _description = value;
                    Save();
                }
            }
        }

        [DbColumn("created_by")]
        public int CreatedBy { get; set; }

        [DbColumn("dbconfig", Length=4096)]
        public string DbConfigString { get; set; }

        //Saves the xml represnetation of the custom rules
        [DbColumn("custom_rules", Length = 100000)]
        public string CustomRules { get; set; }
        #endregion

        public DbCommonConfig DbCommonConfig { get; set; }

        //public DbConfig DbConfigValidation { get { return DbCommonConfig.DbConfigValidation; } }

        public DbConfig DbConfigView { get { return DbCommonConfig.DbConfigView; } }
        internal Info Info { get; set; }
        public bool IsLoaded { get; set; }
        public bool DatabaseTooOld { get; private set; }
        public bool DatabaseTooNew { get; private set; }

        private bool _firstCall = true;
        #endregion

        #region Methods

        public DbConfig GetProfileConfig() {
            DbConfig configDatabase = (DbConfig)DbConfigView.Clone();
            configDatabase.DbName = DbConfigView.DbName + "_dbsearchconfig";
            return configDatabase;
        }

        #region GetOpenConnection
        internal IDatabase GetOpenConnection() {
            //IDatabase conn = ConnectionManager.CreateConnection(new DbConfig("SQLite") { Hostname = DbFile, DbName = "Main", Password = DbPwd });
            DbConfig configDatabase = (DbConfig)DbConfigView.Clone();
            configDatabase.DbName = string.Empty;

            string dbName = DbConfigView.DbName + "_dbsearchconfig";
            if (_firstCall) {
                _firstCall = false;
                IDatabase conn = ConnectionManager.CreateConnection(configDatabase);
                conn.Open();
                conn.CreateDatabaseIfNotExists(dbName);
                conn.Close();


                configDatabase.DbName = dbName;
                conn = ConnectionManager.CreateConnection(configDatabase);
                conn.Open();
                //CreateTables(conn);
                return conn;
            } else {
                configDatabase.DbName = dbName;
                IDatabase conn = ConnectionManager.CreateConnection(configDatabase);
                conn.Open();
                return conn;
            }
            
        }
        #endregion

        #region SaveCommonConfig
        private void SaveCommonConfig() {
            using (IDatabase conn = GetOpenConnection()) {
                conn.DbMapping.Save(DbCommonConfig);
            }
        }
        #endregion SaveCommonConfig

        #region Save
        public void Save() {
            if (!IsLoaded) return;
            DbConfigString = DbCommonConfig.XmlConfig;
            using (IDatabase conn = GetOpenConnection()) {
                conn.DbMapping.Save(this);
            }
        }
        #endregion Save

        #region Load
        public void Load(DbProfile dbProfile) {
            this.Id = dbProfile.Id;
            this.Description = dbProfile.Description;
            this.CustomRules = dbProfile.CustomRules;
            //if (!IsLoaded) {
                using (IDatabase conn = GetOpenConnection()) {
                    List<DbCommonConfig> tmp = conn.DbMapping.Load<DbCommonConfig>("profile_id=" + Id);
                    if (tmp.Count == 1) {
                        DbCommonConfig = tmp[0];
                        DbCommonConfig.DbProfile = this;
                        //DbCommonConfig.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(CommonConfig_PropertyChanged);
                    }
                    else {
                        DbCommonConfig.DbProfile = this;
                    }
                }
            //}
            IsLoaded = true;
        }
        #endregion Load

        #region CheckDatabase
        //Returns true if the database is ok, otherwise false
        public bool CheckDatabase(IDatabase conn) {
            conn.DbMapping.CreateTableIfNotExists<Info>();

            Info = new Info(this);
            string currentDbVersion = Info.DbVerion;
            if (currentDbVersion == null) {
                // init version with current databbase version
                Info.DbVerion = VersionInfo.CurrentDbVersion;
            } else if (VersionInfo.NewerDbVersionExists(currentDbVersion)) {
                DatabaseTooOld = true;
                return false;
                
            } else if (VersionInfo.ProgramVersionToOld(currentDbVersion)) {
                DatabaseTooNew = true;
                return false;
                //throw new Exception(string.Format("Ihre DBSearch Programmversion ({0}) ist zu alt, die Datenbank hat bereits Version {1}. Profil: {2}", VersionInfo.CurrentDbVersion, currentDbVersion, Name));
            }
            return true;
        }
        #endregion CheckDatabase

        #region UpdateDatabase
        public void UpdateDatabase() {
            using (IDatabase conn = GetOpenConnection()) {
                DatabaseUpgrade.UpdateDatabase(this, conn);
            }
            DatabaseTooOld = false;
        }
        #endregion

        #region CreateTables
        internal void CreateTables(IDatabase conn) {

            conn.DbMapping.CreateTableIfNotExists<Info>();
            conn.DbMapping.CreateTableIfNotExists<DbProfile>();
            conn.DbMapping.CreateTableIfNotExists<DbCommonConfig>();
            conn.DbMapping.CreateTableIfNotExists<DbQuery>();
            conn.DbMapping.CreateTableIfNotExists<DbColumn>();

            conn.DbMapping.CreateTableIfNotExists<DbResultSet>();
            conn.DbMapping.CreateTableIfNotExists<DbColumnHitInfo>();
            conn.DbMapping.CreateTableIfNotExists<DbSearchValueResult>();

            IsLoaded = true;
        }
        #endregion CreateTables

        public IDatabase GetDistinctConnection() {
            DbConfig configDatabase = (DbConfig)DbConfigView.Clone();
            //configDatabase.DbName = DbConfigView.DbName + "_dbsearchconfig_idx";
            configDatabase.DbName = DbConfigView.DbName + "_idx";

            IDatabase conn = ConnectionManager.CreateConnection(configDatabase);
            conn.Open();
            return conn;
        }
        #endregion Methods
    }
}
