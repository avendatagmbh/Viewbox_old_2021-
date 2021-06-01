// --------------------------------------------------------------------------------
// author: Sebastian Vetter
// since: 2012-06-06
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using Taxonomy.Enums;
using Taxonomy.Interfaces;
using System.Linq;
using eBalanceKitBusiness.Structures.DbMapping;

namespace eBalanceKitBusiness.Structures {
    /// <summary>
    /// Contains informations about the path to a report part depending on the taxonomy.
    /// </summary>
    public class ReportPartPath {

        public ReportPartPath(Document document) {

            Document = document;

            PartBalance = new ReportPart("de-gcd_genInfo.report.id.reportElement.reportElements.B",
                                         new List<string> {
                                             BalanceSheet
                                         });


            PartManagementReport = new ReportPart(
                "de-gcd_genInfo.report.id.reportElement.reportElements.L",
                new List<string> {
                    ManagementReport
                }
                );

            PartOpeningBalanceWithoutIncomeStatement = new ReportPart(
                "de-gcd_genInfo.report.id.reportElement.reportElements.EB",
                new List<string> {
                    null // ToDo: check, I am not sure about this
                }
                );

            PartInterimManagementReport = new ReportPart(
                "de-gcd_genInfo.report.id.reportElement.reportElements.ZL",
                new List<string> {
                    null // ToDo: find out what it is and where it is
                }
                );

            PartBalanceSheetAss = new ReportPart(
                "de-gcd_genInfo.report.id.reportElement.reportElements.BA",
                new List<string> {
                    null //BalanceSheetAss
                }
                );

            PartBalanceSheetEqLiab = new ReportPart(
                "de-gcd_genInfo.report.id.reportElement.reportElements.BP",
                new List<string> {
                    null //BalanceSheetEqLiab
                }
                );

            PartIncomeStatement = new ReportPart(
                "de-gcd_genInfo.report.id.reportElement.reportElements.GuV",
                new List<string> {
                    IncomeStatement
                }
                );

            PartContingentLiabilities = new ReportPart(
                "de-gcd_genInfo.report.id.reportElement.reportElements.H",
                new List<string> {
                    ContingentLiabilities
                }
                );

            PartTableGross = new ReportPart( //mit Entwicklung der Abschreibungen
                "de-gcd_genInfo.report.id.reportElement.reportElements.BAL",
                new List<string> {
                    TableGross
                }
                );

            PartTableGrossShort = new ReportPart( //- ohne Entwicklung der Abschreibungen
                "de-gcd_genInfo.report.id.reportElement.reportElements.BAK",
                new List<string> {
                    TableGrossShort
                }
                );

            PartTableNet = new ReportPart(
                "de-gcd_genInfo.report.id.reportElement.reportElements.NA",
                new List<string> {
                    TableNet
                }
                );

            PartIncomeUse = new ReportPart(
                "de-gcd_genInfo.report.id.reportElement.reportElements.EV",
                new List<string> {
                    IncomeUse
                }
                );

            PartCashFlowStatement = new ReportPart(
                "de-gcd_genInfo.report.id.reportElement.reportElements.CFS",
                new List<string> {
                    CashFlowStatement
                }
                );

            PartEquityChange = new ReportPart(
                "de-gcd_genInfo.report.id.reportElement.reportElements.EKE",
                new List<string> {
                    EquityChangeStatement // ToDo: find out what it is and where it is
                }
                );

            PartReconciliation = new ReportPart(
                "de-gcd_genInfo.report.id.reportElement.reportElements.STU",
                new List<string> {
                    Reconciliation
                }
                );

            PartTemporaryProfitLoss = new ReportPart(
                "de-gcd_genInfo.report.id.reportElement.reportElements.BGWG",
                new List<string> {
                    TemporaryProfitLoss
                }
                );

            PartTaxableProfitLoss = new ReportPart(
                "de-gcd_genInfo.report.id.reportElement.reportElements.SGE",
                new List<string> {
                    TaxableProfitLoss
                }
                );

            PartTaxableProfitLossGrossMethod = new ReportPart(
                "de-gcd_genInfo.report.id.reportElement.reportElements.SGEP",
                new List<string> {
                    TaxableProfitLossGrossMethod
                }
                );

            PartCapitalAccountChangeStatement = new ReportPart(
                "de-gcd_genInfo.report.id.reportElement.reportElements.KKE",
                new List<string> {
                    CapitalAccountChangeStatement
                }
                );

            PartNotes = new ReportPart(
                "de-gcd_genInfo.report.id.reportElement.reportElements.SA",
                Notes
                );

            PartSegmentReport = new ReportPart(
                "de-gcd_genInfo.report.id.reportElement.reportElements.SB",
                SegmentReport
                );

            PartAuditConfirmation = new ReportPart(
                "de-gcd_genInfo.report.id.reportElement.reportElements.BV",
                new List<string> {
                    null
                    // ToDo: not implemented in the ebk (de-gcd_genInfo.report.audit)
                }
                );

            PartSupervisoryBoardReport = new ReportPart(
                "de-gcd_genInfo.report.id.reportElement.reportElements.ARB",
                new List<string> {
                    SupervisoryBoardReport
                }
                );


            PartOtherReportElements = new ReportPart(
                "de-gcd_genInfo.report.id.reportElement.reportElements.S",
                new List<string> {
                    OtherReportElements
                }
                );

            PartAccountDetails = new ReportPart("de-gcd_genInfo.report.id.reportElement.reportElements.KS", null);

            PartSpecialBalance = new ReportPart("de-gcd_genInfo.report.id.reportElement.reportElements.SEF", null);

            PartDeterminationOfTaxableIncomeSpecialCases = new ReportPart("de-gcd_genInfo.report.id.reportElement.reportElements.SGEB", new List<string>{DeterminationOfTaxableIncomeSpecialCases});

            ReportParts = new Dictionary<string, ReportPart>() {
                {PartBalance.Path, PartBalance},
                {PartAuditConfirmation.Path, PartAuditConfirmation},
                {PartBalanceSheetAss.Path, PartBalanceSheetAss},
                {PartBalanceSheetEqLiab.Path, PartBalanceSheetEqLiab},
                {PartCapitalAccountChangeStatement.Path, PartCapitalAccountChangeStatement},
                {PartCashFlowStatement.Path, PartCashFlowStatement},
                {PartContingentLiabilities.Path, PartContingentLiabilities},
                {PartEquityChange.Path, PartEquityChange},
                {PartIncomeStatement.Path, PartIncomeStatement},
                {PartIncomeUse.Path, PartIncomeUse},
                {PartInterimManagementReport.Path, PartInterimManagementReport},
                {PartManagementReport.Path, PartManagementReport},
                {PartNotes.Path, PartNotes},
                {PartOpeningBalanceWithoutIncomeStatement.Path, PartOpeningBalanceWithoutIncomeStatement},
                {PartOtherReportElements.Path, PartOtherReportElements},
                {PartReconciliation.Path, PartReconciliation},
                {PartSegmentReport.Path, PartSegmentReport},
                {PartSupervisoryBoardReport.Path, PartSupervisoryBoardReport},
                {PartTableGross.Path, PartTableGross},
                {PartTableGrossShort.Path, PartTableGrossShort},
                {PartTableNet.Path, PartTableNet},
                {PartTaxableProfitLoss.Path, PartTaxableProfitLoss},
                {PartTaxableProfitLossGrossMethod.Path, PartTaxableProfitLossGrossMethod},
                {PartTemporaryProfitLoss.Path, PartTemporaryProfitLoss},
                {PartAccountDetails.Path, PartAccountDetails},
                {PartSpecialBalance.Path, PartSpecialBalance},
                {PartDeterminationOfTaxableIncomeSpecialCases.Path, PartDeterminationOfTaxableIncomeSpecialCases}
            };

        }

