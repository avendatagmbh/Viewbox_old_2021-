using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DbAccess;
using DbAccess.Structures;
using eBalanceKitProductManager.Business.Structures.DbMapping;

namespace eBalanceKitProductManager.Business {
    
    internal static class AppConfig {

        public static ConnectionManager ConnectionManager { get; set; }

        public static void Init() {
            try {
                if (ConnectionManager != null) ConnectionManager.Dispose();
                DbConfig dbConfig = new DbConfig("MySQL") { Username = "eBalance", Password = "t9rSLnA=bn38", Hostname = "192.168.67.125" };

                // create database
                var conn = ConnectionManager.CreateConnection(dbConfig);
                conn.Open();
                conn.CreateDatabaseIfNotExists("e_balance_kit_registration");
                conn.Close();
                conn.Dispose();

                dbConfig.DbName = "e_balance_kit_registration";
                ConnectionManager = new ConnectionManager(dbConfig, 4);
                ConnectionManager.Init();

                // create tables if not exist
                using (conn = ConnectionManager.GetConnection()) {
                    conn.DbMapping.CreateTableIfNotExists<Instance>(); 
                    conn.DbMapping.CreateTableIfNotExists<eBalanceKitProductManager.Business.Structures.DbMapping.Version>();
                    conn.DbMapping.CreateTableIfNotExists<Registration>();
                }

            } catch (Exception ex) {
                throw new Exception("Fehler beim Initialisieren der Anwendungskonfiguration: " + Environment.NewLine + ex.Message, ex);
            }
        }

        public static void Dispose() {
            try {
                ConnectionManager.Dispose();
            } catch (Exception ex) {
                System.Diagnostics.Debug.WriteLine("Fehler in AppConfig::Dispose (" + ex.Message + ")");
            }
        }


    }
}
