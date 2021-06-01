// --------------------------------------------------------------------------------
// author: Gabor Bauer
// since: 2012-05-02
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Taxonomy.Interfaces.PresentationTree;
using Utils;
using Utils.Commands;
using eBalanceKitBase.Interfaces;
using eBalanceKitBase.Structures;
using eBalanceKitBase.Windows;
using eBalanceKitBusiness.Exceptions;
using eBalanceKitBusiness.Export;
using eBalanceKitBusiness.Import;
using eBalanceKitBusiness.Interfaces;
using eBalanceKitBusiness.Manager;
using eBalanceKitBusiness.Reconciliation.Enums;
using eBalanceKitBusiness.Reconciliation.Import;
using eBalanceKitBusiness.Reconciliation.Interfaces;
using eBalanceKitBusiness.Reconciliation.Interfaces.ReconciliationTypes;
using eBalanceKitBusiness.Reconciliation.ReconciliationTypes;
using eBalanceKitBusiness.Structures.DbMapping;
using eBalanceKitResources.Localisation;
using System.Windows;
using System.Data;

namespace eBalanceKitBusiness.Reconciliation.ViewModels {
    public class ReconciliationImportModel : NotifyPropertyChangedBase, IReconciliationImportModel {

        #region [ Constructor ]

        public ReconciliationImportModel(ReconciliationsModel reconciliationsModel) {
            this.ReconciliationsModel = reconciliationsModel;
            this.AddFileCommand = new DelegateCommand((o) => true, AddFile);
            this.DeleteFileCommand = new DelegateCommand((o) => true, DeleteFile);
            this.CsvFiles = new ObservableCollectionAsync<string>();
            this.CurrentPage = 1;
        }

        #endregion [ Constructor ]

        #region [ IReconciliationImportModel members ]

        public ReconciliationsModel ReconciliationsModel { get; private set; }
        public ValueImporter Importer { get; private set; }
        public bool? HasErrors {
            get {
                if (Importer == null) return null;
                return Importer.HasErrors;
            }
        }

        #endregion [ IReconciliationImportModel members ]

        #region [ IAssistedImport members ]

        private const string delimiter = ";";

        private char _textDelimiter = '\"';
        public char TextDelimiter {
            get { return _textDelimiter; }
            set {
                _textDelimiter = value;
                OnPropertyChanged("TextDelimiter");
                if (Preview != null) {
                    Preview.TextDelimiter = value;
                }
            }
        }

        private char _separator = ';';
        public char Separator {
            get { return _separator; }
            set { 
                _separator = value;
                OnPropertyChanged("Separator");
                if (Preview != null) {
                    Preview.Separator = value;
                }
            }
        }

        private Encoding _encoding = Encoding.Default;
        public Encoding Encoding {
            get { return _encoding; }
            set {
                _encoding = value;
                OnPropertyChanged("Encoding");
                if (Preview != null) {
                    Preview.Encoding = value;
                }
            }
        }

        public System.Windows.Input.ICommand AddFileCommand { get; private set; }

        public System.Windows.Input.ICommand DeleteFileCommand { get; private set; }
        
        public System.Windows.Input.ICommand CreateExampleFileCommand { get; private set; }

        public void SetCreateExampleFileCommand(DelegateCommand command) {
            this.CreateExampleFileCommand = command;
        }

        public ObservableCollectionAsync<string> CsvFiles { get; set; }
        #region SelectedFile
        private string _selectedFile;

        public string SelectedFile {
            get { return _selectedFile; }
            set {
                if (_selectedFile != value) {
                    _selectedFile = value;
                    OnPropertyChanged("SelectedFile");
                    OnPropertyChanged("HasSelectedFile");
                }
            }
        }
        #endregion SelectedFile

        public bool HasSelectedFile { get { return !string.IsNullOrEmpty(SelectedFile); } }
        public bool HasFiles { get { return CsvFiles.Count > 0; } }
        public bool IsStepPreviewEnabled { get { return HasFiles; } }
        public bool IsStepErrorsEnabled { get { return IsStepPreviewEnabled; } }

