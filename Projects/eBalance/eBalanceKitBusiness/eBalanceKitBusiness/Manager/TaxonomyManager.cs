// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-06-26
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using DbAccess;
using Taxonomy;
using Taxonomy.Enums;
using Taxonomy.Interfaces;
using Utils;
using eBalanceKitBusiness.MappingTemplate;
using eBalanceKitBusiness.Structures;
using System.IO;
using eBalanceKitBusiness.Structures.DbMapping.MappingTemplate;
using eBalanceKitBusiness.Structures.DbMapping.ValueMappings;
using eBalanceKitBusiness.Structures.ValueTree;
using eBalanceKitBusiness.Structures.XbrlElementValue;
using ErrorEventArgs = System.IO.ErrorEventArgs;
using TaxonomyInfo = eBalanceKitBusiness.Structures.DbMapping.TaxonomyInfo;

namespace eBalanceKitBusiness.Manager {

    public static class TaxonomyManager {

        public static HashSet<string> ElementsWithExplanation {
            get {
                return new HashSet<string> {
                    "genInfo.company.id.legalStatus.legalStatus.Other.EUNERL",
                    "genInfo.company.id.legalStatus.legalStatus.Other.KOERERL",
                    "genInfo.company.id.legalStatus.legalStatus.Other.MUNERL",
                    "genInfo.company.id.legalStatus.legalStatus.SRERL",
                    "genInfo.report.preparationAttestation.attester.type.SERL",
                    "genInfo.report.audit.auditor.type.auditorType.SERL",
                    "genInfo.company.id.idNo.type.companyId.SERL",
                    "genInfo.report.id.consolidationRange.consolidationRange.SERL",
                    "genInfo.report.id.incomeStatementFormat.incomeStatementFormat.SERL",
                    "genInfo.report.id.accountingStandard.accountingStandard.SERL",
                    "genInfo.report.id.reportElement.reportElements.SERL",
                    "genInfo.report.id.reportElement.reportElements.SAERL",
                    "genInfo.report.id.reportType.reportType.SERL",
                    "genInfo.company.id.industry.keyType.industryKey.SERL",
                    "genInfo.company.id.idNo.type.companyId.SERL"
                };
            }
        }

