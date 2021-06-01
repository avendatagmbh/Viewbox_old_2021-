using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DbAccess.Structures;

namespace DbSearchBase.Interfaces {
    public interface IColumn {

        /// <summary>
        /// The id of the column
        /// </summary>
        int ColumnId { get; set; }

        /// <summary>
        /// The name of the column
        /// </summary>
        string ColumnName { get; set; }

        /// <summary>
        /// The type of the column
        /// </summary>
        DbColumnTypes ColumnType { get; set; }
    }
}
