using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using DbAccess;
using DbAccess.Structures;
using DbSearchLogic.Config;
using System.Linq;
using log4net;
using LogManager = AvdCommon.Logging.LogManager;
using AV.Log;

namespace DbSearchLogic.SearchCore.KeySearch{
    /// <summary>
    /// The base KeySearcher
    /// </summary>
    public abstract class KeySearcherBase {

        #region [ Constants ]

        /// <summary>
        /// The skipped (eliminate) column name
        /// </summary>
        public const string SKIPPED_COLUMN_NAME = "_row_no_";

        #endregion [ Constants ]

        #region [ Internal fields ]

        internal ILog _log = LogHelper.GetLogger();

        /// <summary>
        /// The profilye object
        /// </summary>
        internal Profile currentProfile;

        /// <summary>
        /// The action for getting all columns as a key candidate
        /// </summary>
        internal Func<string, IEnumerable<KeyCandidate>> actionGetAllColumnsAsKey;

        /// <summary>
        /// The action for getting all column combinations as a key candidate
        /// </summary>
        internal Func<IEnumerable<KeyCandidate>, int, IEnumerable<KeyCandidate>> actionCreateKeyCombinations;

        /// <summary>
        /// The simple key candidates
        /// </summary>
        internal ConcurrentQueue<KeyCandidate> simpleKeyCandidates = new ConcurrentQueue<KeyCandidate>();

        /// <summary>
        /// The composite key candidates
        /// </summary>
        internal ConcurrentQueue<KeyCandidate> compositeKeyCandidates = new ConcurrentQueue<KeyCandidate>();
        
        /// <summary>
        /// The program running time
        /// </summary>
        internal Stopwatch time;

        /// <summary>
        /// The tested values collector
        /// </summary>
        internal ProgramInfos programInfo;

        /// <summary>
        /// The struct of tested data
        /// </summary>
        public struct ProgramInfos {
            /// <summary>
            /// The checked simple key count
            /// </summary>
            public string SimpleKeyCount;
            /// <summary>
            /// The checked composite key count
            /// </summary>
            public string CompositeKeyCount;
            /// <summary>
            /// The tested host name
            /// </summary>
            public string HostName;
            /// <summary>
            /// The tested DB name
            /// </summary>
            public string DbName;
        }
        
        #endregion [ Internal  fields ]

        #region [ Protected  fields ]

        protected string logPrefix = "Key search base";
        protected bool simpleKeysInitialized = false;
        protected bool compositeKeysInitialized = false;

        #endregion [ Protected  fields ]

        #region [ Public abstract methods ]
        
        /// <summary>
        /// Init the composite key search
        /// </summary>
        /// <param name="keySearchParameter">The parameters</param>
        /// <returns>True if key search initialized and can be performed</returns>
        public abstract bool InitCompositeKeySearch(KeySearchParameter keySearchParameter);
        
        #endregion [ Public abstract methods ]

        #region [ Public virtual methods ]

