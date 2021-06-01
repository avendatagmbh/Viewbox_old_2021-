// --------------------------------------------------------------------------------
// author: Sebastian Vetter
// since: 2012-05-14
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Globalization;
using DatabaseManagement.Localisation;
using System.Linq;
using DbAccess;

namespace DatabaseManagement.DbUpgrade {
    public class ReconciliationUpdater16 {

        private static IDatabase _conn;

        /// <summary>
        /// Default value, Column name in destination table
        /// </summary>
        private static Dictionary<string, string> ColumnDefaultsReconPrevYear {
            get {
                return new Dictionary<string, string> {
                    {"type", "0"},
                    {"transfer_kind", "2"},
                    {"name", "NULL"},
                    {"comment", "'Vorjahreswerte'"}
                };
            }
        }

        /// <summary>
        /// Column name in destination table, default value
        /// </summary>
        private static Dictionary<string, string> ColumnDefaultsReconTranPrevYear {
            get {
                return new Dictionary<string, string> {
                    {"transaction_type", "4"}
                };
            }
        }

        /// <summary>
        /// Column name in old table , Column name in new table
        /// </summary>
        private static Dictionary<string, string> ColumnChangeReconTran {
            get {
                return new Dictionary<string, string> {
                    {"id", null},
                    {"document_id", null},
                    {"header_id", "reconciliation_id"},
                    {"value_id", "element_id"},
                    {"transfer_value", "value"},
                    {"0", "transaction_type"}
                };
            }
        }

        /// <summary>
        /// Column name in source table , Column name in destination table
        /// </summary>
        private static Dictionary<string, string> ColumnChangeRecon {
            get {
                return new Dictionary<string, string> {
                    {"id", null},
                    {"document_id", null},
                    {"comment", null},
                    {"name", null},
                    {"type", "transfer_kind"},
                    {"4", "type"}
                };
            }
        }

        /// <summary>
        /// Old table name, referenced dictionary
        /// </summary>
        private static Dictionary<string, Dictionary<string, string>> ColumnTableDict {
            get {
                return new Dictionary<string, Dictionary<string, string>> {
                    {"transfer_hbst_heads", ColumnChangeRecon},
                    {"transfer_hbst_lines", ColumnChangeReconTran}
                };
            }
        }

        public static void UpgradeReconTables(IDatabase databaseConn, bool setIdentity = false) {
            _conn = databaseConn;

            string tableName = "transfer_hbst_heads";
            var tableNameNew = "reconciliations";
            
            UpgradeReconTable(tableName, tableNameNew, setIdentity);

            tableName = "transfer_hbst_lines";
            tableNameNew = "reconciliation_transactions";
            string where = ", " + _conn.Enquote("values_gaap") + string.Format(" g WHERE g.{0}=t.{1} AND ", databaseConn.Enquote("id"), databaseConn.Enquote("value_id")) + _conn.Enquote("transfer_value") + " IS NOT null";
            UpgradeReconTable(tableName, tableNameNew, setIdentity, where);
        }

        private static void UpgradeReconTable(string tableNameOld, string tableNameNew,  bool setIdentity = true, string where = "") {
            
            var sourceColumnsList = new List<string>();
            var sourceCols = _conn.GetColumnNames(tableNameOld);
            var destinationColumnsList = new List<string>();
            var destinationCols = _conn.GetColumnNames(tableNameNew);
            foreach (var columnEntry in ColumnTableDict[tableNameOld]) {
                if (columnEntry.Key.Equals("value_id")) {
                    sourceColumnsList.Add("g" + "." + _conn.Enquote("elem_id")); //_conn.Enquote(columnEntry.Key));
                }
                else {

                    // The KeyEntry is an existing columnName in the old table
                    if (sourceCols.Contains(columnEntry.Key)) {
                        sourceColumnsList.Add("t" + "." + _conn.Enquote(columnEntry.Key));
                    } else {
                        // The KeyEntry is not an existing column in the old table so we handle it as value
                        //sourceColumnsList.Add("'" + columnEntry.Key + "'");
                        sourceColumnsList.Add(columnEntry.Key);
                    }
                }

                if (destinationCols.Contains(columnEntry.Value)) {
                    destinationColumnsList.Add(_conn.Enquote(columnEntry.Value));
                } else if (string.IsNullOrEmpty(columnEntry.Value)) {
                    destinationColumnsList.Add(_conn.Enquote(columnEntry.Key));
                }
                else {
                    // The KeyEntry is not an existing column in the old table so we handle it as value
                    //destinationColumnsList.Add("'" + columnEntry.Value + "'");
                    destinationColumnsList.Add(columnEntry.Value);
                    System.Diagnostics.Debug.Assert(false, "We are in the else!!!");
                }
            }

            
                
                //var valueId =
                //    _conn.ExecuteScalar("SELECT " + _conn.Enquote("elem_id") + " FROM " + _conn.Enquote("values_gaap") +
                //                        " WHERE " + _conn.Enquote("id") + "=" +
                //                        entry.GaapId.ToString(CultureInfo.InvariantCulture));

            //var counter = 0;
            //foreach (var columnEntry in ColumnTableDict[tableNameOld].Values) {
            //    // The KeyEntry is an existing columnName in the new table
            //    if (destinationCols.Contains(columnEntry)) {
            //        destinationColumnsList.Add(_conn.Enquote(columnEntry));
            //    }
            //    else if (string.IsNullOrEmpty(columnEntry)) {
            //        ColumnTableDict[tableNameOld].Keys
            //    }
            //    else {
            //        // The KeyEntry is not an existing column in the old table so we handle it as value
            //        destinationColumnsList.Add("'" + columnEntry + "'");
            //    }
            //    counter++;
            //}

            string columnString = String.Join(",", destinationColumnsList);
            string replacedColumns = String.Join(",", sourceColumnsList);


            string sql = "INSERT INTO " + _conn.Enquote(tableNameNew) + " (" + columnString + ") SELECT " +
                         replacedColumns + " FROM " + _conn.Enquote(tableNameOld) + " t " + where;

            if (_conn.DbConfig.DbType == "SQLServer" && setIdentity)
                _conn.ExecuteNonQuery("SET IDENTITY_INSERT dbo." + _conn.Enquote(tableNameNew) + " ON");
            _conn.ExecuteNonQuery(sql);
            if (_conn.DbConfig.DbType == "SQLServer" && setIdentity)
                _conn.ExecuteNonQuery("SET IDENTITY_INSERT dbo." + _conn.Enquote(tableNameNew) + " OFF");

        }

