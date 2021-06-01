using System.Linq;
using System.Windows;
using eBalanceKit.Models.GlobalSearch;
using eBalanceKit.Windows.Reconciliation.Import;
using eBalanceKit.Windows.Reconciliation.Models;
using eBalanceKitBase.Windows;
using eBalanceKitBusiness.Manager;
using eBalanceKitBusiness.Reconciliation.Import;
using eBalanceKitBusiness.Reconciliation.ViewModels;
using eBalanceKitBusiness.Validators;
using eBalanceKitResources.Localisation;

namespace eBalanceKit.Windows.Reconciliation {
    public partial class DlgReconciliation {
        public DlgReconciliation() {
            InitializeComponent();
        }

        private ReconciliationsModel Model { get { return DataContext as ReconciliationsModel; } }

        public void SetGlobalSearchContext(object reconciliationsModel) {
            CtlGlobalSearch.DataContext = new GlobalSearchModel(DocumentManager.Instance.CurrentDocument, null, null,
                                                                true, DataContext as ReconciliationsModel);
        }

        private void AddReconciliationClick(object sender, RoutedEventArgs e) {
            var dlg = new DlgAddReconciliationAssistant(Model) {Owner = this};
            dlg.ShowDialog();
            var model = (AddReconciliationModel) dlg.DataContext;
            if (model.NewReconciliation != null)
                Model.AddedReconciliation(model.NewReconciliation);
        }

        private void BtnSelectInputModeClick(object sender, RoutedEventArgs e) {
            DlgInputMode inputDlg = new DlgInputMode {Owner = this};
            inputDlg.DataContext = Model;
            inputDlg.ShowDialog();
            if(inputDlg.DialogResult.HasValue && inputDlg.DialogResult.Value) {
                new DlgProgress(UIHelpers.TryFindParent<Window>(this)) {
                    ProgressInfo = { IsIndeterminate = true, Caption = ResourcesCommon.LoadingPositions }
                }.ExecuteModal(Model.Document.ReconciliationManager.RefreshValues);
            }
        }

        private void BtnDeleteReconciliationsClick(object sender, RoutedEventArgs e) {
            var dlg = new DlgDeleteReconciliation{Owner = this};
            var model = new DeleteReconciliationModel(Model.Document.ReconciliationManager,
                                                            Model.ReconciliationList, dlg);
            dlg.DataContext = model;
            dlg.ShowDialog();
            if(dlg.DialogResult.HasValue && dlg.DialogResult.Value) Model.DeletedReconciliations(model.DeletedReconciliations);
        }
        
        private void BtnDeletePreviousYearValuesClick(object sender, RoutedEventArgs e) {
            if (MessageBox.Show(this, ResourcesReconciliation.QuestionDeleteAllPreviousYearValues, ResourcesReconciliation.DeleteReconciliation,
                MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.Yes) {
                Model.DeleteAllPreviousYearValues();
                MessageBox.Show(this, "Vorjahreswerte wurden erfolgreich gelöscht.", "", MessageBoxButton.OK,
                                MessageBoxImage.Information);
            }
        }
        
        private void BtnDeleteImportedValuesClick(object sender, RoutedEventArgs e) {
            if (MessageBox.Show(this, ResourcesReconciliation.QuestionDeleteAllImportedValues, ResourcesReconciliation.DeleteReconciliation, 
                MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.Yes) {
                Window owner = UIHelpers.TryFindParent<Window>(this);
                ReconciliationImportModel model = new ReconciliationImportModel(Model);
                model.PreviewImport(owner);
                model.Import(owner);
                MessageBox.Show(this, "Importierte Überleitungen wurden erfolgreich gelöscht.", "", MessageBoxButton.OK,
                                MessageBoxImage.Information);
            }
        }

        private void BtnImportReconciliationsClick(object sender, RoutedEventArgs e) {
            DlgImportOrSampleFileSelectionReconciliation dlgImportOrSampleFileSelectionReconciliation =
                new DlgImportOrSampleFileSelectionReconciliation(this, Model){Owner = this};
            dlgImportOrSampleFileSelectionReconciliation.ShowDialog();
        }

        private void BtnImportPreviousYearValuesClick(object sender, RoutedEventArgs e) {
            ImportPreviousYearValuesModel model = new ImportPreviousYearValuesModel();
            if (model.SelectedPreviousReport != null && model.SelectedPreviousYearValues != null && (model.SelectedPreviousYearValues.ValuesAssets.Any() || model.SelectedPreviousYearValues.ValuesLiabilities.Any()))
                new DlgImportPreviousYearValues { Owner = this, DataContext = model }.ShowDialog();
            else
                MessageBox.Show(ResourcesReconciliation.MessagePrevYearHasNoTransactions, ResourcesReconciliation.ReconciliationImport, MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.Cancel);
        }

        private void BtnChangeReconciliationsClick(object sender, RoutedEventArgs e) {
            DlgChangeReconciliations dlg = new DlgChangeReconciliations(){Owner=this};
            ChangeReconciliationModel model = new ChangeReconciliationModel(Model.ReconciliationList, dlg);
            dlg.DataContext = model;
            dlg.ShowDialog();
        }

        private void btnValidate_Click(object sender, RoutedEventArgs e) {
            (Model.Document.ValidatorGCD as ValidatorGCD).ValidateReconciliationElements();
        }
    }
}