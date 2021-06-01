// --------------------------------------------------------------------------------
// author: Márton Garai
// since: 2012-09-03
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using Utils;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using eBalanceKitBusiness.Interfaces.BalanceList;
using eBalanceKitBusiness.Structures.DbMapping.BalanceList;
using eBalanceKitBusiness.Structures.DbMapping.MappingTemplate;

namespace eBalanceKitBusiness.MappingTemplate
{
    public class AccountSplittingModel : NotifyPropertyChangedBase {

        public AccountSplittingModel(ApplyTemplateModel model) {
            InitSplittingModel(model);
        }

        /// <summary>
        /// The core of the applying the templates is happening here.
        /// </summary>
        public void InitSplittingModel(ApplyTemplateModel model) {
            foreach (MappingTemplateSplitAccount mappingTemplateSplitAccount in model.Template.SplitAccounts) {
                mappingTemplateSplitAccount.LoadAccountGroup();
            }
            BalanceListSplitList = new List<BalanceListSplitListInfo>();
            foreach (BalanceList selectedBalanceList in model.SelectedBalanceLists) {
                ObservableCollection<AccountAndSplitAccountGroupInfoPair> addedItem =
                    new ObservableCollection<AccountAndSplitAccountGroupInfoPair>();
                foreach (MappingTemplateSplitAccount mappingTemplateSplitAccount in model.Template.SplitAccounts) {
                    foreach (IAccount account in selectedBalanceList.Accounts) {
                        // we only add to the possible split accounts if there is the account number in the selected balance list.
                        if (account.Number == mappingTemplateSplitAccount.SplitAccountGroupInfo.AccountNumber &&
                            account.BalanceList == selectedBalanceList) {
                            // we can add the elements each after, the two column grid will take care of the table lookout.
                            account.PossibleSplitAccount = mappingTemplateSplitAccount.SplitAccountGroupInfo;
                            account.PossibleGridList = addedItem;
                            account.Document = selectedBalanceList.Document;
                            addedItem.Add(new AccountAndSplitAccountGroupInfoPair {Account = account, OldAccount = mappingTemplateSplitAccount.SplitAccountGroupInfo});
                        }
                    }
                }
                BalanceListSplitList.Add(new BalanceListSplitListInfo {
                    AccountGroupByBalanceList = addedItem,
                    HeaderOfBalanceListList = selectedBalanceList.Name
                });
            }
        }

        // used in TaxonomyAndBalanceListBase.xaml
        public class AccountAndSplitAccountGroupInfoPair : NotifyPropertyChangedBase  {
            public IAccount Account { get; set; }

            public SplitAccountGroupInfo OldAccount { get; set; }

            #region IsSelected
            private bool _isSelected;

            public bool IsSelected {
                get { return _isSelected; }
                set {
                    if (_isSelected != value) {
                        _isSelected = value;
                        OnPropertyChanged("IsSelected");
                    }
                }
            }
            #endregion IsSelected
        }

        /// <summary>
        /// struct for wpf to the visualisation in the datatemplate. Each selected balance list have a struct like this.
        /// </summary>
        public struct BalanceListSplitListInfo {
            public ObservableCollection<AccountAndSplitAccountGroupInfoPair> AccountGroupByBalanceList { get; set; }

            public string HeaderOfBalanceListList { get; set; }
        }

        public List<BalanceListSplitListInfo> BalanceListSplitList { get; set; }

        public bool HasSplitAccounts {
            get {
                foreach (BalanceListSplitListInfo balanceListListInfo in BalanceListSplitList) {
                    if (balanceListListInfo.AccountGroupByBalanceList.Count > 0) {
                        return true;
                    }
                }
                return false;
            }
        }
    }
}
