using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using DbSearchBase.Interfaces;
using DbSearchLogic.Config;
using DbSearchLogic.SearchCore.ForeignKeySearch;
using DbSearchLogic.SearchCore.KeySearch;
using Utils;

namespace DbSearchLogic.SearchCore.Keys {

    public class KeyTable : NotifyPropertyChangedBase, IDisplayKey {

        #region [ Members ]

        private string _tableName;
        private int _tableId;
        private ObservableCollectionAsync<IDisplayKey> _keys;
        private KeyManager.SelectedChangedEventHandler _onSelectionChangedEventHandler;
        private Func<IDisplayDbKey, IKey> _initAction;
        private Func<IDisplayKey, ObservableCollection<IDisplayKey>> _loadSubkeyAction;
        private KeySearchManager _keySearchManager;

        #endregion [ Members ]

        #region [ Constructors ]

        public KeyTable(string tableName, int tableId, KeyManager.SelectedChangedEventHandler actionOnSelected, Func<IDisplayDbKey, IKey> initAction, Func<IDisplayKey, ObservableCollection<IDisplayKey>> loadSubkeyAction, KeySearchManager keySearchManager) {
            _tableName = tableName;
            _tableId = tableId;
            _keys = new ObservableCollectionAsync<IDisplayKey>();
            _onSelectionChangedEventHandler = actionOnSelected;
            _initAction = initAction;
            _loadSubkeyAction = loadSubkeyAction;
            _keySearchManager = keySearchManager;
        }

        public KeyTable(string tableName, int tableId, IEnumerable<IKey> primaryKeys, IEnumerable<IForeignKey> foreignKeys,
                        IEnumerable<ForeignKey> primaryKeyRelations, KeyManager.SelectedChangedEventHandler actionOnSelected, Func<IDisplayDbKey, IKey> initAction, Func<IDisplayKey, ObservableCollection<IDisplayKey>> loadSubkeyAction, KeySearchManager keySearchManager) {
            
            _tableName = tableName;
            _tableId = tableId;
            _keys = new ObservableCollectionAsync<IDisplayKey>();
            _onSelectionChangedEventHandler = actionOnSelected;
            _initAction = initAction;
            _loadSubkeyAction = loadSubkeyAction;
            _keySearchManager = keySearchManager;

            Init(primaryKeys, foreignKeys, primaryKeyRelations);
        }

        #endregion [ Constructors ]

        #region [ Init ]

        public void Init(IEnumerable<IKey> primaryKeys, IEnumerable<IForeignKey> foreignKeys, IEnumerable<ForeignKey> primaryKeyRelations) {
            if (_keys.Count > 0) return;
            IEnumerable<DisplayKey> relatedKeys = null;
            foreach (IKey primaryKey in primaryKeys.OrderBy(k => k.Label)) {
                if (primaryKeyRelations != null) {
                    relatedKeys = primaryKeyRelations.Where(fk => string.Compare(fk.PrimaryKeyDisplayString, primaryKey.DisplayString, StringComparison.InvariantCulture) == 0).Select(k => KeySearchManager.CreateRelatedKey((IDisplayKey)primaryKey, k, _onSelectionChangedEventHandler, _initAction, _loadSubkeyAction, _keySearchManager));
                }
                DisplayKey dKey = new DisplayKey(primaryKey, (relatedKeys == null ? relatedKeys : relatedKeys.OrderBy(k => k.Label)), _initAction, _loadSubkeyAction, _keySearchManager);
                dKey.OnIsSelectedChanged += _onSelectionChangedEventHandler;
                _keys.Add(dKey);
            }
            foreach (IForeignKey foreignKey in foreignKeys.OrderBy(k => k.Label)) {
                DisplayKey dKey = new DisplayKey(foreignKey, new List<IDisplayKey>(), _initAction, _loadSubkeyAction, _keySearchManager);
                dKey.OnIsSelectedChanged += _onSelectionChangedEventHandler;
                _keys.Add(dKey);
            }
            IsInitialized = true;
            OnPropertyChanged("IsInitialized");
        }

        #endregion [ Init ]

        public KeySearchManager KeySearchManagerInstance { get { return _keySearchManager; } }

        public int Id { get { return _tableId; } }

        public IDisplayKey Parent { get { return null; } }

        public IDisplayDbKey Key { get { return null; } }

        public string Label { get { return _tableName; } }

        public ObservableCollection<IDisplayKey> Children { get { return _keys; } }

        public event KeyManager.SelectedChangedEventHandler OnIsSelectedChanged;

        private bool _isSelected;
        public bool IsSelected {
            get { return _isSelected; }
            set {
                if (_isSelected == value) return;
                bool prev = _isSelected;
                _isSelected = value;
                bool curr = _isSelected;
                OnPropertyChanged("IsSelected");
                if (this.OnIsSelectedChanged != null)
                    this.OnIsSelectedChanged(this, new SelectedChangedEventArgs(prev, curr));
            }
        }

        private bool _isExpanded;
        public bool IsExpanded {
            get { return _isExpanded; }
            set {
                _isExpanded = value;
                OnPropertyChanged("IsExpanded");
            }
        }

        private bool _isVisible = true;
        public bool IsVisible {
            get { return _isVisible; }
            set {
                if (_isVisible == value) return;
                _isVisible = value;
                OnPropertyChanged("IsVisible");
            }
        }

        public bool IsInitialized { get; internal set; }

        public void LoadSubKeys() {
            // in case of KeyTable Init method needs to be called instead of LoadSubKeys
        }
    }
}
