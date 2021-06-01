using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using AV.Log;
using DbAccess;
using DbAccess.Structures;
using DbSearchBase.Enums;
using DbSearchBase.Interfaces;
using DbSearchLogic.Config;
using DbSearchLogic.ProcessQueue;
using DbSearchLogic.SearchCore.ForeignKeySearch;
using DbSearchLogic.SearchCore.KeySearch;
using DbSearchLogic.SearchCore.Keys;
using Utils;
using log4net;

namespace DbSearchLogic.SearchCore {

    public class KeySearchManager : NotifyPropertyChangedBase, IDisposable {

        internal ILog _log = LogHelper.GetLogger();

        #region [ Constants ]

        public const string TablePostfixKeySearch = "_dbsearchconfig";
        public const string TablePostfixIdx = "_idx";
        public const string ColumnPrefixIdx = "idx_";

        #endregion [ Constants ]

        public KeySearchManager(Profile profile) {
            this._currentProfile = profile;

            try {
                using (var conn = ConnectionManager.CreateConnection(GetSearchDbConfig(DbConfig))) {
                    conn.Open();
                    new KeyCollector().CreateTables(conn);
                    new ForeignKeyCollector().CreateTables(conn);
                    conn.Close();
                }
            }
            catch (Exception ex) {
                _log.Error("KeySearchManager initialization failed in constructor", ex);
            }

            InitiatedSearches = new ObservableCollectionAsync<IKeySearchStatEntry>();
        }

        #region [ Private members ]

        private bool _disposed = false;

        internal DbConfig DbConfig {
            get { return CurrentProfile.DbConfigView; }
        }

        /// <summary>
        /// Create a semaphore instance
        /// </summary>
        private static SemaphoreSlim _semaphorePrimaryKeys = new SemaphoreSlim(1);

        /// <summary>
        /// Create a semaphore instance
        /// </summary>
        private static SemaphoreSlim _semaphoreColumnTypes = new SemaphoreSlim(1);
        
        /// <summary>
        /// Create a semaphore instance
        /// </summary>
        private static SemaphoreSlim _semaphoreColumnInfos = new SemaphoreSlim(1);

        /// <summary>
        /// Create a semaphore instance
        /// </summary>
        private static SemaphoreSlim _semaphoreKeyManager = new SemaphoreSlim(1);

        /// <summary>
        /// Create a semafor instance
        /// </summary>
        private static SemaphoreSlim _semaphoreAllTables = new SemaphoreSlim(1);

        /// <summary>
        /// Create a semafor instance
        /// </summary>
        private static SemaphoreSlim _semaphorePK = new SemaphoreSlim(1);

        private Dictionary<int, DbColumnTypes> _columnTypesById = new Dictionary<int, DbColumnTypes>();
        private ConcurrentDictionary<int, Tuple<string, int, ConcurrentDictionary<int, string>>> _columnInfos = new ConcurrentDictionary<int, Tuple<string, int, ConcurrentDictionary<int, string>>>();
        private KeyManager _keyManager;
        private Dictionary<int, string> _allTables;
        private ConcurrentDictionary<int, Key> _primaryKeysLazy;

        #endregion [ Private members ]

        #region [ Public members ]

        private Profile _currentProfile;
        public Profile CurrentProfile {
            get { return _currentProfile; } 
            internal set { _currentProfile = value; }
        }

        private ObservableCollectionAsync<IKeySearchStatEntry> _initiatedSearches = null;
        public ObservableCollectionAsync<IKeySearchStatEntry> InitiatedSearches {
            get { return _initiatedSearches; }
            set { 
                _initiatedSearches = value;
                OnPropertyChanged("InitiatedSearches");
            } 
        }

        /// <summary>
        /// Gets the column types by id
        /// </summary>
        public Dictionary<int, DbColumnTypes> ColumnTypesById {
            get {
                return _columnTypesById;
            }
        }

        /// <summary>
        /// Gets the column infos (tableId, tableName, idx table row count, columnId, columnName)
        /// </summary>
        public ConcurrentDictionary<int, Tuple<string, int, ConcurrentDictionary<int, string>>> ColumnInfos {
            get {
                return _columnInfos;
            }
        }

