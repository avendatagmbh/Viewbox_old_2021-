// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-01-11
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;
using AV.Log;
using AvdCommon.DataGridHelper;
using Business.Structures.DataTransferAgents;
using Config.Interfaces.DbStructure;
using Config.Structures;
using Logging;
using TransDATA.Structures;
using Utils;
using System.Text;
using log4net;

namespace TransDATA.Models {
    public class SelectedProfileModel : NotifyPropertyChangedBase {
        internal ILog _log = LogHelper.GetLogger();

        internal SelectedProfileModel(IProfile profile) {
            Profile = profile;
            foreach (var table in profile.Tables) {
                if (table.Count > 0) _filledTables.Add(table);
                else _emptyTables.Add(table);
            }

            UpdateFilledTablesFilter();
            UpdateEmptyTablesFilter();

            _tableDisplayOptions.PropertyChanged += (sender, e) => UpdateFilledTablesFilter();
            _emptyTableDisplayOptions.PropertyChanged += (sender, e) => UpdateEmptyTablesFilter();
            _columnDisplayOptions.PropertyChanged += (sender, e) => UpdateColumnsFilter();

            TableSelectionChanged();

            LogDb = new LoggingDb(Config.ConfigDb.ConnectionManager);

            DatasetPreviewCount = 50;
        }

        protected IProfile Profile { get; set; }
        protected LoggingDb LogDb { get; set; }
        private System.Drawing.Color _unselectedColumnColor = System.Drawing.Color.FromArgb(255, 190, 190, 190);

        #region SelectedTable
        private ITable _selectedTable;

        public ITable SelectedTable {
            get { return _selectedTable; }
            set {
                _selectedTable = value;
                OnPropertyChanged("SelectedTable");

                ColumnDisplayOptions.Reset();
                OnPropertyChanged("ColumnDisplayOptions");

                ColumnSelectionChanged();

                _columns.Clear();
                if (_selectedTable != null) {
                    foreach (var column in _selectedTable.Columns) {
                        _columns.Add(column);
                    }
                }

                UpdateColumnsFilter();
                UpdateTableInfo();

                UpdateDataPreview();
            }
        }
        
        #endregion SelectedTable

        #region SelectedColumn
        private ITableColumn _selectedTableColumn;

        public ITableColumn SelectedTableColumn {
            get { return _selectedTableColumn; }
            set {
                _selectedTableColumn = value;
                OnPropertyChanged("SelectedTableColumn");
            }
        }
        #endregion SelectedColumn

        #region DatasetPreviewCount
        private int _datasetPreviewCount;

        public int DatasetPreviewCount {
            get { return _datasetPreviewCount; }
            set {
                _datasetPreviewCount = value;
                OnPropertyChanged("SelectedTableColumn");
            }
        }
        #endregion DatasetPreviewCount

        private readonly ObservableCollectionAsync<ITable> _filledTables = new ObservableCollectionAsync<ITable>();
        private readonly ObservableCollectionAsync<ITable> _emptyTables = new ObservableCollectionAsync<ITable>();
        private readonly ObservableCollectionAsync<ITable> _visibleFilledTables = new ObservableCollectionAsync<ITable>();
        private readonly ObservableCollectionAsync<ITable> _visibleEmptyTables = new ObservableCollectionAsync<ITable>();
        private readonly ObservableCollectionAsync<IColumn> _columns = new ObservableCollectionAsync<IColumn>();
        private readonly ObservableCollectionAsync<IColumn> _visibleColumns = new ObservableCollectionAsync<IColumn>();

        public IEnumerable<ITable> FilledTables { get { return _filledTables; } }

        public IEnumerable<ITable> EmptyTables { get { return _emptyTables; } }

        public IEnumerable<ITable> VisibleFilledTables { get { return _visibleFilledTables; } }

        public IEnumerable<ITable> VisibleEmptyTables { get { return _visibleEmptyTables; } }

        public IEnumerable<IColumn> Columns { get { return _columns; } }

        public IEnumerable<IColumn> VisibleColumns { get { return _visibleColumns; } }

        #region VisableFilledTablesChecked
        private bool? _visableFilledTablesChecked = false;