        /// <summary>
        /// Init the simple key search
        /// </summary>
        /// <param name="keySearchParameter">The parameters</param>
        /// <param name="actionRecreateTables">The method which recreates the tables</param>
        /// <returns>True if key search initialized and can be performed</returns>
        public virtual bool InitSimpleKeySearch(KeySearchParameter keySearchParameter, Func<bool> actionRecreateTables) {

            keySearchParameter.InitiatedKeySearch.StatusDescription = "Step 1: Simple primary key search initialization";
            simpleKeyCandidates = new ConcurrentQueue<KeyCandidate>();
            compositeKeyCandidates = new ConcurrentQueue<KeyCandidate>();
            keySearchParameter.KeyCollectorInstance.EraseKeys();
            keySearchParameter.KeyCollectorInstance.CreateKeys(keySearchParameter.TresholdDbSave, currentProfile);
            time = Stopwatch.StartNew();

            _log.Log(LogLevelEnum.Info, logPrefix + " - initialization started.", true);
            _log.Log(LogLevelEnum.Info, "Parameter - Degree of parallelism: " + keySearchParameter.DegreeOfParallelism, true);
            _log.Log(LogLevelEnum.Info, "Parameter - Row count treshold: " + keySearchParameter.TresholdRowsCount, true);
            _log.Log(LogLevelEnum.Info, "Parameter - Treshold method: " + keySearchParameter.TresholdRowsMethod.ToString(), true);

            if (currentProfile == null) return false;
            if (currentProfile.TableInfoManager == null) return false;

            //Load the default profile's table
            if (currentProfile.TableInfoManager.Tables.Count == 0)
                currentProfile.TableInfoManager.LoadTableInfos();

            //Set the countinue values from database
            if (!SetCountinueValuesFromDb(keySearchParameter)) {
                _log.Log(LogLevelEnum.Info, string.Format("The key search for database {0} already have run.", programInfo.DbName, true));
                if (!keySearchParameter.CancellationToken.IsCancellationRequested && keySearchParameter.InitiatedKeySearch.TaskStatus == KeySearchResult.Running) {
                    if (!actionRecreateTables()) return false;
                }
                else
                    return false;
            }

            bool continueProcessing = false;
            if (simpleKeyCandidates.Count == 0 && compositeKeyCandidates.Count == 0)
                InitKeyCandidates(keySearchParameter);
            else
                continueProcessing = true;

            if (!continueProcessing) {
                if (keySearchParameter.CancellationToken.IsCancellationRequested) return false;
                _log.Log(LogLevelEnum.Info, logPrefix + " - saving simple key candidates started.", true);
                keySearchParameter.KeyCollectorInstance.SaveCandidateKeys(simpleKeyCandidates);
                _log.Log(LogLevelEnum.Info, logPrefix + " - saving simple key candidates finished.", true);
                if (keySearchParameter.CancellationToken.IsCancellationRequested) return false;
                _log.Log(LogLevelEnum.Info, logPrefix + " - saving composite key candidates started.", true);
                keySearchParameter.KeyCollectorInstance.SaveCandidateKeys(compositeKeyCandidates);
                _log.Log(LogLevelEnum.Info, logPrefix + " - saving composite key candidates finished.", true);
            }

            keySearchParameter.InitiatedKeySearch.NumberOfKeyCandidates = simpleKeyCandidates.Count + compositeKeyCandidates.Count;
            _log.Log(LogLevelEnum.Info, logPrefix + " - initialization finished!", true);

            simpleKeysInitialized = true;
            return true;
        }

        /// <summary>
        /// The starts the simple key search
        /// </summary>
        /// <param name="keySearchParameter">The KeySearchParameter object</param>
        public virtual void StartSimpleKeySearch(KeySearchParameter keySearchParameter) {

            keySearchParameter.InitiatedKeySearch.StatusDescription = "Step 2: Simple primary key search";
            const string description = "simple key search";
            int threadNumber = Thread.CurrentThread.ManagedThreadId;

            _log.Log(LogLevelEnum.Info, logPrefix + " - " + description + " started on thread " + threadNumber.ToString() + " ...", true);
            if (!simpleKeysInitialized) {
                _log.Log(LogLevelEnum.Info, logPrefix + " - " + description + " was not initialized!", true);
                return;
            }

            if (currentProfile == null) return;
            double lastLogStatus = 0;
            int initialKeyCandidateCount = simpleKeyCandidates.Count;
            //Get the connection from the profile object
            DbConfig dbConfigIdx = (DbConfig)(currentProfile.DbConfigView).Clone();
            dbConfigIdx.DbName = KeySearchManager.GetIdxDbName(dbConfigIdx);

            while (simpleKeyCandidates.Count > 0) {
                if (keySearchParameter.CancellationToken.IsCancellationRequested) return;

                KeyCandidate candidateKey = null;
                if (simpleKeyCandidates.TryDequeue(out candidateKey)) {
                        
                    if (!candidateKey.Processed && candidateKey.NumberOfRows > 0) {
                        if (IsSimpleKey(keySearchParameter, candidateKey, dbConfigIdx)) {
                            keySearchParameter.InitiatedKeySearch.KeyFound();
                            _log.Log(LogLevelEnum.Info, "Simple key found: " + candidateKey.DisplayString, true);
                        }
                        else
                            keySearchParameter.InitiatedKeySearch.KeyProcessed();
                    }
                    else if (!candidateKey.Processed) keySearchParameter.InitiatedKeySearch.KeyProcessed();
                    keySearchParameter.KeyCollectorInstance.DeleteKeys<KeyCandidate>(currentProfile.DbConfigView, candidateKey);
                    LogProcessingStatus(simpleKeyCandidates.Count, initialKeyCandidateCount, ref lastLogStatus, description);
                }
            }
            
            dbConfigIdx = null;
            GC.Collect();

            _log.Log(LogLevelEnum.Info, logPrefix + " - " + description + " finished on thread " + threadNumber.ToString(), true);
        }

