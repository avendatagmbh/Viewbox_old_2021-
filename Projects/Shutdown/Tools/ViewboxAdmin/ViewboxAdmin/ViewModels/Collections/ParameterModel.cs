using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using SystemDb;
using ViewboxAdmin_ViewModel;

namespace ViewboxAdmin.ViewModels.Collections
{
    public class ParameterModel:NotifyBase, IParameterModel {

        public ParameterModel(IParameter wrappedParameter,int ID,string Name, ObservableCollection<ICollectionModel> parameterValueCollection, ObservableCollection<LanguageTextModel> parameterTextsCollection) {
            this.WrappedParameter = wrappedParameter;
            this.Name = Name;
            this.ParameterValues = parameterValueCollection;
            this.Texts = parameterTextsCollection;
            this.Id = ID;
        }
        public IParameter WrappedParameter { get; private set; }
        #region Name
        private string _name;

        public string Name {
            get { return _name; }
            set {
                if (_name != value) {
                    _name = value;
                    OnPropertyChanged("Name");
                }
            }
        }
        #endregion Name
        public int Id { get; private set; }
        public ObservableCollection<ICollectionModel> ParameterValues { get; private set; }
        public ObservableCollection<LanguageTextModel> Texts { get; set; }
    }
}
