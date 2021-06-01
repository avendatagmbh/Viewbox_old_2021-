// --------------------------------------------------------------------------------
// author: Benjamin Held
// since: 2011-09-07
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;

namespace DbSearch.Structures.Navigation {

    /// <summary>
    /// This class represents the navigation tree of the main window.
    /// </summary>
    /// <author>Mirko Dibbert</author>
    /// <since>2011-08-01</since>
    public class NavigationTree : IEnumerable<NavigationTreeEntry>, INotifyPropertyChanged {

        #region constructor
        public NavigationTree(Window owner) {
           
            Owner = owner;

        }
        #endregion constructor


        #region events
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string property) { if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(property)); }
        #endregion events


        #region properties
        //--------------------------------------------------------------------------------
        private Window Owner { get; set; }


        #region Children
        private ObservableCollection<NavigationTreeEntry> _children = new ObservableCollection<NavigationTreeEntry>();
        public IEnumerable<NavigationTreeEntry> Children { get { return _children; } }
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
        public IEnumerator<NavigationTreeEntry> GetEnumerator() {
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
        public NavigationTreeEntry AddEntry(string header, UIElement uiElement, NavigationTreeEntry parentEntry = null, Binding visibilityBinding = null) {
            NavigationTreeEntry entry = new NavigationTreeEntry { Header = header, Content = uiElement, Parent = parentEntry};
            if(parentEntry == null) _children.Add(entry);
            else parentEntry.Children.Add(entry);
            return entry;
        }
        #endregion
        //--------------------------------------------------------------------------------
        #endregion methods

    }
}
