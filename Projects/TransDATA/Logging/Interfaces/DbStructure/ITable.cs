using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Config.Interfaces.DbStructure;
using DbAccess;

namespace Logging.Interfaces.DbStructure {
    public interface ITable {

        long Count { get; set; }
        long TableId { get; set; }
        long ProfileId { get; set; }
        IEnumerable<IColumn> Columns { get; }
        string InputConfig { get; set; }
        string OutputConfig { get; set; }
        string Username { get; set;  }
        string Filter { get; set; }
        DateTime Timestamp { get; set; }
        long CountDest { get; set; }
        TimeSpan Duration { get; set; }
        ExportStates State { get; set; }
        string Error { get; set; }

        void AddColumn(IColumn column);
    }
}
