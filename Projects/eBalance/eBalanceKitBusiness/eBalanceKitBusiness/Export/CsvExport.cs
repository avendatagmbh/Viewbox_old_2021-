using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Taxonomy.Enums;
using Taxonomy.Interfaces;
using Taxonomy.Interfaces.PresentationTree;
using eBalanceKitBase.Structures;
using eBalanceKitBusiness.Structures.DbMapping.BalanceList;
using eBalanceKitBusiness.Structures.ValueTree;
using eBalanceKitBusiness.Structures.XbrlElementValue;

namespace eBalanceKitBusiness.Export {
    ///<summary>
    ///This module exports the selected object tree to CSV file format.
    ///</summary>
    ///<author>Solueman Hussain</author>
    ///<Date> 24.10.2011 </Date>
    public class CsvExport {
        private StringBuilder _csvContent;
        private readonly string[] _disallowedChars = {"\\", "#", "\"", "/", ";", "!", "?", "%", "^", "`", "=", "~", "<", ">", "|", "," };
        private bool _accountBalance = false;
        private string _filename;

        #region Export

        public void Export(IExportConfig config) {
            Config = config;
            
            foreach (var disallowedChar in _disallowedChars) {
                if(!disallowedChar.Equals("\\"))
                Config.FilePath = Config.FilePath.Replace(disallowedChar, "");    
            }


            var directory = new DirectoryInfo(Config.FilePath);
            if (!directory.Exists) directory.Create();

            try {
                //ExportPart("changesEquityStatement");
                //ExportPart("notes");
                if (Config.ExportCompanyInformation) ExportCompanyInformation();
                if (Config.ExportDocumentInformation) ExportDocumentInformation();
                if (Config.ExportReportInformation) ExportReportInformation();

                if (Config.ExportBalanceSheet && Config.ShowBalanceSheet) ExportBalanceSheets();
                if (Config.ExportIncomeStatement && Config.ShowIncomeStatement) ExportIncomeStatements();
                if (Config.ExportAccountBalances) ExportAccountBalancesWithParameter();

                if (Config.ExportAppropriationProfit && Config.ShowAppropriationProfit) ExportPart("appropriationProfits");
                if (Config.ExportContingentLiabilities && Config.ShowContingentLiabilities) ExportPart("contingentLiabilities");
                if (Config.ExportCashFlowStatement && Config.ShowCashFlowStatement) ExportPart("cashFlowStatement");
                if (Config.ExportAdjustmentOfIncome && Config.ShowAdjustmentOfIncome) ExportPart("adjustmentOfIncome");
                if (Config.ExportDeterminationOfTaxableIncome && Config.ShowDeterminationOfTaxableIncome) ExportPart("determinationOfTaxableIncome");
                if (Config.ExportDeterminationOfTaxableIncomeBusinessPartnership && Config.ShowDeterminationOfTaxableIncomeBusinessPartnership) ExportPart("determinationOfTaxableIncomeBusinessPartnership");
                if (Config.ExportDeterminationOfTaxableIncomeSpecialCases && Config.ShowDeterminationOfTaxableIncomeSpecialCases) ExportPart("determinationOfTaxableIncomeSpecialCases");

                if (Config.ExportNtAssGross && Config.ShowNtAssGross) new HyperCubeExporter(Config).ExportCsv("de-gaap-ci_table.nt.ass.gross");
                if (Config.ExportNtAssGrossShort && Config.ShowNtAssGrossShort) new HyperCubeExporter(Config).ExportCsv("de-gaap-ci_table.nt.ass.gross_short");
                if (Config.ExportNtAssNet && Config.ShowNtAssNet) new HyperCubeExporter(Config).ExportCsv("de-gaap-ci_table.nt.ass.net");
                if (Config.ExportEquityStatement && Config.ShowEquityStatement) new HyperCubeExporter(Config).ExportCsv("de-gaap-ci_table.eqCh");
                
                if (Config.ExportManagementReport && Config.ShowManagementReport) ExportManagementReport();
            } catch (Exception ex) {
                throw new Exception(ex.Message, ex);
            }
        }

        #endregion Export

        #region ExportAccountBalancesWithParameter
        private void ExportAccountBalancesWithParameter() {
            switch (Config.Document.MainTaxonomyInfo.Type) {
                case Taxonomy.Enums.TaxonomyType.GAAP:
                    ExportAccountBalances("http://www.xbrl.de/taxonomies/de-gaap-ci/role/balanceSheet", "http://www.xbrl.de/taxonomies/de-gaap-ci/role/incomeStatement");
                    break;
                case Taxonomy.Enums.TaxonomyType.OtherBusinessClass:
                    ExportAccountBalances("http://www.xbrl.de/taxonomies/de-gaap-ci/role/balanceSheet", "http://www.xbrl.de/taxonomies/de-gaap-ci/role/incomeStatement");
                    break;
                case Taxonomy.Enums.TaxonomyType.Financial:
                    ExportAccountBalances("http://www.xbrl.de/taxonomies/de-fi/role/balanceSheet", "http://www.xbrl.de/taxonomies/de-fi/role/incomeStatementStf");
                    break;
                case Taxonomy.Enums.TaxonomyType.Insurance:
                    ExportAccountBalances("http://www.xbrl.de/taxonomies/de-ins/role/balanceSheet", "http://www.xbrl.de/taxonomies/de-ins/role/incomeStatement");
                    break;
                default:
                    break;
            }
        }
        #endregion

