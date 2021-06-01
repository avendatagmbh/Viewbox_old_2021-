// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2010-12-28
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using DbAccess;
using Taxonomy;
using Taxonomy.Enums;
using Taxonomy.Interfaces;
using Taxonomy.Interfaces.PresentationTree;
using Utils;
using eBalanceKitBase.Structures;
using eBalanceKitBusiness.AuditCorrections;
using eBalanceKitBusiness.EventArgs;
using eBalanceKitBusiness.Exceptions;
using eBalanceKitBusiness.HyperCubes.Interfaces.Structure;
using eBalanceKitBusiness.HyperCubes.Structures;
using eBalanceKitBusiness.Interfaces;
using eBalanceKitBusiness.Localisation;
using eBalanceKitBusiness.Logs;
using eBalanceKitBusiness.Manager;
using eBalanceKitBusiness.Reconciliation;
using eBalanceKitBusiness.Reconciliation.Enums;
using eBalanceKitBusiness.Reconciliation.Interfaces;
using eBalanceKitBusiness.Rights;
using eBalanceKitBusiness.Structures.DbMapping.ValueMappings;
using eBalanceKitBusiness.Structures.Presentation;
using eBalanceKitBusiness.Structures.XbrlElementValue;
using eBalanceKitBusiness.Validators;
using eBalanceKitResources.Localisation;

namespace eBalanceKitBusiness.Structures.DbMapping {
    /// <summary>
    /// This class represents an instance document.
    /// </summary>
    [DbTable("documents", ForceInnoDb = true)]
    public class Document : NotifyPropertyChangedBase, IComparable, ILoggableObject, ITaxonomyIdManagerProvider, IReferenceListManipulator, INamedObject {
        
        private SemaphoreSlim semaphore = new SemaphoreSlim(1);

        public Document() {
            _items.CollectionChanged += (sender, args) => {
                OnPropertyChanged("BalanceLists");
                OnPropertyChanged("BalanceListsImported");
                OnPropertyChanged("BalanceListsVisible");
            };
        }

        #region events

        #region NewLog
        public event LogHandler NewLog;
        private void OnNewLog(string element, object oldValue, object newValue) { if (NewLog != null) NewLog(this, new LogArgs(element, oldValue, newValue)); }
        #endregion NewLog

        #region AssignedTaxonomyInfoChanged
        public event EventHandler<AssignedTaxonomyInfoChangedEventArgs> AssignedTaxonomyInfoChanged;

        private void OnAssignedTaxonomyChanged(ITaxonomyInfo oldValue, ITaxonomyInfo newValue) {
            if (AssignedTaxonomyInfoChanged != null)
                AssignedTaxonomyInfoChanged(this, new AssignedTaxonomyInfoChangedEventArgs(oldValue, newValue));
        }
        #endregion AssignedTaxonomyInfoChanged

        #region HyperCubeCollectionChanged
        public event EventHandler HyperCubeCollectionChanged;
        public void OnHyperCubeCollectionChanged() { if (HyperCubeCollectionChanged != null) HyperCubeCollectionChanged(this, new global::System.EventArgs()); }
        #endregion HyperCubeCollectionChanged

        public void OnAllowedOwnersChanged() {
            OnPropertyChanged("Owner");
        }

        #endregion events

        #region properties

        #region Id
        private int _id;

        [DbColumn("id", AllowDbNull = false)]
        [DbPrimaryKey]
        public int Id {
            get { return _id; }
            set {
                if (_id == value) return;
                _id = value;
                OnPropertyChanged("Id");
            }
        }
        #endregion

        #region Name
        private string _name = ResourcesCommon.DefaultReportName;

        [DbColumn("name", Length = 256, AllowDbNull = false)]
        public string Name {
            get { return _name; }
            set {
                if (_name == value) return;

                // log changed value
                OnNewLog("Name", _name, value);

                _name = value;
                OnPropertyChanged("Name");
            }
        }
        #endregion

        #region Comment
        private string _comment;

        [DbColumn("comment", Length = 1024, AllowDbNull = true)]
        public string Comment {
            get { return _comment; }
            set {
                if (_comment == value) return;

                // log changed value
                OnNewLog("Comment", _comment, value);

                _comment = value;
                OnPropertyChanged("Comment");
            }
        }
        #endregion

        #region Company
        private int _companyId;
        private Company _company;

        [DbColumn("company_id")]
        public int CompanyId {
            get { return _companyId; }
            set {
                if (_companyId == value) return;

                // log changed value (assigned company)
                OnNewLog("CompanyId", _companyId, value);

                _companyId = value;
            }
        }

        public Company Company {
            get { return _company; }
            set {
                if (_company == value) return;

                var oldValue = value;                
                
                OnPropertyChanging("Company");
                _company = value;
                CompanyId = value.Id;
                OnPropertyChanged("Company");

                if (oldValue != null) oldValue.NotifyAssignedReportChanged();
                if (value != null) value.NotifyAssignedReportChanged();
                CompanyManager.Instance.NotifyAssignedReportChanged();
            }
        }
        #endregion Company

        #region System
        private int _systemId;
        private System _system;

        [DbColumn("system_id")]
        public int SystemId {
            get { return _systemId; }
            set {
                if (_systemId == value) return;

                // log changed value (assigned system)
                OnNewLog("SystemId", _systemId, value);

                _systemId = value;
            }
        }

        public System System {
            get { return _system; }
            set {
                if (_system == value) return;

                var oldValue = value;
                
                OnPropertyChanging("System");
                _system = value;
                SystemId = value.Id;
                OnPropertyChanged("System");

                if (oldValue != null) oldValue.NotifyAssignedReportChanged();
                if (value != null) value.NotifyAssignedReportChanged();
                SystemManager.Instance.NotifyAssignedReportChanged();

            }
        }

        #endregion System

        #region FinancialYear
        private int _financialYearId;
        private FinancialYear _financialYear;

        [DbColumn("financial_year_id")]
        public int FinancialYearId {
            get { return _financialYearId; }
            set {
                if (_financialYearId == value) return;

                // log changed value (assigned finacial year)
                OnNewLog("FinancialYearId", _financialYearId, value);

                _financialYearId = value;
            }
        }

        public FinancialYear FinancialYear {
            get { return _financialYear; }
            set {
                if (_financialYear == value) return;
                OnPropertyChanging("FinancialYear");
                //if (_financialYear != null) _financialYear.PropertyChanged -= FinancialYearPropertyChanged;
                _financialYear = value;
                FinancialYearId = value == null ? 0 : value.Id;
                //if (_financialYear != null) _financialYear.PropertyChanged += FinancialYearPropertyChanged;
                OnPropertyChanged("FinancialYear");
            }
        }

        //private void FinancialYearPropertyChanged(object sender, PropertyChangedEventArgs e) { SetFinancialYearValues(); }
        #endregion FinancialYear

        #region Owner
        private int _ownerId;
        private User _owner;

        [DbColumn("owner_id")]
        public int OwnerId {
            get { return _ownerId; }
            set {
                if (_ownerId == value) return;

                // log ownerId changes (assigned owner)
                OnNewLog("OwnerId", _ownerId, value);

                _ownerId = value;
            }
        }

        public User Owner {
            get { return _owner; }
            set {
                if (_owner == value || value == null) return;
                _owner = value;
                OwnerId = value.Id;
                OnPropertyChanged("Owner");
                OnPropertyChanged("AllowEditOwner");

                DocumentManager.Instance.SaveDocument(this);
            }
        }
        #endregion Owner

        #region AllowedOwners
        public ObservableCollection<User> AllowedOwners { get { return UserManager.Instance.ActiveUsers; } }
        #endregion AllowedOwners

        #region assigned taxonomies

        #region GcdTaxonomyInfoId
        private int _gcdTaxonomyInfoId;

        [DbColumn("gcd_taxonomy_info_id")]
        public int GcdTaxonomyInfoId {
            get { return _gcdTaxonomyInfoId; }
            set {
                if (_gcdTaxonomyInfoId == value) return;

                // log changed value (assigned GCD taxonomy)
                OnNewLog("GcdTaxonomyInfoId", _gcdTaxonomyInfoId, value);

                _gcdTaxonomyInfoId = value;
            }
        }
        #endregion

        #region GcdTaxonomyInfo
        private ITaxonomyInfo _gcdTaxonomyInfo;

