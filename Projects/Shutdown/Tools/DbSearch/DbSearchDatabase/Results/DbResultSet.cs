// --------------------------------------------------------------------------------
// author: Benjamin Held
// since: 2011-10-07
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DbAccess;
using DbSearchDatabase.Interfaces;
using DbSearchDatabase.Structures;
using DbSearchDatabase.TableRelated;

namespace DbSearchDatabase.Results {

    [DbTable("results", ForceInnoDb = true)]
    public class DbResultSet : DatabaseObjectBase<int>, IDbResultSet {
        #region Constructor
        internal DbResultSet(DbQuery query) {
            DbQuery = query;
        }

        public DbResultSet() {
        }
        #endregion

        #region Properties
        [DbColumn("query_id", IsInverseMapping = true)]
        public DbQuery DbQuery { get; set; }

        [DbColumn("timestamp")]
        public DateTime Timestamp { get; set; }

        [DbColumn("comment")]
        public string Comment { get; set; }

        [DbColumn("query_snapshot", Length=100000)]
        public string QuerySnapshot { get; set; }

        [DbColumn("column_results")]
        public string ColumnResultsString { get; set; }

        [DbColumn("created_by")]
        public int CreatedBy { get; set; }

        //Shows if the search finished
        [DbColumn("finished")]
        public bool Finished { get; set; }

        #endregion Properties

        #region Methods
        public void Save() {
            using (IDatabase conn = DbQuery.DbProfile.GetOpenConnection()) {
                conn.DbMapping.Save(this);
            }
        }
        #endregion
    }
}
