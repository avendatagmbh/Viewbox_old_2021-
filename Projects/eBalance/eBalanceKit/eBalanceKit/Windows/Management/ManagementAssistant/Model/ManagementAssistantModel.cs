// --------------------------------------------------------------------------------
// author: Sebastian Vetter
// since: 2012-07-02
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using Utils;
using Utils.Commands;
using eBalanceKit.Windows.Management.ManagementAssistant.Add.Models;
using eBalanceKitBase.Structures;
using eBalanceKitBusiness.Import;
using eBalanceKitBusiness.Structures.DbMapping;
using eBalanceKitResources.Localisation;
//using System = eBalanceKitBusiness.Structures.DbMapping.System;

namespace eBalanceKit.Windows.Management.ManagementAssistant.Model {
    public class ManagementAssistantModel : Utils.NotifyPropertyChangedBase{

        public ManagementAssistantModel(DlgManagementAssistant window) {

            _dontAskAgain =
                    !eBalanceKitBusiness.Manager.UserManager.Instance.CurrentUser.Options.ManagementAssistantAskAgain;

            SelectedObject = new Dictionary<ObjectTypes, object>();
            AllowEdit = new Dictionary<ObjectTypes, bool>(){{ObjectTypes.System, true}, {ObjectTypes.Company, false}, {ObjectTypes.FinancialYear, false}};
            ShowObject = new Dictionary<ObjectTypes, bool>(){{ObjectTypes.System, true}, {ObjectTypes.Company, false}, {ObjectTypes.FinancialYear, false}};
            AddReportModel = new AddObjectModel(Owner, ObjectTypes.Report);

            BalanceListImporter = new BalanceListImporterCollection();
            
            Selection = new Dictionary<ObjectTypes, SelectObjectModel>() {
                {ObjectTypes.System, new SelectObjectModel(eBalanceKitBusiness.Manager.SystemManager.Instance.Systems.OrderBy(s => s.Name), ObjectTypes.System)},
                {ObjectTypes.Company, new SelectObjectModel(eBalanceKitBusiness.Manager.CompanyManager.Instance.AllowedCompanies.OrderBy(c => c.Name), ObjectTypes.Company)},
                {ObjectTypes.FinancialYear, new SelectObjectModel(AvailableFinancialYears, ObjectTypes.FinancialYear)}
            };

            Create = new Dictionary<ObjectTypes, AddObjectModel>() {
                {ObjectTypes.System, new AddObjectModel(ObjectTypes.System)},
                {ObjectTypes.Company, new AddObjectModel(ObjectTypes.Company)}
            };


            
            CmdObjectSelected = new DelegateCommand(delegate(object o) {
                if (o is RoutedEventArgs) {
                    return false;
                }
                return true;
            }, delegate(object o) {
                UpdateModificationAllowed();
                ObjectTypes objectType;
                if (Enum.TryParse(o.ToString(), true, out objectType)) {
                    if(SelectedObject.ContainsKey(objectType)) {
                        SelectedObject[objectType] = Selection[objectType].SelectedObject;
                    }
                    else {
                        SelectedObject.Add(objectType, Selection[objectType].SelectedObject);
                    }

                    if (Selection[objectType].SelectedObject == null || Selection[objectType].IsCreateNewObjectSelected){ //SelectedObject[objectType].ToString().Equals(Selection[objectType].DefaultEntry)) {
                        SelectedObject[objectType] = null;
                        if(objectType != ObjectTypes.Company) {
                            CmdCreateObject.Execute(objectType);
                        } else {
                            //window.assistantControl.NavigateBack();
                            //CmdObjectSelected.RaiseCanExecuteChanged();
                            ShowCompanyNameCommentSite = true;
                        }
                    }else if (objectType == ObjectTypes.Company) {
                        //LoadFinancialYears();
                    } else if (objectType == ObjectTypes.FinancialYear) {
                        GenerateObject(objectType);
                        OnPropertyChanged("FinancialYearDetailHeader");
                    }
                } else {
                    Debug.Fail(o + " could not be parsed to ObjectTypes");
                }
                OnPropertyChanged("SelectedObject");
            });


            //CmdCreateObject = new DelegateCommand(o => true, delegate(object o) {
            //    ObjectTypes objectType;
            //    if (Enum.TryParse(o.ToString(), true, out objectType)) {
            //        if (Selection[objectType].IsCreateNewObjectSelected) {
                        
                        
            //        }
            //    }
            //});

            CmdCreateObject = new DelegateCommand(delegate(object o) {
                if (o is RoutedEventArgs) {
                    return false;
                }
                return true;
            }, delegate(object o) {
                ObjectTypes objectType;
                if (Enum.TryParse(o.ToString(), true, out objectType)) {

                    if (objectType == ObjectTypes.Report) {
                        SetDocumentInfo();
                        ResetData();
                        return;
                    }


                    if (SelectedObject.ContainsKey(objectType)) {
                        //Selection[objectType].SelectedObject == null || Selection[objectType].SelectedObject.ToString() != Selection[objectType].DefaultEntry
                        if (SelectedObject[objectType] == null || Selection[objectType].IsCreateNewObjectSelected) {
                            //SelectedObject[objectType].ToString().Equals(Selection[objectType].DefaultEntry)) {
                            if (!GenerateObject(objectType)) {
                                window.assistantControl.NavigateBack();
                            }
                        }
                            // else do nothing because the user choosed an existing object
                        else if (objectType == ObjectTypes.Company) {
                            //window.assistantControl.NavigateNext();
                        }
                    } else {
                        Debug.Fail("The object was not added in the step before");
                        SelectedObject.Add(objectType, Selection[objectType].SelectedObject);
                    }
                } else {
                    Debug.Fail(o + " could not be parsed to ObjectTypes");
                }
                OnPropertyChanged("SelectedObject");
                if (objectType == ObjectTypes.Company) {
                    if (Selection[objectType].IsCreateNewObjectSelected && AskingModels.ContainsKey(ObjectTypes.Company) &&
                        AskingModels[ObjectTypes.Company] is AskingCompanyModel &&
                        Create.ContainsKey(ObjectTypes.Company)) {
                        (AskingModels[ObjectTypes.Company] as AskingCompanyModel).CompanyName =
                            Create[ObjectTypes.Company].Name;
                        OnPropertyChanged("AskingModels");
                    }

                    //LoadFinancialYears();
                }
            });

            Selection[ObjectTypes.Company].PropertyChanged += delegate(object sender, PropertyChangedEventArgs args) {
                                                                    if (args.PropertyName.Equals("IsCreateNewObjectSelected") &&
                                                                        !Selection[ObjectTypes.Company].IsCreateNewObjectSelected) {
                                                                        LoadFinancialYears();
                                                                    }
                                                                };

            //CmdDeleteBalanceList = new DelegateCommand(o => true, delegate(object o) {
            //    if (o is BalanceListImporter) {
            //        BalanceListImporter.Remove(o as BalanceListImporter);
            //    }
            //    if (o is int) {
            //        BalanceListImporter.RemoveAt((int)o);
            //    }
            //});

            AskingModels = new Dictionary<ObjectTypes, IAskingModel>();

            //AskingModels.Add(ObjectTypes.System, new AskingSystemModel(window));
            AskingModels.Add(ObjectTypes.Company, new AskingCompanyModel(window, Create[ObjectTypes.Company].Name));
            //AskingModels.Add(ObjectTypes.Report, null);
            
            //AskingModels[ObjectTypes.System].PropertyChanged += (sender, args) => FirePropertyChanged();
            
            //AskingModels[ObjectTypes.Company].PropertyChanged += (sender, args) => FirePropertyChanged();
            
            BalanceListImportModel = new AskingBalanceListImportModel(window);
            BalanceListImportModel.PropertyChanged += (sender, args) => {
                if (BalanceListImportModel.Result != null) {
                    BalanceListImporter.Add(new MyBalanceListImporter(BalanceListImportModel.Result as BalanceListImporter));
                    BalanceListImported = true;
                }
                                                                //OnPropertyChanged("BalanceListImported");
                                                                //FirePropertyChanged();
                                                                //window.assistantControl.NavigateNext();
                                                            };

            //BalanceListTestlistModel = new AskingBalanceListTestlistModel(window);
            //BalanceListTestlistModel.PropertyChanged += (sender, args) => BalanceListImporter = BalanceListTestlistModel.Result as BalanceListImporter; FirePropertyChanged();

            CmdOk = new DelegateCommand(o => true, o => window.Close());
            //CmdReset = new DelegateCommand(o => true, o => Reset());
            CmdRefreshVisibility = new DelegateCommand(o => true, o => CheckVisibilityStuff());
            CmdGoBackToStart = new DelegateCommand(o => true, o => { window.assistantControl.SelectedIndex = 1; });
            CmdCompanyDetails = new DelegateCommand(o => true, delegate(object o) {
                bool result;
                if (bool.TryParse(o.ToString(), out result)) {
                    InsertCompanyDetails = result;
                    GoForward();
                }
            });
            OnPropertyChanged("CmdGoBackToStart");
            OnPropertyChanged("CmdRefreshVisibility");
            // dirty hack because the command binding for assistantControl is not working like expected
            window.assistantControl.Command = CmdRefreshVisibility;

            Owner = window;

            //RepeatModel = new AskingRepeatModel(window);
            
        }