        protected Document Document { get; set; }

        public ReportPart PartDeterminationOfTaxableIncomeSpecialCases { get; private set; }

        public ReportPart PartSpecialBalance { get; private set; }

        public ReportPart PartAccountDetails { get; private set; }

        public ReportPart PartInterimManagementReport { get; private set; }

        public ReportPart PartEquityChange { get; private set; }

        public ReportPart PartAuditConfirmation { get; private set; }

        public ReportPart PartReconciliation { get; private set; }

        public ReportPart PartContingentLiabilities { get; private set; }

        public ReportPart PartIncomeStatement { get; private set; }

        public ReportPart PartBalanceSheetEqLiab { get; private set; }

        public ReportPart PartCashFlowStatement { get; private set; }

        public ReportPart PartIncomeUse { get; private set; }

        public ReportPart PartTableGross { get; private set; }

        public ReportPart PartTableGrossShort { get; private set; }

        public ReportPart PartBalanceSheetAss { get; private set; }

        public ReportPart PartTemporaryProfitLoss { get; private set; }

        public ReportPart PartTableNet { get; private set; }

        public ReportPart PartOpeningBalanceWithoutIncomeStatement { get; private set; }

        public ReportPart PartTaxableProfitLoss { get; private set; }

        public ReportPart PartCapitalAccountChangeStatement { get; private set; }

