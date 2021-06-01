using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Logging.Interfaces.DbStructure {
    public interface IColumn {
        ITable Table { get; }
        long ColumnId { get; set; }
    }
}
