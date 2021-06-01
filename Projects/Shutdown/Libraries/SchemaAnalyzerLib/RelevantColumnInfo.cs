// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2012-01-07
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using DbAccess.Structures;

namespace SchemaAnalyzerLib {
    internal class RelevantColumnInfo {
        public RelevantColumnInfo(string tableName, string columnName, DbColumnTypes type) {
            Id = _curId++;
            TableName = tableName;
            ColumnName = columnName;
            Type = type;
        }

        private static int _curId = 0;

        public int Id { get; private set; }
        public DbColumnTypes Type { get; private set; }
        public string TableName { get; private set; }
        public string ColumnName { get; private set; }
        public override string ToString() { return TableName + "." + ColumnName; }
    }
}