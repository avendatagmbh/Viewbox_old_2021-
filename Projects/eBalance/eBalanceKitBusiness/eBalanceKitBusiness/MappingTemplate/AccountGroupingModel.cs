// --------------------------------------------------------------------------------
// author: Márton Garai
// since: 2012-08-03
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Utils;
using eBalanceKitBusiness.Algorithm.Interfaces;
using eBalanceKitBusiness.Interfaces.BalanceList;
using eBalanceKitBusiness.Manager;
using eBalanceKitBusiness.Structures.DbMapping.BalanceList;
using eBalanceKitBusiness.Structures.DbMapping.MappingTemplate;
using eBalanceKitResources.Localisation;

namespace eBalanceKitBusiness.MappingTemplate {
    public class AccountGroupingModel : NotifyPropertyChangedBase, IFilterList {

        public AccountGroupingModel(ApplyTemplateModel model) {
            InitGroupingModel(model);
        }

        public BalanceListGroupListInfo SelectedBalanceListInfo { get; set; }

        private IBalanceList _selectedBalanceList;
        public IBalanceList SelectedBalanceList { get { return _selectedBalanceList; } 
            set {
                // if we change the balance list, the accounts in the account group infos will be hidden.
                foreach (IAccount account in value.Accounts) {
                    bool canContinue = false;
                    foreach (MappingTemplateAccountGroup mappingTemplateAccountGroup in _mappingTemplateAccountGroups) {
                        foreach (AccountGroupChildInfo childInfo in mappingTemplateAccountGroup.AccountGroupInfo.ChildrenInfos) {
                            if (childInfo.Number == account.Number) {
                                account.IsVisibleForGroupMapping = false;
                                canContinue = true;
                                break;
                            }
                        }
                        if (canContinue) {
                            break;
                        }
                    }
                }
                _selectedBalanceList = value;
            } 
        }

        /// <summary>
        /// the value of the string in the DlgApplyGroupingTemplate.xaml's TextBlock
        /// </summary>
        public string AccountsInfo {
            get {
                int acc = 0;
                foreach (IAccount account in SelectedBalanceList.Accounts) {
                    if (account.IsVisibleForGroupMapping && !account.HasAssignment) {
                        acc++;
                    }
                }

                if (acc == 0) return ResourcesTemplates.NoAccount;

                if (acc != SelectedBalanceList.AccountsDisplayedForTemplates.Count)
                    return string.Format(ResourcesTemplates.FilteredAccountString,
                                         SelectedBalanceList.AccountsDisplayedForTemplates.Count, acc);

                return string.Format(ResourcesTemplates.AccountAmount, acc);
            }
        }

        /// <summary>
        /// the value of the string in the DlgApplyGroupingTemplate.xaml's TextBlock
        /// </summary>
        public string AccountGroupsInfo {
            get {
                if (SelectedBalanceListInfo.AccountGroupByBalanceList.Count == 0) return ResourcesTemplates.NoAccountGroup;

                //if (_accountGroups.Count != _accountGroupsDisplayed.Count)
                //    return _accountGroupsDisplayed.Count + " von " + _accountGroups.Count + " Kontogruppen (gefiltert)";

                return string.Format(ResourcesTemplates.AccountGroupAmount, SelectedBalanceListInfo.AccountGroupByBalanceList.Count);
            }
        }

        private ObservableCollection<MappingTemplateAccountGroup> _mappingTemplateAccountGroups;

