// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-09-01
// (C)opyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Linq;
using DbAccess;
using Taxonomy;
using Taxonomy.PresentationTree;
using eBalanceKitBase.Structures;
using eBalanceKitBusiness.Interfaces.PresentationTree;

namespace eBalanceKitBusiness.Structures.DbMapping.BalanceList {
    internal abstract class BalanceListEntryBase : PresentationTreeEntry, IBalanceListEntry {
        #region properties

        #region Id
        [DbColumn("id", AllowDbNull = false)]
        [DbPrimaryKey]
        internal long Id { get; set; }
        #endregion

        #region BalanceList
        //public BalanceList BalanceList { get; set; }

        #region BalanceList
        private BalanceList _balanceList;

        [DbColumn("balance_list_id", AllowDbNull = false, IsInverseMapping = true)]
        public virtual BalanceList BalanceList {
            get { return _balanceList; }
            set {
                if (_balanceList != value) {
                    _balanceList = value;
                    OnPropertyChanged("BalanceList");
                }
            }
        }
        #endregion BalanceList
        #endregion

        #region Number
        private string _number;

        [DbColumn("number", AllowDbNull = false, Length = 32)]
        public virtual string Number {
            get { return _number; }
            set {
                _number = value;
                if (_number.Length > 32) _number = _number.Substring(0, 32);
                if (DoDbUpdate) Save();
                OnPropertyChanged("Number");
                OnPropertyChanged("Label");
            }
        }
        #endregion

        #region Name
        private string _name = string.Empty;

        [DbColumn("name", AllowDbNull = false, Length = 128)]
        public virtual string Name {
            get { return _name; }
            set {
                _name = value;
                if (_name.Length > 128) _name = _name.Substring(0, 128);
                if (DoDbUpdate) Save();
                OnPropertyChanged("Name");
                OnPropertyChanged("Label");
            }
        }
        #endregion

        #region AssignedElement
        private IElement _assignedElement;

        [DbColumn("assigned_element_id", AllowDbNull = true)]
        public int AssignedElementId { get; set; } // value initialized from TaxonomyIdManager

        public IElement AssignedElement {
            get { return _assignedElement; }
            set {
                _assignedElement = value;
                OnPropertyChanged("AssignedElement");
            }
        }
        #endregion AssignedElement

        #region Comment
        private string _comment;

        public bool HasComment { get { return !string.IsNullOrEmpty(Comment); } }

        [DbColumn("comment", AllowDbNull = true, Length = 1000)]
        public string Comment {
            get { return _comment; }
            set {
                _comment = value;
                if (DoDbUpdate) Save();
                OnPropertyChanged("Comment");
                OnPropertyChanged("HasComment");
            }
        }
        #endregion Comment

        #region IsHidden
        public bool _isHidden;

        [DbColumn("hidden", AllowDbNull = false)]
        public bool IsHidden {
            get { return _isHidden; }
            set {
                if (_isHidden != value) {
                    _isHidden = value;
                    OnPropertyChanged("IsHidden");
                    Save();

                    if (BalanceList != null) BalanceList.OnPropertyChanged("HiddenUnassignedItemsCount");
                }
            }
        }
        #endregion

        #region SortIndex
        private string _sortIndex = string.Empty;

        [DbColumn("sort_index", AllowDbNull = false, Length = 64)]
        public string SortIndex {
            get { return _sortIndex; }
            set {
                _sortIndex = value;
                if (_sortIndex.Length > 64) _sortIndex = _sortIndex.Substring(0, 64);
                OnPropertyChanged("SortIndex");
            }
        }
        #endregion

        #region IsInTray
        [DbColumn("in_tray", AllowDbNull = false)]
        public bool IsInTray { get; set; }
        #endregion

        #region SendBalance
        public bool SendBalance { get { return Parents.Any(parent => ((IPresentationTreeNode) parent).Value.SendAccountBalances); } }
        #endregion SendBalance

        #region DoDbUpdate
        public bool DoDbUpdate { get; set; }
        #endregion DoDbUpdate

        #region IsSelected
        private bool _isSelected;

        public bool IsSelected {
            get { return _isSelected; }
            set {
                if (_isSelected == value) return;
                _isSelected = value;
                OnPropertyChanged("IsSelected");
            }
        }
        #endregion IsSelected

        #region IsChecked
        private bool _isChecked;

        public bool IsChecked {
            get { return _isChecked; }
            set {
                if (_isChecked == value) return;
                _isChecked = value;
                OnPropertyChanged("IsChecked");

                BalanceList.UpdateAllAssignedItemsChecked();
            }
        }
        #endregion IsChecked