        /// <summary>
        /// The starts the composite key search
        /// </summary>
        /// <param name="keySearchParameter">The KeySearchParameters object</param>
        public virtual void StartCompositeKeySearch(KeySearchParameter keySearchParameter) {

            keySearchParameter.InitiatedKeySearch.StatusDescription = "Step 4: Composite primary key search";
            const string description = "composite key search";
            int threadNumber = System.Threading.Thread.CurrentThread.ManagedThreadId;
            System.Threading.CancellationToken cancellationToken = keySearchParameter.CancellationToken;

            _log.Log(LogLevelEnum.Info, logPrefix + " - " + description + " started on thread " + threadNumber.ToString() + " ...", true);
            if (!compositeKeysInitialized) {
                _log.Log(LogLevelEnum.Info, logPrefix + " - " + description + " was not initialized!", true);
                return;
            }

            if (currentProfile == null) return;
            double lastLogStatus = 0;
            int initialKeyCandidateCount = compositeKeyCandidates.Count;

            while (compositeKeyCandidates.Count > 0) {
                if (cancellationToken.IsCancellationRequested) return;

                KeyCandidate candidateKey = null;
                if (compositeKeyCandidates.TryDequeue(out candidateKey)) {
                        
                    if (!candidateKey.Processed && candidateKey.NumberOfRows > 0) {
                        if (IsCompositeKey(keySearchParameter, candidateKey, currentProfile.DbConfigView)) {
                            keySearchParameter.InitiatedKeySearch.KeyFound();
                            _log.Log(LogLevelEnum.Info, "Composite key found: " + candidateKey.DisplayString, true);
                        }
                        else
                            keySearchParameter.InitiatedKeySearch.KeyProcessed();
                    }
                    else if (!candidateKey.Processed) keySearchParameter.InitiatedKeySearch.KeyProcessed();
                    keySearchParameter.KeyCollectorInstance.DeleteKeys<KeyCandidate>(currentProfile.DbConfigView, candidateKey);
                    LogProcessingStatus(compositeKeyCandidates.Count, initialKeyCandidateCount, ref lastLogStatus, description);
                }
            }

            _log.Log(LogLevelEnum.Info, logPrefix + " - " + description + " finished on thread " + threadNumber.ToString(), true);
        }

        /// <summary>
        /// The key searching is cancelled
        /// </summary>
        public virtual void LogProcessingStatus(int actualKeyCandidateCount, int initialKeyCandidateCount, ref double lastLogStatus, string description) {
            double logStatus = Math.Round(100 - (((double)actualKeyCandidateCount / (double)initialKeyCandidateCount) * 100), 1);
            //logStatus = (int)logStatus;
            if (logStatus != lastLogStatus) {
                _log.Log(LogLevelEnum.Info, string.Format("{0} - {3} {1} % done. (after {2} seconds)", logPrefix, logStatus, time.Elapsed.Seconds, description), true);
                lastLogStatus = logStatus;
            }
        }

        /// <summary>
        /// The key search is cancelled
        /// </summary>
        public virtual void CancelKeySearch(){
            _log.Log(LogLevelEnum.Info, "Key search cancelled! " + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString(), true);
        }

        /// <summary>
        /// The key search is started
        /// </summary>
        public virtual void KeySearchStarted() {
            _log.Log(LogLevelEnum.Info, "Key search started. " + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString(), true);
            _log.Log(LogLevelEnum.Info, "Host name: " + programInfo.HostName, true);
            _log.Log(LogLevelEnum.Info, "Database name: " + programInfo.DbName, true);
        }