        /// <summary>
        /// Key manager instance for current profile
        /// </summary>
        public KeyManager KeyManager {
            get {
                if (_keyManager == null) {
                    _semaphoreKeyManager.Wait();
                    try {
                        if (_keyManager == null) {
                            _keyManager = new KeyManager(this);
                        }
                    }
                    finally {
                        _semaphoreKeyManager.Release();
                    }
                }
                return _keyManager;
            }
        }
        
        /// <summary>
        /// Get all possible tables
        /// </summary>
        /// <returns>The list of the tables</returns>
        public Dictionary<int, string> AllTables {
            get { return GetAllTables(); }
        }

        /// <summary>
        /// The primary keys loaded lazy (not initialized)
        /// </summary>
        public ConcurrentDictionary<int, Key> PrimaryKeysLazy {
            get {
                if (_primaryKeysLazy == null) {
                    _semaphorePK.Wait();
                    try {
                        _primaryKeysLazy = new ConcurrentDictionary<int, Key>();
                    } finally {
                        _semaphorePK.Release();
                    }
                }
                return _primaryKeysLazy;
            }
        }

        #endregion [ Public members ]
        
        #region [ Public methods ]
        
        public KeyManager ChangeProfile(Profile profile) {
            if (profile.Name != _currentProfile.Name) {
                if (_keyManager != null) {
                    _semaphoreKeyManager.Wait();
                    try {
                        _currentProfile = profile;
                        _keyManager = new KeyManager(this);
                        _columnTypesById = new Dictionary<int, DbColumnTypes>();
                        _columnInfos = new ConcurrentDictionary<int, Tuple<string, int, ConcurrentDictionary<int, string>>>();
                        _allTables = null;
                        _primaryKeysLazy = null;
                        OnPropertyChanged("KeyManager");
                    } finally {
                        _semaphoreKeyManager.Release();
                    }
                }
            }
            return KeyManager;
        }

        //public T AddKeySearchStatEnrty<T>(int keyComplexity, int numberOfKeyCandidates, CancellationTokenSource cancellationTokenSource) where T : IKeySearchStatEntry, new() {
        //    T entry = default(T);
        //    if (typeof(IPrimaryKeySearchStatEntry).FullName == typeof(T).FullName || typeof(PrimaryKeySearchStatEntry).FullName == typeof(T).FullName) {
        //        IPrimaryKeySearchStatEntry pk = new PrimaryKeySearchStatEntry(keyComplexity, numberOfKeyCandidates, cancellationTokenSource);
        //        entry = (T)pk;
        //    }
        //    else {
        //        IForeignKeySearchStatEntry fk = new ForeignKeySearchStatEntry(keyComplexity, numberOfKeyCandidates, cancellationTokenSource);
        //        entry = (T)fk;
        //    }
        //    InitiatedSearches.Add(entry);
        //    return entry;
        //}

        public T AddKeySearchStatEnrty<T>(T keySearchStatEntry) where T : IKeySearchStatEntry, new() {
            InitiatedSearches.Add(keySearchStatEntry);
            return keySearchStatEntry;
        }

        public void Init() {
            GetAllTables();
        }

        public void RefreshKeys() {
            _semaphoreKeyManager.Wait();
            try {
                _keyManager = new KeyManager(this);
            } finally {
                _semaphoreKeyManager.Release();
            }
            OnPropertyChanged("KeyManager");
        }

        #endregion [ Public methods ]

        #region [ Static methods ]

        public static string GetKeySeachDbName(DbConfig dbConfig) {
            return dbConfig.DbName + TablePostfixKeySearch;
        }

        public static string GetIdxDbName(DbConfig dbConfig) {
            return dbConfig.DbName + TablePostfixIdx;
        }

        public static DbConfig GetSearchDbConfig(DbConfig dbConfig) {
            DbConfig configDatabase = (DbConfig)dbConfig.Clone();
            configDatabase.DbName = GetKeySeachDbName(dbConfig);
            return configDatabase;
        }

        public static DbConfig GetIdxDbConfig(DbConfig dbConfig) {
            DbConfig configDatabase = (DbConfig)dbConfig.Clone();
            configDatabase.DbName = GetIdxDbName(dbConfig);
            return configDatabase;
        }

