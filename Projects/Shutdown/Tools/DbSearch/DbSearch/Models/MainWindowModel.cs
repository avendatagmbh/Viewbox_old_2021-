// -----------------------------------------------------------
// Created by Benjamin Held - 08.09.2011 10:43:07
// Copyright AvenDATA 2011
// -----------------------------------------------------------

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using DbAccess;
using DbSearch.Controls;
using DbSearch.Controls.Keys;
using DbSearch.Controls.Profile;
using DbSearch.Controls.Result;
using DbSearch.Controls.Search;
using DbSearch.Controls.SearchStatus;
using DbSearch.Manager;
using DbSearch.Models.ListView;
using DbSearch.Models.Profile;
using DbSearch.Models.Result;
using DbSearch.Models.Rules;
using DbSearch.Models.Search;
using DbSearch.Models.SearchStatus;
using DbSearch.Structures.Navigation;
using DbSearch.Windows;
using DbSearch.Windows.Profile;
using DbSearchBase;
using DbSearchLogic.Manager;
using DbSearchLogic.ProcessQueue;
using DbSearchLogic.SearchCore;
using DbSearchLogic.SearchCore.ForeignKeySearch;
using DbSearchLogic.SearchCore.KeySearch;
using DbSearchLogic.SearchCore.Keys;
using DbSearchLogic.SearchCore.Threading;
using DbSearchLogic.Structures.TableRelated;
using AV.Log;
using Utils;
using Utils.Commands;
using log4net;
using Control = System.Windows.Controls.Control;
using Key = DbSearchLogic.SearchCore.KeySearch.Key;
using MessageBox = System.Windows.MessageBox;

namespace DbSearch.Models
{
    public class MainWindowModel : NotifyPropertyChangedBase
    {// : INotifyPropertyChanged{
        /*#region Events
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName) { if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName)); }
        #endregion Events*/

        #region [ Private members ]

        internal ILog _log = LogHelper.GetLogger();

        /// <summary>
        /// Create a semaphore instance
        /// </summary>
        private static SemaphoreSlim _semaphoreKeySearchManager = new SemaphoreSlim(1);

        /// <summary>
        /// Create a semaphore instance
        /// </summary>
        private static SemaphoreSlim _semaphoreRunKeySearch = new SemaphoreSlim(1);

        private static KeyCollector keyCollectorInstance = null;
        private static ForeignKeyCollector foreignKeyCollectorInstance = null;

        #endregion [ Private members ]

        #region Constructor
        public MainWindowModel(DlgMainWindow dlgMainWindow)
        {
            try {
                TaskScheduler.UnobservedTaskException += (sender, args) => {
                    foreach (Exception exc in args.Exception.Flatten().InnerExceptions)
                        Console.WriteLine("Observed exception: " + exc.Message);
                    args.SetObserved();
                    GC.Collect();
                };

                DlgMainWindow = dlgMainWindow;
                CurrentResultsModel = new ResultsModel();
                _currentQueriesModel = new QueriesModel(this);

                RuleListModel = new RuleListModel(this);
                //Init navigation
                NavigationTree = new NavigationTree(dlgMainWindow);
                NavigationTreeEntry generalEntry = NavigationTree.AddEntry("Verwaltung", null);
                NavigationTree.AddEntry("Profile", new CtlListView(new ListViewProfileModel() { Owner = DlgMainWindow }), generalEntry).IsSelected = true;
                NavigationTree.AddEntry("Abfragen", new CtlQueries() { DataContext = _currentQueriesModel }, generalEntry);
                generalEntry.IsExpanded = true;

                CtlSearch = new CtlSearch() { DataContext = SelectedSearchModel };
                SearchNavigationEntry = NavigationTree.AddEntry("Suche", CtlSearch);
                //SearchNavigationEntry.DataContext = this;
                //SearchNavigationEntry.SetBinding(UIElement.VisibilityProperty, new Binding("SelectedQuery") {Converter = new ValueExistsToVisibilityConverter()});
                SearchNavigationEntry.PropertyChanged += SearchNavigationEntry_PropertyChanged;
                InfoModel = new InfoModel(CtlSearch.CtlSearchMatrix.dgvSearchMatrix, this);
            
                foreach (var profile in Profiles)
                    AddedProfile(profile, true);
                //Select profile
                var lastProfile = ProfileManager.GetProfile(ApplicationManager.ApplicationConfig.LastProfile);
                if (lastProfile != null){
                    SelectedProfile = lastProfile;
                }
                else if (SelectedProfile == null && Profiles != null && Profiles.Count > 0)
                    SelectedProfile = Profiles[0];

                SearchStatusEntry = NavigationTree.AddEntry("Suchstatus", new CtlSearchStatus() { DataContext = new SearchStatusModel(this.KeySearchManagerInstance == null ? null : this.KeySearchManagerInstance.InitiatedSearches) });
                _resultEntry = NavigationTree.AddEntry("Ergebnisse", new CtlResults() { DataContext = CurrentResultsModel });
                _keyResult = NavigationTree.AddEntry("Keys", new CtlKeyResults() { DataContext = null /*KeyManager*/ });
                _keyResult.PropertyChanged += KeyResultOnPropertyChanged;

                //ProfileModelManager.GetModel(profile).FinishedLoadingTablesEvent += MainWindowModel_FinishedLoadingTablesEvent;
                ExportScriptCommand = new DelegateCommand(obj => true, ExportScript);
            }
            catch {
                throw;
            }
        }

        private void KeyResultOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs) {
            if (propertyChangedEventArgs.PropertyName == "IsSelected") {
                if(_keyResult.IsSelected && ((_keyResult.Content as CtlKeyResults).DataContext == null) || (KeyManager != null && KeyManager.Profile != SelectedProfile)) {
                    (_keyResult.Content as CtlKeyResults).DataContext = KeyManager;
                }
            }
        }
        #endregion
        
        #region Properties
        public ICommand ExportScriptCommand { get; private set; }
        public NavigationTree NavigationTree { get; set; }
        private DlgMainWindow DlgMainWindow { get; set; }
        public ObservableCollection<DbSearchLogic.Config.Profile> Profiles { get { return ProfileManager.Profiles; } }
        public CtlSearch CtlSearch { get; set; }
        public InfoModel InfoModel { get; set; }
        protected NavigationTreeEntry SearchStatusEntry { get; set; }
        protected QueriesModel _currentQueriesModel { get; set; }
        public bool EnableQueryRelated { get { return SelectedQuery != null; /* && SearchNavigationEntry.IsSelected; */} }
        public RuleListModel RuleListModel { get; private set; }

        #region [ Events ]

        public event EventHandler NoKeysFound;
        public event EventHandler CandidateKeysFound;

        #endregion [ Events ]

        #region SelectedProfile
        private DbSearchLogic.Config.Profile _selectedProfile;

