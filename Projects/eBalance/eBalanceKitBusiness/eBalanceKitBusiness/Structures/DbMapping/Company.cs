// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2010-12-28
// copyright 2010-2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using DbAccess;
using Taxonomy;
using Taxonomy.Enums;
using Taxonomy.Interfaces;
using Utils;
using eBalanceKitBase.Structures;
using eBalanceKitBusiness.Interfaces;
using eBalanceKitBusiness.Localisation;
using eBalanceKitBusiness.Logs;
using eBalanceKitBusiness.Manager;
using eBalanceKitBusiness.Structures.DbMapping.ValueMappings;
using eBalanceKitResources.Localisation;

namespace eBalanceKitBusiness.Structures.DbMapping {
    [DbTable("companies", ForceInnoDb = true)]
    public class Company : NotifyPropertyChangedBase, IComparable, ILoggableObject, ITaxonomyIdManagerProvider, INamedObject {

        private SemaphoreSlim semaphore = new SemaphoreSlim(1);

        public Company() { TaxonomyInfo = TaxonomyManager.GetLatestTaxonomyInfo(TaxonomyType.GCD); }

        #region events
        public event LogHandler NewLog;
        private void OnNewLog(string element, object oldValue, object newValue) { if (NewLog != null) NewLog(this, new LogArgs(element, oldValue, newValue)); }
        #endregion events

        #region properties

        #region Id
        private int _id;

        [DbColumn("Id", AllowDbNull = false)]
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
        private string _name;

        [DbColumn("name", AllowDbNull = true, Length = 256)]
        public string Name {
            get { return _name; }
            set {
                if (_name == value) return;
                _name = StringUtils.Left(value, 256).Trim();
                OnPropertyChanged("Name");
                OnPropertyChanged("DisplayString");
            }
        }
        #endregion

        #region Owner
        private int _ownerId;
        private User _owner;

        [DbColumn("owner_id")]
        public int OwnerId {
            get { return _ownerId; }
            set {
                if (_ownerId == value) return;

                // log changed value (assigned owner)
                if (_ownerId > 0) OnNewLog("OwnerId", _ownerId, value);

                _ownerId = value;
            }
        }

        public User Owner {
            get { return _owner; }
            set {
                if (_owner == value) return;
                _owner = value;
                OwnerId = value.Id;
            }
        }

        #endregion Owner

        #region TaxonomyInfoId
        private int _taxonomyInfoId;

        [DbColumn("taxonomy_info_id")]
        public int TaxonomyInfoId {
            get { return _taxonomyInfoId; }
            private set {
                if (_taxonomyInfoId == value)
                    return;
                _taxonomyInfoId = value;
                TaxonomyInfo = TaxonomyManager.GetTaxonomyInfo(TaxonomyInfoId);
            }
        }
        #endregion

        #region Taxonomy
        public ITaxonomy Taxonomy { get { return TaxonomyManager.GetTaxonomy(TaxonomyInfo); } }
        #endregion

        #region TaxonomyInfo
        private ITaxonomyInfo _taxonomyInfo;

        public ITaxonomyInfo TaxonomyInfo {
            get { return _taxonomyInfo; }
            set {
                if (_taxonomyInfo == value)
                    return;
                _taxonomyInfo = value;
                TaxonomyInfoId = ((TaxonomyInfo) value).Id;

                // reset _taxonomyIdManager
                _valueTree = null;
                _taxonomyIdManager = null;

                OnPropertyChanged("TaxonomyInfo");
            }
        }
        #endregion

        #region TaxonomyIdManager
        private TaxonomyIdManager _taxonomyIdManager;
        public TaxonomyIdManager TaxonomyIdManager { get { return _taxonomyIdManager ?? (_taxonomyIdManager = new TaxonomyIdManager(null, TaxonomyInfo)); } }
        #endregion

        #region FinancialYears
        private ObservableCollection<FinancialYear> _financialYears;