        public static DisplayKey CreateRelatedKey(IDisplayKey displayKey, ForeignKey key, KeyManager.SelectedChangedEventHandler action, Func<IDisplayDbKey, IKey> initAction, Func<IDisplayKey, ObservableCollection<IDisplayKey>> loadSubkeyAction, KeySearchManager searchManagerInstance) {
            DisplayKey dKey = new DisplayKey(key, new List<IDisplayKey>(), initAction, loadSubkeyAction, searchManagerInstance, displayKey);
            dKey.OnIsSelectedChanged += action;
            return dKey;
        }

        #endregion [ Static methods ]

        /// <summary>
        /// Deletes the table
        /// </summary>
        public void DeleteTable<T>() where T : new() {
            using (IDatabase conn = GetOpenConnection()) {
                conn.DbMapping.DropTableIfExists<T>();
                conn.Close();
                conn.Dispose();
            }
        }

        #region [ Get result methods ]

        public bool ExistTable<T>() where T : new() {
            using (IDatabase conn = GetOpenConnection()) {
                return conn.TableExists(DbConfig.DbName + TablePostfixKeySearch, conn.DbMapping.GetTableName<T>());
            }
        }

        /// <summary>
        /// Gets whether keys exists
        /// </summary>
        /// <returns>True if there are keys existing otherwise false</returns>
        public bool KeysExists() {
            return Key.Exists(DbConfig);
        }

        /// <summary>
        /// Gets the primary key
        /// </summary>
        /// <param name="id">The primary key id</param>
        /// <returns>The primary key</returns>
        public Key GetPrimaryKey(int id) {

            Key key = Key.LoadKey(DbConfig, id);
            InitKey(key);
            return key;
        }

        private List<Key> _primaryKeys = null;
        /// <summary>
        /// Gets the primary keys found
        /// </summary>
        /// <returns>The primary keys found</returns>
        public List<Key> GetPrimaryKeys() {
            if (_primaryKeys == null) {
                _semaphorePrimaryKeys.Wait();
                try {
                    _primaryKeys = Key.Load(DbConfig);
                    InitKeys(_primaryKeys);
                }
                finally {
                    _semaphorePrimaryKeys.Release();
                }
            }
            return _primaryKeys;
        }

        /// <summary>
        /// Gets the primary keys found
        /// </summary>
        /// <param name="tableId">The table id</param>
        /// <returns>The primary keys found</returns>
        public List<Key> GetPrimaryKeys(int tableId) {

            List<Key> savedKeys = Key.Load(DbConfig, tableId);
            InitKeys(savedKeys);
            return savedKeys;
        }

        /// <summary>
        /// Gets the primary key
        /// </summary>
        /// <param name="id">The primary key id</param>
        /// <returns>The primary key</returns>
        public Key GetPrimaryKeyLazy(int id) {

            Key key = null;
            if (!PrimaryKeysLazy.TryGetValue(id, out key)) {
                key = Key.LoadKey(DbConfig, id);
                PrimaryKeysLazy[id] = key;
            }
            key.TableName = GetTableName(key.TableId);
            return key;
        }

        /// <summary>
        /// Gets the primary keys found
        /// </summary>
        /// <param name="tableId">The table id</param>
        /// <returns>The primary keys found</returns>
        public List<Key> GetPrimaryKeysLazy(int tableId){

            List<Key> savedKeys = Key.Load(DbConfig, tableId);
            foreach (Key savedKey in savedKeys) {
                savedKey.TableName = GetTableName(savedKey.TableId);
            }
            return savedKeys;
        }

        /// <summary>
        /// Gets the primary keys found
        /// </summary>
        /// <returns>The primary keys found</returns>
        public List<Key> GetPrimaryKeysLazy() {

            List<Key> savedKeys = Key.Load(DbConfig);
            foreach (Key savedKey in savedKeys) {
                savedKey.TableName = GetTableName(savedKey.TableId);
            }
            return savedKeys;
        }

        /// <summary>
        /// Gets the foreign key
        /// </summary>
        /// <returns>The foreign key</returns>
        public ForeignKey GetForeignKey(int id) {

            ForeignKey savedForeignKey = ForeignKey.LoadKey(DbConfig, id);
            InitForeignKey(savedForeignKey, null);
            return savedForeignKey;
        }

