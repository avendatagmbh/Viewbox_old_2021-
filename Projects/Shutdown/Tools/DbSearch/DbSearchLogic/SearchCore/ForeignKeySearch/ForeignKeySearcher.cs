using System;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml.Linq;
using DbAccess;
using DbAccess.Structures;
using DbSearchBase.Interfaces;
using DbSearchLogic.Config;
using DbSearchLogic.SearchCore.KeySearch;
using System.Collections.Concurrent;
using AV.Log;
using log4net;
using LogManager = AvdCommon.Logging.LogManager;

namespace DbSearchLogic.SearchCore.ForeignKeySearch{
    /// <summary>
    /// The Foriegn key searching class
    /// </summary>
    public class ForeignKeySearcher{

        #region [ Members ]

        internal ILog _log = LogHelper.GetLogger();

        /// <summary>
        /// The profilye object
        /// </summary>
        private Profile _currentProfile;

        /// <summary>
        /// The primary keys (detected by key search) which can be the referenced primary keys by BaseTableColumns
        /// </summary>
        private ConcurrentQueue<Key> _primaryKeys;
        
        /// <summary>
        /// The primary keys (detected by key search) which can be the referenced primary keys by BaseTableColumns
        /// </summary>
        private ConcurrentQueue<KeyCandidate> _simpleKeyCandidates;

        /// <summary>
        /// The primary keys (detected by key search) which can be the referenced primary keys by BaseTableColumns
        /// </summary>
        private ConcurrentQueue<KeyCandidate> _compositeKeyCandidates;

        /// <summary>
        /// The columns (column permutations) of the base table which the foreign key candidates
        /// </summary>
        private ConcurrentQueue<KeyCandidate> _baseTableColumns;

        /// <summary>
        /// The primary keys (detected by key search) which can be referenced
        /// </summary>
        private ConcurrentQueue<Key> _baseTableKeys;

        /// <summary>
        /// The columns (column permutations) of the base table which the foreign key candidates
        /// </summary>
        private ConcurrentQueue<KeyCandidate> _foreignKeyCandidates;

        /// <summary>
        /// Get the keys queue count
        /// </summary>
        private decimal _keysCount;
            
        /// <summary>
        /// Get the keys queue count
        /// </summary>
        private decimal _percentOfProcess;

        /// <summary>
        /// Get the current percent of the process
        /// </summary>
        private decimal _currentPercentOfProcess;

        /// <summary>
        /// Get the current ForeignKeySearchingParameter object
        /// </summary>
        private ForeignKeySearchParameter _foreignKeySearchParameter;

        /// <summary>
        /// Get the current search manager instance
        /// </summary>
        private KeySearchManager _searchManager;
        
        #endregion [ Members ]

        #region [ Constructor ]

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="profile">The selected profile</param>
        /// <param name="foreignKeySearchParameter">The ForeignKeySearchParameter object</param>
        public ForeignKeySearcher(Profile profile, ForeignKeySearchParameter foreignKeySearchParameter, KeySearchManager searchManager) {
            //Initialize the variables
            _searchManager = searchManager;
            _currentProfile = profile;
            _foreignKeySearchParameter = foreignKeySearchParameter;
            _primaryKeys = foreignKeySearchParameter.PrimaryKeys;
            _baseTableColumns = foreignKeySearchParameter.BaseTableColumns;
            _baseTableKeys = foreignKeySearchParameter.BaseTableKeys;
            _foreignKeyCandidates = foreignKeySearchParameter.ForeignKeyCandidates;
            
            //Create the Collector instance -> init
            foreignKeySearchParameter.ForeignKeyCollectorInstance.CreateKeys(0, _currentProfile);
            foreignKeySearchParameter.ForeignKeyCollectorInstance.EraseKeys();
                     
            //Create the datatables
            foreignKeySearchParameter.ForeignKeyCollectorInstance.CreateDatabases(profile.DbConfigView);
        }

        #endregion [ Constructor ]

