using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using eBalanceKitBusiness.Structures.DbMapping.BalanceList;

namespace eBalanceKitBusiness {

    public interface IAccount : IBalanceListEntry {
        IAccountGroup AccountGroup { get; set; }
        bool IsUserDefined { get; set; }
        ISplitAccountGroup CreateSplitAccountGroup();        
    }
}
