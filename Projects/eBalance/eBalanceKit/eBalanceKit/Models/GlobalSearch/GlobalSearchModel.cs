using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Utils;
using Utils.Commands;
using eBalanceKitBase.Interfaces;
using eBalanceKitBusiness.Manager;
using eBalanceKitBusiness.Reconciliation.ViewModels;
using eBalanceKitBusiness.Structures.GlobalSearch;
using eBalanceKitResources.Localisation;

namespace eBalanceKit.Models.GlobalSearch {
    class GlobalSearchModel : NotifyPropertyChangedBase {

        #region constructor
        public GlobalSearchModel(eBalanceKitBusiness.Structures.DbMapping.Document document, Action<object> setGeneralTab, INavigationTree navigationTree, bool isReconciliation = false, ReconciliationsModel reconciliationsModel = null) {
            SearchInLabel = true;
            if (isReconciliation) {
                Searcher = new ReconciliationSearcher(document, reconciliationsModel);
            } else {
                Searcher = new GlobalSearcher(document, setGeneralTab, navigationTree);
            }
            _tokenSource = new CancellationTokenSource();
            SearchCommand = new DelegateCommand(o => true, o => {
                // there is the cancellation of the just in time search if the search button is hit, or pressing enter.
                _tokenSource.Cancel();
                _tokenSource = new CancellationTokenSource();
                // set the culture for the task.
                Searcher.SaveHistory(SearchString, _tokenSource.Token);
                OnPropertyChanged("SearchString");
                Task.Factory.StartNew(StartSearch,
                                      new Tuple<CultureInfo, CultureInfo, string, bool, bool, SearcherBase>(
                                          Thread.CurrentThread.CurrentCulture, Thread.CurrentThread.CurrentUICulture,
                                          SearchString, SearchInId, SearchInLabel, Searcher));
            });
        }
        #endregion

        #region properties
        #region SearchString
        private string _searchString;

        public string SearchString {
            get { return _searchString; }
            set {
                if (_searchString != value) {
                    _searchString = value;
                    OnPropertyChanged("SearchString");
                }
            }
        }
        #endregion SearchString

        #region SelectedResultItem
        private IGlobalSearcherTreeNode _selectedResultItem;

        public IGlobalSearcherTreeNode SelectedResultItem {
            get { return _selectedResultItem; }
            set {
                if (_selectedResultItem != value) {
                    _selectedResultItem = value;
                    if (value != null && value is SearchResultItem) {
                        var val = value as SearchResultItem;
                        Searcher.BringIntoView(val);
                        GlobalResources.Info.SelectedElement = (val).PresentationTreeEntryPath.Last().Element;
                    }
                    OnPropertyChanged("SelectedResultItem");
                }
            }
        }
        #endregion SelectedResultItem

        public SearcherBase Searcher { get; set; }

        public GlobalSearchHistoryManager HistoryManager { get { return GlobalSearchHistoryManager.Instance; } }
        /// <summary>
        /// true if any of the 2 checkbox is set.
        /// </summary>
        public bool CanSearch {
            get { return _searchInId || _searchInLabel; }
        }

        #region SearchInId
        private bool _searchInId;

        /// <summary>
        /// also sets the CanSearch
        /// </summary>
        public bool SearchInId {
            get { return _searchInId; }
            set {
                if (_searchInId != value) {
                    _searchInId = value;
                    OnPropertyChanged("SearchInId");
                    //OnPropertyChanged("CanSearch");
                    //if (_tokenSource != null) {
                    //    // there is the cancellation of the just in time search if the search in id is choosen
                    //    _tokenSource.Cancel();
                    //    _tokenSource = new CancellationTokenSource();
                    //    // set the culture for the task.
                    //    Task.Factory.StartNew(StartSearch,
                    //                          new Tuple<CultureInfo, CultureInfo, string, bool, bool, SearcherBase>(
                    //                              Thread.CurrentThread.CurrentCulture,
                    //                              Thread.CurrentThread.CurrentUICulture, SearchString, value,
                    //                              SearchInLabel, Searcher));
                    //}
                }
            }
        }
        #endregion SearchInId

