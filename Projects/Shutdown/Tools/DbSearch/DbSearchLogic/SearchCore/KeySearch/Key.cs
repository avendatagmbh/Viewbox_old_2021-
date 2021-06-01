using System;
using System.Collections.Generic;
using DbAccess;
using DbAccess.Structures;
using DbSearchBase.Interfaces;
using System.Linq;
using System.Data;

namespace DbSearchLogic.SearchCore.KeySearch {
    /// <summary>
    /// The key object which can be stores all information from a table key
    /// </summary>
    [DbTable("keys",ForceInnoDb = true)]
    public class Key : IKey {
        
        #region [ Properties ]
        
        private int _id = 0;

        [DbPrimaryKey()]
        [DbColumn("id", AllowDbNull = false)]
        public int Id {
            get {
                return _id;
            }
            set {
                _id = value;
                foreach (KeyColumn column in KeyColumns) {
                    if (column.Key == null)
                        column.Key = this;
                    else
                        column.Key.Id = value;
                }

            }
        }

        [DbColumn("table", AllowDbNull = false)]
        public int TableId { get; set; }
                
        [DbCollection("Key")]
        public List<KeyColumn> KeyColumns { get; internal set; }

        [DbColumn("label", Length = 500, AllowDbNull = true)]
        public string Label { get; set; }

        public List<IColumn> Columns { get { return new List<IColumn>(KeyColumns.Select(c => c as IColumn)); } }

        /// <summary>
        /// The duration of processing this key in seconds
        /// </summary>
        [DbColumn("processing_duration")]
        public double ProcessingDuration { get; set; }

        public string TableName { get; set; }
        public long NumberOfRows { get; set; }
        public long NumberOfRowsIdx { get; set; }

        /// <summary>
        /// The deleted state of the key which is used only during key processing (for synchronizing multitheraded operations)
        /// </summary>
        public bool Processed { get; set; }

        /// <summary>
        /// Gets the column list of the current instance as a string
        /// </summary>
        private string ColumnList {
            get {
                string columns = string.Join(", ", KeyColumns.Select(column => column.ColumnName));
                return columns;
            }
        }

        /// <summary>
        /// Gets the display string of the current instance
        /// </summary>
        public string DisplayString {
            get {
                //return TableName + " ( " + ColumnList + " )";
                return TableName + " ( " + Label + " )";
            }
        }

        public int _hash = 0;
        /// <summary>
        /// Gets the column order independent hash
        /// </summary>
        public int ColumnOrderIndependentHash {
            get {
                if (_hash == 0) {
                    foreach (KeyColumn column in KeyColumns.OrderBy(c => c.ColumnName)) {
                        _hash ^= column.ColumnName.GetHashCode();
                    }
                }
                return _hash;
            }
        }

        #endregion [ Properties ]
        
        #region [ Constructors ]

        /// <summary>
        /// Constructor
        /// </summary>
        public Key() {
            Init();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public Key(KeyCandidate keyCandidate) {
            Init();

            TableName = keyCandidate.TableName;
            TableId = keyCandidate.TableId;
            NumberOfRows = keyCandidate.NumberOfRows;
            NumberOfRowsIdx = keyCandidate.NumberOfRowsIdx;

            foreach (KeyCandidateColumn keyCandidateColumn in keyCandidate.CandidateColumns) {
                KeyColumns.Add(new KeyColumn(keyCandidateColumn));
            }

            Label = ColumnList;
        }

        #endregion [ Constructors ]

        #region [ Public methods ]
        
        #endregion [ Public methods ]

        #region [ Private Methods ]

        /// <summary>
        /// Initialize the variables
        /// </summary>
        private void Init(){
            KeyColumns = new List<KeyColumn>();
        }

        #region [ Load and Save items to database ]

        /// <summary>
        /// Save key object to database
        /// </summary>
        /// <param name="dbConfig">The current profile config</param>
        internal void Save(DbConfig dbConfig){
            using (IDatabase conn = GetOpenConnection(dbConfig)){
                conn.DbMapping.Save(this);
            }
        }

        /// <summary>
        /// Gets whether keys exists
        /// </summary>
        /// <returns>True if there are keys existing otherwise false</returns>
        internal static bool Exists(DbConfig dbConfig) {
            bool result = false;
            using (IDatabase conn = GetOpenConnection(dbConfig)) {
                string sql = string.Format("SELECT * FROM {0} LIMIT 1", conn.DbConfig.DbName + ".keys");
                using(IDataReader rdr = conn.ExecuteReader(sql)) {
                    if (rdr.Read()) result = true;
                }
                conn.Close();
            }
            return result;
        }

        /// <summary>
        /// Load key objects from database
        /// </summary>
        /// <param name="id">The primary key id</param>
        /// <param name="dbConfig">The current profile config</param>
        internal static Key LoadKey(DbConfig dbConfig, int id) {
            using (IDatabase conn = GetOpenConnection(dbConfig)) {
                return conn.DbMapping.Load<Key>("id = " + id.ToString(), true).FirstOrDefault();
            }
        }

        /// <summary>
        /// Load key objects from database
        /// </summary>
        /// <param name="dbConfig">The current profile config</param>
        internal static List<Key> Load(DbConfig dbConfig){
            using (IDatabase conn = GetOpenConnection(dbConfig)) {
                return conn.DbMapping.Load<Key>("id > 0", true);
            }
        }

        /// <summary>
        /// Load key objects from database
        /// </summary>
        /// <param name="dbConfig">The current profile config</param>
        /// <param name="tableId">The table id</param>
        internal static List<Key> Load(DbConfig dbConfig, int tableId) {
            using (IDatabase conn = GetOpenConnection(dbConfig)) {
                return conn.DbMapping.Load<Key>("`table` = " + tableId.ToString(), true);
            }
        }

        /// <summary>
        /// Get the current connection
        /// </summary>
        /// <param name="DbConfigView">The current profile configuration</param>
        /// <returns>The current connection</returns>
        private static IDatabase GetOpenConnection(DbConfig DbConfigView){
            DbConfig configDatabase = KeySearchManager.GetSearchDbConfig((DbConfig)DbConfigView.Clone());
            IDatabase conn = ConnectionManager.CreateConnection(configDatabase);
            conn.Open();
            return conn;
        }

        #endregion [ Load and Save items to database ]

        #endregion [ Private Methods ]
  }
}
