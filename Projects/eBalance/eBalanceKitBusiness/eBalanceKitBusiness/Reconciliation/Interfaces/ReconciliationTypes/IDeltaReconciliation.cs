// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-03-20
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Collections.Generic;
using Taxonomy;

namespace eBalanceKitBusiness.Reconciliation.Interfaces.ReconciliationTypes {
    public interface IDeltaReconciliation : IReconciliation {
        //IEnumerable<IReconciliationTransaction> Transactions { get; }
        bool HasTransaction { get; }
        ITransactionGroup BalanceListAssets { get; }
        ITransactionGroup BalanceListLiabilities { get; }
        ITransactionGroup IncomeStatement { get; }
        
        void AddTransaction(IElement position);
    }
}