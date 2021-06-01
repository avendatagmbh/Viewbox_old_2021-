// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-11-15
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using Utils;

namespace eBalanceKitBusiness.MappingTemplate {
    public class MappingTemplateBalanceList : NotifyPropertyChangedBase{

        #region BalanceList
        private IBalanceList _balanceList;

        public IBalanceList BalanceList {
            get { return _balanceList; }
            set {
                _balanceList = value;
                OnPropertyChanged("BalanceLis");
            }
        }
        #endregion BalanceList

        #region IsChecked
        private bool _isChecked;

        public bool IsChecked {
            get { return _isChecked; }
            set {
                _isChecked = value;
                OnPropertyChanged("IsChecked");
            }
        }
        #endregion
    }
}