        public class ReconInfo {
            public ReconInfo(int id, int docId, object valPrevYear) { 
                DocumentId = docId;
                ValuePrevYear = valPrevYear;
                Id = id;
            }
            public ReconInfo(object id, object docId, object valPrevYear, object gaapId) { 
                DocumentId = Convert.ToInt32(docId);
                ValuePrevYear = valPrevYear;
                Id = Convert.ToInt64(id);
                GaapId = Convert.ToInt64(gaapId);
            }
            public int NewInsertId;
            public object ValuePrevYear;
            public int DocumentId;
            public long Id;
            public long GaapId;
        }

        /// <summary>
        /// Transfer the transfer_values_previous_year from the old table "transfer_hbst_lines" to the new ones "reconciliations" and "reconciliation_transactions".
        /// </summary>
        /// <param name="conn">The connection to the database.</param>
        /// <param name="setIdentity"></param>
        public static void UpgradePreviousYearValues(IDatabase conn, bool setIdentity = true) {

            System.Data.IDataReader reader = null;
                const string tableNameOld = "transfer_hbst_lines";
                string condition = string.Format("{0} IS NOT null", conn.Enquote("transfer_value"));

            HashSet<ReconInfo> reconInfos = new HashSet<ReconInfo>();

            try {
                _conn = conn;
                
                reader = _conn.GetReaderForTable(tableNameOld, condition);
                while (reader.Read()) {

                    reconInfos.Add(new ReconInfo(reader["id"], reader["document_id"],
                                                 reader.IsDBNull(reader.GetOrdinal("transfer_value_previous_year"))
                                                     ? null
                                                     : reader["transfer_value_previous_year"], reader["value_id"]));
                }
            } finally {
                if (reader != null && !reader.IsClosed) {
                    reader.Close();
                }
            }

            const string reconciliationsTableNameNew = "reconciliations";
            const string transactionsTableNameNew = "reconciliation_transactions";
            int didItForDocId = 0;
            string reconId = string.Empty;
            foreach (var entry in reconInfos) {

                if (didItForDocId != entry.DocumentId) {
                 
                var reconDict = ColumnDefaultsReconPrevYear;
                reconDict.Add("document_id", entry.DocumentId.ToString(CultureInfo.InvariantCulture));
                reconId = (GetMaxInsertId(reconciliationsTableNameNew) + 1).ToString(CultureInfo.InvariantCulture);
                reconDict.Add("id", reconId);
                Execute(reconDict, tableNameOld, reconciliationsTableNameNew, "LIMIT 1");

                    didItForDocId = entry.DocumentId;
                }

                var valueId =
                    _conn.ExecuteScalar("SELECT " + _conn.Enquote("elem_id") + " FROM " + _conn.Enquote("values_gaap") +
                                        " WHERE " + _conn.Enquote("id") + "=" +
                                        entry.GaapId.ToString(CultureInfo.InvariantCulture));

                //entry.NewInsertId = reconId;

                if (entry.ValuePrevYear == null) continue;

                var reconTranDict = ColumnDefaultsReconTranPrevYear;
                reconTranDict.Add("reconciliation_id", reconId);
                reconTranDict.Add("value", entry.ValuePrevYear.ToString());
                reconTranDict.Add("document_id", entry.DocumentId.ToString(CultureInfo.InvariantCulture));
                reconTranDict.Add("element_id", valueId.ToString());
                reconTranDict.Add("id", (GetMaxInsertId(transactionsTableNameNew) + 1).ToString(CultureInfo.InvariantCulture));

                Execute(reconTranDict, tableNameOld, transactionsTableNameNew, " WHERE id = "+ entry.Id + " LIMIT 1");
            }

            //try {

            //    foreach (var reconPrevYearEntry in reconInfos) {
                    
            //        var reconTranDict = ColumnDefaultsReconTranPrevYear;
            //        reconTranDict.Add("reconciliation_id", reconPrevYearEntry.NewInsertId.ToString(CultureInfo.InvariantCulture));
            //        reconTranDict.Add("value", reconPrevYearEntry.ValuePrevYear.ToString());
                
            //        var tableNameNew = "reconciliation_transactions";
                
            //        Execute(reconTranDict, tableNameOld, tableNameNew);
            //    }

            //} catch (Exception) {
                
            //    throw;
            //}

        }


