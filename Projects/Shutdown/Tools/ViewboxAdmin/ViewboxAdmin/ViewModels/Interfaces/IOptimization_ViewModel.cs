using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using SystemDb;
using ViewboxAdmin.CustomEventArgs;

namespace ViewboxAdmin.ViewModels {
    public interface IOptimization_ViewModel {
        ICommand DeleteOptimizationCommand { get; set; }
        string Name { get; set; }
        ObservableCollection<IOptimization_ViewModel> Children { get; set; }
        IOptimization_ViewModel Parent { get; set; }
        IOptimization Optimization { get; set; }
        bool IsExpanded { get; set; }
        bool IsSelected { get; set; }
        event PropertyChangedEventHandler PropertyChanged;
        event EventHandler<OptimizationEventArgs> DeleteOptimization;
        void OnDeleteOptimization(IOptimization opt);
    }
}