        #region ExportAccountBalancesWithParameter
        private void ExportAccountBalances(string _uriBalanaceSheet, string _uriIncomeStatement) {
            XbrlElementValue_List accountBalanceList;
            Dictionary<string, PdfTreeView> nodeDict;
            PdfTreeView tvBS;
            PdfTreeView tvIS;
            Taxonomy.Interfaces.PresentationTree.IPresentationTreeNode rootBS;

            Taxonomy.Interfaces.PresentationTree.IPresentationTreeNode rootIS;
            try {
                _accountBalance = true;
                _csvContent = new StringBuilder();
                IRoleType roleB = Config.Document.MainTaxonomy.GetRole(_uriBalanaceSheet);
                IRoleType roleI = Config.Document.MainTaxonomy.GetRole(_uriIncomeStatement);
                _filename = Config.Document.Company.Name +
                                                 ", Geschäftsjahr " + Config.Document.FinancialYear.FYear + "_" + "Kontensalden_"+
                                                 roleB.Name +"_"+ roleI.Name;


                foreach (var disallowedChar in _disallowedChars) {
                    _filename = _filename.Replace(disallowedChar, "_");
                }
                _filename = Config.FilePath + "\\" + _filename + ".csv";

                using (
                    var csvWrite = new StreamWriter(File.Open(_filename, FileMode.Create),
                                                    Encoding.UTF8)) {
                    _csvContent.Append(Config.Document.Company.Name + ", Geschäftsjahr " +
                                       Config.Document.FinancialYear.FYear);

                    _csvContent.AppendLine();
                    _csvContent.AppendLine();
                    accountBalanceList =
                        Config.Document.ValueTreeMain.Root.Values["detailedInformation.accountBalances"] as
                        XbrlElementValue_List;
                    _csvContent.Append(accountBalanceList);
                    _csvContent.AppendLine();
                    _csvContent.AppendLine();
                    _csvContent.Append("Bilanz;Betrag");

                    _csvContent.AppendLine();

                    nodeDict = new Dictionary<string, PdfTreeView>();
                    tvBS = new PdfTreeView();
                    tvIS = new PdfTreeView();
                    rootBS = Config.Document.GaapPresentationTrees[_uriBalanaceSheet].RootEntries.First();
                    BuildTreeViewAccountBalances(nodeDict, tvBS, rootBS);

                    rootIS = Config.Document.GaapPresentationTrees[_uriIncomeStatement].RootEntries.First();
                    BuildTreeViewAccountBalances(nodeDict, tvIS, rootIS);

                    foreach (var balanceList in Config.Document.BalanceLists) {
                        //BalanceList balanceList = Config.Document.BalanceList;
                        foreach (var entry in balanceList.AssignedItems) {
                            if (entry is IAccountGroup) {
                                if (!entry.SendBalance) continue;
                                var group = entry as IAccountGroup;
                                foreach (var account in group.Items) ExportAccountBalance(nodeDict, account);
                            } else {
                                if (!entry.SendBalance) continue;
                                ExportAccountBalance(nodeDict, entry);
                            }
                        }
                    }

                    ExportTreeView(tvBS, false, true, false);
                    _csvContent.AppendLine();
                    _csvContent.Append("Gewinn-  und Verlustrechnung");
                    _csvContent.AppendLine();
                    ExportTreeView(tvIS, false, true, true);

                    csvWrite.WriteLine(_csvContent);
                    csvWrite.Close();
                    _accountBalance = false;
                }
            } catch (Exception ex) {
                throw new Exception(
                    "Bei der Erstellung des CSV-Reports ist ein Fehler aufgetreten: " + Environment.NewLine + ex.Message,
                    ex);
            }
        }

        private static void ExportAccountBalance(Dictionary<string, PdfTreeView> nodeDict, IBalanceListEntry entry) {
            var elemName = entry.AssignedElement.Id;
            var accNumber = entry.Number;
            var accName = entry.Name;
            var balance = entry.Amount;

            if (nodeDict.ContainsKey(elemName)) {
                var node = nodeDict[elemName];
                var child = node.AddChild(null, accNumber + " - " + accName, 0, true);
                child.Value = LocalisationUtils.CurrencyToString(balance);
                child.HasValue = true;
            } else {
                
            }
        }
        #endregion

        #region BuildTreeViewAccountBalances
        private void BuildTreeViewAccountBalances(Dictionary<string, PdfTreeView> nodeDict, PdfTreeView tv, Taxonomy.Interfaces.PresentationTree.IPresentationTreeNode root) {
            try {
                int pos = 1;
                foreach (IPresentationTreeEntry child in root.Children) {
                    PdfTreeView tvChild = null;

                    if (child is eBalanceKitBusiness.Structures.DbMapping.BalanceList.BalanceListEntryBase) {
                    } else {
                        Taxonomy.Interfaces.PresentationTree.IPresentationTreeNode pchild = child as Taxonomy.Interfaces.PresentationTree.IPresentationTreeNode;

                        tvChild = tv.AddChild(pchild.Element, pchild.Element.MandatoryLabel, pos, false);
                        tvChild.Value = string.Empty;
                        tvChild.HasValue = false;
                        nodeDict[pchild.Element.Id] = tvChild;

                        if (tvChild != null)
                            BuildTreeViewAccountBalances(nodeDict, tvChild, child as Taxonomy.Interfaces.PresentationTree.IPresentationTreeNode);
                    }

                    pos++;
                }
            } catch (Exception ex) {
                throw new Exception(ex.Message, ex);
            }
        }
        #endregion BuildTreeViewAccountBalances


        #region ExportIncomeStatements
        private void ExportIncomeStatements() {
            switch (Config.Document.MainTaxonomyInfo.Type) {
                case TaxonomyType.GAAP:
                    ExportIncomeStatement("http://www.xbrl.de/taxonomies/de-gaap-ci/role/incomeStatement");
                    break;
                case TaxonomyType.OtherBusinessClass:
                    ExportIncomeStatement("http://www.xbrl.de/taxonomies/de-gaap-ci/role/incomeStatement");
                    break;
                case TaxonomyType.Financial:
                    ExportIncomeStatement("http://www.xbrl.de/taxonomies/de-fi/role/incomeStatementStf");
                    break;
                case TaxonomyType.Insurance:
                    ExportIncomeStatement("http://www.xbrl.de/taxonomies/de-ins/role/incomeStatement");
                    break;
                default:
                    break;
            }
        }
        #endregion ExportIncomeStatements

        #region ExportBalanceSheets
        private void ExportBalanceSheets() {
            switch (Config.Document.MainTaxonomyInfo.Type) {
                case TaxonomyType.GAAP:
                    ExportBalanceSheet("http://www.xbrl.de/taxonomies/de-gaap-ci/role/balanceSheet");
                    break;
                case TaxonomyType.OtherBusinessClass:
                    ExportBalanceSheet("http://www.xbrl.de/taxonomies/de-gaap-ci/role/balanceSheet");
                    break;
                case TaxonomyType.Financial:
                    ExportBalanceSheet("http://www.xbrl.de/taxonomies/de-fi/role/balanceSheet");
                    break;
                case TaxonomyType.Insurance:
                    ExportBalanceSheet("http://www.xbrl.de/taxonomies/de-ins/role/balanceSheet");
                    break;
                default:
                    break;
            }
        }
        #endregion ExportBalanceSheets