        /// <summary>
        /// Gets the foreign keys
        /// </summary>
        /// <returns>The foreign keys found</returns>
        public List<ForeignKey> GetForeignKeys() {

            List<Key> savedKeys = GetPrimaryKeys();
            List<ForeignKey> savedForeignKeys = ForeignKey.Load(DbConfig);
            InitForeignKeys(savedKeys, savedForeignKeys);
            return savedForeignKeys;
        }

        /// <summary>
        /// Gets the foreign keys
        /// </summary>
        /// <param name="tableId">The table id</param>
        /// <returns>The foreign keys found</returns>
        public List<ForeignKey> GetForeignKeys(int tableId) {

            List<Key> savedKeys = GetPrimaryKeys(tableId);
            List<ForeignKey> savedForeignKeys = ForeignKey.Load(DbConfig, tableId);
            InitForeignKeys(savedKeys, savedForeignKeys);
            return savedForeignKeys;
        }

        /// <summary>
        /// Gets the foreign keys
        /// </summary>
        /// <param name="tableId">The table id</param>
        /// <returns>The foreign keys found</returns>
        public List<ForeignKey> GetForeignKeysLazy(int tableId) {

            List<ForeignKey> savedForeignKeys = ForeignKey.Load(DbConfig, tableId);
            foreach (ForeignKey savedForeignKey in savedForeignKeys) {
                savedForeignKey.TableName = GetTableName(savedForeignKey.TableId);
            }
            return savedForeignKeys;
        }

        /// <summary>
        /// Gets the foreign keys
        /// </summary>
        /// <returns>The foreign keys found</returns>
        public List<ForeignKey> GetForeignKeysLazy() {

            List<ForeignKey> savedForeignKeys = ForeignKey.Load(DbConfig);
            foreach (ForeignKey savedForeignKey in savedForeignKeys) {
                savedForeignKey.TableName = GetTableName(savedForeignKey.TableId);
            }
            return savedForeignKeys;
        }

        /// <summary>
        /// Gets the foreign keys related to table (PK or FK is in the table)
        /// </summary>
        /// <param name="tableId">The table id</param>
        /// <returns>The foreign keys found</returns>
        public List<ForeignKey> GetForeignKeysRelatedToTable(int tableId) {

            List<Key> savedKeys = GetPrimaryKeys();
            List<ForeignKey> savedForeignKeys = ForeignKey.LoadForeignKeysRelatedToTable(DbConfig, tableId, savedKeys.Where(k => k.TableId == tableId));
            InitForeignKeys(savedKeys, savedForeignKeys);
            return savedForeignKeys;
        }

        /// <summary>
        /// Gets the foreign keys related to a primary key
        /// </summary>
        /// <param name="keyId">The key id</param>
        /// <returns>The foreign keys found</returns>
        public List<ForeignKey> GetForeignKeysRelatedToKeyLazy(int keyId) {

            List<ForeignKey> savedForeignKeys = ForeignKey.LoadForeignKeysRelatedToKey(DbConfig, keyId);
            return savedForeignKeys;
        }

        /// <summary>
        /// Gets the foreign keys related to table (PK or FK is in the table)
        /// </summary>
        /// <param name="tableId">The table id</param>
        /// <returns>The foreign keys found</returns>
        public List<ForeignKey> GetForeignKeysRelatedToTableLazy(int tableId, List<Key> primaryKeys) {

            List<ForeignKey> savedForeignKeys = ForeignKey.LoadForeignKeysRelatedToTable(DbConfig, tableId, primaryKeys.Where(k => k.TableId == tableId));
            foreach (ForeignKey savedForeignKey in savedForeignKeys) {
                savedForeignKey.TableName = GetTableName(savedForeignKey.TableId);
            }
            return savedForeignKeys;
        }