        public string FinancialYearDetailHeader {
            get {
                if (Selection == null || !Selection.ContainsKey(ObjectTypes.FinancialYear) ||
                    Selection[ObjectTypes.FinancialYear] == null ||
                    Selection[ObjectTypes.FinancialYear].SelectedObject == null ||
                    !(Selection[ObjectTypes.FinancialYear].SelectedObject is FinancialYear)) {
                    return ResourcesManamgement.ConfigurationForFinancialYear;
                }
                return ResourcesManamgement.ConfigurationForFinancialYear + " " +
                       (Selection[ObjectTypes.FinancialYear].SelectedObject as FinancialYear).FYear;
            }
        }

        #region ShowCompanyNameCommentSite
        private bool _showCompanyNameCommentSite;

        public bool ShowCompanyNameCommentSite {
            get { return _showCompanyNameCommentSite; }
            set {
                if (_showCompanyNameCommentSite != value) {
                    _showCompanyNameCommentSite = value;
                    OnPropertyChanged("ShowCompanyNameCommentSite");
                }
            }
        }
        #endregion ShowCompanyNameCommentSite


        public DelegateCommand CmdCompanyDetails { get; set; }

        private void ResetData() {
            foreach (var selection in Selection.Values) {
                selection.SelectedObject = selection.DefaultEntry;
            }
            SelectedObject = new Dictionary<ObjectTypes, object>();
            foreach (var askingModel in AskingModels) {
                askingModel.Value.Result = null;
            }
        }

