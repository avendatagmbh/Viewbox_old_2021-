using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using eBalanceKitBusiness.Logs;
using eBalanceKitBusiness.Structures.DbMapping;
using eBalanceKitBusiness.Structures;
using eBalanceKitBusiness.Structures.DbMapping.BalanceList;
using DbAccess;
using eBalanceKitResources.Localisation;

namespace eBalanceKitBusiness.Manager {
    
    /// <summary>
    /// 
    /// </summary>
    /// <author>Mirko Dibbert</author>
    /// <since>2011-06-13</since>
    public class BalanceListManager : IAccountReferenceListManipulator {

        private static BalanceListManager _instance;

        private BalanceListManager() { }

        public static BalanceListManager Instance { get { return _instance ?? (_instance = new BalanceListManager()); } }

        public static void InitBalanceListEntries(Document document, IBalanceList balanceList) {

            List<Account> accounts;
            List<AccountGroup> accountGroups;
            List<SplitAccountGroup> splitAccountGroups;
            List<SplittedAccount> splittedAccounts;
            using (var conn = AppConfig.ConnectionManager.GetConnection()) {

                // load accounts
                accounts = conn.DbMapping.Load<Account>(conn.Enquote("balance_list_id") + "=" + balanceList.Id);

                // load account groups
                accountGroups = conn.DbMapping.Load<AccountGroup>(conn.Enquote("balance_list_id") + "=" + balanceList.Id);
                
                // load splitted accounts groups and splitted accounts
                splitAccountGroups = conn.DbMapping.Load<SplitAccountGroup>(conn.Enquote("balance_list_id") + "=" + balanceList.Id);
                splittedAccounts = conn.DbMapping.Load<SplittedAccount>(conn.Enquote("balance_list_id") + "=" + balanceList.Id);

                // Load account reference list
                bool referenceListExists = false;
                try {
                    conn.DbMapping.Load<DbEntityAccountReferenceList>(conn.Enquote("document_id") + "=" + document.Id + " AND " + conn.Enquote("user_id") + "=" + UserManager.Instance.CurrentUser.Id).ForEach(dbEntity => {
                        dbEntity.Document = document;
                        dbEntity.User = UserManager.Instance.CurrentUser;

                        Instance.ReferenceList = new AccountReferenceList(dbEntity);
                        referenceListExists = true;
                    });
                } catch (Exception ex) {
                    throw new Exception(ResourcesBalanceList.LoadReferenceListException + ex.Message, ex);
                }
                if (!referenceListExists)
                    Instance.ReferenceList = new AccountReferenceList(document, UserManager.Instance.CurrentUser);
            }

            #region [ set IsAssignedToReferenceList on Account instances ]

            foreach (IAccountReferenceListItem item in Instance.ReferenceList.Items) {
                switch (item.AccountType) {
                    case AccountTypeEnum.Account:
                        Account account = accounts.FirstOrDefault(a => a.Id == item.AccountId);
                        if (account != null)
                            account.IsAssignedToReferenceList = true;
                        break;
                    case AccountTypeEnum.AccountGroup:
                        AccountGroup accountGroup = accountGroups.FirstOrDefault(a => a.Id == item.AccountId);
                        if (accountGroup != null)
                            accountGroup.IsAssignedToReferenceList = true;
                        break;
                    case AccountTypeEnum.SplittedAccount:
                        SplittedAccount splittedAccount = splittedAccounts.FirstOrDefault(a => a.Id == item.AccountId);
                        if (splittedAccount != null)
                            splittedAccount.IsAssignedToReferenceList = true;
                        break;
                    default:
                        throw new Exception(ResourcesBalanceList.UnknownAccountType + item.AccountType.ToString());
                }
            }

            #endregion [ set IsAssignedToReferenceList on Account instances ]

            // assign balance list properties
            foreach (var sag in splitAccountGroups) sag.BalanceList = balanceList as BalanceList;
            foreach (var group in accountGroups) group.BalanceList = balanceList as BalanceList;
            
            // init assigned elements
            document.TaxonomyIdManager.AssignElements(accounts);
            document.TaxonomyIdManager.AssignElements(splittedAccounts);
            document.TaxonomyIdManager.AssignElements(accountGroups);

            // create group dictionary
            var groupDictionary = new Dictionary<long, AccountGroup>();
            foreach (var group in accountGroups) {
                groupDictionary[group.Id] = group;
                group.DoDbUpdate = true;
            }
        
            // assign account to account groups
            foreach (var account in accounts) {
                if (account.AccountGroupId > 0) {
                    if (groupDictionary.ContainsKey(account.AccountGroupId)) groupDictionary[account.AccountGroupId].AddAccount(account);
                    else throw new Exception("Database Error (load account groups): Could not find account group with id " + account.AccountGroupId);
                }
            }
            
            // create account dictionary and balance list for accounts
            var accountDict = new Dictionary<long, Account>();
            foreach (var account in accounts) {
                accountDict[account.Id] = account;
                account.BalanceList = balanceList as BalanceList;
                account.DoDbUpdate = true;
            }

            // assign account to split account groups
            foreach (var sag in splitAccountGroups) {
                if (accountDict.ContainsKey(sag.AccountId)) sag.Account = accountDict[sag.AccountId];
                else throw new Exception("Database Error (load splitted accounts): Could not find account with id " + sag.AccountId);
            }

            // assign splitted accounts to respective group
            var splitAccountGroupsById = new Dictionary<long, SplitAccountGroup>();
            foreach (var sag in splitAccountGroups) splitAccountGroupsById[sag.Id] = sag;
            foreach (var sa in splittedAccounts) {
                sa.DoDbUpdate = true;
                if (!splitAccountGroupsById.ContainsKey(sa.SplitAccountGroupId))
                    throw new Exception("Database Error: Could not find assigned group for splitted account.");

                var sag = splitAccountGroupsById[sa.SplitAccountGroupId];
                sag.Add(sa);                
            }

            // init balance list entries
            balanceList.SetEntries(accounts, accountGroups, splitAccountGroups);
        }
        
