// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-11-17
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Collections.Generic;
using Utils;

namespace eBalanceKitBusiness.MappingTemplate {
    public class AssignmentErrorListEntry : NotifyPropertyChangedBase {

        internal AssignmentErrorListEntry(IBalanceList balanceList) {
            BalanceList = balanceList;
            Errors = new List<AssignmentError>();
        }

        public IBalanceList BalanceList { get; private set; }
        public List<AssignmentError> Errors { get; private set; }

        #region IsSelected
        private bool _isSelected;
        public bool IsSelected {
            get { return _isSelected; }
            set {
                _isSelected = value;
                OnPropertyChanged("IsSelected");
            }
        }
        #endregion
    }
}