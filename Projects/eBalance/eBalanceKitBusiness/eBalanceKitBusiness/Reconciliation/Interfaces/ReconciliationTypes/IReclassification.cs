// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-03-20
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using Taxonomy;

namespace eBalanceKitBusiness.Reconciliation.Interfaces.ReconciliationTypes {
    public interface IReclassification : IReconciliation {
        IElement SourceElement { get; set; }
        IReconciliationTransaction SourceTransaction { get; }

        IElement DestinationElement { get; set; }
        IReconciliationTransaction DestinationTransaction { get; }
    }
}