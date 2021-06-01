using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using DbAccess;
using DbAccess.Structures;
using DbSearchDatabase.Results;
using log4net;
using AV.Log;

namespace DbSearchLogic.SearchCore.Structures.Result.DataMapper {
    class ResultSaver :IDisposable {

        internal ILog _log = LogHelper.GetLogger();

        public ResultSaver(ResultSet resultSet, DbConfig dbConfig) {
            _dbConfig = dbConfig;
            _resultSet = resultSet;
            CreateConnection();

            //Create snapshot of query and save it
            resultSet.DbResultSet.QuerySnapshot = resultSet.SnapshotQuery.ToXml();
            lock (StaticLock) {
                _conn.DbMapping.Save(resultSet.DbResultSet);
            }
            foreach (var columnResult in _resultSet.ColumnResults)
                columnResult.ColumnHitsChanged += columnResult_ColumnHitsChanged;
            _lastSave = DateTime.Now;
        }

        private void CreateConnection() {
            if (_conn != null) {
                _conn.Dispose();
            }
            _conn = ConnectionManager.CreateConnection(_dbConfig);
            _conn.Open();
            _conn.SetHighTimeout();
        }

        //Maps the columnhits to database objects and saves them in a transaction
        void columnResult_ColumnHitsChanged(object sender, CollectionChangeEventArgs e) {
            if (e.Action != CollectionChangeAction.Add) return;
            lock (StaticLock) {
                //int hitsCleared = 0;
                ColumnResult columnResult = sender as ColumnResult;
                IList<ColumnHitInfo> hits = e.Element as IList<ColumnHitInfo>;
                if (hits == null || columnResult == null) return;
                try {
                    foreach (var columnHit in hits) {
                        DbColumnHitInfo dbColumnHitInfo = ColumnHitInfoMapper.Map(columnHit, _resultSet,
                                                                                  columnResult);
                        lock (_cachedColumnHitInfos) {
                            _cachedColumnHitInfos.Add(dbColumnHitInfo);
                        }
                        foreach (var result in columnHit.Results) {
                            lock (_cachedSearchValueResults) {
                                _cachedSearchValueResults.AddRange(SearchValueResultMapper.Map(result, _resultSet,
                                                                                               dbColumnHitInfo));
                            }
                            //Clear results to save memory
                            result.Clear();
                        }
                    }

                    hits.Clear();
                    lock (_cachedSearchValueResults) {
                        lock (_cachedColumnHitInfos) {
                            if (_cachedSearchValueResults.Count > 500 || (DateTime.Now - _lastSave).TotalMinutes > 10)
                                SaveCachedValues();
                        }
                    }
                }
                catch (Exception ex) {
                    _log.Log(LogLevelEnum.Error, "Fehler beim Speichern der Ergebnisse für Spalte " + columnResult.Name + ", Fehler: " + ex.Message);
                }
            }
            
        }

        private void SaveCachedValues() {
            lock (_cachedSearchValueResults) {
                lock (_cachedColumnHitInfos) {
                    lock (StaticLock) {
                        try {
                            if (!_conn.IsOpen) {
                                CreateConnection();
                            }
                            //Need to lock the complete table, as the executescalar fetches an id which is then used for the insert of the data, otherwise duplicate primaries keys could be inserted
                            string lockTableSql =
                                "LOCK TABLES results_column_hit_infos READ,results_search_values READ; SET AUTOCOMMIT=0";
                            try {
                                _conn.ExecuteNonQuery(lockTableSql);
                            } catch (Exception ex) {
                                _log.Log(LogLevelEnum.Warn, string.Format("Fehler beim Speichern der Ergebnisse: {0}. Versuche die Verbindung erneut aufzubauen.", ex.Message), true);
                                CreateConnection();
                                _conn.ExecuteNonQuery(lockTableSql);
                            }
                            _conn.BeginTransaction();

                            _conn.DbMapping.Save(typeof(DbColumnHitInfo), _cachedColumnHitInfos);
                            List<DbColumnValues> searchValueResults =
                                new List<DbColumnValues>(_cachedSearchValueResults.Count);

                            //Get maximum id

                            object idAsObject =
                                _conn.ExecuteScalar("SELECT MAX(" + _conn.Enquote("id") + ") FROM " +
                                                    _conn.Enquote("results_search_values"));
                            long id = 1;
                            if (!(idAsObject is DBNull))
                                id = Convert.ToInt64(idAsObject) + 1;

                            //Convert to DbColumnValues
                            foreach (var searchValueResult in _cachedSearchValueResults) {
                                DbColumnValues current = new DbColumnValues();
                                searchValueResult.Id = id;
                                current["id"] = id++;
                                current["query_id"] = searchValueResult.DbQuery.Id;
                                current["result_id"] = searchValueResult.DbResultSet.Id;
                                current["column_hit_id"] = searchValueResult.DbColumnHitInfo.Id;
                                current["row_entry_id"] = searchValueResult.RowEntryId;
                                current["hit_ids"] = searchValueResult.HitIds;
                                current["entry_types"] = searchValueResult.EntryTypes;
                                current["first_hit"] = searchValueResult.FirstHit;
                                searchValueResults.Add(current);
                                searchValueResult.HitIds = null;
                                searchValueResult.EntryTypes = null;
                            }
                            _conn.InsertInto("results_search_values", searchValueResults);
                            _conn.CommitTransaction();
                            foreach (var result in searchValueResults) {
                                result["hit_ids"] = null;
                                result["entry_types"] = null;
                            }
                            _cachedSearchValueResults.Clear();
                            _cachedColumnHitInfos.Clear();
                            GC.Collect();
                        } catch (Exception ex) {
                            _log.Log(LogLevelEnum.Error, "Fehler beim Speichern der Ergebnisse für Spalte: " + ex.Message);
                            try {
                                _conn.RollbackTransaction();
                            } catch (Exception) {

                            }
                        } finally {
                            _lastSave = DateTime.Now;
                            try {
                                _conn.ExecuteNonQuery("UNLOCK TABLES;SET AUTOCOMMIT=1;");
                            } catch (Exception) {

                            }
                        }
                    }
                }
            }
            
        }

        #region Properties
        private List<DbColumnHitInfo> _cachedColumnHitInfos = new List<DbColumnHitInfo>();
        private List<DbSearchValueResult> _cachedSearchValueResults = new List<DbSearchValueResult>(); 
        private static readonly object StaticLock = new object();
        private IDatabase _conn;
        private readonly DbConfig _dbConfig;
        private readonly ResultSet _resultSet;
        private DateTime _lastSave;
        #endregion Properties

        #region Methods
        public void Dispose() {
            if (_conn != null &&_conn.IsOpen) _conn.Close();
        }

        public void Finished() {
            _conn.DbMapping.Save(_resultSet.DbResultSet);
            SaveCachedValues();
            foreach (var columnResult in _resultSet.ColumnResults)
                columnResult.ColumnHitsChanged -= columnResult_ColumnHitsChanged;
            //_resultSet.Unload();
            Dispose();
        }
        #endregion Methods
    }
}
