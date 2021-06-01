using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using SystemDb;

namespace ViewboxAdmin.ViewModels
{
    public interface IItemLoader {
        void InitItems(ObservableCollection<IItemWrapperStructure> itemcollection,ILanguage language);
    }
}