        public ITaxonomyInfo GcdTaxonomyInfo {
            get { return _gcdTaxonomyInfo; }
            set {
                if (_gcdTaxonomyInfo == value) return;
                ITaxonomyInfo oldValue = _gcdTaxonomyInfo;
                _gcdTaxonomyInfo = value;
                GcdTaxonomyInfoId = ((TaxonomyInfo) value).Id;

                if (DoDbUpdate) DocumentManager.Instance.SaveDocument(this);

                // reset _taxonomyIdManager
                _taxonomyIdManager = null;

                OnPropertyChanged("GcdTaxonomyInfo");
                OnAssignedTaxonomyChanged(oldValue, value);
            }
        }
        #endregion

        #region GcdTaxonomy
        public ITaxonomy GcdTaxonomy { get { return TaxonomyManager.GetTaxonomy(GcdTaxonomyInfo); } }
        #endregion

        #region MainTaxonomyInfoId
        private int _mainTaxonomyInfoId;

        [DbColumn("main_taxonomy_info_id")]
        public int MainTaxonomyInfoId {
            get { return _mainTaxonomyInfoId; }
            set {
                if (_mainTaxonomyInfoId == value) return;

                // log changed value (assigned main taxonomy (GAAP, BRA, FI, IN))
                OnNewLog("TaxonomyInfoId", _mainTaxonomyInfoId, value);

                _mainTaxonomyInfoId = value;
            }
        }
        #endregion

        #region MainTaxonomyInfo
        private ITaxonomyInfo _mainTaxonomyInfo;

        public ITaxonomyInfo MainTaxonomyInfo {
            get { return _mainTaxonomyInfo; }
            internal set {
                if (_mainTaxonomyInfo == value) return;
                ITaxonomyInfo oldValue = _mainTaxonomyInfo;
                _mainTaxonomyInfo = value;
                MainTaxonomyInfoId = ((TaxonomyInfo) value).Id;

                if (DoDbUpdate) DocumentManager.Instance.SaveDocument(this);

                // reset _taxonomyIdManager
                _taxonomyIdManager = null;

                OnPropertyChanged("MainTaxonomyInfo");
                OnAssignedTaxonomyChanged(oldValue, value);
            }
        }
        #endregion MainTaxonomyInfo

        #region MainTaxonomyKind
        private TaxonomySubType _mainTaxonomyKind;

        public TaxonomySubType MainTaxonomyKind {
            get { return _mainTaxonomyKind; }
            set {
                if (_mainTaxonomyKind != value) {
                    _mainTaxonomyKind = value;
                    OnPropertyChanged("MainTaxonomyKind");
                }
            }
        }
        #endregion MainTaxonomyKind

        #region MainTaxonomy
        public ITaxonomy MainTaxonomy { get { return TaxonomyManager.GetTaxonomy(MainTaxonomyInfo); } }
        #endregion

        #endregion assigned taxonomies

        #region CreationDate
        [DbColumn("creation_date")]
        public DateTime CreationDate { get; set; }
        #endregion

        #region TaxonomyIdManager
        private TaxonomyIdManager _taxonomyIdManager;

        public TaxonomyIdManager TaxonomyIdManager {
            get {
                return _taxonomyIdManager ??
                       (_taxonomyIdManager = new TaxonomyIdManager(GcdTaxonomyInfo, MainTaxonomyInfo));
            }
        }
        #endregion

        #region BalanceList
        private readonly ObservableCollectionAsync<IBalanceList> _items = new ObservableCollectionAsync<IBalanceList>();
        public ObservableCollectionAsync<IBalanceList> BalanceListsVisible {
            get {
                //Prevents enumeration changed exception
                var items = _items.ToList();
                var relevantBalanceLists = items.Where(x => !x.IsHidden).OrderByDescending(bl => bl.IsImported).ThenBy(bl => bl.Name).ToList();
                var res = new ObservableCollectionAsync<IBalanceList>();
                foreach (var balanceList in relevantBalanceLists) {
                    res.Add(balanceList);
                }
                return res;
            }
        }

        private bool _balanceListsLoaded = false;
        public ObservableCollectionAsync<IBalanceList> BalanceLists {
            get { 
                if (!_balanceListsLoaded) {
                    semaphore.Wait();
                    try {
                        if (!_balanceListsLoaded) {
                            LoadBalanceListsFromDataBase();
                            _balanceListsLoaded = true;
                        }
                    } finally {
                        semaphore.Release();
                    }
                }
                return _items;
            }
        }

        public ObservableCollectionAsync<IBalanceList> BalanceListsImported {
            get {
                var relevantBalanceLists = _items.Where(x => x.IsImported).ToList();
                var res = new ObservableCollectionAsync<IBalanceList>();
                foreach (var balanceList in relevantBalanceLists) {
                    res.Add(balanceList);
                }
                return res;
            }
        }
        #endregion

        #region SelectedBalanceList
        private IBalanceList _selectedBalanceList;

        public IBalanceList SelectedBalanceList {
            get {
                if (_selectedBalanceList == null && BalanceListsVisible.Count > 0) {
                    _selectedBalanceList = BalanceListsVisible[0];
                    _selectedBalanceList.IsDocumentSelectedBalanceList = true;
                }

                return _selectedBalanceList;
            }
            set {
                if (_selectedBalanceList == value) return;
                if (_selectedBalanceList != null) _selectedBalanceList.IsDocumentSelectedBalanceList = false;
                _selectedBalanceList = value;
                if (_selectedBalanceList != null) _selectedBalanceList.IsDocumentSelectedBalanceList = true;
                OnPropertyChanged("SelectedBalanceList");
                OnPropertyChanged("BalanceListsVisible");
            }
        }
        #endregion

        #region ValueTreeGcd
        private ValueTree.ValueTree _valueTreeGcd;

        public ValueTree.ValueTree ValueTreeGcd {
            get { return _valueTreeGcd; }
            private set {
                if (_valueTreeGcd != null) RemoveGcdEventHandler();
                _valueTreeGcd = value;
                if (_valueTreeGcd == null) return;

                InitValueTreeGcd();

                OnPropertyChanged("IsCommercialBalanceSheet");
                OnPropertyChanged("SelectedSpecialBalanceItem");
                OnPropertyChanged("ValueTreeGcd");
            }
        }

        #region InitValueTreeGcd
        private void InitValueTreeGcd() {
            ValidatorGCD = new ValidatorGCD(this);
            ValidatorGCD_Company = new ValidatorGCD_Company(this);
            AddGcdEventHandler();
            //SetFinancialYearValues();
            
            _gcdPresentationTrees = PresentationTree.CreatePresentationTrees(GcdTaxonomy, ValueTreeGcd, null);
            GcdPresentationTreeNodes = new Dictionary<string, IPresentationTreeNode>();

            if (_gcdPresentationTrees.Any()) {
                GcdPresentationTree = _gcdPresentationTrees.Values.FirstOrDefault();

                if (GcdPresentationTree != null) {
                    foreach (var node in GcdPresentationTree.Nodes) {
                        GcdPresentationTreeNodes.Add(node.Element.Id, node);
                    }
                }
            }

            // set default choice, if no choice has been selected
            var val = GetGcdSingleChoiceValue("de-gcd_genInfo.report.id.consolidationRange");
            if (val != null) val.SelectedValue = val.Elements.First();
        }
        #endregion InitGcd


        private Dictionary<string, IPresentationTree> _gcdPresentationTrees;

        public Dictionary<string, IPresentationTreeNode> GcdPresentationTreeNodes { get; set; }

        public IPresentationTree GcdPresentationTree { get; private set; }


        #region AddGcdEventHandler
        private void AddGcdEventHandler() {
            // add event handler for changes of accountingStandard (Bilanzierungsstandard (Handelsbilanz / Steuerbilanz))
            {
                var value = GetGcdSingleChoiceValue("de-gcd_genInfo.report.id.accountingStandard");
                if (value != null) value.PropertyChanged += AccountingStandardPropertyChanged;
            }
        }
        #endregion AddGcdEventHandler

        #region RemoveGcdEventHandler
        private void RemoveGcdEventHandler() {
            // add event handler for changes of accountingStandard (Bilanzierungsstandard (Handelsbilanz / Steuerbilanz))
            {
                var value = GetGcdSingleChoiceValue("de-gcd_genInfo.report.id.accountingStandard");
                if (value != null) value.PropertyChanged -= AccountingStandardPropertyChanged;
            }
        }
        #endregion RemoveGcdEventHandler

