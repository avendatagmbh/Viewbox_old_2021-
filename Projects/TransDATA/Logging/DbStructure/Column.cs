using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Config;
using Logging.Interfaces.DbStructure;
using DbAccess;
using Utils;

namespace Logging.DbStructure {
    
    [DbTable("log_columns")]
    internal class Column : NotifyPropertyChangedBase, IColumn {
        #region Id
        [DbColumn("id", AllowDbNull = false)]
        [DbPrimaryKey]
        internal long Id { get; set; }
        #endregion

        #region Table
        [DbColumn("log_table_id", IsInverseMapping = true)]
        internal Table TableDbMapping { get; set; }

        public ITable Table { get { return TableDbMapping; } }
        #endregion

        #region ColumnId
        [DbColumn("column_id", AllowDbNull = false)]
        public long ColumnId { get; set; }
        #endregion
    }
}
