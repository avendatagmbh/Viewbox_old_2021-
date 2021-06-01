// --------------------------------------------------------------------------------
// author: Benjamin Held
// since: 2011-10-04
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using AV.Log;
using AvdCommon.DataGridHelper.Interfaces;
using DbAccess;
using DbAccess.Structures;
using DbSearch.Manager;
using DbSearch.Structures.Results;
using DbSearchDatabase.DistinctDb;
using DbSearchLogic.SearchCore.Result;
using DbSearchLogic.SearchCore.Structures.Result;
using DbSearchLogic.SearchCore.Structures.Result.DataMapper;
using DbSearchLogic.Structures.TableRelated;
using Utils;
using log4net;
using DataColumn = AvdCommon.DataGridHelper.DataColumn;
using DataRow = AvdCommon.DataGridHelper.DataRow;
using DataTable = AvdCommon.DataGridHelper.DataTable;

namespace DbSearch.Models.Result {

    public class HitsInDatabaseModel : NotifyPropertyChangedBase {

        internal ILog _log = LogHelper.GetLogger();

        class RowIdWithInfo {
            public int RowId { get; set; }
            public SearchValueResult SearchValueResult { get; set; }
            public ColumnResult ColumnResult { get; set; }
            public ColumnHitInfo ColumnHitInfo { get; set; }
            
        }
        class HitInfoAndColumnResult {
            public ColumnHitInfo ColumnHitInfo { get; set; }
            public ColumnResult ColumnResult { get; set; }
        }

        public HitsInDatabaseModel(Dictionary<ColumnResult,List<ColumnHitInfo>> resultToHitInfo, Query query) {
            _resultToHitInfo = resultToHitInfo;
            Loaded = false;
        }


        #region Properties
        private Dictionary<ColumnResult, List<ColumnHitInfo>> _resultToHitInfo;
        public DataTable Data { get; private set; }
        public DataTable Values { get; private set; }
        private bool _loaded = false;
        public bool Loaded { 
            get { return _loaded; } 
            set {
                if (_loaded != value) {
                    _loaded = value;
                    OnPropertyChanged("Loaded");
                }
            }
        }

        private string _error;
        public string Error {
            get { return _error; }
            set {
                if (_error != value) {
                    _error = value;
                    OnPropertyChanged("Error");
                }
            }
        }

        
        #endregion Properties

