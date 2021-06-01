using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Threading;
using Utils;
using Utils.Commands;
using eBalanceKit.Windows.Import.Models;
using eBalanceKitBase.Interfaces;
using eBalanceKitBase.Structures;
using eBalanceKitBase.Windows;
using eBalanceKitBusiness.Exceptions;
using eBalanceKitBusiness.Import.ImportCompanyDetails;
using eBalanceKitResources.Localisation;

namespace eBalanceKit.Models.Import {
    internal class ImportCompanyDetailsModel : NotifyPropertyChangedBase, IImportCompany {
        #region Constructor
        public ImportCompanyDetailsModel(ImportCompanyDataModel owner) {
            _owner = owner;
            AddFileCommand = new DelegateCommand(o => true, AddFile);
            DeleteFileCommand = new DelegateCommand(o => true, DeleteFile);
            CsvFiles = new ObservableCollectionAsync<string>();
            CompanyDetailsImporter = new CompanyDetailsImporter(this);
            CompanyTableList = new List<Tuple<DataTable, List<int>>>();
            Encoding = Encoding.Default;
            IgnoreErrors = false;
            IsAfterImport = false;
        }

        #endregion Constructor

        #region Properties

        //public AssistantControl AssistantControl { get; set; }

        public List<Exception> ErrorsInImport { get; private set; }

        #region IsAfterImport
        private bool _isAfterImport;

        public bool IsAfterImport {
            get { return _isAfterImport; }
            set {
                if (_isAfterImport != value) {
                    _isAfterImport = value;
                    OnPropertyChanged("IsAfterImport");
                }
            }
        }
        #endregion IsAfterImport

        public bool IsSummaryEnabled { get {
            return HasFiles && (CompanyDetailsImporter == null || 
                   (CompanyDetailsImporter.TaxonomyIdErrors.Count == 0 || IgnoreErrors));
        } }

        public bool HasValueErrors { get { return CompanyDetailsImporter.ValueErrors.Count > 0; } }

        public bool HasDateErrors { get { return CompanyDetailsImporter.DateErrors.Count > 0; } }

        private readonly ImportCompanyDataModel _owner;

        //public static RoutedCommand UpdateValidation = new RoutedCommand();

        public ICommand AddFileCommand { get; set; }

        public ICommand DeleteFileCommand { get; set; }

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
        public bool IsStepErrorsEnabled { get { return IsStepValueErrorsEnabled; } }
        public bool IsStepTaxonomyErrorsEnabled { get { return HasFiles && CompanyDetailsImporter.TaxonomyIdErrors.Count > 0; } }

        public bool IsStepValueErrorsEnabled { get {
            return HasFiles && (CompanyDetailsImporter.TaxonomyIdErrors.Count == 0 || IgnoreErrors) &&
                   (CompanyDetailsImporter.ValueErrors.Count > 0 || CompanyDetailsImporter.DateErrors.Count > 0);
        } }

        private bool _ignoreErrors;

        public bool IgnoreErrors {
            get { return _ignoreErrors; }
            set {
                if (_ignoreErrors != value) {
                    _ignoreErrors = value;
                    OnPropertyChanged("IgnoreErrors");
                    OnPropertyChanged("CanImport");
                    OnPropertyChanged("IsSummaryEnabled");
                    //AssistantControl.ValidationValue = null;
                    //OnPropertyChanged("NextAllowed");
                }
                OnPropertyChanged("IsStepErrorsEnabled");
            }
        }

        public List<Tuple<DataTable, List<int>>> CompanyTableList { get; set; }

        #region PreviewData
        private DataTable _previewData;

        public DataTable PreviewData {
            get { return _previewData; }
            set {
                if (_previewData != value) {
                    _previewData = value;
                    OnPropertyChanged("PreviewData");
                }
            }
        }
        #endregion PreviewData

        public int CurrentPage { get; private set; }

        private IImportPreview _preview;

        public IImportPreview Preview {
            get { return _preview; }
            set {
                _preview = value;
                OnPropertyChanged("Preview");
            }
        }

        #region Seperator
        private char _separator = ';';

        public char Separator {
            get { return _separator; }
            set {
                if (_separator != value) {
                    _separator = value;
                    if (Preview != null) {
                        Preview.Separator = value;
                    }
                    OnPropertyChanged("Seperator");
                }
            }
        }
        #endregion Seperator

