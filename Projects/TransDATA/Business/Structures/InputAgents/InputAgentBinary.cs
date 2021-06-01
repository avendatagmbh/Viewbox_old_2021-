// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-01-11
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using Config.Interfaces.DbStructure;
using Business.Interfaces;

namespace Business.Structures.InputAgents {
    internal class InputAgentBinary : InputAgentBase {

        public InputAgentBinary(IInputConfig config)
            : base(config) { }

        public override IDataReader GetDataReader(ITransferEntity entity, bool useAdo = false) {
            throw new NotImplementedException();
            // TODO
        }

        public override bool CheckDataAccess() {
            throw new NotImplementedException();
            // TODO
        }

        public override string GetDescription() {
            return null;
        }
    }
}