//-----------------------------------------------------------------------
// <copyright file="Program.cs" company="">
//     Copyright (c) csb, . All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Data;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using CommandLineParser.Exceptions;
using MySql.Data.MySqlClient;
//using Oracle.DataAccess.Client;
//using Oracle.DataAccess.Types;
using Devart.Data.Oracle;
using System.IO;
using AV.Log;
using log4net;

namespace oracle2mysql
{
    /// <summary>
    /// Contains the program entry point.
    /// </summary>
    public static class Program
    {
        private static Options _options;
        private static ILog log = LogHelper.GetLogger();
        private const char tab = '\t';

        /// <summary>
        /// Defines the program entry point. 
        /// </summary>
        /// <param name="args">An array of <see cref="T:System.String"/> containing command line parameters.</param>
        private static void Main(string[] args)
        {
            CommandLineParser.CommandLineParser parser = new CommandLineParser.CommandLineParser();
            _options = new Options();
            parser.ExtractArgumentAttributes(_options);
            try {
                parser.ParseCommandLine(args);

                try
                {
                    string logPath = Assembly.GetExecutingAssembly().Location;
                    LogHelper.ConfigureLogger(LogHelper.GetLogger(), logPath);
                }
                catch (Exception ex)
                {
                }

                Alpha();
            }
            catch (CommandLineException) {
                parser.ShowUsage();
            }
        }