        public ReportPart PartTaxableProfitLossGrossMethod { get; private set; }

        public ReportPart PartNotes { get; private set; }

        public ReportPart PartSegmentReport { get; private set; }

        public ReportPart PartSupervisoryBoardReport { get; private set; }

        public ReportPart PartManagementReport { get; private set; }

        public ReportPart PartBalance { get; private set; }

        public ReportPart PartOtherReportElements { get; private set; }

        ITaxonomyInfo MainTaxonomyInfo { get { return Document.MainTaxonomyInfo; } }

        //ValueTree.ValueTree GcdTree { get { return Manager.DocumentManager.Instance.CurrentDocument.ValueTreeGcd; } }

        #region TaxonomyElementDefinitions
        
        #region BalanceSheetPath
        /// <summary>
        /// The Path to the Total assets position in the balance ("Aktiva")
        /// </summary>
        public string BalanceSheetAss {
            get {
                switch (MainTaxonomyInfo.Type) {
                    case TaxonomyType.Financial:
                        return "de-fi_bsBanks.ass";
                    case TaxonomyType.Insurance:
                        return "de-ins_bsIns.ass";
                    default:
                        return "de-gaap-ci_bs.ass";
                }
            }
        }
        /// <summary>
        /// The Path to the Total equity and liabilities position in the balance ("Passiva")
        /// </summary>
        public string BalanceSheetEqLiab {
            get {
                switch (MainTaxonomyInfo.Type) {
                    case TaxonomyType.Financial:
                        return "de-fi_bsBanks.eqLiab";
                    case TaxonomyType.Insurance:
                        return "de-ins_bsIns.eqLiab";
                    default:
                        return "de-gaap-ci_bs.eqLiab";
                }
            }
        }
        /// <summary>
        /// The Path to balance
        /// </summary>
        public string BalanceSheet {
            get {
                switch (MainTaxonomyInfo.Type) {
                    case TaxonomyType.Financial:
                        return "de-fi_bsBanks";
                    case TaxonomyType.Insurance:
                        return "de-ins_bsIns";
                    default:
                        return "de-gaap-ci_bs";
                }
            }
        }
        #endregion


        public string IncomeStatementSumPosition {
            get {
                switch (MainTaxonomyInfo.Type) {
                    case TaxonomyType.Financial:
                        return "de-fi_isBanks.gainLoss";
                    case TaxonomyType.Insurance:
                        return "de-ins_isIns.gainLoss";
                    default:
                        return "de-gaap-ci_is.netIncome";
                }
            }
        }
        /// <summary>
        /// GuV
        /// </summary>
        public string IncomeStatement {
            get {
                switch (MainTaxonomyInfo.Type) {
                    case TaxonomyType.Financial:
                        return "de-fi_isBanks";
                    case TaxonomyType.Insurance:
                        return "de-ins_isIns";
                    default:
                        return "de-gaap-ci_is";
                }
            }
        }

        /// <summary>
        /// Lagebericht
        /// </summary>
        public string ManagementReport {
            get {
                switch (MainTaxonomyInfo.Type) {
                    case TaxonomyType.Insurance:
                        return "de-ins_mgmtRep";
                    case TaxonomyType.Financial:
                    default:
                        return "de-gaap-ci_mgmtRep";
                }
            }
        }

