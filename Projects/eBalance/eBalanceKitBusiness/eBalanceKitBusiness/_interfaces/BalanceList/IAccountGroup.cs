using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using eBalanceKitBusiness.Structures.DbMapping.BalanceList;

namespace eBalanceKitBusiness {
    
    public interface IAccountGroup : IBalanceListEntry {
        IEnumerable<IAccount> Items { get; }
        int ItemsCount { get; }

        void Validate();
        void AddAccount(IAccount account);
        void RemoveAccount(IAccount account);
        void ClearItems();
        IAccountGroup Clone();
        void Update(IAccountGroup group);
    }
}
