
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using AvdCommon.Logging;
using DbAccess;
using DbAccess.Structures;
using DbSearchLogic.Config;
using DbSearchLogic.SearchCore.Structures.Db;
using AV.Log;
using MySql.Data.MySqlClient;

namespace DbSearchLogic.SearchCore.KeySearch
{
    /// <summary>
    /// A primary key searcher Advanced approach
    /// </summary>
    public class KeySearcherAdvanced : KeySearcherBase {
        
        #region [ Members ]
        
        /// <summary>
        /// ColumnId, rowId, valueHash
        /// </summary>
        private ConcurrentDictionary<int, SortedDictionary<int, int>> rowIdValueHashCacheByColumnId;

        #endregion [ Members ]

        #region [ Private variables ]
        
        /// <summary>
        /// Collection for key candidates processed with idx table approach
        /// </summary>
        private ConcurrentQueue<KeyCandidate> compositeKeyCandidatesIdx = new ConcurrentQueue<KeyCandidate>();

        /// <summary>
        /// The selected treshold method
        /// </summary>
        private KeySearchParameter.TresholdRowsSelector tresholdRowsMethod;
            
        /// <summary>
        /// The selected treshold value
        /// </summary>
        private long tresholdRowsCount;

        #endregion [ Private variables ]

        #region [ Constructors ]

        /// <summary>
        /// The object constructors
        /// </summary>
        /// <param name="currentProfile">The loaded profile</param>
        public KeySearcherAdvanced(Profile currentProfile, Func<string, IEnumerable<KeyCandidate>> actionGetAllColumnsAsKey, Func<IEnumerable<KeyCandidate>, int, IEnumerable<KeyCandidate>> actionCreateKeyCombinations) {
            logPrefix = "Key search (Advanced approach)";
            this.currentProfile = currentProfile;
            this.actionGetAllColumnsAsKey = actionGetAllColumnsAsKey;
            this.actionCreateKeyCombinations = actionCreateKeyCombinations;
            programInfo.HostName = currentProfile.DbProfile.DbConfigView.Hostname;
            programInfo.DbName = currentProfile.DbProfile.DbConfigView.DbName;
        }

        #endregion [ Constructors ]
        
        #region [ Public methods ]

        /// <summary>
        /// Init the simple key search
        /// </summary>
        /// <param name="keySearchParameter">The parameters</param>
        /// <param name="actionRecreateTables">The method which recreates the tables</param>
        /// <returns>True if key search initialized and can be performed</returns>
        public override bool InitSimpleKeySearch(KeySearchParameter keySearchParameter, Func<bool> actionRecreateTables) {
            
            tresholdRowsMethod = keySearchParameter.TresholdRowsMethod;
            tresholdRowsCount = keySearchParameter.TresholdRowsCount;
            return base.InitSimpleKeySearch(keySearchParameter, actionRecreateTables);
        }

        /// <summary>
        /// Init the composite key search
        /// </summary>
        /// <param name="keySearchParameter">The parameters</param>
        /// <returns>True if key search initialized and can be performed</returns>
        public override bool InitCompositeKeySearch(KeySearchParameter keySearchParameter) {

            keySearchParameter.InitiatedKeySearch.StatusDescription = "Step 3: Composite primary key search initialization";

            _log.Log(LogLevelEnum.Info, logPrefix + " - composite key search initialization started.", true);

            // do not start composite key search if key candidates in simple key init are not initialized
            if (!simpleKeysInitialized) {
                compositeKeysInitialized = false;
                return compositeKeysInitialized;
            }
            
            SetTreshold(compositeKeyCandidates);
            _log.Log(LogLevelEnum.Info, "Parameter - Row count treshold: " + tresholdRowsCount, true);
            
            compositeKeyCandidatesIdx = new ConcurrentQueue<KeyCandidate>();
            ConcurrentQueue<KeyCandidate> compositeKeysSql = new ConcurrentQueue<KeyCandidate>();
            //foreach (KeyCandidate keyCandidate in compositeKeyCandidates) {
            //    if (keyCandidate.NumberOfRows > tresholdRowsCount)
            //        compositeKeyCandidatesIdx.Enqueue(keyCandidate);
            //    else
            //        compositeKeysSql.Enqueue(keyCandidate);
            //}
            ParallelOptions options = new ParallelOptions() { MaxDegreeOfParallelism = keySearchParameter.DegreeOfParallelism };
            Parallel.ForEach(compositeKeyCandidates, options, keyCandidate => {
                if (keyCandidate.NumberOfRows > tresholdRowsCount)
                    compositeKeyCandidatesIdx.Enqueue(keyCandidate);
                else
                    compositeKeysSql.Enqueue(keyCandidate);
            });
            compositeKeyCandidatesIdx = new ConcurrentQueue<KeyCandidate>(compositeKeyCandidatesIdx.OrderBy(k => k.DisplayString));
            //compositeKeyCandidatesIdx = new ConcurrentQueue<KeyCandidate>(compositeKeyCandidatesIdx.Where(k => k.Columns[0].ColumnName == "COMPANY" && k.Columns[1].ColumnName == "VOUCHER_NO"));
            compositeKeyCandidates = compositeKeysSql;

            rowIdValueHashCacheByColumnId = new ConcurrentDictionary<int, SortedDictionary<int, int>>();

            programInfo.CompositeKeyCount = compositeKeyCandidates.Count + " (SQL approach) and " + compositeKeyCandidatesIdx.Count + " (IDX approach)";

            _log.Log(LogLevelEnum.Info, logPrefix + " - composite key search initialization finished!", true);

            compositeKeysInitialized = true;
            return compositeKeysInitialized;
        }
        
