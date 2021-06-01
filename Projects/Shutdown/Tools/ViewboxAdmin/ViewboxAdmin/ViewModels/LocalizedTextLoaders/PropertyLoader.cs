using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using ViewboxAdmin.ViewModels.LocalizedTextWrappers;

namespace ViewboxAdmin.ViewModels.LocalizedTextLoaders
{
    class PropertyLoader:LoaderBase, IItemLoader
    {
        

        public PropertyLoader(SystemDb.ISystemDb sysdb):base(sysdb) {}

        public void InitItems(ObservableCollection<IItemWrapperStructure> itemcollection, SystemDb.ILanguage language) {
            
            foreach (var prop in SystemDb.Properties)
            {
                itemcollection.Add(new PropertyWrapperStructure(prop, language, SystemDb));
            }
        }
    }
}
