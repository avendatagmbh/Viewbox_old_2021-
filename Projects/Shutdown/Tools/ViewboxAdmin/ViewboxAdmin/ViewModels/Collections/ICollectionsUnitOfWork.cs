using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace ViewboxAdmin.ViewModels.Collections
{
    public interface ICollectionsUnitOfWork: INotifyPropertyChanged {
        IParameterEditor ParameterValueEditor { get; }
        List<ICollectionModel> DirtyItems { get;  }
        List<Tuple<ICollectionModel, IParameterModel>> NewItems { get;}
        List<ICollectionModel> DeletedItems { get; }
        void MarkAsDirty(ICollectionModel cm);
        void MarkAsNew(Tuple<ICollectionModel, IParameterModel> collparmap);
        void MarkAsDeleted(ICollectionModel cm);
        void Commit();
        void Rollback();
        string DebugMessage { get; set; }
    }
}
