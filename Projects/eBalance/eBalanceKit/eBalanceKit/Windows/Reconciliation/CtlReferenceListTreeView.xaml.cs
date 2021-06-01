using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Microsoft.Win32;
using Taxonomy.Enums;
using Taxonomy.Interfaces.PresentationTree;
using eBalanceKitBase.Structures;
using eBalanceKitBase.Windows;
using eBalanceKitBusiness.Exceptions;
using eBalanceKitBusiness.Export;
using eBalanceKitBusiness.Interfaces;
using eBalanceKitBusiness.Reconciliation;
using eBalanceKitBusiness.Reconciliation.Enums;
using eBalanceKitBusiness.Reconciliation.Interfaces;
using eBalanceKitBusiness.Reconciliation.ViewModels;
using eBalanceKitBusiness.Structures.Presentation;
using eBalanceKitResources.Localisation;
using eBalanceKitBusiness.Manager;

namespace eBalanceKit.Windows.Reconciliation {
    /// <summary>
    /// Interaction logic for CtlReferenceListTreeView.xaml
    /// </summary>
    public partial class CtlReferenceListTreeView : UserControl {

        private PdfTreeView _pdfTreeView = new PdfTreeView();

        public CtlReferenceListTreeView() {
            InitializeComponent();
        }

        private IReconciliationTree Model { get { return DataContext as IReconciliationTree; } }

        public void ExpandAllSelected(IReconciliationTreeNode node) {
            Model.ExpandAllSelectedNodesForReferenceList(node);
        }

        private void BtnExpandAllNodesClick(object sender, RoutedEventArgs e) {
            if (Model != null)
                new DlgProgress(UIHelpers.TryFindParent<Window>(this)) {
                    ProgressInfo = { IsIndeterminate = true, Caption = ResourcesCommon.LoadingPositions }
                }.ExecuteModal(Model.ExpandAllNodes);
        }

        private void BtnCollapseAllNodesClick(object sender, RoutedEventArgs e) { if (Model != null) Model.CollapseAllNodes(); }

        #region ItemTemplate
        public DataTemplate ItemTemplate { get { return (DataTemplate)GetValue(ItemTemplateProperty); } set { SetValue(ItemTemplateProperty, value); } }

        public static readonly DependencyProperty ItemTemplateProperty =
            DependencyProperty.Register("ItemTemplate", typeof(DataTemplate), typeof(CtlReferenceListTreeView),
                                        new UIPropertyMetadata(null));
        #endregion ItemTemplate

        #region ItemTemplateSelector
        public DataTemplateSelector ItemTemplateSelector {
            get { return (DataTemplateSelector)GetValue(ItemTemplateSelectorProperty); }
            set { SetValue(ItemTemplateSelectorProperty, value); }
        }

        public static readonly DependencyProperty ItemTemplateSelectorProperty =
            DependencyProperty.Register("ItemTemplateSelector", typeof(DataTemplateSelector), typeof(CtlReferenceListTreeView), new UIPropertyMetadata(null));
        #endregion ItemTemplateSelector
                
        private void ReferenceListCancel_Click(object sender, RoutedEventArgs e) {
            UIHelpers.TryFindParent<Window>(this).Close();
        }

