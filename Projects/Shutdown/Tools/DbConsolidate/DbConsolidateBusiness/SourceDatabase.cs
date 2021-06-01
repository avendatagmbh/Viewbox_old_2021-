using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using DbAccess;
using DbAccess.Structures;

namespace DbConsolidateBusiness {
    public class SourceDatabase {
        private DbConfig _dbconfig { get; set; }
        public DbConfig DbConfig { get { return _dbconfig; } }

        private string _prefix { get; set; }
        public string Prefix { get { return _prefix; } }

        private string _suffix { get; set; }
        public string Suffix { get { return _suffix; } }

        private string _replacePrefixWith { get; set; }
        public string ReplacePrefixWith { get { return _replacePrefixWith; } }

        private string _replaceSuffixWith { get; set; }
        public string ReplaceSuffixWith { get { return _replaceSuffixWith; } }

        private List<Table> _tables { get; set; }
        public List<Table> Tables { get { return _tables; } }

        private List<string> _tableNames { get; set; }
        public List<string> TableNames { get { return _tableNames; } }

        public SourceDatabase(DbConfig dbconfig, string dd03lDatabase, string prefix, string suffix, string replacePrefixWith, string replaceSuffixWith, List<string> tableNames) { 
            _dbconfig = dbconfig;
            _prefix = prefix;
            _suffix = suffix;
            _replacePrefixWith = replacePrefixWith;
            _replaceSuffixWith = replaceSuffixWith;
            _tableNames = tableNames;
            InitTables(dd03lDatabase);
        }

        void InitTables(string dd03lDatabase) {
            _tables = new List<Table>();

            using (var db = ConnectionManager.CreateConnection(DbConfig)) {
                var dd03lDbConfig = DbConfig.Clone() as DbConfig;
                dd03lDbConfig.DbName = dd03lDatabase;
                using(var dd03lDb = ConnectionManager.CreateConnection(dd03lDbConfig)) {
                    foreach (var tableName in _tableNames) {
                        string newTableName = tableName;
                        if(newTableName.StartsWith(Prefix)) newTableName = ReplacePrefixWith + newTableName.Substring(Prefix.Length);
                        if(newTableName.EndsWith(Suffix)) newTableName = newTableName.Substring(newTableName.Length - Suffix.Length) + ReplaceSuffixWith;

                        var columns = db.GetColumnNames(tableName);
                        columns.Remove("_row_no_");

                        var table = new Table(tableName, newTableName, db.CountTable(tableName), columns);
                        table.AddUniqueKeyNamesAndLength(dd03lDb);
                        _tables.Add(table);
                    }
                }
            }
        }
    }
}