        #region TextDelimiter
        private char _textDelimiter = '\"';

        public char TextDelimiter {
            get { return _textDelimiter; }
            set {
                if (_textDelimiter != value) {
                    _textDelimiter = value;
                    if (Preview != null) {
                        Preview.TextDelimiter = value;
                    }
                    OnPropertyChanged("TextDelimiter");
                }
            }
        }
        #endregion TextDelimiter

        #region Encoding
        private Encoding _encoding = Encoding.Default;

        public Encoding Encoding {
            get { return _encoding; }
            set {
                if (_encoding != value) {
                    _encoding = value;
                    if (Preview != null) {
                        Preview.Encoding = value;
                    }
                    OnPropertyChanged("Encoding");
                }
            }
        }
        #endregion Encoding

        public CompanyDetailsImporter CompanyDetailsImporter { get; set; }

        #region HasErrors

        public bool HasErrors { get { return CompanyDetailsImporter.HasErrors; } }
        #endregion HasErrors

        #region CanImport
        public bool CanImport {
            get {
                return CompanyDetailsImporter != null && CurrentPage == 5 && IsSummaryEnabled &&
                       (!CompanyDetailsImporter.HasErrors || IgnoreErrors) && !IsAfterImport;
            }
        }

        public void UpdateCanImport() { 
            OnPropertyChanged("CanImport");
            OnPropertyChanged("HasErrors");
        }
        #endregion CanImport

        #region ResultText
        private string _resultText;

        public string ResultText {
            get { return _resultText; }
            set {
                if (_resultText != value) {
                    _resultText = value;
                    OnPropertyChanged("ResultText");
                }
            }
        }
        #endregion ResultText

        #endregion Properties

        #region Methods

        public void PreviewImport(Window window) { ImportData(window); }

        public void ImportData(Window parent) {
            DlgProgress progress = new DlgProgress(parent) {
                ProgressInfo = {
                    IsIndeterminate = true,
                    Caption = ResourcesReconciliation.ProgressImportData
                }
            };
            try {
                progress.ExecuteModal(delegate {
                    CompanyDetailsImporter.TaxonomyIdErrors.Clear();
                    CompanyDetailsImporter.ValueErrors.Clear();
                    CompanyDetailsImporter.VisibleValueErrors.Clear();
                    CompanyDetailsImporter.DateErrors.Clear();
                    CompanyTableList.Clear();
                    foreach (string fileName in CsvFiles) {
                        if (!CompanyDetailsImporter.ImportDetails(fileName, Separator, TextDelimiter, Encoding))
                            break;
                        Tuple<DataTable, List<int>> element =
                            new Tuple<DataTable, List<int>>(CompanyDetailsImporter.CsvData,
                                                            CompanyDetailsImporter.BadRows);
                        CompanyTableList.Add(element);
                    }
                    OnPropertyChanged("Importer");
                    OnPropertyChanged("HasErrors");
                    OnPropertyChanged("CanImport");
                    OnPropertyChanged("IgnoreErrors");
                    OnPropertyChanged("IsStepTaxonomyErrorsEnabled");
                    OnPropertyChanged("HasValueErrors");
                    OnPropertyChanged("HasDateErrors");
                    OnPropertyChanged("IsSummaryEnabled");
                });
            } catch (ExceptionBase ex) {
                System.Windows.MessageBox.Show(ex.Message, ex.Header);
            } catch (Exception ex) {
                System.Windows.MessageBox.Show(ex.Message);
            } finally {
                try {
                    progress.Close();
                } catch (Exception exc) {
                    Debug.WriteLine(exc);
                }
            }
        }

        public bool ValidateAndSetDialogPage(int currentPage) {
            CurrentPage = currentPage;
            OnPropertyChanged("CanImport");
            return true;
        }