        //ManagementAssistantAskAgain

        #region DontAskAgain
        private bool _dontAskAgain;

        public bool DontAskAgain {
            get { return _dontAskAgain; }
            set {
                if (_dontAskAgain != value) {
                    _dontAskAgain = value;                    
                    eBalanceKitBusiness.Manager.UserManager.Instance.CurrentUser.Options.ManagementAssistantAskAgain = !value;
                    eBalanceKitBusiness.Manager.UserManager.Instance.CurrentUser.Options.SaveConfiguration();
                    OnPropertyChanged("DontAskAgain");
                }
            }
        }

        public bool DontAskAgainSavedOption { get { return !eBalanceKitBusiness.Manager.UserManager.Instance.CurrentUser.Options.ManagementAssistantAskAgain; } }
        #endregion DontAskAgain


        private void LoadFinancialYears() {
            var sel = Selection[ObjectTypes.FinancialYear].SelectedObject;
            Selection[ObjectTypes.FinancialYear].AvailableObjects = AvailableFinancialYears;
            // requiered for the case that the user selected a financial year and than goes back and for
            if (sel != null) Selection[ObjectTypes.FinancialYear].SelectedObject = sel;
        }

        public AddObjectModel AddReportModel { get; set; }
        
        public class BalanceListImporterCollection : ObservableCollectionAsync<MyBalanceListImporter> {
            
            public BalanceListImporterCollection(){
                CmdDeleteBalanceList = new DelegateCommand(o => true, delegate(object o) {

                    var msgBoxResult = MessageBox.Show(ResourcesManamgement.DeleteBalanceListConfirmation, ResourcesManamgement.DeleteBalanceListHeader,
                                                       MessageBoxButton.YesNo, MessageBoxImage.Question,
                                                       MessageBoxResult.No);
                    if (msgBoxResult != MessageBoxResult.Yes) {
                        return;
                    }

                    if (o is MyBalanceListImporter) {
                        Remove(o as MyBalanceListImporter);
                    }
                    if (o is BalanceListImporter) {
                        var elem = this.FirstOrDefault(i => i.BalanceListImporter == o as BalanceListImporter);
                        if(elem != null)
                            Remove(elem);
                    }
                    if (o is int) {
                        RemoveAt((int)o);
                    }
                });
            }

            public string FileCount { get { return this.Count.ToString(System.Globalization.CultureInfo.InvariantCulture); } }
            public string AccountsCount { get { return this.Sum(entry => entry.BalanceListImporter.SummaryConfig.NumberOfAccounts).ToString(System.Globalization.CultureInfo.InvariantCulture); } }

            public string AccountsSum {
                get {
                    return
                        LocalisationUtils.CurrencyToString(this.Sum(entry => entry.BalanceListImporter.SummaryConfig.SumOfAccountsDecimal));
                }
            }

            public new void Add(MyBalanceListImporter item) {
                base.Add(item);
                FireOnPropertyChanged();
            }

            public new void Remove(MyBalanceListImporter item) {
                base.Remove(item);
                FireOnPropertyChanged();
            }

            public new void RemoveAt(int itemId) {
                base.RemoveAt(itemId);
                FireOnPropertyChanged();
            }

            private void FireOnPropertyChanged() {
                OnPropertyChanged(new PropertyChangedEventArgs("FileCount"));
                OnPropertyChanged(new PropertyChangedEventArgs("AccountsCount"));
                OnPropertyChanged(new PropertyChangedEventArgs("AccountsSum"));
            }

            public DelegateCommand CmdDeleteBalanceList { get; set; }
        }

        public class MyBalanceListImporter :NotifyPropertyChangedBase {
            
            public MyBalanceListImporter(BalanceListImporter balanceListImporter) { BalanceListImporter = balanceListImporter; }
            
            #region IsSelected
            private bool _isSelected;

