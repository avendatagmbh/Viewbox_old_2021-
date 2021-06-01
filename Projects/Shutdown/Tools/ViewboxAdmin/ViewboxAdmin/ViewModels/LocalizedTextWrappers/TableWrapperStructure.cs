using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using SystemDb;

namespace ViewboxAdmin.ViewModels
{
    /// <summary>
    /// wrapper class around a tableobject
    /// </summary>
    public class TableWrapperStructure : WrapperStructureBase, IItemWrapperStructure {

        public TableWrapperStructure(ITableObject table, ILanguage language,ISystemDb systemDb) : base(language,systemDb) {
            this._table = table;
        }
        
        private ITableObject _table;
        public ITableObject Table { get { return _table; } }

        public string Text { 
            get {
                string text = Table.Descriptions[Language];
                return text;
            } 
            set {
                if (Table.Descriptions[Language] != value)
                    Table.Descriptions[Language] = value;
                    this.SystemDb.UpdateTableObjectText(Table,Language);
                    OnPropertyChanged("Text"); }
        }
        public string Name { get { return Table.Name; } }
        public int Id { get { return Table.Id; } }

        
    }
}