        public bool? VisableFilledTablesChecked {
            get { return _visableFilledTablesChecked; }
            set {
                _visableFilledTablesChecked = value;
                if (!value.HasValue || !_doTableUpdate) return;
                foreach (var visibleFilledTable in VisibleFilledTables) {
                    visibleFilledTable.DoDbUpdate = false;
                    visibleFilledTable.DoExport = (bool) value;
                    visibleFilledTable.DoDbUpdate = true;
                }
                //Mass-update
                Config.ConfigDb.UpdateDoExport(_visibleFilledTables, (bool)value);
                OnPropertyChanged("VisableFilledTablesChecked");
                OnPropertyChanged("VisableFilledTablesCheckedLabel");

            }
        }
        #endregion

        #region VisableFilledTablesCheckedLabel
        public string VisableFilledTablesCheckedLabel {
            get {
                int selectedTables = 0;
                int totalTables = 0;
                foreach (var visibleFilledTable in FilledTables) {
                    totalTables++;
                    if (visibleFilledTable.DoExport) selectedTables++;
                }
                string result = selectedTables + " / " + totalTables + " " + Base.Localisation.ResourcesCommon.Selected;
                if (!string.IsNullOrEmpty(_tableDisplayOptions.Filter))
                    result += " (" + Base.Localisation.ResourcesCommon.Filtered + ")";
                return result;
            }
        }
        #endregion

        #region VisableEmptyTablesChecked
        private bool? _visableEmptyTablesChecked = false;

        public bool? VisableEmptyTablesChecked {
            get { return _visableEmptyTablesChecked; }
            set {
                _visableEmptyTablesChecked = value;
                if (!value.HasValue || !_doTableUpdate) return;
                foreach (var visibleEmptyTable in VisibleEmptyTables) {
                    visibleEmptyTable.DoDbUpdate = false;
                    visibleEmptyTable.DoExport = (bool) value;
                    visibleEmptyTable.DoDbUpdate = true;
                }
                OnPropertyChanged("VisableEmptyTablesChecked");
                OnPropertyChanged("VisableEmptyTablesCheckedLabel");

                Config.ConfigDb.UpdateDoExport(_visibleEmptyTables, (bool)value);
                //Config.ConfigDb.Save(_visibleEmptyTables);
            }
        }
        #endregion

        #region VisableEmptyTablesCheckedLabel
        public string VisableEmptyTablesCheckedLabel {
            get {
                int selectedTables = 0;
                int totalTables = 0;
                foreach (var visibleEmptyTable in EmptyTables) {
                    totalTables++;
                    if (visibleEmptyTable.DoExport) selectedTables++;
                }
                string result = selectedTables + " / " + totalTables + " " + Base.Localisation.ResourcesCommon.Selected;
                if (!string.IsNullOrEmpty(_emptyTableDisplayOptions.Filter))
                    result += " (" + Base.Localisation.ResourcesCommon.Filtered + ")";
                return result;
            }
        }
        #endregion

        #region VisableColumnsChecked
        private bool? _visableColumnsChecked = false;

        public bool? VisableColumnsChecked {
            get { return _visableColumnsChecked; }
            set {
                _visableColumnsChecked = value;
                if (!value.HasValue || !_doColumnUpdate) return;
                foreach (var col in VisibleColumns) {
                    col.DoDbUpdate = false;
                    col.DoExport = (bool)value;
                    col.DoDbUpdate = true;
                }
                OnPropertyChanged("VisableColumnsCheckedLabel");

                Config.ConfigDb.Save(SelectedTable.Columns);
            }
        }
        #endregion

        #region VisableColumnsCheckedLabel
        public string VisableColumnsCheckedLabel {
            get {
                if (SelectedTable == null || SelectedTable.Columns == null) {
                    return string.Empty;
                }
                int selectedColumns = 0;
                int totalColumns = 0;
                foreach (var col in SelectedTable.Columns) {
                    totalColumns++;
                    if (col.DoExport) selectedColumns++;
                }
                string result = selectedColumns + " / " + totalColumns + " " + Base.Localisation.ResourcesCommon.Selected;
                if (!string.IsNullOrEmpty(ColumnDisplayOptions.Filter))
                    result += " (" + Base.Localisation.ResourcesCommon.Filtered + ")";
                return result;
            }
        }
        #endregion

        #region TableDisplayOptions
        private readonly TableDisplayOptions _tableDisplayOptions = new TableDisplayOptions();
        public TableDisplayOptions TableDisplayOptions { get { return _tableDisplayOptions; } }
        #endregion

