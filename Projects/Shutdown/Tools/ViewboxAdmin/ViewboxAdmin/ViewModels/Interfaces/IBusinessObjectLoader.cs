using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using SystemDb;
using ViewboxAdmin.ViewModels.Collections;

namespace ViewboxAdmin.ViewModels
{
    public interface IBusinessObjectLoader {
        ObservableCollection<ICollectionModel> GetParameterValueCollections(IParameter param);
        ObservableCollection<LanguageTextModel> EmptyLanguageTextModels();
        ObservableCollection<IParameterModel> GetParameterCollections();
    }
}