        /// <summary>
        /// Get all possible table columns as key candidates
        /// </summary>
        /// <param name="skippedTableId">The skipped table id</param>
        /// <returns>The list of the tablecolumns</returns>
        public IEnumerable<KeyCandidate> GetAllTableColumsAsKeyCandidates(int skippedTableId) {
            List<KeyCandidate> keys = new List<KeyCandidate>();
            using (IDatabase conn = GetOpenConnection()) {
                //string sql = string.Format("SELECT c.cid, t.name, c.name, t.count, t.tid FROM {0}.columns c JOIN {0}.tables t ON t.tid = c.tid", GetIdxDbName(dbConfig));
                // if size == 0 then no idx table created and no need to deal with tis column (BLOB data or null values trough the whol table in this column)
                string sql = string.Format("SELECT c.cid, t.name, c.name, t.count, t.tid, c.size FROM {0}.columns c JOIN {0}.tables t ON t.tid = c.tid AND c.size > 0", GetIdxDbName(DbConfig));
                System.Data.IDataReader reader = conn.ExecuteReader(sql);
                while (reader.Read()) {
                    int tableId = Convert.ToInt32(reader[4]);
                    if (tableId != skippedTableId) {
                        int colId = Convert.ToInt32(reader[0]);
                        int size = Convert.ToInt32(reader[5]);
                        DbColumnTypes colType = GetColumnTypeById(tableId, colId);
                        KeyCandidate key = new KeyCandidate(tableId, reader[1].ToString(), Convert.ToInt32(reader[3]), size, new List<KeyCandidateColumn>() { new KeyCandidateColumn(reader[2].ToString(), colId, colType) });
                        keys.Add(key);
                    }
                }
                conn.Close();
                conn.Dispose();
            }
            return keys;
        }

        /// <summary>
        /// Get all columns of a table as key candidate
        /// </summary>
        /// <param name="tableName">The table name</param>
        /// <returns>The list of the table columns</returns>
        public IEnumerable<KeyCandidate> GetAllColumnsForTableAsKeyCandidates(string tableName) {
            List<KeyCandidate> keys = new List<KeyCandidate>();
            using (IDatabase conn = GetOpenConnection()) {
                //string sql = string.Format("SELECT c.cid, t.name, c.name, t.count, t.tid FROM {0}.columns c JOIN {0}.tables t ON t.tid = c.tid WHERE t.name = '{1}'", GetIdxDbName(dbConfig), tableName);
                // if size == 0 then no idx table created and no need to deal with tis column (BLOB data or null values trough the whol table in this column)
                string sql = string.Format("SELECT c.cid, t.name, c.name, t.count, t.tid, c.size FROM {0}.columns c JOIN {0}.tables t ON t.tid = c.tid WHERE t.name = '{1}' AND c.size > 0", GetIdxDbName(DbConfig), tableName);
                System.Data.IDataReader reader = conn.ExecuteReader(sql);
                while (reader.Read()) {
                    int tableId = Convert.ToInt32(reader[4]);
                    int colId = Convert.ToInt32(reader[0]);
                    int size = Convert.ToInt32(reader[5]);
                    DbColumnTypes colType = GetColumnTypeById(tableId, colId);
                    KeyCandidate key = new KeyCandidate(tableId, reader[1].ToString(), Convert.ToInt32(reader[3]), size, new List<KeyCandidateColumn> { new KeyCandidateColumn(reader[2].ToString(), colId, colType) });
                    keys.Add(key);
                }
                conn.Close();
                conn.Dispose();
            }
            return keys;
        }

        #endregion [ Get result methods ]

        #region [ Public methods ]

        public IKey LoadDisplayKey(IDisplayDbKey displayKey) {
            if (displayKey is IDisplayPrimaryKey) 
                return GetPrimaryKey(displayKey.Id);
            else 
                return GetForeignKey(displayKey.Id);
        }

        public string GetTableName(int tableId) {
            string retVal = null;
            AllTables.TryGetValue(tableId, out retVal);
            return retVal;
        }

        #endregion [ Public methods ]

        #region [ Private methods ]

        /// <summary>
        /// Gets all tables from database
        /// </summary>
        /// <returns></returns>
        private Dictionary<int, string> GetAllTables() {
            if (_allTables == null) {
                _semaphoreAllTables.Wait();
                try {
                    if (_allTables == null) {
                        _allTables = new Dictionary<int, string>();
                        using (IDatabase conn = GetOpenConnection()) {
                            string sql = string.Format("SELECT t.name, t.tid FROM {0}.tables t ORDER BY t.name", GetIdxDbName(DbConfig));
                            using (System.Data.IDataReader reader = conn.ExecuteReader(sql)) {
                                while (reader.Read())
                                    _allTables.Add(Convert.ToInt32(reader[1]), reader[0].ToString());
                            }
                            conn.Close();
                            conn.Dispose();
                        }
                    }
                } finally {
                    _semaphoreAllTables.Release();
                }
            }
            return _allTables;
        }

