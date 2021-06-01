// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-01-11
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Data;
using Business.Interfaces;
using Config.Interfaces.DbStructure;
using System.Collections.Generic;
using DbAccess;
using DbAccess.Structures;
using IDataReader = Business.Interfaces.IDataReader;

namespace Business.Structures.InputAgents {
    public abstract class InputAgentBase : IInputAgent {

        protected InputAgentBase(IInputConfig config) { Config = config; }

        protected IInputConfig Config { get; set; }

        public virtual IDataReader GetDataReader(ITransferEntity entity, bool useAdo = false) { throw new NotImplementedException(); }
        public virtual DataTable GetPreview(ITransferEntity entity, long count) { throw new NotImplementedException(); }

        public virtual IEnumerable<Tuple<DbTableInfo, long>> GetTableInfos(IDatabase conn, bool doCount = true, bool addColumns = true) { throw new System.NotImplementedException(); }
        public virtual long GetCount(ITransferEntity entity) { throw new System.NotImplementedException(); }
        
        public abstract bool CheckDataAccess();
        public abstract string GetDescription();
    }
}