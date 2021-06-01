// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-08-15
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using Utils;

namespace eBalanceKitBusiness.Structures {

    public enum BalanceListFilterType { Value, Area }

    public class BalanceListFilterOptions : NotifyPropertyChangedBase {
        internal BalanceListFilterOptions(IBalanceList balanceList) { BalanceList = balanceList; }

        #region FilterType
        private BalanceListFilterType _filterType;

        public BalanceListFilterType FilterType {
            get { return _filterType; }
            set {
                _filterType = value;
                OnPropertyChanged("FilterType");

                BalanceList.UpdateFilter();
            }
        }
        #endregion

        #region BalanceList
        internal IBalanceList BalanceList { get; set; }
        #endregion

        #region Filter
        private string _filter;

        public string Filter {
            get { return _filter; }
            set {
                if (_filter != value) {
                    _filter = value;
                    BalanceList.UpdateFilter();
                }
            }
        }
        #endregion

        #region FilterAreaFrom
        private string _filterAreaFrom;

        public string FilterAreaFrom {
            get { return _filterAreaFrom; }
            set {
                if (_filterAreaFrom != value) {
                    _filterAreaFrom = value;
                    BalanceList.UpdateFilter();
                }
            }
        }
        #endregion

        #region FilterAreaTo
        private string _filterAreaTo;

        public string FilterAreaTo {
            get { return _filterAreaTo; }
            set {
                if (_filterAreaTo != value) {
                    _filterAreaTo = value;
                    BalanceList.UpdateFilter();
                }
            }
        }
        #endregion

        #region ShowHiddenItems
        private bool _showHiddenItems;

        public bool ShowHiddenItems {
            get { return _showHiddenItems; }
            set {
                if (_showHiddenItems != value) {
                    _showHiddenItems = value;
                    BalanceList.UpdateFilter();
                }
            }
        }
        #endregion

        #region ExactSearch
        private bool _exactSearch;

        public bool ExactSearch {
            get { return _exactSearch; }
            set {
                if (_exactSearch != value) {
                    _exactSearch = value;
                    BalanceList.UpdateFilter();
                }
            }
        }
        #endregion

        #region SearchAccountNumbers
        private bool _searchAccountNumbers = true;

        public bool SearchAccountNumbers {
            get { return _searchAccountNumbers; }
            set {
                if (_searchAccountNumbers != value) {
                    if (value || SearchAccountNames) {
                        _searchAccountNumbers = value;
                        BalanceList.UpdateFilter();
                    }
                }
            }
        }
        #endregion

        #region SearchAccountNames
        private bool _searchAccountNames = true;

        public bool SearchAccountNames {
            get { return _searchAccountNames; }
            set {
                if (_searchAccountNames != value) {
                    if (value || SearchAccountNumbers) {
                        _searchAccountNames = value;
                        BalanceList.UpdateFilter();
                    }
                }
            }
        }

        #endregion
    }
}