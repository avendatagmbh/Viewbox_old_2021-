using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using SystemDb;
using SystemDb.Internal;
using DbAccess;
using System.ComponentModel;
using DbAccess.Structures;
using Utils;
using ViewAssistantBusiness.Config;
using Base.Localisation;
using ViewAssistant.Controls;
using System.Globalization;
using AV.Log;
using log4net;
using System.Text.RegularExpressions;

namespace ViewAssistantBusiness.Models
{
    public delegate void DataPreviewShowed(object sender, DataPreviewModel table);

    public delegate void ConfigureLocalisationTextsClicked(object sender, IViewboxLocalisable table);

    public delegate void LocalisationAllTextsClicked(object sender);

    public delegate void RenamerClicked(object sender, IRenameable model);

    public delegate void RenamerSettingsClicked(object sender);

    public class MainModel : NotifyPropertyChangedBase, IDisposable
    {
        #region Main

        #region Instance
        private static MainModel _instance;
        
        public static  MainModel Instance
        {
            get { return _instance ?? (_instance = new MainModel()); }
        }

        #endregion Main

        protected MainModel()
        {
            ProgressCalculator.DefaultProgressDescription = "";
            SourceTables = new ObservableCollectionAsync<TableModel>();
            _viewboxTables = new ObservableCollectionAsync<TableModel>();
            _finalTables = new ObservableCollectionAsync<TableModel>();

            TableCollection = new TableCollection(this);
            TableCollection.ConfigureTableTextsClicked += TableCollection_ConfigureTableTextsClicked;
            TableCollection.RenamerClicked += TableCollection_RenamerClicked;
            OptimizationTypeTexts = OptimizationTypeExt.GetOptimizationTypeTexts();
            OperatorsTexts = ViewboxDb.RowNoFilters.Parser.OperatorsExt.GetOperatorsTexts();
            SetAllSelects();

            DeleteSpecialCommand = new RelayCommand(DeleteSpecialClick);
            ShowSourceSpecialCommand = new RelayCommand(ShowSourceSpecialClick);
            ShowFinalSpecialCommand = new RelayCommand(ShowFinalSpecialClick);
            ShowOnlyRelevantFinalTables = true;
            ShowOnlyRelevantViewboxTables = true;
        }

        void TableCollection_ConfigureTableTextsClicked(object sender, IViewboxLocalisable table)
        {
            OnConfigureLocalisationTextsClicked(sender, table);
        }

        public event ConfigureLocalisationTextsClicked ConfigureLocalisationTextsClicked;

        public void OnConfigureLocalisationTextsClicked(object sender, IViewboxLocalisable model)
        {
            if (ConfigureLocalisationTextsClicked != null)
            {
                ConfigureLocalisationTextsClicked(sender, model);
            }
        }

        public event LocalisationAllTextsClicked LocalisationAllTextsClicked;

        public void OnLocalisationAllTextsClicked(object sender)
        {
            if (LocalisationAllTextsClicked != null)
            {
                LocalisationAllTextsClicked(sender);
            }
        }


        void TableCollection_RenamerClicked(object sender, IRenameable model)
        {
            OnRenamerClicked(sender, model);
        }

        public event RenamerClicked RenamerClicked;

        public void OnRenamerClicked(object sender, IRenameable model)
        {
            if (RenamerClicked != null)
            {
                RenamerClicked(sender, model);
            }
        }

        public event RenamerSettingsClicked RenamerSettingsClicked;

        public void OnRenamerSettingsClicked(object sender)
        {
            if (RenamerSettingsClicked != null)
            {
                RenamerSettingsClicked(sender);
            }
        }


        public void Dispose()
        {
            if (_systemDb != null)
                _systemDb.Dispose();
            ConfigDb.Cleanup();
        }

        public void OpenLogDirectory()
        {
            Process.Start(ConfigDb.GetLogDirectoryPath());
        }

        #endregion Main

        #region Properties

        private readonly ILog _logger = LogHelper.GetLogger();
        private const int PreviewRecordCount = 50;
        private const string InsertSql = "INSERT INTO {0} ( {1} ) SELECT {1} FROM {2}";
        private const string InsertSqlWithOpt = "INSERT INTO {0} ( {1} ) SELECT {1} FROM {2} ORDER BY {3}";
        private const string SelectSql = "SELECT {0} FROM {1}";
        private const string TimeSpanFormat = "hh\\:mm\\:ss";

        private readonly char _numberSep = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator[0];

        private TableModel _selectedSourceTable;
        public TableModel SelectedSourceTable
        {
            get { return _selectedSourceTable; }
            set
            {
                if (value != _selectedSourceTable && (value == null || SourceTables.FirstOrDefault(w => w.Name == value.Name) != null))
                {
                    _selectedSourceTable = value;
                    OnPropertyChanged("SelectedSourceTable");
                    if (_selectedSourceTable == null)
                        return;
                    if (_selectedSourceTable.IsInFinal)
                        SelectedFinalTable = _selectedSourceTable;
                    if (_selectedSourceTable.IsInViewbox)
                        SelectedViewboxTable = _selectedSourceTable;
                }
            }
        }

        private TableModel _selectedViewboxTable;
        public TableModel SelectedViewboxTable
        {
            get { return _selectedViewboxTable; }
            set
            {
                if (value != _selectedViewboxTable && (value == null || ViewboxTables.FirstOrDefault(w => w.Name == value.Name) != null))
                {
                    _selectedViewboxTable = value;
                    OnPropertyChanged("SelectedViewboxTable");
                    if (_selectedViewboxTable == null)
                        return;
                    if (_selectedViewboxTable.IsInFinal)
                        SelectedFinalTable = _selectedViewboxTable;
                    if (_selectedViewboxTable.IsInSource)
                        SelectedSourceTable = _selectedViewboxTable;
                }
            }
        }

        private TableModel _selectedFinalTable;
        public TableModel SelectedFinalTable
        {
            get { return _selectedFinalTable; }
            set
            {
                if (value != _selectedFinalTable && (value == null || FinalTables.FirstOrDefault(w => w.Name == value.Name) != null))
                {
                    _selectedFinalTable = value;
                    OnPropertyChanged("SelectedFinalTable");
                    if (_selectedFinalTable == null)
                        return;
                    if (_selectedFinalTable.IsInViewbox)
                        SelectedViewboxTable = _selectedFinalTable;
                    if (_selectedFinalTable.IsInSource)
                        SelectedSourceTable = _selectedFinalTable;
                }
            }
        }

        private ColumnModel _selectedColumn;
        public ColumnModel SelectedColumn
        {
            get { return _selectedColumn; }
            set
            {
                if (value != _selectedColumn)
                {
                    _selectedColumn = value;
                    OnPropertyChanged("SelectedColumn");
                }
            }

        }

        public bool IsInitialized
        {
            get { return IsInitializedSource && IsInitializedViewbox && IsInitializedFinal; }
        }

        private bool _isInitializedSource;
        public bool IsInitializedSource
        {
            get { return _isInitializedSource; }
            set
            {
                if (value != _isInitializedSource)
                {
                    _isInitializedSource = value;
                    OnPropertyChanged("IsInitializedSource");
                    OnPropertyChanged("IsInitialized");
                }
            }
        }

        private SystemDb.SystemDb _systemDb;

        private bool _isInitializedViewbox;
        private bool _isInitializedSystemDb;
        public bool IsInitializedViewbox
        {
            get { return _isInitializedViewbox; }
            set
            {
                if (value != _isInitializedViewbox)
                {
                    _isInitializedViewbox = value;
                    OnPropertyChanged("IsInitializedViewbox");
                    OnPropertyChanged("IsInitialized");
                }
            }
        }

        private bool _isInitializedFinal;
        public bool IsInitializedFinal
        {
            get { return _isInitializedFinal; }
            set
            {
                if (value != _isInitializedFinal)
                {
                    _isInitializedFinal = value;
                    OnPropertyChanged("IsInitializedFinal");
                    OnPropertyChanged("IsInitialized");
                }
            }
        }


        private ProfileConfigModel _currentProfile;
        public ProfileConfigModel CurrentProfile
        {
            get { return _currentProfile; }
            set
            {
                if (_currentProfile != value)
                {
                    _currentProfile = value;
                    InitConnections();
                    OnPropertyChanged("CurrentProfile");
                }
            }
        }


        private bool? _selectAllSource;

        public bool? SelectAllSource
        {
            get { return _selectAllSource; }
            set
            {
                if (_selectAllSource != value)
                {
                    _selectAllSource = value;
                    OnPropertyChanged("SelectAllSource");
                    if (_selectAllSource.HasValue)
                    {
                        DisableCheckEvents = true;
                        try
                        {
                            foreach (var table in TableCollection.SourceTables)
                                table.Value.SourceChecked = _selectAllSource.Value;
                        }
                        finally
                        {
                            DisableCheckEvents = false;
                        }
                    }
                }
            }
        }

        private bool? _selectAllViewBox;
        public bool? SelectAllViewbox
        {
            get { return _selectAllViewBox; }
            set
            {
                if (_selectAllViewBox != value)
                {
                    _selectAllViewBox = value;
                    OnPropertyChanged("SelectAllViewbox");
                    if (_selectAllViewBox.HasValue)
                    {
                        DisableCheckEvents = true;
                        try
                        {
                            foreach (var table in TableCollection.ViewboxTables)
                                table.Value.ViewboxChecked = _selectAllViewBox.Value;
                        }
                        finally
                        {
                            DisableCheckEvents = false;
                        }
                    }
                }
            }
        }

