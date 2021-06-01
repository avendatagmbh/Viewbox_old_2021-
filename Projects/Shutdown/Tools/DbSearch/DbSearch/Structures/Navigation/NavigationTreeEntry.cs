// --------------------------------------------------------------------------------
// author: Benjamin Held
// since: 2011-09-07
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;

namespace DbSearch.Structures.Navigation {
    /// <summary>
    /// Each instance of this class represents an entry in the navigation tree.
    /// </summary>
    /// <author>Mirko Dibbert</author>
    /// <since>2010-11-04</since>
    public class NavigationTreeEntry : INotifyPropertyChanged {

        #region Constructor
        public NavigationTreeEntry() {
            //IsVisible = true;
            //Visibility = Visibility.Visible;
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

        //public new bool IsVisible { get { return Visibility == Visibility.Visible; } }

        //public bool IsVisible {
        //    get { return (bool)GetValue(IsVisibleProperty); }
        //    set { SetValue(IsVisibleProperty, value); }
        //}

        //// Using a DependencyProperty as the backing store for IsVisible.  This enables animation, styling, binding, etc...
        //public static readonly DependencyProperty IsVisibleProperty =
        //    DependencyProperty.Register("IsVisible", typeof(bool), typeof(NavigationTreeEntry), new UIPropertyMetadata(false));



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
        private bool _isVisible = true;
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


        #endregion properties

        #region methods

        public void Addchildren(NavigationTreeEntry entry) {
            entry.Parent = this;
            this.Children.Add(entry);
        }

        #endregion

    }
}
