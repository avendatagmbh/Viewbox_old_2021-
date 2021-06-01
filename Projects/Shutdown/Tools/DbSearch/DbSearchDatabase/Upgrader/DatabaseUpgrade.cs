using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DbAccess;
using DbSearchDatabase.Config;
using log4net;
using AV.Log;

namespace DbSearchDatabase.Upgrader {
    internal class DatabaseUpgrade {

        internal static ILog _log = LogHelper.GetLogger();

        internal static void UpdateDatabase(DbProfile profile, IDatabase conn) {
            while (VersionInfo.NewerDbVersionExists(profile.Info.DbVerion)) {
                switch(profile.Info.DbVerion) {
                    case "1.0":
                        _log.Log(LogLevelEnum.Info, "Update der Datenbank auf Version 1.1. Profil: " + profile.Name, true);
                        UpgradeTo_1_1(conn);
                        profile.Info.DbVerion = "1.1";
                        break;
                    case "1.1":
                        _log.Log(LogLevelEnum.Info, "Update der Datenbank auf Version 1.2. Profil: " + profile.Name, true);
                        UpgradeTo_1_2(conn);
                        profile.Info.DbVerion = "1.2";
                        break;
                }
            }
        }

        private static void UpgradeTo_1_2(IDatabase conn) {
            conn.DbMapping.AddColumn("query_columns", "String", new DbColumnAttribute("orig_name") { Length = 512 }, null, "name");
            conn.ExecuteNonQuery("UPDATE query_columns SET `orig_name`=`name`");
        }

        private static void UpgradeTo_1_1(IDatabase conn) {
            conn.DbMapping.AddColumn("profiles", "String", new DbColumnAttribute("custom_rules") { Length = 100000 }, null, "dbconfig");
            
        }
    }
}
