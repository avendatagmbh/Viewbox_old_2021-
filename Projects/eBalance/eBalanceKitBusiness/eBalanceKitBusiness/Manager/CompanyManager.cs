// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-01-04
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Utils;
using eBalanceKitBusiness.Localisation;
using eBalanceKitBusiness.Logs;
using eBalanceKitBusiness.Rights;
using eBalanceKitBusiness.Structures;
using eBalanceKitBusiness.Structures.DbMapping;
using eBalanceKitBusiness.Structures.DbMapping.ValueMappings;

namespace eBalanceKitBusiness.Manager {
    /// <summary>
    /// This class provides several company management functions.
    /// </summary>
    public class CompanyManager : NotifyPropertyChangedBase {
        
        private CompanyManager() {
            AllowedCompanies = new ObservableCollectionAsync<Company>();
            DictChangedTaxonomy.Add("de-gcd_genInfo.company.id.shareholder.SpecialBalanceRequiered", "de-gcd_genInfo.company.id.shareholder.SpecialBalanceRequired");
            DictChangedTaxonomy.Add("de-gcd_genInfo.company.id.shareholder.extensionRequiered", "de-gcd_genInfo.company.id.shareholder.extensionRequired");

            AllowedCompanies = new ObservableCollectionAsync<Company>();
            NotAllowedCompanies = new ObservableCollectionAsync<Company>();
            AllowedCompanies.CollectionChanged += AllowedCompaniesOnCollectionChanged;
        }

        private void AllowedCompaniesOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs) {
            OnPropertyChanged("HasAllowedCompanies");
            OnPropertyChanged("HasDeletableCompanies");
        }

        private static CompanyManager _instance;

        public static CompanyManager Instance { get { return _instance ?? (_instance = new CompanyManager()); } }

        #region properties

        private readonly Dictionary<int, Company> CompanyById = new Dictionary<int, Company>();
        private readonly Dictionary<string, string> DictChangedTaxonomy = new Dictionary<string, string>();

        #region Companies
        // ReSharper disable InconsistentNaming
        private readonly ObservableCollection<Company> _companies = new ObservableCollection<Company>();
        // ReSharper restore InconsistentNaming
        internal IEnumerable<Company> Companies { get { return _companies; } }
        public Company CurrentCompany { get; set; }
        public FinancialYear CurrentFinancialYear { get; set; }
        #endregion

        public ObservableCollectionAsync<Company> AllowedCompanies { get; private set; }
        public readonly ObservableCollectionAsync<Company> NotAllowedCompanies = new ObservableCollectionAsync<Company>();

        #region HasAllowedCompanies
        public bool HasAllowedCompanies { get { return AllowedCompanies.Any(); } }
        #endregion // HasAllowedCompanies

        #region HasDeletableCompanies
        public bool HasDeletableCompanies { get { return AllowedCompanies.Any(c => !c.HasAnyAssignedDocument); } }
        #endregion // HasDeletableCompanies

        #endregion properties

        #region methods

        #region Init

        public static CompanyUpgradeResults Init(bool doTaxonomyUpgrade) {

            List<Company> tmp;
            using (var conn = AppConfig.ConnectionManager.GetConnection()) {
                tmp = conn.DbMapping.Load<Company>();
            }
            tmp.Sort();

            var companyUpgradeResults = new CompanyUpgradeResults();
            
            Instance.CompanyById.Clear();
            foreach (var company in tmp) {
                Instance.CompanyById[company.Id] = company;
                LogManager.Instance.NewCompany(company, true);

                // init assigned financial years
                FinancialYear lastFYear = null;
                foreach (var fyear in company.FinancialYears) {
                    if (lastFYear != null) {
                        fyear.FiscalYearBeginPrevious = lastFYear.FiscalYearBegin;
                        fyear.FiscalYearEndPrevious = lastFYear.FiscalYearEnd;
                        fyear.BalSheetClosingDatePreviousYear = lastFYear.BalSheetClosingDate;
                        lastFYear.Successor = fyear;
                        fyear.Predecessor = lastFYear;
                    }

                    if (fyear.IsEnabled) company.VisibleFinancialYears.Add(fyear);

                    lastFYear = fyear;
                }

                company.Owner = UserManager.Instance.GetUser(company.OwnerId);

                if (doTaxonomyUpgrade)
                    Instance.UpdateCompanyTaxonomy(companyUpgradeResults, company);
            }

            Instance._companies.Clear();
            foreach (var company in tmp) {
                Instance._companies.Add(company);
            }

            return companyUpgradeResults;
        }

