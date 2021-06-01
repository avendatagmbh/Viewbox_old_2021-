// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-01-18
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using DbAccess;
using Utils;
using eBalanceKitBase.Structures;
using eBalanceKitBusiness.Logs;
using eBalanceKitBusiness.Manager;
using eBalanceKitResources.Localisation;

namespace eBalanceKitBusiness.Structures.DbMapping.BalanceList {
    /// <summary>
    /// This class represents an imported balance list (non peristent part).
    /// </summary>
    internal partial class BalanceList : INotifyPropertyChanged, IBalanceList {

        private class BalanceListSortItem : IComparable {
            public object Item { get; set; }
            public object Value { get; set; }

            public int CompareTo(object obj) {
                if (!(obj is BalanceListSortItem)) return 0;

                var sortItem = obj as BalanceListSortItem;

                if (Item is IBalanceListEntry && sortItem.Item is IBalanceListEntry) {
                    if (Value is Int64 && sortItem.Value is Int64) return ((Int64) Value).CompareTo((Int64) sortItem.Value);
                    if (Value is Int64) return 1;
                    if (sortItem.Value is Int64) return -1;

                    if (Value is Decimal && sortItem.Value is Decimal) return ((Decimal) Value).CompareTo((Decimal) sortItem.Value);
                    if (Value is Decimal) return 1;
                    if (sortItem.Value is Decimal) return -1;

                    return (Value.ToString()).CompareTo(sortItem.Value.ToString());
                }

                if (Item is IBalanceListEntry) return 1;
                if (sortItem.Item is IBalanceListEntry) return -1;
                return (Value.ToString()).CompareTo(sortItem.Value.ToString());
            }
        }

        public BalanceList() {
            SortOptions = new BalanceListSortOptions(this);
            _itemsFilter = new BalanceListFilterOptions(this);
            _accountsFilter = new BalanceListFilterOptions(this) {ShowHiddenItems = true};
            _accountGroupsFilter = new BalanceListFilterOptions(this) {ShowHiddenItems = true};
            _splitAccountGroupsFilter = new BalanceListFilterOptions(this) {ShowHiddenItems = true};
            _isHidden = false;
        }

        #region events
        public event PropertyChangedEventHandler PropertyChanged;

        internal void OnPropertyChanged(string property) { if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(property)); }
        #endregion events

        #region eventHandler
        //--------------------------------------------------------------------------------

        private void ImportedFromPropertyChanged(object sender, PropertyChangedEventArgs e) {
            if (e.PropertyName == "Id") {
                ImportedFromId = ImportedFrom.Id;
            }
        }

        //--------------------------------------------------------------------------------
        #endregion eventHandler

        #region properties

        #region DisplayString
        public string DisplayString { get { return Name + (string.IsNullOrEmpty(Comment) ? "" : " (" + Comment + ")"); } }
        #endregion

        #region IsImported
        public bool IsImported { get { return ImportedFrom != null && ImportDate.HasValue; } }
        #endregion

        #region ImportedFrom
        private User _importedFrom;

        /// <summary>
        /// Gets or sets the user, which imported this instance.
        /// </summary>
        /// <value>The imported from.</value>
        public User ImportedFrom {
            get { return _importedFrom; }
            set {
                if (_importedFrom != value) {
                    _importedFrom = value;
                    _importedFrom.PropertyChanged += ImportedFromPropertyChanged;
                    ImportedFromId = value.Id;
                }
                OnPropertyChanged("ImportedFrom");
                OnPropertyChanged("ImportDescription");
            }
        }
        #endregion

        #region ImportDescription
        public string ImportDescription {
            get {
                return IsImported
                           ? "Importiert von " + ImportedFrom.FullName + " am " +
                             ImportDate.Value.ToString("dd.MM.yyyy") +
                             "."
                           : "Es wurden noch keine Daten importiert.";
            }
        }
        #endregion

        #region Accounts
        private readonly ObservableCollectionAsync<IAccount> _accounts = new ObservableCollectionAsync<IAccount>();

