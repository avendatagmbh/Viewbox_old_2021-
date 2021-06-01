using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using DbAccess;
using DbAccess.Structures;
using DbSearchLogic.Config;
using AvdCommon.Logging;
using DbSearchLogic.SearchCore.ForeignKeySearch;
using System.Linq;
using System.Diagnostics;

namespace DbSearchLogic.SearchCore.KeySearch
{
    /// <summary>
    /// A class to collect the keys [each thread use this global object]
    /// </summary>
    public class KeyCollector {

        #region [ Public members ]

        /// <summary>
        /// A dictionary for the keys
        /// </summary>
        public Dictionary<int, Dictionary<int, Key>> TableKeys;
        
        #endregion [ Public members ]

        #region [ Private members ]

        /// <summary>
        /// Check the Database calling (indicates the first one)
        /// </summary>
        private bool _firstCall = true;

        /// <summary>
        /// Get the Profile obj, to save keys in the DB
        /// </summary>
        private Profile _currentProfile;
        
        /// <summary>
        /// Create the create instance
        /// </summary>
        private SemaphoreSlim _semaphoreCreate = new SemaphoreSlim(1);

        /// <summary>
        /// Create the erase instance
        /// </summary>
        private SemaphoreSlim _semaphoreErase = new SemaphoreSlim(1);

        /// <summary>
        /// Create the add instance
        /// </summary>
        private SemaphoreSlim _semaphoreAdd = new SemaphoreSlim(1);
        
        #endregion [ Private members ]

        #region [ Constructors ]

        /// <summary>
        /// Init the object
        /// </summary>
        public KeyCollector() { }

        #endregion [ Constructors ]
        
        #region [ Public Methods ]

        #region [ Collection Methods ]

        /// <summary>
        /// Create a new instance of keys objects
        /// </summary>
        public void CreateKeys(int minimalSavedToDbItemsCount, Profile currentProfile) {
            _semaphoreCreate.Wait();
            try {
                TableKeys = new Dictionary<int, Dictionary<int, Key>>();
                _currentProfile = currentProfile;
            } finally {
                _semaphoreCreate.Release();
            }
        }

        /// <summary>
        /// Erase the keys objects
        /// </summary>
        public void EraseKeys(){
            _semaphoreErase.Wait();
            try{
                if (TableKeys != null)
                    TableKeys.Clear();
            }
            finally{
                _semaphoreErase.Release();
            }
        }

        #endregion [ Collection Methods ]

        #region [ Key modification methods ]
        /// <summary>
        /// Add a found key 
        /// </summary>
        /// <param name="keyCandidate">The key candidate found as a key</param>
        /// <param name="started">Time when the processing of the key started</param>
        /// <param name="finished">Time when the processing of the key finished</param>
        public void AddKey(KeyCandidate keyCandidate, DateTime started, DateTime finished) {
            _semaphoreAdd.Wait();
            try{
                if (keyCandidate != null){
                    if (IsExists(keyCandidate)) return;
                    Key key = new Key(keyCandidate) { ProcessingDuration = (finished - started).TotalSeconds };
                    if (TableKeys.ContainsKey(key.TableId))
                        TableKeys[key.TableId].Add(keyCandidate.ColumnOrderIndependentHash, key);
                    else
                        TableKeys.Add(key.TableId, new Dictionary<int, Key>() { { key.ColumnOrderIndependentHash, key } });
                    //Save the key object
                    key.Save(_currentProfile.DbConfigView);
                }
            }
            finally{
                _semaphoreAdd.Release();
            }
        }

        #endregion [ Key modification methods ]

        #region [ Key existing check ]

        /// <summary>
        /// Check to exists the specified keys in the KeyCollector
        /// </summary>
        /// <param name="key">The checked key objectname of the table</param>
        /// <returns>true - the column is exist in the collelction
        ///          false - if not</returns>
        public bool IsExists(KeyCandidate key) {
            if (TableKeys.ContainsKey(key.TableId))
                return TableKeys[key.TableId].ContainsKey(key.ColumnOrderIndependentHash);
            else
                return false;
        }

        #endregion [ Key existing check ]

        #region [ Database communications ]

        #region [ Create tables ]

        /// <summary>
        /// Creates the datatable and the tables
        /// </summary>
        /// <param name="dbConfigView">The current profile configuration</param>
        public void CreateDatabases(DbConfig dbConfigView){
            DbConfig configDatabase = (DbConfig)dbConfigView.Clone();
            configDatabase.DbName = string.Empty;

            IDatabase conn;
            string dbName = KeySearchManager.GetKeySeachDbName(dbConfigView);

            if (_firstCall){
                _firstCall = false;
                conn = ConnectionManager.CreateConnection(configDatabase);
                conn.Open();
                conn.CreateDatabaseIfNotExists(dbName);
                conn.Close();
            }

            configDatabase.DbName = dbName;
            conn = ConnectionManager.CreateConnection(configDatabase);
            conn.Open();
            CreateTables(conn);
            CreateTempTables(conn);
        }

