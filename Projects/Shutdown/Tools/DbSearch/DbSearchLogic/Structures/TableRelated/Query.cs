// --------------------------------------------------------------------------------
// author: Benjamin Held
// since: 2011-09-12
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Xml;
using DbAccess;
using DbAccess.Structures;
using DbSearchDatabase.DistinctDb;
using DbSearchDatabase.Factories;
using DbSearchDatabase.Interfaces;
using DbSearchLogic.Config;
using DbSearchLogic.SearchCore.Result;
using DbSearchLogic.SearchCore.Structures;
using DbSearchLogic.SearchCore.Structures.Result;
using Utils;
using log4net;
using AV.Log;

namespace DbSearchLogic.Structures.TableRelated {
    public class Query : INotifyPropertyChanged, ICloneable {

        internal static ILog _log = LogHelper.GetLogger();

        #region Events
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) { if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName)); }
        #endregion Events

        public Query(Profile profile, IDbQuery dbQuery) {
            Profile = profile;
            DbQuery = dbQuery;
            Rows = new ObservableCollectionAsync<Row>();
            Columns = new ObservableCollectionAsync<Column>();
            ResultHistory = new ResultHistory(this);
            SearchParams = new ConfigSearchParams();
            SearchTableDecider = new SearchTableDecider();
            //UserColumnMappings = new UserColumnMappings();
        }

        #region Properties

        internal IDbQuery DbQuery { get; set; }
        public Profile Profile { get; private set; }
        public string Name { get { return DbQuery.Name; } }
        public long Count { get { return DbQuery.Count; } }
        public ObservableCollectionAsync<Row> Rows { get; private set; }
        public ObservableCollectionAsync<Column> Columns { get; private set; }
        public ConfigSearchParams SearchParams { get; private set; }
        private UserColumnMappings _userColumnMappings;
        public UserColumnMappings UserColumnMappings {
            get {
                if (_userColumnMappings == null)
                    _userColumnMappings = new UserColumnMappings(this);
                return _userColumnMappings;
            } 
            private set { _userColumnMappings = value; } }
        //public UserColumnMappings UserColumnMappings { get { return Profile.Queries.GetUserColumnMappings(this); } }
        public string TableName { get { return DbQuery.TableName; } }
        public List<int> DisplayIndexToIndex;
        public SearchTableDecider SearchTableDecider { get; private set; }
        private bool _loaded = false;

        #region ResultHistory
        public ResultHistory ResultHistory { get; private set; }
        #endregion ResultHistory
        #endregion

        #region EventHandler
        private void Row_ColumnValueChanged(object sender, Row.ColumnValueChangedEventArgs e) {
            SetStatusOfColumn(e.Column);
        }
        #endregion EventHandler

        #region Methods

        #region Load
        public void Load() {
            if (_loaded) return;
            if (!DbQuery.Load()) {
                LoadData(15, null);
            }
            AfterDbLoadedData();
            //Load user mappings
            LoadAdditionalInfos(this);
            
            _loaded = true;
        }

        private static void LoadAdditionalInfos(Query query) {
            if (!string.IsNullOrEmpty(query.DbQuery.AdditionalInfos)) {
                using (XmlReader reader = XmlReader.Create(new StringReader(query.DbQuery.AdditionalInfos))) {
                    

                    while (reader.Read()) {
                        if(reader.Name == "CMS")
                            query.UserColumnMappings.FromXml(reader);
                        if (reader.Name == "STD")
                            query.SearchTableDecider.FromXml(reader);
                        query.SearchParams.FromXml(reader);
                    }
                }
            }
        }
        #endregion Load

        #region DisplayIndicesValid
        private bool DisplayIndicesValid(List<IDbColumn> columns) {
            HashSet<int> indices = new HashSet<int>();
            for (int i = 0; i < columns.Count; ++i) indices.Add(i);
            foreach (var column in columns) indices.Remove(column.DisplayIndex);
            //If all indices from 0..column.Count-1 have been removed, then the display indices are valid
            return indices.Count == 0;
        }
        #endregion

        #region AfterDbLoadedData
        //Create all logic data structures from the database data structures
        private void AfterDbLoadedData() {
            Columns.Clear();
            DisplayIndexToIndex = new List<int>(new int[DbQuery.Columns.Count]);
            int i = 0;

            //Check if the display indices are valid
            bool displayIndicesValid = DisplayIndicesValid(DbQuery.Columns);

            foreach (var dbColumn in DbQuery.Columns) {
                int displayIndex = displayIndicesValid ? dbColumn.DisplayIndex : i;
                Columns.Add(new Column(this, dbColumn, displayIndex));
                DisplayIndexToIndex[displayIndex] = i;

                i++;
            }

            Rows.Clear();
            foreach (var dbRow in DbQuery.Rows) {
                Row row = new Row(dbRow, this);
                Rows.Add(row);
                
                row.ColumnValueChanged += Row_ColumnValueChanged;
            }

            UpdateColumnStatus();


        }

        #region UpdateColumnStatus
        private void UpdateColumnStatus() {
            for (int i = 0; i < Columns.Count; ++i)
                SetStatusOfColumn(i);
        }
        #endregion UpdateColumnStatus

        #endregion AfterDbLoadedData

        #region LoadData
        public void LoadData(int limit, List<int> rowNumbers = null) {
            if (_loaded) return;
            DbQuery.LoadData(limit, rowNumbers);
            AfterDbLoadedData();
            _loaded = true;
        }
        #endregion LoadData

        #region RemoveRow
        public void RemoveRow(Row row) {
            row.ColumnValueChanged -= Row_ColumnValueChanged;
            Rows.Remove(row);
            DbQuery.RemoveRow(row.DbRow);
        }
        #endregion RemoveRow

        public void AddRows(int count) {
            for (int i = 0; i < count; ++i) {
                IDbRow dbRow = DatabaseObjectFactory.CreateRow(DbQuery);
                Row row = new Row(dbRow, this);
                Rows.Add(row);
                DbQuery.AddRow(dbRow);
                row.ColumnValueChanged += Row_ColumnValueChanged;
            }
            UpdateColumnStatus();
        }

        #region OptimalRows
        //This algorithm is very simple and should be replaced. At the moment for each column the row numbers of 15 distinct values are extracted
        //and then these rows will be fetched
        //A better algorithm will try to find rows with as many distinct values as possible in order to reduce the total number of rows while
        //ensuring enough (minCountOfEachColumn) distinct values for each column
        public void OptimalRows(int minCountOfEachColumn) {
            DistinctDb distinctDb = new DistinctDb(Profile.DbProfile.GetProfileConfig());
            var tables = distinctDb.Tables;
            var columnDict = tables[DbQuery.TableName];
            HashSet<int> rowNumbers = new HashSet<int>();

            DbConfig config = (DbConfig) Profile.DbProfile.GetProfileConfig().Clone();
            config.DbName += "_idx";
            foreach (var column in columnDict.Columns.Values) {
                using(IDatabase conn = ConnectionManager.CreateConnection(config)) {
                    conn.Open();
                    conn.SetHighTimeout();
                    var valuesToRowNos = column.GetValueToRows(minCountOfEachColumn, conn);
                    foreach (var rowNosForValue in valuesToRowNos.Values) {
                        if (rowNosForValue.Count > 0) rowNumbers.Add(rowNosForValue[0]);
                    }
                }
            }
            _loaded = false;
            DbQuery.DeleteColumns();
            LoadData(0, rowNumbers.ToList());
        }
        #endregion OptimalRows

        #region Save
        public void Save() {
            UpdateXmlRepresentation();
            DbQuery.Save();
        }

        private void UpdateXmlRepresentation() {
            //Create the Usermappings and search params xml representation
            StringWriter stringWriter = new StringWriter();
            using (XmlWriter writer = XmlWriter.Create(stringWriter)) {
                writer.WriteStartDocument();
                writer.WriteStartElement("AddInfo");
                UserColumnMappings.ToXml(writer);
                SearchTableDecider.ToXml(writer);
                SearchParams.ToXml(writer);
                writer.WriteEndElement();
            }
            DbQuery.AdditionalInfos = stringWriter.ToString();
            foreach (var column in Columns) {
                column.SearchParams.ToDbSearchParams(column.DbColumn.DbConfigSearchParams);
                column.DbColumn.Rules = column.RuleSet.ToXml();
            }
        }
        #endregion Save

        #region SetStatusOfColumn
        //Checks for each entry of a column if the entry is null or empty or a duplicate of another entry in this column
        //if so it sets the status appropriately
        private void SetStatusOfColumn(int column) {
            //First set all statuses to used
            for (int i = 0; i < Rows.Count; ++i)
                Rows[i].RowEntries[column].Status = RowEntryStatus.Used;
            
            for (int i = Rows.Count - 1; i >= 0 ; --i) {
                if (Rows[i].RowEntries[column].IsDbNull() || string.IsNullOrEmpty(Rows[i].RowEntries[column].RuleDisplayString))
                    Rows[i].RowEntries[column].Status = RowEntryStatus.NullOrEmpty;

                if (Rows[i].RowEntries[column].Status != RowEntryStatus.Used) continue;

                for (int j = i - 1; j >= 0; --j) {
                    if (Rows[i].RowEntries[column].RuleDisplayString == Rows[j].RowEntries[column].RuleDisplayString) {
                        Rows[i].RowEntries[column].Status = RowEntryStatus.Duplicate;
                        break;
                    }
                }
            }
        }
        #endregion SetStatusOfColumn

        #region IndexOfColumn
        public int IndexOfColumn(Column column) {
            for (int i = 0; i < Columns.Count; ++i)
                if (column.Name == Columns[i].Name) return i;

            throw new ArgumentException("Konnte die Spalte " + column.Name + " nicht finden");
        }
        #endregion IndexOfColumn

        #region IdToRowEntry
        public Dictionary<int, RowEntry> IdToRowEntry() {
            Dictionary<int, RowEntry> result = new Dictionary<int, RowEntry>(Columns.Count*Rows.Count);
            for (int i = 0; i < Columns.Count; ++i)
                for (int j = 0; j < Rows.Count; ++j)
                    result[i*Rows.Count + j] = Rows[j].RowEntries[i];
            return result;
        }
        #endregion IdToRowEntry

        #region RowEntryToId
        public Dictionary<RowEntry, int> RowEntryToId() {
            Dictionary<RowEntry, int> result = new Dictionary<RowEntry, int>(Columns.Count * Rows.Count);
            for (int i = 0; i < Columns.Count; ++i)
                for (int j = 0; j < Rows.Count; ++j)
                    result[Rows[j].RowEntries[i]] = i * Rows.Count + j;
            return result;
        }
        #endregion RowEntryToId

        #region RowIdToColumn
        public Column RowIdToColumn(int rowId) {
            if(Rows.Count == 0) return null;
            return Columns[rowId/Rows.Count];
        }
        #endregion RowIdToColumn

        #region Serialize As Xml
        public string ToXml() {
            StringWriter sw = new StringWriter();
            UpdateXmlRepresentation();
            using (XmlWriter xmlWriter = XmlWriter.Create(sw)) {
                xmlWriter.WriteStartDocument();
                xmlWriter.WriteStartElement("Query");
                DbQuery.ToXml(xmlWriter);
                xmlWriter.WriteEndElement();
            }

            return sw.ToString();
        }

        internal static Query FromXml(Profile profile, string xml) {
            using (XmlReader reader = XmlReader.Create(new StringReader(xml))) {
                while (reader.Read()) {
                    if (reader.Name == "Query") {
                        Query result = new Query(profile,
                                                 DatabaseObjectFactory.DbQueryFromXml(reader, profile != null ? profile.DbProfile : null));
                        //Restore search parameter
                        foreach(var column in result.Columns)
                            column.SearchParams.FromDbSearchParams(column.DbColumn.DbConfigSearchParams);
                        result.SearchParams.FromDbSearchParams(result.DbQuery.DbConfigSearchParams);
                        LoadAdditionalInfos(result);
                        result.AfterDbLoadedData();
                        return result;
                    }
                }
            }
            _log.Log(LogLevelEnum.Error, "Konnte die Abfrage nicht aus dem Xml String erzeugen.");
            return null;
        }

        public object Clone() {
            return FromXml(Profile, ToXml());
        }
        #endregion Serialize As Xml


        #region SearchValuesForColumn
        public int SearchValuesForColumn(Column column) {
            int colIndex = IndexOfColumn(column);
            return Rows.Count((row) => row.RowEntries[colIndex].Status == RowEntryStatus.Used);
        }

        #endregion SearchValuesForColumn

        #region AddColumns
        public void AddColumns(int count) {
            for (int i = 0; i < count; ++i) {
                Columns.Add(CreateColumn());
            }
            foreach (var row in Rows) {
                row.AddColumns(count);
            }

            UpdateColumnStatus();
        }

        #endregion AddColumns

        #region CreateColumn
        private Column CreateColumn() {
            IDbColumn dbColumn = DatabaseObjectFactory.CreateColumn(DbQuery);
            dbColumn.IsUserDefined = true;
            DbQuery.AddColumn(dbColumn);
            //Choose displayIndex == index
            int displayIndex = DisplayIndexToIndex.Count;
            DisplayIndexToIndex.Add(displayIndex);

            Column newColumn = new Column(this, dbColumn, displayIndex) {Name = "Spalte " + (Columns.Count + 1)};
            //Columns.Add(newColumn);
            return newColumn;
        }

        #endregion CreateColumn

        #region DeleteColumns
        public void DeleteColumns(int count) {
            int deleted = 0;
            for (int i = Columns.Count - 1; i >= 0; --i) {
                if (Columns[i].IsUserDefined) {
                    DbQuery.DeleteColumn(i);
                    foreach (var row in Rows)
                        row.DeleteColumn(i);

                    //Need to adjust display indices array (decrement all display indices which have a larger display index then the column which is deleted)
                    for (int j = 0; j < DisplayIndexToIndex.Count; ++j)
                        if (i != j && Columns[i].DisplayIndex < Columns[j].DisplayIndex) {
                            //DisplayIndexToIndex[j]--;
                            Columns[j].DisplayIndex--;
                        }
                    //Adjust index in DisplayIndexToIndex
                    for (int j = 0; j < DisplayIndexToIndex.Count; ++j) {
                        if (i < DisplayIndexToIndex[j])
                            DisplayIndexToIndex[j]--;
                    }
                    DisplayIndexToIndex.RemoveAt(Columns[i].DisplayIndex);

                    Columns.RemoveAt(i);
                    if (++deleted == count)
                        return;
                }
            }
        }

        #endregion DeleteColumns

        #region DuplicateColumn
        public void DuplicateColumn(Column column) {
            Column newColumn = CreateColumn();
            newColumn.Name = column.Name + "_Kopie";
            newColumn.OriginalColumnName = column.OriginalColumnName;
            Columns.Add(newColumn);
            //Copy rules from column
            newColumn.RuleSet.FromXml(column.RuleSet.ToXml());
            int columnIndex = IndexOfColumn(column);
            //Copy RowEntries:
            foreach (var row in Rows) {
                row.AddColumns(1);
            }
            foreach (var row in Rows) {
                row.RowEntries[Columns.Count - 1].SetValue(row.RowEntries[columnIndex].DbRowEntry.Value);
                row.RowEntries[Columns.Count - 1].EditedValue = row.RowEntries[columnIndex].EditedValue;
                row.RowEntries[Columns.Count - 1].UpdateRuleDisplayString();
            }
            SetStatusOfColumn(Columns.Count - 1);
        }

        #endregion DuplicateColumn

        #endregion Methods
    }
}
