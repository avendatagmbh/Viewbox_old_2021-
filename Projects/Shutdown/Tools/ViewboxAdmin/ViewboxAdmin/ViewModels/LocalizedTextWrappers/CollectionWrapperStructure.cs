using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SystemDb;

namespace ViewboxAdmin.ViewModels.LocalizedTextWrappers
{
    class CollectionWrapperStructure : WrapperStructureBase , IItemWrapperStructure
    {

        public CollectionWrapperStructure(IParameterValue paramvalue, ILanguage language, ISystemDb systemdb ):base(language,systemdb) {
            this.ParameterValue = paramvalue;
        }

        public IParameterValue ParameterValue { get; private set; }

        public string Text {
            get { return ParameterValue.Descriptions[Language]; }
            set {
                if(ParameterValue.Descriptions[Language]!=value) {
                    ParameterValue.Descriptions[Language] = value;
                    //do database manip here
                    SystemDb.UpdateParameterValueText(ParameterValue,Language);
                    OnPropertyChanged("Text");
                }
            }
        }

        public string Name {
            get { return ParameterValue.Value; }
        }

        public int Id {
            get { return 0; }
        }



    }
}