        /// <summary>
        /// Anhang
        /// </summary>
        public List<string> Notes {
            get {
                switch (MainTaxonomyInfo.Type) {
                    case TaxonomyType.Financial:
                        return new List<string> {
                            "de-gaap-ci_nt",
                            "de-gaap-ci_bs.ass.fixAss.fin.loansToParticip",
                            "de-gaap-ci_bs.ass.fixAss.fin.particip",
                            "de-gaap-ci_bs.ass.fixAss.fin.loansToAffil",
                            "de-gaap-ci_bs.ass.fixAss.fin.sharesInAffil",
                            "de-gaap-ci_bs.ass.fixAss.tan.otherEquipm",
                            "de-gaap-ci_bs.ass.fixAss.tan.machinery",
                            "de-gaap-ci_bs.ass.fixAss.tan",
                            "de-gaap-ci_bs.ass.fixAss.intan.concessionBrands",
                            "de-gaap-ci_cube_.nt.ass.net"
                        };
                        //de-fi_ntBanks // Anhangsangaben
                    case TaxonomyType.Insurance:
                        return new List<string>{"de-ins_nt"};
                    default:
                        return new List<string>{"de-gaap-ci_nt"};
                }
            }
        }

        /// <summary>
        /// Eigenkapitalspiegel
        /// </summary>
        public string EquityChangeStatement {
            get {
                switch (MainTaxonomyInfo.Type) {
                    case TaxonomyType.Financial:
                    case TaxonomyType.Insurance:
                    default:
                        return "de-gaap-ci_eqCh";
                }
            }
        }

        public string ContingentLiabilities {
            get {
                switch (MainTaxonomyInfo.Type) {
                    case TaxonomyType.Financial:
                        return string.Empty;
                    case TaxonomyType.Insurance:
                        return string.Empty;
                    default:
                        return "de-gaap-ci_bs.contingLiab";
                }
            }
        }

        public string IncomeUse {
            get {
                switch (MainTaxonomyInfo.Type) {
                    case TaxonomyType.Financial:
                        return string.Empty;
                    case TaxonomyType.Insurance:
                        return string.Empty;
                    default:
                        return "de-gaap-ci_incomeUse";
                }
            }
        }
        /// <summary>
        /// KKE
        /// </summary>
        public string CapitalAccountChangeStatement {
            get {
                switch (MainTaxonomyInfo.Type) {
                    case TaxonomyType.Financial:
                    case TaxonomyType.Insurance:
                    default:
                        return "de-gaap-ci_kke";
                }
            }
        }
        /// <summary>
        /// Kapitalflussrechnung
        /// </summary>
        public string CashFlowStatement {
            get {
                switch (MainTaxonomyInfo.Type) {
                    case TaxonomyType.Financial:
                    case TaxonomyType.Insurance:
                    default:
                        return "de-gaap-ci_cfs";
                }
            }
        }
        /// <summary>
        /// Andere Berichtsbestandteile
        /// </summary>
        public string OtherReportElements {
            get {
                switch (MainTaxonomyInfo.Type) {
                    case TaxonomyType.Financial:
                    case TaxonomyType.Insurance:
                    default:
                        return "de-gaap-ci_OtherReportElements";
                }
            }
        }
        /// <summary>
        /// Steuerliche Gewinnermittlung für besondere Fälle
        /// </summary>
        public string DeterminationOfTaxableIncomeSpecialCases {
            get {
                switch (MainTaxonomyInfo.Type) {
                    case TaxonomyType.Financial:
                    case TaxonomyType.Insurance:
                    default:
                        return "de-gaap-ci_DeterminationOfTaxableIncomeSpec";
                }
            }
        }
        /// <summary>
        /// Steuerlicher Gewinn / Verlust nach Bruttomethode (steuerliche Gewinnermittlung bei Personengesellschaften)
        /// </summary>
        public string TaxableProfitLossGrossMethod {
            get {
                switch (MainTaxonomyInfo.Type) {
                    case TaxonomyType.Financial:
                    case TaxonomyType.Insurance:
                    default:
                        return "de-gaap-ci_fplgm.netmethod";
                }
            }
        }
        /// <summary>
        /// Steuerliche Gewinnermittlung
        /// </summary>
        public string TaxableProfitLoss {
            get {
                switch (MainTaxonomyInfo.Type) {
                    case TaxonomyType.Financial:
                    case TaxonomyType.Insurance:
                    default:
                        return "de-gaap-ci_fpl";
                }
            }
        }
        /// <summary>
        /// Übergangsgewinn / Übergangsverlust
        /// </summary>
        public string TemporaryProfitLoss {
            get {
                switch (MainTaxonomyInfo.Type) {
                    case TaxonomyType.Financial:
                        return string.Empty;
                    case TaxonomyType.Insurance:
                        return string.Empty;
                    default:
                        return "de-gaap-ci_tpl";
                }
            }
        }
        /// <summary>
        /// Segmentbericht
        /// </summary>
        public List<string> SegmentReport {
            get {
                switch (MainTaxonomyInfo.Type) {
                    case TaxonomyType.Insurance:
                        return null;
                    case TaxonomyType.Financial:
                    default:
                        return new List<string> { "de-gaap-ci_nt.segmBusiness", "de-gaap-ci_nt.segmGeographical", "de-gaap-ci_nt.segmText" };
                }
            }
        }

