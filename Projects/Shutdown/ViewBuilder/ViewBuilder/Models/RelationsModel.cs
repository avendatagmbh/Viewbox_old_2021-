using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using SystemDb;
using SystemDb.Factories;
using SystemDb.Internal;
using Utils;
using ViewBuilder.Structures;
using ViewBuilderBusiness.Structures;
using ViewBuilderBusiness.Structures.Config;

namespace ViewBuilder.Models {
    public class RelationsModel : NotifyPropertyChangedBase{
        #region Constructor
        public RelationsModel(ProfileConfig profile) {
            _profile = profile;
            _viewboxDb = profile.ViewboxDb;
            CsvPath = profile.ProjectDb.RelationsFilePath;

        }
        #endregion Constructor

        #region Properties
        readonly List<IRelationDatabaseObject> _relations = new List<IRelationDatabaseObject>();
        #region CsvFileOk
        private bool _csvFileOk;

        public bool CsvFileOk {
            get { return _csvFileOk; }
            set {
                if (_csvFileOk != value) {
                    _csvFileOk = value;
                    OnPropertyChanged("CsvFileOk");
                }
            }
        }
        #endregion CsvFileOk

        #region CsvPath
        private string _csvPath;

        public string CsvPath {
            get { return _csvPath; }
            set {
                if (_csvPath != value) {
                    _csvPath = value;
                    Task.Factory.StartNew(() => LoadPreview(value)).ContinueWith(ReportError, TaskContinuationOptions.OnlyOnFaulted);
                    OnPropertyChanged("CsvPath");
                }
            }
        }

        #endregion CsvPath

        #region DataPreview
        public Window Owner { get; set; }
        private DataTable _dataPreview;
        private SystemDb.SystemDb _viewboxDb;
        private ProfileConfig _profile;

        public DataTable DataPreview {
            get { return _dataPreview; }
            set {
                _dataPreview = value;
                OnPropertyChanged("DataPreview");
            }
        }
        #endregion DataPreview
        #endregion Properties

        #region Methods
        private void LoadPreview(string value) {
            _relations.Clear();
            if (File.Exists(value)) {
                CsvReader reader = new CsvReader(value) { HeadlineInFirstRow = true, Separator = ';' };
                DataTable dataPreview = reader.GetCsvData(0, Encoding.ASCII);
                DataColumn col = new DataColumn("Status");
                dataPreview.Columns.Add(col);
                CsvFileOk = true;
                foreach (DataRow row in dataPreview.Rows) {
                    ValidateAndAddRow(row, col);
                }
                DataPreview = dataPreview;
            }
        }

        private enum Columns {
            SourceView = 0,
            SourceColumn,
            TargetView,
            TargetColumn,
            RelationNumber
        };

        #region ValidateAndAddRow
        private bool ValidateAndAddRow(DataRow row, DataColumn col) {
            bool rowOk = true;
            IColumn sourceColumn = null, targetColumn = null;
            int relationId = 0;
            if (row.ItemArray.Length != 6) {
                row[col] =
                    "Es müssen genau 5 Einträge in einer Zeile stehen: Ausgangsview;Ausgangsspalte;Zielview;Zielspalte;Relationsnummer";
                rowOk = false;
            } else {
                ITableObject sourceTable = TableExists(row[(int) Columns.SourceView].ToString());
                ITableObject targetTable = TableExists(row[(int) Columns.TargetView].ToString());
                
                if (sourceTable == null) {
                    row[col] += "Ausgangsview ist nicht vorhanden. ";
                    rowOk = false;
                } else if ((sourceColumn = ColumnExists(sourceTable, row[(int) Columns.SourceColumn].ToString())) == null) {
                    row[col] += "Ausgangsspalte ist nicht vorhanden. ";
                    rowOk = false;
                }

                if (targetTable == null) {
                    row[col] += "Zielview ist nicht vorhanden. ";
                    rowOk = false;
                } else if ((targetColumn = ColumnExists(targetTable, row[(int) Columns.TargetColumn].ToString())) == null) {
                    row[col] += "Zielspalte ist nicht vorhanden. ";
                    rowOk = false;
                }
                if (!Int32.TryParse(row[(int) Columns.RelationNumber].ToString(), out relationId)) {
                    row[col] += "Die Relationsnummer muss eine Zahl sein.";
                    rowOk = false;
                }
                if (sourceColumn != null && targetColumn != null && (sourceColumn.DataType != targetColumn.DataType)) {
                    row[col] +=
                        string.Format(
                            "Warnung: Die Datentypen könnten Probleme machen, Spalte {0} hat Datentyp {1}, Spalte {2} hat Datentyp {3}",
                            sourceColumn.Name, sourceColumn.DataType.ToString(), targetColumn.Name,
                            targetColumn.DataType.ToString());
                }
            }
            if (!rowOk) {
                CsvFileOk = false;
            } else {
                _relations.Add(InternalObjectFactory.CreateRelation(sourceColumn.Id, targetColumn.Id, relationId)
                               );
            }
            return rowOk;
        }

        #endregion ValidateAndAddRow

        private IColumn ColumnExists(ITableObject sourceTable, string columnName) {
            var column = sourceTable.Columns.FirstOrDefault(c => c.Name.ToLower() == columnName.ToLower());
            return column;
        }