        private void ReferenceListSave_Click(object sender, RoutedEventArgs e) {

            // save checked state

            IPresentationTreeNode root1 = DocumentManager.Instance.CurrentDocument.PresentationTreeBalanceSheetTotalAssets.RootEntries.First();
            IPresentationTreeNode root2 = DocumentManager.Instance.CurrentDocument.PresentationTreeBalanceSheetLiabilities.RootEntries.First();
            IPresentationTreeNode root3 = DocumentManager.Instance.CurrentDocument.PresentationTreeIncomeStatement.RootEntries.First();
            
            PdfTreeView _pdfTreeView1 = new PdfTreeView();
            CsvExport.BuildTreeView(_pdfTreeView1, DocumentManager.Instance.CurrentDocument.ValueTreeMain, root1, true, 1, true);
            PdfTreeView _pdfTreeView2 = new PdfTreeView();
            CsvExport.BuildTreeView(_pdfTreeView2, DocumentManager.Instance.CurrentDocument.ValueTreeMain, root2, true, 2, true);
            _pdfTreeView = new PdfTreeView();
            _pdfTreeView1.Parent = _pdfTreeView;
            _pdfTreeView2.Parent = _pdfTreeView;

            // reference list balance
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.FileName = ResourcesReconciliation.ReferenceListFileName;
            dlg.FileOk += SaveToFile;
            dlg.Filter = "csv " + ResourcesCommon.Files + " (*.csv)|*.csv";
            dlg.ShowDialog();

            PdfTreeView _pdfTreeView3 = new PdfTreeView();
            CsvExport.BuildTreeView(_pdfTreeView3, DocumentManager.Instance.CurrentDocument.ValueTreeMain, root3, true, 3, false);
            _pdfTreeView = new PdfTreeView();
            _pdfTreeView3.Parent = _pdfTreeView;

            // reference list profit and loss
            dlg = new SaveFileDialog();
            dlg.FileName = string.Format(ResourcesReconciliation.ReferenceListFileNameProfitAndLoss,ConfigExport.GetDefaultBasename(DocumentManager.Instance.CurrentDocument));
            dlg.FileOk += SaveToFile;
            dlg.Filter = "csv " + ResourcesCommon.Files + " (*.csv)|*.csv";
            dlg.ShowDialog();

            UIHelpers.TryFindParent<Window>(this).Close();
        }

        private void SaveToFile(object sender, CancelEventArgs e) {
            bool successful = false;
            string fileName = ((SaveFileDialog)sender).FileName;
            // save to file
            var progress = new DlgProgress(UIHelpers.TryFindParent<Window>(this)) { ProgressInfo = { IsIndeterminate = true, Caption = ResourcesReconciliation.ProgressSaveReferenceList } };
            try {
                progress.ExecuteModal(delegate() { successful = CreateFileFile(fileName); });
            } catch (ExceptionBase ex) {
                MessageBox.Show(ex.Message, ex.Header);
            } catch (Exception ex) {
                MessageBox.Show(ex.Message);
            } finally {
                try {
                    progress.Close();
                } catch (Exception exc) {
                    System.Diagnostics.Debug.WriteLine(exc);
                }
            }

            if (successful && MessageBox.Show(ResourcesCommon.FileSaveSuccessfulOpen, ResourcesCommon.FileSaved, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes) {
                System.Diagnostics.Process.Start(fileName);
            }
        }

        private bool CreateFileFile(string fileName) {
            try {
                FileInfo file = new FileInfo(fileName);
                DirectoryInfo directory = new DirectoryInfo(file.DirectoryName);
                if (!directory.Exists) directory.Create();

                ReferenceList referenceList = DocumentManager.Instance.CurrentDocument.ReconciliationReferenceList;

                bool isProfitAndLoss = !fileName.Substring(0, fileName.LastIndexOf(".")).EndsWith(ResourcesReconciliation.ReferenceListFileName);

                Exception ex = null;
                StringBuilder csvContent = new StringBuilder();
                if (!Utils.FileHelper.IsFileBeingUsed(fileName, out ex)) {
                    using (StreamWriter csvWrite = new StreamWriter(File.Open(fileName, FileMode.Create), Encoding.Default)) {
                        csvContent.AppendLine(ReconciliationImportModel.GetExportFileHeader(isProfitAndLoss));
                        int rowNumber = 2;
                        List<string> nodesExported = new List<string>();
                        ReconciliationImportModel.ExportTreeView(_pdfTreeView, DocumentManager.Instance.CurrentDocument.PresentationTreeBalanceSheetTotalAssets, referenceList, null, csvContent, isProfitAndLoss, ref rowNumber, nodesExported);

                        csvWrite.WriteLine(csvContent);
                        csvWrite.Close();
                    }
                } else {
                    System.Windows.MessageBox.Show(ex.Message, ResourcesCommon.SaveFileEror, MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
                return true;
            } catch (Exception ex) {
                throw new Exception(
                    ResourcesReconciliation.ReferenceListCreationError + Environment.NewLine + ex.Message, ex);
            }
        }
    }
}
