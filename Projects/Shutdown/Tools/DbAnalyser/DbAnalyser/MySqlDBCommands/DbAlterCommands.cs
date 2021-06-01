using System;
using System.Collections.Generic;
using System.Linq;
using MySql.Data.MySqlClient;
using System.Collections.Concurrent;
using DbAnalyser.Logging;

namespace DbAnalyser.MySqlDBCommands
{
    class DbAlterCommands : DbConnection
    {
        public static void AddTRowIdToFinalTables(List<ViewboxTablesInfo> tables)
        {
            using (var connection = new MySqlConnection(DestinationConnectionString))
            {
                connection.Open();
                string query = tables.Aggregate(String.Empty, (current, table) => current + ("ALTER IGNORE TABLE `" + FinalDatabase + "`.`" + table.tableName + "` " + "ADD `_row_no_` BIGINT UNIQUE AUTO_INCREMENT PRIMARY KEY; "));

                var alterCmd = new MySqlCommand(query, connection);
                try
                {
                    alterCmd.ExecuteNonQuery();
                }
                catch (MySqlException ex)
                {
                    Logger.LogError(DateTime.Now + " - " + ex.ErrorCode + ": " + ex.Message);
                    Logger.LogError("     " + ex.Source + " -> " + ex.StackTrace);
                }
            }
        }

        public static void ChangeTimeStampToDateTime(ConcurrentBag<ViewboxColumnsInfo> cols)
        {
            using (var connection = new MySqlConnection(DestinationConnectionString))
            {
                connection.Open();
                string query = cols.Where(c => c.dataTypeName == "DATETIME" || c.dataTypeName == "TIMESTAMP").Aggregate(String.Empty, (current, col) => current + ("ALTER IGNORE TABLE `" + FinalDatabase + "`.`" + col.tableName + "` " + "MODIFY COLUMN  `" + col.colName + "` DATETIME; "));
                var alterCmd = new MySqlCommand(query, connection) {CommandTimeout = 100000};
                try
                {
                    alterCmd.ExecuteNonQuery();
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