        #region ExportIncomeStatement
        private void ExportIncomeStatement(string _uri) {
            try {
                _csvContent = new StringBuilder();
                var role = Config.Document.MainTaxonomy.GetRole(_uri);

                _filename = Config.Document.Company.Name +
                                                 ", Geschäftsjahr " + Config.Document.FinancialYear.FYear + "_" +
                                                 role.Name;
               
                
                foreach (var disallowedChar in _disallowedChars) {
                    _filename = _filename.Replace(disallowedChar, "_");
                }
                _filename = Config.FilePath + "\\" + _filename + ".csv";

                using (
                    var csvWrite = new StreamWriter(File.Open(_filename, FileMode.Create),
                                                    Encoding.UTF8)) {
                    _csvContent.Append(Config.Document.Company.Name + ", Geschäftsjahr " +
                                       Config.Document.FinancialYear.FYear);
                    _csvContent.AppendLine();
                    if (Config.ExportReconciliationInfo) _csvContent.Append(";Überleitung Aktuell;Vorperioden;Handelsbilanzwert;Steuerbilanzwert");
                    else _csvContent.Append(";Handelsbilanzwert");

                    if (Config.ExportComments) _csvContent.Append(";Kommentar");
                    _csvContent.AppendLine();

                    _csvContent.Append(role.Name + (Config.ExportAccounts ? " mit Kontennachweis" : ""));
                    _csvContent.AppendLine();
                    _csvContent.AppendLine();
                    _csvContent.AppendLine();

                    Taxonomy.Interfaces.PresentationTree.IPresentationTreeNode root =
                        Config.Document.GaapPresentationTrees[_uri].RootEntries.First();
                    var tv = new PdfTreeView();
                    BuildTreeView(tv, Config.Document.ValueTreeMain, root, Config.ExportReconciliationInfo);
                    ExportTreeView(tv, Config.ExportReconciliationInfo, false, true);
                    csvWrite.WriteLine(_csvContent);
                    csvWrite.Close();
                }
            } catch (Exception ex) {
                throw new Exception(
                    "Bei der Erstellung des CSV-Reports ist ein Fehler aufgetreten: " + Environment.NewLine + ex.Message,
                    ex);
            }
        }
        #endregion ExportIncomeStatement
        #region ExportManagementReport
        private void ExportManagementReport() {
            try {
                _csvContent = new StringBuilder();
                
                _filename = Config.Document.Company.Name +
                                                 ", Geschäftsjahr " + Config.Document.FinancialYear.FYear + "_" +
                                                 "Lagebericht";
                foreach (var disallowedChar in _disallowedChars) {
                    _filename = _filename.Replace(disallowedChar, "_");
                }
                _filename = Config.FilePath + "\\" + _filename + ".csv";

                using (
                    var csvWrite = new StreamWriter(File.Open(_filename, FileMode.Create),
                                                    Encoding.UTF8)) {
                    _csvContent.Append(Config.Document.Company.Name + ", Geschäftsjahr " +
                                       Config.Document.FinancialYear.FYear);
                    _csvContent.AppendLine();

                    if (Config.ExportComments) _csvContent.Append(";;Kommentar");
                    _csvContent.AppendLine();
                    _csvContent.AppendLine();

                    IPresentationTreeNode root =
                        Config.Document.GaapPresentationTrees[ExportHelper.GetUri("managementReport", Config.Document)].RootEntries.First();
                    var tv = new PdfTreeView();
                    BuildTreeView(tv, Config.Document.ValueTreeMain, root, Config.ExportReconciliationInfo);
                    ExportTreeViewManagementReport(tv, Config.ExportReconciliationInfo, false, true);
                    csvWrite.WriteLine(_csvContent);
                    csvWrite.Close();
                }
            } catch (Exception ex) {
                throw new Exception(
                    "Bei der Erstellung des CSV-Reports ist ein Fehler aufgetreten: " + Environment.NewLine + ex.Message,
                    ex);
            }
        }
        #endregion ExportManagementReport


        #region ExportPart
        private void ExportPart(string uri) {
            try {
                _csvContent = new StringBuilder();
                IRoleType role = Config.Document.MainTaxonomy.GetRole(ExportHelper.GetUri(uri, Config.Document));

                _filename = Config.Document.Company.Name +
                                                 ", Geschäftsjahr " + Config.Document.FinancialYear.FYear + "_" +
                                                 role.Name;
                foreach (var disallowedChar in _disallowedChars) {
                    _filename = _filename.Replace(disallowedChar, "_");
                }
                _filename = Config.FilePath + "\\" + _filename + ".csv";

                using (
                    var csvWrite = new StreamWriter(File.Open(_filename, FileMode.Create),
                                                    Encoding.UTF8)) {
                    _csvContent.Append(Config.Document.Company.Name + ", Geschäftsjahr " +
                                       Config.Document.FinancialYear.FYear);
                    _csvContent.AppendLine();
                    _csvContent.Append(Config.ExportReconciliationInfo
                                           ? ";Überleitung Aktuell;Vorperioden;Handelsbilanzwert;Steuerbilanzwert"
                                           : ";Handelsbilanzwert");

                    if (Config.ExportComments) _csvContent.Append(";Kommentar");
                    _csvContent.AppendLine();

                    _csvContent.Append(role.Name + (Config.ExportAccounts ? " mit Kontennachweis" : ""));
                    _csvContent.AppendLine();
                    _csvContent.AppendLine();
                    _csvContent.AppendLine();

                    Taxonomy.Interfaces.PresentationTree.IPresentationTreeNode root =
                        Config.Document.GaapPresentationTrees[ExportHelper.GetUri(uri, Config.Document)].RootEntries.First();
                    var tv = new PdfTreeView();
                    BuildTreeView(tv, Config.Document.ValueTreeMain, root, Config.ExportReconciliationInfo);
                    ExportTreeView(tv, Config.ExportReconciliationInfo, false, true);
                    csvWrite.WriteLine(_csvContent);
                    csvWrite.Close();
                }
            } catch (Exception ex) {
                throw new Exception(
                    "Bei der Erstellung des CSV-Reports ist ein Fehler aufgetreten: " + Environment.NewLine + ex.Message,
                    ex);
            }
        }
        #endregion ExportPart

