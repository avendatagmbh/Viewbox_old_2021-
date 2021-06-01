// --------------------------------------------------------------------------------
// author: Benjamin Held
// since: 2011-10-10
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using DbAccess;
using DbSearchDatabase.Interfaces;
using DbSearchDatabase.Structures;
using DbSearchDatabase.TableRelated;

namespace DbSearchDatabase.Results {
    [DbTable("results_search_values", ForceInnoDb = true)]
    public class DbSearchValueResult : DatabaseObjectBase<long>{
        #region Properties
        [DbColumn("query_id", IsInverseMapping = true)]
        public DbQuery DbQuery { get; set; }

        [DbColumn("result_id", IsInverseMapping = true)]
        [DbIndex("_dbsearch_value_results_result_id")]
        public DbResultSet DbResultSet { get; set; }

        [DbColumn("column_hit_id")]
        [DbIndex("_dbsearch_value_results_column_hit_id")]
        public int DbColumnHitInfoId { get; set; }
        public DbColumnHitInfo DbColumnHitInfo { get; set; }

        [DbColumn("row_entry_id")]
        public int RowEntryId { get; set; }

        [DbColumn("hit_ids", Length = 100000, AutoLoad = false)]
        public byte[] HitIds { get; set; }

        [DbColumn("entry_types", Length = 100000, AutoLoad = false)]
        public byte[] EntryTypes { get; set; }

        //Contains the first hit id and first entry type (may be null if no hit was recorded and can therefore be used to preview if there was a hit)
        [DbColumn("first_hit", Length = 256)]
        public byte[] FirstHit { get; set; }
        #endregion

        public static List<DbSearchValueResult> MassLoad(IDatabase conn, string filter) {
            List<DbSearchValueResult> result = new List<DbSearchValueResult>();
            using (var reader = conn.ExecuteReader("SELECT id,column_hit_id,row_entry_id,first_hit FROM results_search_values " + filter)) {
                while (reader.Read()) {
                    DbSearchValueResult dbSearchValueResult = new DbSearchValueResult() {
                        Id = reader.GetInt64(0),
                        DbColumnHitInfoId = reader.GetInt32(1),
                        RowEntryId = reader.GetInt32(2),
                        FirstHit = (byte[])reader["first_hit"]
                    };
                    result.Add(dbSearchValueResult);
                }
            }
            return result;
            
        }
    }
}
