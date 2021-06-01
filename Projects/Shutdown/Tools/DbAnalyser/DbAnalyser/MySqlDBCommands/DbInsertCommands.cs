using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using DbAnalyser.Logging;
using DbAnalyser.Processing.NonSAP.Methods;
using MySql.Data.MySqlClient;

namespace DbAnalyser.MySqlDBCommands
{
    /**
     * This class contains every SQL INSERT function required for the process
     */
    public class DbInsertCommands : DbConnection
    {
        /**
         * This method inserts data to the analyse_database_info table
         */
        public static void InsertToAnalyseDatabaseInfo(ref List<AnalyseDatabaseInfo> infoDatabaseData)
        {
            using (var connection = new MySqlConnection(DestinationConnectionString))
            {
                try
                {
                    connection.Open();
                    var query = new StringBuilder();
                    query.Append(String.Format("INSERT IGNORE INTO `{0}`.`analyse_database_info` (TablesAll, TablesEmpty, TablesAnalysed, ColumnsAll, ColumsNull) VALUES ", AnalysticDatabase));
                    foreach (var item in infoDatabaseData)
                    {
                        query.Append("('" + item.tablesAll + "', " +
                            "'" + item.tablesEmpty + "', " +
                            "'" + item.tablesAnalysed + "', " +
                            "'" + item.columnsAll + "', " +
                            "'" + item.columnsNull + "'" +
                            "),");
                    }
                    query.Length--;
                    query.Append(";");

                    var insertCommand = new MySqlCommand(query.ToString(), connection) {CommandTimeout = 100000};
                    insertCommand.ExecuteNonQuery();
                    connection.Close();

                }
                catch (MySqlException ex)
                {
                    Logger.LogError(DateTime.Now + " - " + ex.ErrorCode + ": " + ex.Message);
                    Logger.LogError("     " + ex.Source + " -> " + ex.StackTrace);
                }
            }
        }

        /**
         * This method inserts data to the analyse_table_info table
         */
        public static void InsertToAnalyseTableInfo(ref List<AnalyseTableInfo> infoTableData)
        {
            using (var connection = new MySqlConnection(DestinationConnectionString))
            {
                try
                {
                    connection.Open(); 
                    var query = new StringBuilder();
                    query.Append("INSERT IGNORE INTO `" + AnalysticDatabase + "`.`analyse_table_info` (Name, TimeStamp, Duration, Comment, Description, AnalysationState, Type, Count) VALUES ");
                    foreach (var item in infoTableData)
                    {
                        query.Append("('" + item.name + "', " +
                            "'" + item.timeStamp.ToString("yyyy-MM-dd HH:mm") + "', " +
                            "'" + 0 + "', " +
                            "'" + item.comment + "', " +
                            "'" + item.description + "', " +
                            "'" + (int)item.analysationState + "', " +
                            "'" + item.type + "', " +
                            "'" + item.count + "'" +
                            "),");
                    }
                    query.Length--;
                    query.Append(";");

                    var insertCommand = new MySqlCommand(query.ToString(), connection) {CommandTimeout = 100000};
                    insertCommand.ExecuteNonQuery();
                    connection.Close();
                }
                catch (MySqlException ex)
                {
                    Logger.LogError(DateTime.Now + " - " + ex.ErrorCode + ": " + ex.Message);
                    Logger.LogError("     " + ex.Source + " -> " + ex.StackTrace);
                }
            }
        }

        /**
         * This method inserts data to the analyse_column_info table
         */
        public static void InsertToAnalyseColumnInfo(ref ConcurrentBag<AnalysedColumnInfo> infoColumnData)
        {
            using (var connection = new MySqlConnection(DestinationConnectionString))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT `TableId`, `Name` FROM `" + AnalysticDatabase + "`.`analyse_table_info`;";
                    var tableIds = new Dictionary<string, int>();

                    var selectCommand = new MySqlCommand(query, connection) {CommandTimeout = 100000};
                    using (var reader = selectCommand.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            tableIds.Add(reader.GetString("Name"), reader.GetInt32("TableId"));
                        }
                    }
                    var insertQuery = new StringBuilder();
                    insertQuery.Append("INSERT IGNORE INTO `" + AnalysticDatabase + "`.`analyse_column_info` (TableId, Name, Description, Length, Type) VALUES ");
                    foreach (var item in infoColumnData.Where(c => c.name != "_row_no_").OrderBy(c => c.tableId).ThenBy(c => c.colId))
                    {
                        try
                        {
                            int realId = tableIds[item.tableName];
                            insertQuery.Append("('" + realId + "', " +
                                "'" + item.name + "', " +
                                "'" + item.description + "', " +
                                "'" + item.length + "', " +
                                "'" + (int)item.typeId + "'" +
                                "),");
                        }
                        catch (Exception)
                        {
                            Logger.LogError("Missing table info from analyse schema - '" + item.tableName + "'");
                        }
                    }
                    insertQuery.Length--;
                    insertQuery.Append(";");