        private readonly ObservableCollectionAsync<IAccount> _accountsDisplayed =
            new ObservableCollectionAsync<IAccount>();

        public IEnumerable<IAccount> AccountsDisplayed { get { return _accountsDisplayed; } }

        public string AccountsInfo {
            get {
                if (_accounts.Count == 0) return "Keine Konten vorhanden";

                if (_accounts.Count != _accountsDisplayed.Count) return _accountsDisplayed.Count + " von " + _accounts.Count + " Konten (gefiltert)";

                return _accounts.Count + " Konten";
            }
        }

        public IEnumerable<IAccount> Accounts { get { return _accounts; } }

        public int AccountsCount { get { return _accounts.Count; } }
        #endregion

        #region AccountGroups
        private readonly ObservableCollectionAsync<IAccountGroup> _accountGroups =
            new ObservableCollectionAsync<IAccountGroup>();

        private readonly ObservableCollectionAsync<IAccountGroup> _accountGroupsDisplayed =
            new ObservableCollectionAsync<IAccountGroup>();

        public IEnumerable<IAccountGroup> AccountGroupsDisplayed { get { return _accountGroupsDisplayed; } }

        public string AccountGroupsInfo {
            get {
                if (_accountGroups.Count == 0) return "Keine Kontogruppen vorhanden";

                if (_accountGroups.Count != _accountGroupsDisplayed.Count) return _accountGroupsDisplayed.Count + " von " + _accountGroups.Count + " Kontogruppen (gefiltert)";

                return _accountGroups.Count + " Kontogruppen";
            }
        }

        public IEnumerable<IAccountGroup> AccountGroups { get { return _accountGroups; } }

        public int AccountGroupsCount { get { return _accountGroups.Count; } }
        #endregion

        #region SplitAccountGroups
        private readonly ObservableCollectionAsync<ISplitAccountGroup> _splitAccountGroups =
            new ObservableCollectionAsync<ISplitAccountGroup>();

        private readonly ObservableCollectionAsync<ISplitAccountGroup> _splitAccountGroupsDisplayed =
            new ObservableCollectionAsync<ISplitAccountGroup>();

        public IEnumerable<ISplitAccountGroup> SplitAccountGroupsDisplayed { get { return _splitAccountGroupsDisplayed; } }

        public string SplitAccountGroupsInfo {
            get {
                if (_splitAccountGroups.Count == 0) return "Keine Kontoaufteilungen vorhanden";

                if (_splitAccountGroups.Count != _splitAccountGroupsDisplayed.Count)
                    return _splitAccountGroupsDisplayed.Count + " von " + _splitAccountGroups.Count +
                           " Kontoaufteilungen (gefiltert)";
                return _splitAccountGroups.Count + " Kontoaufteilungen";
            }
        }

        public IEnumerable<ISplitAccountGroup> SplitAccountGroups { get { return _splitAccountGroups; } }

        public int SplitAccountGroupsCount { get { return _splitAccountGroups.Count; } }
        #endregion

        #region UnassignedItems
        private readonly Dictionary<IBalanceListEntry, IBalanceListEntry> _unassignedItems =
            new Dictionary<IBalanceListEntry, IBalanceListEntry>();

        private readonly ObservableCollectionAsync<IBalanceListEntry> _unassignedItemsDisplayed =
            new ObservableCollectionAsync<IBalanceListEntry>();

        public string UnassignedItemsHeader { get { return string.Format(ResourcesBalanceList.NotAssignedAccounts_Count ,_unassignedItems.Count); } }

        private Dictionary<IBalanceListEntry, IBalanceListEntry> UnassignedItemsDict { get { return _unassignedItems; } }

        public IEnumerable<IBalanceListEntry> UnassignedItemsDisplayed { get { return _unassignedItemsDisplayed; } }

