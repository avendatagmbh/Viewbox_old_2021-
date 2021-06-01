using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using eBalanceKitBusiness.Manager;
using eBalanceKitBusiness.Reconciliation.DbMapping;
using eBalanceKitBusiness.Reconciliation.Interfaces.ReconciliationTypes;
using eBalanceKitBusiness.Structures.DbMapping;

namespace eBalanceKitBusiness.Reconciliation.ReconciliationTypes {
    internal class TaxBalanceValue : DeltaReconciliation, ITaxBalanceValue {

        #region Constructors
        /// <summary>
        /// Constructor for new reconciliation.
        /// </summary>
        /// <param name="document">Assigned document.</param>
        internal TaxBalanceValue(Document document)
            : base(document, Enums.ReconciliationTypes.TaxBalanceValue) {

        }

        /// <summary>
        /// Constructor for existing reconciliation.
        /// </summary>
        /// <param name="dbEntityReconciliation">Assigned reconciliation.</param>
        internal TaxBalanceValue(DbEntityReconciliation dbEntityReconciliation)
            : base(dbEntityReconciliation) { }
        #endregion // constructors   

    }
}
