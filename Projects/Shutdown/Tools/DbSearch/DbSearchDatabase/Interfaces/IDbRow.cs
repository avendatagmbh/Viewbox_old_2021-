using System.Collections.Generic;
using DbSearchDatabase.TableRelated;

namespace DbSearchDatabase.Interfaces {
    public interface IDbRow {
        List<IDbRowEntry> RowEntries { get; set; }
    }
}