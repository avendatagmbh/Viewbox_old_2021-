// --------------------------------------------------------------------------------
// author: Benjamin Held
// since: 2011-12-09
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
    [DbTable("scr_groups")]
    internal class DbScreenshotGroup : DatabaseObjectBase<int>, IDbScreenshotGroup {
        public DbScreenshotGroup() {
            Screenshots = new List<IDbScreenshot>();
        }

        #region Table
        [DbColumn("table_id", IsInverseMapping = true)]
        internal DbTable Table { get; set; }
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

        public List<IDbScreenshot> Screenshots { get; private set; }
        public void Delete() {
            foreach (var screenshot in Screenshots)
                screenshot.Delete();
            using (IDatabase conn = Table.Profile.GetOpenConnection()) {
                conn.ExecuteNonQuery("DELETE FROM " + conn.DbMapping.GetTableName<DbResult>() + " WHERE scr_group_id=" +
                                     Id);
                conn.ExecuteNonQuery("DELETE FROM " + conn.DbMapping.GetTableName<DbResultColumn>() + " WHERE scr_group_id=" +
                                     Id);
                conn.DbMapping.Delete(this);
            }
        }

        public void Load() {
            using (IDatabase conn = Table.Profile.GetOpenConnection()) {
                List<DbScreenshot> screenshots = conn.DbMapping.Load<DbScreenshot>("scr_group_id=" + Id);
                Screenshots.Clear();
                foreach (var screenshot in screenshots) {
                    screenshot.ScreenshotGroup = this;
                    screenshot.Load();
                    Screenshots.Add(screenshot);
                }

            }
        }

        internal void Save() {
            //Table is only null if the table is currently loaded from the database
            if (Table == null) return;
            if (Table.Id == 0) Table.Save();
            using (IDatabase conn = Table.Profile.GetOpenConnection()) {
                conn.DbMapping.Save(this);
            }
        }
    }
}