        /// <summary>
        /// The starts the composite key search
        /// </summary>
        /// <param name="parameter">The KeySearchParameters object</param>
        public override void StartCompositeKeySearch(KeySearchParameter parameter){
            // SQL approach
            base.StartCompositeKeySearch(parameter);
            GC.Collect();

            // IDX approach
            StartCompositeKeySearchByIdx(parameter);
            GC.Collect();
        }

        #endregion [ Public methods ]

        #region [ Private methods ]
        
        #region [ Composite key search - idx ]

        /// <summary>
        /// Searching two column keys in the IDX table
        /// </summary>
        /// <param name="keySearchParameter">The KeySearchParameters object</param>
        private void StartCompositeKeySearchByIdx(KeySearchParameter keySearchParameter) {

            keySearchParameter.InitiatedKeySearch.StatusDescription = "Step 5: Composite primary key search (Advanced)";
            const string description = "composite key search (IDX)";
            int threadNumber = System.Threading.Thread.CurrentThread.ManagedThreadId;
            System.Threading.CancellationToken cancellationToken = keySearchParameter.CancellationToken;

            _log.Log(LogLevelEnum.Info, logPrefix + " - " + description + " started on thread " + threadNumber.ToString() + " ...", true);
            if (!compositeKeysInitialized) {
                _log.Log(LogLevelEnum.Info, logPrefix + " - " + description + " was not initialized!", true);
                return;
            }

            if (currentProfile == null) return;
            double lastLogStatus = 0;
            int initialKeyCandidateCount = compositeKeyCandidatesIdx.Count;
            //Get the connection from the profile object
            DbConfig dbConfigIdx = (DbConfig)(currentProfile.DbConfigView).Clone();
            dbConfigIdx.DbName = KeySearchManager.GetIdxDbName(dbConfigIdx);
            
            while (compositeKeyCandidatesIdx.Count > 0){
                if (cancellationToken.IsCancellationRequested) return;

                KeyCandidate candidateKey = null;
                if (compositeKeyCandidatesIdx.TryDequeue(out candidateKey)) {
                        
                    if (!candidateKey.Processed && candidateKey.NumberOfRows > 0) {
                        if (IsCompositeKeyIdx2(keySearchParameter, candidateKey, dbConfigIdx)) {
                            keySearchParameter.InitiatedKeySearch.KeyFound();
                            _log.Log(LogLevelEnum.Info, "Composite key found: " + candidateKey.DisplayString, true);
                        } 
                        else
                            keySearchParameter.InitiatedKeySearch.KeyProcessed();
                    }
                    keySearchParameter.KeyCollectorInstance.DeleteKeys<KeyCandidate>(currentProfile.DbConfigView, candidateKey);
                    LogProcessingStatus(compositeKeyCandidatesIdx.Count, initialKeyCandidateCount, ref lastLogStatus, description);
                }
            }
                
            //Erase the lists [to GC.Collect]
            dbConfigIdx = null;

            _log.Log(LogLevelEnum.Info, logPrefix + " - " + description + " finished on thread " + threadNumber.ToString(), true);
        }

