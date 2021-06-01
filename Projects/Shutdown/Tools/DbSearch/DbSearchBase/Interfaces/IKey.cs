using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DbSearchBase.Interfaces {
    public interface IKey {

        int Id { get; set; }
        int TableId { get; set; }
        string TableName { get; set; }
        bool Processed { get; set; }

        List<IColumn> Columns { get; }

        string Label { get; }
        string DisplayString { get; }
        int ColumnOrderIndependentHash { get; }
    }

    public interface IKeyColumn : IColumn {

        string DisplayString { get; }
    }
}
