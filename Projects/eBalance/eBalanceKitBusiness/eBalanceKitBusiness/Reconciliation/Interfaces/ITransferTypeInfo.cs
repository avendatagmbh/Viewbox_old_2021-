// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-03-09
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using Taxonomy;

namespace eBalanceKitBusiness.Reconciliation.Interfaces {
    public interface ITransferTypeInfo {
        IElement Element{ get; }
        string ElementId { get; }
    }
}