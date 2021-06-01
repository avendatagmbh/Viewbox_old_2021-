using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using SystemDb;
using ViewboxAdmin.ViewModels.LocalizedTextWrappers;

namespace ViewboxAdmin.ViewModels.LocalizedTextLoaders
{
    class CollectionLoader : LoaderBase, IItemLoader
    {
        public CollectionLoader(ISystemDb sysdb):base(sysdb) {}

        public void InitItems(ObservableCollection<IItemWrapperStructure> itemcollection, ILanguage language) {
           
            foreach (var iss in SystemDb.Issues)
            {
                    foreach (var param in iss.Parameters)
                    {
                        if (param.Values != null) {
                            foreach (var paramvalue in param.Values) {
                                itemcollection.Add(new CollectionWrapperStructure(paramvalue, language, this.SystemDb));
                            }
                        }
                    }
                


            }
        }
    }
}
