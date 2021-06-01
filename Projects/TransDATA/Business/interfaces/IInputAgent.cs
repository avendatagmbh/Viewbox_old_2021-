// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-01-11
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using Config.Interfaces.DbStructure;
using System.Collections.Generic;
using System;
using DbAccess;
using DbAccess.Structures;
using System.Data;

namespace Business.Interfaces {
    public interface IInputAgent {
        IDataReader GetDataReader(ITransferEntity entity, bool useAdo = false);
        DataTable GetPreview(ITransferEntity entity, long count);

        IEnumerable<Tuple<DbTableInfo, long>> GetTableInfos(IDatabase conn, bool doCount = true, bool addColumns = true);
        long GetCount(ITransferEntity entity);
        bool CheckDataAccess();
        string GetDescription();
    }
}