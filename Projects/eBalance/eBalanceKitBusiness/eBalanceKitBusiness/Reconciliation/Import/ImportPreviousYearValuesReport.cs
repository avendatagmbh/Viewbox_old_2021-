// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-04-24
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using Utils;
using eBalanceKitBusiness.Structures.DbMapping;

namespace eBalanceKitBusiness.Reconciliation.Import {
    public class ImportPreviousYearValuesReport : NotifyPropertyChangedBase {
        public ImportPreviousYearValuesReport(Document document, PreviousYearValues previousYearValues) {
            PreviousYearValues = previousYearValues;
            Document = document;
        }

        public PreviousYearValues PreviousYearValues { get; private set; }
        public Document Document { get; private set; }

        #region IsSelected
        private bool _isSelected;

        public bool IsSelected {
            get { return _isSelected; }
            set {
                if (_isSelected == value) return;
                _isSelected = value;
                OnPropertyChanged("IsSelected");
            }
        }
        #endregion // IsSelected   }
    }
}