        #region GetMaxInsertId
        protected static Int64 GetMaxInsertId(string tableName, string idColumn = "id") {
            object obj = _conn.ExecuteScalar("SELECT MAX(" + _conn.Enquote(idColumn) + ") FROM " + _conn.Enquote(tableName));
            if (obj is DBNull) return 0;
            else return Convert.ToInt64(obj);
        }
        #endregion

        /// <summary>
        /// Get the SQL code by calling <see cref="GetSqlCode"/> and execute the INSERT INTO statement.
        /// </summary>
        private static void Execute(Dictionary<string, string> columnDefaults, string oldTableName, string newTableName, string condition = "", bool setIdentity = false) {

            var sql = GetSqlCode(columnDefaults, oldTableName, newTableName, condition);

                if (_conn.DbConfig.DbType == "SQLServer" && setIdentity)
                    _conn.ExecuteNonQuery("SET IDENTITY_INSERT dbo." + _conn.Enquote(newTableName) + " ON");
                _conn.ExecuteNonQuery(sql);
                if (_conn.DbConfig.DbType == "SQLServer" && setIdentity)
                    _conn.ExecuteNonQuery("SET IDENTITY_INSERT dbo." + _conn.Enquote(newTableName) + " OFF");
        }

        /// <summary>
        /// Get the SQL "INSERT INTO ..." code to get the data from oldTableName to newTableName.
        /// </summary>
        /// <param name="columnDefaults">A dictionary that contains "column name" and "default value". 
        /// If there are columns in the newTableName that don't get a default value the value from oldTableName will be taken.</param>
        /// <param name="oldTableName">Name of the source table where the non-default values come from.</param>
        /// <param name="newTableName">Name of the new (destination) table where the new values will be inserted.</param>
        /// <param name="condition">A condition that is attached to the SQL Statement eg. "WHERE x = '2'"</param>
        /// <returns></returns>
        private static string GetSqlCode(Dictionary<string, string> columnDefaults, string oldTableName, string newTableName, string condition = "") {

               var intoColumnsList = new List<string>();
                var fromColumnsList = new List<string>();
            
                var destinationCols1 = _conn.GetColumnNames(newTableName);


            // for each column where we've got a default value
                foreach (var columnEntry in columnDefaults) {
                //fromColumnsList.Add("'" + columnEntry.Value + "'");
                fromColumnsList.Add(columnEntry.Value);
                intoColumnsList.Add(_conn.Enquote(columnEntry.Key));
                //sourceColumnsList.Add(_conn.Enquote(columnEntry.Key));
                //destinationColumnsList.Add("'" + columnEntry.Value + "'");
                destinationCols1.Remove(columnEntry.Key);
            }

            

            // for each column where we take the value from the old table
            foreach (var columnName in destinationCols1) {
                var val = ColumnTableDict[oldTableName].FirstOrDefault(cn => !string.IsNullOrEmpty(cn.Value) && cn.Value.Equals(columnName));
                
                if (ColumnTableDict[oldTableName].ContainsValue(columnName) && val.Value != null) {
                    fromColumnsList.Add(_conn.Enquote(val.Key));
                    intoColumnsList.Add(_conn.Enquote(columnName));
                }
                else {
                    fromColumnsList.Add(_conn.Enquote(columnName));
                    intoColumnsList.Add(_conn.Enquote(columnName));
                }
            }

            string intoColumnString = String.Join(",", intoColumnsList);
            string fromColumnString = String.Join(",", fromColumnsList);

            string sql;

            if (destinationCols1.Count == 0) {
                sql = "INSERT INTO " + _conn.Enquote(newTableName) + " (" + intoColumnString + ") VALUES (" + fromColumnString + ")";
            } else {
                sql = "INSERT INTO " + _conn.Enquote(newTableName) + " (" + intoColumnString + ") SELECT " +
                         fromColumnString + " FROM " + _conn.Enquote(oldTableName) + " " + condition;
            }

            return sql;
        }
    }
}