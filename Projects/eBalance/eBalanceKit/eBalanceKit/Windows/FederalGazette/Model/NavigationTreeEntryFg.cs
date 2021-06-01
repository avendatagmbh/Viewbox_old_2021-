// --------------------------------------------------------------------------------
// author: Solueman Hussain
// since: 2012-01-03
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Linq;
using eBalanceKit.Models;

namespace eBalanceKit.Windows.FederalGazette.Model {
    public class NavigationTreeEntryFg : NavigationTreeEntryBaseFg {
        
        #region properties
        
        private bool _validationWarning;
        public bool ValidationWarning {
            get {
                return _validationWarning ||
                       Children.OfType<NavigationTreeEntryFg>().Any(child => child.ValidationWarning);
            }
            set { _validationWarning = value;
            OnPropertyChanged("ValidationWarning");
                var parent = Parent;
                while (parent!=null) {
                    ((NavigationTreeEntryFg)parent).OnPropertyChanged("ValidationWarning");
                    parent = parent.Parent;
                }
            }
        }

        private bool _validationError;
        public bool ValidationError {
            get { return _validationError || Children.OfType<NavigationTreeEntryFg>().Any(child => child.ValidationError); }
            set { 
                _validationError = value; 
                OnPropertyChanged("ValidationError");
                var parent = Parent;
                while (parent!=null) {
                    ((NavigationTreeEntryFg)parent).OnPropertyChanged("ValidationError");
                    parent = parent.Parent;
                }
            }
        }

        private bool _showHBSTTransferPanel;
        public bool ShowHBSTTransferPanel {
            get { return _showHBSTTransferPanel; }
            set {
                _showHBSTTransferPanel = value;
                OnPropertyChanged("ShowHBSTTransferPanel");
            }
        }

        private bool _showInfoPanel;
        public bool ShowInfoPanel {
            get { return _showInfoPanel; }
            set {
                _showInfoPanel = value;
                OnPropertyChanged("ShowInfoPanel");
            }
        }


        private bool _showBalancelist;
        public bool ShowBalanceList {
            get { return _showBalancelist; }
            set {
                _showBalancelist = value;
                OnPropertyChanged("ShowBalanceList");
            }
        }

        #endregion

        #region Methods
        public void Validate() {
            if (Model != null) {
                if (Model is DisplayValueTreeModel) {
                    (Model as DisplayValueTreeModel).UpdateChildrenContainsValidationErrorElement();
                    (Model as DisplayValueTreeModel).UpdateChildrenContainsValidationWarningElement();
                    ValidationWarning = (Model as DisplayValueTreeModel).ContainsValidationWarningElement;
                    ValidationError = (Model as DisplayValueTreeModel).ContainsValidationErrorElement;
                }
                if (Model is TaxonomyViewModel) {
                    ValidationWarning = (Model as TaxonomyViewModel).PresentationTree.ValidationWarning;
                    ValidationError = (Model as TaxonomyViewModel).PresentationTree.ValidationError;
                }
            }
            foreach (NavigationTreeEntryFg child in Children) {
                child.Validate();
            }
        }
        #endregion
    }
}