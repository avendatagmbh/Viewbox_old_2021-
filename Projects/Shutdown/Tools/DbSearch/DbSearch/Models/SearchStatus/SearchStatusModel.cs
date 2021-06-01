using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DbSearchLogic.Manager;
using DbSearchLogic.ProcessQueue;
using DbSearchLogic.SearchCore.KeySearch;
using DbSearchLogic.SearchCore.QueryExecution;
using DbSearchLogic.SearchCore.Structures;
using DbSearchLogic.SearchCore.Threading;
using Utils;
using AV.Log;

namespace DbSearch.Models.SearchStatus {
    public class SearchStatusModel : INotifyPropertyChanged {
        
        #region [ Members ]

        private static SemaphoreSlim _semaphoreAddItem = new SemaphoreSlim(1);
        private static SemaphoreSlim _semaphoreStartItem = new SemaphoreSlim(1);

        #endregion [ Members ]

        #region [ Events ]

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName) { if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName)); }
        #endregion Events

        #region Constructor
        public SearchStatusModel(ObservableCollectionAsync<IKeySearchStatEntry> initiatedKeySearches) {
            _initiatedKeySearches = initiatedKeySearches;
            QueryInfos.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(QueryInfos_CollectionChanged);
            if (InitiatedKeySearches != null)
                InitiatedKeySearches.CollectionChanged += InitiatedKeySearchesOnCollectionChanged;
        }

        #endregion [ Events ]

        #region [ Properties ]

        public ObservableCollectionAsync<IThreadExecutor> Threads {
            get { return ThreadManager.RunningThreads; }
        }

        public ObservableCollection<QueryInfo> QueryInfos { get { return ThreadManager.QueryInfos; } }

        private ObservableCollectionAsync<IKeySearchStatEntry> _initiatedKeySearches;
        public ObservableCollectionAsync<IKeySearchStatEntry> InitiatedKeySearches { get { return _initiatedKeySearches; } }

        private QueryInfo _selectedQuery;
        public QueryInfo SelectedQuery {
            get { return _selectedQuery; }
            set {
                if (_selectedQuery != value) {
                    _selectedQuery = value;
                    OnPropertyChanged("SelectedQuery");
                }
            }
        }

        public ObservableCollectionAsync<LogEntry> LogEntries {
            get { return LogHelper.LogEntries; }
        }

        #endregion [ Properties ]

        #region [ EventHandler ]

        private void QueryInfos_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            if (QueryInfos.Count == 1) {
                SelectedQuery = QueryInfos[0];
            }
        }

        private void InitiatedKeySearchesOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs) {
            //if (notifyCollectionChangedEventArgs.NewItems != null) {
            //    foreach (var newItem in notifyCollectionChangedEventArgs.NewItems) {
            //        IKeySearchStatEntry kStat = (newItem as IKeySearchStatEntry);
            //        kStat.Deleted += KeyStatOnDeleted;
            //        kStat.KeySearchTask.Start();
            //    }
            //}

            _semaphoreAddItem.Wait();
            try {
                if (notifyCollectionChangedEventArgs.NewItems != null) {
                    foreach (var newItem in notifyCollectionChangedEventArgs.NewItems) {
                        IKeySearchStatEntry kStat = (newItem as IKeySearchStatEntry);
                        kStat.Deleted += KeyStatOnDeleted;
                        kStat.NotifyCompleted += KStatOnNotifyCompleted;
                    }
                }
                StartNewTask();
            } finally
            {
                _semaphoreAddItem.Release();
            }
        }

        private void KStatOnNotifyCompleted(object sender, EventArgs eventArgs) {
            StartNewTask();
        }
        
        private void StartNewTask() {
            _semaphoreStartItem.Wait();
            try {
                if (!InitiatedKeySearches.Any(i => i.IsRunning)) {
                    IKeySearchStatEntry item = InitiatedKeySearches.Where(i => i.IsWaiting).FirstOrDefault();
                    if (item != null) if (item.KeySearchTask.Status != TaskStatus.Running) item.KeySearchTask.Start();
                }
            } finally {
                _semaphoreStartItem.Release();
            }
        }

        private void KeyStatOnDeleted(object sender, EventArgs eventArgs) {
            IEnumerator<IKeySearchStatEntry> enumerator = InitiatedKeySearches.GetEnumerator();
            List<int> removable = new List<int>();
            int i = 0;
            while (enumerator.MoveNext()) {
                if (enumerator.Current.IsDeleted) removable.Add(i);
                i++;
            }
            removable.Reverse();
            foreach (int removableSequence in removable) {
                InitiatedKeySearches.RemoveAt(removableSequence);
            }
        }

        #endregion [ EventHandler ]

        #region [ Public methods ]

        public void DeleteSelectedKeySearches(IList selectedList) {
            // DEVNOTE: creating selected enum to loose the reference (the collection cannot be modified)
            IEnumerable<IKeySearchStatEntry> selected = new List<IKeySearchStatEntry>(selectedList.OfType<IKeySearchStatEntry>()).Select(s => s);
            foreach (IKeySearchStatEntry item in selected) {
                item.DeleteTask();
            }
        }

        public void CancelSelectedKeySearches(IList selectedList) {
            foreach (IKeySearchStatEntry item in selectedList.OfType<IKeySearchStatEntry>()) {
                item.CancelTask();
            }
        }

        #endregion [ Public methods ]
    }
}
