using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DbAccess;
using System.Data.Common;

namespace DbComparison.Business {
    class DatabaseNoSystem : IDatabaseInformation{
        public DatabaseNoSystem(IDatabase conn) {
            _conn = conn;
        }

        IDatabase _conn;

        public void GetTables(List<ComparisonResult.TableInfo> tables) {
            List<string> tableNames = _conn.GetTableList();
            foreach (var tableName in tableNames) {
                long rows = _conn.Count(tableName);
                tables.Add(new ComparisonResult.TableInfo(tableName, rows));
            }

        }

        public void GetColumns(List<ComparisonResult.ColumnInfo> columns, Dictionary<string, List<ComparisonResult.ColumnInfo>> columnsDict, List<ComparisonResult.TableInfo> tables) {
            foreach (var table in tables) {
                using (DbDataReader reader = _conn.ExecuteReader("SELECT * FROM " + _conn.Enquote(table.Name), System.Data.CommandBehavior.SchemaOnly)) {

                    for (int i = 0; i < reader.FieldCount; i++) {
                        ComparisonResult.ColumnInfo column = new ComparisonResult.ColumnInfo(reader.GetName(i), table.Name, reader.GetDataTypeName(i));
                        columns.Add(column);

                        if (!columnsDict.ContainsKey(column.TableName))
                            columnsDict[column.TableName] = new List<ComparisonResult.ColumnInfo>();
                        columnsDict[column.TableName].Add(column);
                    }
                }
            }
        }
    }
}
