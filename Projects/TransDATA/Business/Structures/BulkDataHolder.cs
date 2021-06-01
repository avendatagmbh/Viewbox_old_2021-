using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using DbAccess.Interaction;
using DbAccess.Structures;

namespace Business.Structures {
    public class BulkDataHolder : IBulkDataHolder {
        public IDataReader Reader { get; set; }
        public int BatchSize { get; set; }
        public string TableSchema { get; set; }
        public string TableName { get; set; }
        public SqlRowsCopiedEventHandler Progress { get; set; }
        public IEnumerable<DbColumnInfo> ColumnInfos { get; set; }
    }
}
