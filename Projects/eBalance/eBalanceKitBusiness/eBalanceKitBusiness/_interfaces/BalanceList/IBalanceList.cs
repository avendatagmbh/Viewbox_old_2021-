using System;
using System.Collections.Generic;
using eBalanceKitBusiness.Structures.DbMapping.BalanceList;
using eBalanceKitBusiness.Structures;

namespace eBalanceKitBusiness {

    /// <summary>
    /// Interface for balance lists.
    /// </summary>
    /// <author>Mirko Dibbert</author>
    /// <since>2011-06-26</since>
    public interface IBalanceList {

        int Id { get; set; }
        string Name { get; set; }
        string Comment { get; set; }

        /// <summary>
        /// Source of the balance list (i.e. filename).
        /// </summary>
        string Source { get; set; }

        /// <summary>
        /// Enumeration of all accounts, which are not splitted or assigned to any account group.
        /// </summary>
        IEnumerable<IAccount> Accounts { get; }

        /// <summary>
        /// Gets the count of the Accounts enumeration.
        /// </summary>
        int AccountsCount { get; }

        /// <summary>
        /// Enumeration of all account groups.
        /// </summary>
        IEnumerable<IAccountGroup> AccountGroups { get; }

        /// <summary>
        /// Gets the count of the AccountGroups enumeration.
        /// </summary>
        int AccountGroupsCount { get; }

        /// <summary>
        /// Enumeration of all split account groups.
        /// </summary>
        IEnumerable<ISplitAccountGroup> SplitAccountGroups { get; }

        /// <summary>
        /// Gets the count of the SplitAccountGroups enumeration.
        /// </summary>
        int SplitAccountGroupsCount { get; }

        bool DoDbUpdate { get; set; }

        void AddAssignment(eBalanceKitBusiness.IBalanceListEntry item, string elementId);

        void AddAssignments(List<Tuple<IBalanceListEntry, int, int>> logInfo, DbAccess.IDatabase conn,
                            eBalanceKitBase.Structures.ProgressInfo pi);

        IEnumerable<eBalanceKitBusiness.IBalanceListEntry> AssignedItems { get; }
        string AssignedItemsInfo { get; }
        void ClearEntries();
        string DisplayString { get; }
        eBalanceKitBusiness.Structures.DbMapping.Document Document { get; set; }
        DateTime? ImportDate { get; set; }
        string ImportDescription { get; }
        eBalanceKitBusiness.Structures.DbMapping.User ImportedFrom { get; set; }
        int ImportedFromId { get; set; }
        bool IsImported { get; }
        event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        void RemoveAssignment(eBalanceKitBusiness.IBalanceListEntry account);

        void RemoveAssignments(IEnumerable<eBalanceKitBusiness.IBalanceListEntry> accounts, DbAccess.IDatabase conn,
                               eBalanceKitBase.Structures.ProgressInfo pi);

        void SetEntries(
            IEnumerable<IAccount> accounts,
            IEnumerable<IAccountGroup> groups,
            IEnumerable<ISplitAccountGroup> splittedAccounts);

        IEnumerable<eBalanceKitBusiness.IBalanceListEntry> UnassignedItems { get; }
        string UnassignedItemsInfo { get; }
        void UpdateFilter();

        void AddSplitAccountGroup(ISplitAccountGroup sag);
        void RemoveSplitAccountGroup(ISplitAccountGroup sag);

        /// <summary>
        /// Returns true, if the specified account number is already used. If account is defined,
        /// all splitted accounts in the respective split account group will be ignored.
        /// </summary>
        /// <param name="account"></param>
        /// <param name="number"></param>
        /// <returns></returns>
        bool IsNumberDefined(string number, IBalanceListEntry entry = null);

        string ToXml();

        /// <summary>
        /// Adds the specified item to the balance list.
        /// </summary>
        /// <param name="item"></param>
        void AddItem(IBalanceListEntry item);

        /// <summary>
        /// Removes the specified item from the balance list.
        /// </summary>
        /// <param name="item"></param>
        void RemoveItem(IBalanceListEntry item);

        void AddAccountGroup(IAccountGroup group);
        void RemoveAccountGroup(IAccountGroup group);

        BalanceListSortOptions SortOptions { get; }
        BalanceListFilterOptions ItemsFilter { get; }
        BalanceListFilterOptions AccountsFilter { get; }
        BalanceListFilterOptions AccountGroupsFilter { get; }
        BalanceListFilterOptions SplitAccountGroupsFilter { get; }
    }
}
