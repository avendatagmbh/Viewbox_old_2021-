using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using DbAccess;
using Utils;
using System.ComponentModel;
using eBalanceKitBase.Structures;
using eBalanceKitBusiness.Manager;

namespace eBalanceKitBusiness.Structures.DbMapping.BalanceList {

    public enum ValueInputMode { Absolute, Relative };

    /// <summary>
    /// 
    /// </summary>
    /// <author>Mirko Dibbert</author>
    /// <since>2011-06-13</since>
    internal partial class SplitAccountGroup : ISplitAccountGroup, INotifyPropertyChanged {

        public SplitAccountGroup() {
        }

        #region events
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string property) { if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(property)); }
        #endregion events

        IBalanceList ISplitAccountGroup.BalanceList { get { return BalanceList; } }

        #region Items
        public IEnumerable<ISplittedAccount> Items { get { return _items; } }
        private ObservableCollectionAsync<ISplittedAccount> _items = new ObservableCollectionAsync<ISplittedAccount>();
        #endregion

        #region Account
        private IAccount _account;
        public IAccount Account {
            get { return _account; }
            set {
                _account = value;
                AccountId = ((BalanceListEntryBase)_account).Id;
            }
        }
        #endregion
        
        #region AmountSum
        private decimal _amountSum;
        public decimal AmountSum { get { return _amountSum; } }
        public string AmountSumDisplayString { get { return (_items.Count > 0 ? LocalisationUtils.CurrencyToString(_amountSum) : "-"); } }

        internal void RecomputateAmountSum() {
            _amountSum = 0;
            foreach (var item in Items) {
                _amountSum += item.Amount;
            }

            OnPropertyChanged("AmountSum");
            OnPropertyChanged("AmountSumDisplayString");

            Validate();
        }

        #endregion AmountSum

        #region PercentSum
        private decimal? _percentSum;
        public decimal? PercentSum { get { return _percentSum; } }
        public string PercentSumDisplayString { get { return (_percentSum.HasValue ? _percentSum + " %" : "-"); } }

        internal void RecomputatePercentSum() {
            _percentSum = null;
            foreach (var item in Items) {
                if (item.AmountPercent.HasValue) {
                    if (_percentSum.HasValue) _percentSum += item.AmountPercent;
                    else _percentSum = item.AmountPercent;
                }
            }

            OnPropertyChanged("PercentSum");
            OnPropertyChanged("PercentSumDisplayString");

            Validate();
        }
        #endregion PercentSum

        #region IsVisible
        private bool _isVisible;
        public bool IsVisible {
            get { return _isVisible; }
            set {
                _isVisible = value;
                OnPropertyChanged("IsVisible");
            }
        }
        #endregion IsVisible

        /// <summary>
        /// Creates a new splitted account and adds it to the items collection.
        /// </summary>
        /// <returns></returns>
        public ISplittedAccount AddNewItem() {
            var sa = new SplittedAccount { 
                Number = Account.Number + (_items.Count + 1),
                Name = Account.Name  + " " + (_items.Count + 1) 
            };
            Add(sa);
            sa.Validate();
            return sa;
        }

        /// <summary>
        /// Adds the specified splitted account to the items collections and sets some of 
        /// it's related properties of (e.g. BaseAccount, SplitAccountGroup or BalanceList).
        /// </summary>
        /// <param name="splittedAccount"></param>
        public void Add(ISplittedAccount splittedAccount) {
            var sa = splittedAccount as SplittedAccount;
            sa.BaseAccount = Account;
            sa.SplitAccountGroup = this;
            sa.BalanceList = this.BalanceList;
            _items.Add(sa);
            sa.PropertyChanged += new PropertyChangedEventHandler(item_PropertyChanged);
            Validate();
        }

        void item_PropertyChanged(object sender, PropertyChangedEventArgs e) { Validate(); }

        /// <summary>
        /// Removes the specifed splitted account.
        /// </summary>
        /// <param name="sa"></param>
        public void Remove(ISplittedAccount sa) {
            if (sa.DoDbUpdate) {
                using (var conn = AppConfig.ConnectionManager.GetConnection()) {
                    try {
                        conn.BeginTransaction();
                        conn.DbMapping.Delete(sa);
                        conn.CommitTransaction();
                    } catch (Exception ex) {
                        conn.RollbackTransaction();
                        throw new Exception(ex.Message, ex);
                    }
                }
            }

            sa.PropertyChanged -= new PropertyChangedEventHandler(item_PropertyChanged);
            _items.Remove(sa);
            Validate();
        }
        
        /// <summary>
        /// Removes all items from items collection and from database.
        /// </summary>
        internal void RemoveAllItems(IDatabase conn) {
            conn.Delete("splitted_accounts", conn.Enquote("split_group_id") + "=" + this.Id);
            foreach (var item in _items) item.PropertyChanged -= new PropertyChangedEventHandler(item_PropertyChanged);
            _items.Clear(); 
        }

        public void Save() {
            using (var conn = AppConfig.ConnectionManager.GetConnection()) {
                conn.BeginTransaction();
                try {
                    conn.DbMapping.Save(this);
                    foreach (var value in Items) conn.DbMapping.Save(value);
                    conn.CommitTransaction();
                } catch (Exception ex) {
                    conn.RollbackTransaction();
                    throw new Exception(ex.Message, ex);
                }
            }
        }

        public ISplitAccountGroup Clone() {
            SplitAccountGroup sag = new SplitAccountGroup {
                Id = this.Id,
                Account = this.Account,
                BalanceList = this.BalanceList,
                ValueInputMode = this.ValueInputMode,
            };

            foreach (var item in Items) {
                sag.Add(new SplittedAccount {
                    Id = ((BalanceListEntryBase)item).Id,
                    Number = item.Number,
                    Name = item.Name,
                    AmountPercent = item.AmountPercent,
                    Amount = item.Amount,
                });
            }

            sag.RecomputateAmountSum();
            sag.RecomputatePercentSum();
            sag.Validate();
            return sag;
        }

        public void Update(ISplitAccountGroup sag) {

            this.ValueInputMode = sag.ValueInputMode;

            Dictionary<long, ISplittedAccount> itemDict = new Dictionary<long, ISplittedAccount>();
            Dictionary<long, ISplittedAccount> sagItemDict = new Dictionary<long, ISplittedAccount>();
            List<ISplittedAccount> newItems = new List<ISplittedAccount>();

            foreach (var item in Items) itemDict.Add(((BalanceListEntryBase)item).Id, item);
            foreach (var item in sag.Items) {
                if (((BalanceListEntryBase)item).Id != 0) sagItemDict.Add(((BalanceListEntryBase)item).Id, item);
                else newItems.Add(item);
            }

            List<ISplittedAccount> removedItems = new List<ISplittedAccount>();
            foreach (var item in Items) {
                var id = ((BalanceListEntryBase) item).Id;
                if (sagItemDict.ContainsKey(id)) {
                    item.DoDbUpdate = false;
                    
                    // copy
                    item.Number = sagItemDict[id].Number;
                    item.Name = sagItemDict[id].Name;
                    item.Comment = sagItemDict[id].Comment;
                    item.AmountPercent = sagItemDict[id].AmountPercent;
                    item.Amount = sagItemDict[id].Amount;

                    item.DoDbUpdate = true;
                } else {
                    // deleted item
                    removedItems.Add(item);
                }
            }

            // delete removed items
            foreach (var item in removedItems) {
                    Remove(item);
                    BalanceList.RemoveItem(item);
            }

            // add new items
            foreach (var item in newItems) {
                BalanceList.AddItem(item);
                Add(item);
            }

            BalanceList.UpdateItemsFilter();
            BalanceList.UpdateAccountsFilter();
        }

        #region IsAmountSumValid
        private bool IsAmountSumValid {
            get {
                if (_items.Count == 0) return true;
                return AmountSum == Account.Amount; 
            }
        }
        #endregion

        #region IsPercentSumValid
        private bool IsPercentSumValid {
            get {
                if (_items.Count == 0) return true;
                if (ValueInputMode != DbMapping.BalanceList.ValueInputMode.Relative) return true;
                return PercentSum == 100;
            }
        }
        #endregion

        #region ValidationErrorMessages
        private ObservableCollectionAsync<string> _validationErrorMessages = new ObservableCollectionAsync<string>();
        public IEnumerable<string> ValidationErrorMessages { get { return _validationErrorMessages; } }
        #endregion

        #region IsValid
        private bool _isValid;
        public bool IsValid { 
            get { return _isValid; }
            private set { 
                _isValid = value;
                OnPropertyChanged("IsValid");
            }
        }
        #endregion

        #region Validate
        private void Validate() {
            _validationErrorMessages.Clear();
            IsValid = true;

            if (_items.Count == 0) {
                IsValid = false;
                _validationErrorMessages.Add("Es wurde noch kein Teilkonto angelegt, es müssen mindestens zwei Teilkonten angelegt werden.");

            } else if (_items.Count == 1) {
                IsValid = false;
                _validationErrorMessages.Add("Es wurde nur ein Teilkonto angelegt, es müssen mindestens zwei Teilkonten angelegt werden.");

            } else {
                if (!IsAmountSumValid) {
                    IsValid = false;
                    _validationErrorMessages.Add("Die Summe der Teilkonten stimmt nicht mit dem Saldo des Hauptkontos überein.");
                }

                if (!IsPercentSumValid) {
                    IsValid = false;
                    _validationErrorMessages.Add("Die Summe der Prozentwerte ist ungleich 100%.");
                }
            }

            foreach (var item in Items) {
                if (!item.IsValid) {
                    IsValid = false;
                    //_validationErrorMessages.Add("Mindestens ein Teilkonto enthält fehlerhafte Angaben.");
                    break;
                }
            }

        }
        #endregion Validate

        #region IsSelected
        public bool IsSelected {
            get { return _isSelected; }
            set {
                if (_isSelected != value) {
                    _isSelected = value;
                    OnPropertyChanged("IsSelected");
                }
            }
        }
        private bool _isSelected;
        #endregion

        #region GetVisibility
        public bool GetVisibility(BalanceListFilterOptions filterOptions) {
            return Account.GetVisibility(filterOptions);
        }
        #endregion GetVisibility
    }
}