        #region GetGcdSingleChoiceValue
        /// <summary>
        /// Returns the SingleChoice element with the specified elementName or null, if the element could not be found or is not a SingleChoice value.
        /// In debug mode a new exception will be thrown instead of returning a null value.
        /// </summary>
        private XbrlElementValue_SingleChoice GetGcdSingleChoiceValue(string elementName) {
            var value = _valueTreeGcd.GetValue(elementName);

#if DEBUG
            if (value == null)
                throw new Exception(
                    "Could not find element \"" + elementName + "\" in gcd taxonomy.");
#endif

            var val = value as XbrlElementValue_SingleChoice;

#if DEBUG
            if (val == null)
                throw new Exception(
                    "Element \"" + elementName +
                    "\" does not belong to the expected value type (XbrlElementValue_SingleChoice).");
#endif

            return val;
        }
        #endregion GetGcdSingleChoiceValue

        #region GetValueTreeMainDecimalValue
        /// <summary>
        /// Returns the DecimalValue for the specified entry in ValueTreeMain.
        /// In debug mode a new exception will be thrown instead of returning 0 if the valueTreeMainRootValueId is not existing in the ValueTreeMain.
        /// </summary>
        /// <param name="valueTreeMainRootValueId">The Id to identify the ValueTreeMain entry.</param>
        /// <returns>The decimal value. (0 if not set)</returns>
        private Decimal GetValueTreeMainDecimalValue(string valueTreeMainRootValueId) {
            IValueTreeEntry tmp = ValueTreeMain.GetValue(valueTreeMainRootValueId);
            if (tmp == null) {
#if DEBUG
                //throw new Exception("ValueTreeMain has no value with ID " + valueTreeMainRootValueId);
#endif
                return 0;
            }

            return tmp.DecimalValue;
        }
        #endregion GetValueTreeMainDecimalValue
        
        #region SelectTaxonomyFromBusinessSector

        public static ITaxonomyInfo GetTaxonomyInfoFromBusinessSector(string selectedBusinessSector) {

            // select taxonomy depending on selected industy sector
            switch (selectedBusinessSector) {
                case "genInfo.report.id.specialAccountingStandard.RKV":
                    return TaxonomyManager.GetLatestTaxonomyInfo(TaxonomyType.Financial);
                    // special taxonomy for financial service providers

                case "genInfo.report.id.specialAccountingStandard.RVV":
                    return TaxonomyManager.GetLatestTaxonomyInfo(TaxonomyType.Insurance);
                    // special taxonomy for insurances

                case "genInfo.report.id.specialAccountingStandard.PBV":
                case "genInfo.report.id.specialAccountingStandard.KHBV":
                case "genInfo.report.id.specialAccountingStandard.EBV":
                case "genInfo.report.id.specialAccountingStandard.WUV":
                case "genInfo.report.id.specialAccountingStandard.VUV":
                case "genInfo.report.id.specialAccountingStandard.LUF":
                    // industry sector extension of core taxonomy
                    return TaxonomyManager.GetLatestTaxonomyInfo(TaxonomyType.OtherBusinessClass);

                case "genInfo.report.id.specialAccountingStandard.K":
                    // core taxonomy
                    return TaxonomyManager.GetLatestTaxonomyInfo(TaxonomyType.GAAP);


                default:
                    throw new Exception(
                        "Unhandled choice in genInfo.report.id.specialAccountingStandard: " + selectedBusinessSector);
            }
        }

        private TaxonomySubType GetTaxonomyKind(string selectedItem) {
            switch (selectedItem) {
                case "genInfo.report.id.specialAccountingStandard.PBV":
                    return TaxonomySubType.PBV;
                case "genInfo.report.id.specialAccountingStandard.KHBV":
                    return TaxonomySubType.KHBV;
                case "genInfo.report.id.specialAccountingStandard.EBV":
                    return TaxonomySubType.EBV;
                case "genInfo.report.id.specialAccountingStandard.WUV":
                    return TaxonomySubType.WUV;
                case "genInfo.report.id.specialAccountingStandard.VUV":
                    return TaxonomySubType.VUV;
                case "genInfo.report.id.specialAccountingStandard.LUF":
                    return TaxonomySubType.LUF;
                default:
                    return TaxonomySubType.Other;
            }
        }
        #endregion SelectTaxonomyFromBusinessSector

        #region event handler
        private void AccountingStandardPropertyChanged(object sender, PropertyChangedEventArgs e) { if (e.PropertyName == "SelectedValue") OnPropertyChanged("IsCommercialBalanceSheet"); }
        #endregion event handler

        #endregion ValueTreeGcd

        #region ValueTreeMain
        private ValueTree.ValueTree _valueTreeMain;

        public ValueTree.ValueTree ValueTreeMain {
            get { return _valueTreeMain; }
            private set {
                if (_valueTreeMain != null) RemoveGaapEventHandler();
                _valueTreeMain = value;
                if (_valueTreeMain == null) return;
                InitValueTreeMain();

                OnPropertyChanged("IsCommercialBalanceSheet");
                OnPropertyChanged("ValueTreeMain");
                OnPropertyChanged("BalanceListsVisible");
            }
        }

        #region InitValueTreeMain
        private void InitValueTreeMain() {
            TaxonomyPart = new ReportPartPath(this);
            ValidatorMainTaxonomy = new ValidatorMainTaxonomy(this);

            GaapPresentationTrees = PresentationTree.CreatePresentationTrees(MainTaxonomy, ValueTreeMain, BalanceLists);

            AddGaapEventHandler();

            ReconciliationManager.AssignTransactions();
        }
        #endregion InitValueTreeMain
        
        #region AddGaapEventHandler
        /// <summary>
        /// Add event handler for changes of Total assets or Total equity and liabilities in balance sheet.
        /// </summary>
        private void AddGaapEventHandler() {
            // add event handler for changes of balance sheet total assets ("Aktiva")
            {
                
                var value = ValueTreeMain.GetValue(TaxonomyPart.BalanceSheetAss);
                
                if (value != null) {
                    value.PropertyChanged += BalanceSumChanged;
                }
            }

            // add event handler for changes of balance sheet Total equity and liabilities ("Passiva")
            {
                var value = ValueTreeMain.GetValue(TaxonomyPart.BalanceSheetEqLiab);
                if (value != null) {
                    value.PropertyChanged += BalanceSumChanged;
                }
            }
        }

        #endregion AddGaapEventHandler

        #region RemoveGaapEventHandler
        private void RemoveGaapEventHandler() {
            // add event handler for changes of accountingStandard (Bilanzierungsstandard (Handelsbilanz / Steuerbilanz))
            {
                var value = ValueTreeMain.GetValue(TaxonomyPart.BalanceSheetAss);
                if (value != null) value.PropertyChanged -= BalanceSumChanged;
            }

            // add event handler for changes of specialAccountingStandard (Branche)
            {
                var value = ValueTreeMain.GetValue(TaxonomyPart.BalanceSheetEqLiab);
                if (value != null) value.PropertyChanged -= BalanceSumChanged;
            }
        }
        #endregion RemoveGaapEventHandler

        #region event handler
        /// <summary>
        /// Fire an OnPropertyChanged for BalanceSheetSaldoDifferenceString to refresh the displayed difference of balance sheet sums.
        /// </summary>
        private void BalanceSumChanged(object sender, PropertyChangedEventArgs e) {
            if (e.PropertyName == "DecimalValue") {
                OnPropertyChanged("BalanceSheetSaldoDifferenceString");
                OnPropertyChanged("BalanceSheetSaldoDifferenceValueAsString");
            }
        }
        #endregion event handler

        #endregion ValueTreeMain

        #region Validators
        public IValidator ValidatorGCD { get; private set; }
        public IValidator ValidatorGCD_Company { get; private set; }
        public IValidator ValidatorMainTaxonomy { get; private set; }
        #endregion Validators

        #region GaapPresentationTrees
        private Dictionary<string, IPresentationTree> _gaapPresentationTrees;