        private static void Alpha() {
            
            string[] lines = File.ReadAllLines(_options.FilePath);
            LogInfoMessage(string.Format("Tables are read from file, table count: {0}", lines.Length));

            using (StreamWriter tablesWithError = new StreamWriter(_options.TablesWithErrorPath, true, Encoding.UTF8)) {
                string mySqlConnStr = String.Format("server={0};userid={1};password={2}; Connection Timeout=120", _options.Server, _options.User, _options.Password);
                log.Info("Process started");

                LogInfoMessage("Batch size " + 750);//_options.BatchSize);

                try 
                {
                    using (MySqlConnection mySqlConn = new MySqlConnection(mySqlConnStr)) {
                        mySqlConn.Open();
                        LogInfoMessage("MySql ok");
                        using (MySqlCommand mySqlCmd = new MySqlCommand()) {
                            mySqlCmd.Connection = mySqlConn;
                            mySqlCmd.CommandTimeout = 0;
                            mySqlCmd.CommandType = CommandType.Text;
                            mySqlCmd.CommandText = String.Format("CREATE SCHEMA IF NOT EXISTS `{0}`", _options.Database);
                            mySqlCmd.ExecuteNonQuery();
                        }
                    }
                }
                catch (Exception ex)
                {
                    log.Error(ex.Message, ex);
                    LogInfoMessage("ERROR creating schema");
                    throw;
                }
                
                Encoding ec = Encoding.UTF8;

                string oracleConnStr = String.Format("Data Source={0};User Id={1};Password={2}; Connection Timeout=120", _options.OracleServiceName, _options.Oracleuser, _options.Oraclepassword);
                using(OracleConnection oraConn = new OracleConnection(oracleConnStr)) {
                    oraConn.Open();
                    LogInfoMessage("ORACLE ok");

                    using(OracleCommand oraCmd = new OracleCommand()) {
                        oraCmd.CommandTimeout = 0;
                        oraCmd.Connection = oraConn;
                        oraCmd.CommandType = CommandType.Text;
                            
                        int linesIndex = 0;
                        int linesMax = lines.Count();
                        string commandStr = string.Empty;

                        foreach (string line in lines) {
                            string[] cols = line.Split(tab);
                            //cols[0], cols[1]
                            linesIndex++;
                            LogInfoMessage(string.Format("Processing table: {0}.{1} {2:N0}/{3:N0}", cols[0], cols[1], linesIndex, linesMax));

                            decimal oraCount = -1;
                            try
                            {
                                commandStr = String.Format("SELECT count(*) FROM \"{0}\".\"{1}\" ", cols[0], cols[1]);
                                oraCmd.CommandText = commandStr;
                                using (OracleDataReader drc = oraCmd.ExecuteReader())
                                {
                                    drc.Read();
                                    oraCount = drc.GetDecimal(0);
                                }
                            }
                            catch (Exception ex)
                            {
                                log.Error(ex.Message, ex);
                                LogOracleException(ex);
                                LogInfoMessage("Exception ignored, continue processing...");
                                LogInfoMessage("Error executing commnd: " + commandStr);
                                //throw;
                            }

                            LogInfoMessage(string.Format("Oracle count: {0}", oraCount));

                            if (oraCount != -1) {
                                bool tableExists = false;
                                try {
                                    commandStr = String.Format("SHOW TABLES FROM `{0}` LIKE '{1}__{2}' ", _options.Database, cols[0], cols[1]);
                                    using (MySqlConnection mySqlConn = new MySqlConnection(mySqlConnStr)) {
                                        mySqlConn.Open();
                                        using (MySqlCommand mySqlCmd = new MySqlCommand()) {
                                            mySqlCmd.Connection = mySqlConn;
                                            mySqlCmd.CommandTimeout = 0;
                                            mySqlCmd.CommandType = CommandType.Text;
                                            mySqlCmd.CommandText = commandStr;
                                            using (MySqlDataReader drr = mySqlCmd.ExecuteReader())
                                            {
                                                tableExists = drr.HasRows;
                                            }
                                        }
                                    }
                                }
                                catch (Exception ex) {
                                    LogInfoMessage(string.Format("ERROR during SHOW TABLES {0}.{1}", cols[0], cols[1]));
                                    LogInfoMessage("Error executing commnd: " + commandStr);
                                    log.Error(ex.Message, ex);
                                    throw;
                                }

                                LogInfoMessage(string.Format("Table {0}.{1} exists: {2}", tableExists, cols[0], cols[1]));

                                bool countDifferences = false;
                                decimal mySqlCount = 0;
                                if (tableExists)
                                {
                                    try {
                                        commandStr = String.Format("SELECT count(*) FROM `{0}`.`{1}__{2}` ", _options.Database, cols[0], cols[1]);
                                        using (MySqlConnection mySqlConn = new MySqlConnection(mySqlConnStr)) {
                                            mySqlConn.Open();
                                            using (MySqlCommand mySqlCmd = new MySqlCommand()) {
                                                mySqlCmd.Connection = mySqlConn;
                                                mySqlCmd.CommandTimeout = 0;
                                                mySqlCmd.CommandType = CommandType.Text;
                                                mySqlCmd.CommandText = commandStr;
                                                using (MySqlDataReader drc = mySqlCmd.ExecuteReader())
                                                {
                                                    drc.Read();
                                                    mySqlCount = drc.GetDecimal(0);
                                                }
                                            }
                                        }
                                    }
                                    catch (Exception ex) {
                                        LogInfoMessage(string.Format("ERROR during SELECT COUNT(*) {0}.{1}", cols[0], cols[1]));
                                        LogInfoMessage("Error executing commnd: " + commandStr);
                                        log.Error(ex.Message, ex);
                                        throw;
                                    }
                                    countDifferences = oraCount != mySqlCount;
                                }
                                
                                LogInfoMessage(string.Format("MySql count: {0}", mySqlCount));

                                if (countDifferences && !_options.Continue ) {
                                    LogInfoMessage("Count difference: previous process failed, retrying");
                                    LogInfoMessage(string.Format("DROP TABLE {0}.{1} started", cols[0], cols[1]));

                                    try {
                                        commandStr = String.Format("DROP TABLE IF EXISTS `{0}`.`{1}__{2}` ", _options.Database, cols[0], cols[1]);
                                        using (MySqlConnection mySqlConn = new MySqlConnection(mySqlConnStr)) {
                                            mySqlConn.Open();
                                            using (MySqlCommand mySqlCmd = new MySqlCommand()) {
                                                mySqlCmd.Connection = mySqlConn;
                                                mySqlCmd.CommandTimeout = 0;
                                                mySqlCmd.CommandType = CommandType.Text;
                                                mySqlCmd.CommandText = commandStr;
                                                mySqlCmd.ExecuteNonQuery();
                                            }
                                        }
                                    }
                                    catch (Exception ex) {
                                        LogInfoMessage(string.Format("ERROR during DROP TABLE {0}.{1}", cols[0], cols[1]));
                                        LogInfoMessage("Error executing commnd: " + commandStr);
                                        log.Error(ex.Message, ex);
                                        throw;
                                    }
                                    LogInfoMessage(string.Format("DROP TABLE {0}.{1} finished", cols[0], cols[1]));
                                }

                                if (!tableExists || countDifferences) {
                                    LogInfoMessage(string.Format("Copying data from table {0}.{1}", cols[0], cols[1]));
                                    int rowsProcessed = 0;
                                    try {
                                        commandStr = String.Format("SELECT * FROM \"{0}\".\"{1}\" ", cols[0], cols[1]);
                                        oraCmd.CommandText = commandStr;
                                        using (OracleDataReader dr = oraCmd.ExecuteReader()) {
                                            int oracleFieldCount = dr.FieldCount;

                                            LogInfoMessage(string.Format("Oracle fieldcount for table {0}.{1}: {2}", cols[0], cols[1], oracleFieldCount));

                                            StringBuilder tablestr = new StringBuilder();
                                            tablestr.Append(String.Format("CREATE TABLE IF NOT EXISTS `{0}`.`{1}__{2}` (", _options.Database, cols[0], cols[1]));

                                            for (int x = 0; x < oracleFieldCount; x++) {
                                                string columnName = dr.GetName(x);
                                                //string mysqlTypeString = OracleToMysqlColumnType(dr.GetDataTypeName(x));
                                                string mysqlTypeString = OracleToMysqlColumnType(dr, x);

                                                //**** for OTTO S+S
                                                if (columnName == "HAIX1KEY")
                                                    mysqlTypeString = "varbinary(20)";
                                                //**** for OTTO S+S

                                                tablestr.Append("`");
                                                tablestr.Append(columnName);
                                                tablestr.Append("` ");
                                                tablestr.Append(mysqlTypeString);
                                                tablestr.Append(" DEFAULT NULL");

                                                if (x < oracleFieldCount - 1) {
                                                    tablestr.Append(",");
                                                }
                                            }

                                            ////**** for OTTO S+S
                                            

                                            ////string columnName2 = "base64_HKEY";
                                            ////string mysqlTypeString2 = "VARCHAR";

                                            ////tablestr.Append(",");
                                            ////tablestr.Append("`");
                                            ////tablestr.Append(columnName2);
                                            ////tablestr.Append("` ");
                                            ////tablestr.Append(mysqlTypeString2);
                                            ////tablestr.Append(" DEFAULT NULL");

                                            //string columnName2 = "binary_HKEY";
                                            //string mysqlTypeString2 = "VARBINARY(40)";

                                            //tablestr.Append(",");
                                            //tablestr.Append("`");
                                            //tablestr.Append(columnName2);
                                            //tablestr.Append("` ");
                                            //tablestr.Append(mysqlTypeString2);
                                            //tablestr.Append(" DEFAULT NULL");

                                            ////**** for OTTO S+S

                                            tablestr.Append(") ENGINE=MyISAM DEFAULT CHARSET=utf8;");
                                            
                                            commandStr = tablestr.ToString();
                                            using (MySqlConnection mySqlConn = new MySqlConnection(mySqlConnStr))
                                            {
                                                mySqlConn.Open();
                                                try {
                                                    using (MySqlCommand mySqlCmd = new MySqlCommand())
                                                    {
                                                        mySqlCmd.Connection = mySqlConn;
                                                        mySqlCmd.CommandTimeout = 0;
                                                        mySqlCmd.CommandType = CommandType.Text;
                                                        mySqlCmd.CommandText = commandStr;
                                                        mySqlCmd.ExecuteNonQuery();
                                                    }
                                                }
                                                catch (Exception ex)
                                                {
                                                    LogInfoMessage("Error executing commnd: " + commandStr);
                                                    log.Error(ex.Message, ex);
                                                    throw;
                                                }

                                                LogInfoMessage(string.Format("Create table statement completed {0}.{1}", cols[0], cols[1]));

                                                // OTTO transfer problem
                                                //Start
                                                //SG20.D125U 1/1
                                                //Fatal error encountered during command execution.

                                                int rowsToInsert = 0;
                                                int paramId = 0;
                                                string valueString = null;
                                                List<string> hkeyParams = new List<string>();
                                                StringBuilder insertCommand = new StringBuilder();
                                                //Dictionary<int, string> oracleType = new Dictionary<int, string>();
                                                //Dictionary<int, Type> dataType = new Dictionary<int, Type>();
                                                string[] oracleType = new string[oracleFieldCount];
                                                Type[] dataType = new Type[oracleFieldCount];
                                                string oracleDataType = null;
                                                Type columnType = null;  

                                                string insertCmdPrefix = String.Format("INSERT INTO `{0}`.`{1}__{2}` (HKEY,binary_HKEY,HZTN,HZEILE132) VALUES (", _options.Database, cols[0], cols[1]);

                                                using (MySqlCommand mySqlInsertCmd = new MySqlCommand())
                                                {
                                                    mySqlInsertCmd.Connection = mySqlConn;
                                                    mySqlInsertCmd.CommandTimeout = 0;
                                                    mySqlInsertCmd.CommandType = CommandType.Text;

                                                    try {
                                                        while (dr.Read()) {
                                                            //if (rowsProcessed != 0 && (rowsProcessed % 10000 == 0 || rowsProcessed == mySqlCount))
                                                            if (rowsProcessed != 0 && (rowsProcessed % 1000000 == 0 || rowsProcessed == mySqlCount))
                                                                LogInfoMessage(string.Format("Progress info: processing table {0}.{1}, Tables:{2:N0}/{3:N0} Rows:{4:N0}/{5:N0} Skip:{6:N0}", cols[0], cols[1], linesIndex, linesMax, rowsProcessed, oraCount, mySqlCount));

                                                            if (_options.Continue && rowsProcessed < mySqlCount) {
                                                                rowsProcessed++;
                                                                if (rowsProcessed % 1000000 == 0)
                                                                    Console.WriteLine("Skipping row {0}", rowsProcessed);
                                                                continue;
                                                            }
                                                            
                                                            #region Insert_Line

                                                            try {
                                                            
                                                                insertCommand.Append(insertCmdPrefix);

                                                                #region append parameters

                                                                for (int fieldNumber = 0; fieldNumber < oracleFieldCount; fieldNumber++)
                                                                {
                                                                    //string oracleDataType = dr.GetDataTypeName(fieldNumber).ToUpper();
                                                                    //Type columnType = dr.GetFieldType(fieldNumber);
                                                                    oracleDataType = oracleType[fieldNumber];
                                                                    if (oracleDataType == null) {
                                                                        oracleDataType = dr.GetDataTypeName(fieldNumber).ToUpper();
                                                                        oracleType[fieldNumber] = oracleDataType;
                                                                    }
                                                                    columnType = dataType[fieldNumber];
                                                                    if (columnType == null) {
                                                                        columnType = dr.GetFieldType(fieldNumber);
                                                                        dataType[fieldNumber] = columnType;
                                                                    }

                                                                    if (dr.IsDBNull(fieldNumber))
                                                                    {
                                                                        insertCommand.Append("null");
                                                                    }
                                                                    else {
                                                                        paramId++;
                                                                        insertCommand.Append("@param" + paramId);

                                                                        #region Convert_Field_Value

                                                                        try
                                                                        {
                                                                            if (oracleDataType == "CLOB" || oracleDataType == "NCLOB")
                                                                            {
                                                                                //mySqlInsertCmd.Parameters.AddWithValue("@param" + paramId, dr.GetOracleClob(fieldNumber).Value);
                                                                                mySqlInsertCmd.Parameters.AddWithValue("@param" + paramId, dr.GetOracleBinary(fieldNumber).Value);
                                                                            }
                                                                            else if (oracleDataType == "BLOB")
                                                                            {
                                                                                //byte[] buf = (byte[])dr.GetOracleBlob(fieldNumber).Value;
                                                                                byte[] buf = (byte[])dr.GetOracleBinary(fieldNumber).Value;
                                                                                mySqlInsertCmd.Parameters.AddWithValue("@param" + paramId, ec.GetString(buf));
                                                                            }
                                                                            else if (columnType == typeof(String))
                                                                            {
                                                                                //mySqlInsertCmd.Parameters.AddWithValue("@param" + paramId, dr.GetString(fieldNumber));
                                                                                valueString = dr.GetString(fieldNumber);
                                                                                mySqlInsertCmd.Parameters.AddWithValue("@param" + paramId, valueString);
                                                                                //LogInfoMessage(string.Format("Value {0}, [{1}]", fieldNumber, valueString));
                                                                            }
                                                                            else if (columnType == typeof(Byte[]))
                                                                            {
                                                                                byte[] buf = (byte[])dr.GetValue(fieldNumber);
                                                                                mySqlInsertCmd.Parameters.AddWithValue("@param" + paramId, ec.GetString(buf));
                                                                            }
                                                                            else if (columnType == typeof(Int64))
                                                                            {
                                                                                mySqlInsertCmd.Parameters.AddWithValue("@param" + paramId, dr.GetInt64(fieldNumber).ToString());
                                                                            }
                                                                            else if (columnType == typeof(Int32))
                                                                            {
                                                                                mySqlInsertCmd.Parameters.AddWithValue("@param" + paramId, dr.GetInt32(fieldNumber).ToString());
                                                                            }
                                                                            else if (columnType == typeof(Int16))
                                                                            {
                                                                                mySqlInsertCmd.Parameters.AddWithValue("@param" + paramId, dr.GetInt16(fieldNumber).ToString());
                                                                            }
                                                                            else if (columnType == typeof(Single))
                                                                            {
                                                                                mySqlInsertCmd.Parameters.AddWithValue("@param" + paramId, dr.GetFloat(fieldNumber).ToString());
                                                                            }
                                                                            else if (columnType == typeof(Double))
                                                                            {
                                                                                mySqlInsertCmd.Parameters.AddWithValue("@param" + paramId, dr.GetDouble(fieldNumber).ToString());
                                                                            }
                                                                            //else if (columnType == typeof(Decimal))
                                                                            //{
                                                                            //    string buf = dr.GetOracleDecimal(fieldNumber).ToString();
                                                                            //    // Oracle gets:   .1234
                                                                            //    // we want    :  0.1234
                                                                            //    if (buf.StartsWith("."))
                                                                            //        buf = buf.Insert(0, "0");
                                                                            //    mySqlInsertCmd.Parameters.AddWithValue("@param" + paramId, buf);
                                                                            //}
                                                                            else if (columnType == typeof(DateTime))
                                                                            {
                                                                                string buf;
                                                                                OracleDate odt;
                                                                                odt = dr.GetOracleDate(fieldNumber);
                                                                                int year = odt.Year;
                                                                                if (year > 9999)
                                                                                {
                                                                                    // check max date value
                                                                                    year = 9999;
                                                                                    buf = new DateTime(year, odt.Month, odt.Day, odt.Hour, odt.Minute, odt.Second).ToString();
                                                                                }
                                                                                else if (year < 1)
                                                                                {
                                                                                    // check min date value
                                                                                    year = 1;
                                                                                    buf = new DateTime(year, odt.Month, odt.Day, odt.Hour, odt.Minute, odt.Second).ToString();
                                                                                }
                                                                                else
                                                                                {
                                                                                    buf = odt.Value.ToString();
                                                                                }
                                                                                mySqlInsertCmd.Parameters.AddWithValue("@param" + paramId, buf);
                                                                            }
                                                                            else
                                                                            {
                                                                                throw new Exception(string.Format("Unknow columnType [{0}], oracle data type [{1}]", columnType == null ? "null" : columnType.FullName, oracleDataType));
                                                                            }

                                                                            ////**** for OTTO S+S
                                                                            //if (fieldNumber == 0) {
                                                                            //    hkeyParams.Add("@param" + paramId);
                                                                            //    paramId++;
                                                                            //    insertCommand.Append(",@param" + paramId);
                                                                            //    //mySqlInsertCmd.Parameters.AddWithValue("@param" + paramId, ec.GetBytes(dr.GetOracleString(fieldNumber).Value));
                                                                            //    mySqlInsertCmd.Parameters.AddWithValue("@param" + paramId, ec.GetBytes(valueString));
                                                                            //}
                                                                            ////**** for OTTO S+S
                                                                        }
                                                                        catch (Exception ex)
                                                                        {
                                                                            string msg = String.Format("Convert error. Column: {0} columnType: {1} Line: {2:N0}", dr.GetName(fieldNumber), columnType == null ? "null" : columnType.ToString(), rowsProcessed);
                                                                            throw new Exception(msg, ex);
                                                                        }

                                                                        #endregion
                                                                    }

                                                                    if (fieldNumber < oracleFieldCount - 1)
                                                                    {
                                                                        insertCommand.Append(",");
                                                                    }
                                                                } //for (int x = 0; x < c; x++)

                                                                #endregion append parameters

                                                                insertCommand.Append(");");
                                                                rowsToInsert++;

                                                                if (rowsToInsert >= 750) {//_options.BatchSize) {
                                                                    InsertRows(insertCommand, ref rowsToInsert, mySqlInsertCmd, ref rowsProcessed, hkeyParams, mySqlConnStr, mySqlConn);
                                                                }

                                                                #region old insert
                                                                    //try {
                                                                    //    commandStr = insertCommand.ToString();
                                                                    //    mySqlInsertCmd.CommandText = commandStr;
                                                                    //    mySqlInsertCmd.ExecuteNonQuery();
                                                                    //}
                                                                    //catch (Exception ex)
                                                                    //{
                                                                    //    LogInfoMessage(string.Format("Error executing commnd, rows processed [{0}]: {1}", rowsProcessed, commandStr));
                                                                    //    log.Error(ex.Message, ex);
                                                                    //    LogMySqlException(ex);
                                                                    //    LogInfoMessage("Logging parameters started...");
                                                                    //    foreach (MySqlParameter mySqlParameter in mySqlInsertCmd.Parameters)
                                                                    //    {
                                                                    //        LogInfoMessage(string.Format("Parameter [{0}] value [{1}]", mySqlParameter.ParameterName, mySqlParameter.Value == null ? "mySqlParameter.Value == null" : mySqlParameter.Value.ToString()));
                                                                    //    }
                                                                    //    LogInfoMessage("Logging parameters finished.");
                                                                    //    //throw;

                                                                    //    //**** for OTTO S+S

                                                                    //    LogInfoMessage(string.Format("Error executing commnd, RETRYING, rows processed [{0}]: {1}", rowsProcessed, commandStr));

                                                                    //    try
                                                                    //    {
                                                                    //        commandStr = insertCommand.ToString();
                                                                    //        mySqlInsertCmd.Parameters[0].Value = "";
                                                                    //        mySqlInsertCmd.CommandText = commandStr;
                                                                    //        mySqlInsertCmd.ExecuteNonQuery();
                                                                    //    }
                                                                    //    catch (Exception ex2)
                                                                    //    {
                                                                    //        LogInfoMessage(string.Format("Error executing commnd, rows processed [{0}]: {1}", rowsProcessed, commandStr));
                                                                    //        log.Error(ex.Message, ex2);
                                                                    //        LogMySqlException(ex2);
                                                                    //        LogInfoMessage("Logging parameters started...");
                                                                    //        foreach (MySqlParameter mySqlParameter in mySqlInsertCmd.Parameters)
                                                                    //        {
                                                                    //            LogInfoMessage(string.Format("Parameter [{0}] value [{1}]", mySqlParameter.ParameterName, mySqlParameter.Value == null ? "mySqlParameter.Value == null" : mySqlParameter.Value.ToString()));
                                                                    //        }
                                                                    //        LogInfoMessage("Logging parameters finished.");
                                                                    //        throw;
                                                                    //    }

                                                                    //    //**** for OTTO S+S
                                                                    //}

                                                                    #endregion old insert
                                                            
                                                            }
                                                            catch (Exception ex) {
                                                                LogInfoMessage(string.Format("Error while inserting data, rows processed [{0}], command: {1}", rowsProcessed, commandStr));
                                                                throw;
                                                            }

                                                            #endregion Inser_Line

                                                            rowsProcessed++;
                                                            //if (rowsProcessed%1000 == 0 || rowsProcessed == oraCount)
                                                            //    LogInfoMessage(string.Format("Progress info: processing table {0}.{1}, Tables:{2:N0}/{3:N0} Rows:{4:N0}/{5:N0} Skip:{6:N0}", cols[0], cols[1], linesIndex, linesMax, rowsProcessed, oraCount, mySqlCount));
                                                        } //while dr.read()
                                                    }
                                                    catch (Exception ex) {
                                                        log.Error(ex.Message, ex);
                                                        LogInfoMessage(string.Format("Error logged WHILE outer {0}.{1}", cols[0], cols[1]));
                                                        throw;
                                                    }

                                                    if (rowsToInsert > 0) {
                                                        InsertRows(insertCommand, ref rowsToInsert, mySqlInsertCmd, ref rowsProcessed, hkeyParams, mySqlConnStr, mySqlConn);
                                                    }
                                                }
                                            } // using MySqlConnection
                                        } //using dr
                                    }
                                    catch (Exception ex) {
                                        log.Error(ex.Message, ex);
                                        LogInfoMessage(string.Format("Error logged regarding table {0}.{1}, rows processed [{2}] continue processing next table...", cols[0], cols[1], rowsProcessed));
                                        tablesWithError.WriteLine("{0}\t{1}", cols[0], cols[1]);
                                    }
                                } //if(!tableExists || dropTable)
                            }//if (oraCount != -1) {
                        } //foreach (string line in lines)
                    }
                }
                LogInfoMessage("Processing tables done!");
            }
        }

