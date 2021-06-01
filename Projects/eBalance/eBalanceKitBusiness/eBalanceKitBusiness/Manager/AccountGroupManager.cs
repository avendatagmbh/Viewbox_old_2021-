using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using eBalanceKitBusiness.Structures.DbMapping.BalanceList;

namespace eBalanceKitBusiness.Manager {

    public static class AccountGroupManager {

        public static IAccountGroup CreateAccountGroup(IBalanceList balanceList) {
            if (!(balanceList is BalanceList)) throw new Exception("Invalid IBalanceList implementation: \"" + balanceList.GetType().Name + "\"");
            var group = new AccountGroup { BalanceList = balanceList as BalanceList};
            return group;
        }
    }
}
