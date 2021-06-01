using System.Collections.Generic;
using System.Data.Common;
using DbAccess;
using DbSearchDatabase.Interfaces;
using DbSearchDatabase.Structures;
using DbSearchDatabase.TableRelated;

namespace DbSearchDatabase.Results {
    [DbTable("results_column_hit_infos", ForceInnoDb = true)]
    public class DbColumnHitInfo : DatabaseObjectBase<int>{
        #region Properties
        [DbColumn("query_id", IsInverseMapping = true)]
        public DbQuery DbQuery { get; set; }

        [DbColumn("result_id", IsInverseMapping = true)]
        [DbIndex("_results_column_hit_infos_result_id")]
        public DbResultSet DbResultSet { get; set; }

        [DbColumn("search_column_id")]
        public int SearchColumnId { get; set; }

        [DbColumn("hit_table_name")]
        public string TableName { get; set; }

        [DbColumn("hit_column_name")]
        public string HitColumnName { get; set; }
        #endregion Properties

        public static List<DbColumnHitInfo> MassLoad(IDatabase conn, string filter) {
            List<DbColumnHitInfo> result = new List<DbColumnHitInfo>();
            using (var reader = conn.ExecuteReader("SELECT id,search_column_id,hit_table_name,hit_column_name FROM results_column_hit_infos " + filter)) {
                while (reader.Read()) {
                    DbColumnHitInfo dbColumnHitInfo = new DbColumnHitInfo() {
                                                                                Id = reader.GetInt32(0),
                                                                                SearchColumnId = reader.GetInt32(1),
                                                                                TableName = reader.GetString(2),
                                                                                HitColumnName = reader.GetString(3)
                                                                            };
                    result.Add(dbColumnHitInfo);
                }
            }
            return result;
        }
    }
}
