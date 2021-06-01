using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using DbAccess;
using DbAccess.Structures;
using Itenso.Rtf.Converter.Text;
using Itenso.Rtf.Parser;
using Itenso.Rtf.Support;
using Utils;

namespace RtfConverterBusiness {
    public class RtfConverter {
        #region Constructor
        public RtfConverter() {
            Tables = new ObservableCollectionAsync<Table>();
        }
        #endregion Constructor

        #region Properties
        [XmlArray("Tables"), XmlArrayItem("Table", typeof(Table))]
        public ObservableCollectionAsync<Table> Tables { get; set; }
        #endregion Properties

        #region Methods
        public void StartConversion(ProgressCalculator progress, DbConfig dbConfig, string targetDbName) {
            progress.SetWorkSteps(Tables.Count(table => table.IsChecked), false);

            DbConfig configWithoutDb = (DbConfig)dbConfig.Clone();
            string sourceDbName = configWithoutDb.DbName;
            configWithoutDb.DbName = "";
            
            using (IDatabase conn = ConnectionManager.CreateConnection(dbConfig)) {
                conn.Open();

                conn.CreateDatabaseIfNotExists(targetDbName);

                foreach (var table in Tables) {
                    if (!table.IsChecked)
                        continue;

                    progress.Title = "Bearbeite Tabelle " + table.Name;
                    ConvertTableNew(table, conn, sourceDbName, targetDbName, progress);
                    progress.StepDone();
                }
            }
            
        }

        private void ConvertTableNew(Table table, IDatabase conn, string sourceDbName, string targetDbName, ProgressCalculator progress) {
            try {
                conn.DropTable(targetDbName, table.TargetName);
            } catch (Exception) { }

            string sql;

            //Check for identity column
            string identityColumn = null;
            var columnsInTable = conn.GetColumnInfos(sourceDbName, table.Name);
            foreach (var column in columnsInTable) {
                if (column.IsIdentity) {
                    identityColumn = column.Name;
                }
            }
            if (string.IsNullOrEmpty(identityColumn)) {
                sql = string.Format("ALTER TABLE {0} ADD _row_no_ INT IDENTITY",
                                    conn.Enquote(sourceDbName, table.Name));
                conn.ExecuteNonQuery(sql); //ca 1 min for 900.000 datensätze
            }
            //else {
            //    sql = string.Format("ALTER TABLE {0} ADD _row_no_ INT",
            //                        conn.Enquote(sourceDbName, table.Name));
            //    conn.ExecuteNonQuery(sql);
            //    conn.ExecuteNonQuery(string.Format("UPDATE {0} SET _row_no_ = {1}",
            //                                       conn.Enquote(sourceDbName, table.Name),
            //                                       conn.Enquote(identityColumn)));
            //}

            //erstelle tempTable mit rtf in Klartext
            var tempTableName = "temp_" + table.Name;

            try {
                conn.DropTable(sourceDbName, tempTableName);
            } catch (Exception) { }

            sql = string.Format("SELECT {2}, _row_no_ INTO {0} FROM {1} WHERE 1=2", conn.Enquote(sourceDbName, tempTableName), conn.Enquote(sourceDbName, table.Name), string.Join(",", from column in table.Columns select conn.Enquote(column)));
            conn.ExecuteNonQuery(sql);

            var columns = conn.GetColumnNames(sourceDbName, table.Name);
            sql = string.Format("SELECT {1}, _row_no_ FROM {0}", conn.Enquote(sourceDbName, table.Name), string.Join(",", from column in table.Columns select conn.Enquote(column)));

            HashSet<string> columnsToChange = new HashSet<string>(table.Columns);

            var textConvertSettings = new RtfTextConvertSettings();
            textConvertSettings.IsShowHiddenText = true;
            var textConverter = new RtfTextConverter(textConvertSettings);

            long totalCount = conn.CountTable(sourceDbName, table.Name);
            int inserts = 0;
            const int insertAfter = 5000;
            List<DbColumnValues> values = new List<DbColumnValues>();

            using (var reader = conn.ExecuteReader(sql)) {
                while (reader.Read()) {
                    DbColumnValues value = new DbColumnValues();
                    foreach (var column in table.Columns) {
                        value[column] = reader[column];

                        if (reader[column] != null && columnsToChange.Contains(column.ToLower())) {
                            if (!String.IsNullOrEmpty(reader[column].ToString())) {
                                try {
                                    var structureBuilder = new RtfParserListenerStructureBuilder();
                                    var parser = new RtfParser();
                                    parser.AddParserListener(structureBuilder);
                                    parser.Parse(new RtfSource(reader[column].ToString()));

                                    RtfInterpreterTool.Interpret(structureBuilder.StructureRoot, null, textConverter,
                                                                 null);

                                    value[column] = textConverter.PlainText;
                                } catch (Exception ex) {

                                }
                            }
                        }
                    }

                    values.Add(value);

                    if (values.Count > insertAfter) {
                        inserts++;
                        progress.Title = "Bearbeite Tabelle " + table.Name + " (" + (inserts * insertAfter) + " / " + totalCount + ")";
                        using (var conn2 = ConnectionManager.CreateConnection(conn.DbConfig)) {
                            conn2.Open();
                            conn2.InsertInto(sourceDbName, tempTableName, values);
                        }
                        values.Clear();
                    }
                }
            }

            if (values.Count > 0)
                conn.InsertInto(sourceDbName, tempTableName, values);
            //ca 15 min für 900.000 datensätze

            //join: sourceTable + tempTable zu targetTable
            progress.Title = "Bearbeite Tabelle " + table.Name + ": Zieltabelle " + table.TargetName + " erstellen";

            sql = string.Format("SELECT {0}, {1} INTO {2} FROM {3} s JOIN {4} t ON s._row_no_ = t._row_no_",
                string.Join(",", from column in columns where table.Columns.All(c => c.ToLower() != column.ToLower()) select "s." + conn.Enquote(column)),//alle ohne rtf aus source
                string.Join(",", from column in table.Columns select "t." + conn.Enquote(column)),//alle rtf ohne _row_no_ aus temp
                conn.Enquote(targetDbName, table.TargetName), 
                conn.Enquote(sourceDbName, table.Name), 
                conn.Enquote(sourceDbName, tempTableName));

            conn.ExecuteNonQuery(sql);
            //ca 5 min für 900.000 Datensätze

            //drop tempTable
            try {
                conn.DropTable(sourceDbName, tempTableName);
            } catch (Exception) { }

            table.IsChecked = false;
        }

