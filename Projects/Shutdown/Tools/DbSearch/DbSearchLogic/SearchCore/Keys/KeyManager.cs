using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using AvdCommon.Logging;
using DbSearchBase.Enums;
using DbSearchBase.Interfaces;
using DbSearchLogic.Config;
using DbSearchLogic.SearchCore.ForeignKeySearch;
using DbSearchLogic.SearchCore.KeySearch;
using DbSearchLogic.SearchCore.Threading;
using AV.Log;
using Utils;
using log4net;

namespace DbSearchLogic.SearchCore.Keys {

    public class SelectedChangedEventArgs : EventArgs {
        public bool PreviousState { get; private set; }
        public bool CurrentState { get; private set; }

        public SelectedChangedEventArgs(bool previousState, bool currentState) {
            PreviousState = previousState;
            CurrentState = currentState;
        }
    }

    public class KeyManager : NotifyPropertyChangedBase, IDisposable {

        #region [ Members ]

        internal ILog _log = LogHelper.GetLogger();
        private bool _disposed = false;
        private CancellationTokenSource _cancellationTokenSource;
        private KeySearchManager _searchManager;
        public delegate void SelectedChangedEventHandler(IDisplayKey displayKey, SelectedChangedEventArgs args);
        private Func<IDisplayDbKey, IKey> initAction;
        private Func<IDisplayKey, ObservableCollection<IDisplayKey>> loadSubKeyAction;

        /// <summary>
        /// Create a semaphore instance
        /// </summary>
        private static SemaphoreSlim _semaphoreKeyTables = new SemaphoreSlim(1);
        
        #endregion [ Members ]

        #region [ Properties ]

        private ObservableCollectionAsync<KeyTable> _keyTables = null;
        public ObservableCollectionAsync<KeyTable> KeyTables { get { return GetInitKeyTables(); } }

        private IDisplayKey _selectedKey = null;
        public IDisplayKey SelectedKey { get { return _selectedKey; } }

        private bool _keysInitialized = false;
        public bool KeysInitialized { get { return _keysInitialized; } }

        /// <summary>
        /// The key result filter
        /// </summary>
        //public IEnumerable<BindableDbColumnType> ShowColumnTypesFilter { get; set; }
        public IKeyResultFilter ResultFilter { get; set; }

        #endregion [ Properties ]

        #region [ Constructors ]

        public KeyManager(KeySearchManager searchManager) {

            _cancellationTokenSource = new CancellationTokenSource();
            _searchManager = searchManager;
            initAction = LoadDisplayKey;
            loadSubKeyAction = LoadRelatedForeignKeys;

            ResultFilter = new KeyResultFilter();
            ResultFilter.ColumnTypesToShow = DbColumnTypeHelper.Instance.BindableDbColumnTypeList();

            BackgroundWorker backgroundWorker = new BackgroundWorker();
            backgroundWorker.DoWork += new DoWorkEventHandler(DoInitKeyTables);
            if (backgroundWorker.IsBusy != true) {
                backgroundWorker.RunWorkerAsync();
            }
        }
        
        #endregion [ Constructors ]

        #region [ Public members ]

        public Profile Profile {
            get {
                if (_searchManager != null && _searchManager.CurrentProfile != null)
                    return _searchManager.CurrentProfile;
                return null;
            }
        }

        #endregion [ Public members ]

        #region [ Public methods ]

        public void RefreshVisibleKeyList() {

            foreach (KeyTable keyTable in KeyTables) {
                bool hasVisibleChild = SetVisibilityByFilter(keyTable);
                if (!hasVisibleChild) keyTable.IsVisible = false;
            }
            OnPropertyChanged("KeyTables");
        }

        public void Cancel() {
            _cancellationTokenSource.Cancel();
        }

        #endregion [ Public methods ]

        #region [ Private methods ]

        private void DoInitKeyTables(object sender, DoWorkEventArgs e) {
            InitKeyTables();
        }

        private void InitKeyTables() {
            if (_keyTables == null) {
                _semaphoreKeyTables.Wait();
                try {
                    if (_keyTables == null) {
                        _cancellationTokenSource = new CancellationTokenSource();
                        _keyTables = new ObservableCollectionAsync<KeyTable>();
                        Dictionary<int, string> tables = _searchManager.AllTables;
                        foreach (KeyValuePair<int, string> table in tables) {
                            if (!_cancellationTokenSource.IsCancellationRequested) {
                                KeyTable tk = new KeyTable(table.Value, table.Key, KeyOnIsSelectedChanged, initAction, loadSubKeyAction, _searchManager);
                                tk.OnIsSelectedChanged += KeyOnIsSelectedChanged;
                                _keyTables.Add(tk);
                            }
                            else
                                _keyTables = null;
                        }
                    }
                } finally {
                    _semaphoreKeyTables.Release();
                }
            }
        }

        private void InitKeys() {
            if (!_keysInitialized) {
                _semaphoreKeyTables.Wait();
                try {
                    if (!_keysInitialized) {
                        _log.Log(LogLevelEnum.Info, "Keys are being initialized for database " + _searchManager.DbConfig.DbName + ".", true);
                        foreach (KeyTable keyTable in _keyTables) {
                            if (!_cancellationTokenSource.IsCancellationRequested)
                                InitKeyTable(keyTable);
                            else
                                break;
                        }
                        _keysInitialized = true;
                        OnPropertyChanged("KeysInitialized");
                        if (!_cancellationTokenSource.IsCancellationRequested)
                            _log.Log(LogLevelEnum.Info, "Keys are initialized for database " + _searchManager.DbConfig.DbName + ".", true);
                        else
                            _log.Log(LogLevelEnum.Info, "Key initialization for database " + _searchManager.DbConfig.DbName + " is interrupted.", true);
                    }
                } finally {
                    _semaphoreKeyTables.Release();
                }
            }
        }