        public Dictionary<string, IPresentationTree> GaapPresentationTrees {
            get { return _gaapPresentationTrees; }
            set {
                _gaapPresentationTrees = value;

                if (_gaapPresentationTrees.ContainsKey("http://www.xbrl.de/taxonomies/de-gaap-ci/role/incomeStatement")) {
                    PresentationTreeIncomeStatement = _gaapPresentationTrees["http://www.xbrl.de/taxonomies/de-gaap-ci/role/incomeStatement"];

                } else if (_gaapPresentationTrees.ContainsKey("http://www.xbrl.de/taxonomies/de-fi/role/incomeStatementStf")) {
                    PresentationTreeIncomeStatement = _gaapPresentationTrees["http://www.xbrl.de/taxonomies/de-fi/role/incomeStatementStf"];

                } else if (_gaapPresentationTrees.ContainsKey("http://www.xbrl.de/taxonomies/de-ins/role/incomeStatement")) {
                    PresentationTreeIncomeStatement = _gaapPresentationTrees["http://www.xbrl.de/taxonomies/de-ins/role/incomeStatement"];
                }

                if (_gaapPresentationTrees.ContainsKey("http://www.xbrl.de/taxonomies/de-gaap-ci/role/balanceSheet")) {
                    var ptreeBalanceSheet = _gaapPresentationTrees["http://www.xbrl.de/taxonomies/de-gaap-ci/role/balanceSheet"];
                    foreach (var root in ptreeBalanceSheet.RootEntries) {
                        foreach (IPresentationTreeNode subRoot in root.Children) {
                            if (subRoot.Element.Name == "bs.ass")
                                PresentationTreeBalanceSheetTotalAssets = new PresentationTree(ptreeBalanceSheet, subRoot);

                            if (subRoot.Element.Name == "bs.eqLiab")
                                PresentationTreeBalanceSheetLiabilities = new PresentationTree(ptreeBalanceSheet, subRoot);
                        }
                    }
                } else if (_gaapPresentationTrees.ContainsKey("http://www.xbrl.de/taxonomies/de-fi/role/balanceSheet")) {
                    var ptreeBalanceSheet = _gaapPresentationTrees["http://www.xbrl.de/taxonomies/de-fi/role/balanceSheet"];
                    foreach (var root in ptreeBalanceSheet.RootEntries) {
                        foreach (IPresentationTreeNode subRoot in root.Children) {
                            if (subRoot.Element.Name == "bsBanks.assHeader")
                                PresentationTreeBalanceSheetTotalAssets = new PresentationTree(ptreeBalanceSheet, subRoot);

                            if (subRoot.Element.Name == "bsBanks.eqLiabHeader")
                                PresentationTreeBalanceSheetLiabilities = new PresentationTree(ptreeBalanceSheet, subRoot);
                        }
                    }
                } else if (_gaapPresentationTrees.ContainsKey("http://www.xbrl.de/taxonomies/de-ins/role/balanceSheet")) {
                    var ptreeBalanceSheet = _gaapPresentationTrees["http://www.xbrl.de/taxonomies/de-ins/role/balanceSheet"];
                    foreach (var root in ptreeBalanceSheet.RootEntries) {
                        foreach (IPresentationTreeNode subRoot in root.Children) {
                            if (subRoot.Element.Name == "bsIns.ass")
                                PresentationTreeBalanceSheetTotalAssets = new PresentationTree(ptreeBalanceSheet, subRoot);

                            if (subRoot.Element.Name == "bsIns.eqLiab")
                                PresentationTreeBalanceSheetLiabilities = new PresentationTree(ptreeBalanceSheet, subRoot);
                        }
                    }
                }

                //foreach (var x in value)
                    //x.Value.Filter.ShowOnlyPositionsForSelectedLegalStatus = true;
            }
        }

        public IPresentationTree PresentationTreeBalanceSheetTotalAssets { get; private set; }
        public IPresentationTree PresentationTreeBalanceSheetLiabilities { get; private set; }
        public IPresentationTree PresentationTreeIncomeStatement { get; private set; }
        #endregion GaapPresentationTrees

        #region ReportRights
        private ReportRights _reportRights;

        public ReportRights ReportRights {
            get { return _reportRights; }
            set {
                _reportRights = value;
                OnPropertyChanged("ReportRights");
            }
        }
        #endregion

        #region IsCommercialBalanceSheet
        public bool IsCommercialBalanceSheet {
            get {
                if (ValueTreeGcd == null) return false;
                var value = GetGcdSingleChoiceValue("de-gcd_genInfo.report.id.accountingStandard");
                if (value == null) return false;
                return value.SelectedValue != null &&
                       value.SelectedValue.Id == "de-gcd_genInfo.report.id.accountingStandard.accountingStandard.HGBM";
            }
        }
        #endregion

        #region LegalForm Properties

        #region IsPartnership (Personengesellschaft)

        /// <summary>
        /// Tells if the company is a so called partnership ("Personengesellschaft") (OHG, KG, GKG, AGKG, EWI, GBR, PG, MUN)
        /// </summary>
        public bool IsPartnership {
            get {
                if (ValueTreeGcd.Root.Values["de-gcd_genInfo.company.id.legalStatus"].Value == null) return false;

                string legalForm = ValueTreeGcd.Root.Values["de-gcd_genInfo.company.id.legalStatus"].Value.ToString();
                List<string> legalForms = new List<string>() {
                    "genInfo.company.id.legalStatus.legalStatus.OHG",
                    "genInfo.company.id.legalStatus.legalStatus.KG",
                    "genInfo.company.id.legalStatus.legalStatus.GKG",
                    "genInfo.company.id.legalStatus.legalStatus.AGKG",
                    "genInfo.company.id.legalStatus.legalStatus.EWI",
                    "genInfo.company.id.legalStatus.legalStatus.GBR",
                    "genInfo.company.id.legalStatus.legalStatus.PG",
                    //"genInfo.company.id.legalStatus.legalStatus.PA",
                    //"genInfo.company.id.legalStatus.legalStatus.ST",
                    "genInfo.company.id.legalStatus.legalStatus.Delta.MUN"
                };
                return legalForms.Contains(legalForm);
            }
        }

        #endregion IsPartnership (Personengesellschaft)

        #region IsSoleProprietorship (Einzelunternehmen)

        /// <summary>
        /// Checks if the company is a so called Sole proprietorship ("Einzelunternehmen")
        /// </summary>
        /// <ToDo>Apply the new specification (available 11/2012)</ToDo>
        public bool IsSoleProprietorship { get { return false; } }

        #endregion

        #region IsCorporation (Körperschaft)

        /// <summary>
        /// Checks if the company is a so called Sole Corporation ("Körperschaft")
        /// </summary>
        /// <ToDo>Apply the new specification (available 11/2012)</ToDo>
        public bool IsCorporation {
            get {
                if (ValueTreeGcd.Root.Values["de-gcd_genInfo.company.id.legalStatus"].Value == null)
                    return false;

                string legalForm =
                    ValueTreeGcd.Root.Values["de-gcd_genInfo.company.id.legalStatus"].Value.ToString();
                List<string> legalForms = new List<string>() {
                    //"genInfo.company.id.legalStatus.legalStatus.xx",
                    //"genInfo.company.id.legalStatus.legalStatus.xx",
                    //"genInfo.company.id.legalStatus.legalStatus.xx",
                    //"genInfo.company.id.legalStatus.legalStatus.xx",
                    //"genInfo.company.id.legalStatus.legalStatus.xx",
                    //"genInfo.company.id.legalStatus.legalStatus.xx",
                    //"genInfo.company.id.legalStatus.legalStatus.xx",
                    "genInfo.company.id.legalStatus.legalStatus.AG"
                };
                return legalForms.Contains(legalForm);
            }
        }
        #endregion IsCorporation (Körperschaft)

        #endregion LegalForm Properties

        #region TypeOperating Properties

        #region IsTypeOperatingResultTC (GKV)
        /// <summary>
        /// Checks if the income statement format is TC ("GKV") (de-gcd_genInfo.report.id.incomeStatementFormat)
        /// </summary>
        public bool IsTypeOperatingResultTC {
            get {
                var element =
                    ValueTreeGcd.Root.Values["de-gcd_genInfo.report.id.incomeStatementFormat"] as
                    XbrlElementValue_SingleChoice;
                if (element == null || element.SelectedValue == null) return false;

                return element.SelectedValue.Id ==
                       "de-gcd_genInfo.report.id.incomeStatementFormat.incomeStatementFormat.GKV";
            }
        }
        #endregion IsTypeOperatingResultTC (GKV)

