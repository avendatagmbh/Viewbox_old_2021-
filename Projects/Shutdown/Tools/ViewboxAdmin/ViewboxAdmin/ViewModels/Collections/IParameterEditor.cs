using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SystemDb;

namespace ViewboxAdmin.ViewModels.Collections
{
    public interface IParameterEditor {
        void Delete(ICollectionModel cm);
        void CreateNew(ICollectionModel collectionModel, IParameterModel parameterModel);
        void Update(ICollectionModel cm);
    }
}