        public string UnassignedItemsInfo {
            get {
                try {
                    int unassignedItemsCount = ItemsFilter.ShowHiddenItems
                                                   ? _unassignedItems.Count
                                                   : _unassignedItems.Count - HiddenUnassignedItemsCount;

                    decimal balance = _unassignedItems.Values
                        .Where(item => ItemsFilter.ShowHiddenItems || !item.IsHidden)
                        .Sum(item => item.Amount);

                    string balanceString = " / Summe: " + LocalisationUtils.CurrencyToString(balance);

                    if (_unassignedItems.Count == 0) return "Keine nicht zugeordneten Konten vorhanden";

                    if (unassignedItemsCount != _unassignedItemsDisplayed.Count)
                        return _unassignedItemsDisplayed.Count + " von " + unassignedItemsCount + " Konten (gefiltert)" +
                               balanceString;

                    return unassignedItemsCount + " Konten" + balanceString;
                } catch (Exception) {
                    return "???";
                }
            }
        }

        public IEnumerable<IBalanceListEntry> UnassignedItems { get { return _unassignedItems.Values; } }
        #endregion

        #region AssignedItems
        private readonly Dictionary<IBalanceListEntry, IBalanceListEntry> _assignedItems =
            new Dictionary<IBalanceListEntry, IBalanceListEntry>();

        private readonly ObservableCollectionAsync<IBalanceListEntry> _assignedItemsDisplayed =
            new ObservableCollectionAsync<IBalanceListEntry>();

        public string AssignedItemsHeader { get { return string.Format(ResourcesBalanceList.AssignedAccounts_Count, _assignedItems.Count); } }

        private Dictionary<IBalanceListEntry, IBalanceListEntry> AssignedItemsDict { get { return _assignedItems; } }

        public IEnumerable<IBalanceListEntry> AssignedItemsDisplayed { get { return _assignedItemsDisplayed; } }

        public string AssignedItemsInfo {
            get {
                decimal balance = _assignedItems.Values.Sum(item => item.Amount);
                string balanceString = " / Summe: " + LocalisationUtils.CurrencyToString(balance);

                if (_assignedItems.Count == 0) return "Keine Konten";

                if (_assignedItems.Count != _assignedItemsDisplayed.Count)
                    return _assignedItemsDisplayed.Count + " von " + _assignedItems.Count + " Konten (gefiltert)" +
                           balanceString;

                return _assignedItems.Count + " Konten" + balanceString;
            }
        }

        public IEnumerable<IBalanceListEntry> AssignedItems { get {  return _assignedItems.Values; } }
        #endregion

        #region hidden item counts
        //----------------------------------------
        public int HiddenAccountsCount {
            get {
                try {
                    return _accounts.Count(item => item.IsHidden);
                } catch {
                    return -1;
                }
            }
        }

        public int HiddenAccountGroupsCount {
            get {
                try {
                    return _accountGroups.Count(item => item.IsHidden);
                } catch {
                    return -1;
                }
            }
        }

        public int HiddenUnassignedItemsCount {
            get {
                try {
                    return _unassignedItems.Values.Count(item => item.IsHidden);
                } catch {
                    return -1;
                }
            }
        }

        //----------------------------------------
        #endregion hidden item counts

        #region ItemsFilter
        private readonly BalanceListFilterOptions _itemsFilter;

        public BalanceListFilterOptions ItemsFilter { get { return _itemsFilter; } }
        #endregion

        #region BalanceListSortOptions
        public BalanceListSortOptions SortOptions { get; private set; }
        #endregion

        #region AccountsFilter
        private readonly BalanceListFilterOptions _accountsFilter;

        public BalanceListFilterOptions AccountsFilter { get { return _accountsFilter; } }
        #endregion

        #region AccountGroupsFilter
        private readonly BalanceListFilterOptions _accountGroupsFilter;

        public BalanceListFilterOptions AccountGroupsFilter { get { return _accountGroupsFilter; } }
        #endregion

        #region SplitAccountGroupsFilter
        private readonly BalanceListFilterOptions _splitAccountGroupsFilter;

