using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DbAccess;
using DbAccess.Structures;
using MySql.Data.MySqlClient;

namespace DbConsolidateBusiness {
    public class Consolidater {
        private List<SourceDatabase> _sourceDatabases { get; set; }
        public List<SourceDatabase> SourceDatabases { get { return _sourceDatabases; } }

        private DbConfig _destinationDbConfig { get; set; }
        public DbConfig DestinationDbConfig { get { return _destinationDbConfig; } }

        private DbConfig _dd03lDbConfig { get; set; }
        public DbConfig Dd03lDbConfig { get { return _dd03lDbConfig; } }

        public Consolidater(List<SourceDatabase> sourceDatabases, DbConfig destinationDbConfig, DbConfig dd03lDbConfig) {
            _sourceDatabases = sourceDatabases;
            _destinationDbConfig = destinationDbConfig;
            _dd03lDbConfig = dd03lDbConfig;
        }

        private void CreateIndexOnDd03L() {
            using (var db = ConnectionManager.CreateConnection(Dd03lDbConfig)) {
                var columns = new List<string> { "tabname" };
                if (!db.IsIndexExisting("dd03l", columns)) db.CreateIndex("dd03l", "_dd03l_tabname", columns);
            }
        }

        public void StartConsolidate() {
            using (var db = ConnectionManager.CreateConnection(DestinationDbConfig)) {
                db.Open();

                var filename = DestinationDbConfig.Hostname + "_" + DestinationDbConfig.DbName + ".csv";
                var nameToEntry = ReadFile(filename);

                CreateIndexOnDd03L();

                var cmd = new MySqlCommand("set net_write_timeout=99999; set net_read_timeout=99999", (MySqlConnection) db.Connection); // Setting timeout on mysqlServer
                cmd.ExecuteNonQuery();

                foreach (var sourceDatabase in SourceDatabases) {
                    foreach (var table in sourceDatabase.Tables) {
                        Entry entry = new Entry() { SourceDatabase = sourceDatabase.DbConfig.DbName, OriginalName = table.OriginalName, Name = table.Name, Start = DateTime.Now };
                        try {
                            Consolidate(db, table, nameToEntry, filename, sourceDatabase.DbConfig.DbName);
                        } catch (Exception ex) {
                            using (StreamWriter writer = new StreamWriter(filename, true)) {
                                entry.End = DateTime.Now;
                                entry.Exception = ex.Message;
                                writer.WriteLine(entry.ToCsvLine());
                            }

                        }
                    }
                }
            }
        }

        private void Consolidate(IDatabase db, Table table, Dictionary<string, Entry> nameToEntry, string filename, string sourceDatabaseName) {
            if (nameToEntry.ContainsKey(table.OriginalName))
                return;

            var start = DateTime.Now;

            var warning = String.Empty;
            if (table.UniqueKeyNamesAndLength.Count == 0) {
                warning = "Es wurden keine Primärspalten in der DD03L gefunden";
            } else {
                foreach (var uniqueKeyNameAndLength in table.UniqueKeyNamesAndLength)
                    if (!table.ColumnNames.Contains(uniqueKeyNameAndLength.Key))
                        warning += string.Format("Unique Key Spalte {0} existiert nicht.", uniqueKeyNameAndLength.Key);
            }
            if (!String.IsNullOrEmpty(warning))
                CreateTableDistinctMethod(db, table, sourceDatabaseName);
            else {
                CreateTableEfficientMethod(db, table, sourceDatabaseName);
            }

            long countDestinationTable = db.CountTable(table.Name);
            var entry = new Entry() {
                SourceDatabase = sourceDatabaseName, OriginalName = table.OriginalName, Name = table.Name,
                CountOriginal = table.Count, CountDestinationTable = countDestinationTable, Start = start, End = DateTime.Now, Warning = warning
            };

            using (StreamWriter writer = new StreamWriter(filename, true)) {
                writer.WriteLine(entry.ToCsvLine());
            }
            nameToEntry[entry.OriginalName] = entry;
        }

        private void CreateTableDistinctMethod(IDatabase db, Table table, string sourceDatabase) {
            var sql = string.Format("CREATE TABLE {0} LIKE {1}", db.Enquote(table.Name), db.Enquote(sourceDatabase, table.OriginalName));
            db.ExecuteNonQuery(sql);

            sql = string.Format("INSERT INTO {0} ({1}) SELECT DISTINCT {1} FROM {2}", db.Enquote(table.Name), String.Join(",", table.ColumnNames), db.Enquote(sourceDatabase, table.OriginalName));
            db.ExecuteNonQuery(sql);
        }

        private void CreateTableEfficientMethod(IDatabase db, Table table, string sourceDatabase) {
            var sql = String.Empty;
            if (!db.TableExists(table.Name)) {
                sql = string.Format("CREATE TABLE {0} LIKE {1}", db.Enquote(table.Name), db.Enquote(sourceDatabase, table.OriginalName));
                db.ExecuteNonQuery(sql);

                sql = string.Format("ALTER TABLE {0} ADD UNIQUE KEY({1})", db.Enquote(table.Name),
                                string.Join(",", from u in table.UniqueKeyNamesAndLength select String.Format("{0}({1})", u.Key, u.Value)));
                db.ExecuteNonQuery(sql);
            }

            //TODO: testen ob spalten von zieltabelle und quelltabelle gleich!

            sql = string.Format("INSERT IGNORE INTO {0} ({1}) SELECT {1} FROM {2}", db.Enquote(table.Name), String.Join(",", table.ColumnNames), db.Enquote(sourceDatabase, table.OriginalName));
            db.ExecuteNonQuery(sql);

        }

        private Dictionary<string, Entry> ReadFile(string filename) {
            var nameToEntry = new Dictionary<string, Entry>(StringComparer.InvariantCultureIgnoreCase);
            if (File.Exists(filename)) {
                using (var reader = new StreamReader(filename)) {
                    string line;
                    while ((line = reader.ReadLine()) != null) {
                        if (string.IsNullOrEmpty(line))
                            continue;
                        var entry = new Entry();
                        entry.Read(line);
                        nameToEntry[entry.OriginalName] = entry;
                    }
                }
            }
            return nameToEntry;
        }
    }
}