        #endregion [ Composite key search - idx ]

        #region [ IsCompositeKeyIdx - 1 ]

        /// <summary>
        /// Checks whether the given composite key is unique or not
        /// </summary>
        /// <param name="keySearchParameter">The KeySearchParameters object</param>
        /// <param name="candidateKey">The candidate composite key</param>
        /// <param name="dbConfigIdx">The db config</param>
        /// <returns>True if the key candidate is key in the table</returns>
        private bool IsCompositeKeyIdx(KeySearchParameter keySearchParameter, KeyCandidate candidateKey, DbConfig dbConfigIdx) {
            
            bool retVal = false;

            DateTime started = DateTime.Now;

            if (candidateKey.CandidateColumns.Count == 2) {
                using (var idxConn = ConnectionManager.CreateConnection(dbConfigIdx)) {
                    idxConn.Open();
                    idxConn.SetHighTimeout();
                    // put the column values (hash) into the cache by rowids
                    foreach (KeyCandidateColumn column in candidateKey.CandidateColumns) {
                        rowIdValueHashCacheByColumnId[column.ColumnId] = GetIdxRowIdValueHash(idxConn, column);
                    }
                    idxConn.Close();
                }
                bool isKey = true;
                Dictionary<int, List<int>> valueCombinations = new Dictionary<int, List<int>>();
                SortedDictionary<int, int> rowIdValuesFirstColumn =
                    rowIdValueHashCacheByColumnId[candidateKey.CandidateColumns[0].ColumnId];
                for (int i = 0; i < rowIdValuesFirstColumn.Count; i++) {
                    int index = i + 1;
                    int firstColumnValue;
                    // if index (row with the id) not found it means there is a gap in rowId, row not exists with this id and so this should be skipped
                    if (!rowIdValuesFirstColumn.TryGetValue(index, out firstColumnValue))
                        continue;
                    int nextColumnValue = rowIdValueHashCacheByColumnId[candidateKey.CandidateColumns[1].ColumnId][index];
                    List<int> tmpValues;
                    if (valueCombinations.TryGetValue(firstColumnValue, out tmpValues)) {
                        if (tmpValues.Contains(nextColumnValue)) {
                            isKey = false;
                            break;
                        } else {
                            valueCombinations[firstColumnValue].Add(nextColumnValue);
                        }
                    } else {
                        valueCombinations.Add(firstColumnValue, new List<int>() { nextColumnValue });
                    }
                }

                if (isKey) {
                    DateTime finished = DateTime.Now;
                    keySearchParameter.KeyCollectorInstance.AddKey(candidateKey, started, finished);
                    retVal = true;
                }
            } else {
                // this approach is slower for 2 column keys then the above
                using (var idxConn = ConnectionManager.CreateConnection(dbConfigIdx)) {
                    idxConn.Open();
                    idxConn.SetHighTimeout();
                    // put the column values (hash) into the cache by rowids
                    foreach (KeyCandidateColumn column in candidateKey.CandidateColumns) {
                        rowIdValueHashCacheByColumnId[column.ColumnId] = GetIdxRowIdValueHash(idxConn, column);
                    }
                    idxConn.Close();
                }
                bool isKey = true;
                List<int> valueCombinationsHash = new List<int>();
                for (int i = 0; i < rowIdValueHashCacheByColumnId[candidateKey.CandidateColumns[0].ColumnId].Count; i++) {
                    int index = i + 1;
                    int firstColumnValue;
                    // if index (row with the id) not found it means there is a gap in rowId sequence (the row may deleted), row not exists with this id and so this should be skipped
                    if (
                        !rowIdValueHashCacheByColumnId[candidateKey.CandidateColumns[0].ColumnId].TryGetValue(index, out firstColumnValue))
                        continue;

                    // get all column values for the rowId
                    List<int> nextColumnValues = new List<int>();
                    for (int j = 1; j < candidateKey.CandidateColumns.Count; j++) {
                        nextColumnValues.Add(rowIdValueHashCacheByColumnId[candidateKey.CandidateColumns[j].ColumnId][index]);
                    }

                    // compute hash from all column value hashes
                    int hash = firstColumnValue;
                    foreach (int columnValue in nextColumnValues) {
                        hash ^= columnValue;
                    }
                    // if the hash is already found in the list, then this group of columns is not a key in this table
                    if (valueCombinationsHash.Any(h => h == hash)) {
                        isKey = false;
                        break;
                    } else {
                        valueCombinationsHash.Add(hash);
                    }
                }

                if (isKey) {
                    DateTime finished = DateTime.Now;
                    keySearchParameter.KeyCollectorInstance.AddKey(candidateKey, started, finished);
                    retVal = true;
                }
            }
            return retVal;
        }

