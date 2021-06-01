// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-11-20
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using Taxonomy;
using Utils;

namespace eBalanceKitBusiness.Structures {
    public class UpgradeMissingValue : NotifyPropertyChangedBase {

        internal UpgradeMissingValue(IElement element, string value) {
            Element = element;
            Value = value;
        }

        public IElement Element { get; set; }
        public string Value { get; set; }

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