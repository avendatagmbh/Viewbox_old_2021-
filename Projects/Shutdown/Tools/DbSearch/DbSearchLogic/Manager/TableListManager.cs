using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using DbAccess.Structures;
using DbSearchLogic.SearchCore.Structures;
using DbSearchLogic.SearchCore.Structures.Db;
using DbSearchLogic.Structures.TableRelated;

namespace DbSearchLogic.Manager {
    //Caches the tables from the view database, so that not each query has to fetch them again
    class TableListManager {
        private static object _lock = new object();
        private static Dictionary<DbConfig,DatabaseTableList> _configToDatabaseTableList = new Dictionary<DbConfig, DatabaseTableList>();

        public static TableList TableList(DbConfig viewConfig, TableInfoManager tableInfoManager, SearchTableDecider searchTableDecider) {
            lock (_lock) {
                DatabaseTableList tables;
                if (!_configToDatabaseTableList.ContainsKey(viewConfig)) {
                    tables = new DatabaseTableList();
                    tables.LoadTables(viewConfig, tableInfoManager);
                    _configToDatabaseTableList[viewConfig] = tables;
                }
                tables = _configToDatabaseTableList[viewConfig];

                TableList freeTables = new TableList();
                AddTables(freeTables.HugeTables, tables.GetHugeTables(), searchTableDecider);
                AddTables(freeTables.SmallTables, tables.GetSmallTables(), searchTableDecider);
                //freeTables.HugeTables.AddRange(tables.GetHugeTables());
                //freeTables.SmallTables.AddRange(tables.GetSmallTables());

                return freeTables;
            }
        }

        private static void AddTables(List<TableInfo> addToList, List<TableInfo> tables, SearchTableDecider searchTableDecider) {
            foreach (var table in tables) {
                if(searchTableDecider.IsTableAllowed(table))
                    addToList.Add(table);
            }
        }
    }
}