        private bool _ignoreErrors = false;
        public bool IgnoreErrors {
            get { return _ignoreErrors; }
            set {
                _ignoreErrors = value;
                OnPropertyChanged("IgnoreErrors");
                OnPropertyChanged("CanImport");
            }
        }

        private bool _canImport = false;
        public bool CanImport {
            get {
                if (CurrentPage == 3 && Importer != null && (!Importer.HasErrors || (Importer.HasErrors && this.IgnoreErrors)))
                    _canImport = true;
                else
                    _canImport = false;
                return _canImport;
            }
            private set {
                _canImport = value;
                OnPropertyChanged("CanImport");
            }
        }

        private System.Data.DataTable _previewData = null;
        public System.Data.DataTable PreviewData {
            get { return _previewData; } 
            private set
            {
                _previewData = value;
                OnPropertyChanged("PreviewData");
            } 
        }

        public bool ValidateAndSetDialogPage(int currentPage) {
            this.CurrentPage = currentPage;
            OnPropertyChanged("CurrentPage");
            OnPropertyChanged("CanImport");
            return true;
        }

        public int CurrentPage { get; private set; }

        private IImportPreview _preview; 
        public IImportPreview Preview {
            get {
                return this._preview;
            }
            set { 
                this._preview = value;
                OnPropertyChanged("Preview");
            }
        }

        public void PreviewImport(Window owner) {
            ImportData(owner);
        }

        public void Import(Window owner) {
            DoImport(owner);
        }

        #endregion [ IAssistedImport members ]

        #region [ Methods ]

        private void AddFile(object parameter) {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.FileOk += dlg_FileOk;
            dlg.Filter = "csv " + ResourcesCommon.Files + " (*.csv)|*.csv|txt " + ResourcesCommon.Files + " (*.txt)|*.txt|All " + ResourcesCommon.Files + " (*.*)|*.*";
            dlg.Multiselect = true;
            dlg.ShowDialog();
        }
        private void dlg_FileOk(object sender, CancelEventArgs e) {
            var fileDialog = (OpenFileDialog)sender;
            foreach (string filename in fileDialog.FileNames) {
                if (!CsvFiles.Any(f => string.Compare(f, filename, StringComparison.InvariantCultureIgnoreCase) == 0)) CsvFiles.Add(filename);
            }

            OnPropertyChanged("CsvFiles");
            OnPropertyChanged("HasFiles");
            OnPropertyChanged("IsStepPreviewEnabled");
            OnPropertyChanged("IsStepErrorsEnabled");
        }

        private void DeleteFile(object parameter) {
            CsvFiles.Remove(SelectedFile);

            OnPropertyChanged("CsvFiles");
            OnPropertyChanged("HasFiles");
            OnPropertyChanged("IsStepPreviewEnabled");
            OnPropertyChanged("IsStepErrorsEnabled");
        }

        //private void ExampleFiles(object parameter) {
            //PositionImportExampleFile();
            // currently not needed
            //AccountImportExampleFile();
        //}

