using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DbAccess;
using DbAccess.Structures;
using DbSearchBase.Interfaces;

namespace DbSearchLogic.SearchCore.KeySearch {
    /// <summary>
    /// The stored keyColumn item
    /// </summary>
    [DbTable("key_candidate_column", ForceInnoDb = true)]
    public class KeyCandidateColumn : DbSearchDatabase.Structures.DatabaseObjectBase<int>, IColumn {
        
        #region [ Public Properties ]
            
        /// <summary>
        /// The key id
        /// </summary>
        [DbForeignKey("key_candidate_id", "key_candidate", "id")]
        [DbColumn("key_candidate_id", IsInverseMapping = true)]
        public KeyCandidate Key { get; set; }

        /// <summary>
        /// The id of the column
        /// </summary>
        [DbColumn("key_candidate_column_id")]
        public int ColumnId { get; set; }

        /// <summary>
        /// The name of the column
        /// </summary>
        [DbColumn("key_candidate_column_name")]
        public string ColumnName { get; set; }

        /// <summary>
        /// The type of the column
        /// </summary>
        [DbColumn("key_candidate_column_type")]
        public DbColumnTypes ColumnType { get; set; }
    
        #endregion [ Public Properties ]
        
        #region [ Constructor ]

        /// <summary>
        /// Constructor
        /// </summary>
        public KeyCandidateColumn() {

        }

        /// <summary>
        /// Constructor
        /// </summary>
        public KeyCandidateColumn(DbSearchBase.Interfaces.IColumn column) {
            this.ColumnId = column.ColumnId;
            this.ColumnName = column.ColumnName;
            this.ColumnType = column.ColumnType;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public KeyCandidateColumn(string columName, int columnId, DbColumnTypes columnType) {
            ColumnName = columName;
            ColumnId = columnId;
            ColumnType = columnType;
        }

        #endregion [ Constructor ]
    }
}