        #region SearchInLabel
        private bool _searchInLabel;

        /// <summary>
        /// also sets the CanSearch
        /// </summary>
        public bool SearchInLabel {
            get { return _searchInLabel; }
            set {
                if (_searchInLabel != value) {
                    _searchInLabel = value;
                    OnPropertyChanged("SearchInLabel");
                    OnPropertyChanged("CanSearch");
                    if (_tokenSource != null) {
                        // there is the cancellation of the just in time search if the search if the search in label is choosen
                        _tokenSource.Cancel();
                        _tokenSource = new CancellationTokenSource();
                        // set the culture for the task.
                        Task.Factory.StartNew(StartSearch,
                                              new Tuple<CultureInfo, CultureInfo, string, bool, bool, SearcherBase>(
                                                  Thread.CurrentThread.CurrentCulture,
                                                  Thread.CurrentThread.CurrentUICulture, SearchString, !value,
                                                  value, Searcher));
                    }
                }
            }
        }
        #endregion SearchInLabel

        public DelegateCommand SearchCommand { get; set; }

        public eBalanceKitBusiness.Structures.DbMapping.Document CurrentDocument { get { return DocumentManager.Instance.CurrentDocument; } }
        
        #endregion properties

        #region fields
        private CancellationTokenSource _tokenSource;
        private Task _justInTimeTask;
        // DEVNOTE: if we change back to Thread control the commented code have to be uncommented, and a new binding should be added at constructor for CommandBinding.
        //private Thread _justInTimeTask;
        #endregion fields

        #region consts
        private const int MinCharsToSearch = 3;
        private const int BeforeSearchDelay = 300;
        private static bool _available = true;
        private static readonly SemaphoreSlim Semaphore = new SemaphoreSlim(1);
        #endregion

