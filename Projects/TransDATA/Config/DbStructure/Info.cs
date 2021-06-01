// (C)opyright 2011 AvenDATA GmbH
// ----------------------------------------
// author: Mirko Dibbert
// since : 2011-08-27

using System.Collections.Generic;
using Config.Interfaces.DbStructure;
using DbAccess;
using Utils;

namespace Config.DbStructure {
    /// <summary>
    /// This class represents the info table.
    /// </summary>
    [DbTable("info")]
    internal class Info : NotifyPropertyChangedBase {
        [DbColumn("id", AllowDbNull = false)]
        [DbPrimaryKey]
        public int Id { get; set; }

        [DbColumn("key", AllowDbNull = false)]
        [DbUniqueKey]
        public string Key { get; set; }

        [DbColumn("value", AllowDbNull = true)]
        public string Value { get; set; }


        public static string DbVerion {
            get { return GetValue("DbVersion"); }
            set { SetValue("DbVersion", value); }
        }

        public static string LastProfile {
            get { return GetValue("LastProfile"); }
            set { SetValue("LastProfile", value); }
        }

        public static string LastLanguage
        {
            get { return GetValue("LastLanguage"); }
            set { SetValue("LastLanguage", value); }
        }

        internal static void SetValue(string key, string value) {
            using (IDatabase conn = ConfigDb.GetConnection()) SetValue(key, value, conn);
        }

        internal static void SetValue(string key, string value, IDatabase conn) {
            List<Info> tmp = conn.DbMapping.Load<Info>(conn.Enquote("key") + "='" + key + "'");
            if (tmp.Count > 0) {
                tmp[0].Value = value;
                conn.DbMapping.Save(tmp[0]);
            }
            else {
                // unknown value
                var info = new Info {Key = key, Value = value};
                conn.DbMapping.Save(info);
            }
        }


        internal static string GetValue(string key) {
            using (IDatabase conn = ConfigDb.GetConnection()) return GetValue(key, conn);
        }

        internal static string GetValue(string key, IDatabase conn) {
            object value = conn.ExecuteScalar(
                "SELECT " + conn.Enquote("value") + " FROM " + conn.Enquote(conn.DbMapping.GetTableName<Info>()) +
                " WHERE " + conn.Enquote("key") + "='" + key + "'");
            return value == null ? null : value.ToString();
        }


        internal static void RemoveValue(string key) {
            using (IDatabase conn = ConfigDb.GetConnection()) RemoveValue(key, conn);
        }

        internal static void RemoveValue(string key, IDatabase conn) {
            conn.ExecuteNonQuery(
                "DELETE FROM " + conn.Enquote(conn.DbMapping.GetTableName<Info>()) +
                " WHERE " + conn.Enquote("key") + "='" + key + "'");
        }
    }
}