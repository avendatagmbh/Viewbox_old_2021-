using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using SystemDb;
using ViewboxAdmin.ViewModels.Collections;

namespace ViewboxAdmin.ViewModels
{
    public class BusinessObjectLoader : IBusinessObjectLoader
    {
        /// <summary>
        /// A really dirty class for hiding the loading process of wrapper classes
        /// </summary>
        /// <param name="systemDb"></param>

        public BusinessObjectLoader(ISystemDb systemDb) {
            this.SystemDb = systemDb;
        }

        public ISystemDb SystemDb { get;private set;}
        

        public ObservableCollection<ICollectionModel> GetParameterValueCollections(IParameter param) {

            ObservableCollection<ICollectionModel> coll = new ObservableCollection<ICollectionModel>();

            
            if (param.Values != null) {
                foreach (var paramvalue in param.Values) {
                    var etwas = GetLanguageTextModels(paramvalue.Descriptions, SystemDb.Languages);
                    var cmodel = new CollectionModel(paramvalue,paramvalue.Descriptions,paramvalue.Value, etwas);
                    coll.Add(cmodel);
                }
            }
            return coll;
        }

        public ObservableCollection<IParameterModel> GetParameterCollections() {
            ObservableCollection<IParameterModel> coll = new ObservableCollection<IParameterModel>();
            foreach (var iss in SystemDb.Issues)
            {
                foreach (var param in iss.Parameters)
                {
                    coll.Add(new ParameterModel(param,param.Id,param.Name,GetParameterValueCollections(param),GetLanguageTextModels(param.Descriptions,SystemDb.Languages)));
                }
            }
            return coll;
        }

        private ObservableCollection<LanguageTextModel> GetLanguageTextModels(ILocalizedTextCollection paramvalue, ILanguageCollection lang) {
            var etwas = new TrulyObservableCollection<LanguageTextModel>();
            foreach (var l in lang)
            {
                etwas.Add(new LanguageTextModel(paramvalue, l));
            }
            return etwas;
        }



        public ObservableCollection<LanguageTextModel> EmptyLanguageTextModels() {
            var etwas = new TrulyObservableCollection<LanguageTextModel>();
            foreach (var l in SystemDb.Languages)
            {
                etwas.Add(new LanguageTextModel(l));
            }
            return etwas;
        }
    }
}
