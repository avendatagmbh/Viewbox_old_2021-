// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-05-23
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using eBalanceKitBusiness.Rights;
using eBalanceKitBusiness.Structures.DbMapping;

namespace eBalanceKit.Windows.AuditMode.Models {
    public class DocumentNode : SelectNextYearNodeBase {
        public DocumentNode(Document document, FinancialYearNode financialYearNode, ReportRights rights) {
            Document = document;
            Parent = financialYearNode;
            Rights = rights;
        }

        public Document Document { get; private set; }
        protected ReportRights Rights { get; private set; }

        #region IsChecked
        private bool _isChecked;

        public virtual bool IsChecked {
            get { return _isChecked; }
            set {
                if (_isChecked == value) return;
                _isChecked = value;
                OnPropertyChanged("IsChecked");

                Parent.UpdateIsChecked();
            }
        }
        #endregion // IsChecked

        private FinancialYearNode Parent { get; set; }

    }
}