        #region EmptyTableDisplayOptions
        private readonly TableDisplayOptions _emptyTableDisplayOptions = new TableDisplayOptions();
        public TableDisplayOptions EmptyTableDisplayOptions { get { return _emptyTableDisplayOptions; } }
        #endregion

        #region ColumnDisplayOptions
        private readonly ColumnDisplayOptions _columnDisplayOptions = new ColumnDisplayOptions();
        public ColumnDisplayOptions ColumnDisplayOptions { get { return _columnDisplayOptions; } }
        #endregion

        public void UpdateFilter() {
            UpdateFilledTablesFilter();
            UpdateEmptyTablesFilter();
        }

        private void UpdateFilledTablesFilter() {
            //_visibleFilledTables.RaiseListChangedEvents = false;
            _visibleFilledTables.Clear();

            var filteredTables = new List<ITable>();
            foreach (var filledTable in _filledTables)
            {
                switch (TableDisplayOptions.FilterType)
                {
                    case TableFilterType.TransferedError:
                        if (filledTable.TransferState.State == TransferStates.TransferedError)
                            filteredTables.Add(filledTable);
                        break;

                    case TableFilterType.TransferedOk:
                        if (filledTable.TransferState.State == TransferStates.TransferedOk)
                            filteredTables.Add(filledTable);
                        break;

                    case TableFilterType.TransferedCountDifference:
                        if (filledTable.TransferState.State == TransferStates.TransferedCountDifference)
                            filteredTables.Add(filledTable);
                        break;

                    case TableFilterType.NotTransfered:
                        if (filledTable.TransferState.State == TransferStates.NotTransfered)
                            filteredTables.Add(filledTable);
                        break;

                    default:
                        filteredTables.Add(filledTable);
                        break;
                }
            }

            switch (TableDisplayOptions.SortType) {
                case TableSortType.Original:
                    foreach (
                        var table in
                            filteredTables.Where(
                                table =>
                                string.IsNullOrEmpty(TableDisplayOptions.Filter) ||
                                table.Name.ToLower().Contains(TableDisplayOptions.Filter.ToLower()))) {
                        _visibleFilledTables.Add(table);
                    }
                    break;

                case TableSortType.NameAsc:
                    foreach (
                        var table in
                            filteredTables.Where(
                                table =>
                                string.IsNullOrEmpty(TableDisplayOptions.Filter) ||
                                table.Name.ToLower().Contains(TableDisplayOptions.Filter.ToLower())).OrderBy(item => item.Name)) {
                        _visibleFilledTables.Add(table);
                    }
                    break;

                case TableSortType.NameDesc:
                    foreach (
                        var table in
                            filteredTables.Where(
                                table =>
                                string.IsNullOrEmpty(TableDisplayOptions.Filter) ||
                                table.Name.ToLower().Contains(TableDisplayOptions.Filter.ToLower())).OrderByDescending(item => item.Name)) {
                        _visibleFilledTables.Add(table);
                    }
                    break;

                case TableSortType.CountAsc:
                    foreach (
                        var table in
                            filteredTables.Where(
                                table =>
                                string.IsNullOrEmpty(TableDisplayOptions.Filter) ||
                                table.Name.ToLower().Contains(TableDisplayOptions.Filter.ToLower())).OrderBy(item => item.Count)) {
                        _visibleFilledTables.Add(table);
                    }
                    break;

                case TableSortType.CountDesc:
                    foreach (
                        var table in
                            filteredTables.Where(
                                table =>
                                string.IsNullOrEmpty(TableDisplayOptions.Filter) ||
                                table.Name.ToLower().Contains(TableDisplayOptions.Filter.ToLower())).OrderByDescending(item => item.Count)) {
                        _visibleFilledTables.Add(table);
                    }
                    break;

                case TableSortType.TransferState:
                    foreach (
                        var table in
                            filteredTables.Where(
                                table =>
                                string.IsNullOrEmpty(TableDisplayOptions.Filter) ||
                                table.Name.ToLower().Contains(TableDisplayOptions.Filter.ToLower())).OrderBy(item => item.TransferState.State)) {
                        _visibleFilledTables.Add(table);
                    }
                    break;

                default:
                    ArgumentOutOfRangeException ex = new ArgumentOutOfRangeException();
                    using (NDC.Push(LogHelper.GetNamespaceContext()))
                    {
                        _log.Error(ex.Message, ex);
                    }
                    throw ex;
            }
            TableSelectionChanged();
            //_visibleFilledTables.RaiseListChangedEvents = true;
        }

