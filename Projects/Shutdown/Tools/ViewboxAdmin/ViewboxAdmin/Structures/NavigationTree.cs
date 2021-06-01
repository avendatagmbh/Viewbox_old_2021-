using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;

namespace ViewboxAdmin.Structures {

    /// <summary>
    /// This class represents the navigation tree of the main window.
    /// </summary>
    /// <author>Mirko Dibbert</author>
    /// <since>2011-08-01</since>
    public class NavigationTree : INavigationTree {

        #region constructor
        public NavigationTree(Window owner) {
           
            Owner = owner;

        }
        #endregion constructor


        #region events
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string property) { if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(property)); }
        #endregion events


        #region properties
        //--------------------------------------------------------------------------------
        public Window Owner { get; set; }


        #region Children
        private ObservableCollection<INavigationTreeEntry> _children = new ObservableCollection<INavigationTreeEntry>();
        public ObservableCollection<INavigationTreeEntry> Children { get { return _children; } }
        #endregion Children;
        

        //--------------------------------------------------------------------------------
        #endregion properties



        #region methods
        //--------------------------------------------------------------------------------

        #region GetEnumerator
        /// <summary>
        /// Returns the enumeration for the Children enumeration.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<INavigationTreeEntry> GetEnumerator() {
            return Children.GetEnumerator();
        }


        /// <summary>
        /// Returns the enumeration for the Children enumeration.
        /// </summary>
        /// <returns></returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return Children.GetEnumerator();
        }
        #endregion GetEnumerator

        #region AddEntry
        public INavigationTreeEntry AddEntry(string header, FrameworkElement uiElement, INavigationTreeEntry parentEntry = null, Binding visibilityBinding = null) {
            INavigationTreeEntry entry = new NavigationTreeEntry { Header = header, Content = uiElement, Parent = parentEntry};
            if(parentEntry == null) _children.Add(entry);
            else parentEntry.Children.Add(entry);
            return entry;
        }
        
        #endregion
        //--------------------------------------------------------------------------------
        #endregion methods

    }
}
