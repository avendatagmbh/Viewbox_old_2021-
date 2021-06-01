using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using SystemDb;
using ViewboxAdmin.ViewModels.LocalizedTextLoaders;

namespace ViewboxAdmin.ViewModels
{
    public class CategoryLoader : LoaderBase,IItemLoader
    {
        public CategoryLoader(ISystemDb sysdb):base(sysdb) { }

        public void InitItems(ObservableCollection<IItemWrapperStructure> itemcollection, ILanguage language) {
           
            foreach (var cat in SystemDb.Categories)
            {
                itemcollection.Add(new CategoryWrapperStructure(cat, language, SystemDb));
            }
        }
    }
}