        public BalanceListFilterOptions SplitAccountGroupsFilter { get { return _splitAccountGroupsFilter; } }
        #endregion

        #region IsSelected
        private bool _isSelected;

        /// <summary>
        /// Needed for GUI displaying purposes.
        /// </summary>
        public bool IsSelected {
            get { return _isSelected; }
            set {
                _isSelected = value;
                OnPropertyChanged("IsSelected");
            }
        }
        #endregion

        #region DoDbUpdate
        public bool DoDbUpdate { get; set; }
        #endregion

        #region AllAssignedItemsChecked
        private bool? _allAssignedItemsChecked = false;

        public bool? AllAssignedItemsChecked {
            get { return _allAssignedItemsChecked; }
            set {
                if (!value.HasValue) return;
                _allAssignedItemsChecked = value;

                foreach (var item in _assignedItems.Values) item.IsChecked = value.Value;

                OnPropertyChanged("AllAssignedItemsChecked");
            }
        }
        #endregion AllAssignedItemsChecked

        #region AssignedItemsCount
        private int _checkedAssignedItemsCount;

        public int CheckedAssignedItemsCount {
            get { return _checkedAssignedItemsCount; }
            set {
                _checkedAssignedItemsCount = value;
                OnPropertyChanged("CheckedAssignedItemsCount");
            }
        }
        #endregion

        #region IsDocumentSelectedBalanceList
        private bool _isDocumentSelectedBalanceList;

        public bool IsDocumentSelectedBalanceList {
            get { return _isDocumentSelectedBalanceList; }
            set {
                _isDocumentSelectedBalanceList = value;

                foreach (var item in AssignedItems.Union(UnassignedItems))
                    item.IsDocumentSelectedBalanceList = value;

                OnPropertyChanged("IsDocumentSelectedBalanceList");
            }
        }
        #endregion

        #endregion properties

        #region methods
        //--------------------------------------------------------------------------------

        #region IsHidden
        private bool _isHidden;

        public bool IsHidden {
            get { return _isHidden; }
            set {
                if (_isHidden != value) {
                    _isHidden = value;
                    OnPropertyChanged("IsHidden");
                }
            }
        }
        #endregion IsHidden

        public void FirePropertyChangedEventsForDisplayedItems() {
            OnPropertyChanged("UnassignedItemsDisplayed");
            OnPropertyChanged("AssignedItemsDisplayed");
        }

        public void AddSplitAccountGroup(ISplitAccountGroup sag) {
            _splitAccountGroups.Add(sag);
            foreach (var sa in sag.Items) AddItem(sa);
            RemoveItem(sag.Account);
            UpdateFilter();

            // update visual
            UpdateItemsFilter();
            UpdateSplitAccountGroupsFilter();
        }

        public void RemoveSplitAccountGroup(ISplitAccountGroup sag) {
            _splitAccountGroups.Remove(sag);
            foreach (var sa in sag.Items) RemoveItem(sa);
            AddItem(sag.Account);

            // remove splitted account group from database
            using (IDatabase conn = AppConfig.ConnectionManager.GetConnection()) {
                try {
                    conn.BeginTransaction();

                    (sag as SplitAccountGroup).RemoveAllItems(conn);
                    conn.DbMapping.Delete(sag);

                    conn.CommitTransaction();
                } catch (Exception ex) {
                    conn.RollbackTransaction();
                    throw new Exception(ex.Message, ex);
                }
            }

            // update visual
            UpdateItemsFilter();
            UpdateSplitAccountGroupsFilter();
        }

        public void AddAccountGroup(IAccountGroup group) {
            AddItem(group);
            foreach (var account in group.Items) RemoveItem(account);

            UpdateItemsFilter();
            UpdateAccountGroupsFilter();
        }

