using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Taxonomy.Interfaces.PresentationTree;
using eBalanceKit.Windows.Reconciliation.Import;
using eBalanceKitBase.Structures;
using eBalanceKitBusiness.Export;
using eBalanceKitBusiness.Localisation;
using eBalanceKitBusiness.Manager;
using eBalanceKitBusiness.Reconciliation;
using eBalanceKitBusiness.Reconciliation.Enums;
using eBalanceKitBusiness.Reconciliation.Interfaces;
using eBalanceKitBusiness.Reconciliation.ViewModels;
using eBalanceKitResources.Localisation;
using eBalanceKit.Windows.BalanceList;
using eBalanceKit.Models;
using Utils;
using eBalanceKitBusiness.Structures.DbMapping;
using System.Windows.Forms;
using System.ComponentModel;
using System.IO;
using eBalanceKitBase.Windows;
using eBalanceKitBusiness.Exceptions;
using eBalanceKitBusiness.Reconciliation.Interfaces.ReconciliationTypes;

namespace eBalanceKit.Windows.Reconciliation.DetailViews {
    /// <summary>
    /// Interaction logic for ImportedValuesDetails.xaml
    /// </summary>
    public partial class ImportedValuesDetails {
        
        private PdfTreeView _pdfTreeView = new PdfTreeView();

        public ImportedValuesDetails() { InitializeComponent(); }

        private TaxonomyViewModel _taxonomyModel = null;
        private TaxonomyViewModel TaxonomyModel {
            get {
                if (_taxonomyModel == null) {
                    Window owner = UIHelpers.TryFindParent<Window>(this);
                    ObjectWrapper<Document> documentWrapper = new ObjectWrapper<Document>();
                    documentWrapper.Value = Model.Document;
                    TaxonomyViewModel model = new TaxonomyViewModel(owner, documentWrapper);
                    model.Elements = documentWrapper.Value.MainTaxonomy.Elements;
                    model.RoleURI = Model.Document.PresentationTreeBalanceSheetTotalAssets.Role.RoleUri;
                    this._taxonomyModel = model;
                }
                return _taxonomyModel;
            }
        }
        private ReconciliationsModel Model { get { return DataContext as ReconciliationsModel; } }

        private void ReferenceListOnClick(object sender, RoutedEventArgs e) {
            Window owner = UIHelpers.TryFindParent<Window>(this);
            new DlgReferenceList() {
                Owner = owner,
                DataContext = Model
            }.ShowDialog();
        }

        private void AccountReferenceListOnClick(object sender, RoutedEventArgs e) {
            Window owner = UIHelpers.TryFindParent<Window>(this);
            new DlgAccountReferenceList() {
                Owner = owner,
                DataContext = TaxonomyModel
            }.ShowDialog();
        }

        private void ImportOnClick(object sender, RoutedEventArgs e) {
            if (!Model.Document.ReportRights.WriteTransferValuesAllowed)
                throw new AccessDeniedException(ExceptionMessages.InsufficentWriteRights);

            Window owner = UIHelpers.TryFindParent<Window>(this);
            DlgImportOrSampleFileSelectionReconciliation dlgImportOrSampleFileSelectionReconciliation =
                new DlgImportOrSampleFileSelectionReconciliation(owner, Model){Owner=owner};
            dlgImportOrSampleFileSelectionReconciliation.ShowDialog();
        }

        private void ExportOnClick(object sender, RoutedEventArgs e) {
            if (!Model.Document.ReportRights.ExportAllowed)
                throw new AccessDeniedException(ExceptionMessages.InsufficentWriteRights);

            var dlg = new SaveFileDialog();
            dlg.FileOk += SaveToFile;
            dlg.Filter = "csv " + ResourcesCommon.Files + " (*.csv)|*.csv";
            dlg.ShowDialog();
        }

