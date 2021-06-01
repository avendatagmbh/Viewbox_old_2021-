// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2010-11-04
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using EricWrapper;
using Microsoft.Win32;
using Utils;
using eBalanceKit.Models.GlobalSearch;
using eBalanceKit.Structures;
using eBalanceKit.Windows;
using eBalanceKit.Windows.Export;
using eBalanceKit.Windows.MappingTemplates;
using eBalanceKit.Windows.TaxonomyUpgrade;
using eBalanceKitBase.Interfaces;
using eBalanceKitBase.Structures;
using eBalanceKitBase.Windows;
using eBalanceKitBusiness;
using eBalanceKitBusiness.Export;
using eBalanceKitBusiness.Export.Models;
using eBalanceKitBusiness.Manager;
using eBalanceKitBusiness.Rights;
using eBalanceKitBusiness.Structures;
using eBalanceKitBusiness.Structures.DbMapping;
using eBalanceKitBusiness.Structures.XbrlElementValue;
using eBalanceKitResources.Localisation;

namespace eBalanceKit.Models {
    /// <summary>
    /// Model class for the MainWindow window.
    /// </summary>
    internal class MainWindowModel : INotifyPropertyChanged {

        #region constructor
        public MainWindowModel(Window owner) {
            Owner = owner;
            
            _documentWrapper = new ObjectWrapper<eBalanceKitBusiness.Structures.DbMapping.Document>();
            _navigationTree = new NavigationTree(Owner, _documentWrapper);
            
            Init(Owner);
            
            //GlobalSearchModel globalSearchModel = new GlobalSearchModel(CurrentDocument, SelectedNavigationEntry, NavigationTree);
            //_dlgGlobalSearch = new DlgGlobalSearch {Owner = Owner, DataContext = globalSearchModel};
        }
        #endregion constructor

