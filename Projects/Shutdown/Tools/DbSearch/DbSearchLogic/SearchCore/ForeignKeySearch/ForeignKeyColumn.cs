using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DbAccess;
using DbAccess.Structures;
using DbSearchBase.Interfaces;

namespace DbSearchLogic.SearchCore.ForeignKeySearch {
    /// <summary>
    /// Represents a foreign key column
    /// </summary>
    [DbTable("foreign_key_column", ForceInnoDb = true)]
    public class ForeignKeyColumn :  DbSearchDatabase.Structures.DatabaseObjectBase<int>, IKeyColumn {

        #region [ Properties ]
            
        /// <summary>
        /// The foreignkeyID
        /// </summary>
        [DbForeignKey("foreign_key_id", "foreign_keys", "id")]
        [DbColumn("foreign_key_id", IsInverseMapping = true)]
        public ForeignKey ForeignKey { get; set; }

        /// <summary>
        /// The column index
        /// </summary>
        [DbColumn("foreign_key_column_id", IsInverseMapping = true)]
        public int ColumnId { get; set; }

        /// <summary>
        /// The table name
        /// </summary>
        public string TableName { get; set; }

        /// <summary>
        /// The column name
        /// </summary>
        public  string ColumnName { get; set; }

        /// <summary>
        /// The column type
        /// </summary>
        public DbColumnTypes ColumnType { get; set; }

        /// <summary>
        /// Display string
        /// </summary>
        public string DisplayString {
            get { return TableName + "." + ColumnName; }
        }

        #endregion [ Properties ]

        #region [ Constructor ]

        /// <summary>
        /// Constructor
        /// </summary>
        public ForeignKeyColumn() { 
        }
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="columnId">The column ID</param>
        /// <param name="tableName">The container table name</param>
        /// <param name="columnName">The column name</param>
        public ForeignKeyColumn(int columnId, string tableName, string columnName) {
            ColumnId = columnId;
            TableName = tableName;
            ColumnName = columnName;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="columnId">The column ID</param>
        /// <param name="tableName">The container table name</param>
        /// <param name="columnName">The column name</param>
        /// <param name="columnType">The column type</param>
        public ForeignKeyColumn(int columnId, string tableName, string columnName, DbColumnTypes columnType) {
            ColumnId = columnId;
            TableName = tableName;
            ColumnName = columnName;
            ColumnType = columnType;
        }

        #endregion [ Constructor ]
    }
}
