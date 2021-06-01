using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DbSearchDatabase.Structures;

namespace DbSearchDatabase.Interfaces {
    public interface IDbResultSet : IDatabaseObject<int> {
        DateTime Timestamp { get; set; }
        string Comment { get; set; }
        string QuerySnapshot { get; set; }
        string ColumnResultsString { get; set; }
        void Save();
        bool Finished { get; set; }

    }
}