        #region Methods
        public void LoadData(Query query) {
            if (_resultToHitInfo.Count == 0) return;
            try {
                ColorManager<ColumnResult>.ClearColors();

                DbConfig configDistinctDb = (DbConfig)query.Profile.DbProfile.DbConfigView.Clone();
                configDistinctDb.DbName = query.Profile.DbProfile.DbConfigView.DbName + "_idx";

                using (IDatabase conn = ConnectionManager.CreateConnection(query.Profile.DbConfigView),
                                 connDistinctDb = ConnectionManager.CreateConnection(configDistinctDb)) {
                    conn.Open();
                    conn.SetHighTimeout();

                    connDistinctDb.Open();
                    connDistinctDb.SetHighTimeout();



                    //Project to one-dimensional list of columnHitInfos (with stored columnresult)
                    var columnHitInfos = (from pair in _resultToHitInfo
                                          from hitInfo in pair.Value
                                          select new HitInfoAndColumnResult{ColumnHitInfo = hitInfo, ColumnResult = pair.Key}).ToList();

                    Data = new DataTable();
                    Values = new DataTable(){ CreateRowEntryFunc = obj => new ColoredRowEntry(obj)};

                    //To identify the correct column indices later on, this map is constructed
                    Dictionary<ColumnResult, int> columnResultToColumnIndex = new Dictionary<ColumnResult, int>();
                    int index = 0;
                    foreach (var hitInfo in columnHitInfos) {

                        //Add a column for each ColumnResult
                        if (!columnResultToColumnIndex.ContainsKey(hitInfo.ColumnResult)) {
                            Values.AddColumn(hitInfo.ColumnResult.Name);
                            DataColumn searchValueColumn = new DataColumn("Suchwert aus " + hitInfo.ColumnResult.Name);
                            columnResultToColumnIndex[hitInfo.ColumnResult] = index++;
                            Data.Columns.Add(searchValueColumn);
                        }
                        Values.AddColumn("Gefunden in " + hitInfo.ColumnHitInfo.ColumnName);
                    }

                    //Add table columns
                    List<DataColumn> tableColumns = new List<DataColumn>();
                    //using (IDataReader oReader = conn.ExecuteReader(
                    //    "SELECT * FROM " + conn.Enquote(tableName), System.Data.CommandBehavior.SchemaOnly)) {

                    //    // Iterate through all columns
                    //    for (int i = 0; i < oReader.FieldCount; i++) {
                    string tableName = string.Empty;
                    //As all results have the same tablename, pick one of them
                    foreach (var pair in _resultToHitInfo)
                        tableName = pair.Value[0].TableInfo.Name;
                    DistinctDb distinctDb = new DistinctDb(query.Profile.DbProfile.DbConfigView, tableName);

                    foreach (KeyValuePair<string, DistinctTableInfo> keyValuePair in distinctDb.Tables) {
                        if (keyValuePair.Value == null)
                            MessageBox.Show(string.Format("Distinct table data [{0}] not found in [{1}]", keyValuePair.Key, query.Profile.DbProfile.DbConfigView.DbName + "_idx"), "Distinct table not found", MessageBoxButton.OK, MessageBoxImage.Error);    
                    }

                    foreach (var columnName in conn.GetColumnNames(tableName)) {
                            DataColumn column = new DataColumn(conn.ReplaceSpecialChars(columnName));
                            tableColumns.Add(column);
                            Data.Columns.Add(column);
                    //    }
                    }
                    
                    //First make sure that the hits are loaded
                    foreach (var hitInfo in columnHitInfos)
                        SearchValueResultMapper.LoadAllHits(hitInfo.ColumnHitInfo.Results, query);

                    //Create new list with row ids, the corresponding SearchValueResult and ColumnResult
                    var rowIdsWithInfo = new List<RowIdWithInfo>();
                    Dictionary<int,HashSet<ColumnResult>> rowIdToColumnResults = new Dictionary<int, HashSet<ColumnResult>>();
                    //distinctTableInfo = distinctDb.Tables[tableName];
                    DistinctTableInfo distinctTableInfo = distinctDb.Tables[tableName];



                    GetRowIdsWithInfo(distinctTableInfo, connDistinctDb, rowIdsWithInfo, columnHitInfos, rowIdToColumnResults);

                    //Create a list where each row Id is only contained once (this list will be used to query the database for these row ids)
                    var distinctRowIds = (from rowIdWithInfo in rowIdsWithInfo
                                         where rowIdToColumnResults[rowIdWithInfo.RowId].Count == _resultToHitInfo.Count
                                          select rowIdWithInfo).Distinct(rowIdsWithInfo.CreateEqualityComparerForElements((x) => x.RowId)).ToList();

                    //Show only 10000 entries
                    if(distinctRowIds.Count > 10000) 
                        distinctRowIds.RemoveRange(10000,distinctRowIds.Count-10000);
                    //var distinctRowIds = rowIdsWithInfo.Distinct(rowIdsWithInfo.CreateEqualityComparerForElements((x) => x.RowId)).ToList();
                    
                    Dictionary<int,List<RowIdWithInfo>> rowIdToSearchValueResult = new Dictionary<int, List<RowIdWithInfo>>();

                    //Possible out of memory exception
                    //Add all row ids with there specific infos into a dictionary (with the same information)
                    foreach (var rowIdWithInfo in rowIdsWithInfo) {
                        if (!rowIdToSearchValueResult.ContainsKey(rowIdWithInfo.RowId)) 
                            rowIdToSearchValueResult[rowIdWithInfo.RowId] = new List<RowIdWithInfo> { new RowIdWithInfo { SearchValueResult = rowIdWithInfo.SearchValueResult, ColumnResult = rowIdWithInfo.ColumnResult, ColumnHitInfo = rowIdWithInfo.ColumnHitInfo } };
                        else 
                            rowIdToSearchValueResult[rowIdWithInfo.RowId].Add(new RowIdWithInfo {SearchValueResult = rowIdWithInfo.SearchValueResult, ColumnResult = rowIdWithInfo.ColumnResult, ColumnHitInfo = rowIdWithInfo.ColumnHitInfo });
                    }

                    if (distinctRowIds.Count != 0) {
                        //Create comma seperated row id string
                        StringBuilder rowIdString = new StringBuilder();
                        foreach (var distinctRowId in distinctRowIds) {
                            rowIdString.Append(distinctRowId.RowId).Append(",");
                        }
                        //Remove last comma
                        rowIdString.Remove(rowIdString.Length - 1,1);
                        using (
                            IDataReader reader =
                                conn.ExecuteReader("SELECT * FROM " + conn.Enquote(tableName) +
                                                   " WHERE _row_no_ in (" +
                                                   rowIdString + ");")) {
                            CreateRows(tableColumns, reader, columnResultToColumnIndex, rowIdToSearchValueResult);
                        }
                    }
                }
                ColorManager<Brush>.ClearColors();
 
                OnPropertyChanged("Values");
                OnPropertyChanged("Data");
                Loaded = true;
            } catch (Exception ex) {
                Error = "Es ist ein Fehler beim Abrufen der Daten aufgetreten: " + ex.Message;
                Loaded = false;
                _log.ErrorFormatWithCheck("Error while loading data: {0}", ex.Message, ex);
            }
        }