        private static void InsertRows(StringBuilder insertCommand, ref int rowsToInsert, MySqlCommand mySqlInsertCmd, ref int rowsProcessed, List<string> hkeyParams, string mySqlConnStr, MySqlConnection conn)
        {
            try {
                mySqlInsertCmd.CommandText = insertCommand.ToString();
                mySqlInsertCmd.ExecuteNonQuery();

                LogInfoMessage(rowsToInsert + " records inserted");

                mySqlInsertCmd.Parameters.Clear();
                insertCommand.Clear();
                rowsToInsert = 0;
                hkeyParams.Clear();
            }
            catch (Exception ex)
            {
                log.Error(ex.Message, ex);
                LogInfoMessage(string.Format("Error executing commnd, rows processed [{0}]", rowsProcessed));
                LogMySqlException(ex);
                //LogInfoMessage("Logging parameters started...");
                //foreach (MySqlParameter mySqlParameter in mySqlInsertCmd.Parameters)
                //{
                //    LogInfoMessage(string.Format("Parameter [{0}] value [{1}]", mySqlParameter.ParameterName, mySqlParameter.Value == null ? "mySqlParameter.Value == null" : mySqlParameter.Value.ToString()));
                //}
                //LogInfoMessage("Logging parameters finished.");
                //throw;

                //**** for OTTO S+S

                LogInfoMessage(string.Format("Error executing commnd, RETRYING, rows processed [{0}]", rowsProcessed));

                try
                {
                    //foreach (MySqlParameter mySqlParameter in mySqlInsertCmd.Parameters) {
                    for (int i = 0; i < mySqlInsertCmd.Parameters.Count; i++) {
                        MySqlParameter mySqlParameter = mySqlInsertCmd.Parameters[i];
                        if (hkeyParams.Contains(mySqlParameter.ParameterName))
                            mySqlParameter.Value = "";
                    }
                    conn = new MySqlConnection(mySqlConnStr);
                    conn.Open();
                    mySqlInsertCmd.Connection = conn;
                    mySqlInsertCmd.CommandText = insertCommand.ToString();
                    mySqlInsertCmd.ExecuteNonQuery();

                    LogInfoMessage(rowsToInsert + " records inserted after exception");

                    mySqlInsertCmd.Parameters.Clear();
                    insertCommand.Clear();
                    rowsToInsert = 0;
                    hkeyParams.Clear();
                }
                catch (Exception ex2)
                {
                    log.Error(ex.Message, ex2);
                    LogInfoMessage(string.Format("Error executing commnd, rows processed [{0}]", rowsProcessed));
                    LogMySqlException(ex2);
                    LogInfoMessage("Logging parameters started...");
                    foreach (MySqlParameter mySqlParameter in mySqlInsertCmd.Parameters)
                    {
                        LogInfoMessage(string.Format("Parameter [{0}] value [{1}]", mySqlParameter.ParameterName, mySqlParameter.Value == null ? "mySqlParameter.Value == null" : mySqlParameter.Value.ToString()));
                    }
                    LogInfoMessage("Logging parameters finished.");
                    throw;
                }

                //**** for OTTO S+S
            }
        }

