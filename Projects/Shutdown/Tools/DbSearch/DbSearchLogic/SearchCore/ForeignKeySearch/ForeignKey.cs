using DbSearchBase.Interfaces;
using DbSearchLogic.SearchCore.KeySearch;
using System.Collections.Generic;
using DbAccess;
using DbAccess.Structures;
using System.Linq;

namespace DbSearchLogic.SearchCore.ForeignKeySearch {
    /// <summary>
    /// Represents a forign key item
    /// </summary>
    [DbTable("foreign_keys", ForceInnoDb = true)]
    public class ForeignKey : IForeignKey {

        #region [ Properties ]

        private int _id = 0;

        /// <summary>
        /// The foreign key index
        /// </summary>
        [DbPrimaryKey()]
        [DbColumn("id", AllowDbNull = false)]
        public int Id {
            get { return _id; }
            set {
                _id = value;
                foreach (ForeignKeyColumn foreignKeyColumn in ForeignKeyColumns) {
                    if (foreignKeyColumn.ForeignKey == null)
                        foreignKeyColumn.ForeignKey = new ForeignKey() {Id = value};
                    else
                        foreignKeyColumn.ForeignKey.Id = value;
                }
            }
        }

        /// <summary>
        /// The ID of the keys.id [this gets the table!]
        /// </summary>
        [DbColumn("key_id", AllowDbNull = false)]
        public int KeyId { get; set; }

        /// <summary>
        /// Indicates the table which contains the foreign key
        /// </summary>
        [DbColumn("foreign_table_id", AllowDbNull = false)]
        public int TableId { get; set; }

        /// <summary>
        /// Indicates the table for which the foreign key search started
        /// </summary>
        [DbColumn("searched_table_id", AllowDbNull = false)]
        public int SearchedTableId { get; set; }

        /// <summary>
        /// The duration of processing this key in seconds
        /// </summary>
        [DbColumn("processing_duration")]
        public double ProcessingDuration { get; set; }

        /// <summary>
        /// The name of the fixed table.column
        /// </summary>
        [DbCollection("ForeignKey")]
        public List<ForeignKeyColumn> ForeignKeyColumns { get; set; }

        public List<ForeignKeyColumn> PrimaryKeyColumns { get; set; }

        [DbColumn("label", Length = 500, AllowDbNull = true)]
        public string Label { get; set; }

        public bool Preloaded { get; set; }

        public string TableName { get; set; }
        //public string TableName { get { return ForeignKeyColumns.FirstOrDefault().TableName; } set { ; } }
        
        public bool Processed { get; set; }

        public List<IColumn> Columns { 
            get { return new List<IColumn>(ForeignKeyColumns.Select(fk => fk as IColumn)); } 
        }

        private int _hash = 0;
        /// <summary>
        /// Gets the column order independent hash
        /// </summary>
        public int ColumnOrderIndependentHash {
            get {
                if (_hash == 0) {
                    foreach (ForeignKeyColumn column in ForeignKeyColumns.OrderBy(k => k.ColumnName)) {
                        _hash ^= column.ColumnName.GetHashCode();
                    }
                }
                return _hash;
            }
        }
        /// <summary>
        /// Gets the column order independent hash combined with foreign key table id for the purpose of making the FK globally unique
        /// </summary>
        public int Hash {
            get {
                return ColumnOrderIndependentHash ^ TableId;
            }
        }

        private int _pk_hash = 0;
        /// <summary>
        /// Gets the column order independent hash of primary key and primary key table
        /// </summary>
        public int PrimaryKeyHash {
            get {
                //if (_pk_hash == 0) {
                //    foreach (ForeignKeyColumn column in PrimaryKeyColumns.OrderBy(k => k.ColumnName)) {
                //        _pk_hash ^= column.ColumnName.GetHashCode();
                //    }
                //}
                //_pk_hash ^= pk_table_id;
                //return _pk_hash;
                return KeyId;
            }
        }

        public string DisplayString {
            //get { return ForeignKeyDisplayString + " -> " + PrimaryKeyDisplayString; }
            get { return PrimaryKeyDisplayString + " -> " + ForeignKeyDisplayString; }
        }

        public string DisplayStringForPrimaryKey {
            get { return ForeignKeyDisplayStringWithTable; }
        }

        private string foreignKeyTable = null;
        public string ForeignKeyTable {
            get {
                if (foreignKeyTable == null) {
                    if (this.ForeignKeyColumns != null && this.ForeignKeyColumns.Any())
                        foreignKeyTable = this.ForeignKeyColumns.FirstOrDefault().TableName;
                }
                return foreignKeyTable;
            }
        }

        private string primaryKeyTable = null;
        public string PrimaryKeyTable {
            get {
                if (primaryKeyTable == null) {
                    if (this.PrimaryKeyColumns != null && this.PrimaryKeyColumns.Any())
                        primaryKeyTable = this.PrimaryKeyColumns.FirstOrDefault().TableName;
                }
                return primaryKeyTable;
            }
        }

        public string ForeignKeyDisplayString {
            get {
                string fkDisplayString = string.Empty;
                foreach (ForeignKeyColumn fk in this.ForeignKeyColumns) {
                    if (!string.IsNullOrEmpty(fkDisplayString)) fkDisplayString += ", ";
                    fkDisplayString += fk.ColumnName;
                }
                //fkDisplayString = string.Format("{0} ( {1} )", ForeignKeyTable, fkDisplayString);
                fkDisplayString = string.Format("( {0} )", fkDisplayString);
                return fkDisplayString;
            }
        }

