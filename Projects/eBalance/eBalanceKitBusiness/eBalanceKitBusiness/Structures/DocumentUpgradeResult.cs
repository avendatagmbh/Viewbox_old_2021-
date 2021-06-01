// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-11-20
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Collections.Generic;
using Utils;
using eBalanceKitBusiness.Structures.DbMapping;

namespace eBalanceKitBusiness.Structures {
    public class DocumentUpgradeResult : NotifyPropertyChangedBase {
        internal DocumentUpgradeResult() { }
       
        #region DeletedValues
        private readonly List<UpgradeMissingValue> _deletedValues = new List<UpgradeMissingValue>();
        
        /// <summary>
        /// List of all used values, which has been deleted in the new taxonomy.
        /// </summary>
        public IEnumerable<UpgradeMissingValue> DeletedValues { get { return _deletedValues; } }
        #endregion

        public bool HasDeletedValues { get { return _deletedValues.Count > 0; } }

        internal void AddDeletedValue(UpgradeMissingValue value) { _deletedValues.Add(value); }

        /// <summary>
        /// List of all values, which has been deleted in the new taxonomy.
        /// </summary>
        internal readonly List<IValueMapping> MissingValues = new List<IValueMapping>();
        
        /// <summary>
        /// List of all values with a changed element id (e.g. due to spelling corrections).
        /// </summary>
        internal readonly List<IValueMapping> UpdatedValues = new List<IValueMapping>();

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