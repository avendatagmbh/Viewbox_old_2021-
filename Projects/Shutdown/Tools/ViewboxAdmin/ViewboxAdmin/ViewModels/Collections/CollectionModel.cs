using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using SystemDb;
using ViewboxAdmin_ViewModel;

namespace ViewboxAdmin.ViewModels.Collections
{
    public class CollectionModel : NotifyBase, ICollectionModel {

        public CollectionModel(IParameterValue wrappedParamValue,ILocalizedTextCollection wrappedDescription,string Value, ObservableCollection<LanguageTextModel> textcollection) {
            // inject dependencies
            this.Texts = textcollection;
            this.Value = Value;
            this.WrappedParamValue = wrappedParamValue;
            this.Description = wrappedDescription;
        }

        public ILocalizedTextCollection Description { get; set; }
        public IParameterValue WrappedParamValue { get; private set; }

        public IParameterValue ParameterValue { get; set;}

        public CollectionModel(ObservableCollection<LanguageTextModel> textcollection) {
            this.Texts = textcollection;
        }


        #region Id
        private int _id;

        public int Id {
            get { return _id; }
            set {
                if (_id != value) {
                    _id = value;
                    OnPropertyChanged("Id");
                }
            }
        }
        #endregion Id

        #region Value
        private string _value;

        public string Value {
            get { return _value; }
            set {
                if (_value != value) {
                    _value = value;
                    OnPropertyChanged("Value");
                    
                }
            }
        }
        #endregion Value

        public ObservableCollection<LanguageTextModel> Texts { get; set; }

        

        }
    
}
