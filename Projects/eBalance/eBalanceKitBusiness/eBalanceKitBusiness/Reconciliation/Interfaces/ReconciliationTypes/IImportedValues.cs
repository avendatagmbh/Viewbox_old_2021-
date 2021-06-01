// --------------------------------------------------------------------------------
// author: Gábor Bauer
// since: 2012-04-20
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------

namespace eBalanceKitBusiness.Reconciliation.Interfaces.ReconciliationTypes {
    public interface IImportedValues : IReconciliation {
        ITransactionGroup BalanceListAssets { get; }
        ITransactionGroup BalanceListLiabilities { get; }
        ITransactionGroup IncomeStatement { get; }
    }
}