        #region ExportBalanceSheet
        private void ExportBalanceSheet(string _uri) {
            try {
                _csvContent = new StringBuilder();
                IRoleType role = Config.Document.MainTaxonomy.GetRole(_uri);

                _filename = Config.Document.Company.Name + ", Geschäftsjahr " +
                                              Config.Document.FinancialYear.FYear + "_" + role.Name;
                foreach (var disallowedChar in _disallowedChars) {
                    _filename = _filename.Replace(disallowedChar, "_");
                }
                _filename = Config.FilePath + "\\" + _filename + ".csv";

                using (
                    var csvWrite = new StreamWriter(File.Open(_filename, FileMode.Create),
                                                    Encoding.UTF8)) {
                    _csvContent.Append(Config.Document.Company.Name + ", Geschäftsjahr " +
                                       Config.Document.FinancialYear.FYear);
                    _csvContent.AppendLine();
                    _csvContent.Append(Config.ExportReconciliationInfo
                                           ? ";Überleitung Aktuell;Vorperioden;Handelsbilanzwert;Steuerbilanzwert"
                                           : ";Handelsbilanzwert");

                    if (Config.ExportComments) _csvContent.Append(";Kommentar");
                    _csvContent.AppendLine();

                    _csvContent.Append(role.Name + (Config.ExportAccounts ? " mit Kontennachweis" : ""));
                    _csvContent.AppendLine();
                    _csvContent.AppendLine();
                    _csvContent.AppendLine();

                    Taxonomy.Interfaces.PresentationTree.IPresentationTreeNode root =
                        Config.Document.GaapPresentationTrees[_uri].RootEntries.First();
                    var tv = new PdfTreeView();
                    BuildTreeView(tv, Config.Document.ValueTreeMain, root, Config.ExportReconciliationInfo);
                    ExportTreeView(tv, Config.ExportReconciliationInfo, false, true);
                    csvWrite.WriteLine(_csvContent);
                    csvWrite.Close();
                }
            } catch (Exception ex) {
                throw new Exception(
                    "Bei der Erstellung des CSV-Reports ist ein Fehler aufgetreten: " + Environment.NewLine + ex.Message,
                    ex);
            }
        }
        #endregion ExportBalanceSheet
       
        #region BuildTreeView
        internal static void BuildTreeView(PdfTreeView tv, ValueTree vtree, IPresentationTreeNode root, bool exportReconciliationInfo, int posInitial = 1, bool includeRoot = false) {
            try {
                int pos = posInitial;
                if (includeRoot)
                    BuildTreeViewChild(root, tv, vtree, exportReconciliationInfo, ref pos);
                foreach (IPresentationTreeNode child in root.Children.OfType<IPresentationTreeNode>()) {
                    
                    BuildTreeViewChild(child, tv, vtree, exportReconciliationInfo, ref pos);

                    //PdfTreeView tvChild = null;

                    //if (child is IBalanceListEntry) {
                    //    tvChild = tv.AddChild(null, (child as BalanceListEntryBase).Label, pos, true);
                    //    tvChild.Value = (child as BalanceListEntryBase).ValueDisplayString;
                    //    string comment = (child as BalanceListEntryBase).Comment;
                    //    tvChild.Comment = comment != null ? comment : string.Empty;
                    //} else {
                    //    if (vtree.Root.Values.ContainsKey(child.Element.Id)) {
                    //        tvChild = tv.AddChild(child.Element, child.Element.MandatoryLabel, pos, false);

                    //        IValueTreeEntry value = vtree.Root.Values[child.Element.Id];
                    //        var valueBase = value as XbrlElementValueBase;
                    //        tvChild.Comment = valueBase.Comment != null ? valueBase.Comment : string.Empty;

                    //        if ((value.IsNumeric && value.DecimalValue != 0) ||
                    //            (!value.IsNumeric && !string.IsNullOrEmpty(value.ToString()))
                    //            ||
                    //            (value.ReconciliationInfo != null && value.ReconciliationInfo.HasTransferValue &&
                    //             exportReconciliationInfo)
                    //            ) {
                    //            if ((child.Element.ValueType == XbrlElementValueTypes.String) && (value.Value != null)) {
                    //                // We need the complete string and not the limited DisplayString --sev
                    //                tvChild.Value = value.Value.ToString();
                    //            } else {

                    //                tvChild.Value = value.DisplayString;
                    //            }
                    //            tvChild.ReconciliationInfo = value.ReconciliationInfo;
                    //            tvChild.HasValue = value.HasValue ||
                    //                               (tvChild.ReconciliationInfo != null &&
                    //                                tvChild.ReconciliationInfo.HasTransferValue && exportReconciliationInfo);
                    //        } 
                    //            // create a separate csv for this HyperCube element
                    //        //else if (pchild.Element.IsHypercubeItem || pchild.Element.IsAbstract) {
                    //          //  new HyperCubeExporter(Config).ExportCsv(pchild.Element.Id);
                    //        //}

                    //    } else {
                    //        /*
                    //        // create a separate csv for this HyperCube element
                    //        // ToDo Check only if IsHypercubeItem  -- sev
                    //        if (pchild.Element.IsHypercubeItem || pchild.Element.IsAbstract) {
                    //            new HyperCubeExporter(Config).ExportCsv(pchild.Element.Id);
                    //        }
                    //         */
                    //        tvChild = tv.AddChild(child.Element, child.Element.MandatoryLabel, pos, false);
                    //        tvChild.Value = string.Empty;
                    //        tvChild.HasValue = false;
                    //    }

                    //    if (tvChild != null)
                    //        BuildTreeView(tvChild, vtree, child, exportReconciliationInfo);
                    //}

                    //pos++;
                }
            } catch (Exception ex) {
                throw new Exception(ex.Message, ex);
            }
        }

        internal static void BuildTreeViewChild(IPresentationTreeNode child, PdfTreeView tv, ValueTree vtree, bool exportReconciliationInfo, ref int pos) {
            PdfTreeView tvChild = null;

            if (child is IBalanceListEntry) {
                tvChild = tv.AddChild(null, (child as BalanceListEntryBase).Label, pos, true);
                tvChild.Value = (child as BalanceListEntryBase).ValueDisplayString;
                string comment = (child as BalanceListEntryBase).Comment;
                tvChild.Comment = comment != null ? comment : string.Empty;
            } else {
                if (vtree.Root.Values.ContainsKey(child.Element.Id)) {
                    tvChild = tv.AddChild(child.Element, child.Element.MandatoryLabel, pos, false);

                    IValueTreeEntry value = vtree.Root.Values[child.Element.Id];
                    var valueBase = value as XbrlElementValueBase;
                    tvChild.Comment = valueBase.Comment != null ? valueBase.Comment : string.Empty;

                    if ((value.IsNumeric && value.DecimalValue != 0) ||
                        (!value.IsNumeric && !string.IsNullOrEmpty(value.ToString()))
                        ||
                        (value.ReconciliationInfo != null && value.ReconciliationInfo.HasTransferValue &&
                         exportReconciliationInfo)
                        ) {
                        if ((child.Element.ValueType == XbrlElementValueTypes.String) && (value.Value != null)) {
                            // We need the complete string and not the limited DisplayString --sev
                            tvChild.Value = value.Value.ToString();
                        } else {

                            tvChild.Value = value.DisplayString;
                        }
                        tvChild.ReconciliationInfo = value.ReconciliationInfo;
                        tvChild.HasValue = value.HasValue ||
                                           (tvChild.ReconciliationInfo != null &&
                                            tvChild.ReconciliationInfo.HasTransferValue && exportReconciliationInfo);
                    }
                    // create a separate csv for this HyperCube element
                    //else if (pchild.Element.IsHypercubeItem || pchild.Element.IsAbstract) {
                    //  new HyperCubeExporter(Config).ExportCsv(pchild.Element.Id);
                    //}

                } else {
                    /*
                    // create a separate csv for this HyperCube element
                    // ToDo Check only if IsHypercubeItem  -- sev
                    if (pchild.Element.IsHypercubeItem || pchild.Element.IsAbstract) {
                        new HyperCubeExporter(Config).ExportCsv(pchild.Element.Id);
                    }
                     */
                    tvChild = tv.AddChild(child.Element, child.Element.MandatoryLabel, pos, false);
                    tvChild.Value = string.Empty;
                    tvChild.HasValue = false;
                }

                if (tvChild != null)
                    BuildTreeView(tvChild, vtree, child, exportReconciliationInfo);
            }

            pos++;
        }
        #endregion BuildTreeView