        #region IsTypeOperatingResultCoS (UKV)
        /// <summary>
        /// Checks if the income statement format is CoS ("UKV") (de-gcd_genInfo.report.id.incomeStatementFormat)
        /// </summary>
        public bool IsTypeOperatingResultCoS {
            get {
                var element =
                    ValueTreeGcd.Root.Values["de-gcd_genInfo.report.id.incomeStatementFormat"] as
                    XbrlElementValue_SingleChoice;
                if (element == null || element.SelectedValue == null) return false;

                return element.SelectedValue.Id ==
                       "de-gcd_genInfo.report.id.incomeStatementFormat.incomeStatementFormat.UKV";
            }
        }
        #endregion IsTypeOperatingResultCoS (UKV)

        #region IsTypeOperatingResultNotSelected
        /// <summary>
        /// Checks if the income statement format is not set (de-gcd_genInfo.report.id.incomeStatementFormat)
        /// </summary>
        public bool IsTypeOperatingResultNotSelected {
            get {
                var element =
                    ValueTreeGcd.Root.Values["de-gcd_genInfo.report.id.incomeStatementFormat"] as
                    XbrlElementValue_SingleChoice;
                return element == null || element.SelectedValue == null ||
                       (!element.SelectedValue.Id.EndsWith("incomeStatementFormat.UKV") &&
                        !element.SelectedValue.Id.EndsWith("incomeStatementFormat.GKV"));
            }
        }
        #endregion IsTypeOperatingResultNotSelected

        #endregion TypeOperating Properties

        #region Special Accounting Standard  Properties

        /// <summary>
        /// Bankentaxonomie - Checks if the current report is RechKredV (Branche) - [de-gcd_genInfo.report.id.specialAccountingStandard as XbrlElementValue_SingleChoice]
        /// </summary>
        public bool IsRKV { get { return MainTaxonomyInfo.Type == TaxonomyType.Financial; } }

        /// <summary>
        /// Versicherungstaxonomie - Checks if the current report is RechVersV (Branche) - [de-gcd_genInfo.report.id.specialAccountingStandard as XbrlElementValue_SingleChoice]
        /// </summary>
        public bool IsRVV {
            get { return MainTaxonomyInfo.Type == TaxonomyType.Insurance; } }

        #endregion Special Accounting Standard  Properties

        #region CompanyEditAllowed
        /// <summary>
        /// Returns true, if the current user has write rigths for the assigned company.
        /// </summary>
        public bool CompanyEditAllowed { get { return RightManager.RightDeducer.GetRight(Company).IsWriteAllowed; } }
        #endregion

        #region DoDbUpdate
        internal bool DoDbUpdate { get; set; }
        #endregion

        #region BalanceSheetSaldoDifference
        private decimal BalanceSheetSaldoDifference {
            get {
                return Decimal.Subtract(GetValueTreeMainDecimalValue(TaxonomyPart.BalanceSheetAss), GetValueTreeMainDecimalValue(TaxonomyPart.BalanceSheetEqLiab));
            }
        }

        //public string BalanceSheetSaldoDifferenceString { get { return string.Format(ResourcesCommon.BalanceSheetSaldoDifferenceString, LocalisationUtils.CurrencyToString(BalanceSheetSaldoDifference)); } }
        public string BalanceSheetSaldoDifferenceString { get { return ResourcesCommon.BalanceSheetSaldoDifferenceString; } }
        public string BalanceSheetSaldoDifferenceValueAsString { get { return LocalisationUtils.CurrencyToString(BalanceSheetSaldoDifference); } }

        #endregion BalanceSheetSaldoDifference

        #region BalanceSheetPath

        public ReportPartPath TaxonomyPart { set; get; }

        #endregion

        #region AllowEditOwner
        /// <summary>
        /// Rights for current user not enabled because that leads to problems due to changed rights.
        /// </summary>
        public bool AllowEditOwner { get { return UserManager.Instance.CurrentUser.IsAdmin /*|| UserManager.Instance.CurrentUser == Owner*/; } }
        #endregion // AllowEditOwner

        #region IsCurrentDisplayedReport
        public bool IsCurrentDisplayedReport { get { return DocumentManager.Instance.CurrentDocument != null && DocumentManager.Instance.CurrentDocument.Id == Id; } }
        #endregion // IsCurrentDisplayedReport

        #region [ ReconciliationMode ]

        private ReconciliationMode _reconciliationMode = ReconciliationMode.General;

        [DbColumn("reconciliation_mode")]
        public ReconciliationMode ReconciliationMode {
            get { return _reconciliationMode; }
            set {
                if (_reconciliationMode == value) return;

                // log changed value
                OnNewLog("ReconciliationMode", _reconciliationMode, value);

                _reconciliationMode = value;
                OnPropertyChanged("ReconciliationMode");
            }
        }

        #endregion [ ReconciliationMode ]

        #endregion properties

        #region methods

        internal void Init(TaxonomyType mainTaxonomyType = TaxonomyType.GAAP) {
            Company = CompanyManager.Instance.AllowedCompanies[0];
            System = SystemManager.Instance.Systems[0];
            Name = "Standardreport";
            CreationDate = DateTime.Now;
            Owner = UserManager.Instance.CurrentUser;
            GcdTaxonomyInfo = TaxonomyManager.GetLatestTaxonomyInfo(TaxonomyType.GCD);
            MainTaxonomyInfo = TaxonomyManager.GetLatestTaxonomyInfo(mainTaxonomyType);
            FinancialYear = CompanyManager.Instance.AllowedCompanies[0].VisibleFinancialYears[0];
            DoDbUpdate = true;
        }

        internal void Init(ITaxonomyInfo mainTaxonomyInfo) {
            Init();
            MainTaxonomyInfo = mainTaxonomyInfo;
        }

        internal void Init(IDatabase conn) {
            LogManager.Instance.NewDocument(this, true);
            // BalanceList is lazy loaded because it took too much time to load for all documents
            //var balanceLists =
            //    (from bl in conn.DbMapping.Load<BalanceList.BalanceList>(conn.Enquote("document_id") + "=" + Id)
            //     orderby bl.IsImported descending, bl.Name
            //     select bl).ToList();
            //foreach (var bl in balanceLists) {
            //    BalanceLists.Add(bl);
            //    bl.Document = this;
            //}
            Company = CompanyManager.Instance.GetCompany(CompanyId);
            System = SystemManager.Instance.GetSystem(SystemId);
            Owner = UserManager.Instance.GetUser(OwnerId);
            FinancialYear = Company.GetFinancialYearOrDefault(FinancialYearId);
            GcdTaxonomyInfo = TaxonomyManager.GetTaxonomyInfo(GcdTaxonomyInfoId);
            MainTaxonomyInfo = TaxonomyManager.GetTaxonomyInfo(MainTaxonomyInfoId);
            DoDbUpdate = true;
        }

        #region LoadDetails
        public void LoadDetails(ProgressInfo progressInfo) {
            
            LoadReconcilations(progressInfo); // must been called before calling LoadValueTrees!
            LoadBalanceLists();
            LoadVirtualBalanceList(progressInfo);
            LoadValueTrees(progressInfo);
            AddVirtualBalanceListAssignments(progressInfo);
            LoadHyperCubes(progressInfo);

            //LoadVirtualBalanceList(progressInfo);
            //OnPropertyChanged("SelectedBalanceList.AssignedItemsDisplayed");

            LoadAuditCorrections(progressInfo); // must been called before calling LoadValueTrees!

            ValueTreeMain.Root.ForceRecomputation();
            XbrlElementValueBase.InitStates(ValueTreeMain.Root);

            SetReportPeriodIfEmpty();

        }