        #endregion

        private void UpdateCompanyTaxonomy(CompanyUpgradeResults companyUpgradeResults, Company company) {
            var latestTaxonomyInfo = TaxonomyManager.GetLatestTaxonomyInfo(company.TaxonomyInfo.Type);

            if (company.TaxonomyInfo.Version == latestTaxonomyInfo.Version) return;

            var latestTaxonomy = TaxonomyManager.GetTaxonomy(latestTaxonomyInfo);
            var companyUpgradeResult = companyUpgradeResults.CreateResult(company);
            var oldTaxonomy = TaxonomyManager.GetTaxonomy(company.TaxonomyInfo);
            var listOfMissingValues = new List<ValuesGCD_Company>();

            Action<ValuesGCD_Company> addMissingValues = null;
            addMissingValues = root => {
                listOfMissingValues.Add(root);
                foreach (var child in root.Children) {
                    // ReSharper disable PossibleNullReferenceException
                    // ReSharper disable AccessToModifiedClosure
                    addMissingValues(child as ValuesGCD_Company);
                    // ReSharper restore AccessToModifiedClosure
                    // ReSharper restore PossibleNullReferenceException
                }
            };

            var newTaxonomyIdManager = new TaxonomyIdManager(null, latestTaxonomyInfo);

            // collect missing values
            foreach (
                var missingValue in
                    oldTaxonomy.Elements.Values.Where(element => !latestTaxonomy.Elements.ContainsKey(element.Id)).
                        SelectMany(element => company.ValueTree.GetValues(element.Id))) {

                if (DictChangedTaxonomy.ContainsKey(missingValue.Element.Id)) {
                    var newId = DictChangedTaxonomy[missingValue.Element.Id];
                    missingValue.DbValue.ElementId = newTaxonomyIdManager.GetId(newId);
                    missingValue.DbValue.Element = newTaxonomyIdManager.GetElement(missingValue.DbValue.ElementId);
                    companyUpgradeResults.UpdatedValues.Add(missingValue.DbValue as ValuesGCD_Company);

                } else {
                    addMissingValues(missingValue.DbValue as ValuesGCD_Company);
                }
            }

            // search missing values if any exist
            if (listOfMissingValues.Count > 0) {
                foreach (var deletedValue in listOfMissingValues.Where(deletedValue => deletedValue.Value != null)) {
                    companyUpgradeResult.AddDeletedValue(
                        new UpgradeMissingValue(deletedValue.Element, deletedValue.Value));
                }

                companyUpgradeResults.DeletedValues.AddRange(listOfMissingValues);
            }

            // update the company Taxonomy to the latest version
            company.TaxonomyInfo = TaxonomyManager.GetLatestTaxonomyInfo(company.TaxonomyInfo.Type);

            if (!companyUpgradeResult.HasDeletedValues) {
                companyUpgradeResult.AddDeletedValue(new UpgradeMissingValue(null, "Es wurden keine befüllten Positionen gelöscht."));                
            }
        }

        #region ProcessUpdateChanges
        internal void ProcessUpdateChanges(CompanyUpgradeResults companyUpgradeResults) {

            // save updated values
            using (var conn = AppConfig.ConnectionManager.GetConnection()) {
                try {
                    conn.BeginTransaction();
                    try {
                        foreach (var updatedValue in companyUpgradeResults.UpdatedValues) {
                            conn.DbMapping.Save(updatedValue);
                        }
                        conn.CommitTransaction();
                    } catch
                        (Exception ex) {
                        conn.RollbackTransaction();
                        throw new Exception(ex.Message, ex);
                    }
                } catch (Exception ex) {
                    throw new Exception(ExceptionMessages.CompanyManagerUpdate + ex.Message);
                }
            }

            // delete missing values if any exist
            ValueManager.RemoveValues(companyUpgradeResults.DeletedValues);

            // save changes
            foreach (var result in companyUpgradeResults.Results) {
                SaveCompany(result.Company);
            }
        }
        #endregion