        private void UpdateEmptyTablesFilter() {
            _visibleEmptyTables.Clear();

            var filteredTables = new List<ITable>();
            foreach (var emtyTable in _emptyTables) {
                switch (EmptyTableDisplayOptions.FilterType) {
                    case TableFilterType.TransferedError:
                        if (emtyTable.TransferState.State == TransferStates.TransferedError)
                            filteredTables.Add(emtyTable);
                        break;

                    case TableFilterType.TransferedOk:
                        if (emtyTable.TransferState.State == TransferStates.TransferedOk)
                            filteredTables.Add(emtyTable);
                        break;

                    case TableFilterType.TransferedCountDifference:
                        if (emtyTable.TransferState.State == TransferStates.TransferedCountDifference)
                            filteredTables.Add(emtyTable);
                        break;

                    case TableFilterType.NotTransfered:
                        if (emtyTable.TransferState.State == TransferStates.NotTransfered)
                            filteredTables.Add(emtyTable);
                        break;

                    default:
                        filteredTables.Add(emtyTable);
                        break;
                }
            }

            switch (EmptyTableDisplayOptions.SortType) {
                case TableSortType.Original:
                    foreach (
                        var table in
                            filteredTables.Where(
                                table =>
                                string.IsNullOrEmpty(EmptyTableDisplayOptions.Filter) ||
                                table.Name.ToLower().Contains(EmptyTableDisplayOptions.Filter.ToLower()))) {
                        _visibleEmptyTables.Add(table);
                    }
                    break;

                case TableSortType.NameAsc:
                    foreach (
                        var table in
                            filteredTables.Where(
                                table =>
                                string.IsNullOrEmpty(EmptyTableDisplayOptions.Filter) ||
                                table.Name.ToLower().Contains(EmptyTableDisplayOptions.Filter.ToLower())).OrderBy(item => item.Name)) {
                        _visibleEmptyTables.Add(table);
                    }
                    break;

                case TableSortType.NameDesc:
                    foreach (
                        var table in
                            filteredTables.Where(
                                table =>
                                string.IsNullOrEmpty(EmptyTableDisplayOptions.Filter) ||
                                table.Name.ToLower().Contains(EmptyTableDisplayOptions.Filter.ToLower())).OrderBy(item => item.Name)) {
                        _visibleEmptyTables.Add(table);
                    }
                    break;

                case TableSortType.CountAsc:
                    foreach (
                        var table in
                            filteredTables.Where(
                                table =>
                                string.IsNullOrEmpty(TableDisplayOptions.Filter) ||
                                table.Name.ToLower().Contains(TableDisplayOptions.Filter.ToLower())).OrderBy(item => item.Count)) {
                        _visibleFilledTables.Add(table);
                    }
                    break;

                case TableSortType.CountDesc:
                    foreach (
                        var table in
                            filteredTables.Where(
                                table =>
                                string.IsNullOrEmpty(TableDisplayOptions.Filter) ||
                                table.Name.ToLower().Contains(TableDisplayOptions.Filter.ToLower())).OrderByDescending(item => item.Count)) {
                        _visibleFilledTables.Add(table);
                    }
                    break;
                case TableSortType.TransferState:
                    foreach (
                        var table in
                            filteredTables.Where(
                                table =>
                                string.IsNullOrEmpty(EmptyTableDisplayOptions.Filter) ||
                                table.Name.ToLower().Contains(EmptyTableDisplayOptions.Filter.ToLower())).OrderBy(item => item.TransferState.State)) {
                        _visibleEmptyTables.Add(table);
                    }
                    break;
                default:
                    ArgumentOutOfRangeException ex = new ArgumentOutOfRangeException();
                    using (NDC.Push(LogHelper.GetNamespaceContext()))
                    {
                        _log.Error(ex.Message, ex);
                    }
                    throw ex;
            }
            TableSelectionChanged();
        }