        private void CreateRows(List<DataColumn> tableColumns, IDataReader reader, Dictionary<ColumnResult, int> columnResultToColumnIndex,
                                Dictionary<int, List<RowIdWithInfo>> rowIdToSearchValueResult) {
            //For each read line, create a row
            while (reader.Read()) {
                DataRow row = Data.CreateRow();

                HashSet<string> alreadyAddedSearchValues = new HashSet<string>();
                //These are the first columnResultToColumnIndex.Count columns which contain the search values
                foreach (var myType in rowIdToSearchValueResult[Convert.ToInt32(reader["_row_no_"])]) {
                    if (string.IsNullOrEmpty(row[columnResultToColumnIndex[myType.ColumnResult]].DisplayString)) {
                        row[columnResultToColumnIndex[myType.ColumnResult]] =
                            new ColoredRowEntry(myType.SearchValueResult.SearchValue.String,
                                                ColorManager<ColumnResult>.GetBrush(myType.ColumnResult, 0.5f));
                        alreadyAddedSearchValues.Add(myType.SearchValueResult.SearchValue.String);
                    }
                    else if (!alreadyAddedSearchValues.Contains(myType.SearchValueResult.SearchValue.String)) {
                        row[columnResultToColumnIndex[myType.ColumnResult]] =
                            new ColoredRowEntry(
                                row[columnResultToColumnIndex[myType.ColumnResult]].DisplayString + " und " +
                                myType.SearchValueResult.SearchValue.String,
                                ColorManager<ColumnResult>.GetBrush(myType.ColumnResult, 0.5f));
                        alreadyAddedSearchValues.Add(myType.SearchValueResult.SearchValue.String);
                    }
                }
                foreach (var rowEntryIndex in columnResultToColumnIndex.Values) {
                    if (!(row[rowEntryIndex] is ColoredRowEntry))
                        row[rowEntryIndex] = new ColoredRowEntry(row[rowEntryIndex].DisplayString);
                }

                for (int i = 0; i < tableColumns.Count; ++i) {
                    List<SolidColorBrush> brushes = new List<SolidColorBrush>();
                    if (!reader.IsDBNull(i)) {
                        foreach (var myType in rowIdToSearchValueResult[Convert.ToInt32(reader["_row_no_"])]) {
                            if (tableColumns[i].Name == myType.ColumnHitInfo.ColumnName) {
                                if (myType.SearchValueResult.SearchValue.String == reader[i].ToString())
                                    brushes.Add(ColorManager<ColumnResult>.GetBrush(myType.ColumnResult, 0.5f));
                                else
                                    brushes.Add(ColorManager<ColumnResult>.GetBrush(myType.ColumnResult, 0.25f));
                            }
                        }
                        brushes =
                            brushes.Distinct(EqualityComparer.Create<SolidColorBrush>(x => x.Color.GetHashCode())).ToList();
                    }
                    row[i + columnResultToColumnIndex.Count] = new ColoredRowEntry(reader[i], CreateBrushFromList(brushes));
                }
                Data.Rows.Add(row);
            }
        }