        private void PositionImportExampleFile() {
            string fileName = "PositionImportExample.csv";
            StringBuilder csvContent = new StringBuilder();
            csvContent.AppendFormat("{1}{0}{2}{0}{3}{0}{4}{0}{5}{0}{6}{0}{7}", this.Separator, ResourcesReconciliation.ReferenceListPositionId,
                            ResourcesReconciliation.ReferenceListPositionDescription,
                            ResourcesReconciliation.ReferenceListValue,
                            ResourcesReconciliation.ReferenceListPreviousYearValue,
                            ResourcesReconciliation.ReferenceListReconciliationValue,
                            ResourcesReconciliation.ReferenceListReconciliationName,
                            ResourcesReconciliation.ReferenceListReconciliationDescription);
            csvContent.AppendLine();

            string reconciliationName = "Testimport 1";
            string reconciliationDescription = "Umgliederung 1";
            csvContent.AppendLine(string.Format("bs.ass.unpaidCap.called{0}davon eingefordert{0}{0}{0}500{0}{1}{0}{2}", this.Separator, reconciliationName, reconciliationDescription));
            csvContent.AppendLine(string.Format("bs.ass.unpaidCap.dueCapOfCoop{0}rückständige fällige Einzahlungen auf Geschäftsanteile{0}{0}{0}-120{0}{1}{0}{2}", this.Separator, reconciliationName, reconciliationDescription));
            csvContent.AppendLine(string.Format("bs.ass.assInbetwFixAndCurr{0}Vermögensgegenstände zwischen Anlagevermögen und Umlaufvermögen{0}{0}{0}300{0}{1}{0}{2}", this.Separator, reconciliationName, reconciliationDescription));
            csvContent.AppendLine(string.Format("bs.ass.currAss.inventory{0}Vorräte{0}{0}{0}-146{0}{1}{0}{2}", this.Separator, reconciliationName, reconciliationDescription));
            csvContent.AppendLine(string.Format("bs.ass.currAss.receiv{0}Forderungen und sonstige Vermögensgegenstände{0}{0}{0}1270{0}{1}{0}{2}", this.Separator, reconciliationName, reconciliationDescription));
            reconciliationName = "Testimport 2";
            reconciliationDescription = "Umgliederung 2";
            csvContent.AppendLine(string.Format("bs.ass.unpaidCap.uncalled{0}davon nicht eingefordert{0}{0}{0}1500{0}{1}{0}{2}", this.Separator, reconciliationName, reconciliationDescription));
            csvContent.AppendLine(string.Format("bs.ass.unpaidCap.dueCapOfCoop{0}rückständige fällige Einzahlungen auf Geschäftsanteile{0}{0}{0}-1300{0}{1}{0}{2}", this.Separator, reconciliationName, reconciliationDescription));
            csvContent.AppendLine(string.Format("bs.eqLiab.pretaxRes{0}Sonderposten mit Rücklageanteil{0}{0}{0}720{0}{1}{0}{2}", this.Separator, reconciliationName, reconciliationDescription));
            csvContent.AppendLine(string.Format("bs.eqLiab.otherSpecRes{0}Sonstige Sonderposten{0}{0}{0}3450{0}{1}{0}{2}", this.Separator, reconciliationName, reconciliationDescription));
            csvContent.AppendLine(string.Format("bs.ass.assInbetwFixAndCurr{0}Vermögensgegenstände zwischen Anlagevermögen und Umlaufvermögen{0}{0}{0}8903{0}{1}{0}{2}", this.Separator, reconciliationName, reconciliationDescription));

            SaveFileDialog dlgSaveFile = new SaveFileDialog();
            dlgSaveFile.FileName = fileName;
            dlgSaveFile.DefaultExt = ".csv";
            dlgSaveFile.Filter = "csv " + ResourcesCommon.Files + " (*.csv)|*.csv";
            DialogResult result = dlgSaveFile.ShowDialog();
            if (result == DialogResult.OK || result == DialogResult.Yes) {
                fileName = dlgSaveFile.FileName;
                Exception ex = null;
                if (!Utils.FileHelper.IsFileBeingUsed(fileName, out ex)) {
                    using (StreamWriter csvWrite = new StreamWriter(File.Open(fileName, FileMode.Create), Encoding.Default)) {
                        csvWrite.WriteLine(csvContent);
                        csvWrite.Close();
                    }
                    //if (MessageBox.Show(ResourcesCommon.FileSaveSuccessfulOpen, ResourcesCommon.FileSaved, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes) {
                    //    System.Diagnostics.Process.Start(dlgSaveFile.FileName);
                    //}
                } else {
                    System.Windows.MessageBox.Show(ex.Message, ResourcesCommon.SaveFileEror, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void AccountImportExampleFile() {
            string fileName = "AccountValueImportExample.csv";
            StringBuilder csvContent = new StringBuilder();
            csvContent.AppendFormat("Erläuterung{0}Kontonummer1{0}Kontobezeichnung1{0}Betrag1{0}", this.Separator);
            csvContent.AppendFormat("Kontonummer2{0}Kontobezeichnung2{0}Betrag2{0}", this.Separator);
            csvContent.AppendFormat("Kontonummer3{0}Kontobezeichnung3{0}Betrag3{0}", this.Separator);
            csvContent.AppendFormat("Kontonummer4{0}Kontobezeichnung4{0}Betrag4{0}", this.Separator);
            csvContent.AppendFormat("Kontonummer5{0}Kontobezeichnung5{0}Betrag5", this.Separator);
            csvContent.AppendLine();
            csvContent.AppendFormat("Testimport 1{0}20001010{0}Fertigungsmaterial Bestandsaufnahme{0}5000{0}", this.Separator);
            csvContent.AppendFormat("22990130{0}RHB Wertberichtigungen{0}12000{0}", this.Separator);
            csvContent.AppendFormat("26002000{0}Vorsteuer Ausland{0}3000{0}{0}{0}{0}{0}{0}", this.Separator);
            csvContent.AppendLine();
            csvContent.AppendFormat("Testimport 2{0}8401000{0}Fuhrpark{0}60000{0}{0}{0}{0}{0}{0}{0}{0}{0}{0}{0}{0}", this.Separator);
            csvContent.AppendLine();

            SaveFileDialog dlgSaveFile = new SaveFileDialog();
            dlgSaveFile.FileName = fileName;
            dlgSaveFile.DefaultExt = ".csv";
            dlgSaveFile.Filter = "csv " + ResourcesCommon.Files + " (*.csv)|*.csv";
            DialogResult result = dlgSaveFile.ShowDialog();
            if (result == DialogResult.OK || result == DialogResult.Yes) {
                fileName = dlgSaveFile.FileName;
                Exception ex = null;
                if (!Utils.FileHelper.IsFileBeingUsed(fileName, out ex)) {
                    using (StreamWriter csvWrite = new StreamWriter(File.Open(fileName, FileMode.Create), Encoding.Default)) {
                        csvWrite.WriteLine(csvContent);
                        csvWrite.Close();
                    }
                    //if (MessageBox.Show(ResourcesCommon.FileSaveSuccessfulOpen, ResourcesCommon.FileSaved, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes) {
                    //    System.Diagnostics.Process.Start(dlgSaveFile.FileName);
                    //}
                } else {
                    System.Windows.MessageBox.Show(ex.Message, ResourcesCommon.SaveFileEror, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        public void Preview_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            if (e.PropertyName == "PreviewData") {
                _previewData = Preview.PreviewData;
                OnPropertyChanged("PreviewData");
            }
        }
        
        #region [ Import ]

        public void CreatePreviewData() {
            ObservableCollectionAsync<string> temp = new ObservableCollectionAsync<string>();
            foreach (string fileName in CsvFiles) {
                temp.Add(fileName);
            }
            Preview = new ImportPreview(temp, Preview_PropertyChanged, Encoding, TextDelimiter, Separator);
            OnPropertyChanged("PreviewData");
            OnPropertyChanged("Preview");
        }

        public void ImportData(Window parent) {
            DlgProgress progress = new DlgProgress(parent) {
                ProgressInfo = {
                    IsIndeterminate = true,
                    Caption = ResourcesReconciliation.ProgressImportData
                }
            };
            try {
                progress.ExecuteModal(delegate() { PreviewImport(); });
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
        }

        private void PreviewImport() {
            Importer = new ValueImporter(ReconciliationsModel, TextDelimiter, Separator, Encoding);
            foreach (string fileName in CsvFiles) {
                Importer.ImportFile(fileName);
            }
            OnPropertyChanged("Importer");
            OnPropertyChanged("HasErrors");
            OnPropertyChanged("CanImport");
        }
        
        /// <summary>
        /// When a new Importer calss is produced (with all the data and errors if present) by PreviewImport with the data to be imported, then DoImport imports the data into the ReconciliationModel
        /// </summary>
        private void DoImport(Window parent) {
            DlgProgress progress = new DlgProgress(parent) {
                ProgressInfo = {
                    IsIndeterminate = true,
                    Caption = ResourcesReconciliation.ProgressImportData
                }
            };
            try {
                progress.ExecuteModal(delegate() { Importer.CreateReconciliations(); });
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
        }

        #endregion [ Import ]

        #region [ Export ]

        internal static string GetExportFileHeader(bool isProfitAndLoss = false) {
            return ResourcesReconciliation.ReferenceListPositionId + delimiter +
                   ResourcesReconciliation.ReferenceListPositionDescription + delimiter +
                   ResourcesReconciliation.ReferenceListValue + delimiter +
                   (isProfitAndLoss ? "" : ResourcesReconciliation.ReferenceListPreviousYearValue + delimiter) +
                   ResourcesReconciliation.ReferenceListReconciliationValue + delimiter +
                   ResourcesReconciliation.ReferenceListTaxValue + delimiter +
                   ResourcesReconciliation.ReferenceListReconciliationName + delimiter +
                   ResourcesReconciliation.ReferenceListReconciliationDescription;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pdfTreeView"></param>
        /// <param name="presentationTree"></param>
        /// <param name="referenceList"></param>
        /// <param name="csvContent"></param>
        /// <param name="isProfitAndLoss"></param>
        /// <param name="rowNumber">used for provide rownumber for the computed excel formula</param>
        internal static void ExportTreeView(PdfTreeView pdfTreeView, Taxonomy.Interfaces.PresentationTree.IPresentationTree presentationTree, ReferenceList referenceList, List<int> elementsWithImportedValue, StringBuilder csvContent, bool isProfitAndLoss, ref int rowNumber, List<string> nodesExported) {
            foreach (PdfTreeView child in pdfTreeView.Children) {
                try {
                    if (child.Level > 0) {
                        if (!child.IsAccount) {
                            int elementId = DocumentManager.Instance.CurrentDocument.TaxonomyIdManager.GetId(child.Element.Id);
                            if (child.Element.Id == "is")
                                continue;
                            //if (referenceList.IsElementContainedInReferenceList(elementId)) {
                            bool export = false;
                            if (referenceList != null)
                                export = referenceList.IsElementContainedInReferenceList(elementId);
                            else if (elementsWithImportedValue != null)
                                export = elementsWithImportedValue.Contains(elementId);
                            // check nodes already exported because of inculded roots
                            if (export && !nodesExported.Contains(child.Element.Id)) {
                                string value = string.Empty;
                                string hbValue = string.Empty;
                                string transfer = string.Empty;
                                string stValue = string.Empty;
                                string transferPrevious = string.Empty;

                                if (child.Value != null) value = "0";//child.Value;
                                IPresentationTreeNode node = presentationTree.GetNode(child.Element.Id);
                                if (node != null) {
                                    eBalanceKitBusiness.Structures.Presentation.PresentationTreeNode pTreeNode = node as eBalanceKitBusiness.Structures.Presentation.PresentationTreeNode;
                                    if (pTreeNode != null)
                                        value = pTreeNode.DecimalValue.ToString(LocalisationUtils.GermanCulture);
                                }

                                List<Tuple<string, string, string, string>> reconciliations = new List<Tuple<string, string, string, string>>();
                                if (child.ReconciliationInfo != null) {
                                    hbValue = !child.ReconciliationInfo.HBValue.HasValue ? "" : child.ReconciliationInfo.HBValue.Value.ToString(LocalisationUtils.GermanCulture);
                                    transfer = !child.ReconciliationInfo.TransferValue.HasValue ? "" : child.ReconciliationInfo.TransferValue.Value.ToString(LocalisationUtils.GermanCulture);
                                    transferPrevious = !child.ReconciliationInfo.TransferValuePreviousYear.HasValue ? "" : child.ReconciliationInfo.TransferValuePreviousYear.Value.ToString(LocalisationUtils.GermanCulture);
                                    stValue = !child.ReconciliationInfo.STValue.HasValue ? "" : child.ReconciliationInfo.STValue.Value.ToString(LocalisationUtils.GermanCulture);
                                    if (child.ReconciliationInfo.Transactions.Any(t => t.Reconciliation.ReconciliationType == Enums.ReconciliationTypes.ImportedValues)) {
                                        foreach (IReconciliationTransaction transaction in child.ReconciliationInfo.Transactions.Where(t => t.Reconciliation.ReconciliationType == Enums.ReconciliationTypes.ImportedValues)) {
                                            reconciliations.Add(new Tuple<string, string, string, string>(transferPrevious, (transaction.Value.HasValue ? transaction.Value.Value.ToString(LocalisationUtils.GermanCulture) : string.Empty), transaction.Reconciliation.Name, transaction.Reconciliation.Comment));
                                        }
                                    } else {
                                        reconciliations.Add(new Tuple<string, string, string, string>(transferPrevious, string.Empty, string.Empty, string.Empty));
                                    }
                                } else {
                                    hbValue = value;
                                    stValue = value;
                                    reconciliations.Add(new Tuple<string, string, string, string>(string.Empty, string.Empty, string.Empty, string.Empty));
                                }

                                // Export parents (all parents should be exported no matter is not flagged for export)
                                ExportParents(child.Parent, presentationTree, csvContent, isProfitAndLoss, ref rowNumber, nodesExported);

                                foreach (Tuple<string, string, string, string> reconciliation in reconciliations) {
                                    csvContent.AppendLine(child.Element.Name + delimiter +
                                                      child.HeaderNumber + " " + child.Header + delimiter +
                                                      value + delimiter +
                                                      (isProfitAndLoss ? "" : reconciliation.Item1 + delimiter) +
                                                      reconciliation.Item2 + delimiter +
                                                      (isProfitAndLoss ? string.Format("=C{0}+D{0}", rowNumber) : string.Format("=C{0}+D{0}+E{0}", rowNumber)) + delimiter +
                                                      reconciliation.Item3 + delimiter +
                                                      reconciliation.Item4 + delimiter);
                                    nodesExported.Add(child.Element.Id);
                                    rowNumber++;
                                }
                            }
                        }
                    }
                    ExportTreeView(child, presentationTree, referenceList, elementsWithImportedValue, csvContent, isProfitAndLoss, ref rowNumber, nodesExported);
                } catch (Exception ex) {
                    throw new Exception(ex.Message, ex);
                }
            }
        }

        private static void ExportParents(PdfTreeView parent, Taxonomy.Interfaces.PresentationTree.IPresentationTree presentationTree, StringBuilder csvContent, bool isProfitAndLoss, ref int rowNumber, List<string> nodesExported) {
            if (parent == null) return;

            if (parent.Parent != null) {
                if (parent.Parent.Element != null && !nodesExported.Any(n => n == parent.Parent.Element.Id))
                    ExportParents(parent.Parent, presentationTree, csvContent, isProfitAndLoss, ref rowNumber, nodesExported);
            }

            if (parent.Element != null) {
                if (!nodesExported.Contains(parent.Element.Id)) {
                    string value = null;
                    if (parent.Value != null) value = "0";//parent.Value;
                    IPresentationTreeNode node = presentationTree.GetNode(parent.Element.Id);
                    if (node != null) {
                        eBalanceKitBusiness.Structures.Presentation.PresentationTreeNode pTreeNode = node as eBalanceKitBusiness.Structures.Presentation.PresentationTreeNode;
                        if (pTreeNode != null)
                            value = pTreeNode.DecimalValue.ToString(LocalisationUtils.GermanCulture);
                    }

                    csvContent.AppendLine(parent.Element.Name + delimiter +
                                                          parent.HeaderNumber + " " + parent.Header + delimiter +
                                                          value + delimiter +
                                                          (isProfitAndLoss ? "" : delimiter) +
                                                          delimiter +
                                                          (isProfitAndLoss ? string.Format("=C{0}+D{0}", rowNumber) : string.Format("=C{0}+D{0}+E{0}", rowNumber)) + delimiter +
                                                          delimiter +
                                                          delimiter);
                    nodesExported.Add(parent.Element.Id);
                    rowNumber++;
                }
            }
        }

        #endregion [ Export ]

        #endregion [ Methods ]
    }
}