        public string ForeignKeyDisplayStringWithTable {
            get {
                string fkDisplayStringWithTable = string.Empty;
                foreach (ForeignKeyColumn fk in this.ForeignKeyColumns) {
                    if (!string.IsNullOrEmpty(fkDisplayStringWithTable)) fkDisplayStringWithTable += ", ";
                    fkDisplayStringWithTable += fk.ColumnName;
                }
                fkDisplayStringWithTable = string.Format("{0} ( {1} )", ForeignKeyTable, fkDisplayStringWithTable);
                return fkDisplayStringWithTable;
            }
        }

        public string PrimaryKeyDisplayString {
            get {
                string pkDisplayString = string.Empty;
                foreach (ForeignKeyColumn pk in this.PrimaryKeyColumns) {
                    if (!string.IsNullOrEmpty(pkDisplayString)) pkDisplayString += ", ";
                    pkDisplayString += pk.ColumnName;
                }
                pkDisplayString = string.Format("{0} ( {1} )", PrimaryKeyTable, pkDisplayString);
                return pkDisplayString;
            }
        }

        #endregion [ Public ]
        
        #region [ Constructors ]
            
        /// <summary>
        /// Constructor
        /// </summary>
        public ForeignKey() { 
            ForeignKeyColumns = new List<ForeignKeyColumn>();
            PrimaryKeyColumns = new List<ForeignKeyColumn>();
        }
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="keyCandidate">Foreign key in the table</param>
        /// <param name="primaryKey">Primary key reference</param>
        public ForeignKey(IKey keyCandidate, IKey primaryKey) {
            ForeignKeyColumns = new List<ForeignKeyColumn>();
            PrimaryKeyColumns = new List<ForeignKeyColumn>();
            TableId = keyCandidate.TableId;
            TableName = keyCandidate.TableName;
            KeyId = primaryKey.Id;

            for (int i = 0; i < keyCandidate.Columns.Count; i++) {
                ForeignKeyColumns.Add(new ForeignKeyColumn(keyCandidate.Columns[i].ColumnId, keyCandidate.TableName, keyCandidate.Columns[i].ColumnName));
            }

            for (int i = 0; i < primaryKey.Columns.Count; i++) {
                PrimaryKeyColumns.Add(new ForeignKeyColumn(primaryKey.Columns[i].ColumnId, primaryKey.TableName, primaryKey.Columns[i].ColumnName));
            }

            //Label = ForeignKeyDisplayString;
            Label = DisplayString;
        }

        #endregion [ Constructors ]
        
        #region [ Public Methods ]

        #region [ Load and Save items to database ]

        /// <summary>
        /// Save key object to database
        /// </summary>
        /// <param name="dbConfig">The current profile config</param>
        internal void Save(DbConfig dbConfig) {
            using (IDatabase conn = GetOpenConnection(dbConfig)) {
                conn.DbMapping.Save(this);
            }
        }

        /// <summary>
        /// Load key objects from database
        /// </summary>
        /// <param name="dbConfig">The current profile config</param>
        public static ForeignKey LoadKey(DbConfig dbConfig, int id) {
            using (IDatabase conn = GetOpenConnection(dbConfig)) {
                return conn.DbMapping.Load<ForeignKey>("id = " + id.ToString(), true).FirstOrDefault();
            }
        }

        /// <summary>
        /// Load key objects from database
        /// </summary>
        /// <param name="dbConfig">The current profile config</param>
        public static List<ForeignKey> Load(DbConfig dbConfig) {
            using (IDatabase conn = GetOpenConnection(dbConfig)) {
                return conn.DbMapping.Load<ForeignKey>("id > 0", true);
            }
        }

        /// <summary>
        /// Load key objects from database
        /// </summary>
        /// <param name="dbConfig">The current profile config</param>
        /// <param name="tableId">The table id</param>
        internal static List<ForeignKey> Load(DbConfig dbConfig, int tableId) {
            using (IDatabase conn = GetOpenConnection(dbConfig)) {
                return conn.DbMapping.Load<ForeignKey>(conn.Enquote("foreign_table_id") + " = " + tableId.ToString(), true);
            }
        }

        /// <summary>
        /// Load key objects from database
        /// </summary>
        /// <param name="dbConfig">The current profile config</param>
        /// <param name="keyId">The primary key id</param>
        internal static List<ForeignKey> LoadForeignKeysRelatedToKey(DbConfig dbConfig, int keyId) {
            using (IDatabase conn = GetOpenConnection(dbConfig)) {
                string filter = conn.Enquote("key_id") + " = (" + keyId + ")";
                return conn.DbMapping.Load<ForeignKey>(filter, true);
            }
        }

        /// <summary>
        /// Load key objects from database
        /// </summary>
        /// <param name="dbConfig">The current profile config</param>
        /// <param name="tableId">The table id</param>
        internal static List<ForeignKey> LoadForeignKeysRelatedToTable(DbConfig dbConfig, int tableId, IEnumerable<Key> tablePrimaryKeys) {
            string keyIds = string.Join(",", tablePrimaryKeys.Select(k => k.Id));
            using (IDatabase conn = GetOpenConnection(dbConfig)) {
                string filter = conn.Enquote("foreign_table_id") + " = " + tableId.ToString();
                if (!string.IsNullOrEmpty(keyIds)) filter += " OR " + conn.Enquote("key_id") + " IN (" + keyIds + ")";
                return conn.DbMapping.Load<ForeignKey>(filter, true);
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
        #endregion [ Load and Save items to database ]

        #endregion [ Public Methods ]
    }
}
