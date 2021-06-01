// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-07-03
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Windows;
using System.Windows.Data;
using eBalanceKitBusiness.Manager;
using eBalanceKitBusiness.Reconciliation.Enums;
using eBalanceKitBusiness.Reconciliation.ViewModels;
using eBalanceKitBusiness.Structures;
using eBalanceKitResources.Localisation;

namespace eBalanceKit.Windows.Reconciliation {
    public partial class DlgInputMode {
        public DlgInputMode() { InitializeComponent(); }

        private ReconciliationsModel Model { get { return DataContext as ReconciliationsModel; } }

        private void BtnOkClick(object sender, RoutedEventArgs e) {
            
            ReconciliationMode selectedMode = ReconciliationMode.General;
            if (rbTaxBalanceSheetValues.IsChecked.HasValue && rbTaxBalanceSheetValues.IsChecked.Value) 
                selectedMode = ReconciliationMode.TaxBalanceSheetValues;

            if (selectedMode != Model.Document.ReconciliationMode) {
                if (Model.Document.ReconciliationMode == ReconciliationMode.General) {
                    if (
                        MessageBox.Show(ResourcesReconciliation.QuestionDeleteAllCurrentYearReconciliations,
                                        ResourcesReconciliation.ChangeInputMode, MessageBoxButton.YesNo,
                                        MessageBoxImage.Information, MessageBoxResult.No) == MessageBoxResult.Yes) {
                        Model.DeleteAllCurrentYearReconciliations();
                        Model.Document.ReconciliationMode = ReconciliationMode.TaxBalanceSheetValues;
                        DocumentManager.Instance.SaveDocument(Model.Document);
                        this.DialogResult = true;
                    }
                } else if (Model.Document.ReconciliationMode == ReconciliationMode.TaxBalanceSheetValues) {
                    if (
                        MessageBox.Show(ResourcesReconciliation.QuestionDeleteAllTaxBalanceValueReconciliations,
                                        ResourcesReconciliation.ChangeInputMode, MessageBoxButton.YesNo,
                                        MessageBoxImage.Information, MessageBoxResult.No) == MessageBoxResult.Yes) {
                        Model.DeleteAllTaxBalanceSheetValueReconciliations();
                        Model.Document.ReconciliationMode = ReconciliationMode.General;
                        DocumentManager.Instance.SaveDocument(Model.Document);
                        this.DialogResult = true;
                    }
                }
            }
            Close();
        }
    }
}