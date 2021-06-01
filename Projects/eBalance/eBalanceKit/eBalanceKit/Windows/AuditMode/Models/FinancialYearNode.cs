// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-05-23
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Collections.Generic;
using System.Linq;
using eBalanceKitBusiness.Structures.DbMapping;

namespace eBalanceKit.Windows.AuditMode.Models {
    public sealed class FinancialYearNode : SelectNextYearNodeBase {
        public FinancialYearNode(FinancialYear financialYear) {
            Children = new List<DocumentNode>();
            FinancialYear = financialYear;
            IsChecked = false;
        }

        public FinancialYear FinancialYear { get; private set; }

        public List<DocumentNode> Children { get; private set; }

        #region IsChecked
        private bool? _isChecked;

        public bool? IsChecked {
            get { return _isChecked; }
            set {
                if (_isChecked == value) return;
                _isChecked = value;
                OnPropertyChanged("IsChecked");

                if (!_isChecked.HasValue) return;
                bool val = _isChecked.Value;
                foreach (var child in Children)
                    child.IsChecked = val;
            }
        }
        #endregion // IsChecked

        public void UpdateIsChecked() {
            if (Children.All(c => c.IsChecked)) _isChecked = true;
            else if (Children.All(c => !c.IsChecked)) _isChecked = false;
            else _isChecked = null;
            OnPropertyChanged("IsChecked");
        }
    }
}