using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;

namespace ViewboxAdmin.Structures {
    /// <summary>
    /// Each instance of this class represents an entry in the navigation tree.
    /// </summary>
    /// <author>Mirko Dibbert</author>
    /// <since>2010-11-04</since>
    public class NavigationTreeEntry : INavigationTreeEntry {

        #region Constructor
        public NavigationTreeEntry() {
            IsVisible = true;
        }
        #endregion

        #region events
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string property) { if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(property)); }
        #endregion

        #region properties

        public string Header { get; set; }
        public object Model { get; set; }
        public INavigationTreeEntry Parent { get; set; }

        #region Content

        public FrameworkElement Content { get; set; }

        #endregion

        #region IsVisible
        public bool IsVisible {
            get { return _isVisible; }
            set {
                if (_isVisible != value) {
                    _isVisible = value;
                    OnPropertyChanged("IsVisible");
                }
            }
        }
        private bool _isVisible;
        #endregion

        #region IsExpanded
        public bool IsExpanded {
            get { return _isExpanded; }
            set {
                if (_isExpanded != value) {
                    _isExpanded = value;
                    OnPropertyChanged("IsExpanded");
                }
            }
        }
        private bool _isExpanded;
        #endregion

        #region IsSelected
        public bool IsSelected {
            get { return _isSelected; }
            set {
                _isSelected = value;

                // auto expand subtree if element is selected
                //if (_isSelected && !IsExpanded) IsExpanded = true;

                OnPropertyChanged("IsSelected");
            }
        }
        private bool _isSelected;
        #endregion

        #region Children
        public ObservableCollection<INavigationTreeEntry> Children { get { return _children; } }
        ObservableCollection<INavigationTreeEntry> _children = new ObservableCollection<INavigationTreeEntry>();
        #endregion

        #region ShowInfoPanel
        //--------------------------------------------------------------------------------
        private bool _showInfoPanel = false;

        public bool ShowInfoPanel {
            get { return _showInfoPanel; }
            set { 
                _showInfoPanel = value;
                OnPropertyChanged("ShowInfoPanel");
            }
        }
        //--------------------------------------------------------------------------------
        #endregion ShowInfoShowInfoPanel


        #endregion properties

        #region methods

        public void Addchildren(INavigationTreeEntry entry) {
            entry.Parent = this;
            this.Children.Add(entry);
        }

        #endregion

    }
}