        #endregion [ Create tables ]

        #region [ Existing functions ]

        /// <summary>
        /// Load the saved typed keys
        /// </summary>
        /// <param name="dbConfig">The current profile config</param>
        /// <returns>true - the specified table is exist
        ///          false - not exist</returns>
        public bool ExistTable<T>(DbConfig dbConfig) where T : new(){
            using (IDatabase conn = GetOpenConnection(dbConfig)) {
                return conn.TableExists(dbConfig.DbName + KeySearchManager.TablePostfixKeySearch, conn.DbMapping.GetTableName<T>());
            }
        }

        #endregion [ Existing functions ]

        #region [ Load functions ]

        /// <summary>
        /// Load the saved typed keys
        /// </summary>
        /// <param name="dbConfig">The current profile config</param>
        /// <returns>The list of typed of keys</returns>
        public List<T> LoadKeys<T>(DbConfig dbConfig) where T : new(){
            using (IDatabase conn = GetOpenConnection(dbConfig)) {
                return conn.DbMapping.Load<T>();
            }
        }

        #endregion [ Load functions ]

        #region [ Save functions ]

        /// <summary>
        /// Saves the candidate key
        /// </summary>
        /// <param name="candidateKey">The candidate key object</param>
        public void SaveCandidateKey(KeyCandidate candidateKey) {
            candidateKey.Save(_currentProfile.DbConfigView);
            candidateKey = null;
        }

        /// <summary>
        /// Seave candidate keys to database
        /// </summary>
        /// <param name="candidateKeys"></param>
        public void SaveCandidateKeys(IEnumerable<KeyCandidate> candidateKeys) {
            using (IDatabase conn = GetOpenConnection(_currentProfile.DbConfigView)) {
                conn.DbMapping.Save(typeof(KeyCandidate), candidateKeys);
            }
        }

        #endregion [ Save functions ]

        #region [ Delete functions ]

        /// <summary>
        /// Delete the processed key candidate from the processing queue
        /// </summary>
        /// <param name="dbConfig">The current profile config</param>
        /// <param name="key">The key candidate to delete</param>
        public void DeleteKeys<T>(DbConfig dbConfig, KeyCandidate key) where T : new(){
            using (IDatabase conn = GetOpenConnection(dbConfig)) {
                conn.DbMapping.Delete<T>("id=" + key.Id);
            }
        }

        #endregion [ Delete functions ]

        /// <summary>
        /// Get the current connection
        /// </summary>
        /// <param name="dbConfigView">The current profile configuration</param>
        /// <returns>The current connection</returns>
        private IDatabase GetOpenConnection(DbConfig dbConfigView) {
            DbConfig configDatabase = KeySearchManager.GetSearchDbConfig((DbConfig)dbConfigView.Clone());
            IDatabase conn = ConnectionManager.CreateConnection(configDatabase);
            conn.Open();
            return conn;
        }

        #endregion [ Database communications ]

        #endregion [ Public Methods ]

        #region [ Private Methods ]

        /// <summary>
        /// Create the datatables if not exist
        /// </summary>
        /// <param name="conn">The current profile dbconfifg</param>
        internal void CreateTables(IDatabase conn){
            conn.DbMapping.CreateTableIfNotExists<Key>();
            conn.DbMapping.CreateTableIfNotExists<KeyColumn>();
        }

        /// <summary>
        /// Create the datatables if not exist
        /// </summary>
        /// <param name="conn">The current profile dbconfifg</param>
        internal void CreateTempTables(IDatabase conn) {
            conn.DbMapping.CreateTableIfNotExists<KeyCandidate>();
            conn.DbMapping.CreateTableIfNotExists<KeyCandidateColumn>();
        }

        /// <summary>
        /// Recreate the datatables if not exist
        /// </summary>
        /// <param name="conn">The current profile dbconfifg</param>
        public void ReCreateTables(IDatabase conn) {
            conn.DbMapping.DropTableIfExists<KeyColumn>();
            conn.DbMapping.DropTableIfExists<Key>();
            conn.DbMapping.CreateTableIfNotExists<Key>();
            conn.DbMapping.CreateTableIfNotExists<KeyColumn>();
        }

        /// <summary>
        /// Recreate the datatables if not exist
        /// </summary>
        /// <param name="conn">The current profile dbconfifg</param>
        public void ReCreateTempTables(IDatabase conn) {
            conn.DbMapping.DropTableIfExists<KeyCandidateColumn>();
            conn.DbMapping.DropTableIfExists<KeyCandidate>();
            conn.DbMapping.CreateTableIfNotExists<KeyCandidate>();
            conn.DbMapping.CreateTableIfNotExists<KeyCandidateColumn>();
        }
        
        #endregion [ Private Methods ]
    }
}