        #region ExportTreeView
        private void ExportTreeView(PdfTreeView tv, bool includeUeberleitung, bool accountBalances, bool GuV) {
            foreach (PdfTreeView child in tv.Children) {
                try {
                    if (child.Level > 0) {
                        if (child.IsAccount) {
                            if (Config.ExportAccounts || accountBalances) {
                                if (_accountBalance)
                                    _csvContent.Append(child.HeaderNumber + " " + child.Header + ";" + child.Value);
                                else if (Config.ExportReconciliationInfo) _csvContent.Append(child.HeaderNumber + " " + child.Header + ";;;;" + child.Value);
                                else _csvContent.Append(child.HeaderNumber + " " + child.Header + ";" + child.Value);
                                if (Config.ExportComments) _csvContent.Append(";" + child.Comment);
                                _csvContent.AppendLine();
                            }
                        } else if (child.Element.ValueType == XbrlElementValueTypes.Monetary) {
                            if ((Config.ExportNILValues) || child.ExportElement) {
                                if (includeUeberleitung) {
                                    string hbValue = "", transfer = "", stValue = "", transferPrevious = "";
                                    _csvContent.Append(child.HeaderNumber + " " + child.Header.Replace(";", ", "));

                                    if (child.ReconciliationInfo != null) {
                                        if (child.ReconciliationInfo.HBValue != null) hbValue = child.ReconciliationInfo.GetDisplayString(child.ReconciliationInfo.HBValue);

                                        transfer = child.ReconciliationInfo.GetDisplayString(child.ReconciliationInfo.TransferValue);

                                        transferPrevious =
                                            child.ReconciliationInfo.GetDisplayString(
                                                child.ReconciliationInfo.TransferValuePreviousYear);
                                        if (child.ReconciliationInfo.STValue != null) stValue = child.ReconciliationInfo.GetDisplayString(child.ReconciliationInfo.STValue);
                                    } else {
                                        hbValue = child.Value;
                                        stValue = child.Value;
                                    }

                                    _csvContent.Append(";" + transfer);
                                    _csvContent.Append(";" + transferPrevious);

                                    _csvContent.Append(";" + hbValue);
                                    _csvContent.Append(";" + stValue);
                                    if (Config.ExportComments) _csvContent.Append(";" + child.Comment);
                                    _csvContent.AppendLine();
                                } else {
                                    _csvContent.Append(child.HeaderNumber + " " + child.Header.Replace(";", ", "));
                                    if (child.Value != null && !child.Value.Equals(string.Empty)) {
                                        _csvContent.Append(";" + child.Value);
                                    }
                                    if (Config.ExportComments) _csvContent.Append(";" + child.Comment);

                                    _csvContent.AppendLine();
                                }
                            }
                        }
                    }
                    ExportTreeView(child, includeUeberleitung, accountBalances, GuV);
                } catch (Exception ex) {
                    throw new Exception(ex.Message, ex);
                }
            }
        }
        #endregion ExportTreeView

        #region ExportTreeViewManagementReport
        private void ExportTreeViewManagementReport(PdfTreeView tv, bool includeUeberleitung, bool accountBalances, bool GuV) {
            foreach (PdfTreeView child in tv.Children) {
                try {
                    if (child.Level > 0) {
                        if (child.IsAccount) {
                            if (Config.ExportAccounts || accountBalances) {
                                if (_accountBalance)
                                    _csvContent.Append(child.HeaderNumber + " " + child.Header + ";" + child.Value);
                                else if (Config.ExportReconciliationInfo) _csvContent.Append(child.HeaderNumber + " " + child.Header + ";;;;" + child.Value);
                                else _csvContent.Append(child.HeaderNumber + " " + child.Header + ";" + child.Value);
                                if (Config.ExportComments) _csvContent.Append(";" + child.Comment);
                                _csvContent.AppendLine();
                            }
                        } else if (child.Element.ValueType == XbrlElementValueTypes.Monetary) {
                            if ((Config.ExportNILValues) || child.ExportElement) {
                                if (includeUeberleitung) {
                                    string hbValue = "", transfer = "", stValue = "", transferPrevious = "";
                                    _csvContent.Append(child.HeaderNumber + " " + child.Header.Replace(";", ", "));

                                    if (child.ReconciliationInfo != null) {
                                        if (child.ReconciliationInfo.HBValue != null) hbValue = child.ReconciliationInfo.GetDisplayString(child.ReconciliationInfo.HBValue);

                                        transfer = child.ReconciliationInfo.GetDisplayString(child.ReconciliationInfo.TransferValue);

                                        transferPrevious =
                                            child.ReconciliationInfo.GetDisplayString(
                                                child.ReconciliationInfo.TransferValuePreviousYear);
                                        if (child.ReconciliationInfo.STValue != null) stValue = child.ReconciliationInfo.GetDisplayString(child.ReconciliationInfo.STValue);
                                    } else {
                                        hbValue = child.Value;
                                        stValue = child.Value;
                                    }

                                    _csvContent.Append(";" + transfer);
                                    _csvContent.Append(";" + transferPrevious);

                                    _csvContent.Append(";" + hbValue);
                                    _csvContent.Append(";" + stValue);
                                    if (Config.ExportComments) _csvContent.Append(";" + child.Comment);
                                    _csvContent.AppendLine();
                                } else {
                                    _csvContent.Append(child.HeaderNumber + " " + child.Header.Replace(";", ", "));
                                    if (child.Value != null && !child.Value.Equals(string.Empty)) {
                                        _csvContent.Append(";" + child.Value);
                                    }
                                    if (Config.ExportComments) _csvContent.Append(";" + child.Comment);

                                    _csvContent.AppendLine();
                                }
                            }
                        } else if (child.Element.ValueType == Taxonomy.Enums.XbrlElementValueTypes.String) {
                            if ((child.HasValue && child.Value != null) || Config.ExportNILValues) {
                                _csvContent.Append(child.HeaderNumber + " " + child.Header.Replace(";", ", "));
                                _csvContent.Append(";" + child.Value.ToString().Replace(';', ',').Replace(Environment.NewLine, "   "));

                                if (Config.ExportComments) _csvContent.Append(";" + child.Comment);
                                _csvContent.AppendLine();
                            }
                        }
                    }
                    ExportTreeViewManagementReport(child, includeUeberleitung, accountBalances, GuV);
                } catch (Exception ex) {
                    throw new Exception(ex.Message, ex);
                }
            }
        }
        #endregion ExportTreeViewManagementReport

