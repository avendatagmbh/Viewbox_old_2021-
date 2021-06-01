using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using DbAnalyser.Logging;
using DbAnalyser.Processing;
using DbAnalyser.Processing.NonSAP.Helpers;
using DbAnalyser.Processing.NonSAP.Methods;
using MySql.Data.MySqlClient;

namespace DbAnalyser.MySqlDBCommands
{
    public class DbSelectCommnands : DbConnection
    {
        public static List<string> columnnameresult = new List<string>();
        public static List<string> columtyperesult = new List<string>();

        public static List<string> GetDatabaseList(string connStr)
        {
            var dbList = new List<string>();
            using (var connection = new MySqlConnection(connStr))
            {
                try
                {
                    connection.Open();
                    const string query = "show databases;";
                    var myCommand = new MySqlCommand(query, connection) {CommandTimeout = 100000};

                    using (var myReader = myCommand.ExecuteReader())
                    {
                        while (myReader.Read())
                        {
                            dbList.Add(myReader.GetString("Database"));
                        }
                    }
                }
                catch (MySqlException ex)
                {
                    Logger.LogError(DateTime.Now + " - " + ex.ErrorCode + ": " + ex.Message);
                    Logger.LogError("     " + ex.Source + " -> " + ex.StackTrace);
                }
            }
            return dbList;
        }

        public static bool CheckIfFinalExists()
        {
            using (var connection = new MySqlConnection(SourceConnectionString))
            {
                connection.Open();

                var query = new StringBuilder();
                query.Append("SELECT COUNT(TABLE_NAME) FROM information_schema.tables WHERE table_type = 'BASE TABLE' and table_schema='" + SourceDatabase + "'");

                if (FromRowCount != -1)
                {
                    query.Append(String.Format(" AND table_rows >= {0}", FromRowCount));
                }
                if (FromRowCount != -1)
                {
                    query.Append(String.Format(" AND table_rows =< {0}", ToRowCount));
                }

                query.Append(";");
                var checkerCmd = new MySqlCommand(query.ToString(), connection) {CommandTimeout = 100000};
                long sourceTableNbr = long.Parse(checkerCmd.ExecuteScalar().ToString());

                query.Clear();
                query.Append("SELECT COUNT(TABLE_NAME) FROM information_schema.tables WHERE table_type = 'BASE TABLE' and table_schema='" + FinalDatabase + "';");
                if (FromRowCount != -1)
                {
                    query.Append(String.Format(" AND table_rows >= {0}", FromRowCount));
                }
                if (FromRowCount != -1)
                {
                    query.Append(String.Format(" AND table_rows <= {0}", ToRowCount));
                }

                checkerCmd = new MySqlCommand(query.ToString(), connection) {CommandTimeout = 100000};
                long finalTableNbr = long.Parse(checkerCmd.ExecuteScalar().ToString());

                if (finalTableNbr == sourceTableNbr)
                {
                    return true;
                }
            }

            return false;
        }

