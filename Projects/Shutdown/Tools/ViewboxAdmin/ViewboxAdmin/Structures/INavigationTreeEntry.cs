using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;

namespace ViewboxAdmin.Structures {
    public interface INavigationTreeEntry : INotifyPropertyChanged {
        event PropertyChangedEventHandler PropertyChanged;
        string Header { get; set; }
        object Model { get; set; }
        INavigationTreeEntry Parent { get; set; }
        FrameworkElement Content { get; set; }
        bool IsVisible { get; set; }
        bool IsExpanded { get; set; }
        bool IsSelected { get; set; }
        ObservableCollection<INavigationTreeEntry> Children { get; }
        bool ShowInfoPanel { get; set; }
        void OnPropertyChanged(string property);
        void Addchildren(INavigationTreeEntry entry);
    }
}