        private void SetReportPeriodIfEmpty() {
            if (GcdTaxonomyInfo.Version != "2012-06-01")
                return;
            bool doDbUpdate = XbrlElementValueBase.DoDbUpdate;
            try {
                if(!doDbUpdate) AppConfig.EnableDbUpdates();
                if (ValueTreeGcd.Root.Values["de-gcd_genInfo.report.period.fiscalYearBegin"].Value == null)
                    ValueTreeGcd.Root.Values["de-gcd_genInfo.report.period.fiscalYearBegin"].Value =
                        FinancialYear.FiscalYearBegin;
                if (ValueTreeGcd.Root.Values["de-gcd_genInfo.report.period.fiscalYearEnd"].Value == null)
                    ValueTreeGcd.Root.Values["de-gcd_genInfo.report.period.fiscalYearEnd"].Value =
                        FinancialYear.FiscalYearEnd;
                if (ValueTreeGcd.Root.Values["de-gcd_genInfo.report.period.fiscalPreviousYearBegin"].Value == null)
                    ValueTreeGcd.Root.Values["de-gcd_genInfo.report.period.fiscalPreviousYearBegin"].Value =
                        FinancialYear.FiscalYearBeginPrevious;
                if (ValueTreeGcd.Root.Values["de-gcd_genInfo.report.period.fiscalPreviousYearEnd"].Value == null)
                    ValueTreeGcd.Root.Values["de-gcd_genInfo.report.period.fiscalPreviousYearEnd"].Value =
                        FinancialYear.FiscalYearEndPrevious;
                if (ValueTreeGcd.Root.Values["de-gcd_genInfo.report.period.balSheetClosingDate"].Value == null)
                    ValueTreeGcd.Root.Values["de-gcd_genInfo.report.period.balSheetClosingDate"].Value =
                        FinancialYear.BalSheetClosingDate;
                if (ValueTreeGcd.Root.Values["de-gcd_genInfo.report.period.balSheetClosingDatePreviousYear"].Value == null)
                    ValueTreeGcd.Root.Values["de-gcd_genInfo.report.period.balSheetClosingDatePreviousYear"].Value =
                        FinancialYear.BalSheetClosingDatePreviousYear;
            }
            catch (Exception) {
            }
            finally {
                if(!doDbUpdate) AppConfig.DisableDbUpdates();
            }
        }

        #endregion LoadDetails

        #region ClearDetails
        public void ClearDetails() {
            foreach (var balanceList in BalanceLists) balanceList.ClearEntries();
            GC.Collect();

            ClearHyperCubes();
            ClearReconcilations();
            ClearAuditCorrections();
            RemoveValidationMessages();
            IsSelected = false;
        }
        #endregion ClearDetails

        #region CompareTo
        /// <summary>
        /// Compares documents by name.
        /// </summary>
        public int CompareTo(object obj) {
            if (!(obj is Document)) return 0;
            var dobj = obj as Document;

            string cn1 = Company.Name;
            string cn2 = dobj.Company.Name;

            string sn1 = System.Name;
            string sn2 = dobj.System.Name;

            if (sn1 == null && sn2 == null) return 0;
            if (sn1 == null) return -1;
            if (sn2 == null) return 1;

            int result = sn1.CompareTo(sn2);
            if (result == 0) {
                if (cn1 == null && cn2 == null) return 0;
                if (cn1 == null) return -1;
                if (cn2 == null) return 1;

                result = cn1.CompareTo(cn2);
                if (result == 0) {
                    result = FinancialYear.FYear.CompareTo(dobj.FinancialYear.FYear);
                    if (result == 0) {
                        Name.CompareTo(dobj.Name);
                    }
                }
            }

            return result;
        }
        #endregion

        #region AddAccountAssignments

        /// <summary>
        /// Adds a new account assignment for the currently selected balance list.
        /// </summary>
        public void AddAccountAssignment(IBalanceListEntry account, string elementId) { AddAccountAssignment(account, elementId, SelectedBalanceList); }

        /// <summary>
        /// Adds a new account assignment for the specified balance list.
        /// </summary>
        public void AddAccountAssignment(IBalanceListEntry account, string elementId, IBalanceList balanceList) { balanceList.AddAssignment(account, elementId); }

        /// <summary>
        /// Adds multiple account assignments for for the currently selected balance list.
        /// </summary>
        public void AddAccountAssignments(List<Tuple<IBalanceListEntry, string>> assignments, ProgressInfo pi) { AddAccountAssignments(assignments, pi, SelectedBalanceList); }

        /// <summary>
        /// Adds multiple account assignments for for the specified balance list.
        /// </summary>
        public void AddAccountAssignments(
            List<Tuple<IBalanceListEntry, string>> assignments, ProgressInfo pi, IBalanceList balanceList) {

            // init progress info object
            if (pi != null) {
                pi.Caption = ExceptionMessages.ProgressAddAssignments;
                pi.Value = 0;
                pi.Maximum = assignments.Count*2;
            }

            // assign accounts
            var logData = new List<Tuple<IBalanceListEntry, int, int>>();
                // stores Account, OldAssignmentId, NewAssignmentId
            foreach (var assignment in assignments) {
                IBalanceListEntry account = assignment.Item1;
                string elementId = assignment.Item2;

                account.IsVisible = true;
                account.IsHidden = false;

                logData.Add(new Tuple<IBalanceListEntry, int, int>(account, account.AssignedElementId,
                                                                   TaxonomyIdManager.GetId(elementId)));

                TaxonomyIdManager.SetElementAssignment(account, elementId);
                if (pi != null && pi.Value < pi.Maximum) pi.Value++;
            }

            // save changes to database
            using (IDatabase conn = AppConfig.ConnectionManager.GetConnection()) {
                try {
                    conn.BeginTransaction();
                    balanceList.AddAssignments(logData, conn, pi);
                    conn.CommitTransaction();

                } catch (Exception ex) {
                    conn.RollbackTransaction();
                    throw new Exception("Could not save new account assignments: " + ex.Message, ex);
                }
            }
        }
        #endregion AddAccountAssignments

        #region RemoveAssignments
        /// <summary>
        ///  Removes the account assignment for the specified account.
        /// </summary>
        public void RemoveAssignment(IBalanceListEntry account) { account.BalanceList.RemoveAssignment(account); }

        /// <summary>
        /// Removes the account assignment for all specified accounts.
        /// </summary>
        public void RemoveAssignments(List<IBalanceListEntry> accounts) {
            using (IDatabase conn = AppConfig.ConnectionManager.GetConnection()) {
                SelectedBalanceList.RemoveAssignments(accounts, conn, null);
            }
        }
        #endregion RemoveAssignments

        #region ClearAssignments
        /// <summary>
        /// Removes all assignments for all assigned balance lists.
        /// </summary>
        public void ClearAssignments(ProgressInfo pi) { foreach (IBalanceList balanceList in BalanceLists) ClearAssignments(pi, balanceList); }

        /// <summary>
        /// Removes all assignments for the specified balance list.
        /// </summary>
        public void ClearAssignments(ProgressInfo pi, IBalanceList balanceList) {
            var assignedAccounts = new List<IBalanceListEntry>(balanceList.AssignedItems);

            // init progress info
            if (pi != null) {
                pi.Caption = ExceptionMessages.ProgressDeleteAssignments;
                pi.Value = 0;
                pi.Maximum = assignedAccounts.Count;
            }

            using (var conn = AppConfig.ConnectionManager.GetConnection()) {
                try {
                    conn.BeginTransaction();
                    balanceList.RemoveAssignments(assignedAccounts, conn, pi);
                    conn.CommitTransaction();
                } catch (Exception ex) {
                    conn.RollbackTransaction();
                    throw new Exception("Could not update database: " + ex.Message, ex);
                }
            }

            // remove assignments from presentation trees
            foreach (IBalanceListEntry account in assignedAccounts) {
                account.RemoveFromParents();
            }
        }
        #endregion

        #region RemoveBalanceList
        /// <summary>
        /// Removes the specified balance list.
        /// </summary>
        public void RemoveBalanceList(IBalanceList balanceList) { BalanceLists.Remove(balanceList); OnPropertyChanged("BalanceListsImported");}
        #endregion RemoveBalanceList

        #region LoadValueTrees
        internal void LoadValueTrees(ProgressInfo progressInfo) {
            ValueTreeGcd = ValueTree.ValueTree.CreateValueTree(typeof (ValuesGCD), this, GcdTaxonomy, progressInfo);
            ValueTreeMain = ValueTree.ValueTree.CreateValueTree(typeof (ValuesGAAP), this, MainTaxonomy, progressInfo);
        }
        #endregion LoadValueTrees

