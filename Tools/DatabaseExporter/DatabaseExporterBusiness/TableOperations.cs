using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using DbAccess;
using DbAccess.Structures;
using Utils;

namespace DatabaseExporterBusiness {
    [XmlRoot("Tables", Namespace = "", IsNullable = false)]
    public class TableOperations : NotifyPropertyChangedBase {
        #region Constructor
        public TableOperations() {
            Tables = new ObservableCollectionAsync<Table>();
            OutputDir = @"C:\Temp\exporttest";
        }
        #endregion Constructor

        #region Properties
        [XmlArray("Tables"),XmlArrayItem("Table", typeof(Table))]
        public ObservableCollectionAsync<Table> Tables { get; set; }

        #region OutputDir
        private string _outputDir;

        public string OutputDir {
            get { return _outputDir; }
            set {
                if (_outputDir != value) {
                    _outputDir = value;
                    OnPropertyChanged("OutputDir");
                }
            }
        }
        #endregion OutputDir
        #endregion Properties

        #region Methods
        public void FetchTables(DbConfig config) {
            var loadedTables = Tables.ToList();
            Tables.Clear();
            using (var conn = ConnectionManager.CreateConnection(config)) {
                conn.Open();
                foreach (var table in conn.GetTableList()) {
                    var existingTable = loadedTables.FirstOrDefault(t => t.Name == table);
                    if(existingTable != null)
                        Tables.Add(existingTable);
                    else {
                        Tables.Add(new Table(table));    
                    }
                    
                }
            }
        }
        #endregion Methods

        public void Start(ProgressCalculator progress, DbConfig dbConfig) {
            progress.SetWorkSteps(Tables.Count(table => table.IsChecked), false);
            Directory.CreateDirectory(OutputDir);
            using(IDatabase conn = ConnectionManager.CreateConnection(dbConfig)) {
                conn.Open();
                foreach (var table in Tables) {
                    if (!table.IsChecked)
                        continue;

                    progress.Title = "Bearbeite Tabelle " + table.Name;
                    CreateCsvFile(table, conn);
                    progress.StepDone();
                }
            }
            
        }

        private void CreateCsvFile(Table table, IDatabase conn) {
            var columns = conn.GetColumnInfos(table.Name);
            char seperator = ';';

            var columnString = string.Join(",", from column in columns select conn.Enquote(column.Name));
            string sql = string.Format("SELECT {0} FROM {1} {2}", columnString, conn.Enquote(table.Name), string.IsNullOrWhiteSpace(table.OrderBy) ? string.Empty : "ORDER BY " + table.OrderBy);
            using(StreamWriter writer = new StreamWriter(Path.Combine(OutputDir, table.CsvFile), false, Encoding.Default) ) {
                for (int i = 0; i < columns.Count; ++i) {
                    writer.Write(columns[i].Name);
                    WriteSeperatorIfNecessary(columns, i, writer);
                }
                writer.WriteLine();
                writer.WriteLine(table.SecondLine);
                using (var reader = conn.ExecuteReader(sql)) {
                    while (reader.Read()) {
                        for (int i = 0; i < columns.Count; ++i) {
                            if (reader.IsDBNull(i)) {
                                WriteSeperatorIfNecessary(columns, i, writer);
                            } else {
                                //if (columns[i].OriginalType.ToLower().Contains("text") || columns[i].OriginalType.ToLower().Contains("varchar")) {
                                //    writer.Write("\"" + reader[i].ToString().Replace("\"", "\"\"") + "\"");
                                //} else {

                                //Decimal replace , with .
                                if (columns[i].Type == DbColumnTypes.DbNumeric) {
                                    double value = Convert.ToDouble(reader[i].ToString());
                                    writer.Write(value.ToString().Replace(",", "."));
                                } else {
                                    if (reader[i].ToString() == "SFR")
                                        writer.Write("CHF");
                                    else if (reader[i].ToString() == "DEM")
                                        writer.Write("EUR");
                                    else {
                                        string value =
                                            reader[i].ToString().Replace(";", ",").Replace("\r", "").Trim();
                                        if (value.Contains("\n")) {
                                            writer.Write("\"" + value.Replace("\"", "\"\"") + "\"");
                                        }
                                        else
                                            writer.Write(value);
                                    }
                                }
                                
                                WriteSeperatorIfNecessary(columns, i, writer);
                            }
                        }
                        writer.WriteLine();
                    }
                }
            }
        }

        private void WriteSeperatorIfNecessary(List<DbColumnInfo> columns, int i, StreamWriter writer) {
            if(i != columns.Count - 1)
                writer.Write(";");
        }
    }
}