        /// <summary>
        /// Initialize a key with column names and types
        /// </summary>
        /// <param name="key">The key to initialize</param>
        private void InitKey(Key key) {
            bool first = true;
            foreach (KeyColumn column in key.KeyColumns) {
                Tuple<string, int, string> columnInfo = GetColumnInfo(key.TableId, column.ColumnId);
                Debug.Assert(columnInfo != null);
                if (first) {
                    key.TableName = columnInfo.Item1;
                    key.NumberOfRowsIdx = columnInfo.Item2;
                    first = false;
                }
                column.ColumnName = columnInfo.Item3;
                column.ColumnType = GetColumnTypeById(key.TableId, column.ColumnId);
            }
        }

        /// <summary>
        /// Initialize the key collection with column names and types
        /// </summary>
        /// <param name="keys">The keys collection to initialize</param>
        private void InitKeys(IEnumerable<Key> keys) {
            foreach (Key key in keys) {
                InitKey(key);
            }
        }

        /// <summary>
        /// Initialize the foreign key with column names and types
        /// </summary>
        /// <param name="foreignKey">The foreign key to initialize</param>
        internal void InitForeignKey(ForeignKey foreignKey, Key pKey) {
            if (pKey == null) pKey = GetPrimaryKey(foreignKey.KeyId);
            if (pKey != null) {
                foreach (KeyColumn keyColumn in pKey.KeyColumns) {
                    foreignKey.PrimaryKeyColumns.Add(new ForeignKeyColumn(keyColumn.ColumnId, keyColumn.Key.TableName, keyColumn.ColumnName, keyColumn.ColumnType));
                }
            }
            bool first = true;
            foreach (ForeignKeyColumn column in foreignKey.ForeignKeyColumns) {
                Tuple<string, int, string> columnInfo = GetColumnInfo(foreignKey.TableId, column.ColumnId);
                Debug.Assert(columnInfo != null);
                if (first) {
                    column.TableName = columnInfo.Item1;
                    first = false;
                }
                column.ColumnName = columnInfo.Item3;
                column.ColumnType = GetColumnTypeById(foreignKey.TableId, column.ColumnId);
            }
        }

        /// <summary>
        /// Initialize the foreign key collection with column names and types
        /// </summary>
        /// <param name="keys">The primary key collection</param>
        /// <param name="foreignKeys">The foreign key collection to initialize</param>
        internal void InitForeignKeys(IEnumerable<Key> keys, IEnumerable<ForeignKey> foreignKeys) {
            foreach (ForeignKey foreignKey in foreignKeys) {
                Key pKey = keys.FirstOrDefault(k => k.Id == foreignKey.KeyId);
                InitForeignKey(foreignKey, pKey);
            }
        }

        public bool IsTableExists(IDatabase conn, string dbName, string tableName) {
            return conn.TableExists(dbName, tableName);
        }

        #region [ Column data ]

        private DbColumnTypes GetColumnTypeById(int tableId, int columnId) {
            DbColumnTypes columnType = DbColumnTypes.DbUnknown;
            if (!_columnTypesById.TryGetValue(columnId, out columnType)) {
                _semaphoreColumnTypes.Wait();
                try {
                    foreach (KeyValuePair<int, DbColumnTypes> colType in GetColumnTypesForTable(tableId)) {
                        if (colType.Key == columnId) columnType = colType.Value;
                        _columnTypesById.Add(colType.Key, colType.Value);
                    }
                } finally {
                    _semaphoreColumnTypes.Release();
                }
            }

            return columnType;
        }
        