        private static void LogOracleException(Exception ex) {
            if (ex is OracleException) {
                OracleException oraExc = ex as OracleException;
                LogInfoMessage("OracleException details");
                LogInfoMessage(string.Format("ErrorCode: {0}", oraExc.ErrorCode));
                //LogInfoMessage(string.Format("Number: {0}", oraExc.Number));
                LogInfoMessage(string.Format("Number: {0}", oraExc.Code));
                if (oraExc.InnerException != null) {
                    LogInfoMessage("InnerException details");
                    LogInfoMessage(string.Format("Message: {0}", oraExc.InnerException.Message));
                    LogInfoMessage(string.Format("Source: {0}", oraExc.InnerException.Source));
                    LogInfoMessage(string.Format("StackTrace: {0}", oraExc.InnerException.StackTrace));
                }
            }
        }

        private static void LogMySqlException(Exception ex) {
            if (ex is MySqlException) {
                MySqlException myExc = ex as MySqlException;
                LogInfoMessage("MySqlException details");
                LogInfoMessage(string.Format("ErrorCode: {0}", myExc.ErrorCode));
                LogInfoMessage(string.Format("Number: {0}", myExc.Number));
                if (myExc.InnerException != null) {
                    LogInfoMessage("InnerException details");
                    LogInfoMessage(string.Format("Message: {0}", myExc.InnerException.Message));
                    LogInfoMessage(string.Format("Source: {0}", myExc.InnerException.Source));
                    LogInfoMessage(string.Format("StackTrace: {0}", myExc.InnerException.StackTrace));
                }
            }
        }

