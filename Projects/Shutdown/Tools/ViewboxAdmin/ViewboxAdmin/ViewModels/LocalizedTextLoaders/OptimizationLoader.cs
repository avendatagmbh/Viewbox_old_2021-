using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using SystemDb;
using ViewboxAdmin.ViewModels.LocalizedTextLoaders;

namespace ViewboxAdmin.ViewModels
{
    class OptimizationLoader : LoaderBase, IItemLoader
    {

        public OptimizationLoader(ISystemDb sysdb):base(sysdb) {
        }

        
        public void InitItems(ObservableCollection<IItemWrapperStructure> itemcollection, ILanguage language) {
           
            foreach (var opt in SystemDb.Optimizations)
            {
                itemcollection.Add(new OptimizationWrapperStructure(opt, language, SystemDb));
            }
        }
    }
}