        #region ExportTreeViewManagementReport
        //private void ExportTreeViewManagementReport(PdfTreeView tv, bool includeUeberleitung = false, bool accountBalances = false) {
        
        private void ExportTreeViewManagementReport(ValueTree vtree, Taxonomy.Interfaces.PresentationTree.IPresentationTreeNode root) {
            try {
                foreach (IPresentationTreeEntry child in root.Children) {

                    //Skip if only mandatory fields wanted -- sev
                    if (child.Element != null && Config.ExportMandatoryOnly && !child.Element.IsMandatoryField) {
                        continue;
                    }

                    IPresentationTreeNode pchild = child as IPresentationTreeNode;
                    IValueTreeEntry value = vtree.Root.Values[pchild.Element.Name];
                    XbrlElementValueBase valueBase = value as XbrlElementValueBase;

                    if (value.Value != null) {
                        _csvContent.Append(child.Element.Label + ";" + value.Value.ToString().Replace(';',',').Replace(Environment.NewLine, "   "));
                    } else {_csvContent.Append(child.Element.Label + ";" + string.Empty); }
                    if (Config.ExportComments) _csvContent.Append(";" + valueBase.Comment);
                    _csvContent.AppendLine();
                    

                    ExportTreeViewManagementReport(vtree, child as IPresentationTreeNode);
                }
            } catch (Exception ex) {
                throw new Exception(ex.Message, ex);
            }

        }
        #endregion ExportTreeViewManagementReport

        private IExportConfig Config { get; set; }


        #region Got from the PdfExporter

        #region ExportReportInformation
        void ExportReportInformation() {

            _csvContent = new StringBuilder();
            _filename = Config.Document.Company.Name +
                        ", Geschäftsjahr " + Config.Document.FinancialYear.FYear + "_" +
                        "Berichtsinformationen";
            foreach (var disallowedChar in _disallowedChars) {
                _filename = _filename.Replace(disallowedChar, "_");
            }
            _filename = Config.FilePath + "\\" + _filename + ".csv";

            using (
                var csvWrite = new StreamWriter(File.Open(_filename, FileMode.Create),
                                                Encoding.UTF8)) {

                                                    {
                                                        _csvContent.Append("Identifikationsmerkmale des Berichts");
                                                        _csvContent.AppendLine();

                                                        string[] ids = {
                                                            "de-gcd_genInfo.report.id.reportType",
                                                            "de-gcd_genInfo.report.id.reportStatus",
                                                            "de-gcd_genInfo.report.id.revisionStatus",
                                                            "de-gcd_genInfo.report.id.reportElement.allocation.ntAss",
                                                            "de-gcd_genInfo.report.id.reportElement.allocation.incomeUse",
                                                            "de-gcd_genInfo.report.id.reportElement.allocation.payablesReceivablesAgeingReport"
                                                            ,
                                                            "de-gcd_genInfo.report.id.statementType",
                                                            "de-gcd_genInfo.report.id.statementType.tax",
                                                            "de-gcd_genInfo.report.id.accountingStandard",
                                                            "de-gcd_genInfo.report.id.specialAccountingStandard",
                                                            "de-gcd_genInfo.report.id.incomeStatementFormat",
                                                            "de-gcd_genInfo.report.id.consolidationRange",
                                                            "de-gcd_genInfo.report.id.consideredInConsolidatedStatement"
                                                        };
                                                        CreateTableFromIds("", ids);

                                                        //Two normal booleans
                                                        AddTaxonomyTableCell(
                                                            "de-gcd_genInfo.report.id.statementType.restated", true);
                                                        AddTaxonomyTableCell(
                                                            "de-gcd_genInfo.report.id.incomeStatementendswithBalProfit", true);

                                                        //List of MultipleChoice elements
                                                        XbrlElementValue_MultipleChoice reportElements =
                                                            Config.Document.ValueTreeGcd.Root.Values[
                                                                "de-gcd_genInfo.report.id.reportElement"] as
                                                            XbrlElementValue_MultipleChoice;
                                                        for (int i = 0; i < reportElements.Elements.Count; ++i) {

                                                            bool? isChecked =
                                                                reportElements.IsChecked[reportElements.Elements[i].Id
                                                                    ].BoolValue;

                                                            if (isChecked.HasValue) {
                                                                _csvContent.Append(
                                                                    reportElements.Elements[i].MandatoryLabel);
                                                                _csvContent.Append(isChecked.Value ? ";Ja" : ";Nein");
                                                                _csvContent.AppendLine();
                                                            }

                                                        }

                                                    }
                                                    {

                                                        //XbrlElementValue_Tuple tuple = (XbrlElementValue_Tuple)Config.Document.ValueTreeGcd.Root.Values["de-gcd_genInfo.report.id.accordingTo"];
                                                        //CreateTableFromTuple(tuple);
                                                        AddTaxonomyTableCell("de-gcd_genInfo.report.id.accordingTo");
                                                    }
                                                    {
                                                        _csvContent.Append("Berichtsperiode");
                                                        _csvContent.AppendLine();
                                                        string[] ids = {
                                                            "de-gcd_genInfo.report.period.reportPeriodBegin",
                                                            "de-gcd_genInfo.report.period.reportPeriodEnd",
                                                            "de-gcd_genInfo.report.period.fiscalYearBegin",
                                                            "de-gcd_genInfo.report.period.fiscalYearEnd",
                                                            "de-gcd_genInfo.report.period.balSheetClosingDate",
                                                            "de-gcd_genInfo.report.period.fiscalPreciousYearBegin",
                                                            "de-gcd_genInfo.report.period.fiscalPreciousYearEnd",
                                                            "de-gcd_genInfo.report.period.balSheetClosingDatePreviousYear"
                                                        };

                                                        CreateTableFromIds("", ids);
                                                    }

                csvWrite.WriteLine(_csvContent);
                csvWrite.Close();
            }
        }
        #endregion ExportReportInformation