        private static void LogInfoMessage(string logMsg) {
            Console.WriteLine(logMsg);
            log.Info(logMsg);
        }

        /*
        
        MySQL
        BLOB column with a maximum length of 65,535 bytes. Each BLOB value is stored using a 2-byte length prefix that indicates the number of bytes in the value.
        TEXT column with a maximum length of 65,535 characters. The effective maximum length is less if the value contains multi-byte characters.
        LONGBLOB column with a maximum length of 4,294,967,295 or 4GB bytes.      The effective maximum length of LONGBLOB columns depends on the configured maximum packet size in the client/server protocol and available memory.
        LONGTEXT column with a maximum length of 4,294,967,295 or 4GB characters. The effective maximum length is less if the value contains multi-byte characters. The effective maximum length of LONGTEXT columns also depends on the configured maximum packet size in the client/server protocol and available memory. 
          
        ORACLE
        BLOB  A binary large object. Maximum size is 4 gigabytes.
        CLOB  A character large object containing single-byte characters. Both fixed-width and variable-width character sets are supported, both using the CHAR  database character set. Maximum size is 4 gigabytes.
        NCLOB A character large object containing multibyte characters.   Both fixed-width and variable-width character sets are supported, both using the NCHAR database character set. Maximum size is 4 gigabytes. Stores national character set data.
        RAW (SIZE) Raw binary data of length size bytes. Maximum size is 2000 bytes. You must specify size for a RAW value.
        LONG (SIZE) Character data of variable length up to 2 gigabytes, or 2^31 -1 bytes.
        LONG RAW Raw binary data of variable length up to 2 gigabytes.
          
        */

