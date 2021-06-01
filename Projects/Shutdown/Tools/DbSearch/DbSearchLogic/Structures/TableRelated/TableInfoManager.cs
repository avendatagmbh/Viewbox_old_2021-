// --------------------------------------------------------------------------------
// author: Benjamin Held
// since: 2011-11-01
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DbAccess;
using DbAccess.Structures;
using DbSearchLogic.Config;
using DbSearchLogic.SearchCore.Structures.Db;
using log4net;
using AV.Log;

namespace DbSearchLogic.Structures.TableRelated {
    public class TableInfoManager {

        internal ILog _log = LogHelper.GetLogger();

        #region Constructor
        public TableInfoManager(Profile profile) {
            Profile = profile;
        }
        #endregion Constructor

        #region Properties
        public Profile Profile { get; private set; }
        private readonly Dictionary<string, TableInfo> _tableNameToTableInfo = new Dictionary<string, TableInfo>();
        private readonly object _lock = new object();
        private bool _loadingTableInfos;
        private bool _loadedTableInfos;

        public List<TableInfo> Tables { get { return _tableNameToTableInfo.Values.ToList(); } } 
        #endregion Properties

        #region Methods
        public TableInfo GetTableInfo(string tableName) {
            lock (_lock) {
                if (!_loadingTableInfos && !_loadedTableInfos) {
                    _loadingTableInfos = true;
                    Task.Factory.StartNew(LoadTableInfos).ContinueWith(LoadedTableInfos);
                }

                TableInfo tableInfo;
                if (!_tableNameToTableInfo.TryGetValue(tableName, out tableInfo)) {
                    tableInfo = new TableInfo(tableName, -1);
                    _tableNameToTableInfo.Add(tableName, tableInfo);
                }
                return tableInfo;
            }
        }

        public void LoadTableInfos() {
            DbConfig systemConfig = (DbConfig) Profile.DbProfile.DbConfigView.Clone();
            string systemDbName = systemConfig.DbName + "_system";
            systemConfig.DbName = "";
            try {
            using (IDatabase conn = ConnectionManager.CreateConnection(systemConfig)) {
                conn.Open();
                conn.SetHighTimeout();
                if(conn.DatabaseExists(systemDbName)) {
                    conn.ChangeDatabase(systemDbName);
                    using (IDataReader reader = conn.ExecuteReader("SELECT table_id,name,comment,row_count FROM " + conn.Enquote("table")))
                    {
                        while (reader.Read())
                        {
                            lock (_lock)
                            {
                                string tableName = reader.GetString(1);
                                AddTable(tableName, reader.GetInt32(3), reader.GetInt32(0), reader.GetString(2));
                            }
                        }
                    }
                    LoadColumnInfos(conn);
                }
                else {
                    conn.ChangeDatabase(Profile.DbProfile.DbConfigView.DbName);
                    conn.SetHighTimeout();
                    foreach(var table in conn.GetTableInfos()) {
                        lock (_lock)
                        {
                            string tableName = table.Name;
                            AddTable(tableName, conn.CountTable(tableName), -1, "");
                        }
                    }
                }
            }

            //systemConfig.DbName += "_system";
            //using (IDatabase conn = ConnectionManager.CreateConnection(systemConfig)) {
            //    conn.Open();
            //}
            }
            catch(Exception ex) {
                _log.ErrorFormatWithCheck("Error while loading tables from {0}", systemDbName, ex);
                throw;
            }
        }

        private void AddTable(string tableName, long count, int id, string comment) {
            TableInfo tableInfo;
            if (!_tableNameToTableInfo.TryGetValue(tableName, out tableInfo)) {
                tableInfo = new TableInfo(tableName, count);
                _tableNameToTableInfo.Add(tableName, tableInfo);
            }

            tableInfo.Id = id;
            tableInfo.Comment = comment;
        }

        public void LoadColumnInfos(IDatabase conn) {
            Dictionary<int,TableInfo> idToTableInfo = new Dictionary<int, TableInfo>();
            lock (_lock) {
                foreach (var tableInfo in _tableNameToTableInfo.Values)
                    if (tableInfo.Id != 0) idToTableInfo[tableInfo.Id] = tableInfo;
            }
            using (IDataReader reader = conn.ExecuteReader("SELECT table_id,name,comment FROM " + conn.Enquote("col"))) {
                while (reader.Read()) {
                    if (idToTableInfo.ContainsKey(reader.GetInt32(0))) {
                        idToTableInfo[reader.GetInt32(0)].AddColumnInfo(new ColumnInfo(reader.GetString(1)){Comment = reader.GetString(2)});
                    }
                }
            }
        }

        public void LoadedTableInfos(Task task) {
            lock (_lock) {
                _loadedTableInfos = true;
                _loadingTableInfos = false;
            }
            if (task.IsFaulted) {
                if (task.Exception != null && task.Exception.InnerException != null)
                    _log.Log(LogLevelEnum.Error, "Es ist ein Fehler beim Laden der Tabellen Information aus der System-Datenbank aufgetreten:" + Environment.NewLine + task.Exception.InnerException.Message);
                else
                    _log.Log(LogLevelEnum.Error, "Es ist ein Fehler beim Laden der Tabellen Information aus der System-Datenbank aufgetreten:");
            }
        }

        public void AddTableInfo(TableInfo addTableInfo) {
            lock (_lock) {
                TableInfo tableInfo;
                if (!_tableNameToTableInfo.TryGetValue(addTableInfo.Name, out tableInfo)) {
                    _tableNameToTableInfo.Add(addTableInfo.Name,addTableInfo);
                }
                else {
                    tableInfo.Comment = addTableInfo.Comment;
                }
            }
        }
        #endregion Methods

    }
}
