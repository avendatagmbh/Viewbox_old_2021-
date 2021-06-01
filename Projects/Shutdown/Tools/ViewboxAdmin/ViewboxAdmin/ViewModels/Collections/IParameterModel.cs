using System.Collections.ObjectModel;
using System.ComponentModel;
using SystemDb;

namespace ViewboxAdmin.ViewModels.Collections {
    public interface IParameterModel {
        IParameter WrappedParameter { get; }
        string Name { get; set; }
        int Id { get; }
        ObservableCollection<ICollectionModel> ParameterValues { get; }
        ObservableCollection<LanguageTextModel> Texts { get; set; }
        event PropertyChangedEventHandler PropertyChanged;
    }
}