        private void UpdateColumnsFilter() {
            _visibleColumns.Clear();
            switch (ColumnDisplayOptions.SortType) {
                case ColumnSortType.Original:
                    foreach (
                        var col in
                            _columns.Where(
                                col =>
                                string.IsNullOrEmpty(ColumnDisplayOptions.Filter) ||
                                col.Name.Contains(ColumnDisplayOptions.Filter))) {
                        _visibleColumns.Add(col);
                    }
                    break;

                case ColumnSortType.NameAsc:
                    foreach (
                        var col in
                            _columns.Where(
                                col =>
                                string.IsNullOrEmpty(ColumnDisplayOptions.Filter) ||
                                col.Name.Contains(ColumnDisplayOptions.Filter)).OrderBy(item => item.Name)) {
                        _visibleColumns.Add(col);
                    }
                    break;

                case ColumnSortType.NameDesc:
                    foreach (
                        var col in
                            _columns.Where(
                                col =>
                                string.IsNullOrEmpty(ColumnDisplayOptions.Filter) ||
                                col.Name.Contains(ColumnDisplayOptions.Filter)).OrderByDescending(item => item.Name)) {
                        _visibleColumns.Add(col);
                    }
                    break;

                default:
                    ArgumentOutOfRangeException ex = new ArgumentOutOfRangeException();
                    using (NDC.Push(LogHelper.GetNamespaceContext()))
                    {
                        _log.Error(ex.Message, ex);
                    }
                    throw ex;
            }
            ColumnSelectionChanged();
        }

        //internal void ColumnReordered(Dictionary<int, string> newOrder) {
        //    //ColumnDisplayOptions.SortType = ColumnSortType.UserDefined;
        //    _visibleColumns.Clear();

        //    foreach (var keyPair in newOrder) {
        //        foreach (var column in _columns) {
        //            if (column.Name == keyPair.Value &&
        //                (string.IsNullOrEmpty(ColumnDisplayOptions.Filter) ||
        //                 column.Name.Contains(ColumnDisplayOptions.Filter)))
        //                _visibleColumns.Add(column);

        //        }
        //    }
        //}

        #region TableSelectionChanged
        private bool _doTableUpdate = true;
        public void TableSelectionChanged() {
            _doTableUpdate = false;

            OnPropertyChanged("VisableFilledTablesCheckedLabel");
            OnPropertyChanged("VisableEmptyTablesCheckedLabel");

            int countSelectedVisable = 0;
            int countVisable = 0;

            foreach (var visibleFilledTable in VisibleFilledTables) {
                countVisable++;
                if (visibleFilledTable.DoExport) countSelectedVisable++;
            }
            if (countSelectedVisable == countVisable) {
                VisableFilledTablesChecked = true;
            } else if (countSelectedVisable == 0) {
                VisableFilledTablesChecked = false;
            } else if (countSelectedVisable != countVisable) {
                VisableFilledTablesChecked = null;
            }
            OnPropertyChanged("VisableFilledTablesChecked");

            countSelectedVisable = 0;
            countVisable = 0;

            foreach (var visibleEmptyTable in VisibleEmptyTables) {
                countVisable++;
                if (visibleEmptyTable.DoExport) countSelectedVisable++;
            }
            if (countSelectedVisable == countVisable) {
                VisableEmptyTablesChecked = true;
            } else if (countSelectedVisable == 0) {
                VisableEmptyTablesChecked = false;
            } else if (countSelectedVisable != countVisable) {
                VisableEmptyTablesChecked = null;
            }
            OnPropertyChanged("VisableEmptyTablesChecked");

            _doTableUpdate = true;
        }
        #endregion TableSelectionChanged

        #region ColumnSelectionChanged
        private bool _doColumnUpdate = true;
        public void ColumnSelectionChanged() {
            if(SelectedTable == null || SelectedTable.Columns == null) return;
            _doColumnUpdate = false;

            OnPropertyChanged("VisableColumnsCheckedLabel");

            int countVisableColumns = 0;
            int countVisableSelectedColumns = 0;

            foreach (var column in _visibleColumns) {
                countVisableColumns++;
                if (column.DoExport) countVisableSelectedColumns++;
            }

            if (countVisableColumns == countVisableSelectedColumns) {
                VisableColumnsChecked = true;
            } else if (countVisableSelectedColumns == 0) {
                VisableColumnsChecked = false;
            } else if (countVisableColumns != countVisableSelectedColumns) {
                VisableColumnsChecked = null;
            }

            OnPropertyChanged("VisableColumnsChecked");

            _doColumnUpdate = true;

            SetPreviewColors();
        }

        internal void ColumnSelectionChanged(string changedColumn) {
            foreach (var column in _columns) {
                if (column.Name == changedColumn) column.DoExport = !column.DoExport;
            }
            ColumnSelectionChanged();
        }
        #endregion ColumnSelectionChanged