        #region events
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string property) { if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(property)); }
        #endregion events

        #region eventHandler
        private void Systems_CollectionChanged(object sender,
                                               NotifyCollectionChangedEventArgs e) { OnPropertyChanged("SystemSelectionVisibility"); }
        #endregion
        
        #region properties
        //--------------------------------------------------------------------------------

        private SemaphoreSlim semaphore = new SemaphoreSlim(1);
        private DlgProgress Progress { get; set; }

        public Window Owner { get; private set; }
        public ObservableCollection<string> BalanceListNames { get; private set; }
        public ObservableCollection<object> Systems { get; set; }
        public ObservableCollection<object> Companies { get; set; }
        public ObservableCollection<object> FinancialYears { get; set; }
        public ObservableCollection<object> Documents { get; set; }

        public User CurrentUser { get { return UserManager.Instance.CurrentUser; } }

        public int SelectedMenuItem { get; set; }

        #region SelectedSystem
        private eBalanceKitBusiness.Structures.DbMapping.System _selectedSystem;

        public eBalanceKitBusiness.Structures.DbMapping.System SelectedSystem {
            get { return _selectedSystem; }
            set {
                if (_selectedSystem != value) {
                    _selectedSystem = value;
                    OnPropertyChanged("SelectedSystem");

                    if (value == null) {
                        Companies = null;
                        SelectedCompany = null;
                    } else {
                        Companies = new ObservableCollection<object>(DocumentManager.Instance.GetUsedCompanies(_selectedSystem).Where(c => CompanyManager.Instance.IsCompanyAllowed(c as Company)));
                        if (Companies.Count == 1) {
                            SelectedCompany = Companies[0] as Company;
                        } else if (!Companies.Contains(SelectedCompany)) {
                            SelectedCompany = null;
                        }
                        //DEVNOTE: for the purpose of hiding the combobox on the UI
                        if (Companies.Count == 0)
                            Companies = null;
                    }

                    OnPropertyChanged("Companies");
                    OnPropertyChanged("CompanySelectionVisibility");
                }
            }
        }
        #endregion

        #region SelectedCompany
        private Company _selectedCompany;

        public Company SelectedCompany {
            get { return _selectedCompany; }
            set {
                if (_selectedCompany != value) {
                    _selectedCompany = value;
                    OnPropertyChanged("SelectedCompany");

                    if (value == null) {
                        FinancialYears = null;
                        SelectedFinancialYear = null;
                    } else {
                        FinancialYears = DocumentManager.Instance.GetUsedFinancialYears(_selectedSystem, _selectedCompany);
                        if (FinancialYears.Count == 1) {
                            SelectedFinancialYear = FinancialYears[0] as FinancialYear;
                        } else if (!FinancialYears.Contains(SelectedFinancialYear)) {
                            SelectedFinancialYear = null;
                        }
                    }
                    CompanyManager.Instance.CurrentCompany = _selectedCompany;
                    OnPropertyChanged("FinancialYears");
                    OnPropertyChanged("FinancialYearSelectionVisibility");
                }
            }
        }
        #endregion

        #region SelectedFinancialYear
        private FinancialYear _selectedFinancialYear;

        public FinancialYear SelectedFinancialYear {
            get { return _selectedFinancialYear; }
            set {
                if (_selectedFinancialYear != value) {
                    _selectedFinancialYear = value;
                    OnPropertyChanged("SelectedFinancialYear");

                    if (value == null) {
                        Documents = null;
                        CurrentDocument = null;
                    } else {
                        Documents = DocumentManager.Instance.GetUsedDocuments(_selectedSystem, _selectedCompany,
                                                                     _selectedFinancialYear);
                        if (Documents.Count == 1) {
                            CurrentDocument = Documents[0] as eBalanceKitBusiness.Structures.DbMapping.Document;
                        } else if (!Documents.Contains(CurrentDocument)) {
                            CurrentDocument = null;
                        }
                    }
                    CompanyManager.Instance.CurrentFinancialYear = _selectedFinancialYear;
                    OnPropertyChanged("Documents");
                    OnPropertyChanged("DocumentSelectionVisibility");
                }
            }
        }
        #endregion
        
        public string TaxonomyVersion { get { return CurrentDocument == null ? string.Empty : CurrentDocument.MainTaxonomyInfo.Name; } }

        internal bool ValidationExecuted { get; set; }

        #region CurrentDocument
        private readonly ObjectWrapper<eBalanceKitBusiness.Structures.DbMapping.Document> _documentWrapper;
        internal ObjectWrapper<eBalanceKitBusiness.Structures.DbMapping.Document> DocumentWrapper { get { return _documentWrapper; } }
        
        private eBalanceKitBusiness.Structures.DbMapping.Document _currentDocument;

        public eBalanceKitBusiness.Structures.DbMapping.Document CurrentDocument {
            get { return _currentDocument; }
            set {
                if (_currentDocument != value) {
                    // Set the current document in the DocumentManager so that everyone can access it
                    DocumentManager.Instance.CurrentDocument = value;

                    // remove balance list entry list from current document
                    if (_currentDocument != null)_currentDocument.ClearDetails();
                    
                    _currentDocument = value;

                    OnPropertyChanged("CurrentDocument");
                    OnPropertyChanged("TaxonomyVersion");
                    AppConfig.DisableDbUpdates();
                    XbrlElementValueBase.InitMode = true;

                    if (value != null) {

                        // set rights
                        SelectedSystem = value.System;
                        SelectedCompany = value.Company;
                        SelectedFinancialYear = value.FinancialYear;
                        value.ReportRights = new ReportRights(value);

                        DlgProgress progress = new DlgProgress(Owner);
                        ProgressInfo progressInfo = progress.ProgressInfo;
                        progressInfo.IsIndeterminate = true;
                        progressInfo.Caption = ResourcesCommon.ProgressReportLoading;
                        CultureInfo ci = Thread.CurrentThread.CurrentCulture;
                        CultureInfo ciUI = Thread.CurrentThread.CurrentUICulture;
                        Task.Factory.StartNew(() => {
                            Thread.CurrentThread.CurrentCulture = ci;
                            Thread.CurrentThread.CurrentUICulture = ciUI;
                            LoadDetails(progress, progressInfo);
                        });
                        progress.ShowDialog();
                        
                        //_currentDocument.SelectedBalanceList.FirePropertyChangedEventsForDisplayedItems();
                        // hide company navigation entry in report navigaton, if the company is not allowed
                        if (NavigationTree.ReportCompanyEntry != null) {
                            NavigationTree.ReportCompanyEntry.IsVisible =
                                (RightManager.RightDeducer.CompanyVisible(value.Company) &&
                                 RightManager.RightDeducer.GetRight(value.Company).IsReadAllowed) ||
                                 RightManager.RightDeducer.CompanyEditable(value.Company);
                        }

                        NavigationTree.RemoveValidationInfos();
                        //value.ValidatorGCD.ResetValidationValues();
                        if (value.ValidatorGCD_Company != null) {
                            value.ValidatorGCD_Company.ResetValidationValues();
                        }
                        //value.ValidatorMainTaxonomy.ResetValidationValues();

                        foreach (var v in value.GaapPresentationTrees.Values) {
                            v.Filter.ShowOnlyMandatoryPostions = UserManager.Instance.CurrentUser.Options.ShowOnlyMandatoryPostions;
                        }
                        CheckNavigationVisibility();

                        value.IsSelected = true;

                        //xxOnPropertyChanged("SelectedSystem");
                        //OnPropertyChanged("SelectedCompany");
                        //OnPropertyChanged("SelectedFinancialYear");

                    }

                    //DocumentManager.Instance.CurrentDocument = _currentDocument;
                    _documentWrapper.Value = _currentDocument;

                    AppConfig.EnableDbUpdates();
                    XbrlElementValueBase.InitMode = false;

                    //OnPropertyChanged("SelectedBalanceList");
                    //OnPropertyChanged("ValueTreeMain");

                    ((MainWindow) Owner).CtlGlobalSearch.DataContext = new GlobalSearchModel(
                        _currentDocument, v => SelectedNavigationEntry = v as NavigationTreeEntry, NavigationTree);

                }
            }
        }
        #endregion

        #region selection combo boxes visibility properties
        public Visibility SystemSelectionVisibility {
            get {
                if (Systems == null || Systems.Count == 0) return Visibility.Collapsed;
                else return Visibility.Visible;
            }
        }

        public Visibility CompanySelectionVisibility {
            get {
                if (Companies == null) return Visibility.Collapsed;
                else return Visibility.Visible;
            }
        }

        public Visibility FinancialYearSelectionVisibility {
            get {
                if (FinancialYears == null) return Visibility.Collapsed;
                else return Visibility.Visible;
            }
        }

        public Visibility DocumentSelectionVisibility {
            get {
                if (Documents == null) return Visibility.Collapsed;
                else return Visibility.Visible;
            }
        }
        #endregion

        #region PresentationTreeFilter
        private string _presentationTreeFilter;

        public string PresentationTreeFilter {
            get { return _presentationTreeFilter; }
            set {
                if (_presentationTreeFilter != value) {
                    _presentationTreeFilter = value;
                    //this.DisplayedPresentationTree.SetFilter(_presentationTreeFilter);
                }
            }
        }
        #endregion

        #region NavigationTree
        private readonly NavigationTree _navigationTree;
        public NavigationTree NavigationTree { get { return _navigationTree; } }
        #endregion NavigationTree

        #region SelectedNavigationEntry
        private NavigationTreeEntry _selectedNavigationEntry;

        public NavigationTreeEntry SelectedNavigationEntry {
            get { return _selectedNavigationEntry; }
            set {
                if (_selectedNavigationEntry != value) {
                    _selectedNavigationEntry = value;
                    GlobalResources.Info.SelectedElement = null;
                    OnPropertyChanged("SelectedNavigationEntry");
                    OnPropertyChanged("TaxonomyVersion");
                }
            }
        }
        #endregion SelectedNavigationEntry

        //--------------------------------------------------------------------------------
        #endregion properties

        #region methods

        internal void Init(Window owner) {
            
            SystemManager.Init();
            
            var companyUpgradeResults = CompanyManager.Init(true);
            if (companyUpgradeResults.HasResults())
                new DlgShowCompanyUpgradeResults {Owner = owner, DataContext = companyUpgradeResults}.ShowDialog();

            DocumentManager.Instance.Init();
            RoleManager.Init();
            RightManager.Init();
            // FederalGazetteInfoManager.Init();
            RightManager.InitAllowedDetails();
            //_documentWrapper = new ObjectWrapper<eBalanceKitBusiness.Structures.DbMapping.Document>();

            //_navigationTree = new NavigationTree(Owner, _documentWrapper);
            NavigationTree.InitNavigation();
            Systems = DocumentManager.Instance.GetUsedSystems();
            Systems.CollectionChanged += Systems_CollectionChanged;
            OnPropertyChanged("SystemSelectionVisibility");

            //foreach (var element in DocumentManager.Instance.Documents[0].GcdTaxonomy.ElementsByName)
            //    if (element.Value.Name.StartsWith("genInfo.company.id") && !element.Value.IsAbstract && !element.Value.IsSelectionListEntry)
            //        Console.WriteLine(element.Value.Label + ";" + element.Value.Name);
        }

        /// <summary>
        /// Shows the template dialog.
        /// </summary>
        internal void ShowTemplateDialog() {
            var dialog = new DlgTemplates(CurrentDocument) {Owner = Owner};
            dialog.ShowDialog();
        }

        internal void Export(ExportTypes exportType, bool exportLikeXbrl) {
            if (!RightManager.ExportAllowed(CurrentDocument)) {
                MessageBox.Show(Owner, ResourcesExport.InsufficientRights, "",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
                return;
            }

            switch (exportType) {
                case ExportTypes.Pdf: {
                    if (exportLikeXbrl) {
                        var model = new ExportModel(CurrentDocument)
                                    {Config = {ExportType = exportType, ExportAsXbrl = true}};

                        SaveFileDialog dlg = new SaveFileDialog { FileName = model.Config.DefaultFilename, Filter = ResourcesCommon.FileFilterPdf };
                        if (!dlg.ShowDialog(Owner).Value) return;
                        model.Config.Filename = dlg.FileName;

                        bool? result = model.Export();
                        if (result == null) {
                            MessageBox.Show(ResourcesExport.ExportError + ": " + model.LastExceptionMessage,
                                            ResourcesExport.ExportError,
                                            MessageBoxButton.OK, MessageBoxImage.Error);
                        } else {
                            //MessageBox.Show(ResourcesExport.ExportSuccessMessage, ResourcesExport.ExportSuccess,
                            //                MessageBoxButton.OK, MessageBoxImage.Information);
                            Process.Start("explorer.exe", model.Config.FilePath);
                        }
                    } else {
                        new DlgExport(CurrentDocument, exportType) {Owner = Owner}.ShowDialog();
                    }
                }
                    break;

                case ExportTypes.Csv:
                    new DlgExport(CurrentDocument, exportType) {Owner = Owner}.ShowDialog();
                    break;

                case ExportTypes.Xbrl: {
                    var model = new ExportModel(CurrentDocument) {Config = {ExportType = exportType}};

                    SaveFileDialog dlg = new SaveFileDialog { FileName = model.Config.DefaultFilename, Filter = ResourcesCommon.FileFilterXbrl };
                    if (!dlg.ShowDialog(Owner).Value) return;
                    model.Config.Filename = dlg.FileName;

                    bool? result = model.Export();
                    if (result == null) {
                        MessageBox.Show(ResourcesExport.ExportError + ": " + model.LastExceptionMessage,
                                        ResourcesExport.ExportError,
                                        MessageBoxButton.OK, MessageBoxImage.Error);
                    } else {
                        //MessageBox.Show(ResourcesExport.ExportSuccessMessage, ResourcesExport.ExportSuccess,
                        //                MessageBoxButton.OK, MessageBoxImage.Information);
                        Process.Start("explorer.exe", model.Config.FilePath);
                    }
                }
                    break;

                default:
                    throw new ArgumentOutOfRangeException("exportType");
            }
        }

        /// <summary>
        /// Validates all taxonomy elements in navigation tree.
        /// </summary>
        internal void Validate() {
            string errors = "";
            ValidationExecuted = true;
            if (CurrentDocument.Validate(ref errors)) {
                //Do not call ERIC validation as it cannot handle the current taxonomy, instead show a message to the user.
                MessageBox.Show(Owner,
                                "Die eingegebenen Daten konnten erfolgreich durch die interne Prüfung validiert werden. Es kann aber keine Validierung durch den Elster-Rich-Client (ERIC) von der Finanzverwaltung durchgeführt werden, da die aktuelle Taxonomie (5.1) nicht unterstützt wird. Im November 2012 wird es eine neue ERIC Version geben, mit der die Taxonomie 5.1 validiert werden kann, bis dahin ist kein Testversand möglich.",
                                "ERIC Validierung", MessageBoxButton.OK, MessageBoxImage.Information);


                // call eric validation (only for debugging purposes!)
                //Progress = new DlgProgress(Owner);
                //Progress.ProgressInfo.Caption = ResourcesCommon.validationProgressMsg;
                //Progress.ProgressInfo.IsIndeterminate = true;
                //new Thread(StartEricValidation){ CurrentCulture = Thread.CurrentThread.CurrentCulture, CurrentUICulture = Thread.CurrentThread.CurrentUICulture }.Start();
                //Progress.ShowDialog();
            } else {
                //CurrentDocument.Validate(ref errors);
                MessageBox.Show(Owner,
                    "Die eingegebenen Daten sind unvollständig oder fehlerhaft, bitte prüfen Sie Ihre Eingaben." +
                    Environment.NewLine + errors,
                    "Fehler bei der Plausibilisierung", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            NavigationTree.Validate();
        }

        /// <summary>
        /// Update the visibility of all navigation entries and there PresentationTreeItems
        /// </summary>
        /// <param name="showAllEntries">Defines if all entries should be shown or only the entries for the selected legal form (default)</param>
        public void CheckNavigationVisibility(bool showAllEntries = false) {
            NavigationTree.CheckVisibility(showAllEntries);
        }

        private void StartEricValidation() {
            while (!Progress.IsVisible) Thread.Sleep(10);
            var eric = new Eric();
            eric.Finished += eric_Finished;
            eric.Validate(XbrlExporter.GetElsterXml(CurrentDocument, true));
        }

        private void eric_Finished(object sender, System.EventArgs e) {
            var eric = sender as Eric;
            Owner.Dispatcher.Invoke(
                DispatcherPriority.Background,
                new Action(delegate {
                    if (eric.UnknownErrorOccured) {
                        MessageBox.Show(eric.LastError, "Unbekannter Fehler bei der Plausibilisierung",
                                        MessageBoxButton.OK, MessageBoxImage.Error);
                        Progress.Close();
                        Progress = null;
                    } else {
                        if (!eric.VerificationSucceed) {
                            string msg =
                                "Die eingegebenen Daten sind unvollständig oder ungültig. Bitte prüfen Sie Ihre Eingaben.";
                            int i = 1;
                            foreach (EricResultMessage message in eric.ResultMessages) {
                                msg += Environment.NewLine + Environment.NewLine + i + ". " + message.Text;
                                i++;
                            }


                            if (msg.Contains("eine nicht unterstützte Ressource angefordert")) {

                                msg = "Sehr geehrter Anwender," +
                                      Environment.NewLine +
                                      Environment.NewLine +
                                      "Von der Finanzbehörde wird ein so genannter Elster Rich Client (kurz: ERIC) zur Verfügung gestellt, welcher vor dem Senden des Berichts eine Überprüfung der zu übermittelnden Informationen vornimmt. " +
                                      Environment.NewLine +
                                      "Leider ist der aktuell verfügbare ERIC noch nicht an die neuste Taxonomie angepasst und stößt somit auf Probleme. Die Finanzverwaltung wird im November 2012 ein Update für den ERIC bereitstellen, der mit der Taxonomie 5.1 kompatibel ist. Dieses Update wird dann umgehend in das eBilanz-Kit integriert, bis dahin ist aus technischen Gründen kein Testversand möglich.";
                            }

                            MessageBox.Show(msg, "Fehler bei der Plausibilisierung", MessageBoxButton.OK,
                                            MessageBoxImage.Error);
                            Progress.Close();
                            Progress = null;
                        } else {
                            MessageBox.Show(
                                "Bei der Plausibilisierung der Daten konnten keine Probleme gefunden werden.",
                                "Plausibilisierung erfolgreich", MessageBoxButton.OK, MessageBoxImage.Information);
                            Progress.Close();
                            Progress = null;
                        }
                    }
                }));
        }

        private void LoadDetails(DlgProgress progress, ProgressInfo progressInfo) {
            try {
                CurrentDocument.LoadDetails(progressInfo);

                if (CurrentDocument.GcdTaxonomyInfo.Version != TaxonomyManager.GetLatestTaxonomyInfo(CurrentDocument.GcdTaxonomyInfo.Type).Version ||
                    CurrentDocument.MainTaxonomyInfo.Version != TaxonomyManager.GetLatestTaxonomyInfo(CurrentDocument.MainTaxonomyInfo.Type).Version) {

                    var missingValues = DocumentManager.Instance.UpgradeTaxonomy(CurrentDocument, progressInfo);
                    Owner.Dispatcher.Invoke(
                        DispatcherPriority.Background,
                        new Action(delegate {
                        var dlgShowReportChanges = new DlgShowReportUpgradeResults(missingValues, CurrentDocument) { Owner = Owner };
                        dlgShowReportChanges.ShowDialog();

                        //Force reload of document, due to binding errors
                        var currentDoc = CurrentDocument;
                        CurrentDocument = null;
                        CurrentDocument = currentDoc;
                    }));

                }

            } catch (Exception ex) {
                Owner.Dispatcher.Invoke(
                    DispatcherPriority.Background,
                    new Action(delegate {
                        MessageBox.Show(ex.Message);
                        Owner.Close();
                    }));
            } finally {
                Owner.Dispatcher.Invoke(
                    DispatcherPriority.Background,
                    new Action(() => progress.Close()));
            }
        }
        
        #endregion methods

        #region [ Save / load environment ]

        public void SaveEnvironment() {
            if (CurrentUser == null) return;

            EnvironmentState environment = new EnvironmentState(CurrentUser.Id, SelectedSystem == null ? -1 : SelectedSystem.Id, 
                SelectedCompany == null ? -1 : SelectedCompany.Id, 
                SelectedFinancialYear == null ? -1 : SelectedFinancialYear.Id, 
                CurrentDocument == null ? -1 : CurrentDocument.Id,
                SelectedNavigationEntry == null ? null : GetSelectedNodePath(SelectedNavigationEntry),
                SelectedMenuItem);
            int indexOfSaved = UserConfig.EnvironmentStates.FindIndex(e => e.UserId == CurrentUser.Id);
            if (indexOfSaved >= 0)
                UserConfig.EnvironmentStates[indexOfSaved] = environment;
            else
                UserConfig.EnvironmentStates.Add(environment);

            foreach (INavigationTreeEntryBase navigationTreeEntry in NavigationTree.Children) {
                AddEntryToExpandedList(navigationTreeEntry, environment, new List<string>());
            }

            UserConfig.Save();
        }

        public void RefreshSystems() {
            semaphore.Wait();
            try {
                if (_selectedSystem == null) {
                    Companies = null;
                    SelectedCompany = null;
                } else {
                    List<object> usedSystems = DocumentManager.Instance.GetUsedSystems().ToList();
                    Systems = new ObservableCollection<object>(usedSystems);
                    if (Systems.Count == 1) {
                        SelectedSystem = Systems[0] as eBalanceKitBusiness.Structures.DbMapping.System;
                    } else if (!Systems.Contains(SelectedSystem)) {
                        SelectedSystem = null;
                    }
                    //DEVNOTE: for the purpose of hiding the combobox on the UI
                    if (Systems.Count == 0)
                        Systems = null;
                }

                OnPropertyChanged("Companies");
                OnPropertyChanged("CompanySelectionVisibility");
            } finally {
                semaphore.Release();
            }
        }

        public void RefreshCompanies() {
            semaphore.Wait();
            try {
                if (_selectedSystem == null) {
                    Companies = null;
                    SelectedCompany = null;
                } else {
                    List<object> usedCompanies = DocumentManager.Instance.GetUsedCompanies(_selectedSystem).ToList();
                    Companies = new ObservableCollection<object>(usedCompanies.Where(c => CompanyManager.Instance.IsCompanyAllowed(c as Company)));
                    if (Companies.Count == 1) {
                        SelectedCompany = Companies[0] as Company;
                    } else if (!Companies.Contains(SelectedCompany)) {
                        SelectedCompany = null;
                    }
                    //DEVNOTE: for the purpose of hiding the combobox on the UI
                    if (Companies.Count == 0)
                        Companies = null;
                }

                OnPropertyChanged("Companies");
                OnPropertyChanged("CompanySelectionVisibility");
            } finally {
                semaphore.Release();
            }
        }
        
        public void LoadEnvironment() {

            EnvironmentState state = UserConfig.EnvironmentStates.FirstOrDefault(e => e.UserId == CurrentUser.Id);
            if (state != null) {
                if (state.SelectedSystemId >= 0 && this.Systems != null && this.Systems.Count > 0) {
                    eBalanceKitBusiness.Structures.DbMapping.System system = this.Systems.FirstOrDefault(s => (s as eBalanceKitBusiness.Structures.DbMapping.System).Id == state.SelectedSystemId) as eBalanceKitBusiness.Structures.DbMapping.System;
                    if (system != null)
                        this.SelectedSystem = system;
                }
                if (state.SelectedCompanyId >= 0 && this.Companies != null && this.Companies.Count > 0) {
                    Company company = this.Companies.FirstOrDefault(c => (c as Company).Id == state.SelectedCompanyId) as Company;
                    if (company != null)
                        this.SelectedCompany = company;
                }
                if (state.SelectedFinancialYearId >= 0 && this.FinancialYears != null && this.FinancialYears.Count > 0) {
                    FinancialYear fy = this.FinancialYears.FirstOrDefault(f => (f as FinancialYear).Id == state.SelectedFinancialYearId) as FinancialYear;
                    if (fy != null)
                        this.SelectedFinancialYear = fy;
                }
                if (state.SelectedDocumentId >= 0 && this.Documents != null && this.Documents.Count > 0) {
                    eBalanceKitBusiness.Structures.DbMapping.Document document = this.Documents.FirstOrDefault(d => (d as eBalanceKitBusiness.Structures.DbMapping.Document).Id == state.SelectedDocumentId) as eBalanceKitBusiness.Structures.DbMapping.Document;
                    if (document != null) {
                        this.CurrentDocument = document;
                        //NavigationTree.UpdateCompanyRelatedUi();
                    }
                }
                if (state.ExpandedNavigationTreeEntries != null) {
                    foreach (string expandedNavigationTreeEntry in state.ExpandedNavigationTreeEntries) {
                        LocateNode(expandedNavigationTreeEntry, ExpandNode);
                    }
                }
                if (state.SelectedNavigationTreeEntry != null) {
                    LocateNode(state.SelectedNavigationTreeEntry, SelectNode);
                }
                if (state.SelectedMenuIndex >= 0) {
                    this.SelectedMenuItem = state.SelectedMenuIndex;
                    OnPropertyChanged("SelectedMenuItem");
                }
            }
        }
        
        private string GetSelectedNodePath(INavigationTreeEntryBase entry) { 
            string retVal = entry.NodeId;

            entry = entry.Parent;
            while (entry != null) {
                retVal = entry.NodeId + ";" + retVal;
                entry = entry.Parent;
            }

            return retVal;
        }

        private void AddEntryToExpandedList(INavigationTreeEntryBase navigationTreeEntry, EnvironmentState environment, List<string> path) {
            if (!navigationTreeEntry.IsVisible) {
                return;
            }
            
            if (navigationTreeEntry.IsExpanded) {
                string pathString = string.Join(";", path);
                environment.AddExpandedNavigationTreeEntry((pathString == string.Empty ? "" : pathString + ";") + navigationTreeEntry.NodeId);
            }

            foreach (INavigationTreeEntryBase navigationTreeEntryChild in navigationTreeEntry.Children) {
                List<string> currentPath = new List<string>(path.ToArray());
                currentPath.Add(navigationTreeEntry.NodeId);
                AddEntryToExpandedList(navigationTreeEntryChild, environment, currentPath);
            }
        }

        //private Action<INavigationTreeEntryBase> setTarget;

        private void LocateNode(string path, Action<INavigationTreeEntryBase> action) {
            string[] pathArray = path.Split(new string[] { ";" }, StringSplitOptions.None);
            LocateNode(NavigationTree.Children, pathArray, action);
        }

        private void LocateNode(IEnumerable<INavigationTreeEntryBase> children, string[] pathArray, Action<INavigationTreeEntryBase> action) {
            if (pathArray.Length > 0) {
                INavigationTreeEntryBase node = children.FirstOrDefault(e => e.NodeId == pathArray[0]);
                if (node != null)
                    if (pathArray.Length > 1) {
                        List<string> newPath = pathArray.ToList();
                        newPath.RemoveAt(0);
                        LocateNode(node.Children, newPath.ToArray(), action);
                    } else
                        //node.IsExpanded = true;
                        action(node);
            }
        }

        private void SelectNode(INavigationTreeEntryBase node) { node.IsSelected = true; }
        
        private void ExpandNode(INavigationTreeEntryBase node) { node.IsExpanded = true; }

        #endregion [ Save / load environment ]
    }
}