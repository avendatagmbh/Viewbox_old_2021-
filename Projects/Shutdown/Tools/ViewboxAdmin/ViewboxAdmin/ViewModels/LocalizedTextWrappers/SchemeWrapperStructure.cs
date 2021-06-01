using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SystemDb;

namespace ViewboxAdmin.ViewModels.LocalizedTextWrappers
{
    class SchemeWrapperStructure: WrapperStructureBase, IItemWrapperStructure
    {
        public SchemeWrapperStructure(IScheme scheme, ILanguage language, ISystemDb system) : base(language,system) { this.Scheme = scheme; }

        public IScheme Scheme { get; set; }

        public string Text {
            get { return Scheme.Descriptions[Language]; }
            set {
                if(Scheme.Descriptions[Language]!=value) {
                    Scheme.Descriptions[Language] = value;
                    OnPropertyChanged("Text");
                    SystemDb.UpdateSchemeText(Scheme,Language);

                }
            }
        }

        public string Name {
            get { return Scheme.Partial; }
        }

        public int Id {
            get { return Scheme.Id; }
        }
    }
}