        public void RemoveAccountGroup(IAccountGroup group) {
            RemoveItem(group);
            foreach (var account in group.Items) AddItem(account);
            group.ClearItems();

            // remove account group from database
            using (IDatabase conn = AppConfig.ConnectionManager.GetConnection()) {
                try {
                    conn.BeginTransaction();
                    conn.DbMapping.Delete(group);
                    conn.CommitTransaction();
                } catch (Exception ex) {
                    conn.RollbackTransaction();
                    throw new Exception(ex.Message, ex);
                }
            }

            UpdateItemsFilter();
            UpdateAccountGroupsFilter();
        }

        public bool IsNumberDefined(string number, IBalanceListEntry entry) {
            var tmp = new List<IBalanceListEntry>(UnassignedItems.Concat(AssignedItems));
            foreach (var accountGroup in _accountGroups) tmp.AddRange(accountGroup.Items);
            tmp.AddRange(_splitAccountGroups.Select(sag => sag.Account));

            if (entry == null) return tmp.Any(item => item.Number == number);

            return (tmp.Any(item => (item.Number == number) &&
                                    !(item is SplittedAccount && (item as SplittedAccount).BaseAccount == entry) &&
                                    !(item is AccountGroup && ((BalanceListEntryBase) item).Id == ((BalanceListEntryBase) entry).Id)));
        }

        #region AddItem
        public void AddItem(IBalanceListEntry item) {
            if (item.HasAssignment) AssignedItemsDict[item] = item;
            else UnassignedItemsDict[item] = item;

            if (item is IAccount) {
                _accounts.Add(item as IAccount);
                
                _accountsDisplayed.Add(item as IAccount);
                
                OnPropertyChanged("AccountsInfo");
                OnPropertyChanged("AccountsCount");
            } else if (item is IAccountGroup) {
                _accountGroups.Add(item as IAccountGroup);
                _accountGroupsDisplayed.Add(item as IAccountGroup);
                OnPropertyChanged("AccountGroupsInfo");
                OnPropertyChanged("AccountGroupsCount");
            }

            item.IsDocumentSelectedBalanceList = IsDocumentSelectedBalanceList;
        }
        #endregion AddItem

        #region RemoveItem 
        public void RemoveItem(IBalanceListEntry item) {
            if (item.HasAssignment) {
                item.RemoveFromParents();
                AssignedItemsDict.Remove(item);
            } else {
                UnassignedItemsDict.Remove(item);
            }

            if (item is IAccount) {
                _accounts.Remove(item as IAccount);
                _accountsDisplayed.Remove(item as IAccount);
                OnPropertyChanged("AccountsInfo");
                OnPropertyChanged("AccountsCount");
            } else if (item is IAccountGroup) {
                _accountGroups.Remove(item as IAccountGroup);
                _accountGroupsDisplayed.Remove(item as IAccountGroup);
                OnPropertyChanged("AccountGroupsInfo");
                OnPropertyChanged("AccountGroupsCount");
            }
        }
        #endregion RemoveItem

        /// <param name="showHiddenEntriesAnyway">For example assigned accounts should be shown even if they were invisble before.</param>
        private void UpdateFilter<TKey>(
            BalanceListFilterOptions filterOptions,
            ICollection<IBalanceListEntry> target,
            IEnumerable<IBalanceListEntry> items,
            Func<IBalanceListEntry, TKey> selector,
            bool showHiddenEntriesAnyway = false) {
            var sortedCollection = new List<IBalanceListEntry>(
                from item in items
                orderby selector(item)
                where item.GetVisibility(filterOptions, showHiddenEntriesAnyway)
                select item);

            foreach (var item in sortedCollection) target.Add(item);
        }

        private void UpdateFilter<TKey>(
            BalanceListFilterOptions filterOptions,
            ICollection<IAccount> target,
            IEnumerable<IAccount> items,
            Func<IBalanceListEntry, TKey> selector) {
            var sortedCollection = new List<IBalanceListEntry>(
                from item in items
                orderby selector(item)
                where item.GetVisibility(filterOptions)
                select item);

            foreach (IAccount item in sortedCollection) target.Add(item);
        }

