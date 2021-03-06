using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DbAccess;
using System.Data.Common;

namespace DbComparison.Business {
    public class DatabaseSystemExists : IDatabaseInformation{
        public DatabaseSystemExists(IDatabase conn) {
            _conn = conn;
        }

        IDatabase _conn;

        public void GetTables(List<ComparisonResult.TableInfo> tables) {
            string fullDbName = _conn.DbConfig.DbName + "_system";

            string sql = "SELECT name,row_count FROM " + fullDbName + "." + _conn.Enquote("table");
            using (DbDataReader reader = _conn.ExecuteReader(sql)) {
                while (reader.Read()) {
                    tables.Add(
                        new ComparisonResult.TableInfo(reader["name"].ToString(), Convert.ToInt64(reader["row_count"]))
                        );
                }
            }
        }

        public void GetColumns(List<ComparisonResult.ColumnInfo> columns, Dictionary<string, List<ComparisonResult.ColumnInfo>> columnsDict, List<ComparisonResult.TableInfo> tables) {
            string fullDbName = _conn.DbConfig.DbName + "_system";
            string sql = "SELECT c.name,t.name,c.type FROM " +
                fullDbName + "." + _conn.Enquote("table") + " t," +
                fullDbName + "." + _conn.Enquote("col") + " c" +
                " WHERE t.table_id=c.table_id";


            using (DbDataReader reader = _conn.ExecuteReader(sql)) {
                while (reader.Read()) {

                    ComparisonResult.ColumnInfo column = new ComparisonResult.ColumnInfo(reader[0].ToString(), reader[1].ToString(), reader[2].ToString());
                    columns.Add(column);

                    if (!columnsDict.ContainsKey(column.TableName)) columnsDict[column.TableName] = new List<ComparisonResult.ColumnInfo>();
                    columnsDict[column.TableName].Add(column);
                }
            }
        }

    }
}
