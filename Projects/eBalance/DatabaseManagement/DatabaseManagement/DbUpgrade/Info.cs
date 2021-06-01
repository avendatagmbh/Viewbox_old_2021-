using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DbAccess;
using DbAccess.Structures;
using eBalanceKitBase;

namespace DatabaseManagement.DbUpgrade {
    [DbTable("info", Description = "global information table", ForceInnoDb = true)]
    public class Info {

        IDatabase conn;
        /// <summary>
        /// Initializes a new instance of the <see cref="Info"/> class.
        /// </summary>
        public Info(IDatabase conn) {
            this.conn = conn;
        }
        public Info() {

        }

        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        /// <value>The id.</value>
        [DbColumn("id", AllowDbNull = false)]
        [DbPrimaryKey]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the key.
        /// </summary>
        /// <value>The key.</value>
        [DbColumn("key", AllowDbNull = false)]
        [DbUniqueKey]
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>The value.</value>
        [DbColumn("value", AllowDbNull = true)]
        public string Value { get; set; }

        private ConnectionManager ConnectionManager { get; set; }

        public string DbVerion {
            get {
                try {
                    //using (IDatabase conn = ConnectionManager.GetConnection()) {
                        List<Info> tmp = conn.DbMapping.Load<Info>(conn.Enquote("key") + "='" + "DbVersion" + "'");
                        if (tmp.Count > 0) {
                            return tmp[0].Value.ToString();
                        } else {
                            return null;
                        }
                    //}
                } catch (Exception ex) {
                    global::System.Diagnostics.Debug.WriteLine("error in Info::DbVersion.Get: " + Environment.NewLine + ex.Message);
                    return VersionInfo.Instance.CurrentDbVersion;
                }
            }
            set {
                try {
                    //using (IDatabase conn = ConnectionManager.GetConnection()) {
                        List<Info> tmp = conn.DbMapping.Load<Info>(conn.Enquote("key") + "='" + "DbVersion" + "'");
                        if (tmp.Count > 0) {
                            tmp[0].Value = value;
                            conn.DbMapping.Save(tmp[0]);
                        } else {
                            // unknown value
                            Info info = new Info { Key = "DbVersion" };
                            info.Value = value;
                            conn.DbMapping.Save(info);
                        }
                    //}
                } catch (Exception ex) {
                    global::System.Diagnostics.Debug.WriteLine("error in Info::DbVersion.Set: " + Environment.NewLine + ex.Message);
                }
            }

        }

        public string Serial {
            get {
                try {
                    //using (IDatabase conn = ConnectionManager.GetConnection()) {
                        List<Info> tmp = conn.DbMapping.Load<Info>(conn.Enquote("key") + "='" + "serial" + "'");
                        if (tmp.Count > 0) {
                            return tmp[0].Value;
                        } else {
                            return null;
                        }
                    //}
                } catch (Exception) {
                    return null;
                }
            }
            set {
                try {
                    //using (IDatabase conn = ConnectionManager.GetConnection()) {
                        List<Info> tmp = conn.DbMapping.Load<Info>(conn.Enquote("key") + "='" + "serial" + "'");
                        if (tmp.Count > 0) {
                            tmp[0].Value = value;
                            conn.DbMapping.Save(tmp[0]);
                        } else {
                            // unknown value
                            Info info = new Info { Key = "serial" };
                            info.Value = value;
                            conn.DbMapping.Save(info);
                        }
                    //}
                } catch (Exception) {
                }
            }
        }

        public string RK {
            get {
                try {
                    //using (IDatabase conn = ConnectionManager.GetConnection()) {
                        List<Info> tmp = conn.DbMapping.Load<Info>(conn.Enquote("key") + "='" + "rk" + "'");
                        if (tmp.Count > 0) {
                            return tmp[0].Value;
                        } else {
                            return null;
                        }
                    //}
                } catch (Exception) {
                    return null;
                }
            }
            set {
                try {
                    //using (IDatabase conn = ConnectionManager.GetConnection()) {
                        List<Info> tmp = conn.DbMapping.Load<Info>(conn.Enquote("key") + "='" + "rk" + "'");
                        if (tmp.Count > 0) {
                            tmp[0].Value = value;
                            conn.DbMapping.Save(tmp[0]);
                        } else {
                            // unknown value
                            Info info = new Info { Key = "rk" };
                            info.Value = value;
                            conn.DbMapping.Save(info);
                        }
                    //}
                } catch (Exception) {
                }
            }
        }

        internal void SetValue(string key, string value) {
            //using (IDatabase conn = ConnectionManager.GetConnection()) {
                SetValue(key, value, conn);
            //}
        }

        internal void SetValue(string key, string value, IDatabase conn) {
            List<Info> tmp = conn.DbMapping.Load<Info>(conn.Enquote("key") + "='" + key + "'");
            if (tmp.Count > 0) {
                tmp[0].Value = value;
                conn.DbMapping.Save(tmp[0]);
            } else {
                // unknown value
                Info info = new Info { Key = key };
                info.Value = value;
                conn.DbMapping.Save(info);
            }
        }

        internal string GetValue(string key) {
            //using (IDatabase conn = ConnectionManager.GetConnection()) {
                return GetValue(key, conn);
            //}
        }

        internal string GetValue(string key, IDatabase conn) {
            object value = conn.ExecuteScalar(
            "SELECT " + conn.Enquote("value") + " FROM " + conn.Enquote(conn.DbMapping.GetTableName<Info>()) +
            " WHERE " + conn.Enquote("key") + "='" + key + "'");
            return value == null ? null : value.ToString();
        }

        internal void RemoveValue(string key) {
            //using (IDatabase conn = ConnectionManager.GetConnection()) {
                RemoveValue(key, conn);
            //}
        }

        internal void RemoveValue(string key, IDatabase conn) {
            conn.ExecuteNonQuery(
                "DELETE FROM " + conn.Enquote(conn.DbMapping.GetTableName<Info>()) +
                " WHERE " + conn.Enquote("key") + "='" + key + "'");
        }

        internal void Init() {
            // init db-version for new databases
            //if (DbVerion == null) DbVerion = VersionInfo.;
        }
    }
}