        public DbSearchLogic.Config.Profile SelectedProfile {
            get { return _selectedProfile; }
            set {
                if (_selectedProfile != value) {
                    bool wasNull = _selectedProfile == null;
                    _selectedProfile = value;
                    if (!wasNull || _selectedProfile.IsStatusOk) {
                        KeySearchManagerInstance.KeyManager.Cancel();
                    }

                    if (_selectedProfile != null) {
                        if (_selectedProfile.IsStatusOk) {
                            _selectedProfile.Load();
                            ProfileModel profileModel = ProfileModelManager.GetModel(SelectedProfile);
                            profileModel.LoadQueries();
                            _log.Log(LogLevelEnum.Info, "Key search manager is being initialized for database " + _selectedProfile.DbConfigView.DbName + ".", true);
                            //_keySearchManager = new KeySearchManager(_selectedProfile.DbConfigView);
                            KeySearchManagerInstance.ChangeProfile(_selectedProfile);
                            _log.Log(LogLevelEnum.Info, "Key search manager is initialized for database " + _selectedProfile.DbConfigView.DbName + ".", true);
                            if (_keyResult != null) (_keyResult.Content as CtlKeyResults).DataContext = null;
                        }
                    }

                    OnPropertyChanged("SelectedProfile");
                    OnPropertyChanged("ProfileSelectionVisibility");
                    OnPropertyChanged("KeySearchManagerInstance");
                    OnPropertyChanged("KeyManager");
                    OnPropertyChanged("KeyTables");
                }
            }
        }
        #endregion SelectedProfile

        #region [ SearchManager ]

        private KeySearchManager _keySearchManager = null;
        public KeySearchManager KeySearchManagerInstance {
            get {
                InitKeySearchManager();
                return _keySearchManager;
            }
        }

        private void InitKeySearchManager() {
            if (_keySearchManager == null) {
                _semaphoreKeySearchManager.Wait();
                try {
                    if (_keySearchManager == null) {
                        if (SelectedProfile != null) {
                            _log.Log(LogLevelEnum.Info, "Key search manager being initialized.", true);
                            //_keySearchManager = new KeySearchManager(SelectedProfile.DbConfigView);
                            try {
                                _keySearchManager = new KeySearchManager(SelectedProfile);
                                _log.Log(LogLevelEnum.Info, "Key search manager is initialized.", true);
                            }
                            catch(Exception ex) {
                                _log.Log(LogLevelEnum.Error, "Key search manager initialization failed.", true);
                                _log.Error("Key search manager initialization failed.", ex);
                            }
                            OnPropertyChanged("KeySearchManagerInstance");
                            OnPropertyChanged("KeyManager");
                        }
                    }
                } finally {
                    _semaphoreKeySearchManager.Release();
                }
            }
        }

        public KeyManager KeyManager {
            get {
                if (KeySearchManagerInstance != null && KeySearchManagerInstance.KeyManager != null)
                    return KeySearchManagerInstance.KeyManager;
                else
                    return null;
            }
        }
        
        #endregion [ SearchManager ]

        #region SelectedQuery
        private Query _selectedQuery;
        public Query SelectedQuery
        {
            get { return _selectedQuery; }
            set
            {
                if (_selectedQuery != value)
                {
                    _selectedQuery = value;
                    if (_selectedQuery != null)
                    {
                        _selectedQuery.Load();
                    }
                    CurrentResultsModel.Query = _selectedQuery;
                    OnPropertyChanged("CurrentResultsModel");
                    SearchModel model = ProfileModelManager.GetModel(SelectedProfile).GetSearchModel(_selectedQuery);
                    SelectedSearchModel = model;

                    OnPropertyChanged("EnableQueryRelated");
                    OnPropertyChanged("SelectedQuery");
                    OnPropertyChanged("HasQueries");
                }
                //Do not show search navigation entry if no Query is selected
                SearchNavigationEntry.IsVisible = _selectedQuery != null;
            }
        }

        #region MaxThreads
        public int MaxThreads
        {
            get { return ThreadManager.MaxThreads; }
            set
            {
                ThreadManager.MaxThreads = value;
                OnPropertyChanged("MaxThreads");
            }
        }
        #endregion MaxThreads
        #endregion

        private ResultsModel _currentResultsModel;
        public ResultsModel CurrentResultsModel
        {
            get { return _currentResultsModel; }
            set { 
                _currentResultsModel = value;
                _currentResultsModel.ColumnMappingChanged += CurrentResultsModelOnColumnMappingChanged;
            }
        }

        private NavigationTreeEntry _resultEntry;

        private NavigationTreeEntry _keyResult;

        //Define a performance logging timer        
        private System.Timers.Timer _performanceLogTimer = null;

        #region SelectedSearchModel
        private SearchModel _selectedSearchModel;
        public NavigationTreeEntry SearchNavigationEntry { get; set; }

        public SearchModel SelectedSearchModel {
            get { return _selectedSearchModel; }
            set {
                if (_selectedSearchModel != value) {
                    _selectedSearchModel = value;

                    DlgMainWindow.Dispatcher.Invoke(DispatcherPriority.Background, new Action( () => CtlSearch.DataContext = value));
                    OnPropertyChanged("SelectedSearchModel");

                }
            }
        }
        #endregion

        public bool HasQueries
        {
            get
            {
                if (SelectedProfile == null) return false;
                return SelectedProfile.Queries.Items.Count != 0;
            }
        }

        public Control DummyButtonControl { get; set; }
        #endregion

        #region EventHandler