        #region AddCompany
        public void AddCompany(Company company) {
            using (var conn = AppConfig.ConnectionManager.GetConnection()) {
                try {
                    company.Owner = UserManager.Instance.CurrentUser;
                    conn.BeginTransaction();
                    try {
                        conn.DbMapping.Save(company);
                        LogManager.Instance.NewCompany(company, false);
                        conn.CommitTransaction();
                    } catch (Exception ex) {
                        conn.RollbackTransaction();
                        throw new Exception(ex.Message, ex);
                    }

                    _companies.Add(company);
                    CompanyById[company.Id] = company;
                    AllowedCompanies.Add(company);
                    RightManager.CompanyAdded(company);
                } catch (Exception ex) {
                    throw new Exception(ExceptionMessages.CompanyManagerAdd + ex.Message);
                }
            }

            OnPropertyChanged("HasAllowedCompanies");
            OnPropertyChanged("HasDeletableCompanies");
        }
        #endregion

        #region DeleteCompany
        public void DeleteCompany(Company company) {
            using (var conn = AppConfig.ConnectionManager.GetConnection()) {
                if (conn.DbMapping.ExistsValue(typeof (Document), conn.Enquote("company_id") + "=" + company.Id)) {
                    throw new Exception(ExceptionMessages.CompanyInUse);
                }

                try {
                    LogManager.Instance.DeleteCompany(company);
                    RightManager.CompanyDeleted(company);
                    conn.BeginTransaction();
                    try {
                        conn.ExecuteNonQuery(
                            "DELETE FROM " + conn.Enquote(conn.DbMapping.GetTableName(typeof (ValuesGCD_Company))) +
                            " WHERE " + conn.Enquote("company_id") + "=" + company.Id);
                        conn.DbMapping.Delete(company);
                        conn.CommitTransaction();
                    } catch (Exception ex) {
                        conn.RollbackTransaction();
                        throw new Exception(ex.Message, ex);
                    }

                    _companies.Remove(company);
                    AllowedCompanies.Remove(company);
                    CompanyById.Remove(company.Id);
                } catch (Exception ex) {
                    throw new Exception(ExceptionMessages.CompanyManagerDelete + ex.Message);
                }
            }

            OnPropertyChanged("HasAllowedCompanies");
            OnPropertyChanged("HasDeletableCompanies");
        }
        #endregion

        #region SaveCompany
        public void SaveCompany(Company company) {
            using (var conn = AppConfig.ConnectionManager.GetConnection()) {
                try {
                    conn.BeginTransaction();
                    try {
                        conn.DbMapping.Save(company);
                        conn.CommitTransaction();
                    } catch (Exception ex) {
                        conn.RollbackTransaction();
                        throw new Exception(ex.Message, ex);
                    }
                } catch (Exception ex) {
                    throw new Exception(ExceptionMessages.CompanyManagerUpdate + ex.Message);
                }
            }
        }
        #endregion

        #region GetCompany
        public Company GetCompany(int id) { return CompanyById.ContainsKey(id) ? CompanyById[id] : null; }
        #endregion

        #region InitAllowedCompanies
        public void InitAllowedCompanies(RightDeducer rightDeducer) {
            AllowedCompanies.Clear();
            foreach (Company company in Companies.Where(rightDeducer.CompanyVisible))
                AllowedCompanies.Add(company);

            //DEVNOTE: no need to be updated because newly added companies will be allowed companies
            NotAllowedCompanies.Clear();
            foreach(Company company in Companies.Where(c => !AllowedCompanies.Any(ac => ac.Id == c.Id)))
                NotAllowedCompanies.Add(company);

            OnPropertyChanged("HasAllowedCompanies");
            OnPropertyChanged("HasDeletableCompanies");

        }
        #endregion

        public bool IsCompanyAllowed(Company company) {
            if (company == null)
                return false;
            return AllowedCompanies.Any(c => c.Id == company.Id);
        }

        public void NotifyAssignedReportChanged() { OnPropertyChanged("HasDeletableCompanies"); }
 
        #endregion methods
    }
}