        private void UpdateFilter<TKey>(
            BalanceListFilterOptions filterOptions,
            ICollection<IAccountGroup> target,
            IEnumerable<IAccountGroup> items,
            Func<IBalanceListEntry, TKey> selector) {
            var sortedCollection = new List<IBalanceListEntry>(
                from item in items
                orderby selector(item)
                where item.GetVisibility(filterOptions)
                select item);

            foreach (IAccountGroup item in sortedCollection) target.Add(item);
        }

        private void UpdateFilter<TKey>(
            BalanceListFilterOptions filterOptions,
            ICollection<ISplitAccountGroup> target,
            IEnumerable<ISplitAccountGroup> items,
            Func<ISplitAccountGroup, TKey> selector) {
            var sortedCollection = new List<ISplitAccountGroup>(
                from item in items
                orderby selector(item)
                where item.GetVisibility(filterOptions)
                select item);

            foreach (var item in sortedCollection) target.Add(item);
        }

        public void Save() {
            using (IDatabase conn = AppConfig.ConnectionManager.GetConnection()) {
                if (DoDbUpdate) conn.DbMapping.Save(this);
            }
        }

        internal void UpdateAllAssignedItemsChecked() {
            CheckedAssignedItemsCount = _assignedItems.Values.Count(item1 => item1.IsChecked);

            if (CheckedAssignedItemsCount == 0) _allAssignedItemsChecked = false;
            else if (CheckedAssignedItemsCount == AssignedItems.Count()) _allAssignedItemsChecked = true;
            else _allAssignedItemsChecked = null;

            OnPropertyChanged("AllAssignedItemsChecked");
        }

        #region UpdateFilter
        public void UpdateFilter() {
            UpdateItemsFilter();
            UpdateAccountsFilter();
            UpdateAccountGroupsFilter();
            UpdateSplitAccountGroupsFilter();
        }

        public void UpdateItemsFilter() {
            _unassignedItemsDisplayed.Clear();
            _assignedItemsDisplayed.Clear();

            Func<IBalanceListEntry, BalanceListSortItem> selector = GetSelector();

            UpdateFilter(ItemsFilter, _unassignedItemsDisplayed, UnassignedItems, selector);
            UpdateFilter(ItemsFilter, _assignedItemsDisplayed, AssignedItems, selector, true);

            OnPropertyChanged("HiddenUnassignedItemsCount");
            OnPropertyChanged("UnassignedItemsHeader");
            OnPropertyChanged("UnassignedItemsInfo");

            OnPropertyChanged("AssignedItemsHeader");
            OnPropertyChanged("AssignedItemsInfo");
        }

        public void UpdateAccountsFilter() {
            _accountsDisplayed.Clear();

            Func<IBalanceListEntry, object> selector = GetSelector();

            UpdateFilter(AccountsFilter, _accountsDisplayed, Accounts, selector);
            OnPropertyChanged("HiddenAccountsCount");
            OnPropertyChanged("AccountsInfo");
        }

        public void UpdateAccountGroupsFilter() {
            _accountGroupsDisplayed.Clear();

            Func<IBalanceListEntry, object> selector = GetSelector();

            UpdateFilter(AccountGroupsFilter, _accountGroupsDisplayed, AccountGroups, selector);
            OnPropertyChanged("HiddenAccountGroupsCount");
            OnPropertyChanged("AccountGroupsInfo");
        }

        public void UpdateSplitAccountGroupsFilter() {
            _splitAccountGroupsDisplayed.Clear();

            Func<ISplitAccountGroup, object> selector = GetGroupSelector();
            
            UpdateFilter(SplitAccountGroupsFilter, _splitAccountGroupsDisplayed, SplitAccountGroups, selector);
            OnPropertyChanged("SplitAccountGroupsInfo");
        }
        #endregion