        private void InitKeyTable(KeyTable keyTable) {
            List<Key> primaryKeys = _searchManager.GetPrimaryKeysLazy(keyTable.Id);
            List<ForeignKey> foreignKeys = _searchManager.GetForeignKeysLazy(keyTable.Id);
            keyTable.Init(primaryKeys.Where(pk => string.Compare(pk.TableName, keyTable.Label, StringComparison.InvariantCultureIgnoreCase) == 0),
                            foreignKeys.Where(fk => string.Compare(fk.TableName, keyTable.Label, StringComparison.InvariantCultureIgnoreCase) == 0),
                            null);
        }

        private ObservableCollectionAsync<KeyTable> GetInitKeyTables() {
            InitKeyTables();
            if (_cancellationTokenSource.IsCancellationRequested) return _keyTables;

            BackgroundWorker backgroundWorker = new BackgroundWorker();
            backgroundWorker.DoWork += delegate(object o, DoWorkEventArgs args) { InitKeys(); };
            if (backgroundWorker.IsBusy != true) {
                backgroundWorker.RunWorkerAsync();
            }
            return _keyTables;
        }

        private IKey LoadDisplayKey(IDisplayDbKey displayKey) {
            return _searchManager.LoadDisplayKey(displayKey);
        }

        private ObservableCollection<IDisplayKey> LoadRelatedForeignKeys(IDisplayKey displayKey) {
            List<ForeignKey> foreignKeys = new List<ForeignKey>(_searchManager.GetForeignKeysRelatedToKeyLazy(displayKey.Key.Id).OrderBy(k => k.Label));
            return new ObservableCollection<IDisplayKey>(foreignKeys.Select(k => KeySearchManager.CreateRelatedKey(displayKey, k, KeyOnIsSelectedChanged, initAction, loadSubKeyAction, _searchManager)));
        }

        private bool SetVisibilityByFilter(IDisplayKey key) {
            if (key.Key != null && !key.Key.IsInitialized) key.Key.Initialize();
            bool hasVisibleChild = false;
            key.LoadSubKeys();
            if (key.Children != null) {
                foreach (IDisplayKey child in key.Children) {
                    if (SetVisibilityByFilter(child)) hasVisibleChild = true;
                }
            }

            if (key is KeyTable) key.IsVisible = hasVisibleChild;
            else key.IsVisible = SetVisibility(key);

            if (key.IsVisible) return true;
            if (hasVisibleChild) return true;
            return false;
        }

        private bool SetVisibility(IDisplayKey key) {
            //if (key.Key is IDisplayPrimaryKey) {
            //    IDisplayPrimaryKey pk = key.Key as IDisplayPrimaryKey;
            //    if (!pk.Columns.Any(c => ResultFilter.ColumnTypesToShow.Any(df => df.IsSelected && df.DbType == c.ColumnType)))
            //        key.IsVisible = false;
            //}
            //else if (key.Key is IDisplayForeignKey) {
            //    IDisplayForeignKey fk = key.Key as IDisplayForeignKey;
            //    if (fk.PrimaryKeyColumns.Any(c => ResultFilter.ColumnTypesToShow.Any(df => df.IsSelected && df.DbType == c.ColumnType)))
            //        key.IsVisible = true;
            //    else if (fk.ForeignKeyColumns.Any(c => ResultFilter.ColumnTypesToShow.Any(df => df.IsSelected && df.DbType == c.ColumnType)))
            //        key.IsVisible = true;
            //    else
            //        key.IsVisible = false;
            //}
            //return key.IsVisible;

            if (key.Key is IDisplayPrimaryKey) {
                return ResultFilter.IsVisible(((IDisplayPrimaryKey) key.Key).Columns);
            }
            else if (key.Key is IDisplayForeignKey) {
                return ResultFilter.IsVisible(((IDisplayForeignKey)key.Key).ForeignKeyColumns) && ResultFilter.IsVisible(((IDisplayForeignKey)key.Key).PrimaryKeyColumns);
            }
            return false;
        }

        #endregion [ Private methods ]

        #region [ Events ]

        public event SelectedChangedEventHandler OnIsSelectedChanged;
        public void KeyOnIsSelectedChanged(IDisplayKey displayKey, SelectedChangedEventArgs args) {

            if (args.CurrentState && args.PreviousState != args.CurrentState) {
                _selectedKey = displayKey;
                if (this.OnIsSelectedChanged != null)
                    this.OnIsSelectedChanged(_selectedKey, new SelectedChangedEventArgs(args.PreviousState, args.CurrentState));
                OnPropertyChanged("SelectedKey");
            }
        }

        #endregion [ Events ]

        public void Dispose() {
            if (!_cancellationTokenSource.IsCancellationRequested)
                _cancellationTokenSource.Cancel();
        }

        ~KeyManager() {
            if (!_disposed)
                Dispose();
        }
    }
}