        /// <summary>
        /// Gets the value hash from database for the column by rowid
        /// </summary>
        /// <param name="idxConn">The idx database connection</param>
        /// <param name="candidateColumn">The column</param>
        /// <returns>Ther rowId-valueHash list</returns>
        private SortedDictionary<int, int> GetIdxRowIdValueHash(IDatabase idxConn, KeyCandidateColumn candidateColumn) {

            string sql = string.Format("SELECT rowIds, value FROM {0}", "idx_" + candidateColumn.ColumnId);
            SortedDictionary<int, int> rowIds = new SortedDictionary<int, int>();

            try {
                using (IDataReader reader = idxConn.ExecuteReader(sql)) {
                    while (reader.Read()) {
                        int valueHash = reader[1].ToString().GetHashCode();
                        ReaderBufferToRowNumbers(reader, rowIds, valueHash);
                    }
                }
            } catch (Exception ex) {
                _log.Log(LogLevelEnum.Info, "Exception occurred in GetIdxRowIdValueHash." + ex.Message, true);
            }
            return rowIds;
        }

        /// <summary>
        /// Transforms the rowIds binary field to rowId-valueHash list
        /// </summary>
        /// <param name="reader">The data reader</param>
        /// <param name="rowIdValues">The rowId value hash dictionary as output</param>
        /// <param name="valueHash">The hash of the value for rowIds will be collected</param>
        private void ReaderBufferToRowNumbers(IDataReader reader, SortedDictionary<int, int> rowIdValues, int valueHash) {
            const int BUFFER_SIZE = 1024;
            long offset = 0;
            int col = 0;
            byte[] buffer = new byte[BUFFER_SIZE];
            int bytesRead = 0;
            long readerSize = reader.GetBytes(0, 0, null, 0, 0);
            while ((bytesRead = (int)reader.GetBytes(col, offset, buffer, 0, BUFFER_SIZE)) > 0) {
                for (int i = 0; i < bytesRead; i += 4) {
                    int rowId = BitConverter.ToInt32(buffer, i);
                    rowIdValues.Add(rowId, valueHash);
                }
                offset += bytesRead;
                if (offset >= readerSize) break;
            }
        }

        #endregion [ IsCompositeKeyIdx - 1 ]

        #region [ IsCompositeKeyIdx - 2 ]

        ///// <summary>
        ///// Checks whether the given composite key is unique or not
        ///// </summary>
        ///// <param name="keySearchParameter">The KeySearchParameters object</param>
        ///// <param name="candidateKey">The candidate composite key</param>
        ///// <param name="dbConfigIdx">The db config</param>
        ///// <returns>True if the key candidate is key in the table</returns>
        //private bool IsCompositeKeyIdx2(KeySearchParameter keySearchParameter, KeyCandidate candidateKey, DbConfig dbConfigIdx) { 
            
        //    // TODO: make it work with composite keys with more than 2 columns
        //    if (candidateKey.CandidateColumns.Count > 2)
        //        throw new NotImplementedException("The current approach is only working for composite keys with at most 2 columns so far!");

        //    bool retVal = false;

        //    DateTime started = DateTime.Now;
        //    Dictionary<int, List<int>> firstColRowIds = new Dictionary<int, List<int>>();
        //    Dictionary<int, int> nextColRowIds =new Dictionary<int, int>();

        //    using (var idxConn = ConnectionManager.CreateConnection(dbConfigIdx)) {
        //        idxConn.Open();
        //        idxConn.SetHighTimeout();
        //        firstColRowIds = GetIdxRowIdsByIdxId(idxConn, candidateKey.CandidateColumns[0]);
        //        // if no values found which appearc more than once in this column, it means this is a simple key and then return false (this column shoul be found as simple key)
        //        if (firstColRowIds.Count == 0) return false;
        //        nextColRowIds = GetIdxIdsByRowId(idxConn, candidateKey.CandidateColumns[1]);
        //        idxConn.Close();
        //    }