        /// <summary>
        /// The key search is finished
        /// </summary>
        /// <param name="keySearchParameter">The parameters</param>
        public virtual void KeySearchFinished(KeySearchParameter keySearchParameter)
        {
            time.Stop();
            _log.Log(LogLevelEnum.Info, "Processed single key candidates: " + programInfo.SimpleKeyCount, true);
            _log.Log(LogLevelEnum.Info, "Processed composite key candidates: " + programInfo.CompositeKeyCount, true);

            _log.Log(LogLevelEnum.Info, "Simple keys found: " + keySearchParameter.KeyCollectorInstance.TableKeys.Sum(tk => tk.Value.Count(k => k.Value.KeyColumns.Count == 1)), true);
            _log.Log(LogLevelEnum.Info, "Composite keys found: " + keySearchParameter.KeyCollectorInstance.TableKeys.Sum(tk => tk.Value.Count(k => k.Value.KeyColumns.Count > 1)), true);

            _log.Log(LogLevelEnum.Info, "Running time: " + time.Elapsed.Hours + " hour " + time.Elapsed.Minutes + " min " + time.Elapsed.Seconds + " sec [" + time.ElapsedTicks + " ticks]", true);
            _log.Log(LogLevelEnum.Info, "Key search finished! " + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString(), true);
        }

        /// <summary>
        /// Gets whether the key search is finished already run
        /// <param name="keySearchParameter">The parameters</param>
        /// </summary>
        public virtual bool IsKeySearchFinished(KeySearchParameter keySearchParameter) {
            //If the key table exists and the CheckedCompositeKeyItem table doesn't exist then key search finished and there is nothing to continue with
            //if (KeyCollector.Instance.ExistTableKeys<Key>(currentProfile.DbConfigView) && !KeyCollector.Instance.ExistTableKeys<KeyCandidate>(currentProfile.DbConfigView)) return true;
            if (keySearchParameter.KeyCollectorInstance.ExistTable<Key>(currentProfile.DbConfigView)) {
                if (keySearchParameter.KeyCollectorInstance.ExistTable<KeyCandidate>(currentProfile.DbConfigView))
                    if (keySearchParameter.KeyCollectorInstance.LoadKeys<KeyCandidate>(currentProfile.DbConfigView).Count > 0)
                        return false;
                    else if (keySearchParameter.KeyCollectorInstance.LoadKeys<Key>(currentProfile.DbConfigView).Count > 0)
                        return true;
                    else
                        return false;
                else if (keySearchParameter.KeyCollectorInstance.LoadKeys<Key>(currentProfile.DbConfigView).Count > 0)
                        return true;
            }
            return false;
        }

        #endregion [ Public virtual methods ]

        #region [ Private methods ]
        
        /// <summary>
        /// Set the collections with remaining key candidates from previous interrupted run
        /// </summary>
        /// <param name="keySearchParameter">The parameters</param>
        /// <returns>true - the datatables are exists
        ///          false - doesn't exists</returns>
        private bool SetCountinueValuesFromDb(KeySearchParameter keySearchParameter) {

            if (IsKeySearchFinished(keySearchParameter)) return false;

            keySearchParameter.KeyCollectorInstance.CreateDatabases(currentProfile.DbConfigView);
            List<KeyCandidate> remainingKeys = keySearchParameter.KeyCollectorInstance.LoadKeys<KeyCandidate>(currentProfile.DbConfigView);
            if (remainingKeys.Count > 0) {

                _log.Log(LogLevelEnum.Info, logPrefix + " - continue with preprocessed simple and composite key candidates.", true);

                foreach (KeyCandidate key in remainingKeys) {
                    if (key.CandidateColumns.Count == 1)
                        simpleKeyCandidates.Enqueue(key);
                    else
                        compositeKeyCandidates.Enqueue(key);
                }
            }
            return true;
        }