        private bool _showOnlyRelevantViewboxTables;
        public bool ShowOnlyRelevantViewboxTables
        {
            get { return _showOnlyRelevantViewboxTables; }
            set
            {
                _showOnlyRelevantViewboxTables = value;
                OnPropertyChanged("ShowOnlyRelevantViewboxTables");
                OnPropertyChanged("ViewboxTables");
            }
        }

        private bool _showOnlyRelevantFinalTables;
        public bool ShowOnlyRelevantFinalTables
        {
            get { return _showOnlyRelevantFinalTables; }
            set
            {
                _showOnlyRelevantFinalTables = value;
                OnPropertyChanged("ShowOnlyRelevantFinalTables");
                OnPropertyChanged("FinalTables");
            }
        }

        public ILanguageCollection Languages { get { return _systemDb.Languages; } }

        public TableCollection TableCollection { get; private set; }
        private ObservableCollectionAsync<TableModel> _previousSourceTables = new ObservableCollectionAsync<TableModel>();
        public ObservableCollectionAsync<TableModel> SourceTables { get; set; }

        private ObservableCollectionAsync<TableModel> _previousViewboxTables = new ObservableCollectionAsync<TableModel>();
        private ObservableCollectionAsync<TableModel> _viewboxTables;
        public ObservableCollectionAsync<TableModel> ViewboxTables
        {
            get
            {
                if (ShowOnlyRelevantViewboxTables)
                {
                    var filtered = _viewboxTables.Where(p => p.IsInSource);
                    foreach (var other in _viewboxTables.Where(p => !p.IsInSource))
                    {
                        other.ViewboxChecked = false;
                    }
                    ObservableCollectionAsync<TableModel> result = new ObservableCollectionAsync<TableModel>();
                    result.AddRange(filtered);
                    return result;
                }
                return _viewboxTables;
            }
        }

        private ObservableCollectionAsync<TableModel> _finalTables;
        public ObservableCollectionAsync<TableModel> FinalTables
        {
            get
            {
                if (ShowOnlyRelevantFinalTables)
                {
                    var filtered = _finalTables.Where(p => p.IsInSource && p.IsInViewbox);
                    ObservableCollectionAsync<TableModel> result = new ObservableCollectionAsync<TableModel>();
                    result.AddRange(filtered);
                    return result;
                }
                return _finalTables;
            }
        }

        private ProgressCalculator _sourceProgress;
        public ProgressCalculator SourceProgress
        {
            get { return _sourceProgress; }
            set
            {
                if (value != _sourceProgress)
                {
                    _sourceProgress = value;
                    _sourceProgress.PropertyChanged += (s, e) => UpdateCanDoProperties();
                    OnPropertyChanged("SourceProgress");
                    OnPropertyChanged("SourceProgressIsNotBusy");
                    OnPropertyChanged("SourceProgressIsBusy");
                }
            }
        }


        private ProgressCalculator _viewboxProgress;
        public ProgressCalculator ViewboxProgress
        {
            get { return _viewboxProgress; }
            set
            {
                if (value != _viewboxProgress)
                {
                    _viewboxProgress = value;
                    _viewboxProgress.PropertyChanged += (s, e) => UpdateCanDoProperties();
                    OnPropertyChanged("ViewboxProgress"); 
                    OnPropertyChanged("ViewboxProgressIsNotBusy");
                    OnPropertyChanged("ViewboxProgressIsBusy");
                }
            }
        }

        private ProgressCalculator _finalProgress;
        public ProgressCalculator FinalProgress
        {
            get { return _finalProgress; }
            set
            {
                if (value != _finalProgress)
                {
                    _finalProgress = value;
                    _finalProgress.PropertyChanged += (s, e) => UpdateCanDoProperties();
                    OnPropertyChanged("FinalProgress");
                    OnPropertyChanged("FinalProgressIsNotBusy");
                    OnPropertyChanged("FinalProgressIsBusy"); 
                }
            }
        }

        public bool CanDoAnything
        {
            get
            {
                return
                    (SourceProgress == null || !SourceProgress.IsBusy)
                    && (ViewboxProgress == null || !ViewboxProgress.IsBusy)
                    && (FinalProgress == null || !FinalProgress.IsBusy);
            }
        }

        public bool CanDoAnythingInSourceState
        {
            get
            {
                return CanDoAnything && SourceTables.Count != 0;
            }
        }

        public bool CanDoAnythingInViewboxState
        {
            get
            {
                return CanDoAnythingInSourceState && ViewboxTables.Count != 0;
            }
        }

        public bool CanDoAnythingInFinalState
        {
            get
            {
                return CanDoAnythingInViewboxState && FinalTables.Count != 0;
            }
        }

        public void UpdateRenaming()
        {
            OnPropertyChanged("ViewboxTables");
            OnPropertyChanged("FinalTables");
        }

        public bool IsMigrating { get; set; }
        public bool IsTransferring { get; set; }

        public bool SourceProgressIsNotBusy { get { return SourceProgress == null || !SourceProgress.IsBusy; } }
        public bool SourceProgressIsBusy { get { return SourceProgress != null && SourceProgress.IsBusy; } }
        public bool ViewboxProgressIsNotBusy { get { return ViewboxProgress == null || !ViewboxProgress.IsBusy; } }
        public bool ViewboxProgressIsBusy { get { return ViewboxProgress != null && ViewboxProgress.IsBusy; } }
        public bool FinalProgressIsNotBusy { get { return FinalProgress == null || !FinalProgress.IsBusy; } }
        public bool FinalProgressIsBusy { get { return FinalProgress != null && FinalProgress.IsBusy; } }

        void UpdateCanDoProperties()
        {
            OnPropertyChanged("CanDoAnything");
            OnPropertyChanged("CanDoAnythingInSourceState");
            OnPropertyChanged("CanDoAnythingInViewboxState");
            OnPropertyChanged("CanDoAnythingInFinalState");
        }

        public List<OptimizationTypeExt> OptimizationTypeTexts { get; set; }

        public List<ViewboxDb.RowNoFilters.Parser.OperatorsExt> OperatorsTexts { get; set; }

        private string _sourceError;
        public string SourceError
        {
            get { return _sourceError; }
            set
            {
                if (value != _sourceError)
                {
                    _sourceError = value;
                    OnPropertyChanged("SourceError");
                    OnPropertyChanged("HasSourceError");
                }
            }
        }

        public bool HasSourceError { get { return !String.IsNullOrEmpty(SourceError); } }


        private string _sourceWarning;
        public string SourceWarning
        {
            get { return _sourceWarning; }
            set
            {
                if (value != _sourceWarning)
                {
                    _sourceWarning = value;
                    OnPropertyChanged("SourceWarning");
                    OnPropertyChanged("HasSourceWarning");
                }
            }
        }

        public bool HasSourceWarning { get { return !String.IsNullOrEmpty(SourceWarning); } }


        private string _viewboxError;
        public string ViewboxError
        {
            get { return _viewboxError; }
            set
            {
                if (value != _viewboxError)
                {
                    _viewboxError = value;
                    OnPropertyChanged("ViewboxError");
                    OnPropertyChanged("HasViewboxError");
                }
            }
        }

        public bool HasViewboxError { get { return !String.IsNullOrEmpty(ViewboxError); } }



        private string _viewboxWarning;
        public string ViewboxWarning
        {
            get { return _viewboxWarning; }
            set
            {
                if (value != _viewboxWarning)
                {
                    _viewboxWarning = value;
                    OnPropertyChanged("ViewboxWarning");
                    OnPropertyChanged("HasViewboxWarning");
                }
            }
        }

        public bool HasViewboxWarning { get { return !String.IsNullOrEmpty(ViewboxWarning); } }



        private string _finalError;
        public string FinalError
        {
            get { return _finalError; }
            set
            {
                if (value != _finalError)
                {
                    _finalError = value;
                    OnPropertyChanged("FinalError");
                    OnPropertyChanged("HasFinalError");
                }
            }
        }

        public bool HasFinalError { get { return !String.IsNullOrEmpty(FinalError); } }

        public LocalizationTextsSettingsModel LocalisationSettings { get; private set; }

        public RenamerSettingsModel RenamerSettings { get; private set; }

        #endregion Properties

        #region Connections

        public void InitConnections()
        {
            if (ShowNotSelectedError())
                return;
            LoadSourceData();
            LoadViewboxData();
            LoadFinalData();
        }

        #region SourceData

        public void LoadSourceData()
        {
            if (ShowNotSelectedError()) return;
            SourceProgress = new ProgressCalculator { WorkerSupportsCancellation = true };
            SourceProgress.DoWork += LoadSourceDataDoWork;
            SourceProgress.RunWorkerCompleted += SourceProgress_RunWorkerCompleted;
            SourceProgress.RunWorkerAsync();
        }

        private void ClearSourceInfos(bool clearMessages, bool clearTables)
        {
            IsInitializedSource = false;
            TableCollection.ClearSourceTables();
            if(clearTables)
            {
                SourceTables.Clear();
                _previousSourceTables.Clear();
            }

            if (!clearMessages) return;
            SourceError = "";
            SourceWarning = "";
        }