            public bool IsSelected {
                get { return _isSelected; }
                set {
                    if (_isSelected != value) {
                        _isSelected = value;
                        OnPropertyChanged("IsSelected");
                    }
                }
            }
            #endregion IsSelected
            
            #region BalanceListImporter
            private BalanceListImporter _balanceListImporter;

            public BalanceListImporter BalanceListImporter {
                get { return _balanceListImporter; }
                set {
                    if (_balanceListImporter != value) {
                        _balanceListImporter = value;
                        OnPropertyChanged("BalanceListImporter");
                    }
                }
            }
            #endregion BalanceListImporter

        }

        private void UpdateModificationAllowed() {
            if (Owner.assistantControl.SelectedIndex == 1) {
                AllowEdit[ObjectTypes.System] = true;
                AllowEdit[ObjectTypes.Company] = false;
                AllowEdit[ObjectTypes.FinancialYear] = false;

                ShowObject[ObjectTypes.System] = true;
                ShowObject[ObjectTypes.Company] = false;
                ShowObject[ObjectTypes.FinancialYear] = false;

            }
            if (Owner.assistantControl.SelectedIndex == 3) {
                AllowEdit[ObjectTypes.System] = false;
                AllowEdit[ObjectTypes.Company] = true;
                AllowEdit[ObjectTypes.FinancialYear] = false;

                ShowObject[ObjectTypes.System] = true;
                ShowObject[ObjectTypes.Company] = true;
                ShowObject[ObjectTypes.FinancialYear] = false;
            }
            if (Owner.assistantControl.SelectedIndex == 7) {
                AllowEdit[ObjectTypes.System] = false;
                AllowEdit[ObjectTypes.Company] = false;
                AllowEdit[ObjectTypes.FinancialYear] = true;

                ShowObject[ObjectTypes.System] = true;
                ShowObject[ObjectTypes.Company] = true;
                ShowObject[ObjectTypes.FinancialYear] = true;

                //NewCompanyGenerated = false;
            }
            OnPropertyChanged("ShowObject");
            OnPropertyChanged("AllowEdit");
        }

        public bool SiteForReportGenerationReached {
            get { return Owner.assistantControl.SelectedIndex == Owner.assistantControl.Items.Count - 2; }
        }
        public bool LastPageReached { get { return Owner.assistantControl.SelectedIndex == Owner.assistantControl.Items.Count - 1; } }                   

        private void CheckVisibilityStuff() {
            if (Owner.assistantControl.SelectedIndex > 2 && !LastPageReached && Selection[ObjectTypes.System].IsCreateNewObjectSelected) Owner.assistantControl.SelectedIndex = 1;
            if (Owner.assistantControl.SelectedIndex > 5 && !LastPageReached && Selection[ObjectTypes.Company].IsCreateNewObjectSelected) Owner.assistantControl.SelectedIndex = 3;
            OnPropertyChanged("LastPageReached");
            OnPropertyChanged("SiteForReportGenerationReached");
            UpdateModificationAllowed();
        }


        #region InsertCompanyDetails
        private bool _insertCompanyDetails;

        public bool InsertCompanyDetails {
            get { return _insertCompanyDetails && (!DontAskAgain); }
            set {
                if (_insertCompanyDetails != value) {
                    _insertCompanyDetails = value;
                    if (_insertCompanyDetails) {
                        Owner.ctlCompanyDetails.DataContext = Selection[ObjectTypes.Company].SelectedObject;
                    }
                    OnPropertyChanged("InsertCompanyDetails");
                }
            }
        }
        #endregion InsertCompanyDetails

        #region AllowEdit
        private Dictionary<ObjectTypes, bool> _allowEdit;

        public Dictionary<ObjectTypes, bool> AllowEdit {
            get { return _allowEdit; }
            set {
                if (_allowEdit != value) {
                    _allowEdit = value;
                    OnPropertyChanged("AllowEdit");
                }
            }
        }
        #endregion AllowEdit
        
        #region ShowObject
        private Dictionary<ObjectTypes, bool> _showObject;

        public Dictionary<ObjectTypes, bool> ShowObject {
            get { return _showObject; }
            set {
                if (_showObject != value) {
                    _showObject = value;
                    OnPropertyChanged("ShowObject");
                }
            }
        }
        #endregion ShowObject

        #region NewCompanyGenerated
        private bool _newCompanyGenerated;

        public bool NewCompanyGenerated {
            get { return _newCompanyGenerated; }
            set {
                if (_newCompanyGenerated != value) {
                    _newCompanyGenerated = value;
                    OnPropertyChanged("NewCompanyGenerated");
                }
            }
        }
        #endregion NewCompanyGenerated

