using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using ViewboxAdmin.ViewModels.LocalizedTextWrappers;

namespace ViewboxAdmin.ViewModels.LocalizedTextLoaders
{
    class ColumnLoader: LoaderBase, IItemLoader
    {
       

        public ColumnLoader(SystemDb.ISystemDb sysdb):base(sysdb){}

        public void InitItems(ObservableCollection<IItemWrapperStructure> itemcollection, SystemDb.ILanguage language) {
           
            foreach (var column in SystemDb.Columns)
            {
                itemcollection.Add(new ColumnWrapperStructure(column, language, SystemDb));
            }
        }
    }
}