        private ITableObject TableExists(string tableName) {
            string database = _profile.DbConfig.DbName;
            var table =
                (from obj in _viewboxDb.Objects
                 where obj.TableName.ToLower() == tableName.ToLower() && obj.Database == database
                 select obj).
                    FirstOrDefault();
            return table;
        }
        #endregion Methods

        public void Save() {
            _profile.ProjectDb.RelationsFilePath = CsvPath;
            //DeleteDuplicateEntries();
        }

        private void ReportError(Task task) {
            string errors = "";
            if (task.Exception != null)
                foreach (var error in task.Exception.Flatten().InnerExceptions) {
                    errors += Environment.NewLine + error.Message;
                }
            Owner.Dispatcher.Invoke(
                new Action(() =>
                MessageBox.Show(Owner, "Fehler beim Lesen der Csvdatei: " + errors, "", MessageBoxButton.OK,
                                MessageBoxImage.Error)));
        }

        public void AddRelations() {
            if (!CsvFileOk) {
                if(MessageBox.Show(Owner, "Es sind noch Fehler in der CSV Datei vorhanden, sollen alle Relationen ohne Fehler eingespielt werden?", "",
                                MessageBoxButton.YesNo, MessageBoxImage.Error) == MessageBoxResult.No)
                    return;
            }
            try {
                RelationCreater relationCreater = new RelationCreater(_profile.ViewboxDb, _profile.DbConfig.DbName, _relations, _profile.DbConfig);
                ProgressDialogHelper progressDialog = new ProgressDialogHelper(Owner);
                progressDialog.StartJob(relationCreater.CheckIndizes);
                if (relationCreater.TotalIndexCount > 0) {
                    if (MessageBox.Show(Owner,
                        string.Format("Es müssen {0} Indizes auf {1} Tabellen erstellt werden, die zusammen insgesamt {2:0,0} Zeilen haben. Soll die Indexerstellung jetzt durchgeführt werden?",
                        relationCreater.TotalIndexCount, relationCreater.TotalTables, relationCreater.TotalRowCount.ToString(new NumberFormatInfo() { })), "", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes) {
                        progressDialog.StartJob(relationCreater.CreateIndices);
                    } else if (MessageBox.Show(Owner, "Relationen trotzdem einspielen?", "", MessageBoxButton.YesNo) == MessageBoxResult.No)
                        return;
                }

                _profile.ViewboxDb.DeleteRelations(_profile.DbConfig.DbName);
                _profile.ViewboxDb.AddRelations(_relations);
            } catch (Exception ex) {
                MessageBox.Show(Owner, "Es ist ein Fehler beim Einspielen der Relationen aufgetreten:" + Environment.NewLine + ex.Message, "",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            MessageBox.Show(Owner, "Relationen wurden erfolgreich eingespielt", "",
                            MessageBoxButton.OK, MessageBoxImage.Information);
            //List<Tuple<IColumn, IColumn, int>> relations = new List<Tuple<IColumn, IColumn, int>>();
            //foreach(var relation )
        }

        private void CreateIndices() {
            
        }

        public void UpdateFileContents() {
            Task.Factory.StartNew(() => LoadPreview(CsvPath)).ContinueWith(ReportError, TaskContinuationOptions.OnlyOnFaulted);
        }

        void DeleteDuplicateEntries() {
            var dict = new SortedDictionary<int, List<Tuple<string, string, string, string>>> ();
            foreach (DataRow row in DataPreview.Rows) {
                var relationId = Convert.ToInt32(row[4]);
                if(!dict.ContainsKey(relationId)) dict.Add(relationId, new List<Tuple<string, string, string, string>>());
                dict[relationId].Add(new Tuple<string, string, string, string>(row[0].ToString(), row[1].ToString(), row[2].ToString(), row[3].ToString()));
            }

            var relationList = new List<TempRelation>();
            foreach (var d in dict) {
                var r = new TempRelation(d.Value);
                if(!relationList.Contains(r)) relationList.Add(r);
            }

            System.Diagnostics.Debug.WriteLine(dict.Count);
            System.Diagnostics.Debug.WriteLine(relationList.Count);

            using (var writer = new StreamWriter(@"C:\Users\bes\Desktop\Relationen.csv")) {
                int i = 1;
                foreach(var relation in relationList) {
                    foreach(var r in relation.Relation) {
                        writer.WriteLine(String.Format("{0};{1};{2};{3};{4}", r.Item1, r.Item2, r.Item3, r.Item4, i));
                    }
                    i++;
                }
            }
        }

        class TempRelation {
            public List<Tuple<string, string, string, string>> Relation = new List<Tuple<string, string, string, string>>();

            public TempRelation(List<Tuple<string, string, string, string>> relation) { Relation = relation; }

            public override int GetHashCode() {
                var result = (from r in Relation select r.Item1 + "_" + r.Item2 + "_" + r.Item3 + "_" + r.Item4).ToList();
                result.OrderBy(r => r);

                var codeString = String.Empty;
                foreach (var r in result) codeString += r;

                return codeString.GetHashCode();
            }

            public override bool Equals(object obj) {
                if (!(obj is TempRelation)) throw new ArgumentException("obj is not of type TempRelation");

                return GetHashCode() == obj.GetHashCode();
            }
        }
    }
}