        private void ConvertTable(Table table, IDatabase conn, string sourceDbName, string targetDbName, ProgressCalculator progress) {
            try {
                conn.DropTable(targetDbName, table.TargetName);
            }catch(Exception){}

            string sql = string.Format("SELECT * INTO {0} FROM {1} WHERE 1=2", conn.Enquote(targetDbName, table.TargetName), conn.Enquote(sourceDbName, table.Name));
            //string sql = string.Format("SELECT * INTO {0} FROM {1}", conn.Enquote(targetDbName, table.TargetName), conn.Enquote(sourceDbName, table.Name));
            conn.ExecuteNonQuery(sql);
            

            //Check for identity column
            string identityColumn = null;
            var columnsInTable = conn.GetColumnInfos(sourceDbName, table.Name);
            foreach (var column in columnsInTable) {
                if (column.IsIdentity) {
                    identityColumn = column.Name;
                }
            }
            if (!string.IsNullOrEmpty(identityColumn)) {
                //sql = string.Format("ALTER TABLE {0} ADD _row_no_ INT IDENTITY",
                //                    conn.Enquote(sourceDbName, table.Name));
                //conn.ExecuteNonQuery(sql);
                conn.ExecuteNonQuery(String.Format("SET IDENTITY_INSERT {0} ON", conn.Enquote(targetDbName, table.TargetName)));
            }
            //else {
            //    sql = string.Format("ALTER TABLE {0} ADD _row_no_ INT",
            //                        conn.Enquote(sourceDbName, table.Name));
            //    conn.ExecuteNonQuery(sql);
            //    conn.ExecuteNonQuery(string.Format("UPDATE {0} SET _row_no_ = {1}",
            //                                       conn.Enquote(sourceDbName, table.Name),
            //                                       conn.Enquote(identityColumn)));
            //}
            //sql = string.Format("ALTER TABLE {0} ADD _row_no_ INT IDENTITY",
            //                    conn.Enquote(targetDbName, table.TargetName));
            

            //sql = string.Format("SELECT {1},[_row_no_] FROM {0}", conn.Enquote(targetDbName, table.TargetName), string.Join(",",from column in table.Columns select conn.Enquote(column)));
            
            var columns = conn.GetColumnNames(sourceDbName, table.Name);
            sql = string.Format("SELECT {1} FROM {0}", conn.Enquote(sourceDbName, table.Name), string.Join(",", from column in columns select conn.Enquote(column)));
            
            HashSet<string> columnsToChange = new HashSet<string>(table.Columns);

            var textConvertSettings = new RtfTextConvertSettings();
            textConvertSettings.IsShowHiddenText = true;
            var textConverter = new RtfTextConverter(textConvertSettings);

            long totalCount = conn.CountTable(sourceDbName, table.Name);
            int inserts = 0;
            const int insertAfter = 5000;
            List<DbColumnValues> values = new List<DbColumnValues>();
            //List<string> updates = new List<string>();

            using (var reader = conn.ExecuteReader(sql)) {
                while (reader.Read()) {
                    //Dictionary<string,string> columnToNewText = new Dictionary<string, string>();
                    //foreach (var column in columnsToChange) {
                    //    if (reader[column] != null && columnsToChange.Contains(column.ToLower())) {
                    //        if (!String.IsNullOrEmpty(reader[column].ToString())) {
                    //            try {
                    //                var structureBuilder = new RtfParserListenerStructureBuilder();
                    //                var parser = new RtfParser();
                    //                parser.AddParserListener(structureBuilder);
                    //                parser.Parse(new RtfSource(reader[column].ToString()));

                    //                RtfInterpreterTool.Interpret(structureBuilder.StructureRoot, null, textConverter,
                    //                                             null);

                    //                if (textConverter.PlainText != reader[column].ToString())
                    //                    columnToNewText[column] = textConverter.PlainText;
                    //            } catch (Exception ex) {

                    //            }
                    //        }
                    //    }
                    //}
                    //if (columnToNewText.Count != 0) {
                    //    string setSql = string.Join(",",from pair in columnToNewText
                    //                    select
                    //                        string.Format("{0} = {1}", conn.Enquote(pair.Key),
                    //                                      conn.GetSqlString(pair.Value)));
                    //    updates.Add(string.Format("UPDATE {0} SET {1} WHERE {2};", conn.Enquote(targetDbName, table.TargetName), setSql, "_row_no_ = " + reader["_row_no_"]));
                    //}

                    //progress.Title = "Bearbeite Tabelle " + table.Name + " (" + reader["_row_no_"].ToString() + " / " + totalCount + ")";
                    //if (updates.Count > insertAfter) {
                    //    using (var conn2 = ConnectionManager.CreateConnection(conn.DbConfig)) {
                    //        conn2.Open();
                    //        conn2.ExecuteNonQuery(string.Join(";", updates));
                    //    }
                    //    updates.Clear();
                    //}
                    DbColumnValues value = new DbColumnValues();
                    foreach (var column in columns) {
                        value[column] = reader[column];

                        if (reader[column] != null && columnsToChange.Contains(column.ToLower())) {
                            if (!String.IsNullOrEmpty(reader[column].ToString())) {
                                try {
                                    var structureBuilder = new RtfParserListenerStructureBuilder();
                                    var parser = new RtfParser();
                                    parser.AddParserListener(structureBuilder);
                                    parser.Parse(new RtfSource(reader[column].ToString()));

                                    RtfInterpreterTool.Interpret(structureBuilder.StructureRoot, null, textConverter,
                                                                 null);

                                    value[column] = textConverter.PlainText;
                                } catch (Exception ex) {

                                }
                            }
                        }
                    }

                    values.Add(value);

                    if (values.Count > insertAfter) {
                        inserts++;
                        progress.Title = "Bearbeite Tabelle " + table.Name + " (" + (inserts * insertAfter) + " / " + totalCount + ")";
                        using (var conn2 = ConnectionManager.CreateConnection(conn.DbConfig)) {
                            conn2.Open();
                            conn2.InsertInto(targetDbName, table.TargetName, values);
                        }
                        values.Clear();
                    }
                }
            }

            //if (updates.Count > 0) {
            //    conn.ExecuteNonQuery(string.Join(";", updates));
            //}


            if (values.Count > 0) 
                conn.InsertInto(targetDbName, table.TargetName, values);

            table.IsChecked = false;
        }

        public void FetchTables(DbConfig config) {
            var loadedTables = Tables.ToList();
            Tables.Clear();
            List<Table> tables = new List<Table>();
            using (var conn = ConnectionManager.CreateConnection(config)) {
                conn.Open();
                foreach (var table in conn.GetTableList()) {
                    var existingTable = loadedTables.FirstOrDefault(t => t.Name == table);
                    if (existingTable != null)
                        tables.Add(existingTable);
                    else {
                        tables.Add(new Table(table));
                    }

                }
            }
            tables.Sort((table, table1) => table.Name.CompareTo(table1.Name));
            foreach(var table in tables)
                Tables.Add(table);
        }
        #endregion Methods
    }
}