        /// <summary>
        /// Checks whether the given key is unique or not
        /// </summary>
        /// <param name="keySearchParameter">The parameters</param>
        /// <param name="candidateKey">The key to check</param>
        /// <param name="dbConfigIdx">The db config</param>
        /// <returns>True if the key candidate is key in the table</returns>
        private bool IsSimpleKey(KeySearchParameter keySearchParameter, KeyCandidate candidateKey, DbConfig dbConfigIdx)
        {
            bool retVal = false;

            DateTime started = DateTime.Now;

            if (candidateKey.NumberOfRows == 0) return false;
            long idxTableRowCount = -1;
            using (var idxConn = ConnectionManager.CreateConnection(dbConfigIdx)) {
                idxConn.Open();
                idxConn.SetHighTimeout();
                //if (idxConn.TableExists("idx_" + candidateKey.CandidateColumns[0].ColumnId))
                //    idxTableRowCount = idxConn.CountTable("idx_" + candidateKey.CandidateColumns[0].ColumnId);
                try {
                    idxTableRowCount = idxConn.CountTable("idx_" + candidateKey.CandidateColumns[0].ColumnId);
                } catch (Exception) {
                    // idx table not exists
                    return false;
                }
                idxConn.Close();
            }
            // if the row count of the table equals the row count of the distinct idx table it means that all values of this column is unique
            if (idxTableRowCount == candidateKey.NumberOfRows) {
                candidateKey.NumberOfRowsIdx = idxTableRowCount;
                // set processed = true in case of keys which contains the simple key above as member
                foreach (KeyCandidate compositeKeyCandidate in compositeKeyCandidates.Where(kc => kc.TableId == candidateKey.TableId)) {
                    if (compositeKeyCandidate.CandidateColumns.Any(c => c.ColumnId == candidateKey.CandidateColumns[0].ColumnId)) {
                        compositeKeyCandidate.Processed = true;
                        keySearchParameter.InitiatedKeySearch.KeyProcessed();
                        keySearchParameter.KeyCollectorInstance.DeleteKeys<KeyCandidate>(currentProfile.DbConfigView, compositeKeyCandidate);
                    }
                }
                DateTime finished = DateTime.Now;
                keySearchParameter.KeyCollectorInstance.AddKey(candidateKey, started, finished);
                retVal = true;
            }
            return retVal;
        }

        /// <summary>
        /// Checks whether the given composite key is unique or not
        /// </summary>
        /// <param name="keySearchParameter">The parameters</param>
        /// <param name="candidateKey">The candidate composite key</param>
        /// <param name="dbConfig">The database config</param>
        /// <returns>True if the key candidate is key in the table</returns>
        private bool IsCompositeKey(KeySearchParameter keySearchParameter, KeyCandidate candidateKey, DbConfig dbConfig) {
            bool retVal = false;

            DateTime started = DateTime.Now;

            if (candidateKey.NumberOfRows == 0) return false;
            if (keySearchParameter.KeyCollectorInstance.IsExists(candidateKey)) return false;
            if (candidateKey.CandidateColumns.Any(c => c.ColumnName.ToLower() == SKIPPED_COLUMN_NAME)) return false;
            //if (key.Columns.Any(c => string.IsNullOrEmpty(c))) return retVal;

            int numberOfRowsDistinct = -1;
            using (IDatabase conn = ConnectionManager.CreateConnection(dbConfig)) {
                conn.Open();
                conn.SetHighTimeout();
                string columns = string.Join(", ", candidateKey.CandidateColumns.Select(column => conn.Enquote(column.ColumnName)));
                string sql = string.Format("SELECT COUNT(DISTINCT CONCAT({0})) FROM {1}", columns, conn.DbConfig.DbName + "." + candidateKey.TableName);
                numberOfRowsDistinct = Convert.ToInt32(conn.ExecuteScalar(sql));
                conn.Close();
            }
            if (numberOfRowsDistinct == candidateKey.NumberOfRows) {
                DateTime finished = DateTime.Now;
                keySearchParameter.KeyCollectorInstance.AddKey(candidateKey, started, finished);
                retVal = true;
            }
            return retVal;
        }