        /// <summary>
        /// The core of the applying the templates is happening here.
        /// </summary>
        public void InitGroupingModel(ApplyTemplateModel model) {
            _mappingTemplateAccountGroups = model.Template.AccountGroups;
            foreach (MappingTemplateAccountGroup mappingTemplateAccountGroup in _mappingTemplateAccountGroups) {
                mappingTemplateAccountGroup.LoadAccountGroup();
            }
            BalanceListGroupList = new ObservableCollection<BalanceListGroupListInfo>();
            foreach (BalanceList selectedBalanceList in model.SelectedBalanceLists) {
                Dictionary<string, IBalanceListEntry> entryDict = new Dictionary<string, IBalanceListEntry>();
                foreach (var item in selectedBalanceList.UnassignedItems) {
                    entryDict[item.Number] = item;
                }

                ObservableCollection<AccountGroupInfo> addedItem =
                    new ObservableCollection<AccountGroupInfo>();
                foreach (MappingTemplateAccountGroup mappingTemplateAccountGroup in _mappingTemplateAccountGroups) {
                    if (entryDict.ContainsKey(mappingTemplateAccountGroup.AccountGroupInfo.AccountNumber)) {
                        continue;
                    }
                    // we builded possible group accounts by balance lists. Now we use it, if it's possible to build an account group in the selected balance list, than we add the option to create the
                    // group account on the GUI.
                    AccountGroupInfo actualAccountGroupInfo = new AccountGroupInfo {
                        GroupComment = mappingTemplateAccountGroup.AccountGroupInfo.GroupComment,
                        AccountNumber = mappingTemplateAccountGroup.AccountGroupInfo.AccountNumber,
                        MaybeBalanceList = selectedBalanceList,
                        AccountName = mappingTemplateAccountGroup.AccountGroupInfo.AccountName
                    };
                    foreach (
                        AccountGroupChildInfo accountGroupChildInfo in
                            mappingTemplateAccountGroup.AccountGroupInfo.ChildrenInfos) {
                        if (entryDict.ContainsKey(accountGroupChildInfo.Number)) {
                            actualAccountGroupInfo.ChildrenInfos.Add(
                                new AccountGroupChildInfo {
                                    Comment = entryDict[accountGroupChildInfo.Number].Comment,
                                    Name = entryDict[accountGroupChildInfo.Number].Name,
                                    Number = entryDict[accountGroupChildInfo.Number].Number, 
                                    Parent = actualAccountGroupInfo
                                });
                        } else {
                            // populate the tool tip for already assigned or missing accounts 
                            actualAccountGroupInfo.MissingChildrenInfos.Add(
                                new AccountGroupChildInfo {
                                    Comment = accountGroupChildInfo.Comment,
                                    Name = accountGroupChildInfo.Name,
                                    Number = accountGroupChildInfo.Number,
                                    Parent = actualAccountGroupInfo
                                });
                        }
                    }
                    if (actualAccountGroupInfo.ChildrenInfos.Count == 0) {
                        continue;
                    }
                    addedItem.Add(actualAccountGroupInfo);
                    actualAccountGroupInfo.ContainingList = addedItem;
                }
                BalanceListGroupList.Add(new BalanceListGroupListInfo {
                    AccountGroupByBalanceList = addedItem,
                    HeaderOfBalanceListList = selectedBalanceList.Name,
                });
            }
            IsAccountGroupApplyClickEnabled = false;
        }

        /// <summary>
        /// struct for wpf to the visualisation in the datatemplate. Each selected balance list have a struct like this.
        /// </summary>
        public class BalanceListGroupListInfo {
            public ObservableCollection<AccountGroupInfo> AccountGroupByBalanceList { get; set; }

            public string HeaderOfBalanceListList { get; set; }
        }

        public ObservableCollection<BalanceListGroupListInfo> BalanceListGroupList { get; set; }

        public bool HasAccountGroups { get {
            foreach (BalanceListGroupListInfo balanceListListInfo in BalanceListGroupList) {
                if (balanceListListInfo.AccountGroupByBalanceList.Count > 0) {
                    return true;
                }
            }
            return false;
        } }

        #region IsAccountGroupApplyClickEnabled
        private bool _isAccountGroupApplyClickEnabled;

        public bool IsAccountGroupApplyClickEnabled {
            get { return _isAccountGroupApplyClickEnabled; }
            set {
                if (_isAccountGroupApplyClickEnabled != value) {
                    _isAccountGroupApplyClickEnabled = value;
                    OnPropertyChanged("IsAccountGroupApplyClickEnabled");
                }
            }
        }
        #endregion IsAccountGroupApplyClickEnabled

        private AccountGroupInfo _selectedGroupInfo;
        public AccountGroupInfo SelectedGroupInfo {
            get { return _selectedGroupInfo; } 
            set {
                if (value != null) {
                    IsAccountGroupApplyClickEnabled = true;
                }
                _selectedGroupInfo = value;    
            }
        }

        public void ApplyGroupings() {
            foreach (AccountGroupInfo accountGroupInfo in SelectedBalanceListInfo.AccountGroupByBalanceList) {
                IAccountGroup group = AccountGroupManager.CreateAccountGroup(accountGroupInfo.MaybeBalanceList);
                group.Number = accountGroupInfo.AccountNumber;
                group.Name = accountGroupInfo.AccountName;
                group.Comment = accountGroupInfo.GroupComment;
                accountGroupInfo.MaybeBalanceList.AddAccountGroup(group);
                // first save needed, so the account can have foreign key to the group's key.
                group.DoDbUpdate = true;
                group.Save();
                foreach (AccountGroupChildInfo accountGroupChildInfo in accountGroupInfo.ChildrenInfos) {
                    foreach (IAccount account in accountGroupInfo.MaybeBalanceList.Accounts) {
                        if (account.Number == accountGroupChildInfo.Number) {
                            // the AddAccount will set the account.AccountGroup to the right key.
                            group.AddAccount(account);
                            break;
                        }
                    }
                }
            }
            SelectedBalanceListInfo.AccountGroupByBalanceList.Clear();
        } 

        public void RefreshInfoLines() {
            OnPropertyChanged("AccountsInfo");
            OnPropertyChanged("AccountGroupsInfo");
        }

        public void UpdateFilter() {
            OnPropertyChanged("AccountsInfo");
        }
    }
}
