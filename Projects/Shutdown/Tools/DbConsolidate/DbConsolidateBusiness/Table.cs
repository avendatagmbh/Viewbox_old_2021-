using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DbAccess;

namespace DbConsolidateBusiness {
    public class Table {

        private string _originalName { get; set; }
        public string OriginalName { get { return _originalName; } }

        private string _name { get; set; }
        public string Name { get { return _name; } }

        private long _count { get; set; }
        public long Count { get { return _count; } }

        private List<string> _columnNames { get; set; }
        public List<string> ColumnNames { get { return _columnNames; } }

        private Dictionary<string, long> _uniqueKeyNamesAndLength { get; set; }
        public Dictionary<string, long> UniqueKeyNamesAndLength { get { return _uniqueKeyNamesAndLength; } }

        public Table(string originalName, string name, long count, List<string> columnNames) {
            _originalName = originalName;
            _name = name;
            _count = count;
            _columnNames = columnNames;
            _uniqueKeyNamesAndLength = new Dictionary<string, long>();
        }

        public void AddUniqueKeyNamesAndLength(IDatabase dd03lDb) { _uniqueKeyNamesAndLength = FindUniqueKeyNamesAndLength(dd03lDb); }

        private Dictionary<string, long> FindUniqueKeyNamesAndLength(IDatabase dd03lDb) {
            Dictionary<string, long> result = new Dictionary<string, long>();
            using (var reader = dd03lDb.ExecuteReader(string.Format("SELECT FIELDNAME,KEYFLAG,LENG FROM {0} WHERE TABNAME LIKE {1}", dd03lDb.Enquote("DD03L"), dd03lDb.GetSqlString(Name)))) {
                while (reader.Read()) {
                    string key = reader.GetString(1);
                    if (string.IsNullOrEmpty(key))
                        continue;

                    result[reader.GetString(0).ToLower()] = Convert.ToInt64(reader.GetString(2));
                }
            }
            return result;
        }
    }
}
