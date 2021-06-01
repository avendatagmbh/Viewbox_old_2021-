using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DbAccess;
using DbAccess.Structures;

namespace DbSearchLogic.SearchCore.KeySearch{
    /// <summary>
    /// The stored keyColumn item
    /// </summary>
    [DbTable("key_column", ForceInnoDb = true)]
    public class KeyColumn : DbSearchDatabase.Structures.DatabaseObjectBase<int>, DbSearchBase.Interfaces.IKeyColumn {
        
        #region [ Public Properties ]
            
        /// <summary>
        /// The foreign key id
        /// </summary>
        [DbForeignKey("key_id", "keys", "id")]
        [DbColumn("key_id", IsInverseMapping = true)]
        public Key Key { get; set; }

        /// <summary>
        /// The name of the key column object 
        /// </summary>
        [DbColumn("key_column_id", IsInverseMapping = true)]
        public int ColumnId { get; set; }

        /// <summary>
        /// The name of the column
        /// </summary>
        public string ColumnName { get; set; }

        /// <summary>
        /// The type of the column
        /// </summary>
        public DbColumnTypes ColumnType { get; set; }
            
        #endregion [ Public Properties ]
        
        #region [ Constructor ]

        /// <summary>
        /// Constructor
        /// </summary>
        public KeyColumn(){
        }
        
        /// <summary>
        /// Constructor
        /// </summary>
        public KeyColumn(DbSearchBase.Interfaces.IColumn column) {
            this.ColumnId = column.ColumnId;
            this.ColumnName = column.ColumnName;
            this.ColumnType = column.ColumnType;
        }

        #endregion [ Constructor ]

        /// <summary>
        /// Display string
        /// </summary>
        public string DisplayString {
            get { return Key.TableName + "." + ColumnName; }
        }
    }
}
