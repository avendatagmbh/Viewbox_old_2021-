using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SystemDb;
using DbAccess;
using ViewboxAdmin.ViewModels.MergeDataBase;

namespace ViewboxAdmin.ViewModels
{
    internal interface IMergeMetaDatabaseFactory {
        IMergeMetaDatabases Create(ISystemDb systemdb);
        ILoaderHack CreateLoader(ISystemDb systemdb);
    }

    /// <summary>
    /// hide object creation, to make other classes more testable
    /// </summary>
    internal class MergeMetaDatabaseFactory : IMergeMetaDatabaseFactory {
        public IMergeMetaDatabases Create(ISystemDb systemdb) {
            MapBusinessObjectList mapBusinessObjectList = new MapBusinessObjectList(ConnectionManager.CreateConnection(systemdb.ConnectionManager.DbConfig));
            return new MergeMetaDatabases(new CloneBusinessObjects(), mapBusinessObjectList);
        }

        public ILoaderHack CreateLoader(ISystemDb systemdb) {
            return new LoaderHack(ConnectionManager.CreateConnection(systemdb.ConnectionManager.DbConfig));
        }
    }
}