        [DbCollection("Company", LazyLoad = false, SortOnLoad = true)]
        public ObservableCollection<FinancialYear> FinancialYears { get { return _financialYears ?? (_financialYears = new ObservableCollection<FinancialYear>()); } set { _financialYears = value; } }
        #endregion FinancialYears

        #region Parent
        public Company Parent { get; set; }
        #endregion Parent

        #region VisibleFinancialYears
        private readonly ObservableCollection<FinancialYear> _visibleFinancialYears =
            new ObservableCollection<FinancialYear>();

        public ObservableCollection<FinancialYear> VisibleFinancialYears { get { return _visibleFinancialYears; } }
        #endregion VisibleFinancialYears

        #region ValueTree
        private ValueTree.ValueTree _valueTree;

        public ValueTree.ValueTree ValueTree {
            get {
                var progress = new ProgressInfo();
                return _valueTree ?? (_valueTree = Structures.ValueTree.ValueTree.CreateValueTree(
                    typeof (ValuesGCD_Company), this, Taxonomy, progress));
            }
            set { _valueTree = value; }
        }
        #endregion ValueTree

        #region IsTempObject
        private bool _isTempObject;

        /// <summary>
        /// Indicates if the instance has been created with the CreateTempObject method. Temp objects will not be persistated to database or create any log entries.
        /// </summary>
        public bool IsTempObject {
            get { return _isTempObject; }
            private set {
                if (_isTempObject == value) return;
                _isTempObject = value;
                OnPropertyChanged("IsTempObject");
            }
        }
        #endregion // IsTempObject

        #region DisplayString
        public string DisplayString { get { return string.IsNullOrEmpty(Name) ? "<" + ResourcesCommon.EmptyName + ">" : Name; } }
        #endregion

        #region IsValid
        private bool _isValid;

        public bool IsValid {
            get { return _isValid; }
            private set {
                _isValid = value;
                OnPropertyChanged("IsValid");
            }
        }
        #endregion

        #region HasAnyAssignedDocument
        private bool _hasAnyAssignedDocument = false;
        public bool HasAnyAssignedDocument {
            get {
                return _hasAnyAssignedDocument;
            }
        }

        #endregion

        #region SelectedLocationCountry
        public Country SelectedLocationCountry {
            get { return _selectedLocationCountry; }
            set {
                _selectedLocationCountry = value;

                var locationCountryValue =
                    ValueTree.Root.Values["de-gcd_genInfo.company.id.location.country"];
                locationCountryValue.Value = value.Name;

                var locationIsoValue =
                    ValueTree.Root.Values["de-gcd_genInfo.company.id.location.country.isoCode"];
                locationIsoValue.Value = value.Iso;

                OnPropertyChanged("SelectedLocationCountry");
            }
        }

        private Country _selectedLocationCountry = Country.Countries[0];

        #endregion SelectedLocationCountry

        #endregion properties

        #region methods

        #region CompareTo
        public int CompareTo(object obj) {
            if (!(obj is Company)) return 0;
            var company = (Company) obj;
            return Name == null
                       ? (company.Name == null ? 0 : 1)
                       : (company.Name == null ? -1 : Name.CompareTo(company.Name));
        }
        #endregion

