using System;
using DbAccess;
using DbAccess.Structures;

namespace ViewValidator.Manager {
    public static class DatabaseManager {
        public static string LastError{get;set;}
        public static bool TestConnection(DbConfig config) {
            switch (config.DbType) {
                case "Access":
                    if (string.IsNullOrEmpty(config.Hostname)) {
                        LastError = "Keine Access Datenbank ausgewählt.";
                        return false;
                    }
                    break;
                default:
                    if (string.IsNullOrEmpty(config.Hostname)) {
                        LastError = "Keine View Datenbank ausgewählt.";
                        return false;
                    }
                    break;
            }
            try {
                using (IDatabase conn = ConnectionManager.CreateConnection(config)) {
                    conn.Open();
                }
            } catch (Exception ex) {
                LastError = ex.Message + " Hostname " + config.Hostname;
                return false;
            }
            return true;
        }

        internal static bool TestConnections(DbConfig dbConfig1, DbConfig dbConfig2) {
            string finalError = "";
            bool hasError = false;

            if (!TestConnection(dbConfig1)) {
                finalError = LastError;
                hasError = true;
            }
            if (!TestConnection(dbConfig2)) {
                finalError = hasError ? Environment.NewLine + LastError : LastError;
                hasError = true;
            }
            LastError = finalError;
            return !hasError;
        }
    }
}
