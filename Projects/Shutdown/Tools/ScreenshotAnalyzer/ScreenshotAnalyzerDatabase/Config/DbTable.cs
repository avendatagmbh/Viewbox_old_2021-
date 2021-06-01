// --------------------------------------------------------------------------------
// author: Benjamin Held
// since: 2011-12-07
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DbAccess;
using ScreenshotAnalyzerDatabase.Interfaces;
using ScreenshotAnalyzerDatabase.Structures;

namespace ScreenshotAnalyzerDatabase.Config {

    [DbTable("tables", ForceInnoDb = true)]
    internal class DbTable : DatabaseObjectBase<int>, IDbTable {
        public DbTable() {
            ScreenshotGroups = new List<IDbScreenshotGroup>();
        }

        #region Profile
        [DbColumn("profile_id", IsInverseMapping = true)]
        internal DbProfile Profile { get; set; }
        #endregion

        #region Name
        private string _name;
        [DbColumn("name", AllowDbNull = true)]
        public string Name {
            get { return _name; }
            set {
                if (_name != value) {
                    _name = value;
                    Save();
                }
            }
        }
        #endregion Name

        private string _tableName;
        [DbColumn("tablename", AllowDbNull = true)]
        public string TableName {
            get { return _tableName; }
            set {
                if (_tableName != value) {
                    _tableName = value;
                    Save();
                }
            }
        }

        private string _comment;
        [DbColumn("comment", AllowDbNull = true)]
        public string Comment {
            get { return _comment; }
            set {
                if (_comment != value) {
                    _comment = value;
                    Save();
                }
            }
        }

        public List<IDbScreenshotGroup> ScreenshotGroups { get; private set; }

        public void Delete() {
            foreach(var scrGroup in ScreenshotGroups)
                scrGroup.Delete();
            using (IDatabase conn = Profile.GetOpenConnection()) {
                conn.DbMapping.Delete(this);
            }
        }

        public void Load() {
            using (IDatabase conn = Profile.GetOpenConnection()) {
                List<DbScreenshotGroup> screenshotGroups = conn.DbMapping.Load<DbScreenshotGroup>("table_id=" + Id);
                ScreenshotGroups.Clear();
                foreach (var screenshotGroup in screenshotGroups) {
                    screenshotGroup.Table = this;
                    screenshotGroup.Load();
                    ScreenshotGroups.Add(screenshotGroup);
                }

            }
        }

        internal void Save() {
            //Profile is only null if the table is currently loaded from the database
            if (Profile == null) return;
            using (IDatabase conn = Profile.GetOpenConnection()) {
                conn.DbMapping.Save(this);
            }
        }


    }
}
