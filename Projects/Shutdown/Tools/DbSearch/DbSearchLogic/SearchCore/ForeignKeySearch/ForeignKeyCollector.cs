using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using DbSearchLogic.Config;
using DbSearchLogic.SearchCore.ForeignKeySearch;
using DbAccess;
using DbAccess.Structures;
using System;
using AvdCommon.Logging;

namespace DbSearchLogic.SearchCore.KeySearch{
    /// <summary>
    /// A class to collect the foreign keys [each thread use this global object]
    /// </summary>
    public class ForeignKeyCollector{

        #region [ Properties ]

        /// <summary>
        /// A dictionary for the foreign keys
        /// </summary>
        public ConcurrentDictionary<int, ForeignKey> ForeignKeys;
        
        #endregion [ Properties ]

        #region [ Private Members ]
        
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
        
        /// <summary>
        /// The foreign keycollector first calling indicator
        /// </summary>
        private bool _firstCall = true;

        /// <summary>
        /// Get the current profile obj.
        /// </summary>
        private Profile _currentProfile;

        #endregion [ Private Members ]

        #region [ Constructors ]

        /// <summary>
        /// Init the object
        /// </summary>
        public ForeignKeyCollector(){
        }

        #endregion [ Constructors ]
        
        #region [ Collection Methods ]
            
        /// <summary>
        /// Create a new instance of keys objects
        /// </summary>
        public void CreateKeys(int minimalSavedToDbItemsCount, Profile currentProfile){
            _semaphoreCreate.Wait();
            try{
                ForeignKeys = new ConcurrentDictionary<int, ForeignKey>();
                _currentProfile = currentProfile;
            }
            finally{
                _semaphoreCreate.Release();
            }
        }

        /// <summary>
        /// Erase the keys objects
        /// </summary>
        public void EraseKeys(){
            _semaphoreErase.Wait();
            try{
                //Clear the keys
                if (ForeignKeys != null)
                    ForeignKeys.Clear();
            }
            finally{
                _semaphoreErase.Release();
            }
        }

        #endregion [ Collection Methods ]

        #region [ Key modification Methods ]

        /// <summary>
        /// Add key found
        /// </summary>
        /// <param name="key">The added foreign key value</param>
        /// <param name="started">Time when the processing of the key started</param>
        /// <param name="finished">Time when the processing of the key finished</param>
        /// <returns>true - the key is exist in the database
        ///          false - the key doesn't exist in the DB so saved it</returns>
        public void AddKey(ForeignKey key, DateTime started, DateTime finished) {
            _semaphoreAdd.Wait();
            try{
                if (key != null){
                    if (IsExists(key)) return;
                    key.ProcessingDuration = (finished - started).TotalSeconds;
                    ForeignKeys.TryAdd(key.PrimaryKeyHash, key);
                    //Save the results
                    key.Save(_currentProfile.DbConfigView);
                }
            }
            finally{
                _semaphoreAdd.Release();
            }
        }

        /// <summary>
        /// Adds an already found key (which was loaded from db when fk search started)
        /// </summary>
        /// <param name="key">The foreign key to add</param>
        public void AddExistingKey(ForeignKey key) {
            _semaphoreAdd.Wait();
            try {
                if (key != null) {
                    if (IsExists(key)) return;
                    key.Preloaded = true;
                    ForeignKeys.TryAdd(key.PrimaryKeyHash, key);
                }
            } finally {
                _semaphoreAdd.Release();
            }
        }

        /// <summary>
        /// Adds already found keys (which were loaded from db when fk search started)
        /// </summary>
        /// <param name="keys">The foreign keys to add</param>
        public void AddExistingKeys(IEnumerable<ForeignKey> keys) {
            foreach (ForeignKey foreignKey in keys) {
                AddExistingKey(foreignKey);
            }
        }

        #endregion [ Key modification Methods ]

        #region [ Key existence check ]

