using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using SystemDb;
using ViewboxAdmin.ViewModels.LocalizedTextLoaders;

namespace ViewboxAdmin.ViewModels
{
    public class TableLoader : LoaderBase, IItemLoader
    {

        public TableLoader(ISystemDb sysdb):base(sysdb) {}

        public void InitItems(ObservableCollection<IItemWrapperStructure> itemcollection, ILanguage language) {
            foreach (var o in SystemDb.Objects)
            {
                itemcollection.Add(new TableWrapperStructure(o, language, SystemDb));
            }
        }
    }
}