        #region [ Public methods ]
        /// <summary>
        /// Initialize simple foreign key search
        /// </summary>
        /// <returns>true - the FK search will be started
        ///          false - there isn't any comparter values</returns>
        public bool InitSimpleForeignKeySearch(IEnumerable<KeyCandidate> baseTableColumns) {

            _foreignKeySearchParameter.InitiatedKeySearch.StatusDescription = "Step 1: Simple foreign key search initialization";
            _baseTableColumns = new ConcurrentQueue<KeyCandidate>(baseTableColumns);
            _keysCount = _baseTableColumns.Count;
            // primary keys
            _simpleKeyCandidates = new ConcurrentQueue<KeyCandidate>(_primaryKeys.AsParallel().Where(k => k.TableId != _foreignKeySearchParameter.BaseTableId && k.KeyColumns.Count == 1).Select(k => new KeyCandidate(k)));
            _percentOfProcess = -1;
            _currentPercentOfProcess = -1;

            _log.Log(LogLevelEnum.Info, "Simple foreign key search started...", true);
            //Log for the user
            IEnumerable<KeyCandidate> candidates = _baseTableColumns.Where(c => c.CandidateColumns.Count == 1);
            if (!candidates.Any()) {
                _log.Log(LogLevelEnum.Info, "No simple foreign key candidates found!", true);
                return false;
            } else {
                foreach (KeyCandidate keyCandidate in candidates)
                    _log.Log(LogLevelEnum.Info, "Simple foreign key candidate: " + keyCandidate.DisplayString, true);
            }
            _foreignKeySearchParameter.InitiatedKeySearch.NumberOfKeyCandidates = _simpleKeyCandidates.Count * candidates.Count();
            return true;
        }

        /// <summary>
        /// Initialize composite foreign key search
        /// </summary>
        /// <returns>true - the CFK searching will be started
        ///          false - there isn't any comparter values</returns>
        public bool InitCompositeForeignKeySearch(IEnumerable<KeyCandidate> columnCombinations) {
            
            _foreignKeySearchParameter.InitiatedKeySearch.StatusDescription = "Step 3: Composite foreign key search initialization";
            _baseTableColumns = new ConcurrentQueue<KeyCandidate>(columnCombinations);
            _keysCount = _baseTableColumns.Count;
            // primary keys
            _compositeKeyCandidates = new ConcurrentQueue<KeyCandidate>(_primaryKeys.AsParallel().Where(k => k.KeyColumns.Count > 1).Select(k => new KeyCandidate(k)));
            _percentOfProcess = -1;
            _currentPercentOfProcess = -1;
            
            if (_baseTableColumns.Count(candidate => candidate.CandidateColumns.Count > 1) == 0) {
                _log.Log(LogLevelEnum.Info, "There are no composite foreign key candidates in [" + _foreignKeySearchParameter.BaseTableName + "] table.", true);
                return false;
            }

            _log.Log(LogLevelEnum.Info, "Composite foreign key search started... ", true);
            //Log for the user
            IEnumerable<KeyCandidate> candidates = _baseTableColumns.AsParallel().Where(candidate => candidate.CandidateColumns.Count > 1).OrderBy(k => k.DisplayString);
            if (candidates.Count() == 0)
                _log.Log(LogLevelEnum.Info, "No composite foreign key candidates found!", true);
            else {
                foreach (KeyCandidate candidate in candidates)
                    _log.Log(LogLevelEnum.Info, "Composite foreign key candidate: " + candidate.DisplayString, true);
            }
            _foreignKeySearchParameter.InitiatedKeySearch.NumberOfKeyCandidates += _compositeKeyCandidates.Count * candidates.Count();
            return true;
        }

