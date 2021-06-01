// --------------------------------------------------------------------------------
// author: Benjamin Held
// since: 2011-09-08
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;
using DbAccess;
using DbAccess.Structures;
using DbSearchDatabase.Config;
using ScreenshotAnalyzerDatabase.Interfaces;
using ScreenshotAnalyzerDatabase.Structures;
using ScreenshotAnalyzerDatabase.Upgrader;

namespace ScreenshotAnalyzerDatabase.Config {
    [DbTable("profiles", ForceInnoDb = true)]
    public class DbProfile : DatabaseObjectBase<int>, IDbProfile {
        #region Constructor
        
        public DbProfile() {
            DbConfig = new DbConfig("SQLite");
            Tables = new List<IDbTable>();
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
        #endregion Description

        [DbColumn("created_by")]
        public int CreatedBy { get; set; }

        #region AccessPath
        [DbColumn("access_path")]
        public string AccessPath {
            get { return _accessPath; }
            set {
                if (_accessPath != value) {
                    _accessPath = value;
                    Save();
                }
            }
        }
        private string _accessPath;
        #endregion AccessPath

        public DbConfig DbConfig { get; set; }
        internal Info Info { get; set; }
        public bool IsLoaded { get; set; }
        public bool DatabaseTooOld { get; private set; }
        public bool DatabaseTooNew { get; private set; }

        //[DbCollection("Profile",LazyLoad = false)]
        public List<IDbTable> Tables { get; private set; }
        //private bool _firstCall = true;
        #endregion Properties

        #region Methods

        //public DbConfig GetProfileConfig() {
        //    DbConfig configDatabase = (DbConfig)DbConfigView.Clone();
        //    configDatabase.DbName = DbConfigView.DbName + "_dbsearchconfig";
        //    return configDatabase;
        //}

        #region GetOpenConnection
        public IDatabase GetOpenConnection() {
            DbConfig configDatabase = (DbConfig)DbConfig.Clone();
            IDatabase conn = ConnectionManager.CreateConnection(configDatabase);
            conn.Open();
            return conn;
        }

        //Load all properties from database
        public void Load() {
            if (!IsLoaded) {
                using (IDatabase conn = GetOpenConnection()) {
                    //DbProfile profile = conn.DbMapping.Load<DbProfile>(Id);
                    List<DbProfile> dbProfile = conn.DbMapping.Load<DbProfile>("id=" + Id);
                    if (dbProfile.Count != 1) throw new Exception("Fehler beim Laden des Profils " + Name);
                    this.AccessPath = dbProfile[0].AccessPath;
                    this.CreatedBy = dbProfile[0].CreatedBy;

                    List<DbTable> tables = conn.DbMapping.Load<DbTable>("profile_id=" + Id);
                    Tables.Clear();
                    foreach (var table in tables) {
                        table.Profile = this;
                        table.Load();
                        Tables.Add(table);
                    }
                }
            }
            IsLoaded = true;
        }

        #endregion

        #region Save
        public void Save(bool firstTimeSave) {
            if (!IsLoaded && !firstTimeSave) return;
            using (IDatabase conn = GetOpenConnection()) {
                conn.DbMapping.Save(this);
            }
        }

        public void Save() {
            if (!IsLoaded) return;
            using (IDatabase conn = GetOpenConnection()) {
                conn.DbMapping.Save(this);
            }
        }
        #endregion Save

        #region Load
        //Just the startup "load"
        public void Load(DbProfile dbProfile) {
            this.Id = dbProfile.Id;
            this.Description = dbProfile.Description;
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
            conn.DbMapping.CreateTableIfNotExists<DbTable>();
            conn.DbMapping.CreateTableIfNotExists<DbScreenshotGroup>();
            conn.DbMapping.CreateTableIfNotExists<DbScreenshot>();
            conn.DbMapping.CreateTableIfNotExists<DbOcrRectangle>();
            //conn.DbMapping.DropTableIfExists<DbResultEntry>();
            conn.DbMapping.CreateTableIfNotExists<DbResultEntry>();
            conn.DbMapping.CreateTableIfNotExists<DbResult>();
            conn.DbMapping.CreateTableIfNotExists<DbResultColumn>();
        }
        #endregion CreateTables
        #endregion Methods
    }
}
