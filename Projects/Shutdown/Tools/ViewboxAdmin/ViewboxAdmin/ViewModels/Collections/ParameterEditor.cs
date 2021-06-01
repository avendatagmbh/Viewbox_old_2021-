using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SystemDb;

namespace ViewboxAdmin.ViewModels.Collections
{
    /// <summary>
    /// the systemdb calls are coming here
    /// </summary>
    class ParameterEditor : IParameterEditor
    {

        public ParameterEditor(ISystemDb systemdb) { this.SystemDb = systemdb; }

        public ISystemDb SystemDb { get; private set; }

        public void Update(ICollectionModel cm) {
            
                foreach (var a in cm.Texts)
                {
                    cm.WrappedParamValue.Descriptions[a.Language] = a.Text;
                    SystemDb.UpdateParameterValueText(cm.WrappedParamValue, a.Language);
                }
                //this is working
                if (cm.Value != cm.WrappedParamValue.Value)
                {
                    SystemDb.UpdateParameterValue(cm.WrappedParamValue, cm.Value);
                }
            
        }

        public void Delete(ICollectionModel cm) {
            SystemDb.DeleteParameterValue(cm.WrappedParamValue);
        }

        public void CreateNew(ICollectionModel collectionModel, IParameterModel parameterModel) {
            //storing the localized texts in a dictionary
            Dictionary<ILanguage, string> localizedTexts = new Dictionary<ILanguage, string>();
            foreach (var languageTextModel in collectionModel.Texts)
            {
                localizedTexts.Add(languageTextModel.Language, languageTextModel.Text);
            }
            SystemDb.CreateParameterValue(parameterModel.WrappedParameter, collectionModel.Value, localizedTexts);
        }
    }
}