        #region LoadBalanceLists
        private void LoadBalanceListsFromDataBase() {
            using (var conn = AppConfig.ConnectionManager.GetConnection()) {
                List<BalanceList.BalanceList> balanceLists =
                    (from bl in conn.DbMapping.Load<BalanceList.BalanceList>(conn.Enquote("document_id") + "=" + Id)
                     orderby bl.IsImported descending , bl.Name
                     select bl).ToList();

                foreach (BalanceList.BalanceList bl in balanceLists) {
                    _items.Add(bl);
                    bl.Document = this;
                }
            }
        }

        private void LoadBalanceLists() {
            foreach (IBalanceList balanceList in BalanceLists.Where(x => !(x is VirtualBalanceList) && x.IsImported)) {
                BalanceListManager.InitBalanceListEntries(this, balanceList);
                balanceList.DoDbUpdate = true;
            }
        }
        #endregion LoadBalanceLists

        #region LoadVirtualBalanceList
        /// <summary>
        /// Load the virtual BalanceList from the database or create one if none is there.
        /// </summary>
        /// <param name="progressInfo">Not used at the moment.</param>
        private void LoadVirtualBalanceList(ProgressInfo progressInfo) {
            var vBalList = BalanceLists.Where(x => (x is VirtualBalanceList) || x.IsImported == false).ToList();
            if (!vBalList.Any()) {
                // There is no virtual balance list so we have to create one but do NOT assign the virtual accounts 
                // (the report was not new created so probably there is already some work done and if we would assign the account eg. the balance would be not valid anymore
                VirtualBalanceListAndAccountManager.GenerateAndAssignVirtualBalanceList(this, progressInfo, false);
                vBalList = BalanceLists.Where(x => (x is VirtualBalanceList)).ToList();
            }
            // There should be only one
            foreach (IBalanceList balanceList in vBalList) {
                balanceList.ClearEntries();
                VirtualBalanceListAndAccountManager.InitBalanceListEntries(this, balanceList);
                balanceList.DoDbUpdate = true;
            }
            // If there is no other balance list already selected we select the first virtual balance list
            if (SelectedBalanceList == null) {
                SelectedBalanceList = vBalList.FirstOrDefault();
            }
            
            //if (SelectedBalanceList != null) 
            //    SelectedBalanceList.UpdateFilter();

            OnPropertyChanged("SelectedBalanceList");
            OnPropertyChanged("BalanceListsVisible"); 
            //SelectedBalanceList.FirePropertyChangedEventsForDisplayedItems();
            //OnPropertyChanged("ValueTreeMain");
        }

        private void AddVirtualBalanceListAssignments(ProgressInfo progressInfo) {
            foreach (
                IBalanceList balanceList in BalanceLists.Where(x => (x is VirtualBalanceList) || x.IsImported == false)) {
                if (balanceList.Accounts != null) {
                    List<VirtualAccountConfiguration> virtualAccountConfigurations =
                        VirtualBalanceListAndAccountManager.CreateDefaultTaxonomyElements();
                    foreach (IAccount account in balanceList.Accounts) {
                        VirtualAccount virtualAccount = account as VirtualAccount;
                        if (virtualAccount != null) {
                            foreach (
                                VirtualAccountConfiguration virtualAccountConfiguration in virtualAccountConfigurations) {
                                if (virtualAccount.TaxonomyPosition == virtualAccountConfiguration.SourcePosition &&
                                    ValueTreeMain != null && MainTaxonomy != null) {
                                    try {
                                        ValueTreeMain.GetValue(
                                            MainTaxonomy.Elements[virtualAccountConfiguration.SourcePosition].Id).
                                            PropertyChanged +=
                                            VirtualBalanceListAndAccountManager.MonetaryValuePropertyChanged;
                                    } catch (Exception) {
                                        throw new Exception(string.Format(ResourcesCommon.FollowingPositionsNotFound,
                                                                          virtualAccountConfiguration.SourcePosition));
                                    }
                                    virtualAccount.BalanceList = virtualAccount.BalanceList;
                                }
                            }
                        }
                    }
                }
            }
        }

        #endregion LoadVirtualBalanceList

        #region ChangeAssignedTaxonomy
        public void ChangeAssignedTaxonomy(ITaxonomyInfo oldValue, string selectedBusinessSector, ProgressInfo progressInfo) {

            var newTaxonomyInfo = GetTaxonomyInfoFromBusinessSector(selectedBusinessSector);
            if (MainTaxonomyInfo == newTaxonomyInfo) return;
            
            MainTaxonomyInfo = newTaxonomyInfo;            
            MainTaxonomyKind = GetTaxonomyKind(selectedBusinessSector);

            // remove all assignments
            ProgressInfo pi = new ProgressInfo();
            ClearAssignments(pi);

            // delete all hypercubes
            foreach (var cube in GetHyperCubes()) cube.Delete();
            ClearHyperCubes();

            // delete all reconciliations
            ReconciliationManager.DeleteAllReconciliations();
            
            var oldTaxonomy = TaxonomyManager.GetTaxonomy(oldValue);

            LogManager.Instance.UpdateTaxonomy(this, oldValue.Name);

            var missingValues = new List<ValuesGAAP>();

            Action<ValuesGAAP> addMissingValues = null;
            addMissingValues = (root) => {
                missingValues.Add(root);
                foreach (ValueMappingBase child in root.Children) addMissingValues(child as ValuesGAAP);
            };

            // delete elements, which does not exist in the new taxonomy
            foreach (
                var element in
                    oldTaxonomy.Elements.Values.Where(oldElem => !MainTaxonomy.Elements.ContainsKey(oldElem.Id))) {
                var missingValue = ValueTreeMain.GetValue(element.Name);
                if (missingValue != null) {
                    addMissingValues(missingValue.DbValue as ValuesGAAP);
                }
            }

            ValueManager.RemoveValues(missingValues);

            LoadReconcilations(progressInfo); // must been called before calling LoadValueTrees!
            ValueTreeMain = ValueTree.ValueTree.CreateValueTree(typeof (ValuesGAAP), this, MainTaxonomy, progressInfo);

            ValueTreeMain.Root.ForceRecomputation();
            XbrlElementValueBase.InitStates(ValueTreeMain.Root);

            LoadHyperCubes(pi);
            LoadVirtualBalanceList(pi);

        }
        #endregion ChangeAssignedTaxonomy

        #region Validate
        /// <summary>
        /// Validates this instance. The results must been evaluated from the calling GUI method, 
        /// which must show the available error messages from the ValidatorXXX properties.
        /// </summary>
        public bool Validate(ref string errors) {
            bool result = true;

            var validators = new[] {ValidatorGCD, ValidatorGCD_Company, ValidatorMainTaxonomy};
            var generalErrors = new List<string>();
            foreach (IValidator validator in validators) {
                bool b = result &= validator.Validate();
                generalErrors.AddRange(validator.GeneralErrorMessages);
            }
            string finalMessage = "Folgende Fehler sind aufgetreten: ";
            for (int i = 0; i < generalErrors.Count; ++i) {
                finalMessage += Environment.NewLine + (i + 1) + ". " + generalErrors[i];
            }

            if (generalErrors.Count != 0) errors = finalMessage;
            //MessageBox.Show(finalMessage);
            return result;
        }
        #endregion

        #region RemoveValidationMessages
        /// <summary>
        /// Remove all valiadion messages
        /// </summary>
        public bool RemoveValidationMessages() {
            bool result = true;

            var validators = new[] {ValidatorGCD, ValidatorGCD_Company, ValidatorMainTaxonomy};
            var generalErrors = new List<string>();
            foreach (IValidator validator in validators) {
                //bool b = result &= validator.Validate();
                //generalErrors.AddRange(validator.GeneralErrorMessages);
                validator.ResetValidationValues();
            }
            string finalMessage = "Folgende Fehler sind aufgetreten: ";
            for (int i = 0; i < generalErrors.Count; ++i) {
                finalMessage += Environment.NewLine + (i + 1) + ". " + generalErrors[i];
            }

            return result;
        }
        #endregion RemoveValidationMessages