        /// <summary>
        /// Bericht des Aufsichtsrats
        /// </summary>
        public string SupervisoryBoardReport {
            get {
                switch (MainTaxonomyInfo.Type) {
                    case TaxonomyType.Financial:
                    case TaxonomyType.Insurance:
                    default:
                        return "de-gaap-ci_OtherReportElements.ReportSupervisoryboard";
                }
            }
        }
        /// <summary>
        /// Anlagespiegel / Anlagenspiegel netto
        /// </summary>
        public string TableNet {
            get {
                switch (MainTaxonomyInfo.Type) {
                    case TaxonomyType.Financial:
                        return "de-gaap-ci_cube_.nt.ass.net";
                    case TaxonomyType.Insurance:
                        return string.Empty;
                    default:
                        return "de-gaap-ci_table.nt.ass.net";
                }
            }
        }
        /// <summary>
        /// Anlagespiegel / Anlagenspiegel brutto
        /// </summary>
        public string TableGross {
            get {
                switch (MainTaxonomyInfo.Type) {
                    case TaxonomyType.Insurance:
                        return string.Empty;
                    case TaxonomyType.Financial:
                    default:
                        return "de-gaap-ci_table.nt.ass.gross";
                }
            }
        }
        /// <summary>
        /// Anlagespiegel / Anlagenspiegel brutto, Kurzform
        /// </summary>
        public string TableGrossShort {
            get {
                switch (MainTaxonomyInfo.Type) {
                    case TaxonomyType.Financial:
                        return string.Empty;
                    case TaxonomyType.Insurance:
                        return string.Empty;
                    default:
                        return "de-gaap-ci_table.nt.ass.gross_short";
                }
            }
        }
        /// <summary>
        /// Anlagespiegel / Anlagenspiegel brutto, Kurzform
        /// </summary>
        public string Reconciliation {
            get {
                switch (MainTaxonomyInfo.Type) {
                    case TaxonomyType.Financial:
                    case TaxonomyType.Insurance:
                    default:
                        return "de-gaap-ci_hbst";
                }
            }
        }

        #endregion TaxonomyElementDefinitions

        //public List<string> ReportPartsToSend {
        //    get {
        //        List<string> result = new List<string>();

        //        if (GcdTree.Root.Values.ContainsKey("de-gcd_genInfo.report.id.reportElement")) {
        //            var reportElements =
        //                GcdTree.Root.Values["de-gcd_genInfo.report.id.reportElement"] as
        //                XbrlElementValue.XbrlElementValue_MultipleChoice;
        //            foreach (var elem in reportElements.Elements) {
        //                if (reportElements.IsChecked[elem.Id].BoolValue.HasValue && reportElements.IsChecked[elem.Id].BoolValue.Value) {
        //                    result.Add(elem.Id);
        //                }
        //            }
        //        }
        //        return result;
        //    }
        //}

