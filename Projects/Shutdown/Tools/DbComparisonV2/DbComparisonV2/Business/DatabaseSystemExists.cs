using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DbAccess;
using System.Data.Common;

namespace DbComparisonV2.Business
{
    public class DatabaseSystemExists : IDatabaseInformation
    {
        public DatabaseSystemExists(IDatabase conn)
        {
            _conn = conn;
        }

        IDatabase _conn;

        public void GetTables(List<ComparisonResult.TableInfo> tables)
        {
            string fullDbName = _conn.DbConfig.DbName + "_system";

            string sql = "SELECT name,row_count FROM " + fullDbName + "." + _conn.Enquote("table");
            using (DbDataReader reader = _conn.ExecuteReader(sql) as DbDataReader)
            {
                while (reader.Read())
                {
                    tables.Add(
                        new ComparisonResult.TableInfo(reader["name"].ToString(), Convert.ToInt64(reader["row_count"]))
                        );
                }
            }
        }

        public void GetColumns(List<ComparisonResult.ColumnInfo> columns, Dictionary<string, List<ComparisonResult.ColumnInfo>> columnsDict, ComparisonResult.TableInfoCollection tables)
        {
            string fullDbName = _conn.DbConfig.DbName + "_system";
            string sql = "SELECT c.name,t.name,c.type FROM " +
                fullDbName + "." + _conn.Enquote("table") + " t," +
                fullDbName + "." + _conn.Enquote("col") + " c" +
                " WHERE t.table_id=c.table_id";


            using (DbDataReader reader = _conn.ExecuteReader(sql) as DbDataReader)
            {
                while (reader.Read())
                {

                    ComparisonResult.ColumnInfo colInfo = new ComparisonResult.ColumnInfo(reader[0].ToString(), reader[1].ToString(), reader[2].ToString());
                    columns.Add(colInfo);

                    if (!columnsDict.ContainsKey(colInfo.TableName)) 
                    { 
                        columnsDict[colInfo.TableName] = new List<ComparisonResult.ColumnInfo>(); 
                    }
                    tables[colInfo.TableName].Columns.Add(colInfo);
                    columnsDict[colInfo.TableName].Add(colInfo);
                }
            }
        }

    }
}
