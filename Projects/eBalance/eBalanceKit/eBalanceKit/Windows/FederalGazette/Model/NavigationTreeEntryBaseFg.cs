// --------------------------------------------------------------------------------
// author: Solueman Hussain
// since: 2012-01-04
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace eBalanceKit.Windows.FederalGazette.Model {
    public class NavigationTreeEntryBaseFg : INotifyPropertyChanged, INotifyPropertyChanging
    {
        #region properties
        
        
        public NavigationTreeEntryBaseFg Parent { get; set; }
        private readonly ObservableCollection<NavigationTreeEntryBaseFg> _children= new ObservableCollection<NavigationTreeEntryBaseFg>();
        public IList<NavigationTreeEntryBaseFg> Children { get { return _children; } }

        public string Header { get; set; }
        public Taxonomy.IElement XbrlElement { get; set; }
        public object Model { get; set; }
        public object Content { get; set; }

        private bool _isSelected;
        public bool IsSelected {
            get { return _isSelected; }
            set{ if(_isSelected==value) return;
                _isSelected = value;
                OnPropertyChanged("IsSelected");
            }
        }

        private bool _isVisible;
        public bool IsVisible {
            get { return _isVisible; }
            set { if (_isVisible == value) return;
                _isVisible = value;
                OnPropertyChanged("IsVisible");
            }
        }

        private bool _isExpanded;
        public bool IsExpanded {
            get { return _isExpanded; }
            set{ if(_isExpanded==value) return;
                _isExpanded = value;
                OnPropertyChanged("IsExpanded");
            }
        }
         #endregion

        #region Methods
        public void AddChildren(NavigationTreeEntryBaseFg entry) {
            entry.Parent = this;
            _children.Add(entry);
        }
        #endregion


        #region EventHandler

        private event PropertyChangedEventHandler _propertyChanged;
        public event PropertyChangedEventHandler PropertyChanged {
            add { _propertyChanged += value; }
            remove { _propertyChanged -= value; }
        }
        private event PropertyChangingEventHandler _propertyChanging;
        public event PropertyChangingEventHandler PropertyChanging {
            add { _propertyChanging += value; }
            remove { _propertyChanging -= value; }
        }
        protected void OnPropertyChanged(string propertyName) {
            if(_propertyChanged!=null) _propertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        protected void OnPropertyChanging(string propertyName) {
            if(_propertyChanging!=null) _propertyChanging(this, new PropertyChangingEventArgs(propertyName));
        }
        public void ClearAllEventHandlers() { 
            _propertyChanged = null;
            _propertyChanging = null;
        }
        #endregion



    }
}