        //public string PartReconciliation { get { return "de-gcd_genInfo.report.id.reportElement.reportElements.STU"; } }
        //public string PartBalance { get { return "de-gcd_genInfo.report.id.reportElement.reportElements.B"; } }
        //public string PartAccountDetails { get { return "de-gcd_genInfo.report.id.reportElement.reportElements.KS"; } }

        ///// <summary>
        ///// Dictionary with report parts (de-gcd_genInfo.report.id.reportElement) as Key
        ///// and a list of node ids as Value.
        ///// </summary>
        //public Dictionary<string, List<string>> ReportParts {
        //    get {
        //        //throw new NotImplementedException();
        //        return new Dictionary<string, List<string>> {
        //            {
        //                // Lagebericht
        //                "de-gcd_genInfo.report.id.reportElement.reportElements.L",
        //                new List<string> {
        //                    ManagementReport
        //                }
        //            },
        //            {
        //                // Bilanz
        //                "de-gcd_genInfo.report.id.reportElement.reportElements.B", 
        //                new List<string> {
        //                    BalanceSheet
        //                }
        //            },
        //            { // Eröffnungsbilanz ohne G+V
        //                "de-gcd_genInfo.report.id.reportElement.reportElements.EB", 
        //                new List<string> {
        //                    null // ToDo: check, I am not sure about this
        //                }
        //            },
        //            {
        //                // Zwischenlagebericht
        //                "de-gcd_genInfo.report.id.reportElement.reportElements.ZL", 
        //                new List<string> {
        //                    null // ToDo: find out what it is and where it is
        //                }
        //            },
        //            {
        //                // Bilanz Aktiva
        //                "e-gcd_genInfo.report.id.reportElement.reportElements.BA", 
        //                new List<string> {
        //                    null //BalanceSheetAss
        //                }
        //            },
        //            {
        //                // Bilanz Passiva
        //                "de-gcd_genInfo.report.id.reportElement.reportElements.BP", 
        //                new List<string> {
        //                    null //BalanceSheetEqLiab
        //                }
        //            },
        //            {
        //                // GuV
        //                "de-gcd_genInfo.report.id.reportElement.reportElements.GuV", 
        //                new List<string> {
        //                    IncomeStatement
        //                }
        //            },
        //            {
        //                // Haftungsverhältnisse
        //                "de-gcd_genInfo.report.id.reportElement.reportElements.H", 
        //                new List<string> {
        //                    ContingentLiabilities
        //                }
        //            },
        //            {
        //                // Brutto-Anlagenspiegel mit Entwicklung der Abschreibungen
        //                "de-gcd_genInfo.report.id.reportElement.reportElements.BAL", 
        //                new List<string> {
        //                    TableGross
        //                }
        //            },
        //            {
        //                // Brutto-Anlagenspiegel ohne Entwicklung der Abschreibungen
        //                "de-gcd_genInfo.report.id.reportElement.reportElements.BAK", 
        //                new List<string> {
        //                    TableGrossShort
        //                }
        //            },
        //            {
        //                // Netto Anlagenspiegel
        //                "de-gcd_genInfo.report.id.reportElement.reportElements.NA", 
        //                new List<string> {
        //                    TableNet
        //                }
        //            },
        //            {
        //                // Ergebnisverwendung
        //                "de-gcd_genInfo.report.id.reportElement.reportElements.EV", 
        //                new List<string> {
        //                    IncomeUse
        //                }
        //            },
        //            {
        //                // Cash-Flow Statement
        //                "de-gcd_genInfo.report.id.reportElement.reportElements.CFS", 
        //                new List<string> {
        //                    CashFlowStatement
        //                }
        //            },
        //            {
        //                // Eigenkapitalentwicklung
        //                "de-gcd_genInfo.report.id.reportElement.reportElements.EKE", 
        //                new List<string> {
        //                    null // ToDo: find out what it is and where it is
        //                }
        //            },
        //            {
        //                // steuerliche Überleitungsrechnung
        //                PartReconciliation, 
        //                new List<string> {
        //                    Reconciliation
        //                }
        //            },
        //            {
        //                // Berichtigung des Gewinns bei Wechsel der Gewinnermittlungsart
        //                "de-gcd_genInfo.report.id.reportElement.reportElements.BGWG", 
        //                new List<string> {
        //                    TemporaryProfitLoss
        //                }
        //            },
        //            {
        //                // steuerliche Gewinnermittlung
        //                "de-gcd_genInfo.report.id.reportElement.reportElements.SGE", 
        //                new List<string> {
        //                    TaxableProfitLoss
        //                }
        //            },
        //            {
        //                // steuerliche Gewinnermittlung bei Personengesellschaften
        //                "de-gcd_genInfo.report.id.reportElement.reportElements.SGEP", 
        //                new List<string> {
        //                    TaxableProfitLossGrossMethod
        //                }
        //            },
        //            {
        //                // Kapitalkontenentwicklung
        //                "de-gcd_genInfo.report.id.reportElement.reportElements.KKE", 
        //                new List<string> {
        //                    CapitalAccountChangeStatement
        //                }
        //            },
        //            {
        //                // andere Anhangangaben
        //                "de-gcd_genInfo.report.id.reportElement.reportElements.SA", 
        //                    Notes
        //            },
        //            {
        //                // Segmentbericht
        //                "de-gcd_genInfo.report.id.reportElement.reportElements.SB", 
        //                    SegmentReport
        //            },
        //            {
        //                // Bestätigungsvermerk
        //                "de-gcd_genInfo.report.id.reportElement.reportElements.BV", 
        //                new List<string> {
        //                    null
        //                    // ToDo: not implemented in the ebk (de-gcd_genInfo.report.audit)
        //                }
        //            },
        //            {
        //                // Bericht des Aufsichtsrats
        //                "de-gcd_genInfo.report.id.reportElement.reportElements.ARB", 
        //                new List<string> {
        //                    SupervisoryBoardReport
        //                }
        //            },
        //            {
        //                // Andere Berichtsbestandteile
        //                "de-gcd_genInfo.report.id.reportElement.reportElements.S", 
        //                new List<string> {
        //                    OtherReportElements
        //                }
        //            }
        //        };
        //    }
        //}