        private void MainWindowModel_FinishedLoadingTablesEvent(object sender, FinishedLoadingTablesEventArgs e) {
            if (e.Exception != null) {
                DlgMainWindow.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background,
                    new Action( () => {
                            string errors = "";
                            foreach (var text in e.Exception.Flatten().InnerExceptions)
                                errors += text + Environment.NewLine;
                            MessageBox.Show(DlgMainWindow,
                                            "Es ist ein Fehler beim Abfragen der Verprobungsdatenbank aufgetreten: " +
                                            Environment.NewLine + errors, "", MessageBoxButton.OK,
                                            MessageBoxImage.Error

                                );
                        }
                        ));

            }
            else if (e.Profile.Queries.Items.Count != 0) {
                //SelectedQuery = e.Profile.Queries.Items[0];
                Query query = e.Profile.Queries.FindQuery(ApplicationManager.ApplicationConfig.LastQueryTable);
                if (query != null) SelectedQuery = query;
                else SelectedQuery = e.Profile.Queries.Items[0];
            }
            else SelectedQuery = null;
            OnPropertyChanged("HasQueries");
        }

        private void SearchNavigationEntry_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            OnPropertyChanged("EnableQueryRelated");
        }

        private void CurrentResultsModelOnColumnMappingChanged(object sender, MappingChangedEventArgs eventArgs) {
            if (eventArgs.Mapped) {
                bool isKeysExists = KeysExists();
                if (!isKeysExists) {
                    OnNoKeysFound();
                    return;
                }
                if (ExistTable<KeyCandidate>()) {
                    OnCandidateKeysFound();
                    return;
                }
                InitiateForeignKeySearch(new Table(eventArgs.ColumnHitInfoView.TableInfo.Id, eventArgs.ColumnHitInfoView.TableInfo.Name));
            } 
            else {
                CancelForeignKeySearch(new Table(eventArgs.ColumnHitInfoView.TableInfo.Id, eventArgs.ColumnHitInfoView.TableInfo.Name));
            }
            // DEVNOTE: on save button click
            //CurrentResultsModel.Query.Save();
        }

        private void OnNoKeysFound() {
             if (NoKeysFound != null) NoKeysFound(this, new EventArgs());
        }

        private void OnCandidateKeysFound() {
            if (CandidateKeysFound != null) CandidateKeysFound(this, new EventArgs());
        }

        #endregion

        #region Methods
        internal void DeletedProfile(DbSearchLogic.Config.Profile profile)
        {
            if (Profiles.Count > 0) SelectedProfile = Profiles[0];
            else SelectedProfile = null;
            ProfileModelManager.GetModel(profile).FinishedLoadingTablesEvent -= MainWindowModel_FinishedLoadingTablesEvent;
            OnPropertyChanged("Profiles");
        }

        internal void AddedProfile(DbSearchLogic.Config.Profile profile, bool addedOnStartup = false)
        {
            ProfileModelManager.GetModel(profile).FinishedLoadingTablesEvent += MainWindowModel_FinishedLoadingTablesEvent;
            if (!addedOnStartup)
            {
                //ProfileModelManager.GetModel(profile).LoadQueries();
                SelectedProfile = profile;
                ApplicationManager.Save();
            }
            OnPropertyChanged("Profiles");
        }


        public void SearchAllQueries() {
            if (SelectedProfile == null) {
                MessageBox.Show(DlgMainWindow, "Bitte zuerst ein Profil auswählen.");
                return;
            }
            //foreach (var query in SelectedProfile.Queries.Items) {
            //    ThreadManager.StartNewTableSearch(query);
            //}
            if (!AskForTableSettings(SelectedProfile.Queries.Items))
                return;
            ThreadManager.StartNewTableSearch(SelectedProfile.Queries.Items);
            //.Where(t => t.Name != "01_CJ20N_Statusanzeige_Liste_der_Aenderungen").ToList()
            SearchStatusEntry.IsSelected = true;

        }

        public void ImportQueries() {
            if (SelectedProfile == null) {
                MessageBox.Show(DlgMainWindow, "Bitte zuerst ein Profil auswählen.");
                return;
            }
            ImportQueriesModel model = new ImportQueriesModel(SelectedProfile);
            DlgImportQueries dlg = new DlgImportQueries() { DataContext = model, Owner = DlgMainWindow };
            bool? showDialog = dlg.ShowDialog();
            if (showDialog != null && showDialog.Value) {
                ProfileModel profileModel = ProfileModelManager.GetModel(SelectedProfile);
                profileModel.ImportValidationTables(model.Tables);
            }
        }

        public void StartQuerySearch() {
            if (SelectedSearchModel != null) {
                if (!AskForTableSettings(new List<Query>() { SelectedSearchModel.Query }))
                    return;
                SelectedSearchModel.StartSearch();
                SearchStatusEntry.IsSelected = true;
            }
            else {
                MessageBox.Show(DlgMainWindow, "Keine Abfrage ausgewählt");
            }
        }

        private bool AskForTableSettings(IEnumerable<Query> setForQueries)
        {
            SearchSettingsModel model = new SearchSettingsModel(SelectedProfile.Queries.Items);
            Query firstQuery = setForQueries.FirstOrDefault();
            if (firstQuery != null)
            {
                model.WhitelistTables = firstQuery.SearchTableDecider.WhitelistTablesAsString();
                model.BlacklistTables = firstQuery.SearchTableDecider.BlacklistTablesAsString();
            }
            DlgSearchSettings dlg = new DlgSearchSettings() { DataContext = model, Owner = DlgMainWindow };
            dlg.ShowDialog();
            if (model.Accept)
            {
                foreach (var query in setForQueries)
                    query.SearchTableDecider.FromString(model.BlacklistTables, model.WhitelistTables, query);
            }
            return model.Accept;
        }

        public void FindOptimalRows()
        {
            if (SelectedSearchModel != null)
                SelectedSearchModel.OptimalRows();
        }

        public void SaveQuery()
        {
            if (SelectedSearchModel != null)
                SelectedSearchModel.SaveQuery();
        }

        public void DistinctDb(int threadCount)
        {
            if (SelectedProfile == null) return;
            ThreadManager.DistinctDb(SelectedProfile, threadCount);
        }

        public void AddEmptyQuery()
        {
            if (SelectedProfile == null)
                return;
            DlgAddEmptyQuery dlg = new DlgAddEmptyQuery() { DataContext = new AddEmptyQueryModel(SelectedProfile), Owner = DlgMainWindow };
            dlg.ShowDialog();
            OnPropertyChanged("HasQueries");
        }

        private void ExportScript(object o)
        {
            DummyButtonControl.Focus();
            if (SelectedQuery == null)
            {
                MessageBox.Show(DlgMainWindow, "Keine Abfrage ausgewählt");
                return;
            }

            if (!SelectedQuery.UserColumnMappings.HasMappings())
            {
                MessageBox.Show(DlgMainWindow, "Keine Zuordnungen vorhanden.");
                return;
            }

            DlgExportScript dlg = new DlgExportScript() { DataContext = new ExportScriptModel(SelectedQuery), Owner = DlgMainWindow };
            dlg.ShowDialog();
        }
        
        /// <summary>
        /// A keys search algorithm
        /// </summary>
        private enum KeySearchAlgorithm {
            Basic,      //uses original tables and pure SQL
            Advanced    //mix from the Basic and the Advanced versions which uses the preprocessed IDX tables depending on the size of the table (treshold)
        };

        /// <summary>
        /// The cancellation token source
        /// </summary>
        internal  CancellationTokenSource CancellationTokenSource;

        /// <summary>
        /// Find keys function
        /// </summary>
        /// <param name="actionRecreateTables">The mathod which recreates the tables</param>
        /// <returns>Indicates if key search was interrupted (Canceled) or run successfully (Finished) or already running (Running)</returns>
        public KeySearchResult FindKeys(Func<bool> actionRecreateTables, IPrimaryKeySearchStatEntry pkStatEntry) {

            // DEVNOTE: check whether idx table exists
            using (IDatabase conn = KeySearchManagerInstance.GetOpenConnection()) {
                if (!conn.DatabaseExists(KeySearchManager.GetIdxDbName(SelectedProfile.DbConfigView)))
                    return KeySearchResult.IdxDbMissing;
            }

            pkStatEntry.TaskStatus = KeySearchResult.NotStarted;
            _semaphoreRunKeySearch.Wait();
            try {
                pkStatEntry.TaskStatus = KeySearchResult.Running;
                if (pkStatEntry.CancellationTokenSource.Token.IsCancellationRequested) {
                    pkStatEntry.TaskStatus = KeySearchResult.Canceled;
                    return pkStatEntry.TaskStatus;
                }

                if (keyCollectorInstance != null || foreignKeyCollectorInstance != null) {
                    //return KeySearchResult.Running;
                    pkStatEntry.TaskStatus = KeySearchResult.Canceled;
                    return pkStatEntry.TaskStatus;
                }

                keyCollectorInstance = new KeyCollector();

                KeySearchParameter keySearchParameters = new KeySearchParameter(2, ThreadManager.MaxThreads, pkStatEntry);
                keySearchParameters.TresholdRowsCount = 1000;
                //keySearchParameters.TresholdRowsMethod = KeySearchParameter.TresholdRowsSelector.MEDIAN;
                keySearchParameters.CancellationToken = pkStatEntry.CancellationTokenSource.Token;
                keySearchParameters.KeyCollectorInstance = keyCollectorInstance;

                try {
                    //Attach the process to the console
                    //AttachConsole(-1);
                    Stopwatch stopwatch = Stopwatch.StartNew();

                    KeySearchAlgorithm algorithm = KeySearchAlgorithm.Advanced;
                    //KeySearchAlgorithm algorithm = KeySearchAlgorithm.Basic;
                    KeySearcherBase keySearcher;

                    //Depending on the chosen algorithm, the appropriate strategy object is created
                    switch (algorithm) {
                        case KeySearchAlgorithm.Advanced:
                            keySearcher = new KeySearcherAdvanced(SelectedProfile,
                                                                    (string tableName) => _keySearchManager.GetAllColumnsForTableAsKeyCandidates(tableName),
                                                                    (IEnumerable<KeyCandidate> keys, int degreeOfKeyComplexity) => CreateKeyCombinations(keys, degreeOfKeyComplexity));
                            break;
                        case KeySearchAlgorithm.Basic:
                        default:
                            keySearcher = new KeySearcherBasic(SelectedProfile,
                                                                (string tableName) => _keySearchManager.GetAllColumnsForTableAsKeyCandidates(tableName),
                                                                (IEnumerable<KeyCandidate> keys, int degreeOfKeyComplexity) => CreateKeyCombinations(keys, degreeOfKeyComplexity));
                            break;
                    }

                    keySearcher.KeySearchStarted();

                    _performanceLogTimer = new System.Timers.Timer(60000);
                    _performanceLogTimer.Elapsed += _myTimer_Elapsed;
                    _performanceLogTimer.Enabled = true;
                    _performanceLogTimer.Start();

                    //If cancelled, the results will be saved
                    keySearchParameters.CancellationToken.Register(keySearcher.CancelKeySearch);

                    //Initialize KeySearcher, if false then error occurred or search already have run for database
                    if (!keySearcher.InitSimpleKeySearch(keySearchParameters, actionRecreateTables)) {
                        _log.Log(LogLevelEnum.Info, "Key search finished!", true);
                        //return KeySearchResult.Finished;
                        if (keySearchParameters.CancellationToken.IsCancellationRequested) {
                            pkStatEntry.TaskStatus = KeySearchResult.Canceled;
                            return pkStatEntry.TaskStatus;
                        }
                        pkStatEntry.TaskStatus = KeySearchResult.FinishedAlreadyRun;
                        return pkStatEntry.TaskStatus;
                    }

                    //Create an action for simple key search
                    Action<object> startActionSimple = (object obj) => { keySearcher.StartSimpleKeySearch((KeySearchParameter) obj); };
                    //Create an action for composite key search
                    Action<object> startActionComposite = (object obj) => { keySearcher.StartCompositeKeySearch((KeySearchParameter) obj); };

                    Task task = null;
                    List<Task> tasks = new List<Task>();

                    #region [ Simple key search ]

                    for (int i = 0; i < keySearchParameters.DegreeOfParallelism; i++) {
                        keySearchParameters.TaskNumber = i + 1;
                        task = Task.Factory.StartNew(startActionSimple, keySearchParameters,
                                                        keySearchParameters.CancellationToken);
                        tasks.Add(task);
                    }

                    try {
                        Task.WaitAll(tasks.ToArray(), keySearchParameters.CancellationToken);
                    } catch (AggregateException) {
                        //The cancellation ewent throw this exception
                    }

                    //Save the remaining keys to DB
                    keyCollectorInstance.AddKey(null, DateTime.Now, DateTime.Now);

                    #endregion [ Simple key search ]

                    #region [ Composite key search ]

                    if (!keySearchParameters.CancellationToken.IsCancellationRequested) {
                        if (keySearcher.InitCompositeKeySearch(keySearchParameters)) {
                            tasks = new List<Task>();
                            for (int i = 0; i < keySearchParameters.DegreeOfParallelism; i++) {
                                keySearchParameters.TaskNumber = i + 1;
                                task = Task.Factory.StartNew(startActionComposite, keySearchParameters,
                                                                keySearchParameters.CancellationToken);
                                tasks.Add(task);
                            }

                            try {
                                Task.WaitAll(tasks.ToArray(), keySearchParameters.CancellationToken);
                            } catch (AggregateException) {
                                //The cancellation ewent throw this exception
                            }
                        }
                    }

                    #endregion [ Composite key search ]

                    KeySearchManagerInstance.DeleteTable<KeyCandidateColumn>();
                    KeySearchManagerInstance.DeleteTable<KeyCandidate>();

                    _performanceLogTimer.Stop();
                    _performanceLogTimer.Enabled = false;

                    keySearcher.KeySearchFinished(keySearchParameters);

                    keySearcher = null;
                    GC.Collect();

                    stopwatch.Stop();
                } finally {
                    keyCollectorInstance = null;
                    if (_performanceLogTimer != null)
                        _performanceLogTimer.Stop();
                }

                if (keySearchParameters.CancellationToken.IsCancellationRequested) {
                    pkStatEntry.TaskStatus = KeySearchResult.Canceled;
                    return pkStatEntry.TaskStatus;
                }
                pkStatEntry.TaskStatus = KeySearchResult.Finished;
                return pkStatEntry.TaskStatus;
            } finally {
                keyCollectorInstance = null;
                _semaphoreRunKeySearch.Release();
            }
        }

        public bool IsPrimaryKeySearchRunning() {
            if (keyCollectorInstance != null)
                return true;
            else
                return false;
        }

        public bool ExportKeys(string fileName) {
            // TODO: clear in memory collections in KeySearchManager in order to free up memory
            Exception ex = null;
            StringBuilder csvContent = new StringBuilder();
            if (!Utils.FileHelper.IsFileBeingUsed(fileName, out ex)) {
                using (StreamWriter csvWrite = new StreamWriter(File.Open(fileName, FileMode.Create), Encoding.Default)) {
                    csvContent.AppendLine(string.Format("{1}{0}{2}{0}{3}{0}{4}", ";", "Key id", "Table name", "Processing duration", "Primary key columns"));
                    foreach (Key key in KeySearchManagerInstance.GetPrimaryKeysLazy()) {
                        csvContent.AppendLine(string.Format("{1}{0}{2}{0}{3}{0}{4}", ";", key.Id, key.TableName, TimeSpan.FromSeconds(key.ProcessingDuration).ToString("g"), key.Label));
                    }

                    csvWrite.WriteLine(csvContent);
                    csvWrite.Close();
                }
            } else {
                return false;
            }

            return true;
        }
        
        private void CancelForeignKeySearch(Table table) {
            IEnumerable<IForeignKeySearchStatEntry> fkSearches = KeySearchManagerInstance.InitiatedSearches.OfType<IForeignKeySearchStatEntry>();
            foreach (IForeignKeySearchStatEntry fkSearchStatEntry in fkSearches) {
                if (string.Compare(fkSearchStatEntry.ForeignKeyTableName, table.Name, StringComparison.InvariantCultureIgnoreCase) == 0) fkSearchStatEntry.CancelTask();
            }
        }

        /// <summary>
        /// Initiates a foreign key search for a table
        /// </summary>
        /// <param name="selectedTable">The table to run foreign key search for</param>
        public void InitiateForeignKeySearch(Table selectedTable) {
            ForeignKeySearchStatEntry fkStatEntry = new ForeignKeySearchStatEntry(2, 0, new CancellationTokenSource(), KeySearchManagerInstance.CurrentProfile.DisplayString, selectedTable.Name);
            fkStatEntry.KeySearchTask = new Task<KeySearchResult>(() => { using (fkStatEntry) return PerformForeignKeySerchForTable(new ConcurrentQueue<Key>(GetKeys()), selectedTable, 2, ForeignKeySearchMode.ForeignKeyInTable, DummyMethod, fkStatEntry); });
            KeySearchManagerInstance.AddKeySearchStatEnrty<ForeignKeySearchStatEntry>(fkStatEntry);
        }

        private bool DummyMethod() {
            return false;
        }

        /// <summary>
        /// Performs a foreign key search on the specified table
        /// </summary>
        /// <param name="keys">The primary keys found in database</param>
        /// <param name="selectedTable">The table to search foreign keys in</param>
        /// <param name="degreeOfKeyComplexity">The degree of foreign key complexity (in case of composite keys)</param>
        /// <param name="mode">The foreign key search mode</param>
        /// <param name="actionRecreateTables">The mathod which recreates the tables</param>
        public KeySearchResult PerformForeignKeySerchForTable(ConcurrentQueue<Key> keys, Table selectedTable, int degreeOfKeyComplexity, ForeignKeySearchMode mode, Func<bool> actionRecreateTables, ForeignKeySearchStatEntry fkStatEntry) {

            fkStatEntry.TaskStatus = KeySearchResult.NotStarted;
            _semaphoreRunKeySearch.Wait();
            try {
                fkStatEntry.TaskStatus = KeySearchResult.Running;
                if (fkStatEntry.CancellationTokenSource.Token.IsCancellationRequested) {
                    fkStatEntry.TaskStatus = KeySearchResult.Canceled;
                    return fkStatEntry.TaskStatus;
                }

                if (keyCollectorInstance != null || foreignKeyCollectorInstance != null) {
                    fkStatEntry.TaskStatus = KeySearchResult.Canceled;
                    return fkStatEntry.TaskStatus;
                }

                foreignKeyCollectorInstance = new ForeignKeyCollector();
                try {
                    foreignKeyCollectorInstance.CreateDatabases(SelectedProfile.DbConfigView);
                    ForeignKeySearchParameter foreignKeySearchParameter = new ForeignKeySearchParameter(selectedTable.Name, selectedTable.Id, degreeOfKeyComplexity, mode, fkStatEntry);
                    foreignKeySearchParameter.PrimaryKeys = keys;
                    foreignKeySearchParameter.BaseTableColumns = GetAllColumnsForTable(foreignKeySearchParameter.BaseTableName);
                    foreignKeySearchParameter.ForeignKeyCollectorInstance = foreignKeyCollectorInstance;
                    foreignKeySearchParameter.CancellationToken = fkStatEntry.CancellationTokenSource.Token;
                    foreignKeySearchParameter.TaskCount = ThreadManager.MaxThreads;

                    if (ForeignKeysProcessedForTable(foreignKeySearchParameter, mode)) {
                        _log.Log(LogLevelEnum.Info, "Table [" + foreignKeySearchParameter.BaseTableName + "] is already processed!\nPlease select another one!", true);
                        fkStatEntry.TaskStatus = KeySearchResult.FinishedAlreadyRun;
                        return fkStatEntry.TaskStatus;
                    }

                    Action<object> action = (object obj) => { fkStatEntry.TaskStatus = FindForeignKeys((ForeignKeySearchParameter)obj, actionRecreateTables); };
                    Task task = new Task(action, foreignKeySearchParameter);
                    task.Start();
                    task.Wait();

                    if (fkStatEntry.CancellationTokenSource.Token.IsCancellationRequested) {
                        fkStatEntry.TaskStatus = KeySearchResult.Canceled;
                        return fkStatEntry.TaskStatus;
                    }
                    fkStatEntry.TaskStatus = KeySearchResult.Finished;
                    return fkStatEntry.TaskStatus;
                } finally {
                    foreignKeyCollectorInstance = null;
                }
            } finally
            {
                _semaphoreRunKeySearch.Release();
            }
        }

        /// <summary>
        /// Check whether the foreign key search was already run for this table
        /// </summary>
        /// <param name="foreignKeySearchParameter">The foreign key search parameter</param>
        /// <param name="mode">The foreign key search mode</param>
        /// <returns>Returns true if the foreign key search was already run for this table</returns>
        private bool ForeignKeysProcessedForTable(ForeignKeySearchParameter foreignKeySearchParameter, ForeignKeySearchMode mode) {
            foreignKeySearchParameter.ForeignKeysRelatedToTable = KeySearchManagerInstance.GetForeignKeysRelatedToTable(foreignKeySearchParameter.BaseTableId);
            bool processed = true;

            switch (mode) {
                case ForeignKeySearchMode.ForeignKeyInTable:
                    processed = InTableFkSearchProcessed(foreignKeySearchParameter);
                    break;
                case ForeignKeySearchMode.ForeignKeyReferencesForTable:
                    processed = FkReferencesForTableProcessed(foreignKeySearchParameter);
                    break;
                case ForeignKeySearchMode.BothDirections:
                    processed = InTableFkSearchProcessed(foreignKeySearchParameter) &&
                                FkReferencesForTableProcessed(foreignKeySearchParameter);
                    break;
                default:
                    throw new NotImplementedException();
            }

            return processed;
        }

        /// <summary>
        /// Gets if the fk search for in table fk's was already run
        /// </summary>
        /// <param name="foreignKeySearchParameter">The foreign key search parameter</param>
        /// <returns></returns>
        private bool InTableFkSearchProcessed(ForeignKeySearchParameter foreignKeySearchParameter) {
            return foreignKeySearchParameter.ForeignKeysRelatedToTable.Any(fk =>  fk.SearchedTableId == foreignKeySearchParameter.BaseTableId && fk.TableId == foreignKeySearchParameter.BaseTableId);
        }

        /// <summary>
        /// Gets if the fk search for fk references for this table was already run
        /// </summary>
        /// <param name="foreignKeySearchParameter">The foreign key search parameter</param>
        /// <returns></returns>
        private bool FkReferencesForTableProcessed(ForeignKeySearchParameter foreignKeySearchParameter) {
            return foreignKeySearchParameter.ForeignKeysRelatedToTable.Any(fk =>  fk.SearchedTableId == foreignKeySearchParameter.BaseTableId && fk.PrimaryKeyColumns.Any(pk => string.Compare(pk.TableName, foreignKeySearchParameter.BaseTableName, StringComparison.InvariantCultureIgnoreCase) == 0));
        }

        /// <summary>
        /// Search for the foreign Keys
        /// </summary>
        /// <param name="actionRecreateTables">The method which recreates the tables</param>
        /// <param name="foreignKeySearchParameter">The foreign key search parameter</param>
        private KeySearchResult FindForeignKeys(ForeignKeySearchParameter foreignKeySearchParameter, Func<bool> actionRecreateTables) {

            _log.Log(LogLevelEnum.Info, "Foreign key search for table " + foreignKeySearchParameter.BaseTableName + " started!", true);
            
            Task task = null;
            List<Task> tasks = new List<Task>();
            Stopwatch stopwatch = Stopwatch.StartNew();

            ForeignKeySearcher foreignKeySearcher = new ForeignKeySearcher(SelectedProfile, foreignKeySearchParameter, KeySearchManagerInstance);
            foreignKeySearchParameter.CancellationToken.Register(foreignKeySearcher.CancelledForeignKeySearching);

            foreignKeySearchParameter.ForeignKeyCollectorInstance.AddExistingKeys(foreignKeySearchParameter.ForeignKeysRelatedToTable);

            #region [ ForeignKeyInTable ]

            if (foreignKeySearchParameter.Mode == ForeignKeySearchMode.ForeignKeyInTable || foreignKeySearchParameter.Mode == ForeignKeySearchMode.BothDirections)
            {
                bool processed = InTableFkSearchProcessed(foreignKeySearchParameter);

                #region [ Single Foreign Key search ]

                //Initialize the FK searching
                if (!processed && foreignKeySearcher.InitSimpleForeignKeySearch(foreignKeySearchParameter.BaseTableColumns)) {
                    Action<object> foreignKeyAction = (object obj) => { foreignKeySearcher.StartSimpleForeignKeySearch((ForeignKeySearchParameter) obj); };
                    //Action<object> foreignKeyAction = (object obj, Func<bool> action) => { foreignKeySearcher.StartSimpleForeignKeySearch((ForeignKeySearchParameter)obj, action); };

                    for (int i = 0; i < foreignKeySearchParameter.TaskCount; i++) {
                        task = Task.Factory.StartNew(foreignKeyAction, foreignKeySearchParameter, foreignKeySearchParameter.CancellationToken);
                        tasks.Add(task);
                    }
                }

                #endregion [ Simple Foreign Key search ]

                #region [ Waiting for simple fk search to finish ]

                //DEVNOTE: only run composite fk key search when simple fk search finised to get a reduced input set, hence a column is single foreign key then cannot be part of composite foreign key
                try {
                    Task.WaitAll(tasks.ToArray(), foreignKeySearchParameter.CancellationToken);
                } catch (AggregateException) {
                }

                #endregion [ Waiting for simple fk search to finish ]

                #region [ Composite Foreign Key search ]

                bool hasCompositeKeys = foreignKeySearchParameter.PrimaryKeys.Any(c => c.KeyColumns.Count > 1);

                if (!processed && hasCompositeKeys) {
                    //Initialize the FK searching, using the permutation of columns because the type of the key colums determines the order of the columns 
                    // but we do not know the order so we should produce all the permutations and use the one we need later
                    if (foreignKeySearcher.InitCompositeForeignKeySearch(CreateKeyPermutations(foreignKeySearchParameter.BaseTableColumns, foreignKeySearchParameter.DegreeOfKeyComplexity))) {
                        tasks = new List<Task>();
                        Action<object> foreignKeyAction2 = (object obj) => foreignKeySearcher.StartCompositeForeignKeySearch((ForeignKeySearchParameter)obj);

                        for (int i = 0; i < foreignKeySearchParameter.TaskCount; i++) {
                            task = Task.Factory.StartNew(foreignKeyAction2, foreignKeySearchParameter, foreignKeySearchParameter.CancellationToken);
                            tasks.Add(task);
                        }

                        try{
                            Task.WaitAll(tasks.ToArray(), foreignKeySearchParameter.CancellationToken);
                        }
                        catch(AggregateException){
                        }
                    }
                }
                else
                    _log.Log(LogLevelEnum.Info, "The table doesn't have composite key candidates!", true);

                #endregion [ Composite Foreign Key search ]
            }

            #endregion [ ForeignKeyInTable ]

            #region [ ForeignKeyReferencesForTable Fk search mode ]

            if (foreignKeySearchParameter.Mode == ForeignKeySearchMode.ForeignKeyReferencesForTable || foreignKeySearchParameter.Mode == ForeignKeySearchMode.BothDirections) {

                foreignKeySearchParameter.BaseTableKeys = new ConcurrentQueue<Key>(KeySearchManagerInstance.GetPrimaryKeys(foreignKeySearchParameter.BaseTableId));
                foreignKeySearchParameter.ForeignKeyCandidates = new ConcurrentQueue<KeyCandidate>(KeySearchManagerInstance.GetAllTableColumsAsKeyCandidates(foreignKeySearchParameter.BaseTableId));

                bool processed = FkReferencesForTableProcessed(foreignKeySearchParameter);

                #region [ Single Foreign Key search ]

                //Initialize the foreign key reference search for a set of primary keys
                if (!processed && foreignKeySearcher.InitSimpleForeignKeyForPrimaryKeySearch(foreignKeySearchParameter)) {
                    tasks = new List<Task>();
                    Action<object> foreignKeyAction = (object obj) => { foreignKeySearcher.StartSimpleForeignKeyForPrimaryKeySearher((ForeignKeySearchParameter)obj); };

                    for (int i = 0; i < foreignKeySearchParameter.TaskCount; i++) {
                        task = Task.Factory.StartNew(foreignKeyAction, foreignKeySearchParameter, foreignKeySearchParameter.CancellationToken);
                        tasks.Add(task);
                    }
                }

                #endregion [ Simple Foreign Key search ]

                #region [ Waiting for simple fk search to finish ]

                //DEVNOTE: only run composite fk key search when simple fk search finised to get a reduced input set, hence a column is single foreign key then cannot be part of composite foreign key
                try {
                    Task.WaitAll(tasks.ToArray(), foreignKeySearchParameter.CancellationToken);
                } catch (AggregateException) {
                }

                #endregion [ Waiting for simple fk search to finish ]

                #region [ Composite Foreign Key search ]

                bool hasCompositeKeys = foreignKeySearchParameter.BaseTableKeys.Any(c => c.KeyColumns.Count > 1);

                if (!processed && hasCompositeKeys) {
                    foreignKeySearchParameter.ForeignKeyCandidates = new ConcurrentQueue<KeyCandidate>(CreateKeyPermutationsByTable(foreignKeySearchParameter.ForeignKeyCandidates, foreignKeySearchParameter.BaseTableKeys.Max(k => k.KeyColumns.Count)));

                    //Initialize the FK search, using the permutation of columns because the type of the key colums determines the order of the columns 
                    // but we do not know the order so we should produce all the permutations and use the one we need later
                    if (foreignKeySearcher.InitCompositeForeignKeyForPrimaryKeySearch(foreignKeySearchParameter)) {
                        tasks = new List<Task>();
                        Action<object> foreignKeyAction2 = (object obj) => foreignKeySearcher.StartCompositeForeignKeyForPrimaryKeySearch((ForeignKeySearchParameter)obj);

                        for (int i = 0; i < foreignKeySearchParameter.TaskCount; i++) {
                            task = Task.Factory.StartNew(foreignKeyAction2, foreignKeySearchParameter, foreignKeySearchParameter.CancellationToken);
                            tasks.Add(task);
                        }

                        try {
                            Task.WaitAll(tasks.ToArray(), foreignKeySearchParameter.CancellationToken);
                        } catch (AggregateException) {
                        }
                    }
                } else
                    _log.Log(LogLevelEnum.Info, "The table doesn't have composite key candidates!", true);

                #endregion [ Composite Foreign Key search ]
            }

            #endregion [ ForeignKeyReferencesForTable Fk search mode ]

            stopwatch.Stop();
            _log.Log(LogLevelEnum.Info, "Foreign key search for table " + foreignKeySearchParameter.BaseTableName + " took: " + stopwatch.Elapsed.Minutes.ToString().PadLeft(2, '0') + ":" + stopwatch.Elapsed.Seconds.ToString().PadLeft(2, '0') + "  - " + stopwatch.ElapsedTicks, true);

            foreignKeySearcher.ForeignKeySearchFinished(foreignKeySearchParameter);

            if (foreignKeySearchParameter.CancellationToken.IsCancellationRequested) {
                foreignKeySearchParameter.InitiatedKeySearch.TaskStatus = KeySearchResult.Canceled;
                return foreignKeySearchParameter.InitiatedKeySearch.TaskStatus;
            }
            foreignKeySearchParameter.InitiatedKeySearch.TaskStatus = KeySearchResult.Finished;
            return foreignKeySearchParameter.InitiatedKeySearch.TaskStatus;
        }

        public bool ExportForeignKeys(string fileName) {
            Exception ex = null;
            StringBuilder csvContent = new StringBuilder();
            if (!Utils.FileHelper.IsFileBeingUsed(fileName, out ex)) {
                using (StreamWriter csvWrite = new StreamWriter(File.Open(fileName, FileMode.Create), Encoding.Default)) {
                    csvContent.AppendLine(string.Format("{1}{0}{2}{0}{3}{0}", ";", "Key id", "Processing duration", "Foreign key -> Primary key"));
                    //foreach (ForeignKey key in KeySearchManagerInstance.GetForeignKeys()) {
                    //    csvContent.AppendLine(string.Format("{1}{0}{2}{0}{3}", ";", key.Id, TimeSpan.FromSeconds(key.ProcessingDuration).ToString("g"), key.DisplayString));
                    //}
                    foreach (ForeignKey key in KeySearchManagerInstance.GetForeignKeysLazy()) {
                        Key pk = KeySearchManagerInstance.GetPrimaryKeyLazy(key.KeyId);
                        csvContent.AppendLine(string.Format("{1}{0}{2}{0}{3}", ";", key.Id, TimeSpan.FromSeconds(key.ProcessingDuration).ToString("g"), key.Label + " -> " + pk.TableName + "( " + pk.Label + " )"));
                    }
                    csvWrite.WriteLine(csvContent);
                    csvWrite.Close();
                }
                TimeSpan x = TimeSpan.FromSeconds(231);
            } else {
                return false;
            }

            return true;
        }

        private IEnumerable<KeyCandidate> CreateKeyPermutationsByTable(IEnumerable<KeyCandidate> allTableColumns, int degreeOfKeyComplexity) {

            List<KeyCandidate> keyCandidates = new List<KeyCandidate>();
            IEnumerable<int> tablesInvolved = allTableColumns.Select(c => c.TableId).Distinct();
            foreach (int tableId in tablesInvolved) {
                IEnumerable<KeyCandidate> tableColumns = allTableColumns.Where(c => c.TableId == tableId);
                keyCandidates.AddRange(CreateKeyPermutations(tableColumns, degreeOfKeyComplexity));
            }
            return keyCandidates;
        }

        private IEnumerable<KeyCandidate> CreateKeyPermutations(IEnumerable<KeyCandidate> baseTableColumns, int degreeOfKeyComplexity) {

            if (baseTableColumns.Count() < degreeOfKeyComplexity)
                degreeOfKeyComplexity = baseTableColumns.Count();

            if (degreeOfKeyComplexity == 0) return new ConcurrentQueue<KeyCandidate>();

            //List<KeyCandidate> keyPermutations = new List<KeyCandidate>();
            Dictionary<int, KeyCandidate> keyPermutations = new Dictionary<int, KeyCandidate>();
            string tableName = baseTableColumns.Any() ? baseTableColumns.FirstOrDefault().TableName : null;

            Permutations permutations = new Permutations(baseTableColumns.Select(c => c.CandidateColumns[0]).ToArray());
            while (permutations.MoveNext()) {
                bool skipp = false;
                KeyCandidate key = new KeyCandidate(tableName);
                Array currentCombination = (Array)permutations.Current;
                for (int i = 0; i < degreeOfKeyComplexity; i++) {
                    string keyComposant = ((KeyCandidateColumn)currentCombination.GetValue(i)).ColumnName;
                    if (key.Columns.Count > 0) {
                        if (string.Compare(keyComposant, key.Columns[0].ColumnName, StringComparison.InvariantCultureIgnoreCase) == 0){
                            skipp = true;
                            break;
                        }
                    }
                    KeyCandidate baseKey = baseTableColumns.Where(c => c.CandidateColumns.FirstOrDefault().ColumnName == keyComposant).FirstOrDefault();
                    key.AddColumn(keyComposant, baseKey.CandidateColumns[0].ColumnId, baseKey.CandidateColumns[0].ColumnType);
                    key.NumberOfRows = baseKey.NumberOfRows;
                    key.NumberOfRowsIdx = baseKey.NumberOfRowsIdx;
                    key.TableId = baseKey.TableId;
                }
                if (!skipp) {
                    //if (!keyPermutations.Any(k => k.DisplayString == key.DisplayString))
                    if (!keyPermutations.ContainsKey(key.ColumnOrderIndependentHash)) keyPermutations.Add(key.ColumnOrderIndependentHash, key);
                }
            }
            return new ConcurrentQueue<KeyCandidate>(keyPermutations.Values);
        }

        private IEnumerable<KeyCandidate> CreateKeyCombinations(IEnumerable<KeyCandidate> baseTableColumns, int degreeOfKeyComplexity) {

            if (baseTableColumns.Count() < degreeOfKeyComplexity)
                degreeOfKeyComplexity = baseTableColumns.Count();

            if (degreeOfKeyComplexity == 0) return new ConcurrentQueue<KeyCandidate>();

            //List<KeyCandidate> keyCombinations = new List<KeyCandidate>();
            Dictionary<int, KeyCandidate> keyCombinations = new Dictionary<int, KeyCandidate>();
            string tableName = baseTableColumns.Any() ? baseTableColumns.FirstOrDefault().TableName : null;
            
            Combinations combinations = new Combinations(baseTableColumns.Select(c => c.CandidateColumns[0]).ToArray(), degreeOfKeyComplexity);
            while (combinations.MoveNext()) {
                bool skipp = false;
                KeyCandidate key = new KeyCandidate(tableName);
                Array currentCombination = (Array)combinations.Current;
                for (int i = 0; i < currentCombination.Length; i++) {
                    //int nVali = (int)currentCombination.GetValue(i);	// Just access the value. This requres boxing.
                    //Object nVal = currentCombination.GetValue(i);		// Just access the value. This requres no boxing.
                    string keyComposant = ((KeyCandidateColumn)currentCombination.GetValue(i)).ColumnName;
                    if (key.Columns.Count > 0) {
                        if (string.Compare(keyComposant, key.Columns[0].ColumnName, StringComparison.InvariantCultureIgnoreCase) == 0) {
                            skipp = true;
                            break;
                        }
                    }
                    KeyCandidate baseKey = baseTableColumns.Where(c => c.CandidateColumns.FirstOrDefault().ColumnName == keyComposant).FirstOrDefault();
                    key.AddColumn(keyComposant, baseKey.CandidateColumns[0].ColumnId, baseKey.CandidateColumns[0].ColumnType);
                    key.NumberOfRows = baseKey.NumberOfRows;
                    key.NumberOfRowsIdx = baseKey.NumberOfRowsIdx;
                    key.TableId = baseKey.TableId;
                }
                if (!skipp) {
                    //if (!keyCombinations.Any(k => k.DisplayString == key.DisplayString))
                    if (!keyCombinations.ContainsKey(key.ColumnOrderIndependentHash)) keyCombinations.Add(key.ColumnOrderIndependentHash, key);
                }
            }
            return new ConcurrentQueue<KeyCandidate>(keyCombinations.Values);
        }

        /// <summary>
        /// Gets whether keys exists
        /// </summary>
        /// <returns>True if there are keys existing otherwise false</returns>
        public bool ExistTable<T>() where T : new() {
            return KeySearchManagerInstance.ExistTable<T>();
        }

        /// <summary>
        /// Gets whether keys exists
        /// </summary>
        /// <returns>True if there are keys existing otherwise false</returns>
        public bool KeysExists() {
            return KeySearchManagerInstance.KeysExists();
        }

        /// <summary>
        /// Gets the keys found
        /// </summary>
        /// <returns>The list of the keys</returns>
        public IEnumerable<Key> GetKeys() {
            return KeySearchManagerInstance.GetPrimaryKeys();
        }
        
        /// <summary>
        /// Gets all tables
        /// </summary>
        /// <param name="skippedTableId">The table id to skip</param>
        /// <returns>The list of the keys</returns>
        public List<Table> GetAllTables() {
            //return KeySearchManagerInstance.AllTables;
            List<Table> tables = new List<Table>();
            foreach (KeyValuePair<int, string> table in KeySearchManagerInstance.AllTables) {
                tables.Add(new Table(table.Key, table.Value));
            }
            return tables;
        }

        /// <summary>
        /// Gets all possible keys
        /// </summary>
        /// <param name="skippedTableId">The table id to skip</param>
        /// <returns>The list of the keys</returns>
        public ConcurrentQueue<KeyCandidate> GetAllColumns(int skippedTableId){
            return new ConcurrentQueue<KeyCandidate>(KeySearchManagerInstance.GetAllTableColumsAsKeyCandidates(skippedTableId));
        }

        /// <summary>
        /// Gets all columns for a table as a key
        /// </summary>
        /// <param name="tableName">The table name</param>
        /// <returns>The list of the columns as a key</returns>
        public ConcurrentQueue<KeyCandidate> GetAllColumnsForTable(string tableName) {
            return new ConcurrentQueue<KeyCandidate>(KeySearchManagerInstance.GetAllColumnsForTableAsKeyCandidates(tableName));
        }

        /// <summary>
        /// The timer ellapsed -> Write performance log
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Event</param>
        private void _myTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e){
            _log.Log(LogLevelEnum.Info, GetPerformanceLog(), true);
        }

        /// <summary>
        /// Create a performance log text
        /// </summary>
        internal string GetPerformanceLog(){
            PerformanceItem performanceItem = new PerformanceItem();
            return "CPU usage: " + performanceItem.CpuUsage + "% free RAM: " + performanceItem.RamFree + " allocated RAM: " + performanceItem.RamAllocated;
        }

        /// <summary>
        /// Recreate the datatables if not exist
        /// </summary>
        public void ReCreateTables() {
            using (var conn = ConnectionManager.CreateConnection(KeySearchManager.GetSearchDbConfig(SelectedProfile.DbConfigView))) {
                conn.Open();
                if (keyCollectorInstance == null) {
                    try {
                        keyCollectorInstance = new KeyCollector();
                        keyCollectorInstance.ReCreateTables(conn);
                    }
                    finally {
                        keyCollectorInstance = null;
                    }
                } 
                else keyCollectorInstance.ReCreateTables(conn);
                if (foreignKeyCollectorInstance == null) {
                    try {
                        foreignKeyCollectorInstance = new ForeignKeyCollector();
                        foreignKeyCollectorInstance.ReCreateTables(conn);   
                    }
                    finally {
                        foreignKeyCollectorInstance = null;
                    }
                } 
                else foreignKeyCollectorInstance.ReCreateTables(conn);   
                conn.Close();
            }
        }

        /// <summary>
        /// Recreate the datatables if not exist
        /// </summary>
        public void ReCreateTempTables() {
            using (var conn = ConnectionManager.CreateConnection(KeySearchManager.GetSearchDbConfig(SelectedProfile.DbConfigView))) {
                conn.Open();
                if (keyCollectorInstance != null) keyCollectorInstance.ReCreateTempTables(conn);
                conn.Close();
            }
        }

        #endregion Methods
    }
}