        //    bool isKey = true;
        //    foreach (KeyValuePair<int, List<int>> colRowIds in firstColRowIds) {
        //        int firstIdxId = colRowIds.Key;
        //        Dictionary<int, int> nextIds = new Dictionary<int, int>();
        //        foreach (int colRowId in colRowIds.Value) {
        //            int nextIdxId;
        //            if (nextColRowIds.TryGetValue(colRowId, out nextIdxId)) {
        //                if (nextIds.ContainsKey(nextIdxId)) {
        //                    isKey = false;
        //                    break;
        //                } else {
        //                    nextIds.Add(nextIdxId, nextIdxId);
        //                }
        //            } else {
        //                isKey = false;
        //                break;
        //            }
        //        }
        //        if (!isKey) break;
        //    }

        //    if (isKey) {
        //        DateTime finished = DateTime.Now;
        //        keySearchParameter.KeyCollectorInstance.AddKey(candidateKey, started, finished);
        //        retVal = true;
        //    }

        //    return retVal;
        //}

        /// <summary>
        /// Checks whether the given composite key is unique or not
        /// </summary>
        /// <param name="keySearchParameter">The KeySearchParameters object</param>
        /// <param name="candidateKey">The candidate composite key</param>
        /// <param name="dbConfigIdx">The db config</param>
        /// <returns>True if the key candidate is key in the table</returns>
        private bool IsCompositeKeyIdx2(KeySearchParameter keySearchParameter, KeyCandidate candidateKey, DbConfig dbConfigIdx) { 
            
            // TODO: make it work with composite keys with more than 2 columns
            if (candidateKey.CandidateColumns.Count > 2)
                throw new NotImplementedException("The current approach is only working for composite keys with at most 2 columns so far!");

            bool retVal = false;

            DateTime started = DateTime.Now;
            bool isKey = true;
            Dictionary<int, List<int>> firstColRowIds = null;

            using (RowIdEnumerator rowIdEnumerator = new RowIdEnumerator(dbConfigIdx, candidateKey.CandidateColumns[0])) {
                while (rowIdEnumerator.GetRowIds(out firstColRowIds)) {
                    Dictionary<int, int> idxIdsnextCol = null;
                    using(IdxIdEnumerator idxIdEnumerator = new IdxIdEnumerator(dbConfigIdx, candidateKey.CandidateColumns[1])) {
                        while (idxIdEnumerator.GetIdxIds(out idxIdsnextCol)) {

                            foreach (KeyValuePair<int, List<int>> colRowIds in firstColRowIds) {
                                Dictionary<int, int> nextIds = new Dictionary<int, int>();
                                foreach (int colRowId in colRowIds.Value) {
                                    int nextIdxId;
                                    if (idxIdsnextCol.TryGetValue(colRowId, out nextIdxId)) {
                                        if (nextIds.ContainsKey(nextIdxId)) {
                                            isKey = false;
                                            break;
                                        } else {
                                            nextIds.Add(nextIdxId, nextIdxId);
                                        }
                                    } else {
                                        isKey = false;
                                        break;
                                    }
                                }
                                if (!isKey) break;
                            }

                        }
                    }
                }
            }

            if (isKey) {
                DateTime finished = DateTime.Now;
                keySearchParameter.KeyCollectorInstance.AddKey(candidateKey, started, finished);
                retVal = true;
            }

            return retVal;
        }

        #endregion [ IsCompositeKeyIdx - 2 ]

        #region [ Other methods ]

        /// <summary>
        /// Set the treshold (row number limit)
        /// </summary>
        /// <param name="keyCandidates">Collection of the key candidates</param>
        private void SetTreshold(IEnumerable<KeyCandidate> keyCandidates){
            if (!keyCandidates.Any()) return;
            //Calculate the treshold value, which depends on the caller method
            switch (tresholdRowsMethod){
                case KeySearchParameter.TresholdRowsSelector.AVG:
                    tresholdRowsCount = keyCandidates.Sum(k => k.NumberOfRows) / keyCandidates.Count();
                    break;
                case KeySearchParameter.TresholdRowsSelector.MEDIAN:
                    tresholdRowsCount = keyCandidates.OrderBy(key => key.NumberOfRows).ElementAt(keyCandidates.Count() / 2).NumberOfRows;
                    break;
                case KeySearchParameter.TresholdRowsSelector.NONE:
                default:
                    tresholdRowsCount = 0;
                    break;
            }
        }

        #endregion [ Other methods ]

        #endregion [ Private methods ]
    }
}
