using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using eBalanceKitBusiness.Reconciliation.Interfaces.ReconciliationTypes;

namespace eBalanceKitBusiness.Reconciliation.Interfaces {

    public interface IReconciliationManagerManagement : IReconciliationManager {
        string GetReconciliationNameManagement(string id);
    }

}
