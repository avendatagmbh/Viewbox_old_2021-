using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using eBalanceKitBusiness.Structures.DbMapping.BalanceList;
using System.ComponentModel;
using eBalanceKitBusiness.Structures;

namespace eBalanceKitBusiness {
    
    public interface ISplitAccountGroup {

        IAccount Account { get; }
        IBalanceList BalanceList { get; }
        ValueInputMode ValueInputMode { get; set; }
        IEnumerable<ISplittedAccount> Items { get; }
        bool IsVisible { get; }
        
        decimal AmountSum { get; }
        string AmountSumDisplayString { get; }

        void Add(ISplittedAccount splittedAccount);
        void Remove(ISplittedAccount splittedAccount);
        ISplittedAccount AddNewItem();
        void Save();
        
        ISplitAccountGroup Clone();

        /// <summary>
        /// Updates all editable values from the specified splitted account group.
        /// </summary>
        /// <param name="sag"></param>
        void Update(ISplitAccountGroup sag);

        IEnumerable<string> ValidationErrorMessages { get; }
        bool IsValid { get; }

        bool GetVisibility(BalanceListFilterOptions filterOptions);
    }
}
