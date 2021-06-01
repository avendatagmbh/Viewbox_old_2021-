// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-03-09
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
namespace eBalanceKitBusiness.Reconciliation.Enums {
    /// <summary>
    /// This enumeration lists all available transfer kinds, which correspond with the provided types in the "hbst.transfer.kind" Position.
    /// </summary>
    public enum TransferKinds {
        Reclassification,
        ValueChange,
        ReclassificationWithValueChange
    }
}