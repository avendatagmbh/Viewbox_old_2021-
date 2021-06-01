using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SystemDb;

namespace ViewboxAdmin.ViewModels.LocalizedTextWrappers
{
    class ParameterWrapperStructure : WrapperStructureBase,IItemWrapperStructure
    {
        public ParameterWrapperStructure(IParameter param, ILanguage language, ISystemDb systemdb ):base(language,systemdb) {
            this.Parameter = param;
        }
        public string Text {
            get { return Parameter.Descriptions[Language]; }
            set {
                if(Parameter.Descriptions[Language]!=value) {
                    Parameter.Descriptions[Language] = value;
                    OnPropertyChanged("Text");
                    SystemDb.UpdateParameterText(Parameter,Language);
                }
            }
        }

        public string Name {
            get { return Parameter.Name;}
        }

        public int Id {
            get { return Parameter.Id; }
        }

        public IParameter Parameter { get; private set; }
    }
}