        public static long ReadingRowCount(string tableName)
        {
            const long rowCount = 0;
            using (var connection = new MySqlConnection(SourceConnectionString))
            {
                try
                {
                    connection.Open();
                    string mySelectQuery = "SELECT `table_rows` FROM `information_schema`.`tables` WHERE table_schema = '" + SourceDatabase + "' AND table_name = '" + tableName + "';";
                    var myCommand = new MySqlCommand(mySelectQuery, connection) {CommandTimeout = 100000};

                    MySqlDataReader reader = myCommand.ExecuteReader();
                    while (reader.Read())
                    {
                        return long.Parse(reader.GetString(0));
                    }
                }
                catch (MySqlException ex)
                {
                    Logger.LogError(DateTime.Now + " - " + ex.ErrorCode + ": " + ex.Message);
                    Logger.LogError("     " + ex.Source + " -> " + ex.StackTrace);
                }
            }
            return rowCount;
        }
        public static QueryResult ReadingTables()
        {
            var result = new QueryResult {technicalresult = SqlQueryResult.Exception};

            using (var connection = new MySqlConnection(SourceConnectionString))
            {
                try
                {
                    connection.Open();
                    var c = new MySqlCommand("set global net_write_timeout=99999; set global net_read_timeout=99999", connection);
                    c.ExecuteNonQuery();
                    var mySelectQuery = new StringBuilder("SELECT TABLE_NAME,TABLE_ROWS FROM information_schema.tables WHERE table_type = 'BASE TABLE' and table_schema='" + SourceDatabase + "' ");

                    if (FromRowCount != -1)
                    {
                        mySelectQuery.Append(String.Format(" AND table_rows >= {0}", FromRowCount));
                    }
                    if (FromRowCount != -1)
                    {
                        mySelectQuery.Append(String.Format(" AND table_rows <= {0}", ToRowCount));
                    }
                    mySelectQuery.Append(";");

                    var myCommand = new MySqlCommand(mySelectQuery.ToString(), connection) {CommandTimeout = 100000};

                    using (var myReader = myCommand.ExecuteReader())
                    {
                        while (myReader.Read())
                        {
                            string tablename = myReader.GetString(0);
                            int rowcount = Convert.ToInt32(myReader.GetString(1));
                            result.tableresult.Add(tablename);
                            result.rownumber.Add(tablename, rowcount);
                            using (var connection2 = new MySqlConnection(SourceConnectionString))
                            {
                                connection2.Open();
                                string qry = "SELECT count(1) FROM `information_schema`.`columns` WHERE `table_name` = '" + tablename + "' AND table_schema = '" + SourceDatabase + "';";
                                var cmd = new MySqlCommand(qry, connection2);
                                result.columnnumber.Add(Int32.Parse(cmd.ExecuteScalar().ToString()));
                            }
                        }
                    }
                    result.technicalresult = SqlQueryResult.Successful;
                }
                catch (MySqlException ex)
                {
                    Logger.LogError(DateTime.Now + " - " + ex.ErrorCode + ": " + ex.Message);
                    Logger.LogError("     " + ex.Source + " -> " + ex.StackTrace);
                }
            }
            return result;
        }

        public static QueryResult ReadingColumnsName(string tablename)
        {
            var result = new QueryResult {technicalresult = SqlQueryResult.Exception};

            using (var connection = new MySqlConnection(SourceConnectionString))
            {
                try
                {
                    connection.Open();
                    string mySelectQuery = " SELECT COLUMN_NAME,ORDINAL_POSITION FROM information_schema.columns WHERE table_schema='" + SourceDatabase + "' and table_name = '" + tablename + "';";
                    var myCommand = new MySqlCommand(mySelectQuery, connection) {CommandTimeout = 100000};

                    using (var myReader = myCommand.ExecuteReader())
                    {
                        while (myReader.Read())
                        {
                            string columnname = myReader.GetString(0);
                            string columnnr = myReader.GetString(1);
                            result.tableresult.Add(columnname);
                            result.columnnumber.Add(Convert.ToInt32(columnnr));
                        }
                    }
                    result.technicalresult = SqlQueryResult.Successful;
                }
                catch (MySqlException ex)
                {
                    Logger.LogError(DateTime.Now + " - " + ex.ErrorCode + ": " + ex.Message);
                    Logger.LogError("     " + ex.Source + " -> " + ex.StackTrace);
                }
            }
            return result;
        }