        private bool GenerateObject(ObjectTypes objectType) {

            switch (objectType) {
                case ObjectTypes.Company:
                    try {
                        if (string.IsNullOrEmpty(Create[objectType].Name))
                            return false;
                        bool createNew = true;
                        var existingCompany = eBalanceKitBusiness.Manager.CompanyManager.Instance.Companies.Where(comp => !string.IsNullOrEmpty(comp.Name) && comp.Name.ToLower().Equals(Create[objectType].Name.ToLower())).ToList();
                        var existingAllowedCompany = eBalanceKitBusiness.Manager.CompanyManager.Instance.AllowedCompanies.Where(comp => !string.IsNullOrEmpty(comp.Name) && comp.Name.ToLower().Equals(Create[objectType].Name.ToLower())).ToList();

                        if (existingCompany.Any()) {
                            var msg = string.Format(ResourcesManamgement.MsgExistingObject, ResourcesMain.Company);

                            if (!existingAllowedCompany.Any()) {
                                msg += Environment.NewLine +
                                       string.Format(ResourcesManamgement.MsgNoRightsFor, ResourcesMain.Company);
                            }
                            msg += Environment.NewLine +
                                   string.Format(ResourcesManamgement.QNewSameName, ResourcesMain.Company);
                            var msgBox = MessageBox.Show(msg,
                                                         string.Format(ResourcesManamgement.HeaderFoundIdentical,
                                                                       ResourcesMain.Company), MessageBoxButton.YesNo,
                                                         MessageBoxImage.Question, MessageBoxResult.No);

                            createNew = msgBox == MessageBoxResult.Yes;

                            if (!createNew && existingAllowedCompany.Any()) {
                                Selection[objectType].SelectedObject = existingAllowedCompany.First();
                                Create[ObjectTypes.Company].Name = string.Empty;
                            }
                        }

                            if (!createNew) {
                                ////Owner.assistantControl.SelectedIndex = 3;
                                //while (Owner.assistantControl.SelectedIndex > 3) {
                                //    Console.WriteLine(Owner.assistantControl.SelectedIndex);
                                //    Owner.assistantControl.NavigateBack();
                                //}
                                NewCompanyGenerated = false;
                                return false;
                            }

                        var company = new Company();
                        if (!string.IsNullOrEmpty(Create[objectType].Name)) company.Name = Create[objectType].Name;
                        eBalanceKitBusiness.Manager.CompanyManager.Instance.AddCompany(company);
                        company.SetFinancialYearIntervall(2009, 2030);
                        company.SetVisibleFinancialYearIntervall(DateTime.Now.Year, DateTime.Now.Year);
                        Selection[objectType].AvailableObjects =
                            eBalanceKitBusiness.Manager.CompanyManager.Instance.AllowedCompanies.OrderBy(c => c.Name);
                        Selection[objectType].SelectedObject = company;
                        AskingModels[ObjectTypes.Company] = new AskingCompanyModel(Owner, company); 
                        OnPropertyChanged("AskingModels");
                        NewCompanyGenerated = true;
                        Create[objectType].Name = string.Empty;
                        ShowCompanyNameCommentSite = false;
                    }
                    catch (Exception ex) {
                        MessageBox.Show(ex.Message, ResourcesCommon.ErrorsCaption, MessageBoxButton.OK,
                                        MessageBoxImage.Error);
                        ExceptionLogging.LogException(ex);
                        return false;
                    }
                    return true;
                case ObjectTypes.System:
                    try {
                        bool createNew = true;
                        var existingSystem = eBalanceKitBusiness.Manager.SystemManager.Instance.Systems.Where(s => !string.IsNullOrEmpty(s.Name) && s.Name.ToLower().Equals(Create[objectType].Name.ToLower())).ToList();
                        //var existingAllowedCompany = eBalanceKitBusiness.Manager.SystemManager.Instance.AllowedCompanies.Where(comp => !string.IsNullOrEmpty(comp.Name) && comp.Name.ToLower().Equals(Create[objectType].Name.ToLower())).ToList();


                        if (existingSystem.Any()) {
                            var msg = string.Format(ResourcesManamgement.MsgExistingObject, ResourcesMain.System);

                            //if (!existingAllowedCompany.Any()) {
                            //    msg += Environment.NewLine + string.Format(ResourcesManamgement.MsgNoRightsFor, ResourcesMain.System);
                            //}

                            //msg += Environment.NewLine + string.Format(ResourcesManamgement.QNewSameName, ResourcesMain.System);
                            //var msgBox = MessageBox.Show(msg, string.Format(ResourcesManamgement.HeaderFoundIdentical, ResourcesMain.System), MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
                            var msgBox = MessageBox.Show(msg, string.Format(ResourcesManamgement.HeaderFoundIdentical, ResourcesMain.System), MessageBoxButton.OK, MessageBoxImage.Exclamation, MessageBoxResult.No);

                            createNew = msgBox == MessageBoxResult.Yes;

                            if (!createNew) {
                                Selection[objectType].SelectedObject = existingSystem.First();
                            }
                        }
                        if (!createNew) {
                            //Owner.assistantControl.SelectedIndex = 1;
                            //while (Owner.assistantControl.SelectedIndex > 1) {
                            //    Console.WriteLine(Owner.assistantControl.SelectedIndex);
                            //    Owner.assistantControl.NavigateBack();
                            //}
                                
                            return false;
                        }

                        var system = new eBalanceKitBusiness.Structures.DbMapping.System(){Name = Create[objectType].Name, Comment = Create[objectType].Comment };
                        eBalanceKitBusiness.Manager.SystemManager.Instance.AddSystem(system);
                        
                        Selection[objectType].AvailableObjects =
                            eBalanceKitBusiness.Manager.SystemManager.Instance.Systems.OrderBy(s => s.Name);
                        Selection[objectType].SelectedObject = system;
                    }
                    catch (Exception ex) {
                        MessageBox.Show(ex.Message, ResourcesCommon.ErrorsCaption, MessageBoxButton.OK,
                                        MessageBoxImage.Error);
                        ExceptionLogging.LogException(ex);
                        return false;
                    }
                    return true;
                case ObjectTypes.FinancialYear:
                    try {
                        var selectedCompany = SelectedObject[ObjectTypes.Company] as Company;
                        var selectedFinancialYear = SelectedObject[ObjectTypes.FinancialYear] as FinancialYear;

                        if (selectedCompany == null || selectedFinancialYear == null) {
                            Debug.Fail("SelectedObject[ObjectTypes.Company] as Company OR SelectedObject[ObjectTypes.FinancialYear] as FinancialYear IS EMPTY");
                            return false;
                        }

                        var allowed = eBalanceKitBusiness.Manager.RightManager.RightDeducer.HasFinancialYearWriteRight(selectedFinancialYear);

                        if (!selectedCompany.VisibleFinancialYears.Any(fy => fy.FYear == selectedFinancialYear.FYear)) {

                            var visibleFinancialYears = selectedCompany.VisibleFinancialYears.OrderBy(fy => fy.FYear).ToList();

                            var from = (visibleFinancialYears.First().FYear < selectedFinancialYear.FYear)
                                           ? visibleFinancialYears.First().FYear
                                           : selectedFinancialYear.FYear;

                            var to = (visibleFinancialYears.Last().FYear > selectedFinancialYear.FYear)
                                         ? visibleFinancialYears.Last().FYear
                                         : selectedFinancialYear.FYear;



                            allowed &= eBalanceKitBusiness.Manager.RightManager.RightDeducer.CompanyEditable(selectedCompany);
                            if (allowed) {
                                selectedCompany.SetVisibleFinancialYearIntervall(from, to);
                            }
                        }

                        if (!allowed) {
                            MessageBox.Show(string.Format(ResourcesManamgement.InsufficentRightsForSelected, ResourcesMain.FinancialYear), ResourcesCommon.InsufficientRights, MessageBoxButton.OK, MessageBoxImage.Error);
                            Owner.assistantControl.NavigateBack();
                        }
                        
                        return allowed;
                    }
                    catch (Exception ex) {
                        MessageBox.Show(ex.Message, ResourcesCommon.ErrorsCaption, MessageBoxButton.OK,
                                        MessageBoxImage.Error);
                        ExceptionLogging.LogException(ex);
                        return false;
                    }
            }
            return false;
        }