        /// <summary>
        /// Gets the column types for the given table
        /// </summary>
        private Dictionary<int, DbColumnTypes> GetColumnTypesForTable(int tableId) {
            
            Dictionary<int, DbColumnTypes> colTypesById = new Dictionary<int, DbColumnTypes>();
            
            DbConfig configDatabase = (DbConfig) DbConfig.Clone();
            configDatabase.DbName = GetIdxDbName(configDatabase);

            using (IDatabase conn = GetOpenConnection()) {
                Dictionary<int, int> columns = new Dictionary<int, int>();
                string sql = string.Format("SELECT c.cid, c.size FROM {0}.columns c WHERE c.tid = {1}", configDatabase.DbName, tableId);
                using (System.Data.IDataReader reader = conn.ExecuteReader(sql)) {
                    while (reader.Read()) {
                        columns.Add(Convert.ToInt32(reader[0]), Convert.ToInt32(reader[1]));
                    }
                }

                foreach (KeyValuePair<int, int> column in columns) {
                    DbColumnTypes colType = DbColumnTypes.DbUnknown;
                    // if size == 0 then no idx table created and no need to deal with tis column (BLOB data or null values trough the whol table in this column)
                    if (column.Value > 0) {
                        try {
                            colType = conn.GetColumnInfos(configDatabase.DbName, ColumnPrefixIdx + column.Key)[1].Type;
                        }
                        catch (Exception) {
                            // something magic happened, idx table not found, treat column type as unknown
                        }
                    }
                    colTypesById.Add(column.Key, colType);
                }
                conn.Close();
                conn.Dispose();
            }
            return colTypesById;
        }

        /// <summary>
        /// Gets the column info
        /// </summary>
        /// <param name="tableId">The table id</param>
        /// <param name="columnId">The column id</param>
        /// <returns>tableName, idx table row count, columnName</returns>
        private Tuple<string, int, string> GetColumnInfo(int tableId, int columnId) {
            Tuple<string, int, string> columnInfo = null;
            Tuple<string, int, ConcurrentDictionary<int, string>> tableInfo = null;
            if (!_columnInfos.TryGetValue(tableId, out tableInfo)) {
                _semaphoreColumnInfos.Wait();
                try {
                    tableInfo = GetTableColumnInfos(tableId);
                    _columnInfos[tableId] = tableInfo;
                } finally {
                    _semaphoreColumnInfos.Release();
                }
            }
            columnInfo = new Tuple<string, int, string>(tableInfo.Item1, tableInfo.Item2, tableInfo.Item3[columnId]);
            return columnInfo;
        }

        private Tuple<string, int, ConcurrentDictionary<int, string>> GetTableColumnInfos(int tableId) {
            Tuple<string, int, ConcurrentDictionary<int, string>> colInfos = null;
            using (IDatabase conn = GetOpenConnection()) {
                string sql = string.Format( "SELECT c.cid, t.name, c.name, t.count FROM {0}.columns c JOIN {0}.tables t ON t.tid = c.tid WHERE t.tid = {1}", GetIdxDbName(DbConfig), tableId);

                using (System.Data.IDataReader reader = conn.ExecuteReader(sql)) {
                    while (reader.Read()) {
                        if (colInfos == null) {
                            colInfos = new Tuple<string, int, ConcurrentDictionary<int, string>>(reader[1].ToString(), Convert.ToInt32(reader[3]), new ConcurrentDictionary<int, string>());
                        }
                        int columnId = Convert.ToInt32(reader[0]);
                        string colName = reader[2].ToString();
                        colInfos.Item3[columnId] = colName;
                    }
                }
                conn.Close();
                conn.Dispose();
            }
            return colInfos;
        }
        
        #endregion [ Column data ]

        #endregion [ Private methods ]

        #region [ Communication methods ]

        /// <summary>
        /// Get the DB connection
        /// </summary>
        /// <returns>The DB connection</returns>
        public IDatabase GetOpenConnection() {
            DbConfig configDatabase = (DbConfig)DbConfig.Clone();
            configDatabase.DbName = string.Empty;
            configDatabase.DbName = GetKeySeachDbName(DbConfig);

            IDatabase conn = ConnectionManager.CreateConnection(configDatabase);
            conn.Open();
            return conn;
        }

        #endregion [ Communication methods ]

        public void Dispose()
        {
            KeyManager.Dispose();
        }

        ~KeySearchManager()
        {
            if (!_disposed)
                Dispose();
        }
    }
}