        private void DeleteOnClick(object sender, RoutedEventArgs e) {
            if (System.Windows.MessageBox.Show(ResourcesReconciliation.DeleteAllReconciliation, ResourcesCommon.DeleteAll, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes) {
                Window owner = UIHelpers.TryFindParent<Window>(this);
                ReconciliationImportModel model = new ReconciliationImportModel(Model);
                model.PreviewImport(owner);
                model.Import(owner);
            }
        }

        private void SaveToFile(object sender, CancelEventArgs e) {
            bool successful = false;
            string fileName = ((SaveFileDialog)sender).FileName;
            // save to file
            var progress = new DlgProgress(UIHelpers.TryFindParent<Window>(this)) { ProgressInfo = { IsIndeterminate = true, Caption = ResourcesReconciliation.ProgressExportData } };
            try {
                List<int> elementsWithImportedValue = new List<int>();
                this.Dispatcher.Invoke(
                System.Windows.Threading.DispatcherPriority.Normal,
                new Action(
                    delegate() {
                        IEnumerable<IImportedValues> importedValues = null;
                        importedValues = Model.Document.ReconciliationManager.ImportedValues;
                        foreach (IImportedValues importedValue in importedValues) {
                            foreach (IReconciliationTransaction transaction in importedValue.Transactions) {
                                int elementId = Model.Document.TaxonomyIdManager.GetId(transaction.Position.Id);
                                elementsWithImportedValue.Add(elementId);
                            }
                        }
                    }
                ));
                progress.ExecuteModal(delegate() { successful = CreateFileFile(fileName, elementsWithImportedValue); });
            } catch (ExceptionBase ex) {
                System.Windows.MessageBox.Show(ex.Message, ex.Header);
            } catch (Exception ex) {
                System.Windows.MessageBox.Show(ex.Message);
            } finally {
                try {
                    progress.Close();
                } catch (Exception exc) {
                    System.Diagnostics.Debug.WriteLine(exc);
                }
            }

            if (successful && System.Windows.MessageBox.Show(ResourcesCommon.FileSaveSuccessfulOpen, ResourcesCommon.FileSaved, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes) {
                System.Diagnostics.Process.Start(fileName);
            }
        }

        private bool CreateFileFile(string fileName, List<int> elementsWithImportedValue) {
            try {
                FileInfo file = new FileInfo(fileName);
                DirectoryInfo directory = new DirectoryInfo(file.DirectoryName);
                if (!directory.Exists) directory.Create();

                IPresentationTreeNode root1 = DocumentManager.Instance.CurrentDocument.PresentationTreeBalanceSheetTotalAssets.RootEntries.First();
                IPresentationTreeNode root2 = DocumentManager.Instance.CurrentDocument.PresentationTreeBalanceSheetLiabilities.RootEntries.First();
                IPresentationTreeNode root3 = DocumentManager.Instance.CurrentDocument.PresentationTreeIncomeStatement.RootEntries.First();

                PdfTreeView _pdfTreeView1 = new PdfTreeView();
                CsvExport.BuildTreeView(_pdfTreeView1, DocumentManager.Instance.CurrentDocument.ValueTreeMain, root1, true, 1, true);
                PdfTreeView _pdfTreeView2 = new PdfTreeView();
                CsvExport.BuildTreeView(_pdfTreeView2, DocumentManager.Instance.CurrentDocument.ValueTreeMain, root2, true, 2, true);
                PdfTreeView _pdfTreeView3 = new PdfTreeView();
                CsvExport.BuildTreeView(_pdfTreeView3, DocumentManager.Instance.CurrentDocument.ValueTreeMain, root3, true, 3, true);
                _pdfTreeView = new PdfTreeView();
                _pdfTreeView1.Parent = _pdfTreeView;
                _pdfTreeView2.Parent = _pdfTreeView;
                _pdfTreeView3.Parent = _pdfTreeView;
                
                Exception ex = null;
                StringBuilder csvContent = new StringBuilder();
                if (!Utils.FileHelper.IsFileBeingUsed(fileName, out ex)) {
                    using (StreamWriter csvWrite = new StreamWriter(File.Open(fileName, FileMode.Create), Encoding.Default)) {
                        csvContent.AppendLine(ReconciliationImportModel.GetExportFileHeader(false));
                        int rowNumber = 2;
                        List<string> nodesExported = new List<string>();
                        ReconciliationImportModel.ExportTreeView(_pdfTreeView, DocumentManager.Instance.CurrentDocument.PresentationTreeBalanceSheetTotalAssets, null, elementsWithImportedValue, csvContent, false, ref rowNumber, nodesExported);

                        csvWrite.WriteLine(csvContent);
                        csvWrite.Close();
                    }
                    return true;
                } else {
                    System.Windows.MessageBox.Show(ex.Message, ResourcesCommon.SaveFileEror, MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
            } catch (Exception ex) {
                throw new Exception(
                    ResourcesReconciliation.ReferenceListCreationError + Environment.NewLine + ex.Message, ex);
            }
        }
    }
}