        public static QueryResult ReadingColumnsType(string tablename, string columnname)
        {
            var result = new QueryResult {technicalresult = SqlQueryResult.Exception};
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-GB");
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-GB");

            using (var connection = new MySqlConnection(SourceConnectionString))
            {
                try
                {
                    connection.Open();
                    string mySelectQuery = " SELECT `" + columnname + "` FROM `" + SourceDatabase + "`.`" + tablename + "` LIMIT " + Treshold + ";";
                    var myCommand = new MySqlCommand(mySelectQuery, connection) {CommandTimeout = 100000};

                    using (var myReader = myCommand.ExecuteReader())
                    {
                        while (myReader.Read())
                        {
                            if (!myReader.IsDBNull(0))
                            {
                                string columvalue = myReader.GetString(0);
                                result.tableresult.Add(columvalue);
                            }
                        }
                    }
                    result.technicalresult = SqlQueryResult.Successful;
                }
                catch (MySqlException ex)
                {
                    Logger.LogError(DateTime.Now + " - " + ex.ErrorCode + ": " + ex.Message);
                    Logger.LogError("     " + ex.Source + " -> " + ex.StackTrace);
                }
            }
            return result;
        }

        public static List<SAPDbData> ReadingAllTheSAPTableData(ref List<string> tableNames, string tableName = "dd03l")
        {
            var sapTableData = new List<SAPDbData>();

            using (var connection = new MySqlConnection(SourceConnectionString))
            {
                try
                {
                    string query = "SELECT `TABNAME`, " +
                        "`FIELDNAME`, `DATATYPE`, " +
                        "`INTLEN`, `LENG`, `DECIMALS` " +
                        "FROM `" + SourceDatabase + "`.`" + tableName +
                        "` WHERE `AS4LOCAL` = 'A' ";

                    query += String.Format("AND `TABNAME` IN ('{0}')", tableNames.ToArray().Aggregate((current, next) => current + "', '" +  next));

                    connection.Open();
                    using (connection)
                    {
                        var myCommand = new MySqlCommand(query, connection) {CommandTimeout = 100000};
                        using (var myReader = myCommand.ExecuteReader())
                        {
                            while (myReader.Read())
                            {
                                if (!myReader.IsDBNull(0))
                                {
                                    sapTableData.Add(new SAPDbData(
                                         myReader.GetString("TABNAME"),
                                         myReader.GetString("FIELDNAME"),
                                         myReader.GetString("DATATYPE"),
                                         myReader.GetInt32("INTLEN"),
                                         myReader.GetInt32("LENG"),
                                         myReader.GetInt32("DECIMALS")));
                                }
                            }
                        }
                    }
                   
                }
                catch (MySqlException ex)
                {
                    Logger.LogError(DateTime.Now + " - " + ex.ErrorCode + ": " + ex.Message);
                    Logger.LogError("     " + ex.Source + " -> " + ex.StackTrace);
                }
            }
            return sapTableData;
        }