        /// <summary>
        /// Check to exists the specified keys in the foreign key collection
        /// </summary>
        /// <param name="key">The checked key objectname of the table</param>
        /// <returns>true - the column is exist in the collelction
        ///          false - if not</returns>
        public bool IsExists(ForeignKey key){
            return ForeignKeys.ContainsKey(key.PrimaryKeyHash);
        }

        #endregion [ Key existence check ]

        #region [ Database related methods ]
        
        /// <summary>
        /// Creates the datatable and the tables
        /// </summary>
        /// <param name="DbConfigView">The current profile configuration</param>
        public void CreateDatabases(DbConfig DbConfigView) {
            DbConfig configDatabase = (DbConfig)DbConfigView.Clone();
            configDatabase.DbName = string.Empty;

            IDatabase conn;
            string dbName = KeySearchManager.GetKeySeachDbName(DbConfigView);

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
        }
        
        /// <summary>
        /// Load the saved typed keys
        /// </summary>
        /// <param name="dbConfig">The current profile config</param>
        /// <returns>true - the specified table is exist
        ///          false - not exist</returns>
        public bool IsTableExists<T>(DbConfig dbConfig) where T : new(){
            using (IDatabase conn = GetOpenConnection(dbConfig)){
                return conn.TableExists(KeySearchManager.GetKeySeachDbName(dbConfig), conn.DbMapping.GetTableName<T>());
            }
        }
        
        /// <summary>
        /// Loads the existing foreign keys related to a table (regardless of PK or FK is in the table)
        /// </summary>
        /// <param name="dbConfig"></param>
        /// <param name="tableId"></param>
        public void LoadExistingKeysForTable(DbConfig dbConfig, int tableId, KeySearchManager sm) {
            _semaphoreAdd.Wait();
            try {
                List<ForeignKey> fks = sm.GetForeignKeysRelatedToTable(tableId);
                foreach (ForeignKey key in fks) {
                    if (IsExists(key)) return;
                    ForeignKeys.TryAdd(key.PrimaryKeyHash, key);   
                }
            } finally {
                _semaphoreAdd.Release();
            }
        }

        /// <summary>
        /// Get the DB connection
        /// </summary>
        /// <param name="DbConfigView">The profile configuration</param>
        /// <returns>The DB connection</returns>
        public IDatabase GetOpenConnection(DbConfig DbConfigView){
            DbConfig configDatabase = (DbConfig)DbConfigView.Clone();
            configDatabase.DbName = string.Empty;
            configDatabase.DbName = KeySearchManager.GetKeySeachDbName(DbConfigView);

            IDatabase conn = ConnectionManager.CreateConnection(configDatabase);
            conn.Open();
            return conn;
        }

        #endregion [ Database related methods ]

        /// <summary>
        /// Shows the foreign key to the user
        /// </summary>
        /// <param name="key">Foreign Key</param>
        /// <returns>The foreign key text</returns>
        public string GetForeignKeyRelationDisplayString(ForeignKey key) {
            return key.DisplayString;
        }

        #region [ Private Methods ]

        /// <summary>
        /// Create the datatables if not exist
        /// </summary>
        /// <param name="conn">The current profile dbconfifg</param>
        internal void CreateTables(IDatabase conn){
            conn.DbMapping.CreateTableIfNotExists<ForeignKey>();
            conn.DbMapping.CreateTableIfNotExists<ForeignKeyColumn>();
        }

        /// <summary>
        /// Recreate the datatables
        /// </summary>
        /// <param name="conn">The current profile dbconfifg</param>
        public void ReCreateTables(IDatabase conn) {
            conn.DbMapping.DropTableIfExists<ForeignKeyColumn>();
            conn.DbMapping.DropTableIfExists<ForeignKey>();
            conn.DbMapping.CreateTableIfNotExists<ForeignKey>();
            conn.DbMapping.CreateTableIfNotExists<ForeignKeyColumn>();
        }
        
        #endregion [ Private Methods ]
    }
}