        public Dictionary<ObjectTypes, SelectObjectModel> Selection { get; set; }
        public Dictionary<ObjectTypes, Add.Models.AddObjectModel> Create { get; set; }
        public Dictionary<ObjectTypes, object> SelectedObject { get; set; }

        public DelegateCommand CmdObjectSelected { get; set; }
        public DelegateCommand CmdCreateObject { get; set; }
        public DelegateCommand CmdDeleteBalanceList { get; set; }
        public DelegateCommand CmdRefreshVisibility { get; set; }
        public DelegateCommand CmdGoBackToStart { get; set; }

        private void SetDocumentInfo() {
            
            var newDocument = AddReportModel.Result as Document;
            if (newDocument == null) {
                //System.Diagnostics.Debug.Fail("Selected Document is null");
                AddReportModel.GenerateReport();
                newDocument = AddReportModel.Result as Document;
                //return;
            }

            if (newDocument == null) return;

            //if (BalanceListImporter == null) {
            //    //System.Diagnostics.Debug.Fail("BalanceListImporter is null");

            //    return;
            //}
            newDocument.System = Selection[ObjectTypes.System].SelectedObject as eBalanceKitBusiness.Structures.DbMapping.System;
            newDocument.Company = Selection[ObjectTypes.Company].SelectedObject as Company;
            newDocument.FinancialYear = Selection[ObjectTypes.FinancialYear].SelectedObject as FinancialYear ?? AvailableFinancialYears.First();


            newDocument.ValueTreeGcd.Root.Values["de-gcd_genInfo.report.period.fiscalYearBegin"].Value =
                newDocument.FinancialYear.FiscalYearBegin;
            newDocument.ValueTreeGcd.Root.Values["de-gcd_genInfo.report.period.fiscalYearEnd"].Value =
                newDocument.FinancialYear.FiscalYearEnd;
            newDocument.ValueTreeGcd.Root.Values["de-gcd_genInfo.report.period.fiscalPreviousYearBegin"].Value =
                newDocument.FinancialYear.FiscalYearBeginPrevious;
            newDocument.ValueTreeGcd.Root.Values["de-gcd_genInfo.report.period.fiscalPreviousYearEnd"].Value =
                newDocument.FinancialYear.FiscalYearEndPrevious;
            newDocument.ValueTreeGcd.Root.Values["de-gcd_genInfo.report.period.balSheetClosingDate"].Value =
                newDocument.FinancialYear.BalSheetClosingDate;
            newDocument.ValueTreeGcd.Root.Values["de-gcd_genInfo.report.period.balSheetClosingDatePreviousYear"].Value =
                newDocument.FinancialYear.BalSheetClosingDatePreviousYear;

            eBalanceKitBusiness.Manager.DocumentManager.Instance.SaveDocument(newDocument);

            if (BalanceListImporter != null && BalanceListImporter.Any()) {
                foreach (var balanceListImporter in BalanceListImporter) {
                    balanceListImporter.BalanceListImporter.Import(newDocument);
                    
                }
            }
            GoForward();
        }

