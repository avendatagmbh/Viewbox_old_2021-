using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Config.DbStructure;
using DbAccess.Structures;

namespace Config.Interfaces.DbStructure {
    public interface IColumn {
        long Id { get; set; }
        string Name { get; set; }
        bool DoExport { get; set; }
        int MaxLength { get; set; }
        string Comment { get; set; }
        string TypeName { get; set; }
        bool HasComment { get; }
        bool DoDbUpdate { get; set; }

        DbColumnTypes DbType { get; set; }
        ColumnTypes Type { get; set; }
        string DefaultValue { get; set; }
        bool AllowDBNull { get; set; }
        bool AutoIncrement { get; set; }
        int NumericScale { get; set; }
        bool IsPrimaryKey { get; set; }
        bool IsUnsigned { get; set; }
        bool IsIdentity { get; set; }
        int OrdinalPosition { get; set; }
    }
}
