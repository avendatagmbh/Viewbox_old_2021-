using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DbAccess;

namespace eBalanceKitBusiness.Logs.Actions {
    public interface ActionLog {
        void DoAction(IDatabase conn);
    }
}
