using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SystemDb;


namespace ViewboxAdmin.ViewModels.LocalizedTextWrappers
{
    class ColumnWrapperStructure: WrapperStructureBase, IItemWrapperStructure
    {
        public ColumnWrapperStructure(IColumn column,ILanguage language, ISystemDb system) : base(language,system) { this.Column = column; }

        public string Text {
            get { return Column.Descriptions[Language]; }
            set {
                if(Column.Descriptions[Language]!=value) {
                    Column.Descriptions[Language] = value;
                    OnPropertyChanged("Text");
                    SystemDb.UpdateColumnText(Column,Language);
                }
            }
        }

        public string Name {
            get { return Column.Name; }
        }

        public int Id {
            get { return Column.Id; }
        }

        public IColumn Column { get; private set; }
    }
}
