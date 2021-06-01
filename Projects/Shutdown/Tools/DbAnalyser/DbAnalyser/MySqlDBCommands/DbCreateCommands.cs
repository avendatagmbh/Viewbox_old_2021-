using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DbAnalyser.Logging;
using MySql.Data.MySqlClient;

namespace DbAnalyser.MySqlDBCommands
{
    /**
     * This class contains every SQL CREATE function required for the process
     */ 
    public class DbCreateCommands : DbConnection
    {
        /**
         * Creates the *_analyse database 
         */
        public static void CreateAnalyticsDatabase()
        {
            using (var connection = new MySqlConnection(DestinationConnectionString))
            {
                try
                {
                    string query = String.Format("CREATE DATABASE IF NOT EXISTS `{0}` CHARACTER SET utf8 COLLATE utf8_general_ci;", AnalysticDatabase);
                    var myCommand = new MySqlCommand(query, connection) {CommandTimeout = 100000};

                    connection.Open();
                    myCommand.ExecuteNonQuery();
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
         * Creates analyse_data_base_info table in the *_analyse database
         */ 
        public static void CreateDatabaseInfo()
        {
            using (var connection = new MySqlConnection(DestinationConnectionString))
            {
                try
                {
                    string query = String.Format("CREATE TABLE IF NOT EXISTS `{0}`.`analyse_database_info`(" +
                        "`TableId` INT AUTO_INCREMENT PRIMARY KEY UNIQUE, " +
                        "`TablesAll` INT, " +
                        "`TablesEmpty` INT, " +
                        "`TablesAnalysed` INT, " +
                        "`ColumnsAll` INT, " +
                        "`ColumsNull` INT);", AnalysticDatabase);
                    var createCommand = new MySqlCommand(query, connection) {CommandTimeout = 100000};

                    connection.Open();
                    createCommand.ExecuteNonQuery();
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
         * Creates analyse_table_info table in the *_analyse database
         */
        public static void CreateTableInfo()
        {
            using (var connection = new MySqlConnection(DestinationConnectionString))
            {
                try
                {
                    string query = String.Format("CREATE TABLE IF NOT EXISTS `{0}`.`analyse_table_info`( " +
                         "`TableId` INT AUTO_INCREMENT, " +
                         "`Name` VARCHAR(100) NOT NULL, " +
                         "`TimeStamp` DATETIME, " +
                         "`Duration` BIGINT, " +
                         "`Comment` VARCHAR(100), " +
                         "`Description` VARCHAR(100), " +
                         "`AnalysationState` INT, " +
                         "`Type` TINYINT," +
                         "`Count` BIGINT NOT NULL, " +
                         "PRIMARY KEY(`TableId`), " +
                         "UNIQUE KEY my_table_unique(`Name`, `Count`)" +
                         ");", AnalysticDatabase);
                    var createCommand = new MySqlCommand(query, connection) {CommandTimeout = 100000};

                    connection.Open();
                    createCommand.ExecuteNonQuery();
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
         * Creates analyse_column_info table in the *_analyse database
         */
        public static void CreateColumnInfo()
        {
            using (var connection = new MySqlConnection(DestinationConnectionString))
            {
                try
                {
                    string query = String.Format("CREATE TABLE IF NOT EXISTS `{0}`.`analyse_column_info`(" +
                        "`ColId` INT UNIQUE AUTO_INCREMENT, " +
                        "`TableId` INT NOT NULL, " +
                        "`Name` VARCHAR(64) NOT NULL, " +
                        "`Description` VARCHAR(255), " +
                        "`Length` INT, " +
                        "`Type` INT, " +
                        "PRIMARY KEY(`ColId`), " +
                        "UNIQUE KEY `my_unique_col_key` (`TableId`, `Name`)" +
                        ");", AnalysticDatabase);
                    var createCommand = new MySqlCommand(query, connection) {CommandTimeout = 100000};

                    connection.Open();
                    createCommand.ExecuteNonQuery();
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
        * Creates used_pattern_cache table in the *_analyse database
        */
        public static void CreatePatternCache()
        {
            using (var connection = new MySqlConnection(DestinationConnectionString))
            {
                try
                {
                    string query = String.Format("CREATE TABLE IF NOT EXISTS `{0}`.`pattern_cache`(" +
                        "`RelationId` INT PRIMARY KEY AUTO_INCREMENT, " +
                        "`Pattern` TEXT, " +
                        "`Datatype` INT);", AnalysticDatabase);
                    var createCommand = new MySqlCommand(query, connection) {CommandTimeout = 100000};

                    connection.Open();
                    createCommand.ExecuteNonQuery();
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
         * This method creates the *_final database 
         */
        public static void CreateFinalDatabase()
        {
            using (var connection = new MySqlConnection(DestinationConnectionString))
            {
                try
                {
                    string query = String.Format("CREATE DATABASE IF NOT EXISTS `{0}` CHARACTER SET utf8 COLLATE utf8_general_ci;", FinalDatabase);
                    var myCommand = new MySqlCommand(query, connection) {CommandTimeout = 100000};

                    connection.Open();
                    myCommand.ExecuteNonQuery();
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
         * This method creates every table in the *_final database,
         * using the informtaion required earlier in the process
         */
        public static void CreateFinalTables(ref List<AnalyseTableInfo> tableInfos, ref ConcurrentBag<AnalysedColumnInfo> colInfos)
        {
            if (tableInfos.Count > 0)
            {
                foreach (var tableInfo in tableInfos)
                {
                    using (var connection = new MySqlConnection(DestinationConnectionString))
                    {
                        try
                        {
                            connection.Open();
                            var createQuery = new StringBuilder();
                            createQuery.Append("CREATE TABLE IF NOT EXISTS `" + FinalDatabase + "`.`" + tableInfo.name + "` (`_row_no_` BIGINT UNIQUE AUTO_INCREMENT PRIMARY KEY, ");
                            foreach (var colInfo in colInfos.Where(c => c.tableId == tableInfo.tableId && c.name != "_row_no_").OrderBy(c => c.colId))
                            {
                                if (colInfo.type != "BLOB" && colInfo.type != "TEXT" && colInfo.type != "BOOL" && colInfo.type != "DATE" && colInfo.type != "DATETIME" && colInfo.type != "TIME" && colInfo.type != "DECIMAL")
                                {
                                    createQuery.Append(String.Format("`{0}` {1}({2}),", colInfo.name, colInfo.type, colInfo.length));
                                }
                                else if (colInfo.type == "DECIMAL")
                                {
                                    createQuery.Append(String.Format("`{0}` {1}({2}, {3}),",colInfo.name, colInfo.type, (colInfo.length + colInfo.decimals), colInfo.decimals));
                                }
                                else
	                            {
                                    createQuery.Append(String.Format("`{0}` {1},", colInfo.name, colInfo.type));
	                            }
                            }
                            createQuery.Length--;
                            createQuery.Append(") ENGINE MyISAM;");

                            var myCommand = new MySqlCommand(createQuery.ToString(), connection)
                            {
                                CommandTimeout = 100000
                            };
                            myCommand.ExecuteNonQuery();
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
            else
            {
                // TODO - Read data from *_analyse tables if the process was interrupted somehow

            }
        }

       

        /**
         * Create *_final_system database
         */ 
        public static void CreateFinalSystemDatabase()
        {
            using (var connection = new MySqlConnection(DestinationConnectionString))
            {
                try
                {
                    string query = String.Format("CREATE DATABASE IF NOT EXISTS `{0}` CHARACTER SET utf8 COLLATE utf8_general_ci; ", FinalSystemDatabase);
                    var myCommand = new MySqlCommand(query, connection) {CommandTimeout = 100000};

                    connection.Open();
                    myCommand.ExecuteNonQuery();
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
         * Create 'col' and 'table' tables in the *_final_system database
         */ 
        public static void CreateFinalSystemTables()
        {
            using (var connection = new MySqlConnection(DestinationConnectionString))
            {
                try
                {
                    connection.Open();

                    //query for 'col' table
                    var query = new StringBuilder();
                    query.Append(String.Format("CREATE TABLE IF NOT EXISTS `{0}`.`table`(" +
                        "`table_id` INT UNIQUE," +
                        "`system_id` INT," +
                        "`name` VARCHAR(128) NOT NULL," +
                        "`comment` VARCHAR(1024)," +
                        "`filter` VARCHAR(16384)," +
                        "`row_count` INT," +
                        "`mandt_split` TINYINT," +
                        "`mandt_col` VARCHAR(128)," +
                        "`gjahr_split` TINYINT," +
                        "`gjahr_col` VARCHAR(128)," +
                        "`bukrs_split` TINYINT," +
                        "`bukrs_col` VARCHAR(128)," +
                        "`is_view` TINYINT," +
                        "`parent_table_id` INT," +
                        "PRIMARY KEY(`table_id`), " +
                        "UNIQUE KEY `unique_table_id` (`name`, `row_count`)" +
                        "); ", FinalSystemDatabase));

                    query.Append(String.Format("CREATE TABLE IF NOT EXISTS `{0}`.`col`(" +
                        "`col_id` INT UNIQUE AUTO_INCREMENT," +
                        "`table_id` INT NOT NULL," +
                        "`name` VARCHAR(64) NOT NULL," +
                        "`comment` VARCHAR(1024)," +
                        "`is_selected` TINYINT," +
                        "`type` VARCHAR(128)," +
                        "`length` INT," +
                        "`decimal_place` TINYINT," +
                        "`filter` VARCHAR(4096)," +
                        "`is_empty` TINYINT," +
                        "`const_value` VARCHAR(128), " + 
                        "PRIMARY KEY(`col_id`), " +
                        "FOREIGN KEY (`table_id`) REFERENCES `table`(`table_id`), " +
                        "UNIQUE KEY `unique_cols` (`name`, `table_id`) " +
                        "); ", FinalSystemDatabase));

                    //query for 'user' table 
                    query.Append(String.Format("CREATE TABLE IF NOT EXISTS `{0}`.`user`(" +
                        "`user_id` INT UNIQUE AUTO_INCREMENT PRIMARY KEY," +
                        "`username` VARCHAR(32) UNIQUE," +
                        "`forename` VARCHAR(256)," +
                        "`surename` VARCHAR(256)," +
                        "`password` VARCHAR(32)" +
                        "); ", FinalSystemDatabase));

                    //query for 'company' table 
                    query.Append(String.Format("CREATE TABLE IF NOT EXISTS `{0}`.`company`(" +
                        "`company_id` INT UNIQUE AUTO_INCREMENT PRIMARY KEY," +
                        "`name` VARCHAR(256)," +
                        "`description` VARCHAR(4096)," +
                        "`bezeichnung` VARCHAR(1024)," +
                        "`ort` VARCHAR(1024)); ", FinalSystemDatabase));                    

                    //query for 'company_has_role_has_right' table 
                    query.Append(String.Format("CREATE TABLE IF NOT EXISTS `{0}`.`company_has_role_has_right`(" +
                        "`company_id` INT UNIQUE," + // key
                        "`role_id` INT UNIQUE," + // key
                        "`right_id` INT UNIQUE,"+ // key
                        "PRIMARY KEY (`company_id`, `role_id`, `right_id`)" +
                        "); ", FinalSystemDatabase));

                    //query for 'mandt' table 
                    query.Append(String.Format("CREATE TABLE IF NOT EXISTS `{0}`.`mandt`(" +
                        "`id` VARCHAR(128) PRIMARY KEY," +
                        "`name` VARCHAR(128)," +
                        "`ort` VARCHAR(128)); ", FinalSystemDatabase));

                    //query for 'order_area' table 
                    query.Append(String.Format("CREATE TABLE IF NOT EXISTS `{0}`.`order_area`(" +
                        "`table_name` VARCHAR(127)," + // key
                        "`part_no` INT UNIQUE," + // key
                        "`name` VARCHAR(127)," + // key
                        "`start` INT UNIQUE," +
                        "`count` INT UNIQUE," +
                        "PRIMARY KEY (`table_name`, `part_no`, `name`)); ", FinalSystemDatabase));

                    //query for 'right' table 
                    query.Append(String.Format("CREATE TABLE IF NOT EXISTS `{0}`.`right`(" +
                        "`right_id` INT UNIQUE AUTO_INCREMENT PRIMARY KEY," +
                        "`name` VARCHAR(255) UNIQUE); ", FinalSystemDatabase));

                    //query for 'role' table 
                    query.Append(String.Format("CREATE TABLE IF NOT EXISTS `{0}`.`role`(" +
                        "`role_id` INT UNIQUE AUTO_INCREMENT PRIMARY KEY," +
                        "`name` VARCHAR(64),"+
                        "`description` VARCHAR(4096)," +
                        "`is_admin` TINYINT" +
                        "); ", FinalSystemDatabase));

                    //query for 'role_has_right' table 
                    query.Append(String.Format("CREATE TABLE IF NOT EXISTS `{0}`.`role_has_right`(" +
                        "`role_id` INT UNIQUE," + // key
                        "`right_id` INT UNIQUE," + // key
                        "PRIMARY KEY (`role_id`, `right_id`)" +
                        "); ", FinalSystemDatabase));

                    //query for 'system' table 
                    query.Append(String.Format("CREATE TABLE IF NOT EXISTS `{0}`.`system`(" +
                        "`system_id` INT UNIQUE AUTO_INCREMENT PRIMARY KEY," +
                        "`company_id` INT UNIQUE," +
                        "`name` VARCHAR(256)," +
                        "`description` VARCHAR(4096)" +
                        "); ",FinalSystemDatabase));

                    //query for 'system_has_role_has_right' table 
                    query.Append(String.Format("CREATE TABLE IF NOT EXISTS `{0}`.`system_has_role_has_right`(" +
                        "`system_id` INT UNIQUE," + // key
                        "`role_id` INT UNIQUE," + // key
                        "`right_id` INT UNIQUE," + // key
                        "PRIMARY KEY (`system_id`, `role_id`, `right_id`)" +
                        "); ", FinalSystemDatabase));

                    //query for 'system_login' table 
                    query.Append(String.Format("CREATE TABLE IF NOT EXISTS `{0}`.`system_login`(" +
                        "`ID` INT," +
                        "`sytem` INT," +
                        "`Name` VARCHAR(255)," +
                        "`system_id` VARCHAR(255)" +
                        "); ", FinalSystemDatabase));

                    //query for 'table_has_role_has_right' table 
                    query.Append(String.Format("CREATE TABLE IF NOT EXISTS `{0}`.`table_has_role_has_right`(" +
                        "`table_id` INT UNIQUE," + // key
                        "`role_id`  INT UNIQUE," + // key
                        "`right_id`  INT UNIQUE," + // key
                        "PRIMARY KEY (`table_id`, `role_id`, `right_id`)" +
                        "); ", FinalSystemDatabase));

                    //query for 'table_parts' table 
                    query.Append(String.Format("CREATE TABLE IF NOT EXISTS `{0}`.`table_parts`(" +
                        "`table_name` CHAR(128)," + // key
                        "`part_no`  INT UNIQUE," + // key
                        "`row_no_start`  INT UNIQUE," +
                        "`name`  CHAR(128)," +
                        "PRIMARY KEY (`table_name`, `part_no`)" +
                        "); ", FinalSystemDatabase));

                    //query for 'user_has_role' table 
                    query.Append(String.Format("CREATE TABLE IF NOT EXISTS `{0}`.`user_has_role`(" +
                        "`user_id` INT UNIQUE," + // key
                        "`role_id`  INT UNIQUE," + // key
                        "PRIMARY KEY (`user_id`, `role_id`)" +
                        ");", FinalSystemDatabase));

                    var myCommand = new MySqlCommand(query.ToString(), connection) {CommandTimeout = 100000};
                    myCommand.ExecuteNonQuery();
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
