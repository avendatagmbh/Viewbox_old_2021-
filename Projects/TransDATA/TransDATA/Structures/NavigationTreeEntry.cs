using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace TransDATA.Structures {
    /// <summary>
    /// Each instance of this class represents an entry in the navigation tree.
    /// </summary>
    /// <author>Mirko Dibbert</author>
    /// <since>2010-11-04</since>
    public class NavigationTreeEntry : INotifyPropertyChanged {

        #region Constructor
        public NavigationTreeEntry() {
            IsVisible = true;
        }
        #endregion

        #region events
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string property) { if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(property)); }
        #endregion

        #region properties

        public string Header { get; set; }
        public object Model { get; set; }
        public NavigationTreeEntry Parent { get; set; }

        #region Content

        public object Content { get; set; }

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
        public ObservableCollection<NavigationTreeEntry> Children { get { return _children; } }
        ObservableCollection<NavigationTreeEntry> _children = new ObservableCollection<NavigationTreeEntry>();
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

        #region ValidationWarning
        /// <summary>
        /// Returns false, if the node itself or at least one of it's child nodes (full recursive) is not valid or true otherwhise.
        /// </summary>
        public bool ValidationWarning {
            get { return _validationWarning || Children.OfType<NavigationTreeEntry>().Any(child => child.ValidationWarning); }
            set {
                _validationWarning = value;

                OnPropertyChanged("ValidationWarning");

                // notify property changed for all parent nodes
                var parent = Parent;
                while (parent != null) {
                    ((NavigationTreeEntry)parent).OnPropertyChanged("ValidationWarning");
                    parent = parent.Parent;
                }

            }
        }
        private bool _validationWarning;
        #endregion

        #region ValidationError
        public bool ValidationError {
            get { return _validationError || Children.OfType<NavigationTreeEntry>().Any(child => child.ValidationError); }
            set {
                _validationError = value;

                OnPropertyChanged("ValidationError");

                // notify property changed for all parent nodes
                var parent = Parent;
                while (parent != null) {
                    ((NavigationTreeEntry)parent).OnPropertyChanged("ValidationError");
                    parent = parent.Parent;
                }
            }
        }
        private bool _validationError;
        #endregion

        #endregion properties

        #region methods

        public void Addchildren(NavigationTreeEntry entry) {
            entry.Parent = this;
            this.Children.Add(entry);
        }

        #endregion

    }
}
