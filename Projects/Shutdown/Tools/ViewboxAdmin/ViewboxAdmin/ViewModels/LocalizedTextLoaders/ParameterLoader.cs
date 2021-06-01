using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using SystemDb;
using ViewboxAdmin.ViewModels.LocalizedTextWrappers;

namespace ViewboxAdmin.ViewModels.LocalizedTextLoaders
{
    class ParameterLoader : LoaderBase, IItemLoader
    {

        public ParameterLoader(ISystemDb sysdb) :base(sysdb) {
        }

        public void InitItems(ObservableCollection<IItemWrapperStructure> itemcollection, ILanguage language) {
            
            foreach (var iss in SystemDb.Issues)
            {
                foreach(var par in iss.Parameters)
                itemcollection.Add(new ParameterWrapperStructure(par, language, SystemDb));
            }
        }

       
    }
}
