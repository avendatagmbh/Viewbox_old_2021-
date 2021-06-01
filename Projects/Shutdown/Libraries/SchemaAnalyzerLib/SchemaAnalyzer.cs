// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2012-01-07
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DbAccess;
using DbAccess.Structures;

namespace SchemaAnalyzerLib {
    public sealed class SchemaAnalyzer {
        public SchemaAnalyzer(string hostname, string user, string password, string dbName, int port = 3306) {

            const int maxConnections = 10;

            ConnectionManager = new ConnectionManager(new DbConfig("MySQL") {
                Hostname = hostname,
                Username = user,
                Password = password,
                DbName = dbName,
                Port = port
            }, maxConnections);

            ConnectionManagerWork = new ConnectionManager(new DbConfig("MySQL") {
                Hostname = hostname,
                Username = user,
                Password = password,
                DbName = dbName + "_schema_info_tmp",
                Port = port
            }, maxConnections);

            ConnectionManagerResult = new ConnectionManager(new DbConfig("MySQL") {
                Hostname = hostname,
                Username = user,
                Password = password,
                DbName = dbName + "_schema_info",
                Port = port
            }, maxConnections);


            Progress = new SchemaAnalyzerProgress();
        }

        public SchemaAnalyzerProgress Progress { get; private set; }

        private ConnectionManager ConnectionManager { get; set; }
        private ConnectionManager ConnectionManagerWork { get; set; }
        private ConnectionManager ConnectionManagerResult { get; set; }

        private Dictionary<string, List<RelevantColumnInfo>> RelevantColumns { get; set; }
        private List<RelevantColumnInfo> RelevantDateColumns { get; set; }
        private List<RelevantColumnInfo> RelevantTimeColumns { get; set; }
        private List<RelevantColumnInfo> RelevantDateTimeColumns { get; set; }
        private List<RelevantColumnInfo> RelevantIntegerColumns { get; set; }

        #region Keys
        private readonly Dictionary<string, KeyCollection> _keys = new Dictionary<string, KeyCollection>();
        private Dictionary<string, KeyCollection> Keys { get { return _keys; } }
        #endregion

        public void Start() {
            using (var conn = ConnectionManager.GetConnection()) {
                conn.DropDatabaseIfExists(ConnectionManagerWork.DbConfig.DbName);
                conn.CreateDatabaseIfNotExists(ConnectionManagerWork.DbConfig.DbName);

                conn.DropDatabaseIfExists(ConnectionManagerResult.DbConfig.DbName);
                conn.CreateDatabaseIfNotExists(ConnectionManagerResult.DbConfig.DbName);
            }

            GetRelevantColumns();
            CreateDistinctTables();
            GetKeys();
            /*

         * 3. create global distinct table (only integer, char and varchar columns)
         * 4. analyse foreign keys using global distinct table
             * possible aproach?
             * foreach table in tables {
             *   for each key in table.Keys {
             *     foreach tuple in table.<key columns>.rows {
             *       foreach(value in tuple.value) {
             *         get all colums/table/row (ctr) combinations for this value from global distinct table
             *       }
             *       fkey-candidates == empty:
             *         all value combinations with equal table/row combinations ==> fkey-candidates
             *       otherwhise:
             *         fkey-candidates INTERSECT all value combinations with equal table/row combinations ==> fkey-candidates
             *     }
             *   }
             * }
             * Optimization using table distinct tables possible?
         */
        }

        #region GetRelevantColumns
        /// <summary>
        /// Gets all date, time, datetime and integer columns. Only columns of the same type will be comared.
        /// </summary>
        private void GetRelevantColumns() {
            RelevantColumns = new Dictionary<string, List<RelevantColumnInfo>>();
            RelevantDateColumns = new List<RelevantColumnInfo>();
            RelevantTimeColumns = new List<RelevantColumnInfo>();
            RelevantDateTimeColumns = new List<RelevantColumnInfo>();
            RelevantIntegerColumns = new List<RelevantColumnInfo>();

            Progress.Caption = "getting relevant columns...";

            DbTableInfo[] tableInfos;
            using (var conn = ConnectionManager.GetConnection()) {
                tableInfos = conn.GetTableInfos().ToArray();
            }

            Progress.ProgressMin = 0;
            Progress.ProgressCurrent = 0;
            Progress.ProgressMax = tableInfos.Length;

            var tasks = new Task[tableInfos.Length];
            for (var i = 0; i < tableInfos.Length; i++) {
                var tableInfo = tableInfos[i];
                tasks[i] = Task.Factory.StartNew(() => GetRelevantColumns(tableInfo));
            }

            Task.WaitAll(tasks);
        }

