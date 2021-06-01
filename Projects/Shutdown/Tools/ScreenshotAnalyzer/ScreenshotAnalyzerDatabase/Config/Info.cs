// --------------------------------------------------------------------------------
// author: Benjamin Held
// since: 2011-09-09
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------

using System.Collections.Generic;
using DbAccess;
using DbSearchDatabase.Config;
using ScreenshotAnalyzerDatabase.Structures;

namespace ScreenshotAnalyzerDatabase.Config {
    [DbTable("info", ForceInnoDb = true)]
    internal class Info : DatabaseObjectBase<int> {
        #region Constructor
        public Info() {
            
        }
        public Info(DbProfile dbProfile) {
            DbProfile = dbProfile;
        }
        #endregion

        #region Properties

        [DbColumn("key", AllowDbNull = false)]
        [DbUniqueKey]
        public string Key { get; set; }

        [DbColumn("value", AllowDbNull = true)]
        public string Value { get; set; }


        public string DbVerion {
            get { return GetValue("DbVersion"); }
            set { SetValue("DbVersion", value); }
        }
        #endregion

        private readonly DbProfile DbProfile;

        internal void SetValue(string key, string value) {
            using (IDatabase conn = DbProfile.GetOpenConnection()) SetValue(key, value, conn);
        }

        internal void SetValue(string key, string value, IDatabase conn) {
            List<Info> tmp = conn.DbMapping.Load<Info>(conn.Enquote("key") + "=" + conn.GetSqlString(key));
            if (tmp.Count > 0) {
                tmp[0].Value = value;
                conn.DbMapping.Save(tmp[0]);
            }
            else {
                // unknown value
                var info = new Info(DbProfile) {Key = key, Value = value};
                conn.DbMapping.Save(info);
            }
        }


        internal string GetValue(string key) {
            using (IDatabase conn = DbProfile.GetOpenConnection()) return GetValue(key, conn);
        }

        internal string GetValue(string key, IDatabase conn) {
            string sql = "SELECT " + conn.Enquote("value") + " FROM " + conn.Enquote(conn.DbMapping.GetTableName<Info>()) +
                " WHERE " + conn.Enquote("key") + "=" + conn.GetSqlString(key);
            object value = conn.ExecuteScalar(sql);
            return value == null ? null : value.ToString();
        }


        internal void RemoveValue(string key) {
            using (IDatabase conn = DbProfile.GetOpenConnection()) RemoveValue(key, conn);
        }

        internal void RemoveValue(string key, IDatabase conn) {
            conn.ExecuteNonQuery(
                "DELETE FROM " + conn.Enquote(conn.DbMapping.GetTableName<Info>()) +
                " WHERE " + conn.Enquote("key") + "='" + key + "'");
        }
    }
}