        //#region SetFinancialYearValues
        //private void SetFinancialYearValues() {
        //    if (ValueTreeGcd == null) return;
        //    ValueTreeGcd.Root.Values["de-gcd_genInfo.report.period.fiscalYearBegin"].Value = FinancialYear.FiscalYearBegin;
        //    ValueTreeGcd.Root.Values["de-gcd_genInfo.report.period.fiscalYearEnd"].Value = FinancialYear.FiscalYearEnd;
        //    ValueTreeGcd.Root.Values["de-gcd_genInfo.report.period.balSheetClosingDate"].Value =
        //        FinancialYear.BalSheetClosingDate;
        //    if (ValueTreeGcd.Root.Values.ContainsKey("de-gcd_genInfo.report.period.fiscalPreviousYearBegin")) {
        //        ValueTreeGcd.Root.Values["de-gcd_genInfo.report.period.fiscalPreviousYearBegin"].Value =
        //            FinancialYear.FiscalYearBeginPrevious;
        //    }
        //    if (ValueTreeGcd.Root.Values.ContainsKey("de-gcd_genInfo.report.period.fiscalPreciousYearBegin")) {
        //        ValueTreeGcd.Root.Values["de-gcd_genInfo.report.period.fiscalPreciousYearBegin"].Value =
        //            FinancialYear.FiscalYearBeginPrevious;
        //    }

        //    if (ValueTreeGcd.Root.Values.ContainsKey("de-gcd_genInfo.report.period.fiscalPreciousYearEnd")) {
        //        ValueTreeGcd.Root.Values["de-gcd_genInfo.report.period.fiscalPreciousYearEnd"].Value =
        //            FinancialYear.FiscalYearEndPrevious;
        //    }
        //    if (ValueTreeGcd.Root.Values.ContainsKey("de-gcd_genInfo.report.period.fiscalPreviousYearEnd")) {
        //        ValueTreeGcd.Root.Values["de-gcd_genInfo.report.period.fiscalPreviousYearEnd"].Value =
        //            FinancialYear.FiscalYearEndPrevious;
        //    }
        //    if (ValueTreeGcd.Root.Values.ContainsKey("de-gcd_genInfo.report.period.balSheetClosingDatePreviousYear")) {
        //        ValueTreeGcd.Root.Values["de-gcd_genInfo.report.period.balSheetClosingDatePreviousYear"].Value =
        //            FinancialYear.BalSheetClosingDatePreviousYear;
        //    }
        //    if (ValueTreeGcd.Root.Values.ContainsKey("de-gcd_genInfo.report.period.balSheetClosingDatePreciousYear")) {
        //        ValueTreeGcd.Root.Values["de-gcd_genInfo.report.period.balSheetClosingDatePreciousYear"].Value =
        //            FinancialYear.BalSheetClosingDatePreviousYear;
        //    }
        //}
        //#endregion SetFinancialYearValues

        /// <summary>
        /// Create a copy of this document.
        /// </summary>
        public void Copy() {
            DocumentManager.Instance.CopyDocument(this);
        }

        #endregion methods

        #region HyperCubes

        private readonly Dictionary<string, IPresentationTreeNode> _hyperCubesNameToNode =
            new Dictionary<string, IPresentationTreeNode>();

        private readonly Dictionary<string, IHyperCube> _hyperCubesByName =
            new Dictionary<string, IHyperCube>();


        private void LoadHyperCubes(ProgressInfo progressInfo) {
            ClearHyperCubes();
            foreach (var node in MainTaxonomy.PresentationTrees.SelectMany(ptree => ptree.HypercubeContainerNodes)) {
                _hyperCubesNameToNode[node.Element.Id] = node;
                if (HyperCube.ExistsCube(this, node)) _hyperCubesByName[node.Element.Id] = new HyperCube(this, node);
            }
            OnHyperCubeCollectionChanged();
        }

        internal void ClearHyperCubes() {
            _hyperCubesNameToNode.Clear();
            _hyperCubesByName.Clear();
            OnHyperCubeCollectionChanged();
        }

        /// <summary>
        /// Returns true iif. the hypercube with tthe specified name exists.
        /// </summary>
        /// <param name="name">Element name of the hyper cube node.</param>
        public bool ExistsHyperCube(string name) {
            if (!ReportRights.ReadRestAllowed)
                throw new AccessDeniedException(ExceptionMessages.InsufficentReadRights);

            return _hyperCubesByName.ContainsKey(name);
        }


        /// <summary>
        /// Returns an enumeration of all existing hyper cubes.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<IHyperCube> GetHyperCubes() {
            if (!ReportRights.ReadRestAllowed)
                throw new AccessDeniedException(ExceptionMessages.InsufficentReadRights);

            return _hyperCubesByName.Values;
        }

        /// <summary>
        /// Gets the hypercube, which is specified by the name parameter.
        /// </summary>
        /// <param name="name">Element name of the hyper cube node.</param>
        public IHyperCube GetHyperCube(string name) {
            if (!ReportRights.ReadRestAllowed)
                throw new AccessDeniedException(ExceptionMessages.InsufficentReadRights);
            
            return _hyperCubesByName.ContainsKey(name) ? _hyperCubesByName[name] : null;
        }

        /// <summary>
        /// Creates a new hypercube, which is specified by the name parameter.
        /// </summary>
        /// <param name="name">Element name of the hyper cube node.</param>
        public IHyperCube CreateHyperCube(string name) {
            if (!ReportRights.WriteRestAllowed)
                throw new AccessDeniedException(ExceptionMessages.InsufficentWriteRights);

            if (_hyperCubesByName.ContainsKey(name)) throw new Exception("The specified hypercube already exists.");

            _hyperCubesByName[name] = new HyperCube(this, _hyperCubesNameToNode[name]);
            OnHyperCubeCollectionChanged();
            LogManager.Instance.NewHyperCube(_hyperCubesByName[name]);
            return _hyperCubesByName[name];
        }

        /// <summary>
        /// Deletes the hypercube, which is specified by the name parameter.
        /// </summary>
        /// <param name="name">Element name of the hyper cube node.</param>
        public void DeleteHyperCube(string name) {
            if (!ReportRights.WriteRestAllowed)
                throw new AccessDeniedException(ExceptionMessages.InsufficentWriteRights);

            if (!_hyperCubesByName.ContainsKey(name)) throw new Exception("The specified hypercube does not exist.");

            LogManager.Instance.DeleteHyperCube(_hyperCubesByName[name]);
            _hyperCubesByName[name].Delete();
            _hyperCubesByName.Remove(name);
            OnHyperCubeCollectionChanged();
        }
        #endregion HyperCubes

        #region Reconciliation
        internal IReconciliationManagerInternal ReconciliationManager { get; private set; }
        private void LoadReconcilations(ProgressInfo progress) { ReconciliationManager = new ReconciliationManager(this); }
        private void ClearReconcilations() { ReconciliationManager = null; }
        #endregion Reconciliation

        #region AuditMode

        #region AuditCorrectionManager
        private IAuditCorrectionManager _auditCorrectionManager;

        public IAuditCorrectionManager AuditCorrectionManager {
            get { return _auditCorrectionManager; }
            private set {
                _auditCorrectionManager = value;
                OnPropertyChanged("AuditCorrectionManager");
            }
        }
        #endregion // AuditCorrectionManager

        private void LoadAuditCorrections(ProgressInfo progress) { AuditCorrectionManager = new AuditCorrectionManager(this); }
        private void ClearAuditCorrections() { AuditCorrectionManager = null; }
        #endregion // AuditMode

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

        #region IReferenceListManipulator members

        public void AddItemToReferenceList(IReferenceListItem item) {
            ReconciliationManager.ReferenceList.AddItemToReferenceList(item);
        }

        public void RemoveItemFromReferenceList(IReferenceListItem item) {
            ReconciliationManager.ReferenceList.RemoveItemFromReferenceList(item);
        }

        public bool IsElementContainedInReferenceList(int elementId) {
            return ReconciliationManager.ReferenceList.IsElementContainedInReferenceList(elementId);
        }

        #endregion IReferenceListManipulator members

        #region Reference list related members

        // DEVNOTE: it only updates the ReferencList and not it's items collection
        public void SaveReferenceList() { ReconciliationManager.ReferenceList.Save(); }

        // accessor for the reconciliation reference list
        public ReferenceList ReconciliationReferenceList {
            get { return ReconciliationManager.ReferenceList; }
        }

        #endregion Reference list related members

        public override string ToString() {
            return string.Format("Id: {3}, System: {0}, Company: {1}, Company Id: {4}, FYear: {2}",
                                 System != null ? System.ToString() : "NULL",
                                 Company != null ? Company.ToString() : "NULL",
                                 FinancialYear != null ? FinancialYear.ToString() : "NULL",
                                 Id,
                                 CompanyId
                );
        }
    }
}
