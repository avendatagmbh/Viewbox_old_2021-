// --------------------------------------------------------------------------------
// author: Benjamin Held
// since: 2011-12-09
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using DbAccess;
using ScreenshotAnalyzerDatabase.Interfaces;
using ScreenshotAnalyzerDatabase.Structures;

namespace ScreenshotAnalyzerDatabase.Config {

    [DbTable("screenshots")]
    internal class DbScreenshot : DatabaseObjectBase<int>, IDbScreenshot {
        #region DbScreenshotGroup
        [DbColumn("scr_group_id", IsInverseMapping = true)]
        internal DbScreenshotGroup ScreenshotGroup { get; set; }
        #endregion DbScreenshotGroup

        #region Path
        private string _path;

        [DbColumn("name", AllowDbNull = true)]
        public string Path {
            get { return _path; }
            set {
                if (_path != value) {
                    _path = value;
                    Save();
                }
            }
        }
        #endregion Path

        public void Save() {
            //ScreenshotGroup is only null if the table is currently loaded from the database
            if (ScreenshotGroup == null) return;
            using (IDatabase conn = ScreenshotGroup.Table.Profile.GetOpenConnection()) {
                conn.DbMapping.Save(this);
            }
        }

        public void Delete() {
            using (IDatabase conn = ScreenshotGroup.Table.Profile.GetOpenConnection()) {
                conn.ExecuteNonQuery("DELETE FROM " + conn.DbMapping.GetTableName<DbOcrRectangle>() + " WHERE scr_id=" +
                                     Id);
                conn.ExecuteNonQuery("DELETE FROM " + conn.DbMapping.GetTableName<DbResultEntry>() + " WHERE scr_id=" +
                                     Id);
                conn.DbMapping.Delete(this);
            }
        }

        public void Load() {
            
        }
    }
}