        #region ExportDocumentInformation
        private void ExportDocumentInformation() {
            
                _csvContent = new StringBuilder();
                _filename = Config.Document.Company.Name +
                                                 ", Geschäftsjahr " + Config.Document.FinancialYear.FYear + "_" +
                                                 "Dokumentinformationen";
                foreach (var disallowedChar in _disallowedChars) {
                    _filename = _filename.Replace(disallowedChar, "_");
                }
                _filename = Config.FilePath + "\\" + _filename + ".csv";

                using (
                    var csvWrite = new StreamWriter(File.Open(_filename, FileMode.Create),
                                                    Encoding.UTF8)) {

                                                        {
                                                            _csvContent.Append(
                                                                "Identifikationsmerkmale des Dokuments");
                                                            _csvContent.AppendLine();

                                                            string[] ids = {
                                                                "de-gcd_genInfo.doc.id.generationDate",
                                                                "de-gcd_genInfo.doc.id.generationReason",
                                                                "de-gcd_genInfo.doc.id.contents",
                                                                "de-gcd_genInfo.doc.id.origLanguage",
                                                                "de-gcd_genInfo.doc.id.disclosable"
                                                            };
                                                            CreateTableFromIds("", ids);
                                                        }
                                                        {
                                                            //pdf.AddSubHeadline("Dokumentersteller");
                                                            //XbrlElementValue_Tuple tuple = (XbrlElementValue_Tuple)Config.Document.ValueTreeGcd.Root.Values["de-gcd_genInfo.doc.author"];
                                                            AddTaxonomyTableCell("de-gcd_genInfo.doc.author");
                                                        }
                                                        {

                                                            _csvContent.Append("Dokumentrevisionen");
                                                            _csvContent.AppendLine();
                                                            string[] ids = {
                                                                "de-gcd_genInfo.doc.rev.date",
                                                                "de-gcd_genInfo.doc.rev.versionNo",
                                                                "de-gcd_genInfo.doc.rev.dateOfChange",
                                                                "de-gcd_genInfo.doc.rev.changeInitiator"
                                                            };
                                                            CreateTableFromIds("", ids);
                                                        }

                    csvWrite.WriteLine(_csvContent);
                    csvWrite.Close();
                }
        }
        #endregion ExportDocumentInformation

        #region ExportCompanyInformation
        private void ExportCompanyInformation() {
            //"Unternehmensinformation");

            bool nl = false;

                _csvContent = new StringBuilder();
                _filename = Config.Document.Company.Name +
                                                 ", Geschäftsjahr " + Config.Document.FinancialYear.FYear + "_" +
                                                 "Unternehmensinformation";
                foreach (var disallowedChar in _disallowedChars) {
                    _filename = _filename.Replace(disallowedChar, "_");
                }
                _filename = Config.FilePath + "\\" + _filename + ".csv";

                using (
                    var csvWrite = new StreamWriter(File.Open(_filename, FileMode.Create),
                                                    Encoding.UTF8)) {
                                                        {
                                                            //_csvContent.Append("Allgemein");
                                                            //_csvContent.AppendLine();

                                                            AddTaxonomyTableCell("de-gcd_genInfo.company.id.name", false);

                                                            //Former company name (if available)
                                                            if (
                                                                AddTaxonomyTableCell(
                                                                    "de-gcd_genInfo.company.id.name.formerName", true))
                                                                AddTaxonomyTableCell(
                                                                    "de-gcd_genInfo.company.id.name.dateOfLastChange", true);

                                                            //Address
                                                            string address =
                                                                Config.Document.ValueTreeGcd.Root.Values[
                                                                    "de-gcd_genInfo.company.id.location.street"].DisplayString +
                                                                " "
                                                                +
                                                                Config.Document.ValueTreeGcd.Root.Values[
                                                                    "de-gcd_genInfo.company.id.location.houseNo"].DisplayString +
                                                                ", "
                                                                +
                                                                Config.Document.ValueTreeGcd.Root.Values[
                                                                    "de-gcd_genInfo.company.id.location.zipCode"].DisplayString +
                                                                " "
                                                                +
                                                                Config.Document.ValueTreeGcd.Root.Values[
                                                                    "de-gcd_genInfo.company.id.location.city"].DisplayString +
                                                                ", "
                                                                +
                                                                Config.Document.ValueTreeGcd.Root.Values[
                                                                    "de-gcd_genInfo.company.id.location.country"].DisplayString;
                                                            _csvContent.Append("Adresse");
                                                            _csvContent.Append(";" + address);
                                                            _csvContent.AppendLine();

                                                            nl = AddTaxonomyTableCell("de-gcd_genInfo.company.id.location", true);
                                                            if (nl) {
                                                            _csvContent.AppendLine();
                                                            }
                                                            //Legalform
                                                            nl = AddTaxonomyTableCell("de-gcd_genInfo.company.id.legalStatus", true);

                                                            if (nl) {
                                                            _csvContent.AppendLine();
                                                            }
                                                            //Former legalform (if available)
                                                            if (
                                                                AddTaxonomyTableCell(
                                                                    "de-gcd_genInfo.company.id.legalStatus.formerStatus", true)) {
                                                                nl = AddTaxonomyTableCell(
                                                                    "de-gcd_genInfo.company.id.legalStatus.dateOfLastChange",
                                                                    true);
                                                            }

                                                        }

                                                        {
                                                            //_csvContent.Append("Kennnummern");
                                                            //_csvContent.AppendLine();
                                                            //XbrlElementValue_Tuple tuple =
                                                            //    (XbrlElementValue_Tuple)
                                                            //    Config.Document.ValueTreeGcd.Root.Values[
                                                            //        "de-gcd_genInfo.company.id.idNo"];
                                                            //AddTupleToTable(tuple);
                                                            if (nl) {
                                                            _csvContent.AppendLine();
                                                            }
                                                            nl = AddTaxonomyTableCell("de-gcd_genInfo.company.id.idNo", true);
                                                        }

                                                        {
                                                            if (nl) {
                                                                _csvContent.AppendLine();
                                                            }
                                                            nl = AddTaxonomyTableCell("de-gcd_genInfo.company.id.shareholder", true);
                                                        }

                                                        {
                                                            if (nl) {
                                                                _csvContent.AppendLine();
                                                            }
                                                            _csvContent.Append("Registereintrag");
                                                            _csvContent.AppendLine();
                                                            if (
                                                                Config.Document.ValueTreeGcd.Root.Values[
                                                                    "de-gcd_genInfo.company.id.Incorporation.Type"].HasValue) {
                                                                string[] ids = new string[] {
                                                                    "Type", "prefix", "section", "number",
                                                                    "suffix", "court",
                                                                    "dateOfFirstRegistration"
                                                                };
                                                                nl = CreateTableFromIds("de-gcd_genInfo.company.id.Incorporation.",
                                                                                   ids);
                                                            }
                                                        }
                                                        {
                                                            if (nl) {
                                                                _csvContent.AppendLine();
                                                            }
                                                            nl = AddTaxonomyTableCell("de-gcd_genInfo.company.id.stockExch", true);
                                                        }

                                                        {
                                                            if (nl) {
                                                                _csvContent.AppendLine();
                                                            }
                                                            nl = AddTaxonomyTableCell("de-gcd_genInfo.company.id.contactAddress", true);
                                                        }
                    //Add all other information
                                                        {
                                                            if (nl) {
                                                                _csvContent.AppendLine();
                                                            }
                                                            _csvContent.Append("Sonstige Informationen");
                                                            string[] ids = new string[] {
                                                                "de-gcd_genInfo.company.id.lastTaxAudit",
                                                                "de-gcd_genInfo.company.id.sizeClass",
                                                                "de-gcd_genInfo.company.id.business",
                                                                "de-gcd_genInfo.company.id.CompanyStatus",
                                                                "de-gcd_genInfo.company.id.FoundationDate",
                                                                "de-gcd_genInfo.company.id.internet",
                                                                "de-gcd_genInfo.company.id.internet.description",
                                                                "de-gcd_genInfo.company.id.internet.url",
                                                                "de-gcd_genInfo.company.id.comingfrom",
                                                                "de-gcd_genInfo.company.id.companyLogo",
                                                                "de-gcd_genInfo.company.userSpecific"
                                                            };
                                                            CreateTableFromIds("", ids);
                                                            XbrlElementValue_Tuple tuple =
                                                                (XbrlElementValue_Tuple)
                                                                Config.Document.ValueTreeGcd.Root.Values[
                                                                    "de-gcd_genInfo.company.id.industry"];
                                                            foreach (var item in tuple.Items[0].Values) {
                                                                if (!item.Value.HasValue) continue;
                                                                _csvContent.Append(item.Value.ToString());
                                                                _csvContent.Append(";" + item.Value.DisplayString);
                                                                _csvContent.AppendLine();
                                                            }

                                                        }

                    csvWrite.WriteLine(_csvContent);
                    csvWrite.Close();
                }
        }
        #endregion ExportCompanyInformation


