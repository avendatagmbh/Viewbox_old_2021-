// --------------------------------------------------------------------------------
// author: Benjamin Held
// since: 2011-12-13
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

    [DbTable("results", ForceInnoDb = true)]
    internal class DbResult : DatabaseObjectBase<int>, IDbResult {
        public DbResult() {
            ResultColumns = new List<IDbResultColumn>();
            ResultEntries = new List<IDbResultEntry>();
        }

        public DbResult(DbScreenshotGroup dbScreenshotGroup) {
            ScreenshotGroup = dbScreenshotGroup;
            CreatedOn = DateTime.UtcNow;
            ResultColumns = new List<IDbResultColumn>();
            ResultEntries = new List<IDbResultEntry>();
            Save();
        }


        #region Properties
        #region DbScreenshotGroup
        [DbColumn("scr_group_id", IsInverseMapping = true)]
        internal DbScreenshotGroup ScreenshotGroup { get; set; }
        #endregion DbScreenshotGroup

        [DbColumn("created")]
        public DateTime CreatedOn { get; set; }

        public List<IDbResultColumn> ResultColumns { get; set; }
        public List<IDbResultEntry> ResultEntries { get; set; } 
        #endregion Properties

        public void Save() {
            using (IDatabase conn = ScreenshotGroup.Table.Profile.GetOpenConnection()) {
                conn.DbMapping.Save(this);
            }
        }

        public void Load() {
            using (IDatabase conn = ScreenshotGroup.Table.Profile.GetOpenConnection()) {
                IEnumerable<DbResultColumn> columns = conn.DbMapping.Load<DbResultColumn>("result_id = " + Id);
                foreach (var column in columns) {
                    column.Result = this;
                    column.ScreenshotGroup = ScreenshotGroup;
                    ResultColumns.Add(column);
                }

                IEnumerable<DbResultEntry> resultEntries = conn.DbMapping.Load<DbResultEntry>("result_id = " + Id);
                foreach (var resultEntry in resultEntries) {
                    //Set screenshot from id
                    foreach(DbScreenshot screenshot in ScreenshotGroup.Screenshots)
                        if(screenshot.Id == resultEntry.ScreenshotId) {
                            resultEntry.Screenshot = screenshot;
                            break;
                        }

                    resultEntry.Result = this;
                    ResultEntries.Add(resultEntry);
                }
            }
        }
    }
}
