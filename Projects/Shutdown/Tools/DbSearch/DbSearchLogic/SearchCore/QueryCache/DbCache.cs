using System;
using System.Collections.Generic;
using System.Data;
using DbAccess;
using System.Data.Common;
using System.Diagnostics;
using System.Globalization;
using DbAccess.Structures;
using DbSearchLogic.SearchCore.SearchMatrix;
using DbSearchLogic.SearchCore.Structures;
using log4net;
using AV.Log;

namespace DbSearchLogic.SearchCore.QueryCache {

    public static class DbCache {

        internal static ILog _log = LogHelper.GetLogger();

        // Constants for distinct db
        private const string TABLE_ID_COLUMN = "tid";
        private const string TABLENAME_COLUMN = "name";
        private const string COLUMN_ID_COLUMN = "cid";
        private const string COLUMNNAME_COLUMN = "name";
        private const string DISTINCT_DB_PREFIX = "_idx";
        private const string DISTINCT_COLUMN_VALUE_NAME = "value";
        private const string DISTINCT_TABLE_NAME_TABLES = "tables";
        private const string DISTINCT_TABLE_NAME_COLUMNS = "columns";
        private const string SIZE_COLUMN = "size";

        #region Properties


        #endregion Properties


        #region Constructor

        static DbCache() {
        }

        #endregion Constructor

        #region GetColumn
        /// <summary>
        /// Gets the values of the specified database column. If possible, the values are returned from cache, otherwise they are loaded from database.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="tableColumn">The table column.</param>
        /// <param name="db">The db.</param>
        /// <param name="svm">The SVM.</param>
        /// <param name="info">The info.</param>
        /// <param name="dbSpecific">The db specific.</param>
        /// <returns></returns>
        internal static IDbColumnEnumerator GetColumn(
            string tableName,
            TableColumn tableColumn,
            IDatabase db,
            SearchValueMatrix svm,
            QueryInfo info
            ) {

            string sql = GetCommandString(tableName, tableColumn, db, info);

            // Return, if no command string is avaliable.
            if (sql == null) return null;

            IDataReader reader = db.ExecuteReader(sql);
            if (!((DbDataReader)reader).HasRows) {
                reader.Close();
                reader.Dispose();
                return null;
            }
            return new ReaderEnumerator(reader, sql);
        }
        #endregion GetColumn

        #region GetCommandString

        private static string GetCommandString(
            string tableName,
            TableColumn tableColumn,
            IDatabase db,
            QueryInfo info
            ) {
            string distinctTableName = (info.DistinctDbExists ? GetDistinctTableName(tableName, tableColumn.Name, db) : null);

            return GetFilterCommandString(tableName, tableColumn, db,distinctTableName);
        }
        #endregion GetCommandString

        #region GetFilterCommandString

        private static string GetFilterCommandString(
            string tableName, 
            TableColumn tableColumn, 
            IDatabase db, 
            string distinctTableName) {
            
            string oDbCommand;

            // get only relevant values
            string searchTableName;
            string searchDbName;
            string searchColumnName;
            

            if (distinctTableName != null) {

                ////////////////////////////////////////////////////////////
                // load values from distinct database table
                ////////////////////////////////////////////////////////////
                
                //CHANGED
                searchDbName = db.DbConfig.DbName + DISTINCT_DB_PREFIX;
                //searchDbName = db.Database + DISTINCT_DB_PREFIX;
                searchTableName = distinctTableName;
                searchColumnName = DISTINCT_COLUMN_VALUE_NAME;

                //if (tableColumn.Type == DbColumnTypes.DbDateTime) {
                //    oDbCommand =
                //        "SELECT IF(" + db.Enquote(searchColumnName) + " = '0000-00-00',NULL," + db.Enquote(searchColumnName) + ")" +
                //        "," + db.Enquote("Id") + " " +
                //        "FROM " + db.Enquote(searchDbName, searchTableName);

                //} else {
                    oDbCommand =
                        "SELECT " + db.Enquote(searchColumnName) + "," + db.Enquote("Id") + " " + 
                        "FROM " + db.Enquote(searchDbName, searchTableName);

                //}
            
            } else {

                ////////////////////////////////////////////////////////////
                // load values from complete database table
                ////////////////////////////////////////////////////////////

                //searchDbName = db.Database;
                searchDbName = db.DbConfig.DbName;
                searchTableName = tableName;
                searchColumnName = tableColumn.Name;

                //if (tableColumn.Type == DbColumnTypes.DbDateTime) {
                //    oDbCommand =
                //        "SELECT DISTINCT IF(" + db.Enquote(searchColumnName) + " = '0000-00-00',NULL," +
                //        db.Enquote(searchColumnName) + ") "  +
                //        "FROM " + db.Enquote(searchDbName, searchTableName);

                //} 
                //else {
                    oDbCommand = 
                        "SELECT DISTINCT " + db.Enquote(searchColumnName) + " " +
                        "FROM " + db.Enquote(searchDbName, searchTableName);
                //}

            }

            return oDbCommand;
        }
        #endregion GetFilterCommandString

        #region GetDistinctTableName
        private static string GetDistinctTableName(string table, string column, IDatabase conn) {
            IDataReader reader = null;
            string dbName = conn.DbConfig.DbName + DISTINCT_DB_PREFIX;
            try {
                string sql =
                    "SELECT " + conn.Enquote(COLUMN_ID_COLUMN) + "," + conn.Enquote("size") + " " +
                    "FROM " + conn.Enquote(dbName) + "." + conn.Enquote(DISTINCT_TABLE_NAME_TABLES) + " t " +
                    "INNER JOIN " + conn.Enquote(dbName) + "." + conn.Enquote(DISTINCT_TABLE_NAME_COLUMNS) + " c " +
                    "ON t." + conn.Enquote(TABLE_ID_COLUMN) + " = c." + conn.Enquote(TABLE_ID_COLUMN) + " " +
                    "WHERE " +
                    "t." + conn.Enquote(TABLENAME_COLUMN) + " = '" + table + "' AND " +
                    "c." + conn.Enquote(COLUMNNAME_COLUMN) + " = '" + column + "'";
                        //+ "' AND " + conn.Enquote(SIZE_COLUMN) + " > 0";

                // Setting tiimeout on MySQL Server
                //MySqlCommand cmd = new MySqlCommand("set net_write_timeout=99999; set net_read_timeout=99999", (MySqlConnection)conn.Connection);
                //cmd.ExecuteNonQuery();

                reader = conn.ExecuteReader(sql);

                if (!reader.Read()) {
                    _log.Log(LogLevelEnum.Warn, "Keine Distinct-Tabelle für Spalte " + table + "." + column + " gefunden!", true);
                    return null;
                } else {
                    //if (Convert.ToInt64(reader[1]) == 0)
                    //    return null;
                    return "idx_" + reader[0].ToString();
                }

            } catch (Exception ex) {
                _log.Log(LogLevelEnum.Error, "Ein Fehler ist aufgetreten beim Abfragen der Distinct Tabellen für Tabelle " + table + " und Spalte " + column + Environment.NewLine + ex.Message);
                return null;
            }
            finally {
                if (reader != null) {
                    reader.Close();
                    reader.Dispose();
                }
            }
        }
        #endregion GetDistinctTableName
    }
}