        static TaxonomyManager() {
            DictChangedTaxonomy.Add("de-gcd_genInfo.doc.author.function.fuctionType.head", "de-gcd_genInfo.doc.author.function.functionType.head");
            DictChangedTaxonomy.Add("de-gcd_genInfo.doc.author.function.fuctionType.contactPerson", "de-gcd_genInfo.doc.author.function.functionType.contactPerson");
            DictChangedTaxonomy.Add("de-gcd_genInfo.doc.author.function.fuctionType.taxAdvisor", "de-gcd_genInfo.doc.author.function.functionType.taxAdvisor");
            DictChangedTaxonomy.Add("de-gcd_genInfo.company.id.shareholder.SpecialBalanceRequiered", "de-gcd_genInfo.company.id.shareholder.SpecialBalanceRequired");
            DictChangedTaxonomy.Add("de-gcd_genInfo.company.id.shareholder.extensionRequiered", "de-gcd_genInfo.company.id.shareholder.extensionRequired");
            DictChangedTaxonomy.Add("de-gaap-ci_is.netIncome.regular.operatingTC.grossTradingProfit.totalOutput.netSales.grossSales.reductedRateVAT", "de-gaap-ci_is.netIncome.regular.operatingTC.grossTradingProfit.totalOutput.netSales.grossSales.reducedRateVAT");
            DictChangedTaxonomy.Add("de-gaap-ci_is.netIncome.regular.operatingTC.grossTradingProfit.totalOutput.netSales.reductionsFromGrossSales.reductedRateVAT", "de-gaap-ci_is.netIncome.regular.operatingTC.grossTradingProfit.totalOutput.netSales.reductionsFromGrossSales.reducedRateVAT");
            DictChangedTaxonomy.Add("de-gaap-ci_is.netIncome.regular.operatingTC.coopertive.Refunds", "de-gaap-ci_is.netIncome.regular.operatingTC.cooperative.Refunds");
            DictChangedTaxonomy.Add("de-gaap-ci_is.netIncome.regular.operatingCOGS.grossOpProfit.netSales.grossSales.reductedRateVAT", "de-gaap-ci_is.netIncome.regular.operatingCOGS.grossOpProfit.netSales.grossSales.reducedRateVAT");
            DictChangedTaxonomy.Add("de-gaap-ci_is.netIncome.regular.operatingCOGS.grossOpProfit.netSales.reductionsFromGrossSales.reductedRateVAT", "de-gaap-ci_is.netIncome.regular.operatingCOGS.grossOpProfit.netSales.reductionsFromGrossSales.reducedRateVAT");
            DictChangedTaxonomy.Add("de-gaap-ci_is.netIncome.regular.operatingCOGS.materialServices.material.rawMatConsSup.reductedRateVAT", "de-gaap-ci_is.netIncome.regular.operatingCOGS.materialServices.material.rawMatConsSup.reducedRateVAT");
            DictChangedTaxonomy.Add("de-gaap-ci_is.netIncome.regular.operatingCOGS.materialServices.material.purchased.reductedRateVAT", "de-gaap-ci_is.netIncome.regular.operatingCOGS.materialServices.material.purchased.reducedRateVAT");
            DictChangedTaxonomy.Add("de-gaap-ci_nt.genInfo.derivatetFinancialinstruments", "de-gaap-ci_nt.genInfo.derivatedFinancialinstruments");
            DictChangedTaxonomy.Add("de-gaap-ci_nt.genInfo.derivatetFinancialinstruments.name", "de-gaap-ci_nt.genInfo.derivatedFinancialinstruments.name");
            DictChangedTaxonomy.Add("de-gaap-ci_nt.genInfo.derivatetFinancialinstruments.value", "de-gaap-ci_nt.genInfo.derivatedFinancialinstruments.value");
            DictChangedTaxonomy.Add("de-gaap-ci_nt.genInfo.derivatetFinancialinstruments.measureMeth", "de-gaap-ci_nt.genInfo.derivatedFinancialinstruments.measureMeth");
            DictChangedTaxonomy.Add("de-gaap-ci_nt.genInfo.derivatetFinancialinstruments.AcBalSheetPos", "de-gaap-ci_nt.genInfo.derivatedFinancialinstruments.AcBalSheetPos");
            DictChangedTaxonomy.Add("de-gaap-ci_nt.genInfo.derivatetFinancialinstruments.netBookValue", "de-gaap-ci_nt.genInfo.derivatedFinancialinstruments.netBookValue");
            DictChangedTaxonomy.Add("de-gaap-ci_nt.genInfo.derivatetFinancialinstruments.ReasonsforUncertain", "de-gaap-ci_nt.genInfo.derivatedFinancialinstruments.ReasonsforUncertain");
            DictChangedTaxonomy.Add("de-gaap-ci_hbst.accountbalances.positionname", "de-gaap-ci_detailedInformation.accountbalances.positionName");
            DictChangedTaxonomy.Add("de-gaap-ci_hbst.accountbalances.accountnumber", "de-gaap-ci_detailedInformation.accountbalances.accountNumber");
            DictChangedTaxonomy.Add("de-gaap-ci_hbst.accountbalances.accountdescription", "de-gaap-ci_detailedInformation.accountbalances.accountDescription");
            DictChangedTaxonomy.Add("de-gaap-ci_hbst.accountbalances.amount", "de-gaap-ci_detailedInformation.accountbalances.amount");
        }

        #region properties
        public static Taxonomy.ITaxonomy GCD_Taxonomy { get { return GetTaxonomy(GetLatestTaxonomyInfo(TaxonomyType.GCD)); } }

