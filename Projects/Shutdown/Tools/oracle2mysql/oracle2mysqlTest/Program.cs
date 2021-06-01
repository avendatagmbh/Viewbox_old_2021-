using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;

namespace oracle2mysqlTest
{
    class Program
    {
        static void Main(string[] args)
        {
            string mySqlConnStr = String.Format("server={0};userid={1};password={2}; Connection Timeout=120", "dbottohosting", "root", "avendata");

            using (MySqlConnection mySqlConn = new MySqlConnection(mySqlConnStr))
            {
                mySqlConn.Open();
                using (MySqlCommand mySqlCmd = new MySqlCommand())
                {
                    mySqlCmd.Connection = mySqlConn;
                    mySqlCmd.CommandTimeout = 0;
                    mySqlCmd.CommandType = CommandType.Text;

                    //StringBuilder insertCommand = new StringBuilder();
                    //insertCommand.Append(String.Format("INSERT INTO `{0}`.`{1}__{2}` VALUES (", "test_oracle", "sg20", "d125u"));
                    //insertCommand.Append("@param1");
                    //insertCommand.Append("@param2");
                    //insertCommand.Append("@param3");
                    //mySqlCmd.CommandText = insertCommand.ToString();
                    ////mySqlCmd.Parameters.AddWithValue("@param1", "@$al a"); // original: fails
                    ////mySqlCmd.Parameters.AddWithValue("@param1", "@$al a"); // no backspace: fails
                    ////mySqlCmd.Parameters.AddWithValue("@param1", "@$al a"); // no backspace, no stxf: fails
                    //mySqlCmd.Parameters.AddWithValue("@param1", "@$al a"); // no backspace, no stxf, no ff
                    //mySqlCmd.Parameters.AddWithValue("@param2", "1");
                    //mySqlCmd.Parameters.AddWithValue("@param3", "====================================================================================================================================");

                    //try
                    //{
                    //    mySqlCmd.CommandText = insertCommand.ToString();
                    //    mySqlCmd.ExecuteNonQuery();
                    //}
                    //catch (Exception ex)
                    //{
                    //    throw;
                    //}

                    StringBuilder insertCommand = new StringBuilder();
                    insertCommand.Append(String.Format("INSERT INTO `{0}`.`{1}__{2}` VALUES (", "ottod125", "sg20", "d125u"));
                    insertCommand.Append("@param1");
                    insertCommand.Append(",@param2");
                    insertCommand.Append(",@param3");
                    insertCommand.Append(");");
                    mySqlCmd.CommandText = insertCommand.ToString();
                    mySqlCmd.Parameters.AddWithValue("@param1", "@$al a"); // original: ok
                    //mySqlCmd.Parameters.AddWithValue("@param1", "@$al a"); // no backspace: ok
                    //mySqlCmd.Parameters.AddWithValue("@param1", "@$al a"); // no backspace, no stxf: ok
                    //mySqlCmd.Parameters.AddWithValue("@param1", "@$al a"); // no backspace, no stxf, no ff: ok
                    mySqlCmd.Parameters.AddWithValue("@param2", "1");
                    mySqlCmd.Parameters.AddWithValue("@param3", "====================================================================================================================================");

                    try
                    {
                        mySqlCmd.CommandText = insertCommand.ToString();
                        mySqlCmd.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        throw;
                    }
                }
            }
        }
    }
}
