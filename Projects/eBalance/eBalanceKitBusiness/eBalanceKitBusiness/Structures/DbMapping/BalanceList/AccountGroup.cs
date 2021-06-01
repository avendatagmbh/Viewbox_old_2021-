using System;
using DbAccess;
using eBalanceKitBusiness.Interfaces;
using Utils;
using System.Collections.Generic;

namespace eBalanceKitBusiness.Structures.DbMapping.BalanceList {
 
    /// <summary>
    ///  (non peristent part)
    /// </summary>
    /// <author>Mirko Dibbert</author>
    /// <since>2011-06-13</since>
    internal partial class AccountGroup : BalanceListEntryBase, IAccountGroup {

        #region Amount
        public override decimal Amount {
            get { 
                decimal amount = 0;
                foreach (var item in Items) amount += item.Amount;
                return amount;
            }
            set { }
        }
        #endregion

        #region Items
        public IEnumerable<IAccount> Items { get { return _items; } }
        public int ItemsCount { get { return _items.Count; } }
        private ObservableCollectionAsync<IAccount> _items = new ObservableCollectionAsync<IAccount>();
        #endregion



        public void AddAccount(IAccount account) { 
            _items.Add(account);
            BalanceList.RemoveItem(account);
            account.AccountGroup = this;
            account.Save();
            
            OnPropertyChanged("Amount");
            OnPropertyChanged("ValueDisplayString");
            OnPropertyChanged("ItemsCount");

            BalanceList.UpdateItemsFilter();
            BalanceList.UpdateAccountsFilter();
        }



        public void RemoveAccount(IAccount account) {
            _items.Remove(account);
            BalanceList.AddItem(account);
            account.IsSelected = false;
            account.AccountGroup = null;
            account.Save();

            OnPropertyChanged("Amount");
            OnPropertyChanged("ValueDisplayString");
            OnPropertyChanged("ItemsCount");

            BalanceList.UpdateItemsFilter();
            BalanceList.UpdateAccountsFilter();
        }
        
        public void ClearItems() {
            foreach (var item in _items) {
                item.AccountGroup = null;
                item.Save();
            }
            _items.Clear();
        }

        #region number validation
        public bool IsNumberValid { get; private set; }
        public string NumberValidataionErrorMessage { get; private set; }

        private void ValidateNumber() {

            if (BalanceList != null) {
                if (string.IsNullOrEmpty(Number)) {
                    IsNumberValid = false;
                    NumberValidataionErrorMessage = "Es wurde keine Kontonummer angegeben.";

                } else if (BalanceList.IsNumberDefined(Number, this)) {
                    IsNumberValid = false;
                    NumberValidataionErrorMessage = "Die angegebene Kontonummer ist bereits vergeben.";

                } else {
                    IsNumberValid = true;
                    NumberValidataionErrorMessage = string.Empty;
                }
            }

            OnPropertyChanged("IsNumberValid");
            OnPropertyChanged("NumberValidataionErrorMessage");
            OnPropertyChanged("IsValid");
        }
        #endregion number validation

        #region name validation
        public bool IsNameValid { get; set; }
        public string NameValidataionErrorMessage { get; private set; }
        
        private void ValidateName() {
            if (string.IsNullOrEmpty(Name)) {
                IsNameValid = false;
                NameValidataionErrorMessage = "Kontoname wurde nicht angegeben.";
            } else {
                IsNameValid = true;
                NameValidataionErrorMessage = string.Empty;
            }

            OnPropertyChanged("IsNameValid");
            OnPropertyChanged("NameValidataionErrorMessage");
            OnPropertyChanged("IsValid");
        }
        #endregion name validation

        public void Validate() {
            ValidateName();
            ValidateNumber();
        }

        public bool IsValid { get { return IsNumberValid && IsNameValid; } }

        public IAccountGroup Clone() {
            AccountGroup clone = new AccountGroup {
                BalanceList = this.BalanceList,
                Id = this.Id,
                Number = this.Number,
                Name = this.Name,
                Comment = this.Comment
            };

            return clone;
        }

        public void Update(IAccountGroup group) {
            this.Number = group.Number;
            this.Name = group.Name;
            this.Comment = group.Comment;
        }

        public bool IsAssignedToReferenceList { get; set; }
    }
}