        private static readonly Dictionary<TaxonomyType, List<ITaxonomyInfo>> TaxonomyInfos
            = new Dictionary<TaxonomyType, List<ITaxonomyInfo>>();

        private static readonly Dictionary<int, ITaxonomyInfo> TaxonomyInfoById = new Dictionary<int, ITaxonomyInfo>();

        private static readonly Dictionary<string, ITaxonomyInfo> TaxonomyInfoByName = new Dictionary<string, ITaxonomyInfo>();

        private static readonly Dictionary<ITaxonomyInfo, ITaxonomy> Taxonomies = new Dictionary<ITaxonomyInfo, ITaxonomy>();

        private static readonly Dictionary<string, string> DictChangedTaxonomy = new Dictionary<string, string>();
        
        #endregion

        #region events
        public static event ErrorEventHandler Error;
        private static void OnError(Exception ex) { if (Error != null) Error(null, new ErrorEventArgs(ex)); }
        #endregion
        
        #region InitTaxonomyTable
        /// <summary>
        /// Initializes table_info table with references to official taxonomy files.
        /// </summary>
        /// <param name="conn"></param>
        public static void InitTaxonomyTable(IDatabase conn) {
            var taxonomyInfos = new List<ITaxonomyInfo> {
                    new TaxonomyInfo {Type = TaxonomyType.GCD, Name = "de-gcd-2010-12-16",  Path = @"Taxonomy\base\de-gcd-2010-12-16", Filename = "de-gcd-2010-12-16-shell.xsd", Version = "2010-12-16" },
                    new TaxonomyInfo {Type = TaxonomyType.GAAP, Name = "de-gaap-2010-12-16", Path = @"Taxonomy\base\de-gaap-ci-2010-12-16", Filename = "de-gaap-ci-2010-12-16-shell-fiscal.xsd", Version = "2010-12-16" },

                    new TaxonomyInfo {Type = TaxonomyType.GCD, Name = "de-gcd-2011-09-14",  Path = @"Taxonomy\base\de-gcd-2011-09-14", Filename = "de-gcd-2011-09-14-shell.xsd", Version = "2011-09-14" },
                    new TaxonomyInfo {Type = TaxonomyType.GAAP, Name = "de-gaap-2011-09-14", Path = @"Taxonomy\base\de-gaap-ci-2011-09-14", Filename = "de-gaap-ci-2011-09-14-shell-fiscal.xsd", Version = "2011-09-14" },
                    new TaxonomyInfo {Type = TaxonomyType.OtherBusinessClass, Name = "de-bra-2011-09-14", Path = @"Taxonomy\base\de-bra-2011-09-14", Filename = "de-bra-2011-09-14-shell-fiscal.xsd", Version = "2011-09-14" },
                    new TaxonomyInfo {Type = TaxonomyType.Financial, Name = "de-fi-2011-09-14", Path = @"Taxonomy\fi\de-fi-2011-09-14", Filename = "de-fi-2011-09-14-shell-staffelform-fiscal.xsd", Version = "2011-09-14" },
                    new TaxonomyInfo {Type = TaxonomyType.Insurance, Name = "de-ins-2011-09-14", Path = @"Taxonomy\ins\de-ins-2011-09-14", Filename = "de-ins-2011-09-14-shell-fiscal.xsd", Version = "2011-09-14" },
                    
                    
                    new TaxonomyInfo {Type = TaxonomyType.GCD, Name = "de-gcd-2012-06-01",  Path = @"Taxonomy\base\de-gcd-2012-06-01", Filename = "de-gcd-2012-06-01-shell.xsd", Version = "2012-06-01" },
                    new TaxonomyInfo {Type = TaxonomyType.GAAP, Name = "de-gaap-2012-06-01", Path = @"Taxonomy\base\de-gaap-ci-2012-06-01", Filename = "de-gaap-ci-2012-06-01-shell-fiscal.xsd", Version = "2012-06-01" },
                    new TaxonomyInfo {Type = TaxonomyType.OtherBusinessClass, Name = "de-bra-2012-06-01", Path = @"Taxonomy\base\de-bra-2012-06-01", Filename = "de-bra-2012-06-01-shell-fiscal.xsd", Version = "2012-06-01" },
                    new TaxonomyInfo {Type = TaxonomyType.Financial, Name = "de-fi-2012-06-01", Path = @"Taxonomy\fi\de-fi-2012-06-01", Filename = "de-fi-2012-06-01-shell-staffelform-fiscal.xsd", Version = "2012-06-01" },
                    new TaxonomyInfo {Type = TaxonomyType.Insurance, Name = "de-ins-2012-06-01", Path = @"Taxonomy\ins\de-ins-2012-06-01", Filename = "de-ins-2012-06-01-shell-fiscal.xsd", Version = "2012-06-01" },

                    //Bundesanzeiger taxonomien
                    //new TaxonomyInfo {Type = TaxonomyType.GCD, Name = "de-gcd-2011-02-28", Path = @"Taxonomy\base\de-gcd-2011-02-28", Filename = "de-gcd-2011-02-28-shell.xsd", Version = "2011-02-28" },
                    //new TaxonomyInfo {Type = TaxonomyType.GAAP, Name = "de-gaap-ci-2011-02-28", Path = @"Taxonomy\base\de-gaap-ci-2011-02-28", Filename = "de-gaap-ci-2011-02-28-shell.xsd", Version = "2011-02-28" },
                    //new TaxonomyInfo {Type = TaxonomyType.Financial, Name = "de-fi-2008-02-19", Path = @"Taxonomy\fi\de-fi-2008-02-19", Filename = "de-fi-2008-02-19-shell-staffelform.xsd", Version = "2008-02-19" },
                    //new TaxonomyInfo {Type = TaxonomyType.GAAP, Name = "de-gaap-ci-2008-02-24", Path = @"Taxonomy\fi\de-gaap-ci-2008-02-24", Filename = "de-gaap-ci-2007-12-01-shell.xsd", Version = "2007-12-01" }
                };
            try {
                //conn.BeginTransaction();
                conn.DbMapping.Save(typeof(TaxonomyInfo), taxonomyInfos);
                //conn.CommitTransaction();
            } catch (Exception ex) {
                //conn.RollbackTransaction(); 
                throw new Exception(ex.Message);
            }
        }
        #endregion

