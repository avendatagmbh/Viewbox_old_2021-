using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DbComparison.Business {
    public interface IDatabaseInformation {
        void GetTables(List<ComparisonResult.TableInfo> tables);
        void GetColumns(List<ComparisonResult.ColumnInfo> columns, Dictionary<string, List<ComparisonResult.ColumnInfo>> columnsDict, List<ComparisonResult.TableInfo> tables);
    }
}
