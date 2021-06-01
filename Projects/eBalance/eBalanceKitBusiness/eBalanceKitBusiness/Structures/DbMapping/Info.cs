using System;
using System.Collections.Generic;
using DbAccess;
using eBalanceKitBase;

namespace eBalanceKitBusiness.Structures.DbMapping {

    /// <summary>
    /// This table stores key/value pairs.
    /// </summary>
    /// <author>Mirko Dibbert</author>
    /// <since>2011-01-17</since>
    [DbTable("info", Description = "global information table", ForceInnoDb = true)]
    public class Info {
        /// <summary>
        /// Initializes a new instance of the <see cref="Info"/> class.
        /// </summary>
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

        public static string DbVerion {
            get {
                try {
                    using (IDatabase conn = AppConfig.ConnectionManager.GetConnection()) {
                        List<Info> tmp = conn.DbMapping.Load<Info>(conn.Enquote("key") + "='" + "DbVersion" + "'");
                        if (tmp.Count > 0) {
                            return tmp[0].Value.ToString();
                        } else {
                            return null;
                        }
                    }
                } catch (Exception ex) {
                    global::System.Diagnostics.Debug.WriteLine("error in Info::DbVersion.Get: " + Environment.NewLine + ex.Message);
                    return VersionInfo.Instance.CurrentDbVersion;
                }
            }
            set {
                try {
                    using (IDatabase conn = AppConfig.ConnectionManager.GetConnection()) {
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
                    }
                } catch (Exception ex) {
                    global::System.Diagnostics.Debug.WriteLine("error in Info::DbVersion.Set: " + Environment.NewLine + ex.Message);
                }
            }

        }

        public static string Serial {
            get {
                try {
                    using (IDatabase conn = AppConfig.ConnectionManager.GetConnection()) {
                        List<Info> tmp = conn.DbMapping.Load<Info>(conn.Enquote("key") + "='" + "serial" + "'");
                        if (tmp.Count > 0) {
                            return tmp[0].Value;
                        } else {
                            return null;
                        }
                    }
                } catch (Exception) {
                    return null;
                }
            }
            set {
                try {
                    using (IDatabase conn = AppConfig.ConnectionManager.GetConnection()) {
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
                    }
                } catch (Exception) {
                }
            }
        }

        public static string RK {
            get {
                try {
                    using (IDatabase conn = AppConfig.ConnectionManager.GetConnection()) {
                        List<Info> tmp = conn.DbMapping.Load<Info>(conn.Enquote("key") + "='" + "rk" + "'");
                        if (tmp.Count > 0) {
                            return tmp[0].Value;
                        } else {
                            return null;
                        }
                    }
                } catch (Exception) {
                    return null;
                }
            }
            set {
                try {
                    using (IDatabase conn = AppConfig.ConnectionManager.GetConnection()) {
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
                    }
                } catch (Exception) {
                }
            }
        }

        public static string RegCompany {
            get {
                try {
                    using (IDatabase conn = AppConfig.ConnectionManager.GetConnection()) {
                        List<Info> tmp = conn.DbMapping.Load<Info>(conn.Enquote("key") + "='" + "reg_company" + "'");
                        if (tmp.Count > 0) {
                            return tmp[0].Value;
                        } else {
                            return null;
                        }
                    }
                } catch (Exception) {
                    return null;
                }
            }
            set {
                try {
                    using (IDatabase conn = AppConfig.ConnectionManager.GetConnection()) {
                        List<Info> tmp = conn.DbMapping.Load<Info>(conn.Enquote("key") + "='" + "reg_company" + "'");
                        if (tmp.Count > 0) {
                            tmp[0].Value = value;
                            conn.DbMapping.Save(tmp[0]);
                        } else {
                            // unknown value
                            Info info = new Info { Key = "reg_company" };
                            info.Value = value;
                            conn.DbMapping.Save(info);
                        }
                    }
                } catch (Exception) {
                }
            }
        }

        public static string RegName {
            get {
                try {
                    using (IDatabase conn = AppConfig.ConnectionManager.GetConnection()) {
                        List<Info> tmp = conn.DbMapping.Load<Info>(conn.Enquote("key") + "='" + "reg_name" + "'");
                        if (tmp.Count > 0) {
                            return tmp[0].Value;
                        } else {
                            return null;
                        }
                    }
                } catch (Exception) {
                    return null;
                }
            }
            set {
                try {
                    using (IDatabase conn = AppConfig.ConnectionManager.GetConnection()) {
                        List<Info> tmp = conn.DbMapping.Load<Info>(conn.Enquote("key") + "='" + "reg_name" + "'");
                        if (tmp.Count > 0) {
                            tmp[0].Value = value;
                            conn.DbMapping.Save(tmp[0]);
                        } else {
                            // unknown value
                            Info info = new Info { Key = "reg_name" };
                            info.Value = value;
                            conn.DbMapping.Save(info);
                        }
                    }
                } catch (Exception) {
                }
            }
        }

        public static string RegMail {
            get {
                try {
                    using (IDatabase conn = AppConfig.ConnectionManager.GetConnection()) {
                        List<Info> tmp = conn.DbMapping.Load<Info>(conn.Enquote("key") + "='" + "reg_company" + "'");
                        if (tmp.Count > 0) {
                            return tmp[0].Value;
                        } else {
                            return null;
                        }
                    }
                } catch (Exception) {
                    return null;
                }
            }
            set {
                try {
                    using (IDatabase conn = AppConfig.ConnectionManager.GetConnection()) {
                        List<Info> tmp = conn.DbMapping.Load<Info>(conn.Enquote("key") + "='" + "reg_mail" + "'");
                        if (tmp.Count > 0) {
                            tmp[0].Value = value;
                            conn.DbMapping.Save(tmp[0]);
                        } else {
                            // unknown value
                            Info info = new Info { Key = "reg_mail" };
                            info.Value = value;
                            conn.DbMapping.Save(info);
                        }
                    }
                } catch (Exception) {
                }
            }
        }

        internal static void SetValue(string key, string value) {
            using (IDatabase conn = AppConfig.ConnectionManager.GetConnection()) {
                SetValue(key, value, conn);
            }
        }

        internal static void SetValue(string key, string value, IDatabase conn) {
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

        internal static string GetValue(string key) {
            using (IDatabase conn = AppConfig.ConnectionManager.GetConnection()) {
                return GetValue(key, conn);
            }
        }

        internal static string GetValue(string key, IDatabase conn) {
            object value = conn.ExecuteScalar(
            "SELECT " + conn.Enquote("value") + " FROM " + conn.Enquote(conn.DbMapping.GetTableName<Info>()) +
            " WHERE " + conn.Enquote("key") + "='" + key + "'");
            return value == null ? null : value.ToString();
        }

        internal static void RemoveValue(string key) {
            using (IDatabase conn = AppConfig.ConnectionManager.GetConnection()) {
                RemoveValue(key, conn);
            }
        }

        internal static void RemoveValue(string key, IDatabase conn) {
                conn.ExecuteNonQuery(
                    "DELETE FROM " + conn.Enquote(conn.DbMapping.GetTableName<Info>()) + 
                    " WHERE " + conn.Enquote("key") + "='" + key + "'");        
        }

        internal static void Init() {
            // init db-version for new databases
            if (DbVerion== null) DbVerion = VersionInfo.Instance.CurrentDbVersion;
        }
    }
}
