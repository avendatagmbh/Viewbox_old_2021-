using System.Windows;
using eBalanceKitBusiness.Reconciliation.ViewModels;

namespace eBalanceKit.Windows.Reconciliation.Import
{
    /// <summary>
    /// Interaction logic for DlgImportOrSampleFileSelection.xaml
    /// </summary>
    public partial class DlgImportOrSampleFileSelectionReconciliation
    {
        public DlgImportOrSampleFileSelectionReconciliation(Window owner, ReconciliationsModel reconciliationsModel) {
            InitializeComponent();
            _owner = owner;
            _model = reconciliationsModel;
        }

        private readonly Window _owner; 

        private readonly ReconciliationsModel _model; 

        private void BtnImportReconciliationCsvFilesClick(object sender, RoutedEventArgs e) {
            DlgReconciliationImport dlgReconciliationImport = new DlgReconciliationImport {
                DataContext =
                    new ReconciliationImportModel(_model),
                Owner = _owner
            };
            dlgReconciliationImport.ShowDialog();
            DialogResult = true;
        }

        private void BtnCreateCsvFilesClick(object sender, RoutedEventArgs e) {
            new DlgReferenceList {
                Owner = _owner,
                DataContext = _model
            }.ShowDialog();
            DialogResult = true;
        }
    }
}