        public void CorrectEntries() {
            foreach (var valueError in CompanyDetailsImporter.ValueErrors) {
                if (!String.IsNullOrEmpty(valueError.SelectedValueId.Key)) {
                    foreach (var companyTuple in CompanyTableList) {
                        if (valueError.Path == companyTuple.Item1.TableName) {
                            companyTuple.Item1.Rows[valueError.RowNumber][valueError.Column] =
                                valueError.SelectedValueId.Key.Substring(7);
                            companyTuple.Item2.Remove(valueError.RowNumber);
                        }
                    }
                }
            }
            foreach (var dateError in CompanyDetailsImporter.DateErrors) {
                foreach (var companyTuple in CompanyTableList) {
                    if (dateError.Path == companyTuple.Item1.TableName) {
                        companyTuple.Item1.Rows[dateError.LineNumber][dateError.Column] =
                            dateError.SelectedValue.ToShortDateString();
                        companyTuple.Item2.Remove(dateError.LineNumber);
                    }
                }
            }
            ResultText =
                string.Format(ResourcesCompanyImport.SummaryMessage,
                              CompanyTableList.Count,
                              CompanyDetailsImporter.TaxonomyIdErrors.Count +
                              CompanyDetailsImporter.ValueErrors.Count(error => string.IsNullOrEmpty(error.SelectedValueId.Key)));
            OnPropertyChanged("HasErrors");
        }

        //public RoutedEventHandler UpdateValidationEventHandler = (sender, args) => UpdateValidation.Execute(sender, args.Source as IInputElement);

        private void DeleteFile(object parameter) {
            CsvFiles.Remove(SelectedFile);

            OnPropertyChanged("CsvFiles");
            OnPropertyChanged("HasFiles");
            //AssistantControl.ValidationValue = null;
            //UpdateValidation.Execute(this, _owner.Owner);
            //OnPropertyChanged("NextAllowed");
            OnPropertyChanged("IsStepPreviewEnabled");
            OnPropertyChanged("IsStepTaxonomyErrorsEnabled");
            OnPropertyChanged("IsStepErrorsEnabled");
            OnPropertyChanged("IsSummaryEnabled");
        }

        private void AddFile(object parameter) {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.FileOk += dlg_FileOk;
            dlg.Filter = ResourcesCommon.FileFilterCsv + "|txt " + ResourcesCommon.Files + " (*.txt)|*.txt|All " + ResourcesCommon.Files + " (*.*)|*.*";
            dlg.Multiselect = true;
            dlg.ShowDialog();
        }

        private void dlg_FileOk(object sender, CancelEventArgs e) {
            var fileDialog = (OpenFileDialog)sender;
            foreach (string filename in fileDialog.FileNames.Where(filename => CsvFiles.All(f => string.Compare(f, filename, StringComparison.InvariantCultureIgnoreCase) != 0))) {
                CsvFiles.Add(filename);
            }

            OnPropertyChanged("CsvFiles");
            OnPropertyChanged("HasFiles");
            //UpdateValidation.Execute(this, _owner.Owner);
            //AssistantControl.ValidationValue = null;
            //OnPropertyChanged("NextAllowed");
            OnPropertyChanged("IsStepPreviewEnabled");
            OnPropertyChanged("IsStepErrorsEnabled");
            OnPropertyChanged("IsStepTaxonomyErrorsEnabled");
            OnPropertyChanged("IsSummaryEnabled");
        }

        //private void UpdatePreview() {
        //    if (CsvFiles.Count == 0) return;
        //    CsvReader reader = new CsvReader(CsvFiles[0]) { Separator = Separator, HeadlineInFirstRow = false, StringsOptionallyEnclosedBy = TextDelimiter };
        //    PreviewData = reader.GetCsvData(500, Encoding);
        //    OnPropertyChanged("UpdatePreview");
        //}

        public void Preview_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            if (e.PropertyName == "PreviewData") {
                _previewData = Preview.PreviewData;
                OnPropertyChanged("PreviewData");
            }
        }

        public void CreatePreviewData() {
            ObservableCollectionAsync<string> temp = new ObservableCollectionAsync<string>();
            foreach (string fileName in CsvFiles) {
                temp.Add(fileName);
            }
            Preview = new ImportPreview(temp, Preview_PropertyChanged, Encoding, TextDelimiter, Separator);
            OnPropertyChanged("Preview");
        }
            
        //private int[] CountMistakes() {
        //    int[] firmsAndMistakes = new int[2];
        //    firmsAndMistakes[0] = CompanyTableList.Count;
        //    firmsAndMistakes[1] = 0;
        //    foreach (var companyTable in CompanyTableList) {
        //        firmsAndMistakes[1] += companyTable.Item2.Count;
        //    }
        //    return firmsAndMistakes;
        //}