        /// <summary>
        /// Start the simple foreign keys search
        /// </summary>
        /// <param name="foreignKeySearchParameter">The ForeignKeySearchParameter obj</param>
        public void StartSimpleForeignKeySearch(ForeignKeySearchParameter foreignKeySearchParameter){
            //Check the loaded profile
            if (_currentProfile == null) {
                _log.Log(LogLevelEnum.Warn, "There is no selected profile! So the foreign key search stopped!", true);
                foreignKeySearchParameter.InitiatedKeySearch.TaskStatus = KeySearchResult.Canceled;
                return;
            }

            foreignKeySearchParameter.InitiatedKeySearch.StatusDescription = "Step 2: Simple foreign key search";

            while (_baseTableColumns.Count != 0) {
                KeyCandidate key;
                if (_baseTableColumns.TryDequeue(out key)) {
                    if (key.CandidateColumns.Count > 1)
                        continue;

                    DateTime started = DateTime.Now;

                    //Show the current alg. percentage
                    _currentPercentOfProcess = Math.Round((1 - (_baseTableColumns.Count / _keysCount)) * 100, 0);
                    if (_percentOfProcess < _currentPercentOfProcess) {
                        _percentOfProcess = _currentPercentOfProcess;
                        _log.Log(LogLevelEnum.Info, "Foreign key search -> " + _percentOfProcess + " % done. [simple keys found: " + foreignKeySearchParameter.ForeignKeyCollectorInstance.ForeignKeys.Count(k => k.Value.ForeignKeyColumns.Count == 1) + "]", true);
                    }

                    ////Check the table if exist
                    //if (!conn.TableExists(KeySearchManager.GetIdxDbName(conn.DbConfig), "idx_" + key.CandidateColumns[0].ColumnId)) {
                    //    _log.LogWithBuffer(LogLevelEnum.Info, "idx_" + key.CandidateColumns[0].ColumnId + " table doesn't exist!");
                    //    continue;
                    //}

                    //Get all possible column occurences
                    foreach (KeyCandidate keyCandidate in _simpleKeyCandidates) {
                        if (foreignKeySearchParameter.CancellationToken.IsCancellationRequested) return;
                        if (key.TableName == keyCandidate.TableName) continue;
                        if (keyCandidate.CandidateColumns.Count > 1) continue;

                        if (IsSimpleKey(_currentProfile.DbConfigView, started, key, keyCandidate, foreignKeySearchParameter)) {
                            foreignKeySearchParameter.InitiatedKeySearch.KeyFound();
                        } else {
                            foreignKeySearchParameter.InitiatedKeySearch.KeyProcessed();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Start the composite foreign keys search
        /// </summary>
        /// <param name="foreignKeySearchParameter">The ForeignKeySearchParameter obj</param>
        public void StartCompositeForeignKeySearch(ForeignKeySearchParameter foreignKeySearchParameter) {
            //Check the loaded profile
            if (_currentProfile == null) {
                _log.Log(LogLevelEnum.Warn, "There is no selected profile! So the foreign key search stopped!", true);
                foreignKeySearchParameter.InitiatedKeySearch.TaskStatus = KeySearchResult.Canceled;
                return;
            }

            foreignKeySearchParameter.InitiatedKeySearch.StatusDescription = "Step 4: Composit foreign key search";

            KeyCandidate key;
            while(_baseTableColumns.Count != 0) {
                if(_baseTableColumns.TryDequeue(out key)) {
                    if (key.CandidateColumns.Count < 2)
                        continue;
                    if (key.Processed)
                        continue;

                    DateTime started = DateTime.Now;

                    //Show the current alg. percentage
                    _currentPercentOfProcess = Math.Round((1 - (_baseTableColumns.Count / _keysCount)) * 100, 0);
                    if(_percentOfProcess < _currentPercentOfProcess){
                        _percentOfProcess = _currentPercentOfProcess;
                        _log.Log(LogLevelEnum.Info, "Foreign key search -> " + _percentOfProcess + " % done. [composite keys found: " + foreignKeySearchParameter.ForeignKeyCollectorInstance.ForeignKeys.Count(k => k.Value.ForeignKeyColumns.Count > 1) + "]", true);
                    }

                    foreach(KeyCandidate keyCandidate in _compositeKeyCandidates) {
                        if (foreignKeySearchParameter.CancellationToken.IsCancellationRequested) return;
                        if (key.TableName == keyCandidate.TableName) continue;
                        if (keyCandidate.CandidateColumns.Count == 1) continue;

                        if (IsCompositeKey(_currentProfile.DbConfigView, started, key, keyCandidate, foreignKeySearchParameter, delegate(ForeignKey fk) {
                            foreach (KeyCandidate k in _baseTableColumns.Where(c => c.ColumnOrderIndependentHash == fk.ColumnOrderIndependentHash)) {
                                k.Processed = true;
                                foreignKeySearchParameter.InitiatedKeySearch.KeyProcessed();
                            }
                        })) {
                            foreignKeySearchParameter.InitiatedKeySearch.KeyFound();
                        }
                        else
                            foreignKeySearchParameter.InitiatedKeySearch.KeyProcessed();
                    }
                }
            }
        }

        /// <summary>
        /// The foreign key search is cancelled
        /// </summary>
        public void CancelledForeignKeySearching() {
            _log.Log(LogLevelEnum.Info, "Foreign key search cancelled...", true);
        }

        /// <summary>
        /// The end of the foreign key search
        /// </summary>
        /// <param name="foreignKeySearchParameter">The ForeignKeySearchParameter obj</param>
        public void ForeignKeySearchFinished(ForeignKeySearchParameter foreignKeySearchParameter) {
            int singleForeignKeysCount = 0;
            int compositeForeignKeysCount = 0;
            //Get the saved foreignKeys from the object
            foreach (ForeignKey foreignKey in foreignKeySearchParameter.ForeignKeyCollectorInstance.ForeignKeys.Values.Where(k => !k.Preloaded).OrderBy(k => k.DisplayString)) {
                _log.Log(LogLevelEnum.Info, "Foreign key found: " + foreignKey.DisplayString, true);
                if (foreignKey.ForeignKeyColumns.Count == 1)
                    singleForeignKeysCount++;
                else if (foreignKey.ForeignKeyColumns.Count > 1)
                    compositeForeignKeysCount++;
            }
            //Stores the checked table if there aren't any foreign key
            if ((singleForeignKeysCount + compositeForeignKeysCount) == 0 && _baseTableColumns.Count > 0 && !foreignKeySearchParameter.ForeignKeyCollectorInstance.ForeignKeys.Any())
                new ForeignKey(_baseTableColumns.ElementAt(0), _baseTableColumns.ElementAt(0)).Save(_currentProfile.DbConfigView);

            _log.Log(LogLevelEnum.Info, "Simple foreign keys found: " + singleForeignKeysCount, true);
            _log.Log(LogLevelEnum.Info, "Composite foreign keys found: " + compositeForeignKeysCount, true);
            _log.Log(LogLevelEnum.Info, "Foreign key search finished, " + (singleForeignKeysCount + compositeForeignKeysCount) + " foreign key(s) found in table " + _foreignKeySearchParameter.BaseTableName, true);
            _log.Log(LogLevelEnum.Info, "Foreign keys / foreign key references for the table in total: " + foreignKeySearchParameter.ForeignKeyCollectorInstance.ForeignKeys.Count.ToString(), true);
        }

        #endregion [ Public methods ]

        #region [ Public methods for revesrse FK search ]

        /// <summary>
        /// Initialize simple reverse foreign key search
        /// </summary>
        /// <returns>true - the FK search will be started</returns>
        public bool InitSimpleForeignKeyForPrimaryKeySearch(ForeignKeySearchParameter foreignKeySearchParameter) {
            _baseTableKeys = new ConcurrentQueue<Key>(foreignKeySearchParameter.BaseTableKeys.AsParallel().Where(k => k.KeyColumns.Count == 1));
            _keysCount = _baseTableKeys.Count;
            _foreignKeyCandidates = foreignKeySearchParameter.ForeignKeyCandidates;
            _simpleKeyCandidates = new ConcurrentQueue<KeyCandidate>(_foreignKeyCandidates.AsParallel().Where(k => k.CandidateColumns.Count == 1));
            _percentOfProcess = -1;
            _currentPercentOfProcess = -1;

            _log.Log(LogLevelEnum.Info, "Simple reverse foreign key search started...", true);

            // reduce the set of key candidates with the already processed keys
            foreignKeySearchParameter.ForeignKeyCollectorInstance.LoadExistingKeysForTable(_currentProfile.DbConfigView, foreignKeySearchParameter.BaseTableId, _searchManager);
            _simpleKeyCandidates = new ConcurrentQueue<KeyCandidate>(_simpleKeyCandidates.AsParallel().Where(c =>
                                        !foreignKeySearchParameter.ForeignKeyCollectorInstance.ForeignKeys.Any(fk => fk.Value.TableId == foreignKeySearchParameter.BaseTableId
                                                || (fk.Value.TableId == c.TableId && fk.Value.ColumnOrderIndependentHash == c.ColumnOrderIndependentHash))));

            if (!_baseTableKeys.Any()) {
                _log.Log(LogLevelEnum.Info, "No simple key candidates found in table " + foreignKeySearchParameter.BaseTableName + " !", true);
                return false;
            }

            IEnumerable<KeyCandidate> candidates = _simpleKeyCandidates;
            if (!candidates.Any()) {
                _log.Log(LogLevelEnum.Info, "No simple foreign key candidates found!", true);
                return false;
            } else {
                foreach (KeyCandidate keyCandidate in candidates)
                    _log.Log(LogLevelEnum.Info, "Simple foreign key candidate: " + keyCandidate.DisplayString, true);
            }
            return true;
        }

        /// <summary>
        /// Initialize the reverse composite foreign key search
        /// </summary>
        /// <returns>true - the FK search will be started</returns>
        public bool InitCompositeForeignKeyForPrimaryKeySearch(ForeignKeySearchParameter foreignKeySearchParameter) {
            _baseTableKeys = new ConcurrentQueue<Key>(foreignKeySearchParameter.BaseTableKeys.AsParallel().Where(k => k.KeyColumns.Count > 1));
            _keysCount = _baseTableKeys.Count;
            _foreignKeyCandidates = foreignKeySearchParameter.ForeignKeyCandidates;
            _compositeKeyCandidates = new ConcurrentQueue<KeyCandidate>(_foreignKeyCandidates.AsParallel().Where(k => k.CandidateColumns.Count > 1));
            _percentOfProcess = -1;
            _currentPercentOfProcess = -1;

            _log.Log(LogLevelEnum.Info, "Composite reverse foreign key search started... ", true);

            if (!_baseTableKeys.Any(candidate => candidate.KeyColumns.Count > 1)) {
                _log.Log(LogLevelEnum.Info, "There are no composite key candidates in [" + _foreignKeySearchParameter.BaseTableName + "] table.", true);
                return false;
            }

            // reduce the set of key candidates with the already processed keys
            foreignKeySearchParameter.ForeignKeyCollectorInstance.LoadExistingKeysForTable(_currentProfile.DbConfigView, foreignKeySearchParameter.BaseTableId, _searchManager);
            _compositeKeyCandidates = new ConcurrentQueue<KeyCandidate>(_compositeKeyCandidates.AsParallel().Where(c =>
                                        !foreignKeySearchParameter.ForeignKeyCollectorInstance.ForeignKeys.Any(fk => fk.Value.TableId == foreignKeySearchParameter.BaseTableId
                                                || (fk.Value.TableId == c.TableId && fk.Value.ColumnOrderIndependentHash == c.ColumnOrderIndependentHash))));

            //Log for the user
            IEnumerable<KeyCandidate> candidates = _compositeKeyCandidates;
            if (candidates.Count() == 0)
                _log.Log(LogLevelEnum.Info, "No composite foreign key candidates found!", true);
            else {
                foreach (KeyCandidate candidate in candidates)
                    _log.Log(LogLevelEnum.Info, "Composite foreign key candidate: " + candidate.DisplayString, true);
            }
            return true;
        }

        /// <summary>
        /// Start the reverse simple foreign keys search
        /// </summary>
        /// <param name="foreignKeySearchParameter">The ForeignKeySearchParameter obj</param>
        public void StartSimpleForeignKeyForPrimaryKeySearher(ForeignKeySearchParameter foreignKeySearchParameter) {
            //Check the loaded profile
            if (_currentProfile == null) {
                _log.Log(LogLevelEnum.Warn, "There is no selected profile! So the foreign key search stopped!", true);
                return;
            }
         
            while (_baseTableKeys.Count != 0) {
                Key key;
                if (_baseTableKeys.TryDequeue(out key)) {
                    if (key.KeyColumns.Count > 1)
                        continue;

                    DateTime started = DateTime.Now;

                    //Show the current alg. percentage
                    _currentPercentOfProcess = Math.Round((1 - (_baseTableColumns.Count / _keysCount)) * 100, 0);
                    if (_percentOfProcess < _currentPercentOfProcess) {
                        _percentOfProcess = _currentPercentOfProcess;
                        _log.Log(LogLevelEnum.Info, "Foreign key search -> " + _percentOfProcess + " % done. [simple keys found: " + foreignKeySearchParameter.ForeignKeyCollectorInstance.ForeignKeys.Count(k => k.Value.ForeignKeyColumns.Count == 1) + "]", true);
                    }

                    ////Check the table if exist
                    //if (!conn.TableExists(KeySearchManager.GetIdxDbName(conn.DbConfig), "idx_" + key.KeyColumns[0].ColumnId)) {
                    //    _log.LogWithBuffer(LogLevelEnum.Info, "idx_" + key.KeyColumns[0].ColumnId + " table doesn't exist!");
                    //    continue;
                    //}

                    //Get all possible column occurences
                    foreach (KeyCandidate keyCandidate in _simpleKeyCandidates) {
                        if (foreignKeySearchParameter.CancellationToken.IsCancellationRequested) return;
                        if (key.TableName == keyCandidate.TableName) continue;
                        if (keyCandidate.CandidateColumns.Count > 1) continue;

                        if (IsSimpleKey(_currentProfile.DbConfigView, started, keyCandidate, key, foreignKeySearchParameter))
                            foreignKeySearchParameter.InitiatedKeySearch.KeyFound();
                        else
                            foreignKeySearchParameter.InitiatedKeySearch.KeyProcessed();
                    }
                }
            }
        }

        /// <summary>
        /// Start the reverse composite foreign keys search
        /// </summary>
        /// <param name="foreignKeySearchParameter">The ForeignKeySearchParameter obj</param>
        public void StartCompositeForeignKeyForPrimaryKeySearch(ForeignKeySearchParameter foreignKeySearchParameter) {
            //Check the loaded profile
            if (_currentProfile == null) {
                _log.Log(LogLevelEnum.Warn, "There is no selected profile! So the foreign key search stopped!", true);
                return;
            }
            
            Key key;
            while (_baseTableKeys.Count != 0) {
                if (_baseTableKeys.TryDequeue(out key)) {
                    if (key.KeyColumns.Count < 2)
                        continue;
                    if (key.Processed)
                        continue;

                    DateTime started = DateTime.Now;

                    //Show the current alg. percentage
                    _currentPercentOfProcess = Math.Round((1 - (_baseTableColumns.Count / _keysCount)) * 100, 0);
                    if (_percentOfProcess < _currentPercentOfProcess) {
                        _percentOfProcess = _currentPercentOfProcess;
                        _log.Log(LogLevelEnum.Info, "Foreign key search -> " + _percentOfProcess + " % done. [composite keys found: " + foreignKeySearchParameter.ForeignKeyCollectorInstance.ForeignKeys.Count(k => k.Value.ForeignKeyColumns.Count > 1) + "]", true);
                    }

                    foreach (KeyCandidate keyCandidate in _compositeKeyCandidates) {
                        if (foreignKeySearchParameter.CancellationToken.IsCancellationRequested) return;
                        if (key.TableName == keyCandidate.TableName) continue;
                        if (keyCandidate.CandidateColumns.Count == 1) continue;

                        if (IsCompositeKey(_currentProfile.DbConfigView, started, keyCandidate, key, foreignKeySearchParameter, delegate(ForeignKey fk) {
                            foreach (Key k in _baseTableKeys.Where(c => c.ColumnOrderIndependentHash == fk.ColumnOrderIndependentHash)) {
                                k.Processed = true;
                                foreignKeySearchParameter.InitiatedKeySearch.KeyProcessed();
                            }
                        }))
                            foreignKeySearchParameter.InitiatedKeySearch.KeyFound();
                        else
                            foreignKeySearchParameter.InitiatedKeySearch.KeyProcessed();
                    }
                }
            }
        }

        #endregion [ Public methods for revesrse FK search ]

        #region [ Private methods ]

        private bool IsColumnTypesMatch(IKey key, IKey keyCandidate) {
            bool match = true;
            if (!(key.Columns.Count > 0 && keyCandidate.Columns.Count == key.Columns.Count))
                return false;
            for (int i = 0; i < key.Columns.Count; i++) {
                if (key.Columns[i].ColumnType != keyCandidate.Columns[i].ColumnType) {
                    match = false;
                    break;
                }
            }
            return match;
        }

        private bool IsSimpleKey(DbConfig dbConfig, DateTime started, IKey foreignKey, IKey primaryKey, ForeignKeySearchParameter foreignKeySearchParameter) {
            //Check the column types to reduce the input sets
            if (!IsColumnTypesMatch(foreignKey, primaryKey)) return false;

            bool isForeignKey = false;
            using (IDatabase conn = ConnectionManager.CreateConnection(dbConfig)) {
                conn.Open();
                conn.SetHighTimeout();
                //Get the IDX columns
                //if (conn.TableExists(KeySearchManager.GetIdxDbName(conn.DbConfig) + ".idx_" + primaryKey.Columns[0].ColumnId)
                //    && conn.TableExists(KeySearchManager.GetIdxDbName(conn.DbConfig) + ".idx_" + foreignKey.Columns[0].ColumnId)) 
                {
                    string sql = string.Format(
                        "SELECT ( SELECT COUNT(*) FROM {0}.{2} AS t2 ) = ( SELECT COUNT(*) FROM {0}.{2} AS t2 WHERE EXISTS (SELECT `id` FROM {0}.{1} AS t1 WHERE t1.{3} = t2.{3}) )",
                        KeySearchManager.GetIdxDbName(conn.DbConfig),
                        "idx_" + primaryKey.Columns[0].ColumnId,
                        "idx_" + foreignKey.Columns[0].ColumnId,
                        conn.Enquote("value"));
                    isForeignKey = Convert.ToInt32(conn.ExecuteScalar(sql)) == 1;
                }
            }

            //Simple foreign key
            if (isForeignKey) {
                DateTime finished = DateTime.Now;
                ForeignKey fk = new ForeignKey(foreignKey, primaryKey);
                fk.SearchedTableId = foreignKeySearchParameter.BaseTableId;
                foreignKeySearchParameter.ForeignKeyCollectorInstance.AddKey(fk, started, finished);
                _log.Log(LogLevelEnum.Info, "Simple foreign key found: " + fk.DisplayString, true);
                return true;
            }

            return false;
        }

        private bool IsCompositeKey(DbConfig dbConfig, DateTime started, IKey foreignKey, IKey primaryKey, ForeignKeySearchParameter foreignKeySearchParameter, Action<ForeignKey> actionToSetSameKeyPermutationsDisabled) {
            //Check the column types to reduce the input sets
            if (!IsColumnTypesMatch(foreignKey, primaryKey)) return false;

            string additionalColumnCheck = string.Empty;
            for (int i = 1; i < foreignKey.Columns.Count; i++) {
                additionalColumnCheck += string.Format(" AND t1.{0} = t2.{1}", primaryKey.Columns[i].ColumnName, foreignKey.Columns[i].ColumnName);
            }

            bool isForeignKey = false;
            using (var conn = ConnectionManager.CreateConnection(dbConfig)) {
                conn.Open();
                conn.SetHighTimeout();
                string sql = string.Format("SELECT ( SELECT COUNT(*) FROM {0}.{2} AS t2 ) = ( SELECT COUNT(*) FROM {0}.{1} AS t1 INNER JOIN {0}.{2} AS t2 ON t1.{3} = t2.{4} {5} )",
                                        conn.DbConfig.DbName,
                                        primaryKey.TableName,
                                        foreignKey.TableName,
                                        conn.Enquote(primaryKey.Columns[0].ColumnName),
                                        conn.Enquote(foreignKey.Columns[0].ColumnName),
                                        additionalColumnCheck);
                isForeignKey = Convert.ToInt32(conn.ExecuteScalar(sql)) == 1;
            }
            //Is composite foreign key
            if (isForeignKey) {
                DateTime finished = DateTime.Now;
                ForeignKey fk = new ForeignKey(foreignKey, primaryKey);
                fk.SearchedTableId = foreignKeySearchParameter.BaseTableId;
                foreignKeySearchParameter.ForeignKeyCollectorInstance.AddKey(fk, started, finished);
                //foreach (Key k in _baseTableKeys.Where(c => c.ColumnOrderIndependentHash == fk.ColumnOrderIndependentHash)) {
                //    k.Processed = true;
                //}
                actionToSetSameKeyPermutationsDisabled(fk);
                _log.Log(LogLevelEnum.Info, "Composite foreign key found: " + fk.DisplayString, true);
                return true;
            }
            return false;
        }

        #endregion [ Private methods ]
    }
}