                    var insertCommand = new MySqlCommand(insertQuery.ToString(), connection) {CommandTimeout = 100000};
                    insertCommand.ExecuteNonQuery();
                    connection.Close();
                }
                catch (MySqlException ex)
                {
                    Logger.LogError(DateTime.Now + " - " + ex.ErrorCode + ": " + ex.Message);
                    Logger.LogError("     " + ex.Source + " -> " + ex.StackTrace);
                }
            }
        }

        /**
         * This method inserts data to the pattern_cache table
         */
        public static void InsertToPatternCache()
        {
            using (var connection = new MySqlConnection(DestinationConnectionString))
            {
                try
                {
                    connection.Open();
                    var query = new StringBuilder();
                    query.Append("INSERT IGNORE INTO `" + AnalysticDatabase + "`.`pattern_cache` (Pattern, Datatype) VALUES ");
                    foreach (var item in DateTimeAnalyser.getUsedRegexes())
                    {
                        query.Append("('" + item + "', " +
                            "'" + 5 + "'),");
                    }
                    foreach (var item in TimeAnalyser.GetUsedRegexes())
                    {
                        query.Append("('" + item + "', " +
                            "'" + 4 + "'),");
                    }
                    foreach (var item in DateAnalyser.getUsedRegexes())
                    {
                        query.Append("('" + item + "', " +
                            "'" + 3 + "'),");
                    }
                    query.Length--;
                    query.Append(";");

                    var insertCommand = new MySqlCommand(query.ToString(), connection) {CommandTimeout = 100000};
                    insertCommand.ExecuteNonQuery();
                    connection.Close();
                }
                catch (MySqlException ex)
                {
                    Logger.LogError(DateTime.Now + " - " + ex.ErrorCode + ": " + ex.Message);
                    Logger.LogError("     " + ex.Source + " -> " + ex.StackTrace);
                }
            }
        }

        /**
         * This method copies every table from the old schema to the one located in the *_final database 
         */
        public static void InsertTableIntoFinal(string tableName, List<AnalysedColumnInfo> colInfos, long transferRowCount, long from)
        {
            while (IsPaused)
            {
                Thread.Sleep(1000);
            }
            if (IsPaused == false)
            {
                var query = new StringBuilder();
                query.Append("INSERT IGNORE INTO `" + FinalDatabase + "`.`" + tableName + "`(");
                foreach (var colInfo in colInfos)
                {
                    query.Append("`" + colInfo.name + "`,");
                }
                query.Length--;
                query.Append(") ");
                query.Append(" SELECT ");
                foreach (var colInfo in colInfos)
                {
                    if ((colInfo.type == "DATETIME" || colInfo.type == "DATE") && colInfo.dateFormat != null)
                    {
                        query.Append(String.Format("STR_TO_DATE(`{0}`, '{1}'),", colInfo.name, colInfo.dateFormat));
                    }
                    else if ((colInfo.type == "DECIMAL" || colInfo.type == "DOUBLE"))
                    {
                        query.Append(colInfo.decimalFormat);
                    }
                    else
                    {
                        query.Append("`" + colInfo.name + "`,");
                    }
                }
                query.Length--;
                query.Append(" FROM `" + SourceDatabase + "`.`" + tableName + "` LIMIT " + from + ", " + InsertStepSize + ";");

                using (var connection = new MySqlConnection(DestinationConnectionString))
                {

                    connection.Open();
                    var copyCommand = new MySqlCommand(query.ToString(), connection) {CommandTimeout = 100000};
                    copyCommand.ExecuteNonQuery();

                }
            }
        }
        public static void FinishedInsertingToFinal(IAsyncResult res)
        {
            var command = res.AsyncState as MySqlCommand;
            if (command != null)
            {
                command.CommandTimeout = 100000;
                try
                {
                    command.EndExecuteNonQuery(res);
                    command.Connection.Close();
                    command.Connection.Dispose();
                }
                catch (MySqlException ex)
                {
                    Logger.LogError(DateTime.Now + " - " + ex.ErrorCode + ": " + ex.Message);
                    Logger.LogError("     " + ex.Source + " -> " + ex.StackTrace);
                }
            }
        }

        /**
         * Insert data to the 'col' table in the *_final_system database
         */
        public static void InsertToFinalSystemCol(ref ConcurrentBag<ViewboxColumnsInfo> columnsInfo)
        {
            var tableIds = new Dictionary<string, int>();
            using (var connection = new MySqlConnection(DestinationConnectionString))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT `Table_Id`, `Name` FROM `" + FinalSystemDatabase + "`.`table`;";
                    var selectCommand = new MySqlCommand(query, connection) {CommandTimeout = 100000};
                    using (var reader = selectCommand.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            tableIds.Add(reader.GetString("Name"), reader.GetInt32("Table_Id"));
                        }
                    }
                    connection.Close();
                }
                catch
                {
                }
            }

            using (var connection = new MySqlConnection(DestinationConnectionString))
            {
                try
                {
                    connection.Open();
                    var insertQuery = new StringBuilder();
                    insertQuery.Append("INSERT IGNORE INTO `" + FinalSystemDatabase + "`.`col` (table_id, name, is_selected, type, length, decimal_place, is_empty) VALUES ");
                    foreach (var item in columnsInfo.OrderBy(c => c.tableId).ThenBy(c => c.ordinal))
                    {
                        if (tableIds.Keys.Contains(item.tableName))
                        {
                            int realId = tableIds[item.tableName];
                            insertQuery.Append("('" + realId + "', " +
                                "'" + item.colName + "', " +
                                "'1',");
                            switch (item.dataTypeName)
                            {
                                case "FLOAT":
                                case "DOUBLE":
                                case "DECIMAL":
                                    insertQuery.Append("'numeric', ");
                                    break;
                                case "TINYINT":
                                case "BIGINT":
                                case "INT":
                                    insertQuery.Append("'integer', ");
                                    break;
                                case "DATE":
                                    insertQuery.Append("'date', ");
                                    break;
                                case "TIME":
                                    insertQuery.Append("'time', ");
                                    break;
                                case "DATETIME":
                                    insertQuery.Append("'datetime', ");
                                    break;
                                case "CHAR":
                                case "VARCHAR":
                                case "VARCHAR2":
                                    insertQuery.Append("'string', ");
                                    break;
                                case "TEXT":
                                    insertQuery.Append("'string', ");
                                    break;
                                default:
                                    insertQuery.Append("'string', ");
                                    break;
                            }
                            insertQuery.Append("'" + item.maxLength + "', ");
                            if (item.dataTypeName == "DECIMAL" || item.dataTypeName == "FLOAT" || item.dataTypeName == "DOUBLE")
                            {
                                insertQuery.Append("'" + item.decimals + "', ");
                            }
                            else
                            {
                                insertQuery.Append("0, ");
                            }
                            insertQuery.Append("'" + item.isEmpty + "'" + "),");
                        }
                    }
                    insertQuery.Length--;
                    insertQuery.Append(";");

                    var insertCommand = new MySqlCommand(insertQuery.ToString(), connection) {CommandTimeout = 100000};
                    insertCommand.ExecuteNonQuery();
                    connection.Close();
                }
                catch (MySqlException ex)
                {
                    Logger.LogError(DateTime.Now + " - " + ex.ErrorCode + ": " + ex.Message);
                    Logger.LogError("     " + ex.Source + " -> " + ex.StackTrace);
                }
            }
        }
        /**
         *  Insert data to the 'table' table in the *_final_system database
         */
        public static void InsertToFinalSystemTable(ref List<ViewboxTablesInfo> tablesInfo)
        {
            // TODO - get if exists
            var tableIds = new Dictionary<string, int>();
            using (var connection = new MySqlConnection(DestinationConnectionString))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT `TableId`, `Name` FROM `" + AnalysticDatabase + "`.`analyse_table_info`;";
                    var selectCommand = new MySqlCommand(query, connection) {CommandTimeout = 100000};
                    using (var reader = selectCommand.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            tableIds.Add(reader.GetString("Name"), reader.GetInt32("TableId"));
                        }
                    }
                    connection.Close();
                }
                catch
                {
                }
            }
            using (var connection = new MySqlConnection(DestinationConnectionString))
            {

                connection.Open();
                var insertQuery = new StringBuilder();
                insertQuery.Append("INSERT IGNORE INTO `" + FinalSystemDatabase + "`.`table` (table_id, system_id, name, comment, mandt_split, gjahr_split, bukrs_split, is_view, row_count) VALUES ");
                foreach (var item in tablesInfo)
                {
                    try
                    {
                        insertQuery.Append("('" + tableIds[item.tableName] + "'," +
                        "'" + 1 + "'," +
                        "'" + item.tableName + "', " +
                        "''," +
                        "'" + 0 + "'," +
                        "'" + 0 + "'," +
                        "'" + 0 + "'," +
                        "'" + 0 + "'," +
                        "'" + item.rowCount + "'" +
                        "),");
                    }
                    catch (Exception)
                    {
                        insertQuery.Append("('" + item.tableId + "'," +
                        "'" + 1 + "'," +
                        "'" + item.tableName + "', " +
                        "''," +
                        "'" + 0 + "'," +
                        "'" + 0 + "'," +
                        "'" + 0 + "'," +
                        "'" + 0 + "'," +
                        "'" + item.rowCount + "'" +
                        "),");
                        /*Logger.LogError(DateTime.Now.ToString() + " - " + ex.ErrorCode + ": " + ex.Message);
                        Logger.LogError("     " + ex.Source + " -> " + ex.StackTrace);*/
                    }

                }
                insertQuery.Length--;
                insertQuery.Append(";");

                var insertCommand = new MySqlCommand(insertQuery.ToString(), connection) {CommandTimeout = 100000};

                insertCommand.ExecuteNonQuery();
                connection.Close();

            }
        }

        /**
         * Insert data to the 'col' table in the *_final_system database
         */
        public static void InsertToFinalSystemCol_Generation(ref ConcurrentBag<ViewboxColumnsInfo> columnsInfo)
        {
            using (var connection = new MySqlConnection(DestinationConnectionString))
            {
                try
                {
                    connection.Open();
                    var insertQuery = new StringBuilder();
                    insertQuery.Append("INSERT IGNORE INTO `" + FinalSystemDatabase + "`.`col` (table_id, name, is_selected, type, length, decimal_place, is_empty) VALUES ");
                    foreach (var item in columnsInfo.OrderBy(c => c.tableId).ThenBy(c => c.ordinal))
                    {
                        int realId = item.tableId;
                        insertQuery.Append("('" + realId + "', " +
                            "'" + item.colName + "', " +
                            "'1',");
                        switch (item.dataTypeName)
                        {
                            case "FLOAT":
                            case "DOUBLE":
                            case "DECIMAL":
                                insertQuery.Append("'numeric', ");
                                break;
                            case "TINYINT":
                            case "SMALLINT":
                            case "BIGINT":
                            case "INT":
                                insertQuery.Append("'integer', ");
                                break;
                            case "DATE":
                                insertQuery.Append("'date', ");
                                break;
                            case "TIME":
                                insertQuery.Append("'time', ");
                                break;
                            case "DATETIME":
                                insertQuery.Append("'datetime', ");
                                break;
                            case "CHAR":
                            case "VARCHAR":
                            case "VARCHAR2":
                                insertQuery.Append("'string', ");
                                break;
                            case "TEXT":
                                insertQuery.Append("'string', ");
                                break;
                            default:
                                insertQuery.Append("'string', ");
                                break;
                        }
                        insertQuery.Append("'" + item.maxLength + "', ");
                        if (item.dataTypeName == "DECIMAL" || item.dataTypeName == "FLOAT" || item.dataTypeName == "DOUBLE")
                        {
                            insertQuery.Append("'" + item.decimals + "', ");
                        }
                        else
                        {
                            insertQuery.Append("0, ");
                        }
                        insertQuery.Append("'" + item.isEmpty + "'" + "),");
                    }
                    insertQuery.Length--;
                    insertQuery.Append(";");

                    var insertCommand = new MySqlCommand(insertQuery.ToString(), connection) {CommandTimeout = 1000000};
                    insertCommand.ExecuteNonQuery();
                    connection.Close();
                }
                catch (MySqlException ex)
                {
                    Logger.LogError(DateTime.Now + " - " + ex.ErrorCode + ": " + ex.Message);
                    Logger.LogError("     " + ex.Source + " -> " + ex.StackTrace);
                }
            }
        }
        /**
         *  Insert data to the 'table' table in the *_final_system database
         */
        public static void InsertToFinalSystemTable_Generation(ref List<ViewboxTablesInfo> tablesInfo)
        {
            using (var connection = new MySqlConnection(DestinationConnectionString))
            {
                try
                {
                    connection.Open();
                    var insertQuery = new StringBuilder();
                    insertQuery.Append("INSERT IGNORE INTO `" + FinalSystemDatabase + "`.`table` (table_id, system_id, name, comment, mandt_split, gjahr_split, bukrs_split, is_view, row_count) VALUES ");
                    foreach (var item in tablesInfo)
                    {
                        insertQuery.Append("('" + item.tableId + "'," +
                            "'" + 1 + "'," +
                            "'" + item.tableName + "', " +
                            "''," +
                            "'" + 0 + "'," +
                            "'" + 0 + "'," +
                            "'" + 0 + "'," +
                            "'" + 0 + "'," +
                            "'" + item.rowCount + "'" +
                            "),");
                    }
                    insertQuery.Length--;
                    insertQuery.Append(";");

                    var insertCommand = new MySqlCommand(insertQuery.ToString(), connection) {CommandTimeout = 100000};

                    insertCommand.ExecuteNonQuery();
                    connection.Close();
                }
                catch (MySqlException ex)
                {
                    Logger.LogError(DateTime.Now + " - " + ex.ErrorCode + ": " + ex.Message);
                    Logger.LogError("     " + ex.Source + " -> " + ex.StackTrace);
                }
            }
        }

        public static void InsertToFinalSystemDefValues()
        {
            using (var connection = new MySqlConnection(DestinationConnectionString))
            {
                try
                {
                    string query = "INSERT IGNORE INTO `" + FinalSystemDatabase + "`.`user` (username, forename, surename, password) VALUES " +
                        "('admin', '', '', '21232f297a57a5a743894a0e4a801fc3')," +
                        "('standardnutzer', '', '', '994eac2395df9e381358bf3b40abaacd')," +
                        "('pruefer', '', '', '940033b54ee59aaebc5b3672a5e7c3c7'); ";

                    query += "INSERT IGNORE INTO `" + FinalSystemDatabase + "`.`company` (company_id, name) VALUES " +
                        "('1', 'root'); ";

                    query += "INSERT IGNORE INTO `" + FinalSystemDatabase + "`.`right` (name) VALUES " +
                        "('read'), ('save'); ";

                    query += "INSERT IGNORE INTO `" + FinalSystemDatabase + "`.`role` (name, description, is_admin) VALUES " +
                            "('Administrator', '', 1)," +
                            "('Nutzer', '', 0)," +
                            "('Betriebsprüfer', '', 0); ";

                    query += "INSERT IGNORE INTO `" + FinalSystemDatabase + "`.`system` (company_id, name) VALUES " +
                            "('1', ''); ";

                    query += "INSERT IGNORE INTO `" + FinalSystemDatabase + "`.`user_has_role` (user_id, role_id) VALUES " +
                            "('1', '1'), ('2', '2'), ('3', '3');";

                    var cmd = new MySqlCommand(query, connection) {CommandTimeout = 100000};

                    connection.Open();
                    cmd.ExecuteNonQuery();
                    connection.Close();
                }
                catch (MySqlException ex)
                {
                    Logger.LogError(DateTime.Now + " - " + ex.ErrorCode + ": " + ex.Message);
                    Logger.LogError("     " + ex.Source + " -> " + ex.StackTrace);
                }
            }
        }
    }
}
