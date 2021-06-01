using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using ViewboxAdmin.ViewModels.LocalizedTextWrappers;

namespace ViewboxAdmin.ViewModels.LocalizedTextLoaders
{
    class SchemeLoader :LoaderBase, IItemLoader
    {

        public SchemeLoader(SystemDb.ISystemDb sysdb):base(sysdb) {
        }

        public void InitItems(ObservableCollection<IItemWrapperStructure> itemcollection, SystemDb.ILanguage language) {
            foreach (var scheme in SystemDb.Schemes) {
                
                itemcollection.Add(new SchemeWrapperStructure(scheme, language, SystemDb));
            }
        }
    }
}
