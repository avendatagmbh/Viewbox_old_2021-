using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;

namespace ViewboxAdmin.Structures {
    public interface INavigationTree : IEnumerable<INavigationTreeEntry>, INotifyPropertyChanged {
        event PropertyChangedEventHandler PropertyChanged;
        Window Owner { get; set; }
        ObservableCollection<INavigationTreeEntry> Children { get; }
        void OnPropertyChanged(string property);

        /// <summary>
        /// Returns the enumeration for the Children enumeration.
        /// </summary>
        /// <returns></returns>
        IEnumerator<INavigationTreeEntry> GetEnumerator();

        INavigationTreeEntry AddEntry(string header, FrameworkElement uiElement, INavigationTreeEntry parentEntry = null, Binding visibilityBinding = null);
    }
}