        public static List<ColData> ReadingRows(string tableName, int tableId)
        {
            var cols = new List<List<string>>();
            var completeColumnData = new List<ColData>();
            using (var connection = new MySqlConnection(SourceConnectionString))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT * FROM `" + SourceDatabase + "`.`" + tableName + "` LIMIT " + Treshold + ";";
                    var cmd = new MySqlCommand(query, connection) {CommandTimeout = 100000};
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var values = new Object[reader.FieldCount];
                            reader.GetValues(values);
                            if (cols.Count == 0)
                            {
                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    cols.Add(new List<string>());
                                }
                            }
                            for (int i = 0; i < values.Length; i++)
                            {
                                cols[i].Add(values[i] != DBNull.Value ? values[i].ToString() : String.Empty);
                            }
                        }
                    }
                    query = "SHOW COLUMNS FROM `" + SourceDatabase + "`.`" + tableName + "`;";
                    cmd = new MySqlCommand(query, connection);
                    using (var reader = cmd.ExecuteReader())
                    {
                        int colId = 1; // TODO
                        while (reader.Read())
                        {
                            ColData colData = cols.Count == 0 ? new ColData(colId, reader.GetString("FIELD"), new List<string>(), tableName, tableId) : new ColData(colId, reader.GetString("FIELD"), cols[colId - 1], tableName, tableId);

                            completeColumnData.Add(colData);
                            colId++;
                        }
                    }
                }
                catch (MySqlException ex)
                {
                    Logger.LogError(DateTime.Now + " - " + ex.ErrorCode + ": " + ex.Message);
                    Logger.LogError("     " + ex.Source + " -> " + ex.StackTrace);
                }
            }
            return completeColumnData;
        }

        public static List<string> ReadingActualSAPColumnNames(string tableName)
        {
            var sapColumNames = new List<string>();

            using (var connection = new MySqlConnection(SourceConnectionString))
            {
                try
                {
                    string query = "EXPLAIN `" + SourceDatabase + "`.`" + tableName + "`;";
                    var myCommand = new MySqlCommand(query, connection);
                    connection.Open();
                    myCommand.CommandTimeout = 100000;
                    using (var myReader = myCommand.ExecuteReader())
                    {
                        while (myReader.Read())
                        {
                            if (!myReader.IsDBNull(0))
                            {
                                sapColumNames.Add(myReader.GetString("Field"));
                            }
                        }
                    }
                }
                catch (MySqlException ex)
                {
                    Logger.LogError(DateTime.Now + " - " + ex.ErrorCode + ": " + ex.Message);
                    Logger.LogError("     " + ex.Source + " -> " + ex.StackTrace);
                }
            }
            return sapColumNames;
        }

        public static int ReadRequiredColumnLength(string tableName, string colName)
        {
            using (var connection = new MySqlConnection(SourceConnectionString))
            {
                try
                {
                    string query = "SELECT max(length(`" + colName + "`)) LEN FROM `" + SourceDatabase + "`.`" + tableName + "` ORDER BY LEN DESC LIMIT 1;";
                    var myCommand = new MySqlCommand(query, connection);
                    connection.Open();
                    myCommand.CommandTimeout = 100000;
                    using (var myReader = myCommand.ExecuteReader())
                    {
                        while (myReader.Read())
                        {
                            if (!myReader.IsDBNull(0))
                            {
                                return myReader.GetInt32(0);
                            }
                        }
                    }
                }
                catch (MySqlException ex)
                {
                    Logger.LogError(DateTime.Now + " - " + ex.ErrorCode + ": " + ex.Message);
                    Logger.LogError("     " + ex.Source + " -> " + ex.StackTrace);
                }
            }
            return 0;
        }

        public static int GetDistinctRowCount(string tableName, string colName)
        {
            using (var connection = new MySqlConnection(SourceConnectionString))
            {
                try
                {
                    string query = "SELECT DISTINCT COUNT(`" + colName + "`) AS CNT FROM `" + SourceDatabase + "`.`" + tableName + "`;";
                    var myCommand = new MySqlCommand(query, connection);
                    connection.Open();
                    myCommand.CommandTimeout = 100000;
                    using (var myReader = myCommand.ExecuteReader())
                    {
                        while (myReader.Read())
                        {
                            if (!myReader.IsDBNull(0))
                            {
                                return myReader.GetInt32("CNT");
                            }
                        }
                    }
                }
                catch (MySqlException ex)
                {
                    Logger.LogError(DateTime.Now + " - " + ex.ErrorCode + ": " + ex.Message);
                    Logger.LogError("     " + ex.Source + " -> " + ex.StackTrace);
                }
            }
            return 0;
        }

        public static bool DoesAnalyseDbExists()
        {
            using (var connection = new MySqlConnection(SourceConnectionString))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT IF(EXISTS (SELECT SCHEMA_NAME FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME = '" + AnalysticDatabase + "'), 'Yes','No')";
                    var testCmd = new MySqlCommand(query, connection) {CommandTimeout = 100000};

                    using (var reader = testCmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            if (reader.GetString(0) == "Yes")
                            {
                                return true;
                            }
                        }
                    }
                }
                catch (MySqlException ex)
                {
                    Logger.LogError(DateTime.Now + " - " + ex.ErrorCode + ": " + ex.Message);
                    Logger.LogError("     " + ex.Source + " -> " + ex.StackTrace);
                }
            }
            return false;
        }

        public static bool DoesFinalDbExists()
        {
            using (var connection = new MySqlConnection(SourceConnectionString))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT IF(EXISTS (SELECT SCHEMA_NAME FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME = '" + FinalDatabase + "'), 'Yes','No')";
                    var testCmd = new MySqlCommand(query, connection) {CommandTimeout = 100000};

                    using (var reader = testCmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            if (reader.GetString(0) == "Yes")
                            {
                                return true;
                            }
                        }
                    }
                }
                catch (MySqlException ex)
                {
                    Logger.LogError(DateTime.Now + " - " + ex.ErrorCode + ": " + ex.Message);
                    Logger.LogError("     " + ex.Source + " -> " + ex.StackTrace);
                }
            }
            return false;
        }

        public static string IsColumnIsInt(string tableName, string colName, Regex regex, long realRowCount)
        {
            using (var connection = new MySqlConnection(SourceConnectionString))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT COUNT(`" + colName + "`) FROM `" + SourceDatabase + "`.`" + tableName + "` WHERE `" + colName + "` REGEXP '" + regex + "';";
                    var typeTester = new MySqlCommand(query, connection) {CommandTimeout = 100000};
                    var rowCount = (long)typeTester.ExecuteScalar();
                    if (rowCount == realRowCount)
                    {
                        return "INT";
                    }
                    if (rowCount > 0)
                    {
                        return "TEXT";
                    }
                }
                catch (MySqlException ex)
                {
                    Logger.LogError(DateTime.Now + " - " + ex.ErrorCode + ": " + ex.Message);
                    Logger.LogError("     " + ex.Source + " -> " + ex.StackTrace);
                }
            }
            return "unknown";
        }

        public static string IsColumnIsDecimal(string tableName, string colName, List<Regex> regexes, long realRowCount)
        {
            using (var connection = new MySqlConnection(SourceConnectionString))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT COUNT(`" + colName + "`) FROM `" + SourceDatabase + "`.`" + tableName + "` WHERE ";
                    for (int i = 0; i < regexes.Count; i++)
                    {
                        if (i == 0)
                        {
                            query += String.Format("`{0}` REGEXP '{1}' ", colName, regexes[i]);
                        }
                        else
                        {
                            query += String.Format(" OR `{0}` REGEXP '{1}' ;", colName, regexes[i]);
                        }
                    }
                    var typeTester = new MySqlCommand(query, connection) {CommandTimeout = 100000};
                    var decimalCount = (long)typeTester.ExecuteScalar();
                    long notDecimals = realRowCount - decimalCount;
                    if (decimalCount == 0)
                    {
                        return "unknown";
                    }
                    if (notDecimals == 0 && decimalCount > 0)
                    {
                        return "DECIMAL";
                    }
                    query = "SELECT COUNT(`" + colName + "`) FROM `" + SourceDatabase + "`.`" + tableName + "` WHERE `" + colName + "` REGEXP '" + IntAnalyser.IntFilter.getRegex() + "';";
                    typeTester = new MySqlCommand(query, connection) {CommandTimeout = 100000};
                    var intRowCount = (long)typeTester.ExecuteScalar();
                    long notInt = notDecimals - intRowCount;
                    if (notInt == 0 && decimalCount > 0)
                    {
                        return "DECIMAL";
                    }
                    query = "SELECT COUNT(`" + colName + "`) FROM `" + SourceDatabase + "`.`" + tableName + "` WHERE `" + colName + "` = '';";
                    typeTester = new MySqlCommand(query, connection) {CommandTimeout = 100000};
                    var emptyRowCount = (long)typeTester.ExecuteScalar();
                    if (notInt == emptyRowCount && emptyRowCount != realRowCount && decimalCount > 0)
                    {
                        return "DECIMAL";
                    }
                }
                catch (MySqlException ex)
                {
                    Logger.LogError(DateTime.Now + " - " + ex.ErrorCode + ": " + ex.Message);
                    Logger.LogError("     " + ex.Source + " -> " + ex.StackTrace);
                }
            }
            return "unknown";
        }

        public static string IsColumnIsString(string tableName, string colName, Regex regex)
        {
            using (var connection = new MySqlConnection(SourceConnectionString))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT COUNT(`" + colName + "`) FROM `" + SourceDatabase + "`.`" + tableName + "` WHERE `" + colName + "` REGEXP '" + regex + "';";
                    var typeTester = new MySqlCommand(query, connection) {CommandTimeout = 100000};
                    var rowCount = (long)typeTester.ExecuteScalar();
                    if (rowCount > 0)
                    {
                        return "TEXT";
                    }
                }
                catch (MySqlException ex)
                {
                    Logger.LogError(DateTime.Now + " - " + ex.ErrorCode + ": " + ex.Message);
                    Logger.LogError("     " + ex.Source + " -> " + ex.StackTrace);
                }
            }
            return "unknown";
        }

        public static List<ViewboxTablesInfo> ReadTableInfo()
        {
            var ret = new List<ViewboxTablesInfo>();
            using (var connection = new MySqlConnection(DestinationConnectionString))
            {
                connection.Open();
                string query = "SHOW TABLE STATUS FROM `" + FinalDatabase + "` WHERE ROWS IS NOT NULL;";
                var cmd = new MySqlCommand(query, connection) {CommandTimeout = 100000};
                using (var reader = cmd.ExecuteReader())
                {
                    int i = 1;
                    while (reader.Read())
                    {
                        var vTableInfoItem = new ViewboxTablesInfo
                        {
                            category = 0,
                            databaseName = SourceDatabase,
                            tableName = reader.GetString("Name"),
                            type = 1,
                            rowCount = Int32.Parse(reader.GetString("Rows")),
                            visible = 1,
                            archived = 0,
                            objectType = 0,
                            defaultSheme = 0,
                            transactionNumber = 10000 + i,
                            tableId = i,
                            userDefined = 0,
                            ordinal = i - 1
                        };
                        ret.Add(vTableInfoItem);

                        i++;
                    }
                }
            }

            return ret;
        }

        public static ConcurrentBag<ViewboxColumnsInfo> ReadColInfo(ViewboxTablesInfo tableInfo)
        {
            var ret = new ConcurrentBag<ViewboxColumnsInfo>();

            using (var connection = new MySqlConnection(DestinationConnectionString))
            {
                connection.Open();
                MySqlDataReader reader = null;
                try
                {
                    string query = "SELECT * FROM `information_schema`.`columns` WHERE `table_schema` = '" + FinalDatabase + "' AND `table_name` = '" + tableInfo.tableName + "';";
                    var cmd = new MySqlCommand(query, connection) {CommandTimeout = 100000};
                    reader = cmd.ExecuteReader();
                }
                catch (MySqlException)
                {
                    Thread.Sleep(1000);
                    ReadColInfo(tableInfo);
                }

                using (reader)
                {
                    int i = 0;
                    while (reader != null && reader.Read())
                    {
                        string type = reader.GetString("DATA_TYPE").ToUpper();
                        string typeWithLen = reader.GetString("COLUMN_TYPE");
                        int length = 0;
                        int decimals = 0;

                        if (type != "BLOB" && type != "TEXT" &&
                            type != "BOOL" && type != "DATE" &&
                            type != "DATETIME" && type != "TIME" &&
                            type != "DECIMAL" && type != "DOUBLE" &&
                            type != "LONGTEXT" && type != "SMALLTEXT" && type != "MEDIUMTEXT")
                        {
                            string[] parts = typeWithLen.Split('(');
                            length = Int32.Parse(parts[1].Replace(")", "").Replace(" unsigned", ""));
                        }

                        if (type == "DECIMAL" || type == "DOUBLE")
                        {

                            length = Int32.Parse(reader.GetString("NUMERIC_PRECISION"));
                            try
                            {
                                if (reader.IsDBNull(11))
                                {
                                    Int32.TryParse(reader.GetString("NUMERIC_SCALE"), out decimals);
                                }
                                else
                                {
                                    decimals = 2;
                                }
                            }
                            catch (Exception)
                            {
                                decimals = 2;
                            }
                        }

                        var vColInfoItem = new ViewboxColumnsInfo
                        {
                            isVisible = 1,
                            tableId = tableInfo.tableId,
                            tableName = tableInfo.tableName,
                            dataTypeName = type.ToUpper(),
                            originalName = "",
                            optimaliztaionType = 0,
                            isEmpty = 0,
                            maxLength = length,
                            decimals = decimals,
                            paramOperators = 0,
                            flag = 0,
                            colName = reader.GetString("COLUMN_NAME"),
                            userDefined = 0,
                            ordinal = i
                        };

                        switch (type.ToUpper())
                        {
                            case "DATE":
                                vColInfoItem.dataType = ProcessDataTypes.Date;
                                break;
                            case "TIMESTAMP":
                            case "DATETIME":
                                vColInfoItem.dataType = ProcessDataTypes.DateTime;
                                break;
                            case "TIME":
                                vColInfoItem.dataType = ProcessDataTypes.Time;
                                break;
                            case "VARCHAR":
                            case "TEXT":
                            case "LONGTEXT":
                            case "MEDIUMTEXT":
                            case "SMALLTEXT":
                                vColInfoItem.dataType = ProcessDataTypes.String;
                                break;
                            case "BIGINT":
                            case "SMALLINT":
                            case "INT":
                                vColInfoItem.dataType = ProcessDataTypes.Integer;
                                break;
                            case "DECIMAL":
                                vColInfoItem.dataType = ProcessDataTypes.Decimal;
                                break;
                            case "BOOL":
                                vColInfoItem.dataType = ProcessDataTypes.Bool;
                                break;
                            case "BLOB":
                                vColInfoItem.dataType = ProcessDataTypes.Blob;
                                break;
                        }

                        if (reader.GetString("COLUMN_NAME") != "_row_no_")
                        {
                            i++;
                            ret.Add(vColInfoItem);
                        }
                    }
                }

            }

            return ret;
        }

        public static string ReadLongestColumnLength(string tableName, string colName)
        {
            using (var connection = new MySqlConnection(SourceConnectionString))
            {
                try
                {
                    string query = "SELECT `" + colName + "`, length(`" + colName + "`) LEN FROM `" + SourceDatabase + "`.`" + tableName + "` ORDER BY LEN DESC;";
                    var myCommand = new MySqlCommand(query, connection);
                    connection.Open();
                    myCommand.CommandTimeout = 100000;
                    using (var myReader = myCommand.ExecuteReader())
                    {
                        while (myReader.Read())
                        {
                            if (!myReader.IsDBNull(0))
                            {
                                return myReader.GetString(0);
                            }
                        }
                    }
                }
                catch (MySqlException ex)
                {
                    Logger.LogError(DateTime.Now + " - " + ex.ErrorCode + ": " + ex.Message);
                    Logger.LogError("     " + ex.Source + " -> " + ex.StackTrace);
                }
            }
            return String.Empty;
        }

        public static long ReadingCurrentRowCount(string tableName)
        {
            while (IsPaused)
            {
                Thread.Sleep(10);
            }
            long rowCount;
            using (var connection = new MySqlConnection(DestinationConnectionString))
            {
                connection.Open();
                var cmd = new MySqlCommand("SELECT COUNT(*) FROM `" + FinalDatabase + "`.`" + tableName + "`;", connection)
                {
                    CommandTimeout = 1000000
                };
                rowCount = long.Parse(cmd.ExecuteScalar().ToString());
            }
            return rowCount;
        }
    }
}
