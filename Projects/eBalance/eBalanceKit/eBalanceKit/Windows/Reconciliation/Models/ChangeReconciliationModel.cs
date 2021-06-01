using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Forms.VisualStyles;
using Utils;
using Utils.Commands;
using eBalanceKit.Windows.Management.Delete.Models;
using eBalanceKitBusiness.Reconciliation.Enums;
using eBalanceKitBusiness.Reconciliation.Interfaces;
using eBalanceKitBusiness.Reconciliation.Interfaces.ReconciliationTypes;

namespace eBalanceKit.Windows.Reconciliation.Models {
    public class ChangeReconciliationModel : NotifyPropertyChangedBase{
        #region Constructor
        public ChangeReconciliationModel(IEnumerable<IReconciliation> transactions, Window owner) {
            Owner = owner;
            Reconciliations = new List<IReconciliation>(transactions.Where(t =>
                    t.ReconciliationType == ReconciliationTypes.Reclassification ||
                    t.ReconciliationType == ReconciliationTypes.ValueChange ||
                    t.ReconciliationType == ReconciliationTypes.Delta || 
                    t.ReconciliationType == ReconciliationTypes.TaxBalanceValue));
            OkCommand = new DelegateCommand(o => true, o => ClickOk());
            DeletedReconciliations = new List<IReconciliation>();
        }

        #endregion Constructor

        #region Properties
        public Window Owner { get; set; }
        public DelegateCommand OkCommand { get; private set; }
        public List<IReconciliation> Reconciliations { get; private set; }
        public List<IReconciliation> DeletedReconciliations { get; set; }

        #region Name
        private string _name;

        public string Name {
            get { return _name; }
            set {
                if (_name != value) {
                    _name = value;
                    OnPropertyChanged("Name");
                }
            }
        }
        #endregion Name

        #region Comment
        private string _comment;

        public string Comment {
            get { return _comment; }
            set {
                if (_comment != value) {
                    _comment = value;
                    OnPropertyChanged("Comment");
                }
            }
        }
        #endregion Comment
        #endregion Properties

        #region Methods
        private void ClickOk() {
            Owner.Close();
        }
        #endregion Methods
    }
}