        #region GetLatestTaxonomyInfo
        public static ITaxonomyInfo GetLatestTaxonomyInfo(TaxonomyType taxonomyType) { return TaxonomyInfos[taxonomyType].First(); }
        #endregion

        #region InitGAAPMembers

        public static void InitGAAPMembers(ITaxonomy t) {
            IElement elem;
            t.Elements.TryGetValue("de-gaap-ci_detailedInformation.accountBalances", out elem);
            if (elem != null) elem.IsPersistant = false;
        }
        #endregion

        #region InitGCDMembers
        public static void InitGCDMembers(ITaxonomy t, Type valuesGcdCompanyType) {

            SetTableType(t.Elements["de-gcd_genInfo.company"].PresentationTreeNodes.First(), valuesGcdCompanyType);
            
            //New: Done by using styles (Taxonomy project)

            //// The following elements and subelements of these are not included in the value tree.
            //// The period values could be received from the Document.FinancialYear property.
            //t.Elements["de-gcd_genInfo.report.period.fiscalYearBegin"].IsPersistant = false;
            //t.Elements["de-gcd_genInfo.report.period.fiscalYearEnd"].IsPersistant = false;
            //t.Elements["de-gcd_genInfo.report.period.balSheetClosingDate"].IsPersistant = false;
            //t.Elements["de-gcd_genInfo.report.period.fiscalPreciousYearBegin"].IsPersistant = false;
            //t.Elements["de-gcd_genInfo.report.period.fiscalPreciousYearEnd"].IsPersistant = false;
            //t.Elements["de-gcd_genInfo.report.period.balSheetClosingDatePreviousYear"].IsPersistant = false;
        }
        #endregion