        #region methods
        /// <summary>
        /// start a new search with button, or hitting enter. No delay.
        /// </summary>
        /// <param name="state">state of culture</param>
        public void StartSearch(object state) {
            try {
                // the previous search is aborted at CommandBinding.
                // read the culture info. Must be set at invoke, or binding, and read here.
                var tuple = state as Tuple<CultureInfo, CultureInfo, string, bool, bool, SearcherBase>;
                Debug.Assert(tuple != null);
                Thread.CurrentThread.CurrentCulture = tuple.Item1;
                Thread.CurrentThread.CurrentUICulture = tuple.Item2;
                if (tuple.Item6 == null) return;
                if (!_available)
                    return;
                Semaphore.Wait();
                if (!_available)
                    return;
                _available = false;
                tuple.Item6.Search(tuple.Item3, tuple.Item4, tuple.Item5, _tokenSource.Token);
                if (tuple.Item6.ResultTreeRoots.Count == 0) {
                    MessageBox.Show(ResourcesGlobalSearch.NoResults, "", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            } finally {
                Semaphore.Release();
                _available = true;
            }
            //Task.Factory.StartNew(() => GlobalSearcher.Search(SearchString, SearchInId, SearchInLabel, token), token);
            //Task.Factory.StartNew(
            //    (state) => StartJustInTimeSearch(SearchString, SearchInId, SearchInLabel, GlobalSearcher, token, state),
            //    new Tuple<CultureInfo, CultureInfo>(Thread.CurrentThread.CurrentCulture,
            //                                        Thread.CurrentThread.CurrentUICulture), token);
            //GlobalSearcher.SaveHistory(SearchString);
            //if (GlobalSearcher.Results.Count == 0) {
            //    MessageBox.Show(eBalanceKitResources.Localisation.ResourcesGlobalSearch.NoResults);
            //}
        }

        public void ExpandAllNodes() { Searcher.ExpandAllNodes(_tokenSource.Token); }

        public void CollapseAllNodes() { Searcher.CollapseAllNodes(); }

        public void SelectionChanged(string selectedItem) {
            if (_tokenSource != null) {
                // there is the cancellation of the just in time search if the search if the search in label is choosen
                _tokenSource.Cancel();
                _tokenSource = new CancellationTokenSource();
                // set the culture for the task.
                Searcher.SaveHistory(selectedItem, _tokenSource.Token);
                OnPropertyChanged("SearchString");
                Task.Factory.StartNew(StartSearch,
                                      new Tuple<CultureInfo, CultureInfo, string, bool, bool, SearcherBase>(
                                          Thread.CurrentThread.CurrentCulture, Thread.CurrentThread.CurrentUICulture,
                                          selectedItem, SearchInId, SearchInLabel, Searcher));
            }
        }

        public void KeyUp(Key key) {
            OnPropertyChanged("SearchString");
            // abort the previous search
            if (_justInTimeTask != null && !_justInTimeTask.IsCompleted) {
                //if (_justInTimeTask != null && _justInTimeTask.IsAlive){
                _tokenSource.Cancel();
                //_justInTimeTask.Abort();
            }
            // search if enough caracter is pressed, and it's not an enter.
            if (SearchString == null || SearchString.Length < MinCharsToSearch || key == Key.Enter)
                return;
            // start a new just in time search. the culture is passed by the state parameter.
            _tokenSource = new CancellationTokenSource();
            CancellationToken token = _tokenSource.Token;
            _justInTimeTask = Task.Factory.StartNew(state => StartJustInTimeSearch(token, state),
                                                    new Tuple
                                                        <CultureInfo, CultureInfo, string, bool, bool, SearcherBase>(
                                                        Thread.CurrentThread.CurrentCulture,
                                                        Thread.CurrentThread.CurrentUICulture, SearchString, SearchInId,
                                                        SearchInLabel, Searcher), token);
            //_justInTimeTask = new Thread(StartJustInTimeSearch) {
            //    CurrentCulture = Thread.CurrentThread.CurrentCulture,
            //    CurrentUICulture = Thread.CurrentThread.CurrentUICulture
            //};
            //Tuple<string, bool, bool, GlobalSearcher> param = new Tuple<string, bool, bool, GlobalSearcher>(
            //    SearchString, SearchInId, SearchInLabel, GlobalSearcher);
            //_justInTimeTask.Start(param);
        }

        /* private static void StartJustInTimeSearch(object param) {
            try {
                Thread.Sleep(BeforeSearchDelay);
                Tuple<string, bool, bool, GlobalSearcher> tupleParam = param as Tuple<string, bool, bool, GlobalSearcher>;
                Debug.Assert(tupleParam != null);
                // should add _semafore stuff
                tupleParam.Item4.Search(tupleParam.Item1, tupleParam.Item2, tupleParam.Item3);
            } catch (ThreadAbortException) {
            }
        }*/

        /// <summary>
        /// give every info about the search to the task, wait BeforeSearchDelay millisec, than start a search. This type of method is invoked with Task.StartNew
        /// </summary>
        /// <param name="token">cancellation token is passed to the global searcher</param>
        /// <param name="state">contains information about the culture, and local variables. Must be set at invoke, and read in the begining</param>
        private static void StartJustInTimeSearch(CancellationToken token, object state) {
            try {
                // reading the culture info.
                var tuple = state as Tuple<CultureInfo, CultureInfo, string, bool, bool, SearcherBase>;
                Debug.Assert(tuple != null);
                Thread.CurrentThread.CurrentCulture = tuple.Item1;
                Thread.CurrentThread.CurrentUICulture = tuple.Item2;
                Thread.Sleep(BeforeSearchDelay);
                if (token.IsCancellationRequested)
                    //token.ThrowIfCancellationRequested();
                    return;
                if (!_available)
                    return;
                Semaphore.Wait();
                if (!_available)
                    return;
                _available = false;
                tuple.Item6.Search(tuple.Item3, tuple.Item4, tuple.Item5, token);
            } finally {
                Semaphore.Release();
                _available = true;
            }
        }

        #endregion methods
    }
}
