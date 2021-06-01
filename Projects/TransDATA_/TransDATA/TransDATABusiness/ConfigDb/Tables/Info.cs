/**************************************************************************************************************
 * author               date            comment
 * ------------------------------------------------------------------------------------------------------------
 * Mirko Dibbert        2010-11-21      initial implementation
 *************************************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using DbAccess;

namespace TransDATABusiness.ConfigDb.Tables {

    /// <summary>
    /// Mapping class for the system database table "info".
    /// </summary>
    internal static class Info {

        internal static string KEY_VERSION = "version";
        internal static string VALUE_VERSION_ACTUAL = "1.0.0";

        internal static string KEY_PASSWORD = "password";
        internal static string VALUE_INITIAL_PASSWORD = "admin";

        internal const string TABLENAME = "info";
        private const string COLNAME_KEY = "key";
        private const string COLNAME_VALUE = "value";

        /// <summary>
        /// Creates the table.
        /// </summary>
        /// <param name="conn">The connection.</param>
        internal static void CreateTable(IDatabase conn) {
            if (conn.DbConfig.DbType.Equals(
                DbAccess.ConnectionManager.GetDbName(
                typeof(DbAccess.DbSpecific.SQLite.Database)))) {

                if (conn.TableExists(TABLENAME)) return;

                conn.ExecuteNonQuery(
                    "CREATE TABLE IF NOT EXISTS " + conn.Enquote(TABLENAME) + "(" +
                        conn.Enquote(COLNAME_KEY) + "  TEXT," +
                        conn.Enquote(COLNAME_VALUE) + " TEXT," +
                        "PRIMARY KEY (" + conn.Enquote(COLNAME_KEY) + ")" +
                    ")");

                // set datbase version
                string version = GetValue(conn, KEY_VERSION);
                if (version == null) {
                    SetValue(conn, KEY_VERSION, VALUE_VERSION_ACTUAL);
                }

                // set initial password
                string password = GetValue(conn, KEY_PASSWORD);
                if (password == null) {
                    SetValue(conn, KEY_PASSWORD, ComputePasswordHash(VALUE_INITIAL_PASSWORD));
                }
            } else {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <param name="conn">The connection.</param>
        /// <param name="key">The key.</param>
        internal static string GetValue(IDatabase conn, string key) {
            object result = conn.ExecuteScalar(
                "SELECT " + conn.Enquote(COLNAME_VALUE) + " FROM " + conn.Enquote(TABLENAME) + " " +
                "WHERE " + conn.Enquote(COLNAME_KEY) + "=" + conn.GetSqlString(key));

            if (result == null) {
                return null;
            } else {
                return result.ToString();
            }
        }

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="conn">The connection.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        internal static void SetValue(IDatabase conn, string key, string value) {
            conn.ExecuteNonQuery("REPLACE INTO " + conn.Enquote(TABLENAME) + " VALUES (" + conn.GetSqlString(key) + "," + conn.GetSqlString(value) + ")");
        }

        /// <summary>
        /// Deletes the key.
        /// </summary>
        /// <param name="conn">The connection.</param>
        /// <param name="key">The key.</param>
        internal static void DeleteKey(IDatabase conn, string key) {
            conn.ExecuteNonQuery("DELETE FROM " + conn.Enquote(TABLENAME) + " WHERE " + conn.Enquote(COLNAME_KEY) + "=" + conn.GetSqlString(key));
        }

        /*****************************************************************************************************/

        #region passwordHash

        /// <summary>
        /// Checks the password.
        /// </summary>
        /// <param name="password">The password.</param>
        /// <returns></returns>
        public static bool CheckPassword(IDatabase conn, string password) {
            string currentPassword = GetValue(conn, KEY_PASSWORD);
            string passwordHash = ComputePasswordHash(password);
            return currentPassword.Equals(passwordHash);
        }

        /// <summary>
        /// Sets the password.
        /// </summary>
        /// <param name="password">The password.</param>
        public static void SetPassword(IDatabase conn, string password) {
            SetValue(conn, KEY_PASSWORD, ComputePasswordHash(password));
        }

        /// <summary>
        /// Computes this instance.
        /// </summary>
        /// <param name="password">The password.</param>
        /// <param name="salt">The salt.</param>
        /// <returns></returns>
        private static string ComputePasswordHash(string password) {
            return ByteArrayToString(
                    new SHA256CryptoServiceProvider().ComputeHash(
                        ASCIIEncoding.ASCII.GetBytes(password)));
        }

        /// <summary>
        /// Bytes the array to string.
        /// </summary>
        /// <param name="arrInput">The arr input.</param>
        /// <returns></returns>
        private static string ByteArrayToString(byte[] arrInput) {
            StringBuilder sOutput = new StringBuilder(arrInput.Length);
            for (int i = 0; i < arrInput.Length - 1; i++) {
                sOutput.Append(arrInput[i].ToString("X2"));
            }
            return sOutput.ToString();
        }


        #endregion passwordHash
    }
}