        #region AddTaxonomyTableCell
        /// <summary>
        /// Adds two strings to the table by taking the name of the GCD_Taxonomy of the specified id
        /// and the value specified in the database. If checkExistance is set, then the strings are only added
        /// if the value in the database is non-null.
        /// </summary>
        /// <param name="table">Input table</param>
        /// <param name="id">Example: genInfo.company.id.lastTaxAudit</param>
        /// <param name="checkExistance">If true: Cells will only be created if the entry exists</param>
        /// <returns>Returns true if the value in the database is non-null</returns>
        private bool AddTaxonomyTableCell(string id, bool checkExistance = false) {
            if (checkExistance && !Config.Document.ValueTreeGcd.Root.Values[id].HasValue)
                return false;
            _csvContent.Append(Config.Document.GcdTaxonomy.Elements[id].MandatoryLabel);

            if (Config.Document.ValueTreeGcd.Root.Values[id].Element.ValueType == Taxonomy.Enums.XbrlElementValueTypes.Boolean
                && Config.Document.ValueTreeGcd.Root.Values[id].HasValue) {
                XbrlElementValue_Boolean element = Config.Document.ValueTreeGcd.Root.Values[id] as XbrlElementValue_Boolean;
                _csvContent.Append((bool)element.Value ? ";Ja" : ";Nein");
                _csvContent.AppendLine();
            } else if (Config.Document.ValueTreeGcd.Root.Values[id].Element.ValueType == XbrlElementValueTypes.Tuple
                && Config.Document.ValueTreeGcd.Root.Values[id].HasValue) {
                _csvContent.AppendLine();
                AddTupleToTable(Config.Document.ValueTreeGcd.Root.Values[id] as XbrlElementValue_Tuple);
            } else {
                _csvContent.Append(";" + Config.Document.ValueTreeGcd.Root.Values[id].DisplayString);
                _csvContent.AppendLine();
            }
            return true;
        }
        #endregion


        #region AddTupleToTable
        private void AddTupleToTable(XbrlElementValue_Tuple tuple) {
            foreach (var person in tuple.Items) {
                foreach (var item in person.Values) {

                    //Check if ExportMandatoryOnly -- sev
                    if (Config.ExportMandatoryOnly && !item.Value.Element.IsMandatoryField) {
                        continue;
                    }

                    if (item.Value.Element.ValueType == Taxonomy.Enums.XbrlElementValueTypes.Tuple) {
                        XbrlElementValue_Tuple localTuple = (XbrlElementValue_Tuple)item.Value;
                        if (item.Key == "de-gcd_genInfo.company.id.shareholder.ShareDivideKey") {
                            _csvContent.Append(item.Value.ToString() + ";");
                            _csvContent.Append(localTuple.Items[0].Values["de-gcd_genInfo.company.id.shareholder.ShareDivideKey.numerator"].DisplayString + " / " +
                                localTuple.Items[0].Values["de-gcd_genInfo.company.id.shareholder.ShareDivideKey.denominator"].DisplayString);
                    _csvContent.AppendLine();

                        } else AddTupleToTable(localTuple);
                        //System.Diagnostics.Debug.WriteLine(item.);
                    } else if (item.Value.Element.ValueType == Taxonomy.Enums.XbrlElementValueTypes.Boolean
                        && item.Value.HasValue) {
                        XbrlElementValue_Boolean flag = item.Value as XbrlElementValue_Boolean;
                        _csvContent.Append(item.Value.ToString());
                        _csvContent.Append((bool)flag.Value ? ";Ja" : ";Nein");
                    _csvContent.AppendLine();
                    } else if (item.Value.HasValue) {
                        _csvContent.Append(item.Value.ToString());
                        _csvContent.Append(";" + item.Value.DisplayString);
                    _csvContent.AppendLine();
                    }

                }

            }
        }
        #endregion AddTupleToTable

        #region CreateTableFromIds
        /// <summary>
        /// For each id two cells are created. The ids used are prefix+id[x].
        /// </summary>
        /// <param name="prefix">The prefix for each id</param>
        /// <param name="ids">Array of ids</param>
        /// <returns></returns>
        private bool CreateTableFromIds(string prefix, string[] ids) {
            bool ret = false;
            foreach (var id in ids) {
                ret = AddTaxonomyTableCell(prefix + id, true);
            }
            return ret;
        }
        #endregion CreateTableFromIds

        #endregion Got from the PdfExporter


    }
}