        void SourceProgress_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null || e.Cancelled || SourceProgress.CancellationPending)
            {
                SourceProgress.Description = "Operation cancelled";
                SourceError = e.Error == null ? "Operation cancelled" : e.Error.ToString();
                ClearSourceInfos(false, true);
                if (e.Error != null)
                    _logger.Error("Problem with loading source data", e.Error);
                else
                    _logger.Info("Loading source data cancelled");
            }
            else
            {
                IsInitializedSource = true;
                if (SourceTables.Any(w => w.HasSourceError))
                {
                    SourceError = "There are tables with errors";
                }
                CheckTables();
            }
        }

        private void LoadSourceDataDoWork(object sender, DoWorkEventArgs e)
        {
            ClearSourceInfos(true, false);
            try
            {
                CurrentProfile.SourceConnection.TestConnection();
            }
            catch (Exception ex)
            {
                throw new Exception("Problem with the connection", ex);
            }

            var mandt = CurrentProfile.DefaultMandtCols;
            var bukrs = CurrentProfile.DefaultBukrsCols;
            var gjahr = CurrentProfile.DefaultGjahrCols;
            try
            {
                var progress = sender as ProgressCalculator;
                if (progress == null)
                    return;

                using (var conn = CurrentProfile.SourceConnection.CreateConnection())
                {
                    var tables = conn.GetTableList();

                    progress.SetWorkSteps(tables.Count, false);
                    progress.SetStep(0);
                    foreach (var tableName in tables)
                    {
                        progress.Description = "Loading source Database: " + tableName;
                        TableModel table = null;
                        try
                        {
                            table = TableCollection.GetTable(tableName);
                            TableCollection.AddSourceTable(table);

                            if (CurrentProfile.HideRowCounts)
                                table.SourceRowCount = 0;
                            else
                                table.SourceRowCount = conn.GetRowCount(tableName);

                            if (!ValidateTableOrColumnName(table.Name))
                            {
                                table.SourceError = ResourcesCommon.ContainsSpecCharOrWhiteSpace;
                            }
                            else
                                table.SourceError = null;

                            foreach (var columnInfo in conn.GetColumnInfos(tableName))
                            {
                                var column = table.GetColumn(columnInfo.Name);
                                column.SourceInfo = columnInfo;
                                table.AddSourceColumn(column);

                                column.RenamerClicked -= column_RenamerClicked;
                                column.RenamerClicked += column_RenamerClicked;

                                if (!ValidateTableOrColumnName(column.Name))
                                {
                                    column.SourceError = ResourcesCommon.ContainsSpecCharOrWhiteSpace;
                                }
                                else
                                    column.SourceError = null;
                            }

                            foreach (var man in mandt)
                            {
                                var col = table.SourceColumns.FirstOrDefault(w => w.Name == man);
                                if (col != null)
                                {
                                    col.SourceOptimizationType = OptimizationType.IndexTable;
                                    break;
                                }
                            }
                            foreach (var buk in bukrs)
                            {
                                var col = table.SourceColumns.FirstOrDefault(w => w.Name == buk);
                                if (col != null)
                                {
                                    col.SourceOptimizationType = OptimizationType.SplitTable;
                                    break;
                                }
                            }
                            foreach (var gj in gjahr)
                            {
                                var col = table.SourceColumns.FirstOrDefault(w => w.Name == gj);
                                if (col != null)
                                {
                                    col.SourceOptimizationType = OptimizationType.SortColumn;
                                    break;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.Error("Problem during loading source Database: " + tableName, ex);
                            if (table != null)
                                table.SourceError = ex.ToString();
                        }

                        if (SourceProgress.CancellationPending)
                        {
                            e.Cancel = true;
                            return;
                        }

                        progress.StepDone();
                    }
                }
                _previousSourceTables.AddRange(SourceTables.Where(w => w.SourceChecked));
                SourceTables.Clear();
                SourceTables.AddRange(TableCollection.SourceTables.Values.OrderBy(w => w.Name));
                OnPropertyChanged("ViewboxTables");
                OnPropertyChanged("FinalTables");
                progress.Description = "Ready";
                progress.SetStep(0);
            }
            catch (Exception ex)
            {
                throw new Exception("Error during LoadSourceData", ex);
            }
        }

        void column_RenamerClicked(object sender, IRenameable model)
        {
            OnRenamerClicked(sender, model);
        }

        #endregion SourceData

        #region ViewboxData

        public void LoadViewboxData()
        {
            if (ShowNotSelectedError()) return;
            ViewboxProgress = new ProgressCalculator { WorkerSupportsCancellation = true };
            ViewboxProgress.DoWork += LoadViewboxDataDoWork;
            ViewboxProgress.RunWorkerCompleted += ViewboxProgress_RunWorkerCompleted;
            ViewboxProgress.RunWorkerAsync();
        }

        private void ClearViewboxInfos(bool clearMessages, bool clearTables)
        {
            IsInitializedViewbox = false;
            TableCollection.ClearViewboxTables();
            if (clearTables)
            {
                ViewboxTables.Clear();
                _previousViewboxTables.Clear();
            }

            if (!clearMessages) return;
            ViewboxError = "";
            ViewboxWarning = "";
        }

        void ViewboxProgress_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null || e.Cancelled || ViewboxProgress.CancellationPending)
            {
                ViewboxProgress.Description = "Operation cancelled";
                ViewboxError = e.Error == null ? "Operation cancelled" : e.Error.ToString();
                ClearViewboxInfos(false, true);
                if (e.Error != null)
                    _logger.Error("Problem with loading viewbox data", e.Error);
                else
                    _logger.Info("Loading viewbox data cancelled");
            }
            else
            {
                IsInitializedViewbox = true;
                if (ViewboxTables.Any(w => w.HasViewboxError))
                {
                    ViewboxError = "There are tables with errors";
                }
                CheckTables();
            }
        }

        private void LoadViewboxDataDoWork(object sender, DoWorkEventArgs e)
        {
            ClearViewboxInfos(true, false);
            try
            {
                var progress = sender as ProgressCalculator;
                if (progress == null)
                {
                    e.Cancel = true;
                    return;
                }

                try
                {
                    CurrentProfile.ViewboxConnection.CreateDbIfNotExists();
                    CurrentProfile.ViewboxConnection.TestConnection();
                }
                catch (Exception ex)
                {
                    throw new Exception("Problem with viewbox connection", ex);
                }

                if (!ConnectSystemDb(progress))
                {
                    e.Cancel = true;
                    return;
                }

                progress.Description = "Setting up Database informations";
                var tables = _systemDb.Tables.Where(w => w.Type == TableType.Table && w.Database.ToLower() == CurrentProfile.FinalConnection.DbName).ToList();
                progress.SetWorkSteps(tables.Count, false);
                progress.SetStep(0);
                foreach (var tableinfo in tables)
                {
                    var tableObject = tableinfo as Table;
                    progress.Description = "Loading viewbox Database (2/2): " + tableinfo.TableName;
                    TableModel table = null;
                    try
                    {
                        table = TableCollection.GetTable(tableinfo.TableName);

                        if (CurrentProfile.HideRowCounts)
                            table.ViewboxRowCount = 0;
                        else
                            table.ViewboxRowCount = tableinfo.RowCount;

                        table.ViewboxTable = tableObject;
                        TableCollection.AddViewboxTable(table);

                        if (!ValidateTableOrColumnName(table.Name))
                        {
                            table.ViewboxError = ResourcesCommon.ContainsSpecCharOrWhiteSpace;
                        }

                        foreach (var columnInfo in tableinfo.Columns)
                        {
                            var column = table.GetColumn(columnInfo.Name);
                            column.ViewboxInfo = columnInfo as Column;
                            table.AddViewboxColumn(column);
                            column.ConfigureLocalisationTextsClicked -= column_ConfigureLocalisationTextsClicked;
                            column.ConfigureLocalisationTextsClicked += column_ConfigureLocalisationTextsClicked;

                            if (!ValidateTableOrColumnName(column.Name))
                            {
                                table.ViewboxError = ResourcesCommon.ContainsSpecCharOrWhiteSpace;
                            }
                        }
                        progress.StepDone();
                    }
                    catch (Exception ex)
                    {
                        _logger.Error("Problem during loading source Database: " + tableinfo.TableName, ex);
                        if (table != null)
                            table.ViewboxError = ex.ToString();
                    }

                    if (ViewboxProgress.CancellationPending)
                    {
                        e.Cancel = true;
                        return;
                    }

                }
                _previousViewboxTables.AddRange(_viewboxTables.Where(w => w.ViewboxChecked));
                _viewboxTables.Clear();
                _viewboxTables.AddRange(TableCollection.ViewboxTables.Values.OrderBy(w => w.Name));
                OnPropertyChanged("ViewboxTables");
                OnPropertyChanged("FinalTables");
                progress.SetStep(0);
                progress.Description = "Ready";
            }
            catch (Exception ex)
            {
                throw new Exception("Problem with LoadViewboxData", ex);
            }
        }

        void column_ConfigureLocalisationTextsClicked(object sender, IViewboxLocalisable model)
        {
            OnConfigureLocalisationTextsClicked(sender, model);
        }


        public bool ConnectSystemDb(ProgressCalculator progress)
        {
            try
            {
                _isInitializedSystemDb = false;
                if (_systemDb != null)
                {
                    _systemDb.Dispose();
                    _systemDb = null;
                }
                progress.SetWorkSteps(Enum.GetNames(typeof(SystemDb.SystemDb.Part)).Length, false);
                progress.SetStep(0);
                _systemDb = new SystemDb.SystemDb();
                _systemDb.SystemDbInitialized += systemDb_SystemDbInitialized;
                _systemDb.Connect(CurrentProfile.ViewboxConnection.DbType, CurrentProfile.ViewboxConnection.ConnectionString, 1000);
                _systemDb.PartLoadingCompleted += _systemDb_PartLoadingCompleted;
                while (!_isInitializedSystemDb)
                {
                    Thread.Sleep(100);
                }
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Problem with loading viewbox data", ex);
            }
        }

        void _systemDb_PartLoadingCompleted(SystemDb.SystemDb sender, SystemDb.SystemDb.Part part)
        {
            ViewboxProgress.Description = "Loading viewbox database (1/2): " + part;
            ViewboxProgress.StepDone();
            if (ViewboxProgress.CancellationPending)
            {
                ViewboxProgress.Description = "Operation cancelled";
                throw new Exception("Loading cancelled");
            }
        }

        private void systemDb_SystemDbInitialized(object sender, EventArgs e)
        {
            using (IDatabase db = _systemDb.ConnectionManager.GetConnection())
            {
                if (!db.DatabaseExists(CurrentProfile.ViewboxConnection.DbName) || !db.TableExists("users"))
                {
                    _systemDb.LoadTables(db);
                }

                var databaseOutOfDateInformation = _systemDb.HasDatabaseUpgrade(db);
                if (databaseOutOfDateInformation != null)
                {
                    _systemDb.UpgradeDatabase(db);
                    _systemDb.LoadTables(db);
                }
            }

            _isInitializedSystemDb = true;
        }

        #endregion ViewboxData

        #region FinalData

        public void LoadFinalData()
        {
            if (ShowNotSelectedError()) return;
            FinalProgress = new ProgressCalculator { WorkerSupportsCancellation = true };
            FinalProgress.DoWork += LoadFinalDataDoWork;
            FinalProgress.RunWorkerCompleted += FinalProgress_RunWorkerCompleted;
            FinalProgress.RunWorkerAsync();
        }

        private void ClearFinalInfos(bool clearMessages)
        {
            IsInitializedFinal = false;
            TableCollection.ClearFinalTables();
            _finalTables.Clear();

            if (!clearMessages) return;
            FinalError = "";
        }


        void FinalProgress_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null || e.Cancelled || FinalProgress.CancellationPending)
            {
                FinalProgress.Description = "Operation cancelled";
                FinalError = e.Error == null ? "Operation cancelled" : e.Error.ToString();
                ClearFinalInfos(false);
                if (e.Error != null)
                    _logger.Error("Problem with loading final data", e.Error);
                else
                    _logger.Info("Loading final data cancelled");
            }
            else
            {
                IsInitializedFinal = true;
                if (FinalTables.Any(w => w.HasFinalError))
                {
                    FinalError = "There are tables with errors";
                }
                CheckTables();
            }
        }


        private void LoadFinalDataDoWork(object sender, DoWorkEventArgs e)
        {
            ClearFinalInfos(true);
            var progress = sender as ProgressCalculator;
            if (progress == null)
                return;

            try
            {
                CurrentProfile.FinalConnection.CreateDbIfNotExists();
                CurrentProfile.FinalConnection.TestConnection();
            }
            catch (Exception ex)
            {
                throw new Exception("Problem with final connection", ex);
            }

            try
            {
                using (var conn = CurrentProfile.FinalConnection.CreateConnection())
                {
                    var tables = conn.GetTableList();
                    progress.SetWorkSteps(tables.Count, false);
                    progress.SetStep(0);
                    progress.Description = "";
                    foreach (var tableName in tables)
                    {
                        TableModel table = null;
                        try
                        {
                            progress.Description = "Loading final Database: " + tableName;
                            table = TableCollection.GetTable(tableName);

                            if (CurrentProfile.HideRowCounts)
                                table.FinalRowCount = 0;
                            else
                                table.FinalRowCount = conn.GetRowCount(tableName);

                            TableCollection.AddFinalTable(table);

                            foreach (var columnInfo in conn.GetColumnInfos(tableName))
                            {
                                var column = table.GetColumn(columnInfo.Name);
                                column.FinalInfo = columnInfo;
                                table.AddFinalColumn(column);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.Error("Problem during loading source Database: " + tableName, ex);
                            if (table != null)
                                table.FinalError = ex.ToString();
                        }
                        progress.StepDone();
                        if (FinalProgress.CancellationPending)
                        {
                            e.Cancel = true;
                            return;
                        }
                    }
                }

                _finalTables.AddRange(TableCollection.FinalTables.Values.OrderBy(w => w.Name));
				OnPropertyChanged("ViewboxTables");
                OnPropertyChanged("FinalTables");
                progress.Description = "Ready";
                progress.SetStep(0);
            }
            catch (Exception ex)
            {
                throw new Exception("Problem with loading final data", ex);
            }
        }

        #endregion FinalData

        #endregion Connections

        #region Cancel

        public void CancelSource()
        {
            lock (SourceProgress)
            {
                if (SourceProgress != null && SourceProgress.IsBusy)
                    SourceProgress.CancelAsync();
            }
        }

        public void CancelViewbox()
        {
            lock (ViewboxProgress)
            {
                if (ViewboxProgress != null && ViewboxProgress.IsBusy)
                    ViewboxProgress.CancelAsync();
            }
        }

        public void CancelFinal()
        {
            lock (FinalProgress)
            {
                if (FinalProgress != null && FinalProgress.IsBusy)
                    FinalProgress.CancelAsync();
            }
        }

        #endregion Cancel

        #region CheckTables

        private readonly object _checkTableLock = new object();
        public bool DisableCheckEvents { get; set; }

        public void CheckTables()
        {
            if (DisableCheckEvents)
                return;
            lock (_checkTableLock)
            {
                DisableCheckEvents = true;
                try
                {
                    SourceWarning = "";
                    ViewboxWarning = "";
                    var hasSourceChecked = false;
                    var hasViewboxChecked = false;
                    for (var i = 0; i < TableCollection.Tables.Values.Count(); ++i)
                    {
                        var table = TableCollection.Tables.Values.ElementAt(i);
                        if (table.IsInSource)
                        {
                            table.SourceWarning = "";
                            table.SourceChecked = !table.IsInViewbox || table.SourceViewboxDiff();
                            if (table.SourceChecked)
                                table.SourceWarning = "Database need to be migrated to viewbox";
                            table.SourceChecked |= _previousSourceTables.Count(w => w.Name == table.Name) != 0;
                            hasSourceChecked |= table.SourceChecked;
                            _previousSourceTables.Clear();
                        }

                        if (table.IsInViewbox)
                        {
                            table.ViewboxWarning = "";
                            table.ViewboxChecked = !table.IsInFinal || table.ViewboxFinalDiff();
                            if (table.ViewboxChecked)
                                table.ViewboxWarning = "Database need to be transfered to final";
                            table.ViewboxChecked |= _previousViewboxTables.Count(w => w.Name == table.Name) != 0;
                            hasViewboxChecked |= table.ViewboxChecked;
                            _previousViewboxTables.Clear();
                        }
                    }
                    if (hasSourceChecked)
                        SourceWarning = "Tables need to be migrated";
                    if (hasViewboxChecked)
                        ViewboxWarning = "Tables need to be transfer";
                }
                finally
                {
                    DisableCheckEvents = false;
                }
            }
            UpdateCanDoProperties();
            SetAllSelects();
        }

        public void SetAllSelects()
        {
            if (DisableCheckEvents)
                return;
            lock (_checkTableLock)
            {
                DisableCheckEvents = true;
                try
                {
                    if (SourceTables.All(w => w.SourceChecked))
                        SelectAllSource = true;
                    else if (SourceTables.All(w => !w.SourceChecked))
                        SelectAllSource = false;
                    else
                        SelectAllSource = null;

                    if (ViewboxTables.All(w => w.ViewboxChecked))
                        SelectAllViewbox = true;
                    else if (ViewboxTables.All(w => !w.ViewboxChecked))
                        SelectAllViewbox = false;
                    else
                        SelectAllViewbox = null;
                }
                finally
                {
                    DisableCheckEvents = false;
                }
            }
        }

        #endregion CheckTables

        #region Migrate

        public void MigrateViewboxMetadata(bool withOptimization)
        {
            if (ShowNotSelectedError()) return;
            SourceProgress = new ProgressCalculator { WorkerSupportsCancellation = true };
            SourceProgress.DoWork += MigrateViewboxMetadataDoWork;
            SourceProgress.RunWorkerCompleted += MigrateProgress_RunWorkerCompleted;
            SourceProgress.RunWorkerAsync(withOptimization);
        }

        void MigrateProgress_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null || e.Cancelled || SourceProgress.CancellationPending)
            {
                SourceProgress.Description = "Operation cancelled";
                SourceError = e.Error == null ? "Operation cancelled" : e.Error.ToString();
                if (e.Error != null)
                    _logger.Error("Problem with loading source data", e.Error);
                else
                    _logger.Error("Migrating cancelled");
            }
            else
            {
                if (SourceTables.Any(w => w.HasSourceError))
                {
                    SourceError = "There are tables with errors";
                }
            }
        }

        public void MigrateViewboxMetadataDoWork(object sender, DoWorkEventArgs e)
        {
            SourceError = "";
            var withOptimization = (bool)e.Argument;
            var progress = sender as ProgressCalculator;
            if (progress == null)
                return;
            try
            {
                using (var conn = _systemDb.ConnectionManager.GetConnection())
                {
                    var maxTableOrdinal = _systemDb.GetMaxTableObjectOrdinal(TableType.Table);

                    foreach (var table in TableCollection.SourceTables.Values)
                    {
                        table.SourceProgress = new ProgressCalculator { WorkerSupportsCancellation = true };
                    }

                    var tables = TableCollection.SourceTables.Values.Where(w => w.SourceChecked).ToList();

                    progress.SetWorkSteps(tables.Count, false);
                    progress.SetStep(0);
                    if (SourceProgress.CancellationPending)
                    {
                        e.Cancel = true;
                        return;
                    }
                    foreach (var table in tables)
                    {
                        try
                        {
                            if (SourceProgress.CancellationPending)
                            {
                                table.SourceProgress.Description = "Cancelled";
                                e.Cancel = true;
                                return;
                            }

                            progress.Description = "Migrating source metadata to viewbox database: " + table.Name;
                            table.SourceProgress.SetWorkSteps(3 + table.SourceColumns.Count, false);
                            table.SourceProgress.SetStep(0);
                            table.SourceProgress.Description = "Migrating Database";

                            table.ViewboxTable = table.ViewboxTable ?? new Table();

                            table.ViewboxTable.Ordinal = table.ViewboxTable.Ordinal == 0 ? ++maxTableOrdinal : table.ViewboxTable.Ordinal;
                            table.ViewboxTable.TableName = table.Name;
                            table.ViewboxTable.Database = CurrentProfile.FinalConnection.DbName;
                            table.ViewboxTable.RowCount = table.SourceRowCount;
                            table.ViewboxTable.IsVisible = true;
                            table.ViewboxTable.CategoryId = 0;
                            table.ViewboxTable.DefaultSchemeId = 0;
                            table.ViewboxTable.Type = TableType.Table;
                            table.ViewboxTable.UserDefined = false;
                            table.ViewboxTable.IsArchived = false;
                            table.ViewboxTable.ObjectTypeCode = 0;

                            conn.DbMapping.Save(table.ViewboxTable);

                            table.SourceProgress.StepDone();
                            var columnOrdinal = 0;

                            // Add new columns
                            foreach (var sourceColumn in table.SourceColumns)
                            {
                                table.SourceProgress.Description = "Migrating column: " + sourceColumn.Name;

                                sourceColumn.ViewboxInfo = sourceColumn.ViewboxInfo ?? new Column();

                                sourceColumn.ViewboxInfo.Ordinal = columnOrdinal++;
                                sourceColumn.ViewboxInfo.Table = table.ViewboxTable;
                                sourceColumn.ViewboxInfo.Name = sourceColumn.Name;
                                sourceColumn.ViewboxInfo.MaxLength = sourceColumn.SourceInfo.MaxLength;
                                sourceColumn.ViewboxInfo.IsVisible = true;
                                sourceColumn.ViewboxInfo.IsEmpty = false;
                                sourceColumn.ViewboxInfo.UserDefined = false;
                                sourceColumn.ViewboxInfo.DataType = SqlTypeHelper.ColumnTypeToSqlType(sourceColumn.SourceInfo.Type);

                                if (withOptimization)
                                    sourceColumn.ViewboxInfo.OptimizationType = sourceColumn.SourceOptimizationType;

                                conn.DbMapping.Save(sourceColumn.ViewboxInfo);
                                if (!table.ViewboxTable.Columns.Contains(sourceColumn.ViewboxInfo.Id))
                                    table.ViewboxTable.Columns.Add(sourceColumn.ViewboxInfo);
                                table.SourceProgress.StepDone();
                            }


                            // Remove unneccessary columns
                            table.SourceProgress.Description = "Removing unnecessary columns";
                            foreach (var viewboxColumn in table.ViewboxColumns.Where(w => !w.IsInSource))
                            {
                                conn.DbMapping.Delete(viewboxColumn.ViewboxInfo);
                                if (!table.ViewboxTable.Columns.Contains(viewboxColumn.ViewboxInfo.Id))
                                    table.ViewboxTable.Columns.RemoveById(viewboxColumn.ViewboxInfo.Id);
                            }

                            table.SourceProgress.StepDone();

                            if (withOptimization)
                            {
                                table.SourceProgress.Description = "Removing unnecessary special columns";
                                foreach (var viewboxColumn in table.ViewboxSpecialColumns.Where(w => !w.IsInSource))
                                {
                                    conn.DbMapping.Delete(viewboxColumn.ViewboxInfo);
                                    if (!table.ViewboxTable.Columns.Contains(viewboxColumn.ViewboxInfo.Id))
                                        table.ViewboxTable.Columns.RemoveById(viewboxColumn.ViewboxInfo.Id);
                                }
                            }
                            table.SourceProgress.StepDone();
                            table.SourceProgress.Description = "Migrating ready";

                            progress.StepDone();
                        }
                        catch (Exception ex)
                        {
                            table.SourceError = "Problem with migrating Database" + ex;
                            _logger.Error("Problem with migrating Database: " + table.Name, ex);
                        }
                    }
                }
                LoadViewboxData();
                progress.Description = "Ready";
                progress.SetStep(0);
            }
            catch (Exception ex)
            {
                throw new Exception("Problem with migrating source metadata to viewbox", ex);
            }
        }

        #endregion Migrate

        #region Transfer

        private ConcurrentQueue<TableModel> _transferTables;
        private readonly List<BackgroundWorker> _workers = new List<BackgroundWorker>();

        public void TransferData()
        {
            if (ShowNotSelectedError()) return;
            ViewboxError = "";
            _transferTables = new ConcurrentQueue<TableModel>();
            foreach (var table in TableCollection.ViewboxTables.Values)
            {
                table.ViewboxProgress = new ProgressCalculator { WorkerSupportsCancellation = true };
                table.ViewboxError = "";
                if (table.ViewboxChecked)
                    _transferTables.Enqueue(table);
            }

            ViewboxProgress = new ProgressCalculator { WorkerSupportsCancellation = true };
            ViewboxProgress.SetWorkSteps(_transferTables.Count, false);
            ViewboxProgress.SetStep(0);

            ViewboxProgress.DoWork += TransferDataBigDoWork;
            ViewboxProgress.RunWorkerCompleted += workerBig_RunWorkerCompleted;

            _workers.Clear();
            for (var i = 0; i < CurrentProfile.ThreadsNumber; i++)
            {
                var worker = new BackgroundWorker { WorkerSupportsCancellation = true };
                worker.DoWork += TransferDataDoWork;
                worker.RunWorkerCompleted += worker_RunWorkerCompleted;
                _workers.Add(worker);
                worker.RunWorkerAsync();
            }

            ViewboxProgress.RunWorkerAsync();
        }

        public void TransferDataBigDoWork(object sender, DoWorkEventArgs e)
        {
            while (true)
            {
                if (ViewboxProgress.CancellationPending)
                {
                    e.Cancel = true;
                    lock (_workers)
                    {
                        foreach (var worker in _workers)
                        {
                            worker.CancelAsync();
                        }
                    }
                    return;
                }
                lock (_workers)
                {
                    if (!_workers.Any())
                        break;
                }
                Thread.Sleep(100);
            }

        }

        void workerBig_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (ViewboxProgress.CancellationPending || e.Cancelled)
                ViewboxProgress.Description = "Transfer cancelled";
            else
            {
                ViewboxProgress.Description = "Transfer ready";
                if (ViewboxProgress.IsBusy)
                    ViewboxProgress.SetStep(0);
            }

            if (TableCollection.ViewboxTables.Any(w => w.Value.HasViewboxError))
                ViewboxError = "Errors during Database transfers";
            LoadSourceData();
            LoadFinalData();
            ViewboxProgress = new ProgressCalculator { WorkerSupportsCancellation = true };
            ConnectSystemDb(ViewboxProgress);
            ViewboxProgress.Description = e.Cancelled ? "Transfer cancelled" : "Transfer ready";
        }

        void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            lock (_workers)
            {
                if (e.Error != null || e.Cancelled)
                {
                    ViewboxError += (e.Error == null ? "Operation cancelled" : e.Error.ToString());
                    if (e.Error != null)
                        _logger.Error("Problem with transfering", e.Error);
                    else
                        _logger.Info("Transferring cancelled");
                }

                _workers.Remove(sender as BackgroundWorker);
            }
        }

        public void TransferDataDoWork(object sender, DoWorkEventArgs e)
        {
            TableModel table;
            var watch = new Stopwatch();
            while (_transferTables.TryDequeue(out table))
            {
                if (!table.IsInSource)
                {
                    table.ViewboxError = "Not in the source database!";
                }
                else
                {
                    if (ViewboxProgress.CancellationPending)
                    {
                        e.Cancel = true;
                        table.ViewboxProgress.SetStep(0);
                        table.ViewboxProgress.Description = "Cancelled";
                        return;
                    }
                    watch.Reset();
                    watch.Start();
                    try
                    {
                        using (var sourceConn = CurrentProfile.SourceConnection.CreateConnection())
                        using (var viewboxConn = CurrentProfile.ViewboxConnection.CreateConnection())
                        using (var finalConn = CurrentProfile.FinalConnection.CreateConnection())
                        {
                            table.ViewboxProgress.SetWorkSteps(table.ViewboxRowCount + 8, false);
                            var tempName = (table.Name.Length <= 46 ? table.Name : table.Name.Substring(0, 46)) +
                                           "__temp_assistant__";
                            var opts = table.GetOptimizationColumns();
                            var optString = String.Join(",", opts);

                            var columns = table.SourceColumns.Select(w => w.SourceInfo).ToList();
                            foreach (var column in columns)
                            {
                                if (column.Type == DbColumnTypes.DbNumeric)
                                {
                                    column.MaxLength = 65 + 30;
                                    column.NumericScale = 30;
                                }
                            }
                            columns.AddRange(table.ViewboxSpecialColumns.Select(specialColumn =>
                                                                                    specialColumn.IsSpecialFormat ?

                                                                                (new DbColumnInfo
                                                                                    {
                                                                                        AllowDBNull = true,
                                                                                        Name = specialColumn.Name,
                                                                                        Type = DbColumnTypes.DbText,
                                                                                        MaxLength =
                                                                                            specialColumn.
                                                                                            FromColumnFormat.
                                                                                            Count(w => w == '1')
                                                                                    }) :
                                                                                    (new DbColumnInfo
                                                                                    {
                                                                                        AllowDBNull = true,
                                                                                        Name = specialColumn.Name,
                                                                                        Type = DbColumnTypes.DbDateTime
                                                                                    })
                                                                                    ));

                            if (ViewboxProgress.CancellationPending)
                            {
                                e.Cancel = true;
                                table.ViewboxProgress.SetStep(0);
                                table.ViewboxProgress.Description = "Cancelled";
                                return;
                            }

                            // ----------------- 1. Creating temp Database ---------------------- //
                            table.ViewboxProgress.Description = "Creating temp Database (1/8)";
                            table.ViewboxProgress.SetStep(0);

                            var tempColumns = CloneColumnListWithoutNumerics(columns);
                            CreateTable(table, finalConn, tempColumns, tempName);

                            if (ViewboxProgress.CancellationPending)
                            {
                                e.Cancel = true;
                                table.ViewboxProgress.SetStep(0);
                                table.ViewboxProgress.Description = "Cancelled";
                                return;
                            }

                            // -------------- 2. Transferring to temp Database ------------------ //
                            table.ViewboxProgress.Description = "Transferring to temp Database (2/8)";
                            table.ViewboxProgress.StepDone();

                            TranferToFinalTemp(table, sourceConn, finalConn, tempName, viewboxConn, e);

                            if (ViewboxProgress.CancellationPending)
                            {
                                e.Cancel = true;
                                table.ViewboxProgress.SetStep(0);
                                table.ViewboxProgress.Description = "Cancelled";
                                return;
                            }

                            // ------------ 3. Creating index on temp Database ----------------- //
                            table.ViewboxProgress.Description = "Creating index on temp Database (3/8)";
                            table.ViewboxProgress.StepDone();

                            /*if (opts.Count > 0)
                                finalConn.CreateIndex(tempName, tempName + "_opt_idx", opts);*/

                            if (ViewboxProgress.CancellationPending)
                            {
                                e.Cancel = true;
                                table.ViewboxProgress.SetStep(0);
                                table.ViewboxProgress.Description = "Cancelled";
                                return;
                            }

                            // ---------------- 4. Creating final Database ---------------------- //
                            table.ViewboxProgress.Description = "Creating final Database (4/8)";
                            table.ViewboxProgress.StepDone();

                            CreateTable(table, finalConn, columns);

                            if (ViewboxProgress.CancellationPending)
                            {
                                e.Cancel = true;
                                table.ViewboxProgress.SetStep(0);
                                table.ViewboxProgress.Description = "Cancelled";
                                return;
                            }

                            // --------------- 5. Reordering to final Database ------------------ //
                            table.ViewboxProgress.Description = "Reordering data (5/8)";
                            table.ViewboxProgress.StepDone();

                            var columnString = String.Join(",", columns.Select(w => finalConn.Enquote(w.Name)));

                            var sql = opts.Count > 0
                                          ? String.Format(InsertSqlWithOpt, finalConn.Enquote(table.Name),
                                                          columnString, finalConn.Enquote(tempName),
                                                          optString)
                                          : String.Format(InsertSql, finalConn.Enquote(table.Name),
                                                          columnString, finalConn.Enquote(tempName));
                            finalConn.ExecuteNonQuery(sql);

                            if (ViewboxProgress.CancellationPending)
                            {
                                e.Cancel = true;
                                table.ViewboxProgress.SetStep(0);
                                table.ViewboxProgress.Description = "Cancelled";
                                return;
                            }

                            // --------------- 6. Dropping temp Database ------------------ //
                            table.ViewboxProgress.Description = "Dropping temp Database (6/8)";
                            table.ViewboxProgress.StepDone();

                            finalConn.DropTableIfExists(tempName);

                            if (ViewboxProgress.CancellationPending)
                            {
                                e.Cancel = true;
                                table.ViewboxProgress.SetStep(0);
                                table.ViewboxProgress.Description = "Cancelled";
                                return;
                            }

                            // ------------ 7. Creating index on final Database ----------------- //
                            table.ViewboxProgress.Description = "Creating index on final Database (7/8)";
                            table.ViewboxProgress.StepDone();

                            if (opts.Count > 0)
                                finalConn.CreateIndex(table.Name, table.Name + "_opt_idx", opts);

                            if (ViewboxProgress.CancellationPending)
                            {
                                e.Cancel = true;
                                table.ViewboxProgress.SetStep(0);
                                table.ViewboxProgress.Description = "Cancelled";
                                return;
                            }

                            // --------------- 8. Generate order areas - final Database ------------------ //
                            table.ViewboxProgress.StepDone();
                            table.ViewboxProgress.Description = "Generate order areas (8/8)";

                            lock (this)
                            {
                                _systemDb.CalculateOrderArea2(table.ViewboxTable, optString);
                            }

                            if (ViewboxProgress.CancellationPending)
                            {
                                e.Cancel = true;
                                table.ViewboxProgress.SetStep(0);
                                table.ViewboxProgress.Description = "Cancelled";
                                return;
                            }

                            // ------------------------ DONE ------------------------------ //
                            watch.Stop();
                            table.ViewboxProgress.StepDone();
                            table.ViewboxProgress.Description = "Transfer ready (" +
                                                                watch.Elapsed.ToString(TimeSpanFormat) +
                                                                ")";
                        }
                    }
                    catch (Exception ex)
                    {
                        table.ViewboxError = "Problem with transferring: " + ex;
                        _logger.Error("Problem with transferring Database: " + table.Name, ex);
                    }
                }
                lock (ViewboxProgress)
                {

                    if (String.IsNullOrEmpty(table.ViewboxError))
                        ViewboxProgress.Description = "Database transfer ready: " + table.Name + " (" +
                                                      watch.Elapsed.ToString(TimeSpanFormat) + ")";
                    else
                        ViewboxProgress.Description = "Database transfer error: " + table.Name + " (" +
                                                      watch.Elapsed.ToString(TimeSpanFormat) + ")";
                    ViewboxProgress.StepDone();
                }
            }
        }

        private List<DbColumnInfo> CloneColumnListWithoutNumerics(IEnumerable<DbColumnInfo> columns)
        {
            var tempColumns = columns.Select(column => column.Clone()).ToList();
            foreach (var tempColumn in tempColumns.Where(tempColumn => tempColumn.Type == DbColumnTypes.DbNumeric))
            {
                tempColumn.Type = DbColumnTypes.DbText;
            }
            return tempColumns;
        }

        private void CreateTable(TableModel table, IDatabase conn, List<DbColumnInfo> columnsToTransfer, string name = "")
        {
            List<DbColumnInfo> columns;
            if (String.IsNullOrEmpty(name))
            {
                name = table.Name;
                columns = columnsToTransfer.ToList();
                columns.Add(new DbColumnInfo
                {
                    AllowDBNull = false,
                    AutoIncrement = true,
                    IsPrimaryKey = true,
                    Name = "_row_no_",
                    Type = DbColumnTypes.DbBigInt
                });
            }
            else
                columns = columnsToTransfer;
            conn.DropTableIfExists(name);
            conn.CreateTable(name, columns);
        }

        private void TranferToFinalTemp(TableModel table, IDatabase sourceConn, IDatabase finalConn, string tempName, IDatabase viewboxConn, DoWorkEventArgs e)
        {
            try
            {
                var columnNames = table.SourceColumns.Select(w => w.Name.ToLower()).ToList();

                var selectSql = String.Format(SelectSql, String.Join(",", columnNames.Select(sourceConn.Enquote)),
                                                 sourceConn.Enquote(table.Name));
                using (var reader = sourceConn.ExecuteReader(selectSql))
                {
                    // ------------------------  Preparing  --------------------------------- //
                    var builder = new StringBuilder();
                    var values = new List<DbColumnValues>();

                    // ------------------------  Preparing analyse columns --------------------------------- //
                    var sourceColumnsToAnalyse = table.SourceColumnDict
                        .Where(w => w.Value.SourceInfo.Type == DbColumnTypes.DbNumeric || w.Value.SourceInfo.Type == DbColumnTypes.DbText)
                        .ToDictionary(w => w.Key, w => w.Value);
                    foreach (var sourceColumn in sourceColumnsToAnalyse.Values)
                    {
                        if (sourceColumn.SourceInfo.Type == DbColumnTypes.DbNumeric)
                        {
                            sourceColumn.SourceInfo.MaxLength = 0;
                            sourceColumn.SourceInfo.NumericScale = 0;
                        }
                        if (sourceColumn.SourceInfo.Type == DbColumnTypes.DbText)
                        {
                            sourceColumn.SourceInfo.MaxLength = 0;
                        }
                    }

                    // ------------------------  Preparing special columns --------------------------------- //
                    var specialColumns = table.ViewboxSpecialColumns.Where(w => w.IsSpecialFormat).ToDictionary(col => col.ViewboxInfo,
                                                                                  col => col.ViewboxInfo.FromColumnIndexes());
                    var specialDateColumns = table.ViewboxSpecialColumns.Where(w => !w.IsSpecialFormat).ToList();

                    foreach (var specialColumn in specialColumns.Keys.ToList())
                    {
                        if (columnNames.All(w => w != specialColumn.FromColumn.ToLower()))
                            specialColumns.Remove(specialColumn);
                    }

                    foreach (var specialDateColumn in specialDateColumns)
                    {
                        if (columnNames.All(w => w != specialDateColumn.FromColumn.ToLower()))
                            specialDateColumns.Remove(specialDateColumn);
                    }

                    // --------------------------  Transfer --------------------------------- //
                    while (reader.Read())
                    {
                        var dbValues = new DbColumnValues();
                        for (var i = 0; i < reader.FieldCount; i++)
                        {
                            var columnName = columnNames[i];
                            var value = reader.GetValue(i);
                            dbValues[columnName] = value;

                            if (value == null) continue;

                            //  --------------- Analyse Numeric, and Text types ---------------
                            var strVal = value.ToString();
                            if (!sourceColumnsToAnalyse.ContainsKey(columnName))
                                continue;
                            var sourceColumn = sourceColumnsToAnalyse[columnName];
                            switch (sourceColumn.SourceInfo.Type)
                            {
                                case DbColumnTypes.DbNumeric:
                                    {
                                        var eIndex = strVal.IndexOf('E');
                                        if (eIndex != -1)
                                        {
                                            Int32 numStr;
                                            var b = Int32.TryParse(strVal.Substring(eIndex + 1), out numStr);
                                            if (!b || Math.Abs(numStr) > 307)
                                            {
                                                sourceColumn.SourceInfo.MaxLength =
                                                    Math.Max(
                                                        sourceColumn.SourceInfo.MaxLength + sourceColumn.SourceInfo.NumericScale + 1, strVal.Length);
                                                sourceColumn.SourceInfo.Type = DbColumnTypes.DbText;
                                                sourceColumn.ViewboxInfo.DataType = SqlType.String;
                                                viewboxConn.DbMapping.Save(sourceColumn.ViewboxInfo);
                                            }
                                            else
                                            {
                                                sourceColumn.SourceInfo.MaxLength = Math.Max(sourceColumn.SourceInfo.MaxLength, 1);
                                                sourceColumn.SourceInfo.NumericScale =
                                                    Math.Max(sourceColumn.SourceInfo.NumericScale, Math.Abs(numStr));
                                            }
                                        }
                                        else
                                        {
                                            var sepIndex = strVal.IndexOf(_numberSep);
                                            if (sepIndex == -1)
                                                sourceColumn.SourceInfo.MaxLength =
                                                    Math.Max(sourceColumn.SourceInfo.MaxLength, strVal.Length);
                                            else
                                            {
                                                strVal = strVal.TrimEnd('0');
                                                sourceColumn.SourceInfo.MaxLength =
                                                    Math.Max(sourceColumn.SourceInfo.MaxLength, sepIndex);
                                                sourceColumn.SourceInfo.NumericScale =
                                                    Math.Max(sourceColumn.SourceInfo.NumericScale,
                                                             strVal.Length - sepIndex - 1);
                                            }
                                        }
                                    }
                                    break;
                                case DbColumnTypes.DbText:
                                    sourceColumn.SourceInfo.MaxLength = Math.Max(sourceColumn.SourceInfo.MaxLength,
                                                                                 strVal.Length);
                                    break;
                            }
                        }

                        // Convert special columns
                        foreach (var specialColumn in specialColumns)
                        {
                            var fromValue = dbValues[specialColumn.Key.FromColumn.ToLower()];
                            if (fromValue == null)
                                dbValues[specialColumn.Key.Name] = null;
                            else
                            {

                                builder.Clear();
                                var valuestr = fromValue.ToString();
                                foreach (var index in specialColumn.Value)
                                {
                                    if (valuestr.Length > index)
                                    {
                                        builder.Append(valuestr[index]);
                                    }
                                }
                                dbValues[specialColumn.Key.Name] = builder.ToString();
                            }
                        }

                        foreach (var specialDateColumn in specialDateColumns)
                        {
                            var fromValue = dbValues[specialDateColumn.FromColumn.ToLower()];
                            if (fromValue == null)
                                dbValues[specialDateColumn.Name] = null;
                            else
                            {
                                var valuestr = fromValue.ToString();
                                DateTime res;
                                if (DateTime.TryParseExact(valuestr, specialDateColumn.FromColumnFormat, null, DateTimeStyles.None, out res))
                                    dbValues[specialDateColumn.Name] = res;
                                else
                                    dbValues[specialDateColumn.Name] = null;
                            }
                        }

                        values.Add(dbValues);

                        if (values.Count < 99) continue;

                        if (ViewboxProgress.CancellationPending)
                        {
                            e.Cancel = true;
                            table.ViewboxProgress.SetStep(0);
                            table.ViewboxProgress.Description = "Cancelled";
                            return;
                        }

                        finalConn.InsertInto(finalConn.DbConfig.DbName, tempName, values);
                        table.ViewboxProgress.StepsDone(values.Count);
                        values.Clear();
                    }

                    if (values.Count > 0)
                    {
                        finalConn.InsertInto(finalConn.DbConfig.DbName, tempName, values);
                        table.ViewboxProgress.StepsDone(values.Count);
                        values.Clear();
                    }

                    foreach (var sourceColumn in sourceColumnsToAnalyse.Values)
                    {
                        switch (sourceColumn.SourceInfo.Type)
                        {
                            case DbColumnTypes.DbNumeric:
                                sourceColumn.SourceInfo.MaxLength = sourceColumn.SourceInfo.MaxLength +
                                                              2 * sourceColumn.SourceInfo.NumericScale;
                                if (sourceColumn.SourceInfo.MaxLength == 0)
                                    sourceColumn.SourceInfo.MaxLength = 1;
                                if (sourceColumn.ViewboxInfo != null)
                                {
                                    sourceColumn.ViewboxInfo.MaxLength = sourceColumn.SourceInfo.NumericScale;
                                    viewboxConn.DbMapping.Save(sourceColumn.ViewboxInfo);
                                }
                                break;
                            case DbColumnTypes.DbText:
                                if (sourceColumn.SourceInfo.MaxLength == 0)
                                    sourceColumn.SourceInfo.MaxLength = 1;
                                if (sourceColumn.ViewboxInfo != null)
                                {
                                    sourceColumn.ViewboxInfo.MaxLength = sourceColumn.SourceInfo.MaxLength;
                                    viewboxConn.DbMapping.Save(sourceColumn.ViewboxInfo);
                                }
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Problem while transferring Database: " + table.Name, ex);
            }
        }

        #endregion Transfer

        #region Data Preview

        private DataPreviewModel DataPreview(TableModel table, IDatabase conn)
        {
            using (var reader = conn.ExecuteReader(String.Format("SELECT * FROM {0}", conn.Enquote(table.Name))))
            {
                var dataTable = new DataPreviewModel();
                var temp = 0;

                while (reader.Read() && temp < PreviewRecordCount)
                {
                    if (dataTable.Columns.Count == 0)
                    {
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            dataTable.Columns.Add(reader.GetName(i));
                        }
                    }
                    var dbValues = new DbColumnValues();
                    var values = new List<String>();
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        dbValues[dataTable.Columns[i]] = reader.GetValue(i);
                        values.Add(reader.GetValue(i).ToString());
                    }

                    dataTable.Rows.Add(values);
                    temp++;
                }
                return dataTable;
            }
        }

        #endregion Data Preview

        public void GenerateScripts(string path)
        {
            var selectedViewBoxTables = SelectedViewBoxTables();
            var savedViewBoxTables = new List<TableModel>();
            var messageSb = new StringBuilder();

            using (var conn = CurrentProfile.FinalConnection.CreateConnection())
            {
                foreach (var table in selectedViewBoxTables)
                {
                    var sb = new StringBuilder();
                    sb.Append(path);
                    sb.Append("\\");
                    sb.Append(table.Name);
                    sb.Append("_view.sql");
                    var generationModel = new ScriptGenerationModel
                                              {
                                                  Path = sb.ToString(),
                                                  Table = table,
                                                  Languages = this.Languages,
                                              };
                    var isError = false;

                    generationModel.ScriptFileGenerationFinished +=
                        (sender, status) =>
                        {
                            savedViewBoxTables.Add(table);
                            if (status == GenerationStatus.Error)
                            {
                                isError = true;
                                messageSb.Append(Environment.NewLine);
                                messageSb.Append(generationModel.Path);
                            }
                            if (selectedViewBoxTables.Count() == savedViewBoxTables.Count)
                            {
                                if (messageSb.Length != 0)
                                {
                                    messageSb.Insert(0, Environment.NewLine);
                                    messageSb.Insert(0, ResourcesCommon.CreatingFileError);
                                    messageSb.Insert(0, Environment.NewLine);
                                    messageSb.Insert(0, Environment.NewLine);
                                }
                                messageSb.Insert(0, ResourcesCommon.GeneratingScriptFinished);
                                MessageBox.Show(messageSb.ToString(), "", MessageBoxButton.OK,
                                                isError ? MessageBoxImage.Error : MessageBoxImage.Information);
                            }
                        };
                    generationModel.GenerateScriptFile(conn);
                }
            }

        }

        private List<TableModel> SelectedViewBoxTables()
        {
            return ViewboxTables.Where(r => r.ViewboxChecked).ToList();
        }

        #region DeleteSpecial
        private void DeleteSpecialClick(object obj)
        {
            var col = obj as ColumnModel;
            if (col == null)
                return;
            using (var conn = CurrentProfile.ViewboxConnection.CreateConnection())
            {
                conn.DbMapping.Delete(col.ViewboxInfo);
                col.TableModel.RemoveColumn(col);
            }
        }
        public ICommand DeleteSpecialCommand { get; set; }
        #endregion DeleteSpecial

        #region ShowSourceSpecial

        private void ShowSourceSpecialClick(object obj)
        {
            var temp = obj as TableModel;
            if (temp != null)
            {
                using (var sourceConn = CurrentProfile.SourceConnection.CreateConnection())
                {
                    OnDataPreviewShowed(DataPreview(temp, sourceConn));
                }
            }
        }
        public ICommand ShowSourceSpecialCommand { get; set; }

        #endregion ShowSourceSpecial

        #region ShowFinalSpecial

        private void ShowFinalSpecialClick(object obj)
        {
            var temp = obj as TableModel;
            if (temp != null)
            {
                using (var finalConn = CurrentProfile.FinalConnection.CreateConnection())
                {
                    OnDataPreviewShowed(DataPreview(temp, finalConn));

                }
            }
        }
        public ICommand ShowFinalSpecialCommand { get; set; }

        #endregion ShowFinalSpecial

        #region DataPreviewShowed

        public void OnDataPreviewShowed(DataPreviewModel model)
        {
            if (DataPreviewShowed != null)
            {
                DataPreviewShowed(this, model);
            }
        }

        public event DataPreviewShowed DataPreviewShowed;

        #endregion DataPreviewShowed

        #region ShowNotSelectedError
        public bool ShowNotSelectedError()
        {
            if (CurrentProfile == null)
            {
                MessageBox.Show(ResourcesCommon.NotSelectedProfile, "", MessageBoxButton.OK, MessageBoxImage.Error);
                return true;
            }
            return false;
        }
        #endregion ShowNotSelectedError

        #region ShowNotSelectedViewBoxTablesError
        public bool ShowNotSelectedViewBoxTablesError()
        {
            if (SelectedViewBoxTables().Count() == 0)
            {
                MessageBox.Show(ResourcesCommon.NotSelectedViewBoxTables, "", MessageBoxButton.OK, MessageBoxImage.Error);
                return true;
            }
            return false;
        }
        #endregion ShowNotSelectedViewBoxTablesError

        #region Create localizations

        public ICommand CreateLocalisationsForAllCommand { get { return new RelayCommand(CreateLocalisationsForAllExecuted); } }

        private void CreateLocalisationsForAllExecuted(object sender)
        {
            if (LocalisationSettings == null)
            {
                LocalisationSettings = new LocalizationTextsSettingsModel(this);
                LocalisationSettings.OnSettingsAccepted += new LocalisationTextsSettingsEventHandler(LocalisationSettings_OnSettingsAccepted);
            }

            OnLocalisationAllTextsClicked(this);
        }

        void LocalisationSettings_OnSettingsAccepted(object sender, LocalisationTextsSettingsEventArgs e)
        {
            ViewboxProgress = new ProgressCalculator { WorkerSupportsCancellation = true };
            ViewboxProgress.DoWork += CreateLocalisationsForAllExecutedDoWork;
            ViewboxProgress.RunWorkerCompleted += CreateLocalisationsForAllExecutedDoWork_RunWorkerCompleted;
            ViewboxProgress.RunWorkerAsync(e);
        }

        public void CreateLocalisationsForAllExecutedDoWork(object sender, DoWorkEventArgs e)
        {
            var settings = e.Argument as LocalisationTextsSettingsEventArgs;

            var checkedTables = ViewboxTables.Where(p => p.ViewboxChecked).ToList();
            long count = checkedTables.Count + checkedTables.Sum(p => p.ViewboxColumns.Count);

            ViewboxProgress.SetWorkSteps(count, false);
            ViewboxProgress.SetStep(0);

            using (var conn = CurrentProfile.ViewboxConnection.CreateConnection())
            {
                foreach (var table in checkedTables)
                {
                    if (settings.OnTableNames)
                    {
                        ViewboxProgress.Description = String.Format("Create localisations for table: {0}", table.Name);
                        var loc = new LocalizationTextsConfigurationModel(table, this);

                        if (settings.CountryCode == ResourcesCommon.All)
                        {
                            foreach (var item in Languages.Where(p => p.CountryCode != ResourcesCommon.All))
                            {
                                settings.CountryCode = item.CountryCode;
                                loc.AutoCorrectTexts(conn, settings);
                            }

                            settings.CountryCode = ResourcesCommon.All;
                        }
                        else
                            loc.AutoCorrectTexts(conn, settings);
                    }

                    ViewboxProgress.StepDone();

                    foreach (var col in table.ViewboxColumns)
                    {
                        if (settings.OnColumnNames)
                        {
                            ViewboxProgress.Description = String.Format("Create localisations for column: {0}.{1}", table.Name, col.Name);
                            var locForColumns = new LocalizationTextsConfigurationModel(col, this);

                            if (settings.CountryCode == ResourcesCommon.All)
                            {
                                foreach (var item in Languages.Where(p => p.CountryCode != ResourcesCommon.All))
                                {
                                    settings.CountryCode = item.CountryCode;
                                    locForColumns.AutoCorrectTexts(conn, settings);
                                }

                                settings.CountryCode = ResourcesCommon.All;
                            }
                            else
                                locForColumns.AutoCorrectTexts(conn, settings);
                        }

                        ViewboxProgress.StepDone();
                    }
                }
            }
            ViewboxProgress.SetStep(0);
        }

        private void CreateLocalisationsForAllExecutedDoWork_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null || e.Cancelled || ViewboxProgress.CancellationPending)
            {
                ViewboxProgress.Description = "Operation cancelled";
                ViewboxError = e.Error == null ? "Operation cancelled" : e.Error.ToString();
                if (e.Error != null)
                    _logger.Error("Problem with loading Viewbox data", e.Error);
                else
                    _logger.Info("Loading Viewbox data cancelled");
            }
            else
            {
                ViewboxProgress.Description = "Ready";
            }
        }

        #endregion

        #region Renamer

        public ICommand RenameingForAllCommand { get { return new RelayCommand(RenameingForAllExecuted); } }

        private void RenameingForAllExecuted(object sender)
        {
            if (RenamerSettings == null)
            {
                RenamerSettings = new RenamerSettingsModel();
                RenamerSettings.OnSettingsAccepted += RenamerSettings_OnSettingsAccepted;
            }

            OnRenamerSettingsClicked(this);
        }

        void RenamerSettings_OnSettingsAccepted(object sender, RenamerSettingsEventArgs e)
        {
            SourceProgress = new ProgressCalculator { WorkerSupportsCancellation = true };
            SourceProgress.DoWork += RenameingForAllExecutedDoWork;
            SourceProgress.RunWorkerCompleted += RenameingForAllExecutedDoWork_RunWorkerCompleted;
            SourceProgress.RunWorkerAsync(e);
        }

        public void RenameingForAllExecutedDoWork(object sender, DoWorkEventArgs e)
        {
            var settings = e.Argument as RenamerSettingsEventArgs;

            var checkedTables = SourceTables.Where(p => p.SourceChecked).ToList();
            long count = checkedTables.Count + checkedTables.Sum(p => p.SourceColumns.Count);

            SourceProgress.SetWorkSteps(count, false);
            SourceProgress.SetStep(0);

            using (var conn = CurrentProfile.SourceConnection.CreateConnection())
            {
                foreach (var table in checkedTables)
                {
                    string newTableName = null;

                    if (settings.OnTableNames)
                    {
                        SourceProgress.Description = String.Format("Renaming table: {0}", table.Name);
                        var ren = new RenamerModel(table, this);
                        ren.AutoRename(conn, settings);
                        newTableName = ren.ToName;
                    }

                    SourceProgress.StepDone();

                    foreach (var col in table.SourceColumns)
                    {
                        if (settings.OnColumnNames)
                        {
                            if (newTableName != null)
                            {
                                col.TableModel.Name = newTableName;
                            }

                            SourceProgress.Description = String.Format("Renaming column: {0}.{1}", table.Name, col.Name);
                            var renForColumns = new RenamerModel(col, this);
                            renForColumns.AutoRename(conn, settings);
                        }

                        SourceProgress.StepDone();
                    }
                }
            }

            SourceProgress.SetStep(0);
        }

        private void RenameingForAllExecutedDoWork_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null || e.Cancelled || SourceProgress.CancellationPending)
            {
                SourceProgress.Description = "Operation cancelled";
                SourceError = e.Error == null ? "Operation cancelled" : e.Error.ToString();
                if (e.Error != null)
                    _logger.Error("Problem with loading source data", e.Error);
                else
                    _logger.Info("Loading source data cancelled");
            }
            else
            {
                SourceProgress.Description = "Ready";
            }
        }

        #endregion

        #region ValidateTableOrColumnName

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns>Is valid?</returns>
        public bool ValidateTableOrColumnName(string name)
        {
            Regex myregex = new Regex(@"[^a-zA-Z0-9\-\\/(),_\s]+");

            if (myregex.IsMatch(name) || name.Contains(" "))
                return false;

            return true;
        }

        #endregion
    }
}