        #region SetFinancialYearIntervall
        public void SetFinancialYearIntervall(int firstFYear, int lastFYear) {
            using (IDatabase conn = AppConfig.ConnectionManager.GetConnection()) {
                conn.BeginTransaction();
                try {
                    conn.DbMapping.Delete<FinancialYear>(conn.Enquote("company_id") + "=" + Id);
                    FinancialYears.Clear();

                    var fyear = new FinancialYear {
                        Company = this,
                        FYear = firstFYear - 1,
                        FiscalYearBegin = new DateTime(firstFYear - 1, 1, 1),
                        FiscalYearEnd = new DateTime(firstFYear - 1, 12, 31),
                        BalSheetClosingDate = new DateTime(firstFYear - 1, 12, 31),
                        FiscalYearBeginPrevious = new DateTime(firstFYear - 2, 1, 1),
                        FiscalYearEndPrevious = new DateTime(firstFYear - 2, 12, 31),
                        BalSheetClosingDatePreviousYear = new DateTime(firstFYear - 2, 12, 31),
                    };

                    conn.DbMapping.Save(fyear);
                    FinancialYears.Add(fyear);

                    for (int curYear = firstFYear; curYear <= lastFYear; curYear++) {
                        fyear = new FinancialYear {
                            Company = this,
                            FYear = curYear,
                            FiscalYearBegin = new DateTime(curYear, 1, 1),
                            FiscalYearEnd = new DateTime(curYear, 12, 31),
                            BalSheetClosingDate = new DateTime(curYear, 12, 31),
                            FiscalYearBeginPrevious = new DateTime(curYear - 1, 1, 1),
                            FiscalYearEndPrevious = new DateTime(curYear - 1, 12, 31),
                            BalSheetClosingDatePreviousYear = new DateTime(curYear - 1, 12, 31),
                        };

                        conn.DbMapping.Save(fyear);
                        FinancialYears.Add(fyear);
                    }
                    conn.CommitTransaction();
                } catch (Exception ex) {
                    conn.RollbackTransaction();
                    throw new Exception(ex.Message, ex);
                }
            }
        }
        #endregion SetFinancialYearIntervall

        #region SetVisibleFinancialYearIntervall
        public void SetVisibleFinancialYearIntervall(int from, int to) {

            List<FinancialYear> changedValues = new List<FinancialYear>();

            //enable/disable the financial years and update in database if necessary
            foreach (FinancialYear financialYear in FinancialYears) {
                if (financialYear.FYear >= from && financialYear.FYear <= to) {
                    if (!financialYear.IsEnabled) {
                        financialYear.IsEnabled = true;
                        changedValues.Add(financialYear);
                    }
                } else {
                    if (financialYear.IsEnabled) {

                        // Check if there is a financial year that has to become disabled but still has a report 
                        if (DocumentManager.Instance.Documents.Any(doc => doc.FinancialYear == financialYear)) {
                            throw new Exception(ExceptionMessages.FinancialYearInUse);
                        }

                        financialYear.IsEnabled = false;
                        changedValues.Add(financialYear);
                    }
                }
            }

            using (IDatabase conn = AppConfig.ConnectionManager.GetConnection()) {
                foreach (var financialYear in changedValues) {
                    conn.DbMapping.Save(financialYear);
                }
            }

            // delete all financial years which do not belong in the list
            for (int i = VisibleFinancialYears.Count - 1; i >= 0; --i)
                if (VisibleFinancialYears[i].FYear < from || VisibleFinancialYears[i].FYear > to)
                    VisibleFinancialYears.RemoveAt(i);

            // add the financial years in a sorted fashion
            foreach (FinancialYear financialYear in FinancialYears) {
                if (financialYear.IsEnabled && !VisibleFinancialYears.Contains(financialYear)) {
                    int i;
                    for (i = VisibleFinancialYears.Count - 1; i >= 0; --i)
                        if (financialYear.FYear > VisibleFinancialYears[i].FYear) {
                            VisibleFinancialYears.Insert(i + 1, financialYear);
                            break;
                        }
                    if (i == -1) VisibleFinancialYears.Insert(0, financialYear);
                }
            }
        }
        #endregion SetVisibleFinancialYearIntervall

        public FinancialYear GetFinancialYearOrDefault(int id) { return FinancialYears.Where(fyear => fyear.Id == id).DefaultIfEmpty(VisibleFinancialYears[0]).First(); }
        public override string ToString() { return DisplayString; }

        public FinancialYear GetMaybeFinancialYear(int id) { return FinancialYears.Where(fyear => fyear.Id == id).DefaultIfEmpty(null).First(); }

        public void NotifyAssignedReportChanged() {
            semaphore.Wait();
            try {
                _hasAnyAssignedDocument = DocumentManager.Instance.Documents.Any(d => d.CompanyId == this.Id);
                OnPropertyChanged("HasAnyAssignedDocument");
            } finally {
                semaphore.Release();
            }
        }

        #endregion methods
    }
}