        public List<ReportPart> ReportPartsToSend { get { return ReportParts.Values.Where(part => part.IsSelected == true).ToList(); } }

        public Dictionary<string, ReportPart> ReportParts { get; private set; }

        public class ReportPart : Utils.NotifyPropertyChangedBase {

            private ReportPart() {
                _isVisible = true;
            }

            public ReportPart(string name, string path, List<string> taxonomyElements)
                : this(path, taxonomyElements) { Name = name; }

            public ReportPart(string path, List<string> taxonomyElements)
                : this() {
                Path = path;
                TaxonomyElements = taxonomyElements;
            }

            public bool? IsSelected { get { return ReportPartSelection.IsChecked[Path].BoolValue; } private set { ReportPartSelection.IsChecked[Path].BoolValue = value; } }

            private bool _isVisible;
            public bool IsVisible { get { return _isVisible; } set { _isVisible = value; OnPropertyChanged("IsVisible");
                if (!value && Element.IsMandatoryField) {
                    IsSelected = false;
                }
            } }
            
            public List<string> TaxonomyElements { get; private set; }
            public string Path { get; private set; }
            public string Name { get; set; }

            public Taxonomy.IElement Element {
                get {
                    return Document.GcdTaxonomy.Elements.ContainsKey(Path) ? Document.GcdTaxonomy.Elements[Path] : null;
                    // GcdTree.GetValue(Path) != null ? GcdTree.GetValue(Path).Element 
                }
            }

            private XbrlElementValue.XbrlElementValue_MultipleChoice ReportPartSelection {
                get {
                    if (GcdTree.Root.Values.ContainsKey("de-gcd_genInfo.report.id.reportElement")) {
                        return GcdTree.Root.Values["de-gcd_genInfo.report.id.reportElement"] as
                            XbrlElementValue.XbrlElementValue_MultipleChoice;
                    }
                    return null;
                }
            }
            private ValueTree.ValueTree GcdTree { get { return Document.ValueTreeGcd; } }
            private DbMapping.Document Document { get { return Manager.DocumentManager.Instance.CurrentDocument; } }



            public override string ToString() {
                return Path;
            }

        }
    }
}