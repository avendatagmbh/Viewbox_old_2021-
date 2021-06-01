// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-11-17
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------

using Taxonomy;
using Utils;

namespace eBalanceKitBusiness.MappingTemplate {
    public class AssignmentError : NotifyPropertyChangedBase {

        internal AssignmentError(IElement element, string accountLabel, string message) {
            Element = element;
            AccountLabel = accountLabel;
            Message = message;
        }

        public IElement Element { get; private set; }
        public string AccountLabel { get; set; }
        public string Message { get; private set; }

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