// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-11-20
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Collections.Generic;
using Utils;
using eBalanceKitBusiness.Structures.DbMapping;
using eBalanceKitBusiness.Structures.DbMapping.ValueMappings;

namespace eBalanceKitBusiness.Structures {
    public class CompanyUpgradeResult : NotifyPropertyChangedBase {
        public CompanyUpgradeResult(Company company) {
            Company = company;
        }
        
        public Company Company { get; private set; }
        

        #region DeletedValues
        private readonly List<UpgradeMissingValue> _deletedValues = new List<UpgradeMissingValue>();
        public IEnumerable<UpgradeMissingValue> DeletedValues { get { return _deletedValues; } }
        #endregion

        public bool HasDeletedValues { get { return _deletedValues.Count > 0; } }

        internal void AddDeletedValue(UpgradeMissingValue value) { _deletedValues.Add(value); }

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