        //private static string OracleToMysqlColumnType(string p)
        //{
        //    switch (p.ToUpper()) {
        //        case "CLOB":
        //            return "LONGTEXT";
        //        case "BLOB":
        //            return "LONGBLOB";
        //        case "NCLOB":
        //            return "LONGTEXT";
        //        case "RAW":
        //            return "LONGBLOB";
        //        case "LONG":
        //            return "LONGTEXT";
        //        case "LONG RAW":
        //            return "LONGBLOB";
        //        default:
        //            return "TEXT";
        //    }
        //}

        private static string OracleToMysqlColumnType(OracleDataReader dr, int fieldOrdinal)
        {
            LogInfoMessage(string.Format("Field name [{0}] type [{1}]", dr.GetName(fieldOrdinal), dr.GetDataTypeName(fieldOrdinal)));
            switch (dr.GetDataTypeName(fieldOrdinal))
            {
                case "CLOB":
                    return "LONGTEXT";
                case "BLOB":
                    return "LONGBLOB";
                case "NCLOB":
                    return "LONGTEXT";
                case "RAW":
                    return "LONGBLOB";
                case "LONG":
                    return "LONGTEXT";
                case "LONG RAW":
                    return "LONGBLOB";
                default:
                    return "TEXT";
            }
        }
    }
}