        #region SetTableType
        private static void SetTableType(Taxonomy.Interfaces.PresentationTree.IPresentationTreeNode root, Type tableType) {
            if (root.Element.Parents.Count == 0) root.Element.TableType = tableType;
            foreach (var child in root.Children) SetTableType(child as Taxonomy.Interfaces.PresentationTree.IPresentationTreeNode, tableType);
        }
        #endregion

        #region InitTaxonomyInfos
        public static void InitTaxonomyInfos() {
            using (var conn = AppConfig.ConnectionManager.GetConnection()) {
                foreach (var taxonomyInfo in conn.DbMapping.Load<TaxonomyInfo>()) {
                    List<ITaxonomyInfo> taxonomyInfoList;
                    TaxonomyInfos.TryGetValue(taxonomyInfo.Type, out taxonomyInfoList);
                    if (taxonomyInfoList == null) {
                        taxonomyInfoList = new List<ITaxonomyInfo>();
                        TaxonomyInfos[taxonomyInfo.Type] = taxonomyInfoList;                        
                    }
                    TaxonomyInfoById[taxonomyInfo.Id] = taxonomyInfo;
                    TaxonomyInfoByName[taxonomyInfo.Name] = taxonomyInfo;
                    taxonomyInfoList.Add(taxonomyInfo);
                }
            }

            foreach (var taxonomyInfoList in TaxonomyInfos.Values) {
                var sortedCollection = new List<ITaxonomyInfo>(
                    from item in taxonomyInfoList
                    orderby item.Version descending 
                    select item).ToList();
                taxonomyInfoList.Clear();
                taxonomyInfoList.AddRange(sortedCollection);
            }

            new Thread(PreloadLatestTaxonomies) { CurrentCulture = Thread.CurrentThread.CurrentCulture, CurrentUICulture = Thread.CurrentThread.CurrentUICulture }.Start();
        }
        #endregion

        #region GetTaxonomy
        public static ITaxonomy GetTaxonomy(ITaxonomyInfo taxonomyInfo) {
            ITaxonomy result;
            lock (Taxonomies) {

                Taxonomies.TryGetValue(taxonomyInfo, out result);
                if (result != null) return result;

                // load taxonomy, if not yet loaded
                try {
                    result = new Taxonomy.Taxonomy(taxonomyInfo);
                    Taxonomies[taxonomyInfo] = result;
                    InitGAAPMembers(result);

                    if (taxonomyInfo.Type == TaxonomyType.GCD) {
                        Type valuesGcdCompanyType = typeof (ValuesGCD_Company);
                        InitGCDMembers(result, valuesGcdCompanyType);
                    }
                } catch (Exception ex) {
                    OnError(new Exception(Localisation.ExceptionMessages.TaxonomiesLoadError + ex.Message));
                }
            }
            return result;
        }
        #endregion

        public static ITaxonomyInfo GetTaxonomyInfo(int id) {
            ITaxonomyInfo taxonomyInfo;
            TaxonomyInfoById.TryGetValue(id, out taxonomyInfo);
            return taxonomyInfo;
        }

        public static ITaxonomyInfo GetTaxonomyInfo(string name) {
            ITaxonomyInfo taxonomyInfo;
            TaxonomyInfoByName.TryGetValue(name, out taxonomyInfo);
            return taxonomyInfo;
        }

