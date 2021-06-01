using System.Collections.ObjectModel;
using System.ComponentModel;
using SystemDb;

namespace ViewboxAdmin.ViewModels.Collections {
    public interface ICollectionModel: INotifyPropertyChanged {
        ILocalizedTextCollection Description { get; set; }
        IParameterValue WrappedParamValue { get; }
        IParameterValue ParameterValue { get; set; }
        int Id { get; set; }
        string Value { get; set; }
        ObservableCollection<LanguageTextModel> Texts { get; set; }
        
    }
}