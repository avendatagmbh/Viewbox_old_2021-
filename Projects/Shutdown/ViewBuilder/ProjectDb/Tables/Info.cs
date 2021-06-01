using System.Collections.Generic;
using DbAccess;

namespace ProjectDb.Tables
{
    /// <summary>
    ///   Mapping class for the system database table "info".
    /// </summary>
    [DbTable(TABLENAME)]
    internal class Info
    {
        internal const string TABLENAME = "info";
        private const string COLNAME_KEY = "key";
        private const string COLNAME_VALUE = "value";
        internal static string KEY_VERSION = "version";
        internal static string VALUE_VERSION_ACTUAL = "1.0.1";

        [DbColumn("id"), DbPrimaryKey]
        public int Id { get; set; }

        [DbColumn(COLNAME_KEY, Length = 64)]
        public string Key { get; set; }

        [DbColumn(COLNAME_VALUE, Length = 1024)]
        public string Value { get; set; }

        /// <summary>
        ///   Creates the table.
        /// </summary>
        /// <param name="conn"> The connection. </param>
        internal static void CreateTable(IDatabase conn)
        {
            conn.DbMapping.CreateTableIfNotExists<Info>();
            string version = GetValue(conn, KEY_VERSION);
            if (version == null)
            {
                SetValue(conn, KEY_VERSION, VALUE_VERSION_ACTUAL);
            }
        }

        /// <summary>
        ///   Gets the value.
        /// </summary>
        /// <param name="conn"> The connection. </param>
        /// <param name="key"> The key. </param>
        internal static string GetValue(IDatabase conn, string key)
        {
            object result = conn.ExecuteScalar(
                "SELECT " + conn.Enquote(COLNAME_VALUE) + " FROM " + conn.Enquote(TABLENAME) + " " +
                "WHERE " + conn.Enquote(COLNAME_KEY) + "=" + conn.GetSqlString(key));
            if (result == null)
            {
                return null;
            }
            else
            {
                return result.ToString();
            }
        }

        /// <summary>
        ///   Sets the value.
        /// </summary>
        /// <param name="conn"> The connection. </param>
        /// <param name="key"> The key. </param>
        /// <param name="value"> The value. </param>
        internal static void SetValue(IDatabase conn, string key, string value)
        {
            List<Info> infos =
                conn.DbMapping.Load<Info>(string.Format("{0}={1}", conn.Enquote(COLNAME_KEY), conn.GetSqlString(value)));
            Info info = new Info {Key = key, Value = value};
            if (infos.Count >= 1)
                info = infos[0];
            conn.DbMapping.Save(info);
            //conn.ExecuteNonQuery("REPLACE INTO " + conn.Enquote(TABLENAME) + " VALUES (" + conn.GetSqlString(key) + "," + conn.GetSqlString(value) + ")");
        }

        /// <summary>
        ///   Deletes the key.
        /// </summary>
        /// <param name="conn"> The connection. </param>
        /// <param name="key"> The key. </param>
        internal static void DeleteKey(IDatabase conn, string key)
        {
            conn.ExecuteNonQuery("DELETE FROM " + conn.Enquote(TABLENAME) + " WHERE " + conn.Enquote(COLNAME_KEY) + "=" +
                                 conn.GetSqlString(key));
        }
    }
}