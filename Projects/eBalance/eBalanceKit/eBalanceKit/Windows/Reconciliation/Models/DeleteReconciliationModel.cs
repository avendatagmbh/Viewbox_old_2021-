using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Forms.VisualStyles;
using Utils.Commands;
using eBalanceKit.Windows.Management.Delete.Models;
using eBalanceKitBusiness.Reconciliation.Enums;
using eBalanceKitBusiness.Reconciliation.Interfaces;
using eBalanceKitBusiness.Reconciliation.Interfaces.ReconciliationTypes;
using eBalanceKitResources.Localisation;

namespace eBalanceKit.Windows.Reconciliation.Models {
    public class DeleteReconciliationModel {
        #region Constructor
        public DeleteReconciliationModel(IReconciliationManager reconciliationManager, IEnumerable<IReconciliation> transactions, Window owner) {
            _manager = reconciliationManager;
            Owner = owner;
            Reconciliations = new List<CheckableItemWrapper<IReconciliation>>(transactions.Where(t =>
                    t.ReconciliationType == ReconciliationTypes.Reclassification ||
                    t.ReconciliationType == ReconciliationTypes.ValueChange ||
                    t.ReconciliationType == ReconciliationTypes.Delta || 
                    t.ReconciliationType == ReconciliationTypes.TaxBalanceValue).Select(t => new CheckableItemWrapper<IReconciliation>(t)));
            OkCommand = new DelegateCommand(o => true, o => ClickOk());
            DeleteSelectedCommand = new DelegateCommand(o => true, o => DeleteSelected());
            DeletedReconciliations = new List<IReconciliation>();
        }

        #endregion Constructor

        #region Properties
        public Window Owner { get; set; }
        public DelegateCommand OkCommand { get; private set; }
        public DelegateCommand DeleteSelectedCommand { get; private set; }
        public List<CheckableItemWrapper<IReconciliation>> Reconciliations { get; private set; }
        private readonly IReconciliationManager _manager;
        public List<IReconciliation> DeletedReconciliations { get; set; } 
        #endregion Properties

        #region Methods
        private void ClickOk() {
            Owner.Close();
        }
        private void DeleteSelected() {
            if (Reconciliations.Count(recon => recon.IsChecked) == 0) {
                MessageBox.Show(Owner, ResourcesReconciliation.NoSelectedReconciliation, string.Empty, MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                return;
            }
            if(MessageBox.Show(Owner, ResourcesReconciliation.AreYouSureDeleteReconciliation, string.Empty,
                            MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                return;

            foreach (var recon in Reconciliations.Where(recon => recon.IsChecked)) {
                _manager.DeleteReconciliation(recon.Item);
                DeletedReconciliations.Add(recon.Item);
            }
            Owner.DialogResult = true;
            MessageBox.Show(Owner, ResourcesReconciliation.ReconciliationDeleteSuccessful, ResourcesReconciliation.DeleteReconciliationSuccessfulTitle,
                            MessageBoxButton.OK, MessageBoxImage.Information);
        }
        #endregion Methods
    }
}