        public static DocumentUpgradeResult UpgradeDocumentTaxonomyVersion(ITaxonomyInfo taxonomyInfo, ValueTree valueTree) {
            var taxonomy = GetTaxonomy(taxonomyInfo);
            var latestTaxonomyInfo = GetLatestTaxonomyInfo(taxonomyInfo.Type);
            var latestTaxonomy = GetTaxonomy(latestTaxonomyInfo);

            var result = new DocumentUpgradeResult();

            var missingValues = new List<IValueMapping>();
            Action<IValueMapping> addMissingValues = null;
            addMissingValues = (root) => {
                missingValues.Add(root);
                foreach (var child in ((ValueMappingBase) root).Children)
                    addMissingValues(child as IValueMapping);
            };

            var newTaxonomyIdManager = new TaxonomyIdManager(null, latestTaxonomyInfo);

            // collect missing values
            foreach (IElement element in taxonomy.Elements.Values) {
                if (!latestTaxonomy.Elements.ContainsKey(element.Id)) {
                    foreach (var missingValue in valueTree.GetValues(element.Id)) {
                        if (missingValue != null) {
                            if (DictChangedTaxonomy.ContainsKey(missingValue.Element.Id)) {
                                var newId = DictChangedTaxonomy[missingValue.Element.Id];
                                missingValue.DbValue.ElementId = newTaxonomyIdManager.GetId(newId);
                                missingValue.DbValue.Element =
                                    newTaxonomyIdManager.GetElement(missingValue.DbValue.ElementId);
                                result.UpdatedValues.Add(missingValue.DbValue);
                            }
                            else {

                                if (!missingValue.Element.IsPersistant) {
                                    continue;
                                }

                                missingValues.Add(missingValue.DbValue);
                                //addMissingValues(missingValue.DbValue);
                            }
                        }
                    }
                }
            }

            // search missing values if any exist
            if (missingValues.Count > 0) {
                foreach (var deletedValue in missingValues.Where(deletedValue => deletedValue.Value != null)) {
                    result.AddDeletedValue(
                        new UpgradeMissingValue(deletedValue.Element, deletedValue.Value));
                }

                result.MissingValues.AddRange(missingValues);
            }

            if (!result.HasDeletedValues) {
                result.AddDeletedValue(new UpgradeMissingValue(null, "Es wurden keine befüllten Positionen gelöscht."));
            }


            return result;
        }

        public static UpdateTemplateResultModel UpgradeTemplate(MappingTemplateHead template) {

            var result = new UpdateTemplateResultModel(template);

            var taxonomy = GetTaxonomy(GetLatestTaxonomyInfo(template.TaxonomyInfo.Type));

            foreach (var templateLine in template.Assignments) {
                if (templateLine.DebitElementId != null &&
                    !taxonomy.Elements.ContainsKey(templateLine.DebitElementId)) {

                    var element = template.TaxonomyIdManager.GetElement(templateLine.DebitElementId);
                    templateLine.DebitElementId = DictChangedTaxonomy.ContainsKey(templateLine.DebitElementId)
                                                      ? DictChangedTaxonomy[templateLine.DebitElementId]
                                                      : null;
                    result.AddDeletedEntry(templateLine.AccountLabel, "S", element);
                }

                if (templateLine.CreditElementId != null &&
                    !taxonomy.Elements.ContainsKey(templateLine.CreditElementId)) {

                    var element = template.TaxonomyIdManager.GetElement(templateLine.CreditElementId);
                    templateLine.CreditElementId = DictChangedTaxonomy.ContainsKey(templateLine.CreditElementId)
                                                       ? DictChangedTaxonomy[templateLine.CreditElementId]
                                                       : null;
                    result.AddDeletedEntry(templateLine.AccountLabel, "H", element);
                }
            }

            if (!result.HasDeletedEntries)
                result.AddDeletedEntry("Es wurden keine Zuordnungen gelöscht.", null, null);

            return result;
        }

        private static void PreloadLatestTaxonomies() {
            GetTaxonomy(GetLatestTaxonomyInfo(TaxonomyType.GCD));
            //GetTaxonomy(GetLatestTaxonomyInfo(TaxonomyType.GAAP));
            //GetTaxonomy(GetLatestTaxonomyInfo(TaxonomyType.Financial));
            //GetTaxonomy(GetLatestTaxonomyInfo(TaxonomyType.Insurance));
            //GetTaxonomy(GetLatestTaxonomyInfo(TaxonomyType.OtherBusinessClass));
        }
    }
}