        #region TransferInfo
        private Logging.Interfaces.DbStructure.ITable _lastLogTable;
        public Logging.Interfaces.DbStructure.ITable LastLogTable { get { return _lastLogTable; } }

        public string LastLogTableTimestamp {
            get {
                if (LastLogTable == null) return string.Empty;
                return LastLogTable.Timestamp.ToString("F");
            }
        }

        public string LastLogTableDuration {
            get {
                if (LastLogTable == null) return string.Empty;
                return TimespanToString(LastLogTable.Duration);
            }
        }

        private string TimespanToString(TimeSpan span) {
            if (span.TotalSeconds < 1) return "< 1 " + Base.Localisation.ResourcesCommon.Second;
            StringBuilder sb = new StringBuilder();
            if (span.Days > 0) {
                sb.Append(span.Days + " ");
                sb.Append(span.Days > 1 ? Base.Localisation.ResourcesCommon.Days : Base.Localisation.ResourcesCommon.Day);
                sb.Append(" ");
            }
            if (span.Hours > 0) {
                sb.Append(span.Hours + " ");
                sb.Append(span.Hours > 1 ? Base.Localisation.ResourcesCommon.Hours : Base.Localisation.ResourcesCommon.Hour);
                sb.Append(" ");
            }
            if (span.Minutes > 0) {
                sb.Append(span.Minutes + " ");
                sb.Append(span.Minutes > 1 ? Base.Localisation.ResourcesCommon.Minutes : Base.Localisation.ResourcesCommon.Minute);
                sb.Append(" ");
            }
            if (span.Seconds > 0) {
                sb.Append(span.Seconds + " ");
                sb.Append(span.Seconds > 1 ? Base.Localisation.ResourcesCommon.Seconds : Base.Localisation.ResourcesCommon.Second);
            }
            return sb.ToString();
        }

        #region UpdatetableInfo
        private void UpdateTableInfo() {
            if (SelectedTable != null) {
                List<Logging.Interfaces.DbStructure.ITable> lTables = LogDb.GetLogTables(SelectedTable.Id);
                if (lTables.Count == 0) {
                    _lastLogTable = null;
                } else {
                    _lastLogTable = lTables[0];
                    foreach (var table in lTables) {
                        if (table.Timestamp > LastLogTable.Timestamp) _lastLogTable = table;
                    }
                }
                OnPropertyChanged("LastLogTable");
                OnPropertyChanged("LastLogTableTimestamp");
                OnPropertyChanged("LastLogTableDuration");
            }
        }
        #endregion
        #endregion

        #region Data preview
        private AvdCommon.DataGridHelper.DataTable _dataPreview;

        public AvdCommon.DataGridHelper.DataTable DataPreview {
            get { return _dataPreview; }
            set {
                _dataPreview = value;
                OnPropertyChanged("DataPreview");
            }
        }

        private Thread _dataPreviewThread;
        private void UpdateDataPreview() { 
            DataPreview = null;

            if (_dataPreviewThread != null && _dataPreviewThread.IsAlive) _dataPreviewThread.Abort();
            _dataPreviewThread = new Thread(LoadPreviewData);
            _dataPreviewThread.Start();
        }

        private void LoadPreviewData() {
            if (SelectedTable == null)
                return;
            try {
                var result =
                    DataGridCreater.CreateDataTable(new DataTransferAgent(Profile).GetPreview(SelectedTable,
                                                                                              DatasetPreviewCount));
                foreach (var dataColumn in result.Columns) {
                    foreach (var column in _columns) {
                        if (dataColumn.Name == column.Name) {
                            dataColumn.Color = !column.DoExport ? _unselectedColumnColor : System.Drawing.Color.White;
                        }
                    }
                    DataPreview = result;
                }
            } catch(Exception ex) {
                using (NDC.Push(LogHelper.GetNamespaceContext()))
                {
                    _log.Error(ex.Message, ex);
                }

                MessageBox.Show(ex.Message);
            }
        }

        private void SetPreviewColors() {
            if(DataPreview == null) return;
            foreach (var dataColumn in DataPreview.Columns) {
                foreach (var column in _columns) {
                    if (dataColumn.Name == column.Name) {
                        dataColumn.Color = !column.DoExport ? _unselectedColumnColor : System.Drawing.Color.White;
                    }
                }
            }
            OnPropertyChanged("DataPreview");
        }
        #endregion
    }
}