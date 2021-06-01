using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DbAccess;
using System.Data.Common;

namespace DbComparisonV2.Business
{
    class DatabaseNoSystem : IDatabaseInformation
    {
        public DatabaseNoSystem(IDatabase conn)
        {
            _conn = conn;
        }

        IDatabase _conn;

        public void GetTables(List<ComparisonResult.TableInfo> tables)
        {
            List<string> tableNames = _conn.GetTableList();
            foreach (var tableName in tableNames)
            {
                long rows = _conn.CountTable(tableName);
                tables.Add(new ComparisonResult.TableInfo(tableName, rows));
            }

        }

        public void GetColumns(List<ComparisonResult.ColumnInfo> columns, Dictionary<string, List<ComparisonResult.ColumnInfo>> columnsDict, ComparisonResult.TableInfoCollection tables)
        {
            foreach (var table in tables)
            {
                using (DbDataReader reader = _conn.ExecuteReader("SELECT * FROM " + _conn.Enquote(table.Name), System.Data.CommandBehavior.SchemaOnly) as DbDataReader)
                {

                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        ComparisonResult.ColumnInfo colInfo = new ComparisonResult.ColumnInfo(reader.GetName(i), table.Name, reader.GetDataTypeName(i));
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
}
