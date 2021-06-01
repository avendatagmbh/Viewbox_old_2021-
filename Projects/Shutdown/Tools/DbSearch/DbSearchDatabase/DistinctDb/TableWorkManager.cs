using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using DbAccess;

namespace DbSearchDatabase.DistinctDb {
    public class TableWorkManager {
        #region Constructor
        public TableWorkManager() {
        }
        #endregion Constructor

        #region Properties
        private Dictionary<DistinctTable, DistinctTableInfo> _tableToInfo = new Dictionary<DistinctTable, DistinctTableInfo>();
        //public DistinctTable Table { get; set; }
        //public DistinctTableInfo TableInfo { get; set; }
        //public List<DistinctColumn> Columns { get; set; }

        #endregion Properties

        #region Methods
        public DistinctColumnInfo GetColumnInfo(DistinctColumn column, IDatabase dbSrc, IDatabase dbDst) {
            lock (_tableToInfo) {
                if (_tableToInfo.Count == 0)
                    LoadPreviousResults(dbSrc, dbDst);

                DistinctTableInfo ti;
                if (!_tableToInfo.TryGetValue(column.Table, out ti)) {
                    string table = column.Table.Name;
                    
                    //ti = DistinctTableInfo.OpenTableInfo(dbDst, table);
                    bool save = false;
                    if (ti == null) {
                        // create new table info
                        ti = new DistinctTableInfo(table);
                        //ti.Count = (uint) dbDst.CountTable(dbSrc.DbConfig.DbName, table);
                        save = true;
                        //ti.Started = DateTime.Now;
                        //ti.Save(dbDst);
                    }
                    else if (ti.Count !=
                             dbDst.CountTable(
                                 dbDst.DbConfig.DbName.Substring(0,dbDst.DbConfig.DbName.IndexOf(DbDistincter.DISTINCT_DB_SUFFIX)), table)) {
                        // truncate the columns, datasets changed
                        string sSql = "SELECT `cid` FROM `columns` WHERE `tid` = " + ti.Id;
                        var oReader = dbDst.ExecuteReader(sSql);
                        List<string> lColumnIds = new List<string>();
                        while (oReader.Read()) {
                            lColumnIds.Add(oReader[0].ToString());
                        }
                        oReader.Close();
                        foreach (string sColumnId in lColumnIds) {
                            sSql = "DROP TABLE IF EXISTS `idx_" + sColumnId + "`";
                            dbDst.ExecuteNonQuery(sSql);
                            sSql = "DROP TABLE IF EXISTS `rowno_" + sColumnId + "`";
                            dbDst.ExecuteNonQuery(sSql);
                        }
                        sSql = "DELETE FROM `tables` WHERE `tid` = " + ti.Id;
                        dbDst.ExecuteNonQuery(sSql);
                        sSql = "DELETE FROM `columns` WHERE `tid` = " + ti.Id;
                        dbDst.ExecuteNonQuery(sSql);

                        ti = new DistinctTableInfo(table);
                        //ti.Save(dbDst);
                        save = true;
                        
                    }
                    if (save) {
                        ti.Count = (uint)dbDst.CountTable(dbSrc.DbConfig.DbName, table);
                        ti.Started = DateTime.Now;
                        ti.Save(dbDst);
                    }
                    
                    //DistinctColumnInfo.OpenColumnInfos(dbDst, ti);
                    //Set DbColumnInfo
                    foreach (var dbColumnInfo in dbSrc.GetColumnInfos(table)) {
                        if(dbColumnInfo.Name.ToLower() != "_row_no_")
                            ti.Columns.Add(dbColumnInfo.Name, new DistinctColumnInfo(ti, dbColumnInfo.Name){ DbColumnInfo = dbColumnInfo});
                        //bool found = false;
                        //foreach (var col in ti.Columns.Values)
                        //    if (col.Name.ToLower() == dbColumnInfo.Name.ToLower()) {
                        //        col.DbColumnInfo = dbColumnInfo;
                        //        found = true;
                        //        break;
                        //    }
                        //if(!found)
                        //    throw new Exception(string.Format("Spalte {0} ist nicht vorhanden", dbColumnInfo.Name));
                    }

                    _tableToInfo[column.Table] = ti;
                }
            }
            //This is necessary if the tables were Loaded previously
            //column.Table.Id = _tableToInfo.Keys[column.Table].Id;
            DistinctColumnInfo columnInfo;
            if (_tableToInfo[column.Table].Columns.TryGetValue(column.Name, out columnInfo)) return columnInfo;
            return null;
        }

        private void LoadPreviousResults(IDatabase dbSrc, IDatabase dbDst) {
            List<DistinctColumnInfo> columns = dbDst.DbMapping.Load<DistinctColumnInfo>();
            List<DistinctTableInfo> tables = dbDst.DbMapping.Load<DistinctTableInfo>();
            foreach (var column in columns) {
                foreach (var table in tables) {
                    if (table.Finished != default(DateTime))
                        continue;

                    if (column.TableId == table.Id ) {
                        column.TableInfo = table;
                        table.Columns.Add(column.Name, column);
                        break;
                    }
                }
            }
            foreach (var table in tables) {
                //Need to load columns
                if (table.Finished == default(DateTime)) {

                    foreach (var dbColumnInfo in dbSrc.GetColumnInfos(table.Name)) {
                        if (dbColumnInfo.Name.ToLower() == "_row_no_") continue;
                        if (!table.Columns.ContainsKey(dbColumnInfo.Name))
                            table.Columns.Add(dbColumnInfo.Name, new DistinctColumnInfo(table, dbColumnInfo.Name) { DbColumnInfo = dbColumnInfo });
                        else table.Columns[dbColumnInfo.Name].DbColumnInfo = dbColumnInfo;
                    }

                    //delete all finished columns
                    foreach (var col in table.Columns.Values.Where(c => c.Finished != default(DateTime)).ToList())
                        table.Columns.Remove(col.Name);
                }
                _tableToInfo.Add(new DistinctTable(table.Name, table.Id), table);
            }
            
        }

        public void ColumnFinished(DistinctColumnInfo column, IDatabase db) {
            //Save table, save column
            column.Finished = DateTime.Now;
            column.Save(db);
            column.TableInfo.Columns.Remove(column.Name);
            if (column.TableInfo.Columns.Count == 0) {
                column.TableInfo.Finished = DateTime.Now;
                column.TableInfo.Save(db);
            }
        }
        #endregion Methods
    }
}
