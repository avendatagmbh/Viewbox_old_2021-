using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using eBalanceKitBusiness.Structures.DbMapping.BalanceList;

namespace eBalanceKitBusiness {
    
    public interface ISplittedAccount : IBalanceListEntry {        
        decimal? AmountPercent { get; set; }
        ISplitAccountGroup SplitAccountGroup { get; }
        bool IsValid { get; }
        void Validate();
        bool IsAssignedToReferenceList { get; set; }
    }
}