        private void GetRowIdsWithInfo(DistinctTableInfo distinctTableInfo, IDatabase connDistinctDb, List<RowIdWithInfo> rowIdsWithInfo, 
            IEnumerable<HitInfoAndColumnResult> columnHitInfos, Dictionary<int, HashSet<ColumnResult>> rowIdToColumnResults) {
            //Loop through all SearchValueResults and retrieve the row Ids (by inspecting the distinct database)
            //Additionally add the data for the Values DataGrid
            int index = -2;
            int lastSearchValueColumnIndex = 0;
            //Maps for each ColumnResult the search value to the row index it is written in
            Dictionary<ColumnResult, Dictionary<string, int>> columnResultToSearchValueToRowIndex =
                new Dictionary<ColumnResult, Dictionary<string, int>>();
            foreach (var hitInfo in columnHitInfos) {
                //If the columnresult is new, then the column containing the search values does not exist and therefore needs to be filled
                //otherwise only the column "found in ..." needs to be filled, therefore the index only increases by one
                if (!columnResultToSearchValueToRowIndex.ContainsKey(hitInfo.ColumnResult)) {
                    columnResultToSearchValueToRowIndex.Add(hitInfo.ColumnResult, new Dictionary<string, int>());
                    index += 2;
                    lastSearchValueColumnIndex = index;
                }
                else index += 1;

                Dictionary<string, int> searchValueToRowIndex = columnResultToSearchValueToRowIndex[hitInfo.ColumnResult];
                foreach (var result in hitInfo.ColumnHitInfo.Results) {
                    //Add all distinct ids to a list
                    List<uint> distinctIds = new List<uint>();
                    foreach (var hit in result) {
                        if (!distinctIds.Contains(hit.Id))
                            distinctIds.Add(hit.Id);
                    }

                    List<int> rowIds =
                        distinctTableInfo.Columns[hitInfo.ColumnHitInfo.ColumnName].GetRowIdsToDistinctIds(
                            distinctIds,
                            connDistinctDb);

                    //Check if the row has already been created, if not do so
                    IDataRow currentValuesRow = null;

                    if (!searchValueToRowIndex.ContainsKey(result.SearchValue.String)) {
                        //Try to find a free spot in the existing rows
                        for (int i = 0; i < Values.Rows.Count; ++i) {
                            if (string.IsNullOrEmpty(Values.Rows[i].RowEntries[lastSearchValueColumnIndex].DisplayString)) {
                                currentValuesRow = Values.Rows[i];
                                searchValueToRowIndex[result.SearchValue.String] = i;
                                break;
                            }
                        }
                        //If none was found, create a new row
                        if (currentValuesRow == null) {
                            currentValuesRow = Values.CreateRow();
                            Values.AddRow(currentValuesRow);
                            searchValueToRowIndex[result.SearchValue.String] = Values.Rows.Count - 1;
                        }
                        currentValuesRow[lastSearchValueColumnIndex] = new ColoredRowEntry(result.SearchValue.String,
                                                                                           ColorManager<ColumnResult>.GetBrush(
                                                                                               hitInfo.ColumnResult, 0.5f));
                    }
                    else currentValuesRow = Values.Rows[searchValueToRowIndex[result.SearchValue.String]];
                    currentValuesRow[index + 1] = new ColoredRowEntry(rowIds.Count.ToString() + "x");


                    if (rowIds.Count == 0) continue;
                    int maxIds = 100000;
                    if (rowIds.Count > maxIds) rowIds.RemoveRange(maxIds, rowIds.Count - maxIds);

                    foreach (var rowId in rowIds) {
                        rowIdsWithInfo.Add(new RowIdWithInfo {
                                                                 RowId = rowId,
                                                                 SearchValueResult = result,
                                                                 ColumnResult = hitInfo.ColumnResult,
                                                                 ColumnHitInfo = hitInfo.ColumnHitInfo
                                                             });
                        //Add to rowIdToColumnResults
                        HashSet<ColumnResult> columnResults;
                        if (!rowIdToColumnResults.TryGetValue(rowId, out columnResults)) {
                            columnResults = new HashSet<ColumnResult>() { hitInfo.ColumnResult };
                            rowIdToColumnResults.Add(rowId, columnResults);
                        } else if (!columnResults.Contains(hitInfo.ColumnResult))
                            columnResults.Add(hitInfo.ColumnResult);
                    }
                }
            }
        }

        private Brush CreateBrushFromList(List<SolidColorBrush> brushes ) {
            if (brushes.Count == 0) return Brushes.White;
            if(brushes.Count == 1) return brushes[0];
            LinearGradientBrush result = new LinearGradientBrush();
            result.StartPoint = new Point(0, 0);
            result.EndPoint = new Point(1, 1);
            int index = 0;
            foreach(var brush in brushes) {
                
                GradientStop gradientStop = new GradientStop(new Color(){R=brush.Color.R,G=brush.Color.G,B=brush.Color.B,A=(byte)(brush.Opacity*255)}, index/((float) brushes.Count - 1));
                result.GradientStops.Add(gradientStop);
                index++;
            }
            result.Freeze();
            return result;
        }
        #endregion Methods
    }
}
