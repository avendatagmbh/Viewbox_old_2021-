using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ViewboxAdmin.ViewModels.LocalizedTextWrappers;

namespace ViewboxAdmin.ViewModels.LocalizedTextLoaders
{
    class OptimizationGroupLoader : LoaderBase, IItemLoader
    {

        public OptimizationGroupLoader(SystemDb.ISystemDb sysdb):base(sysdb) {}
        

        public void InitItems(System.Collections.ObjectModel.ObservableCollection<IItemWrapperStructure> itemcollection, SystemDb.ILanguage language) {
            
            foreach (var optgroup in SystemDb.OptimizationGroups)
            {
                
                            itemcollection.Add(new OptimizationGroupTextWrapperStructure(optgroup, language, SystemDb));

            }
        }
    }
}
