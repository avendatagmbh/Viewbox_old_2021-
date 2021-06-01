using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DbAccess;
using DbAccess.Structures;
using DbSearchBase.Interfaces;

namespace DbSearchLogic.SearchCore.KeySearch{
    /// <summary>
    /// Represetns the keycollector.TableKeys item (one key)
    /// </summary>
    [DbTable("key_candidate", ForceInnoDb = true)]
    public class KeyCandidate : IKey {
        
        #region [ Public Properties ]

        private int _id = 0;

        [DbPrimaryKey()]
        [DbColumn("id", AllowDbNull = false)]
        public int Id {
            get {
                return _id;
            }
            set {
                _id = value;
                foreach (KeyCandidateColumn column in CandidateColumns) {
                    if (column.Key == null)
                        column.Key = this;
                    else
                        column.Key.Id = value;
                }

            }
        }

        /// <summary>
        /// The table id
        /// </summary>
        [DbColumn("table_id", AllowDbNull = false)]
        public int TableId { get; set; }

        /// <summary>
        /// The checked table name
        /// </summary>
        [DbColumn("table_name", AllowDbNull = false)]
        public string TableName { get; set; }

        /// <summary>
        /// The row count
        /// </summary>
        [DbColumn("row_count", AllowDbNull = false)]
        public long NumberOfRows { get; set; }

        /// <summary>
        /// The IDX row count
        /// </summary>
        [DbColumn("row_count_idx", AllowDbNull = false)]
        public long NumberOfRowsIdx { get; set; }

        private List<KeyCandidateColumn> columns = new List<KeyCandidateColumn>();
        /// <summary>
        /// The columns of the key candidate
        /// </summary>
        [DbCollection("Key")]
        public List<KeyCandidateColumn> CandidateColumns {
            get { return columns; }
            set { columns = value; }
        }

        [DbColumn("label", Length = 500, AllowDbNull = true)]
        public string Label { get; set; }

        public List<IColumn> Columns { get { return new List<IColumn>(columns.Select(c => c as IColumn)); } }

        public bool Processed { get; set; }

        #endregion [ Public Properties ]

        #region [ Constructors ]

        /// <summary>
        /// Constructor
        /// </summary>
        public KeyCandidate() {
            
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="keyTableName">Key table name</param>
        public KeyCandidate(string keyTableName) {
            TableName = keyTableName;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public KeyCandidate(Key key) {
            Processed = false;
            TableName = key.TableName;
            TableId = key.TableId;
            NumberOfRows = key.NumberOfRows;
            NumberOfRowsIdx = key.NumberOfRowsIdx;
            Id = key.Id;

            foreach (KeyColumn keyColumn in key.KeyColumns) {
                CandidateColumns.Add(new KeyCandidateColumn(keyColumn));
            }

            Label = DisplayString;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="keyTable">Key table id</param>
        /// <param name="keyTableName">Key table name</param>
        /// <param name="numberOfRows">The number of rows in table</param>
        /// <param name="numberOfRowsIdx">The number of rows in idx table</param>
        /// <param name="columns">The columns of the key candidate</param>
        public KeyCandidate(int keyTable, string keyTableName, long numberOfRows, long numberOfRowsIdx, IEnumerable<KeyCandidateColumn> columns){
            TableId = keyTable;
            TableName = keyTableName;
            NumberOfRows = numberOfRows;
            NumberOfRowsIdx = numberOfRowsIdx;
            CandidateColumns = new List<KeyCandidateColumn>(columns);
        }

        #endregion [ Constructors ]

        #region [ Save items to database ]

        /// <summary>
        /// Save key object to database
        /// </summary>
        /// <param name="dbConfig">The current profile config</param>
        public void Save(DbConfig dbConfig){
            using (IDatabase conn = GetOpenConnection(dbConfig)){
                conn.DbMapping.Save(this);
            }
        }

        /// <summary>
        /// Load key objects from database
        /// </summary>
        /// <param name="dbConfig">The current profile config</param>
        internal static List<KeyCandidate> Load(DbConfig dbConfig) {
            using (IDatabase conn = GetOpenConnection(dbConfig)) {
                return conn.DbMapping.Load<KeyCandidate>("id > 0", true);
            }
        }

        /// <summary>
        /// Get the current connection
        /// </summary>
        /// <param name="dbConfigView">The current profile configuration</param>
        /// <returns>The current connection</returns>
        private static IDatabase GetOpenConnection(DbConfig dbConfigView) {
            DbConfig configDatabase = KeySearchManager.GetSearchDbConfig((DbConfig)dbConfigView.Clone());
            IDatabase conn = ConnectionManager.CreateConnection(configDatabase);
            conn.Open();
            return conn;
        }

        #endregion [ Save items to database ]

        /// <summary>
        /// Gets the display string of the current instance
        /// </summary>
        public string DisplayString {
            get {
                string columns = string.Join(", ", CandidateColumns.Select(column => column.ColumnName));
                return TableName + " ( " + columns + " )";
            }
        }

        public int _hash = 0;
        /// <summary>
        /// Gets the column order independent hash
        /// </summary>
        public int ColumnOrderIndependentHash {
            get {
                if (_hash == 0) {
                    foreach (KeyCandidateColumn column in CandidateColumns.OrderBy(c => c.ColumnName)) {
                        _hash ^= column.ColumnName.GetHashCode();
                    }
                }
                return _hash;
            }
        }

        public void AddColumn(string columnName, int columnId, DbColumnTypes columnType) {
            this.CandidateColumns.Add(new KeyCandidateColumn(columnName, columnId, columnType));
            Label = DisplayString;
        }
    }
}