        public AskingRepeatModel RepeatModel { get; set; }

        /// <summary>
        /// The owner window. Should be DlgManagementAssistant.
        /// </summary>
        private DlgManagementAssistant Owner { get; set; }

        //#region SystemSelected
        //public bool SystemSelected {
        //    get { return SelectedSystem != null; }
        //}
        //#endregion SystemSelected

        //#region CompanySelected
        //public bool CompanySelected {
        //    get { return SelectedCompany != null; }
        //}
        //#endregion CompanySelected

        //#region FinancialYearSelected
        //public bool FinancialYearSelected {
        //    get { return SelectedFinancialYear != null; }
        //}
        //#endregion FinancialYearSelected

        #region BalanceListImported
        #region BalanceListImported
        private bool _balanceListImported;

        public bool BalanceListImported {
            get { return _balanceListImported; }
            set {
                if (_balanceListImported != value) {
                    _balanceListImported = value;
                    OnPropertyChanged("BalanceListImported");
                }
            }
        }
        #endregion BalanceListImported
        public bool BalanceListSelected { get { return BalanceListImporter != null; } }
        #endregion BalanceListImported

        //#region ReportSelected
        //public bool ReportSelected {
        //    get { return SelectedDocument != null; }
        //}
        //#endregion ReportSelected

        public DelegateCommand CmdOk { get; set; }
        //public DelegateCommand CmdReset { get; set; }

        public Dictionary<ObjectTypes, IAskingModel> AskingModels { get; set; }
        public AskingBalanceListImportModel BalanceListImportModel { get; set; }
        //public AskingBalanceListTestlistModel BalanceListTestlistModel { get; set; }

        //public eBalanceKitBusiness.Structures.DbMapping.System SelectedSystem { get { return AskingModels[ObjectTypes.System].Result as eBalanceKitBusiness.Structures.DbMapping.System; } }

        //public Company SelectedCompany { get { return AskingModels[ObjectTypes.Company].Result as Company; } }

        //public Document SelectedDocument { get { return AskingModels[ObjectTypes.Report] == null ? null : AskingModels[ObjectTypes.Report].Result as Document; } }

        public bool NextAllowed { get; set; }

        public BalanceListImporterCollection BalanceListImporter { get; private set; }

        #region SelectedFinancialYear
        private FinancialYear _selectedFinancialYear;

        public FinancialYear SelectedFinancialYear {
            get { return _selectedFinancialYear; }
            set {
                if (_selectedFinancialYear != value) {
                    _selectedFinancialYear = value;
                    OnPropertyChanged("SelectedFinancialYear");
                }
            }
        }
        #endregion SelectedFinancialYear

        ///// <summary>
        ///// Save the financial year information for the <see cref="SelectedCompany"/> if allowed.
        ///// </summary>
        ///// <returns>Returns if the user has WriteRights for this financial year or the right to add this year.</returns>
        //public bool SetFinancialYear() {

        //    var allowed = eBalanceKitBusiness.Manager.RightManager.RightDeducer.HasFinancialYearWriteRight(SelectedFinancialYear);

        //    if (!SelectedCompany.VisibleFinancialYears.Any(fy => fy.FYear == SelectedFinancialYear.FYear)) {

        //        var visibleFinancialYears = SelectedCompany.VisibleFinancialYears.OrderBy(fy => fy.FYear);

        //        var from = (visibleFinancialYears.First().FYear < SelectedFinancialYear.FYear)
        //                       ? visibleFinancialYears.First().FYear
        //                       : SelectedFinancialYear.FYear;

        //        var to = (visibleFinancialYears.Last().FYear > SelectedFinancialYear.FYear)
        //                     ? visibleFinancialYears.Last().FYear
        //                     : SelectedFinancialYear.FYear;