        #region AddAssignment
        public void AddAssignment(IBalanceListEntry item, string elementId) {
            // remove hidden state (assigned items should never be hidden)
            item.IsHidden = false;

            LogManager.Instance.UpdateAssignment(item, item.AssignedElementId,
                                                 Document.TaxonomyIdManager.GetId(elementId), Document);
            Document.TaxonomyIdManager.SetElementAssignment(item, elementId);
            using (IDatabase conn = AppConfig.ConnectionManager.GetConnection()) {
                conn.DbMapping.Save(item);
            }

            AssignedItemsDict[item] = item;
            if (UnassignedItemsDict.ContainsKey(item)) UnassignedItemsDict.Remove(item);

            UpdateFilter();
        }
        #endregion

        #region AddAssignments
        //Stores Account, OldAssignmentId, NewAssignmentId
        public void AddAssignments(List<Tuple<IBalanceListEntry, int, int>> accounts, IDatabase conn, ProgressInfo pi) {
            LogManager.Instance.UpdateAssignments(accounts, Document);
            foreach (var account in accounts) {
                if (account.Item1 is VirtualAccount && !account.Item1.HasAssignment) {
                    // We don't need to change the account dicts because there is no valid assignment processing here
                    continue;
                }
                conn.DbMapping.Save(account.Item1);
                // only if the assigned account is in the current balancelist we need to update the account dicts
                if (this == account.Item1.BalanceList) {//account.Item1.BalanceList.IsDocumentSelectedBalanceList
                    AssignedItemsDict[account.Item1] = account.Item1;
                    if (UnassignedItemsDict.ContainsKey(account.Item1)) UnassignedItemsDict.Remove(account.Item1);
                }
                if (pi != null && pi.Value < pi.Maximum) pi.Value++;
            }
            UpdateFilter();
        }
        #endregion

        #region RemoveAssignment
        public void RemoveAssignment(IBalanceListEntry account) {
            if (!UnassignedItemsDict.ContainsKey(account)) {
                LogManager.Instance.DeleteAssignment(account, Document);
                // update account
                TaxonomyIdManager.RemoveElementAssignment(account);
                using (IDatabase conn = AppConfig.ConnectionManager.GetConnection()) {
                    conn.DbMapping.Save(account);
                }

                // update account lists
                AssignedItemsDict.Remove(account);
                UnassignedItemsDict.Add(account, account);
                UpdateFilter();
            }
        }
        #endregion

        #region RemoveAssignments
        public void RemoveAssignments(IEnumerable<IBalanceListEntry> accounts, IDatabase conn, ProgressInfo pi) {
            LogManager.Instance.DeleteAssignments(accounts, Document);
            foreach (var account in accounts) {
                if (!UnassignedItemsDict.ContainsKey(account)) {
                    // update account
                    TaxonomyIdManager.RemoveElementAssignment(account);
                    conn.DbMapping.Save(account);

                    // update account lists
                    AssignedItemsDict.Remove(account);
                    UnassignedItemsDict.Add(account, account);
                }

                if (pi != null && pi.Value < pi.Maximum) pi.Value++;
            }
            UpdateFilter();
        }
        #endregion

        #region ClearEntries
        public void ClearEntries() {
            _accounts.Clear();
            _accountGroups.Clear();
            _splitAccountGroups.Clear();
            _assignedItems.Clear();
            _unassignedItems.Clear();
        }
        #endregion

        #region SetEntries
        public void SetEntries(
            IEnumerable<IAccount> accounts,
            IEnumerable<IAccountGroup> groups,
            IEnumerable<ISplitAccountGroup> splitAccountGroups) {
            ClearEntries();

            var groupedAccounts = new HashSet<long>();

            // collect grouped accounts
            if (groups != null)
                foreach (var g in groups)
                    foreach (var account in g.Items) {
                        groupedAccounts.Add(((BalanceListEntryBase) account).Id);
                    }

            // collect splitted accounts
            if (splitAccountGroups != null) 
                foreach (var sag in splitAccountGroups) 
                    groupedAccounts.Add(((BalanceListEntryBase) sag.Account).Id);

            // add accounts (splitted or grouped accounts will be ignored)
            foreach (var account in accounts) {

                if (!groupedAccounts.Contains(((BalanceListEntryBase)account).Id)) {
                    
                    
                    // Change request: Hide "empty" accounts (done by sev)
                    //if (account.Amount == decimal.Parse("0.00")) {
                    //    account.DoDbUpdate = false;
                    //    account.IsVisible = false;
                    //    account.IsHidden = true;
                    //    account.DoDbUpdate = true;
                    //}
                    AddItem(account);
                }
            }
            // add account groups
            if (groups != null) 
                foreach (var g in groups) 
                    AddItem(g);

            // add splitted accounts
            if (splitAccountGroups != null) {
                foreach (var sag in splitAccountGroups) {
                    _splitAccountGroups.Add(sag);
                    foreach (var sa in sag.Items) AddItem(sa);
                }
            }

            UpdateFilter();
        }
        #endregion

