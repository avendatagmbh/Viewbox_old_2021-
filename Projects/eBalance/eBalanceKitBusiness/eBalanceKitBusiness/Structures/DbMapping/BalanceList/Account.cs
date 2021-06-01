using System;
using DbAccess;
using Taxonomy.Interfaces.PresentationTree;
using eBalanceKitBusiness.Interfaces;
using Utils;
using eBalanceKitBusiness.Manager;

namespace eBalanceKitBusiness.Structures.DbMapping.BalanceList {
    
    /// <summary>
    /// This class represents an account of an imported balance list (non persistent part).
    /// </summary>
    /// <author>Mirko Dibbert</author>
    /// <since>2010-12-28</since>
    internal partial class Account : BalanceListEntryBase, IAccount, IComparable {
        
        public ISplitAccountGroup CreateSplitAccountGroup() {
            SplitAccountGroup sag = new SplitAccountGroup { Account = this, BalanceList = this.BalanceList as BalanceList};
            
            // add minimum valid count of entries
            sag.AddNewItem();
            sag.AddNewItem();
            
            return sag;
        }

        /// <summary>
        /// Checks if an assignment that is in process is allowed. Allways true if it is not a VirtualAccount.
        /// </summary>
        /// <param name="assignedNode">The destination IPresentationTreeNode where this account should be assigned to.</param>
        /// <returns>Is it allowed?</returns>
        public virtual bool CheckAssignmentAllowed(IPresentationTreeNode assignedNode) {
            //TODO: Polymorphie?!
            //if (this is VirtualAccount) {
            //    return (this as VirtualAccount).CheckAssignmentAllowed(assignedNode);
            //}
            return true;
        }

        #region AccountGroup
        IAccountGroup _accountGroup;

        public IAccountGroup AccountGroup {
            get { return _accountGroup; }
            set { 
                _accountGroup = value;
                AccountGroupId = (value != null ? ((BalanceListEntryBase)value).Id : 0);
            }
        }
        #endregion

        public bool IsAssignedToReferenceList { get; set; }
    }
}
