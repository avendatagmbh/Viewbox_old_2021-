// -----------------------------------------------------------
// Created by Benjamin Held - 22.07.2011 15:45:01
// Copyright AvenDATA 2011
// -----------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using eBalanceKitBase;
using DatabaseManagement.DbUpgrade;

namespace eBalanceKitBackup {
    class Program {
        static string CommandHelp = 
            "eBalanceKitBackup.exe --dir Ausgabeverzeichnis [--comment \"Hier kann ein Kommentar stehen\"] [--quiet]" + Environment.NewLine
            ;
        static bool Quiet { get; set; }

        static bool CheckDirectory(string dir) {
            DirectoryInfo dirInfo = new DirectoryInfo(dir);

            if (!dirInfo.Exists) {
                Output("Das Verzeichnis \"" + dir + "\" existiert nicht.");
                return false;
            }

            return true;
        }

        static int Output(string message) {
            if(!Quiet)
                Console.WriteLine(message);
            return 1;
        }

        static void Backup(string dir, DatabaseConfig config, string comment, out string filename) {
            DatabaseManagement.DbUpgrade.eBalanceBackup.UserInfo userInfo = new DatabaseManagement.DbUpgrade.eBalanceBackup.UserInfo() { Comment = comment };

            filename = dir + "eBalanceKitBackup_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".bak";
            eBalanceBackup backup = new eBalanceBackup();
            using (var conn = DbAccess.ConnectionManager.CreateConnection(config.GetDbConfig())) {
                conn.Open();
                backup.ExportDatabase(conn, filename, userInfo);
            }
        }

        static int Main(string[] args) {
            CommandLineParser parser = new CommandLineParser(args, "--");

            if (parser.HasSwitch("help")) {
                Output(CommandHelp);
                return 0;
            }
            if (parser.HasSwitch("quiet")) Quiet = true;
            if (!parser.HasSwitch("dir")) return Output("Sie haben kein Ausgabeverzeichnis angegeben: " + Environment.NewLine + CommandHelp);
            try {
                string dir = parser.Argument("dir");
                if (!dir.EndsWith("/") || !dir.EndsWith("\\")) dir += "\\";
                string comment = "";
                if (parser.HasSwitch("comment")) comment = parser.Argument("comment");

                if (!CheckDirectory(dir))
                    return 1;
                DatabaseConfig config = new DatabaseConfig();

                if (!config.ExistsConfigfile) {
                    Output("Die Config Datei \"" + config.UsedConfigPath + DatabaseConfig.ConfigFile + "\" konnte nicht gelesen werden.");
                    return 1;
                }
                config.LoadConfig();

                string filename;
                Backup(dir, config, parser.HasSwitch("comment") ? parser.Argument("comment") : "", out filename);
                Output("Backup erfolgreich gespeichert unter \"" + filename + "\".");
            }
            catch(Exception ex){
                return Output("Die Datenbank konnte nicht exportiert werden: " + Environment.NewLine + ex.Message);
            }
            return 0;
        }
    }
}