        private Func<IBalanceListEntry, BalanceListSortItem> GetSelector() {
            Func<IBalanceListEntry, BalanceListSortItem> selector;
            Int64 tmp = 0;
            switch (SortOptions.SortType) {
                case BalanceListSortType.AccountNumber:
                    if (SortOptions.UseNumericSortIfPossible)
                        selector =
                            item =>
                            (Int64.TryParse(item.Number, out tmp)
                                 ? new BalanceListSortItem { Item = item, Value = tmp }
                                 : new BalanceListSortItem { Item = item, Value = item.Number });
                    else
                        selector = item => new BalanceListSortItem { Item = item, Value = item.Number };
                    break;

                case BalanceListSortType.AccountName:
                    selector = item => new BalanceListSortItem { Item = item, Value = item.Name };
                    break;

                case BalanceListSortType.Index:
                    if (SortOptions.UseNumericSortIfPossible)
                        selector =
                            item =>
                            (Int64.TryParse(item.SortIndex, out tmp)
                                 ? new BalanceListSortItem { Item = item, Value = tmp }
                                 : new BalanceListSortItem { Item = item, Value = item.SortIndex });
                    else
                        selector = item => new BalanceListSortItem { Item = item, Value = item.SortIndex };
                    break;

                case BalanceListSortType.Original:
                    selector = item => new BalanceListSortItem { Item = item, Value = ((BalanceListEntryBase) item).Id };
                    break;

                default:
                    throw new NotImplementedException();
            }
            return selector;
        }

        private Func<ISplitAccountGroup, BalanceListSortItem> GetGroupSelector() {
            Func<ISplitAccountGroup, BalanceListSortItem> selector;
            Int64 tmp = 0;
            switch (SortOptions.SortType) {
                case BalanceListSortType.AccountNumber:
                    if (SortOptions.UseNumericSortIfPossible)
                        selector =
                            item =>
                            (Int64.TryParse(item.Account.Number, out tmp)
                                 ? new BalanceListSortItem { Item = item, Value = tmp }
                                 : new BalanceListSortItem { Item = item, Value = item.Account.Number });
                    else
                        selector = item => new BalanceListSortItem { Item = item, Value = item.Account.Number };
                    break;

                case BalanceListSortType.AccountName:
                    selector = item => new BalanceListSortItem { Item = item, Value = item.Account.Name };
                    break;

                case BalanceListSortType.Index:
                    if (SortOptions.UseNumericSortIfPossible)
                        selector =
                            item =>
                            (Int64.TryParse(item.Account.SortIndex, out tmp)
                                 ? new BalanceListSortItem { Item = item, Value = tmp }
                                 : new BalanceListSortItem { Item = item, Value = item.Account.SortIndex });
                    else
                        selector = item => new BalanceListSortItem { Item = item, Value = item.Account.SortIndex };
                    break;

                case BalanceListSortType.Original:
                    selector = item => new BalanceListSortItem { Item = item, Value = ((BalanceListEntryBase) item.Account).Id };
                    break;

                default:
                    throw new NotImplementedException();
            }
            return selector;
        }
        
        //--------------------------------------------------------------------------------
        #endregion methods
    }
}