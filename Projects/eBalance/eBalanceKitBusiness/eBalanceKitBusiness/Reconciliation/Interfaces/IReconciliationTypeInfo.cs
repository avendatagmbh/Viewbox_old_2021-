// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-03-07
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
namespace eBalanceKitBusiness.Reconciliation.Interfaces {
    public interface IReconciliationTypeInfo {
        Enums.ReconciliationTypes ReconciliationType { get; }
        string ShortLabel { get; }
        string Label { get; }
    }
}