        /// <summary>
        /// Init the key candidate collections
        /// </summary>
        /// <param name="keySearchParameter">The parameters</param>
        /// <returns>The number of possible keys</returns>
        public virtual void InitKeyCandidates(KeySearchParameter keySearchParameter) {
            ConcurrentQueue<KeyCandidate> simpleKeys = new ConcurrentQueue<KeyCandidate>();
            ConcurrentQueue<KeyCandidate> compositeKeys = new ConcurrentQueue<KeyCandidate>();
            //List<KeyCandidate> simpleKeys = new List<KeyCandidate>();
            //List<KeyCandidate> compositeKeys = new List<KeyCandidate>();
            //foreach (TableInfo tableInfo in currentProfile.TableInfoManager.Tables) {
            //    if (keySearchParameter.CancellationToken.IsCancellationRequested) return;
            //    List<KeyCandidate> tableSimpleKeys = new List<KeyCandidate>(actionGetAllColumnsAsKey(tableInfo.Name));
            //    simpleKeys.AddRange(tableSimpleKeys);
            //    List<KeyCandidate> tableCompositeKeys = new List<KeyCandidate>(actionCreateKeyCombinations(tableSimpleKeys, keySearchParameter.DegreeOfKeyComplexity));
            //    compositeKeys.AddRange(tableCompositeKeys);
            //    LogInitStatus(tableInfo.Name);
            //}
            int tableCount = currentProfile.TableInfoManager.Tables.Count;
            int processed = 0;
            ParallelOptions options = new ParallelOptions() { MaxDegreeOfParallelism = keySearchParameter.DegreeOfParallelism };
            Parallel.ForEach(currentProfile.TableInfoManager.Tables, options, tableInfo => {
                if (keySearchParameter.CancellationToken.IsCancellationRequested) return;
                List<KeyCandidate> tableSimpleKeys = new List<KeyCandidate>(actionGetAllColumnsAsKey(tableInfo.Name));
                foreach (KeyCandidate tableSimpleKey in tableSimpleKeys) {
                    simpleKeys.Enqueue(tableSimpleKey);
                }
                List<KeyCandidate> tableCompositeKeys = new List<KeyCandidate>(actionCreateKeyCombinations(tableSimpleKeys, keySearchParameter.DegreeOfKeyComplexity));
                foreach (KeyCandidate tableCompositeKey in tableCompositeKeys) {
                    compositeKeys.Enqueue(tableCompositeKey);
                }
                Interlocked.Increment(ref processed);
                LogInitStatus(tableInfo.Name, tableCount, processed);
            });
            //foreach (TableInfo tableInfo in currentProfile.TableInfoManager.Tables)
            //{
            //    if (keySearchParameter.CancellationToken.IsCancellationRequested) return;
            //    List<KeyCandidate> tableSimpleKeys = new List<KeyCandidate>(actionGetAllColumnsAsKey(tableInfo.Name));
            //    foreach (KeyCandidate tableSimpleKey in tableSimpleKeys) {
            //        simpleKeys.Enqueue(tableSimpleKey);
            //    }
            //    List<KeyCandidate> tableCompositeKeys = new List<KeyCandidate>(actionCreateKeyCombinations(tableSimpleKeys, keySearchParameter.DegreeOfKeyComplexity));
            //    foreach (KeyCandidate tableCompositeKey in tableCompositeKeys) {
            //        compositeKeys.Enqueue(tableCompositeKey);
            //    }
            //    LogInitStatus(tableInfo.Name);
            //}
            simpleKeyCandidates = simpleKeys;
            compositeKeyCandidates = compositeKeys;
            
            //Save the init values
            programInfo.SimpleKeyCount = simpleKeyCandidates.Count.ToString();
            programInfo.CompositeKeyCount = compositeKeyCandidates.Count.ToString();
        }

        /// <summary>
        /// The key searching is cancelled
        /// </summary>
        public virtual void LogInitStatus(string tableName, int tableCount, int processed) {
            _log.Log(LogLevelEnum.Info, string.Format("{0} - initializing table {1} done. ({2:0.00} %)", logPrefix, tableName, (((double)processed / (double)tableCount)) * 100), true);
        }

        #endregion [ Private methods ]
    }
}
