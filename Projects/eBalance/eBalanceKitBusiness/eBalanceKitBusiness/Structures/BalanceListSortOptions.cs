// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-09-22
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using Utils;

namespace eBalanceKitBusiness.Structures {
    
    public enum BalanceListSortType {
        AccountNumber,
        AccountName,
        Index,
        Original
    }
    
    public class BalanceListSortOptions : NotifyPropertyChangedBase {
        internal BalanceListSortOptions(IBalanceList balanceList) {
            BalanceList = balanceList;
        }

        #region BalanceList
        internal IBalanceList BalanceList { get; set; }
        #endregion

        #region SortType
        private BalanceListSortType _sortType;

        public BalanceListSortType SortType {
            get { return _sortType; }
            set {
                _sortType = value;
                OnPropertyChanged("SortType");

                BalanceList.UpdateFilter();
            }
        }
        #endregion

        #region UseNumericSortIfPossible
        private bool _useNumericSortIfPossible = true;

        public bool UseNumericSortIfPossible {
            get { return _useNumericSortIfPossible; }
            set {
                _useNumericSortIfPossible = value;
                OnPropertyChanged("UseNumericSortIfPossible");

                BalanceList.UpdateFilter();
            }
        }
        #endregion
    }
}