        public static void AddBalanceList(Document document, IBalanceList balanceList) {
            document.BalanceLists.Add(balanceList);
            balanceList.Document = document;
            using (var conn = AppConfig.ConnectionManager.GetConnection()) {
                conn.BeginTransaction();
                try {
                    conn.DbMapping.Save(balanceList);
                    Save(conn, balanceList);
                    LogManager.Instance.BalanceListChange(balanceList, true);
                    conn.CommitTransaction();
                } catch (Exception ex) {
                    conn.RollbackTransaction();
                    throw ex;
                }
            }
            
        }

        internal static void Save(IDatabase conn, IBalanceList balanceList) {
            conn.DbMapping.Save(typeof(Account), balanceList.Accounts);
        }

        #region RemoveBalanceList
        /// <summary>
        /// Removes the specified balance list.
        /// </summary>
        public static void RemoveBalanceList(IBalanceList balanceList) {
            try {
                // log balance list deletion
                LogManager.Instance.BalanceListChange(balanceList, false);

                // remove balance list from database
                RemoveBalanceListFromDb(balanceList);

                // remove assigned items from presentation trees
                foreach (var item in balanceList.AssignedItems) item.RemoveFromParents();

                // remove balance list from document
                balanceList.Document.RemoveBalanceList(balanceList);
            
            } catch (Exception ex) {
                throw new Exception("Failed to remove balance list: " + ex.Message, ex);
            }
        }
        #endregion RemoveBalanceList

        #region RemoveBalanceListFromDb
        /// <summary>
        /// Removes the specified balance list from database.
        /// </summary>
        internal static void RemoveBalanceListFromDb(IBalanceList balanceList) {
            using (var conn = AppConfig.ConnectionManager.GetConnection()) {
                conn.BeginTransaction();
                try {
                    // delete depending tables (assertion: existing column balance_list_id)
                    foreach (var ttable in new List<Type> { 
                        typeof(Account), 
                        typeof(VirtualAccount), 
                        typeof(SplittedAccount), 
                        typeof(SplitAccountGroup), 
                        typeof(AccountGroup) }) {

                        conn.Delete(conn.DbMapping.GetTableName(ttable), conn.Enquote("balance_list_id") + "=" + balanceList.Id);
                    }

                    conn.DbMapping.Delete(balanceList);
                    conn.CommitTransaction();

                } catch (Exception ex) {
                    conn.RollbackTransaction();
                    throw new Exception("Could not delete database entries: " + ex.Message, ex);
                }
            }
        }
        #endregion RemoveBalanceListFromDb

        #region [ Account reference list ]

        public AccountReferenceList ReferenceList { get; private set; }

        #region [ IAccountReferenceListManipulator members ]

        public void AddItemToReferenceList(IAccountReferenceListItem item) {
            ReferenceList.AddItemToReferenceList(item);
        }

        public void RemoveItemFromReferenceList(IAccountReferenceListItem item) {
            ReferenceList.RemoveItemFromReferenceList(item);
        }

        public bool IsAccountContainedInReferenceList(AccountTypeEnum accountType, long accounttId) {
            return ReferenceList.IsAccountContainedInReferenceList(accountType, accounttId);
        }

        public bool IsAccountContainedInReferenceList(IAccountReferenceListItem item) {
            return ReferenceList.IsAccountContainedInReferenceList(item);
        }
        
        #endregion [ IAccountReferenceListManipulator members ]

        #endregion [ Account reference list ]
    }
}