        //        allowed &= eBalanceKitBusiness.Manager.RightManager.RightDeducer.CompanyEditable(SelectedCompany);
        //        if (allowed) {
        //            SelectedCompany.SetVisibleFinancialYearIntervall(from, to);
        //        }
        //    }

        //    if (allowed) {
        //        AvailableDocuments = eBalanceKitBusiness.Manager.DocumentManager.
        //                                    Instance.AllowedDocuments.Where(
        //                                        doc =>
        //                                        doc.FinancialYear ==
        //                                        SelectedFinancialYear &&
        //                                        doc.Company == SelectedCompany).ToList();
        //        AskingModels[ObjectTypes.Report] = new AskingDocumentModel(Owner,
        //                                                                   AvailableDocuments){AddObjectModel = new AddObjectModel(Owner, ObjectTypes.Report)};

        //        AskingModels[ObjectTypes.Report].PropertyChanged += (sender, args) => FirePropertyChanged();
        //        AskingModels[ObjectTypes.Report].PropertyChanged += (sender, args) => SetDocumentInfo();
        //        OnPropertyChanged("AskingModels[Report]");
        //        OnPropertyChanged("AskingModels");
        //        OnPropertyChanged("AskingModels[ObjectTypes.Report]");
        //    }
            
        //    return allowed;
        //}

        //#region AvailableDocuments
        //private IEnumerable<Document> _availableDocuments;

        //public IEnumerable<Document> AvailableDocuments {
        //    get { return _availableDocuments; }
        //    set {
        //        if (_availableDocuments != value) {
        //            _availableDocuments = value;
        //            OnPropertyChanged("AvailableDocuments");
        //        }
        //    }
        //}
        //#endregion AvailableDocuments

        /// <summary>
        /// List of financial years starting 2010 up to current year + 1. Returns empty list as long as no <see cref="SelectedCompany"/>.
        /// </summary>
        public IEnumerable<FinancialYear> AvailableFinancialYears {
            get {
                List<FinancialYear> result = new List<FinancialYear>();

                if (Selection == null || !Selection.ContainsKey(ObjectTypes.Company)) {
                    return result;
                }
                var selectedCompany = Selection[ObjectTypes.Company].SelectedObject as Company;
                var year = 2010;

                if (selectedCompany == null || selectedCompany.Id == 0 || selectedCompany.FinancialYears == null) {
                    return result;
                }

                result = selectedCompany.FinancialYears.Where(fy => fy.FYear >= year && fy.FYear <= DateTime.UtcNow.Year + 1).OrderBy(fy => fy.FYear).ToList();
                //SelectedFinancialYear = selectedCompany.FinancialYears.First(fy => fy.FYear == DateTime.UtcNow.Year);
                //Selection[ObjectTypes.FinancialYear] = new SelectObjectModel(result, ObjectTypes.FinancialYear);
                OnPropertyChanged("Selection");
                return result;
            }
        }

        private void FirePropertyChanged() {
            OnPropertyChanged("SelectedSystem");
            OnPropertyChanged("SelectedCompany");
            OnPropertyChanged("SelectedFinancialYear");
            OnPropertyChanged("AvailableFinancialYears"); 
            OnPropertyChanged("SelectedDocument");
            OnPropertyChanged("SystemSelected");
            OnPropertyChanged("FinancialYearSelected");
            OnPropertyChanged("CompanySelected");
            OnPropertyChanged("BalanceListImported");
            OnPropertyChanged("BalanceListSelected");
            OnPropertyChanged("ReportSelected");
        }

        #region Helper methods
        /// <summary>
        /// Shows a window with the Height and Width of <see cref="Owner"/> as a dialog. Hides <see cref="Owner"/> while the dialog is shown. Returns the dialog result.
        /// </summary>
        /// <param name="dlg">The window that should be shown as dialog.</param>
        /// <returns>The dialog result.</returns>
        protected bool? ShowWindow(Window dlg) {
            dlg.Height = Owner.Height;
            dlg.Width = Owner.Width;
            dlg.Owner = Owner;
            dlg.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            Owner.Hide();

            var result = dlg.ShowDialog();

            Owner.Show();

            return result;
        }
        /// <summary>
        /// Navigates to the next page in an <see cref="AvdWpfControls.AssistantControl"/> by calling the NavigateNext() method.
        /// </summary>
        protected void GoForward() {
            var dlgManagementAssistant = Owner as DlgManagementAssistant;
            if (dlgManagementAssistant != null)
                dlgManagementAssistant.assistantControl.NavigateNext();
        }
        #endregion

        //public void Reset() {
        //    foreach (var askingModel in AskingModels.Values) {
        //        if (askingModel != null) {
        //            askingModel.Result = null;
        //        }
        //    }
        //    BalanceListImported = false;
        //    BalanceListImportModel.Result = null;
        //    BalanceListTestlistModel.Result = null;
        //    BalanceListImporter = null;
        //    FirePropertyChanged();
        //}
    }

}