        IBalanceList IBalanceListEntry.BalanceList { get { return BalanceList; } }
        public abstract decimal Amount { get; set; }
        public virtual string ValueDisplayString { get { return LocalisationUtils.CurrencyToString(Amount); } }
        public string Label { get { return (Number != null ? Number + " - " : "") + Name; } }
        public bool HasAssignment { get { return AssignedElement != null; } }

        #region IsDocumentSelectedBalanceList
        private bool _isDocumentSelectedBalanceList;

        public bool IsDocumentSelectedBalanceList {
            get { return _isDocumentSelectedBalanceList; }
            set {
                _isDocumentSelectedBalanceList = value;
                OnPropertyChanged("IsDocumentSelectedBalanceList");
            }
        }
        #endregion IsBalanceListSelected

        #region dummy property to avoid data binding exceptions when binding to items in presentation tree
        public bool IsExpanded {
            get { return false; }
            set {
                /* do nothing */
            }
        }
        // binded to TreeViewItem
        #endregion

        #region IsVisible
        private bool _isVisible = true;

        public bool IsVisible {
            get { return _isVisible; }
            set {
                _isVisible = value;
                OnPropertyChanged("IsVisible");
            }
        }
        #endregion IsVisible
        
        #endregion properties

        #region RemoveFromParents
        public override void RemoveFromParents() {
            base.RemoveFromParents();
            IsSelected = false;
        }
        #endregion
        
        #region Save
        public void Save() {
            if (!DoDbUpdate) return;
            using (IDatabase conn = AppConfig.ConnectionManager.GetConnection()) {
                conn.BeginTransaction();
                try {
                    conn.DbMapping.Save(this);
                    conn.CommitTransaction();
                } catch (Exception ex) {
                    conn.RollbackTransaction();
                    throw new Exception(ex.Message, ex);
                }
            }
        }
        #endregion
        
        #region CompareTo
        public int CompareTo(object obj) {
            if (!(obj is BalanceListEntryBase)) return 0;
            string n1 = Number;
            string n2 = ((BalanceListEntryBase) obj).Number;
            int result = n1.CompareTo(n2);
            return result;
        }
        #endregion

        #region GetVisibility
        
        /// <param name="showHiddenEntriesAnyway">For example assigned accounts should be shown even if they were invisble before.</param>
        public bool GetVisibility(BalanceListFilterOptions filterOptions,
            bool showHiddenEntriesAnyway = false) {
                if (!showHiddenEntriesAnyway) {
                    // do not show hidden items visible, if the respective balance list option is disabled
                    if (!filterOptions.ShowHiddenItems && IsHidden) {
                        return false;
                    }
                }
            if (filterOptions.FilterType == BalanceListFilterType.Value && string.IsNullOrEmpty(filterOptions.Filter)) {
                return true;
            }

            if (filterOptions.FilterType == BalanceListFilterType.Value) {
                // ignore leading zeros
                string f = filterOptions.Filter.ToLower();
                if (!filterOptions.ExactSearch) while (f.StartsWith("0")) f = f.Remove(0, 1);

                string number = Number.ToLower();
                if (!filterOptions.ExactSearch) while (number.StartsWith("0")) number = number.Remove(0, 1);

                if (filterOptions.ExactSearch) {
                    if ((filterOptions.SearchAccountNumbers && number.StartsWith(f)) ||
                        (filterOptions.SearchAccountNames && Name.ToLower().StartsWith(f))) {
                        return true;
                    }
                } else {
                    if ((filterOptions.SearchAccountNumbers && number.Contains(f)) ||
                        (filterOptions.SearchAccountNames && Name.ToLower().Contains(f))) {
                        return true;
                    }
                }
            } else {
                // ignore leading zeros
                string f1 = filterOptions.FilterAreaFrom != null ? filterOptions.FilterAreaFrom.ToLower() : null;
                string f2 = filterOptions.FilterAreaTo != null ? filterOptions.FilterAreaTo.ToLower() : null;
                string number = Number.ToLower();

                //Int64 nf1, nf2, nNumber;
                //if (Int64.TryParse(f1, out nf1) && Int64.TryParse(f2, out nf2) && Int64.TryParse(number, out nNumber)) {
                //    return nNumber >= nf1 && nNumber <= nf2;
                //}

                return (f1 == null || number.CompareTo(f1) >= 0 || number.StartsWith(f1)) &&
                       (f2 == null || number.CompareTo(f2) <= 0 || number.StartsWith(f2));
            }

            return false;
        }
        #endregion GetVisibility
    }
}