        //private void CreateResultText() {
        //    var issues = CountMistakes();
        //    ResultText = "Es werden insgesamt " + issues[0].ToString() + " Firmen importiert, dabei wurden " + issues[1].ToString() + " Fehler ignoriert.";
        //    OnPropertyChanged("CreateResultText");
        //}


        //public bool SetCurrentStep(int currentStep) {
        //    CanImport = false;
        //    bool navigateNext = false;
        //    if(currentStep == 2) {
        //        UpdatePreview();
        //    }
        //    else if(currentStep == 3) {
        //        CompanyDetailsImporter.TaxonomyIdErrors.Clear();
        //        CompanyDetailsImporter.ValueErrors.Clear();
        //        foreach(var csvFile in CsvFiles) {
        //            var company = CompanyDetailsImporter.ImportDetails(csvFile, Separator, TextDelimiter, Encoding);
        //            CompanyTableList.Add(company);
        //        }
        //            
        //        if (CompanyDetailsImporter.TaxonomyIdErrors.Count == 0) {
        //                navigateNext = true;
        //        }
        //        
        //    } else if (currentStep == 4) {
        //        
        //        
        //    } else if (currentStep == 5) {
        //        CanImport = true;
        //        CorrectEtries();
        //        CreateResultText();
        //    }
        //    return navigateNext;
        //}

        public void Import(Window window) { DoImport(window); }

        /// <summary>
        /// When a new Importer calss is produced (with all the data and errors if present) by PreviewImport with the data to be imported, then DoImport imports the data into the ReconciliationModel
        /// </summary>
        private void DoImport(Window parent) {
            DlgProgress progress = new DlgProgress(parent) {
                ProgressInfo = {
                    IsIndeterminate = true,
                    Caption = ResourcesCompanyImport.ProgressImportData
                }
            };
            try {
                progress.ExecuteModal(delegate { 
                    ErrorsInImport = new List<Exception>();
                    CustomCreateCompanyArgs args = new CustomCreateCompanyArgs(CompanyTableList);
                    parent.Dispatcher.Invoke(
                        DispatcherPriority.Normal,
                        new Action<object, CustomCreateCompanyArgs>(_owner.CreateCompanies), this, args);
                    foreach (Exception exception in args.ErrorList) {
                        ErrorsInImport.Add(exception);
                    }
                    IsAfterImport = true;
                    if (args.ErrorList.All(error => !error.Data.Contains("detail"))) {
                        ResultText =
                            string.Format(ResourcesCompanyImport.FinalMessage,
                                          CompanyTableList.Count,
                                          CompanyDetailsImporter.TaxonomyIdErrors.Count +
                                          CompanyDetailsImporter.ValueErrors.Count(error => string.IsNullOrEmpty(error.SelectedValueId.Key)));
                    } else {
                        if (
                            ErrorsInImport.All(
                                error =>
                                error.Data.Contains("detail") && error.Data["detail"] is Tuple<DataTable, List<int>>)) {
                            ResultText = string.Join(";" + Environment.NewLine,
                                                     ErrorsInImport.Select(
                                                         error =>
                                                         error.Message +
                                                         (error.Data["detail"] as Tuple<DataTable, List<int>>).Item1.
                                                             ToString() + Environment.NewLine + ResourcesCompanyImport.ErrorsInFile +
                                                         string.Join(Environment.NewLine + "\t",
                                                                     (error.Data["detail"] as
                                                                      Tuple<DataTable, List<int>>).
                                                                         Item2.Select(
                                                                             intElement =>
                                                                             ResourcesCompanyImport.LineNumber + intElement.ToString()))));
                        } else {
                            ResultText = string.Join(Environment.NewLine, ErrorsInImport.Select(error => error.Message));
                        }
                    }
                    System.Windows.MessageBox.Show(ResourcesCompanyImport.ImportSuccessful, ResourcesCompanyImport.Summary);
                    OnPropertyChanged("IsAfterImport");
                    OnPropertyChanged("ResultText");
                    OnPropertyChanged("CanImport");
                });
            } catch (ExceptionBase ex) {
                System.Windows.MessageBox.Show(ex.Message, ex.Header);
            } catch (Exception ex) {
                System.Windows.MessageBox.Show(ex.Message);
            } finally {
                try {
                    progress.Close();
                } catch (Exception exc) {
                    Debug.WriteLine(exc);
                }
            }
        }
        #endregion Methods

    }

}