        private void GetRelevantColumns(DbTableInfo tableInfo) {

            try {
                using (var conn = ConnectionManager.GetConnection()) {
                    if (conn.CountTable(tableInfo.Name) == 0) return;
                    conn.AddColumnInfos(tableInfo);
                }

                foreach (var columnInfo in tableInfo.Columns.Where(columnInfo => !columnInfo.AutoIncrement)) {
                    // filter to relevant datatypes
                    switch (columnInfo.Type) {
                        case DbColumnTypes.DbDate:
                            AddRelevantColumnInfo(tableInfo, columnInfo, RelevantDateColumns);
                            break;

                        case DbColumnTypes.DbTime:
                            AddRelevantColumnInfo(tableInfo, columnInfo, RelevantTimeColumns);
                            break;

                        case DbColumnTypes.DbDateTime:
                            AddRelevantColumnInfo(tableInfo, columnInfo, RelevantDateTimeColumns);
                            break;

                        case DbColumnTypes.DbBigInt:
                        case DbColumnTypes.DbInt:
                        case DbColumnTypes.DbBool:
                            AddRelevantColumnInfo(tableInfo, columnInfo, RelevantIntegerColumns);
                            break;
                    }
                }

                Progress.ProgressCurrent++;

            } catch (Exception ex) {
                throw;
            }
        }

        private void AddRelevantColumnInfo(DbTableInfo tableInfo, DbColumnInfo columnInfo,
                                           List<RelevantColumnInfo> detailList) {
            lock (RelevantColumns) {
                if (!RelevantColumns.ContainsKey(tableInfo.Name))
                    RelevantColumns[tableInfo.Name] = new List<RelevantColumnInfo>();
            }

            var relevantColumnInfo = new RelevantColumnInfo(
                tableInfo.Name, columnInfo.Name, columnInfo.Type);

            RelevantColumns[tableInfo.Name].Add(relevantColumnInfo);
            detailList.Add(relevantColumnInfo);
        }
        #endregion GetRelevantColumns

        #region CreateDistinctTables
        private void CreateDistinctTables() {
            using (var conn = ConnectionManagerWork.GetConnection()) {
                foreach (var pair in RelevantColumns) {
                    foreach (var col in pair.Value) {
                        conn.ExecuteNonQuery(
                            "CREATE TABLE distinct_" + col.Id + " SELECT DISTINCT " +
                            conn.Enquote(col.ColumnName) + " FROM " +
                            conn.Enquote(ConnectionManager.DbConfig.DbName, col.TableName));
                    }
                }
            }
        }
        #endregion CreateDistinctTables

        #region GetKeys
        private void GetKeys() {

            // TODO: optimization: precompute counts for all relevant tables and distinct tables

            using (var conn = ConnectionManager.GetConnection()) {
                using (var connWork = ConnectionManagerWork.GetConnection()) {                                       
                    foreach (var pair in RelevantColumns) {

                        var remainingColumns = new List<RelevantColumnInfo>();

                        // analyse 1-col-keys
                        var tableCount = conn.CountTable(pair.Key);
                        foreach (var col in pair.Value) {
                            if (tableCount == connWork.CountTable("distinct_" + col.Id)) {
                                if (!Keys.ContainsKey(col.TableName))
                                    Keys[col.TableName] = new KeyCollection();

                                var keyInfo = new KeyInfo();
                                keyInfo.Columns.Add(col);
                                Keys[col.TableName].Add(keyInfo);
                            } else {
                                remainingColumns.Add(col);
                            }
                        }

                        GetKeys(pair.Key, remainingColumns, conn, connWork, tableCount);
                    }
                }
            }
        }

        private void GetKeys(string tableName, List<RelevantColumnInfo> remainingColumns, IDatabase conn, IDatabase connWork, long tableCount, int keyLen = 2) {
            if (remainingColumns.Count < keyLen) return;
            foreach (var combination in Utils.Math.Statistics.GetCombinations(remainingColumns, keyLen)) {
                long countProdukt = combination.Aggregate<RelevantColumnInfo, long>(
                    1, (current, col) => current*connWork.CountTable("distinct_" + col.Id));
                // (product of all actual distinct counts must be greater or equal to the total count, otherwhise the pkey condition "count = distinct count" cannot apply)
                if (countProdukt < tableCount) continue; // combination could not be a key
                string sql = "SELECT COUNT(*)  FROM (SELECT DISTINCT " +
                             string.Join(", ", combination.Select(col => connWork.Enquote(col.ColumnName))) + " FROM " +
                             conn.Enquote(tableName) + ") x";
                var count = (long)conn.ExecuteScalar(sql);
                if (count == tableCount) {
                    if (!Keys.ContainsKey(tableName))
                        Keys[tableName] = new KeyCollection();

                    var keyInfo = new KeyInfo();
                    foreach (var col in combination) {
                        keyInfo.Columns.Add(col);
                        remainingColumns.Remove(col);
                    }
                    Keys[tableName].Add(keyInfo);                    
                }

            }
            GetKeys(tableName, remainingColumns, conn, connWork, tableCount, keyLen + 1);
        }
        #endregion GetKeys

    }
}