// --------------------------------------------------------------------------------
// author: Benjamin Held
// since: 2011-12-05
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using AvdCommon.Logging;
using DbAccess;
using DbSearchDatabase.Config;
using ScreenshotAnalyzerDatabase.Config;

namespace ScreenshotAnalyzerDatabase.Upgrader {
    internal class DatabaseUpgrade {
        internal static void UpdateDatabase(DbProfile profile, IDatabase conn) {
            while (VersionInfo.NewerDbVersionExists(profile.Info.DbVerion)) {
                switch(profile.Info.DbVerion) {
                    case "1.0":
                        LogManager.Status("Update der Datenbank auf Version 1.1. Profil: " + profile.Name);
                        UpgradeTo_1_1(conn);
                        profile.Info.DbVerion = "1.1";
                        break;
                    case "1.1":
                        LogManager.Status("Update der Datenbank auf Version 1.2. Profil: " + profile.Name);
                        UpgradeTo_1_2(conn);
                        profile.Info.DbVerion = "1.2";
                        break;
                }
            }
        }

        private static void UpgradeTo_1_2(IDatabase conn) {
        }

        private static void UpgradeTo_1_1(IDatabase conn) {
        }
    }
}
