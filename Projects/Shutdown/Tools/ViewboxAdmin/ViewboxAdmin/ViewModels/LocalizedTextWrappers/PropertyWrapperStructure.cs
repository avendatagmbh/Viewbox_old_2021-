using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SystemDb;

namespace ViewboxAdmin.ViewModels.LocalizedTextWrappers
{
    class PropertyWrapperStructure : WrapperStructureBase, IItemWrapperStructure
    {
        public PropertyWrapperStructure(IProperty propety,ILanguage language, ISystemDb systemdb) :base(language, systemdb) { this.Property = propety; }
        public string Text {
            get { return Property.Descriptions[Language]; }
            set {
                if (Property.Descriptions[Language]!=value) {
                    Property.Descriptions[Language] = value;
                    OnPropertyChanged("Text");
                    SystemDb.UpdatePropertyText(Property,Language);
                } 
            }
        }

        public string Name {
            get { return this.Property.Value; }
        }

        public int Id {
            get { return Property.Id; }
        }

        public IProperty Property { get; private set; }
    }
}
