using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using DbSearchBase.Interfaces;
using DbSearchLogic.SearchCore.ForeignKeySearch;
using DbSearchLogic.SearchCore.KeySearch;
using Utils;

namespace DbSearchLogic.SearchCore.Keys
{
    public class DisplayKey : NotifyPropertyChangedBase, IDisplayKey {

        #region [ Members ]

        private bool _initialized = false;
        private IDisplayKey _parent;
        private IDisplayDbKey _key;
        private Func<IDisplayDbKey, IKey> _initAction;
        private Func<IDisplayKey, ObservableCollection<IDisplayKey>> _loadSubkeyAction;
        private KeySearchManager _keySearchManager;

        #endregion [ Members ]

        #region [ Constructor ]

        public DisplayKey(IKey key, IEnumerable<IDisplayKey> keys, Func<IDisplayDbKey, IKey> initAction, Func<IDisplayKey, ObservableCollection<IDisplayKey>> loadSubkeyAction, KeySearchManager keySearchManager, IDisplayKey parentKey = null) {

            _initAction = initAction;
            _loadSubkeyAction = loadSubkeyAction;
            _keySearchManager = keySearchManager;
            _parent = parentKey;

            if (key is ForeignKey)
                _key = new DisplayForeignKey(key as ForeignKey, _initAction, IsChildOfPrimaryKey(_parent), _parent);
            else if (key is Key)
                _key = new DisplayPrimaryKey(key as Key, _initAction);

            if (keys != null)
                Children = new ObservableCollection<IDisplayKey>(keys);
        }

        #endregion [ Constructor ]

        public KeySearchManager KeySearchManagerInstance { get { return _keySearchManager; } }

        public IDisplayKey Parent { get { return _parent; } }

        public IDisplayDbKey Key { get { return _key; } }

        public string Label { get { return Key.Label; } }

        public ObservableCollection<IDisplayKey> Children { get; internal set; }

        public event KeyManager.SelectedChangedEventHandler OnIsSelectedChanged;

        private bool _isSelected;
        public bool IsSelected {
            get { return _isSelected; }
            set {
                if (_isSelected == value) return;
                bool prev = _isSelected;
                _isSelected = value;
                bool curr = _isSelected;
                if (_isSelected && !_key.IsInitialized) {
                    this.Key.Initialize();
                    LoadSubKeys();
                }
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

        public bool IsChildOfPrimaryKey(IDisplayKey parentKey) {
            if (parentKey != null && parentKey.Key is IDisplayPrimaryKey) return true;
            return false;
        }

        public void LoadSubKeys() {
            if (Children == null && _key is DisplayPrimaryKey) {
                Children = _loadSubkeyAction(this);
                OnPropertyChanged("Children");
            }
        }
    }
}
