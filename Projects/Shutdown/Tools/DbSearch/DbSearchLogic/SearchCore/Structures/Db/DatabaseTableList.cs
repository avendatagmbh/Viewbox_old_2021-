using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using DbAccess;
using DbAccess.Structures;
using DbSearchLogic.Structures.TableRelated;
using log4net;
using AV.Log;

namespace DbSearchLogic.SearchCore.Structures.Db {
    public class DatabaseTableList {

        internal ILog _log = LogHelper.GetLogger();

        public event EventHandler UpdateStarted;
        //public event EventHandler UpdateFinished;

        private void OnUpdateStarted() { if (UpdateStarted != null) UpdateStarted(this, null); }
        //private void OnUpdateFinished() { if (UpdateStarted != null) UpdateFinished(this, null); }

        public DatabaseTableList() {
            this.TableInfos = new List<TableInfo>();
        }

        public void LoadTables(DbConfig dbConfig, TableInfoManager tableInfoManager) {

            OnUpdateStarted();

            TableInfos.Clear();

            try {
                using (IDatabase conn = ConnectionManager.CreateConnection(dbConfig)) {
                    conn.Open();
                    conn.SetHighTimeout();

                    List<TableInfo> lTables = new List<TableInfo>();
                    if (conn.DatabaseExists(conn.DbConfig.DbName + "_system")) {
                        using (IDataReader reader = conn.ExecuteReader("SELECT table_id,name,comment,row_count FROM " + conn.Enquote(dbConfig.DbName + "_system", "table") + " WHERE system_id=1")) {
                            while (reader.Read()) {
                                TableInfo tableInfo = new TableInfo(reader.GetString(1), reader.GetInt32(3));
                                tableInfo.Id = reader.GetInt32(0);
                                tableInfo.Comment = reader.GetString(2);
                                lTables.Add(tableInfo);
                            }
                        }
                    } else {
                        lTables.AddRange(conn.GetTableList().Select(tableName => new TableInfo(tableName, -1)));
                    }

                    foreach (TableInfo tableInfo in lTables) {
                        if(tableInfo.Count <= 0) tableInfo.Count = conn.CountTable(tableInfo.Name);
                        //List<string> lColumnNames = conn.GetColumnNames(tableInfo.Name);
                        //tableInfo.ColumnCount= lColumnNames.Count;
                        TableInfos.Add(tableInfo);
                        tableInfoManager.AddTableInfo(tableInfo);
                    }
                }

            } catch (Exception ex) {
                _log.Log(LogLevelEnum.Error, "Fehler beim laden der Tabellen aus der ViewDatenbank:" + Environment.NewLine + ex.Message);
                throw;

            } 
            //finally {
            //    OnUpdateFinished();
            //}
            try {
                DbConfig systemConfig = (DbConfig)dbConfig.Clone();
                systemConfig.DbName += "_system";
                using (IDatabase conn = ConnectionManager.CreateConnection(systemConfig)) {
                    conn.Open();
                    conn.SetHighTimeout();
                    tableInfoManager.LoadColumnInfos(conn);
                }
            } catch (Exception) {
                
            }
        }

        public List<TableInfo> TableInfos { get; set; }

        /// <summary>
        /// Gets a list of all huge tables.
        /// </summary>
        /// <returns></returns>
        public List<TableInfo> GetHugeTables() {
            List<TableInfo> oResult = new List<TableInfo>();
            foreach (TableInfo oTableInfo in TableInfos) {
                if (oTableInfo.IsHugeTable) {
                    oResult.Add(oTableInfo);
                }
            }
            return oResult;
        }

        /// <summary>
        /// Gets a list of all small tables.
        /// </summary>
        /// <returns></returns>
        public List<TableInfo> GetSmallTables() {
            List<TableInfo> oResult = new List<TableInfo>();
            foreach (TableInfo oTableInfo in TableInfos) {
                if (!oTableInfo.IsHugeTable) {
                    oResult.Add(oTableInfo);
                }
            }
            return oResult;
        }

    }
}
