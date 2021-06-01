// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-11-19
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Collections.Generic;
using Utils;

namespace eBalanceKitBusiness.MappingTemplate {
    public class AssignmentConflictListEntry : NotifyPropertyChangedBase{
        
        internal AssignmentConflictListEntry(IBalanceList balanceList) {
            BalanceList = balanceList;
            Conflicts = new List<AssignmentConflict>();
        }

        public IBalanceList BalanceList { get; private set; }
        public List<AssignmentConflict> Conflicts { get; private set; }

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