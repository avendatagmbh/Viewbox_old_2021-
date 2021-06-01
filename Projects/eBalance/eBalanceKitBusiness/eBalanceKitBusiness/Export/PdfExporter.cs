using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.AccessControl;
using System.Security.Permissions;
using System.Xml.Linq;
using Taxonomy.Enums;
using Taxonomy.Interfaces;
using Taxonomy.Interfaces.PresentationTree;
using eBalanceKitBase.Structures;
using eBalanceKitBusiness.Interfaces;
using eBalanceKitBusiness.Reconciliation;
using eBalanceKitBusiness.Reconciliation.Enums;
using eBalanceKitBusiness.Reconciliation.Interfaces;
using eBalanceKitBusiness.Reconciliation.Interfaces.ReconciliationTypes;
using eBalanceKitBusiness.Reconciliation.ViewModels;
using eBalanceKitBusiness.Structures;
using eBalanceKitBusiness.Structures.DbMapping.BalanceList;
using eBalanceKitBusiness.Structures.ValueTree;
using eBalanceKitBusiness.Structures.XbrlElementValue;
using eBalanceKitResources.Localisation;
using iTextSharp.text;
using iTextSharp.text.pdf;
using PdfGenerator;
using Taxonomy.PresentationTree;
using System.Linq;
using System.IO;
using eBalanceKitBusiness.Manager;
using iTextSharp.text.pdf.draw;
using IElement = Taxonomy.IElement;
using Utils;

namespace eBalanceKitBusiness.Export {
    /// <summary>
    /// 
    /// </summary>
    /// <author>Mirko Dibbert / Benjamin Held</author>
    /// <since>2011-05-10</since>
    public class PdfExporter {
        private ConfigExport Config { get; set; }

        private Font font = new Font(Font.FontFamily.HELVETICA, 8);
        private Font fontBold = new Font(Font.FontFamily.HELVETICA, 8, Font.BOLD);
        private Font fontItalic = new Font(Font.FontFamily.HELVETICA, 8, Font.ITALIC);
        private PdfGenerator.PdfGenerator pdf;

        #region Export
        public void Export(IExportConfig config) {
            Config = config as ConfigExport;
            UserManager.Instance.CurrentUser.Options.SetPdfExportOptionsValue(config);

            var directoryName = ExportHelper.GetValidFilePath(Config.Filename);
            var directory = new DirectoryInfo(directoryName);
            if (!directory.Exists) directory.Create();


            // Set default name if no file name is specified (avoid DirectoryNotFoundException) -- SeV
            var defaultname = Config.Document.Company.Name + "_" + Config.Document.FinancialYear.FYear + "_" +
                              Config.Document.Name + ".pdf";

            var tmpFileName = ExportHelper.GetValidFileName(Config.Filename);
            tmpFileName = string.IsNullOrEmpty(tmpFileName) ? defaultname : tmpFileName;

            var fileName = Config.Filename = directoryName + "\\" + tmpFileName;

            try {
                PdfTraits traits = new PdfTraits("eBilanz-Kit", "eBilanz-Kit");

                pdf = new PdfGenerator.PdfGenerator(traits);

                GenerateContent();

                Phrase headerPhrase = new Phrase(
                    Config.Document.Name +
                    " (" + Config.Document.Company.Name + ", Geschäftsjahr " + Config.Document.FinancialYear.FYear + ")",
                    pdf.Traits.fontH1);

                pdf.WriteFile(fileName, headerPhrase);
                //Process.Start(fileName);
            } catch (IOException ioEx) {
                if (ioEx.Message.Contains("fehlt ein erforderliches Recht")) {
                    throw new Exception(
                        "Bei der Erstellung des PDF-Reports ist ein Fehler aufgetreten: " + Environment.NewLine +
                        "Keine Berechtigung.");
                }

                if (ioEx.Message.Contains("no pages")) {
                    throw new Exception(
                        "Bei der Erstellung des PDF-Reports ist ein Fehler aufgetreten: " + Environment.NewLine +
                        "Das Dokument enthält keine Inhalte. Vermutlich wurde keine Berichtsbestandteil zum Export gewählt.");
                }

                throw new Exception(
                    "Bei der Erstellung des PDF-Reports ist ein Fehler aufgetreten: " + Environment.NewLine +
                    "Scheinbar ist die Datei bereits vorhanden und wird von einer anderen Anwendung verwendet.");
            } catch (Exception ex) {
                throw new Exception(
                    "Bei der Erstellung des PDF-Reports ist ein Fehler aufgetreten: " + Environment.NewLine + ex.Message,
                    ex);
            }
        }
        #endregion Export

        #region GenerateContent
        private void GenerateContent() {
            string[] shortParts = {
                "appropriationProfits", "contingentLiabilities", "cashFlowStatement",
                "adjustmentOfIncome", "determinationOfTaxableIncome", "determinationOfTaxableIncomeBusinessPartnership",
                "determinationOfTaxableIncomeSpecialCases"
            };
            bool[] exportParts = {
                Config.ExportAppropriationProfit, Config.ExportContingentLiabilities,
                Config.ExportCashFlowStatement, Config.ExportAdjustmentOfIncome,
                Config.ExportDeterminationOfTaxableIncome, Config.ExportDeterminationOfTaxableIncomeBusinessPartnership,
                Config.ExportDeterminationOfTaxableIncomeSpecialCases
            };
            Dictionary<string, bool> partPairs = new Dictionary<string, bool>();
            for (int i = 0; i < shortParts.Length; i++) {
                var uri = ExportHelper.GetUri(shortParts[i], Config.Document);
                if (!string.IsNullOrEmpty(uri))
                    partPairs.Add(uri, exportParts[i]);
            }
            if (Config.ExportAsXbrl || Config.ExportDocumentInformation) ExportDocumentInformation();
            if (Config.ExportAsXbrl || Config.ExportReportInformation) ExportReportInformation();
            if ((Config.ExportAsXbrl || Config.ExportCompanyInformation) && RightManager.RightDeducer.CompanyVisibleForUser(Config.Document.Company, UserManager.Instance.CurrentUser))
                ExportCompanyInformation();
            RightManager.ReadRestDocumentAllowed(Config.Document);
            if (Config.ExportAsXbrl || Config.ExportBalanceSheet) ExportBalanceSheets();
            if (Config.ExportAsXbrl || Config.ExportIncomeStatement) ExportIncomeStatements();
            foreach (KeyValuePair<string, bool> keyValuePair in partPairs) {
                if (Config.ExportAsXbrl || keyValuePair.Value)
                    ExportPart(keyValuePair.Key);
            }
            if (Config.ExportAsXbrl || Config.ExportNotes) ExportDefault("notes");
            _isFirstAdd = true;
            if (Config.ExportAsXbrl || Config.ExportManagementReport) ExportManagementReport();
            if (Config.ExportAsXbrl || Config.ExportOtherReportElements) ExportDefault("OtherReportElements");

            if (Config.ExportNtAssGross) ExportHyperCube("de-gaap-ci_table.nt.ass.gross");
            if (Config.ExportNtAssGrossShort) ExportHyperCube("de-gaap-ci_table.nt.ass.gross_short");
            if (Config.ExportNtAssNet) ExportHyperCube("de-gaap-ci_table.nt.ass.net");
            if (Config.ExportEquityStatement) ExportHyperCube("de-gaap-ci_table.eqCh");
            if (Config.ExportAsXbrl) ExportReconciliations();

            //if (Config.ExportAccountBalances) ExportAccountBalances();
            if (!Config.ExportAsXbrl && Config.ExportUnassignedAccounts) ExportUnassignedAccounts();
        }
        #endregion GenerateContent

        #region ExportReconciliations
        private readonly Dictionary<ReconciliationTypes, string> _reconciliationTypeMapping =
            new Dictionary<ReconciliationTypes, string> {
                {ReconciliationTypes.Reclassification, ResourcesReconciliation.ReconciliationReclassification},
                {ReconciliationTypes.ValueChange, ResourcesReconciliation.ReconciliationValueChange},
                {ReconciliationTypes.Delta, ResourcesReconciliation.ReconciliationDelta},
                {ReconciliationTypes.ImportedValues, ResourcesReconciliation.ReconciliationImport},
                {ReconciliationTypes.AuditCorrectionPreviousYear, ResourcesReconciliation.AuditCorrectionPreviousYearValueCaption},
                {ReconciliationTypes.AuditCorrection, ResourcesReconciliation.ReconciliationAuditCorrection},
                {ReconciliationTypes.TaxBalanceValue, ResourcesReconciliation.TaxBalanceValue},
            };

        private readonly Dictionary<ReconciliationPresentationTreeTypes, string>
            _reconciliationPresentationTreeTypesMapping = new Dictionary<ReconciliationPresentationTreeTypes, string> {
                {
                    ReconciliationPresentationTreeTypes.IncomeStatement,
                    ResourcesReconciliation.IncomeStatement
                    },
                {
                    ReconciliationPresentationTreeTypes.
                        BalanceSheetTotalAssets, ResourcesReconciliation.BalanceListAssets
                    },
                {
                    ReconciliationPresentationTreeTypes.
                        BalanceSheetLiabilities, ResourcesReconciliation.BalanceListLiabilities
                    }
            };

        private struct MultiKeyId {
            public IReconciliation Reconciliation;
            public ReconciliationPresentationTreeTypes PresentationTreeType;
        }

        private void ExportReconciliations() {
            var role =
                Config.Document.MainTaxonomy.GetRole(ExportHelper.GetUri("transfersCommercialCodeToTax", Config.Document));
            pdf.AddHeadline(role.Name);

            ObjectWrapper<Structures.DbMapping.Document> documentWrapper =
                new ObjectWrapper<Structures.DbMapping.Document> {Value = Config.Document};
            ReconciliationsModel reconciliationsModel = new ReconciliationsModel(documentWrapper.Value);
            IReconciliationManager mgr = new ReconciliationManager(Config.Document);
            // we can iterate through all reconciliations, and transactions, but there is no differencing in tabs like liabilities, assets, income statement.
            // we build that type of group byed data structure.
            Dictionary
                <MultiKeyId, List<IReconciliationTransaction>>
                groupedReconciliations = BuildGroupedReconciliations(mgr.Reconciliations, reconciliationsModel);

            pdf.AddSubHeadline(ResourcesReconciliation.Reconciliations);
            PdfPTable resultTable = new PdfPTable(1) {SpacingBefore = 5f, WidthPercentage = 100f};
            resultTable.DefaultCell.PaddingBottom = 10f;
            resultTable.DefaultCell.Border = 0;
            Font defaultFont = new Chunk().Font;

            foreach (IReconciliation reconciliation  in mgr.Reconciliations) {
                Phrase phrase = new Phrase {
                    new Chunk(ResourcesCommon.Name + ": ", pdf.Traits.fontH1),
                    new Chunk(reconciliation.Name + ResourcesPdfExport.NotSend, defaultFont)
                };
                resultTable.AddCell(phrase);
                phrase = new Phrase {
                    new Chunk(ResourcesPdfExport.ReconciliationType, pdf.Traits.fontH1),
                    new Chunk(_reconciliationTypeMapping[reconciliation.ReconciliationType], defaultFont)
                };
                resultTable.AddCell(phrase);
                phrase = new Phrase {
                    new Chunk(ResourcesPdfExport.CommentLabel, pdf.Traits.fontH1),
                    new Chunk(reconciliation.Comment, defaultFont)
                };
                resultTable.AddCell(phrase);

                PdfPTable assetsTable = GenerateTableFromGroupedReconciliation(groupedReconciliations, reconciliation,
                                                                               ReconciliationPresentationTreeTypes.
                                                                                   BalanceSheetTotalAssets);
                // > 1 because the first row is the header row, it's always present.
                if (assetsTable.Rows.Count > 1) {
                    phrase = new Phrase {
                        new Chunk(
                            _reconciliationPresentationTreeTypesMapping[
                                ReconciliationPresentationTreeTypes.BalanceSheetTotalAssets], pdf.Traits.fontH1)
                    };
                    resultTable.AddCell(phrase);
                    resultTable.AddCell(assetsTable);
                }

                PdfPTable liabilitiesTable = GenerateTableFromGroupedReconciliation(groupedReconciliations,
                                                                                    reconciliation,
                                                                                    ReconciliationPresentationTreeTypes.
                                                                                        BalanceSheetLiabilities);
                // > 1 because the first row is the header row, it's always present.
                if (liabilitiesTable.Rows.Count > 1) {
                    phrase = new Phrase {
                        new Chunk(
                            _reconciliationPresentationTreeTypesMapping[
                                ReconciliationPresentationTreeTypes.BalanceSheetLiabilities], pdf.Traits.fontH1)
                    };
                    resultTable.AddCell(phrase);
                    resultTable.AddCell(liabilitiesTable);
                }

                PdfPTable incomeStatementTable = GenerateTableFromGroupedReconciliation(groupedReconciliations,
                                                                                        reconciliation,
                                                                                        ReconciliationPresentationTreeTypes
                                                                                            .IncomeStatement);
                // > 1 because the first row is the header row, it's always present.
                if (incomeStatementTable.Rows.Count > 1) {
                    phrase = new Phrase {
                        new Chunk(
                            _reconciliationPresentationTreeTypesMapping[
                                ReconciliationPresentationTreeTypes.IncomeStatement], pdf.Traits.fontH1)
                    };
                    resultTable.AddCell(phrase);
                    resultTable.AddCell(incomeStatementTable);
                }

                LineSeparator lineSeparator = new LineSeparator(1f, 100f, BaseColor.BLACK, Element.ALIGN_LEFT, 0f);
                resultTable.AddCell(new Phrase(new Chunk(lineSeparator)));
            }
            pdf.CurrentSection.Add(resultTable);
        }

        /// <summary>
        /// creates a table from the grouped reconciliations depending on the given reconciliation,
        ///  and presentation tree type (for example value change)
        /// </summary>
        /// <param name="groupedReconciliations">the source of information</param>
        /// <param name="reconciliation">the given reconciliation</param>
        /// <param name="presentationTreeType">the given presentation tree type</param>
        /// <returns>pdf table</returns>
        private PdfPTable GenerateTableFromGroupedReconciliation(
            Dictionary<MultiKeyId, List<IReconciliationTransaction>> groupedReconciliations,
            IReconciliation reconciliation, ReconciliationPresentationTreeTypes presentationTreeType) {

            // header row
            PdfPTable ret = new PdfPTable(2) {HeaderRows = 1};
            ret.AddCell(ResourcesPdfExport.Position);
            ret.AddCell(ResourcesPdfExport.ActualYearValue);

            foreach (
                KeyValuePair
                    <MultiKeyId,
                        List<IReconciliationTransaction>> groupedReconciliation in groupedReconciliations) {
                if (groupedReconciliation.Key.Reconciliation != reconciliation ||
                    groupedReconciliation.Key.PresentationTreeType != presentationTreeType)
                    continue;

                foreach (IReconciliationTransaction transaction in groupedReconciliation.Value) {
                    ret.AddCell(string.Format("{0} ({1})",
                                              transaction.Position.Label,
                                              transaction.Position.Name));
                    switch (reconciliation.ReconciliationType) {
                        case ReconciliationTypes.Reclassification:
                            // if it's a reclassification we need the value from the value tree to have the transfer value, which will be displayed.
                            XbrlElementValue_Monetary monetaryValue =
                                (Config.Document.ValueTreeMain.GetValue(transaction.Position.Id) as
                                 XbrlElementValue_Monetary);
                            Debug.Assert(monetaryValue != null);
                            //IReconciliationInfo info = new ReconciliationInfo(Config.Document,
                            //                                                  Config.Document.ValueTreeMain.GetValue(
                            //                                                      transaction.Position.Id),
                            //                                                  reconciliation.Transactions.ToList());
                            ret.AddCell(monetaryValue.ReconciliationInfo.TransferValueDisplayString);
                            break;
                        case ReconciliationTypes.ImportedValues:
                        case ReconciliationTypes.ValueChange:
                        case ReconciliationTypes.Delta:
                        case ReconciliationTypes.AuditCorrection:
                        case ReconciliationTypes.AuditCorrectionPreviousYear:
                        case ReconciliationTypes.TaxBalanceValue:
                            ret.AddCell(transaction.ValueDisplayString);
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                }
            }
            return ret;
        }

        /// <summary>
        /// creates a data structure for exporting reconciliations grouped by their tab.
        /// </summary>
        /// <param name="reconciliations">all reconciliation</param>
        /// <param name="reconciliationsModel">a model for extracting information about which id is inside each tab</param>
        /// <returns>grouped data structure</returns>
        private Dictionary<MultiKeyId, List<IReconciliationTransaction>> BuildGroupedReconciliations(
            IEnumerable<IReconciliation> reconciliations, ReconciliationsModel reconciliationsModel) {
            Dictionary<MultiKeyId, List<IReconciliationTransaction>> ret =
                new Dictionary<MultiKeyId, List<IReconciliationTransaction>>();
            // differencing tabs by id prefix.
            string prefixBalanceListAssets =
                reconciliationsModel.PresentationTreeBalanceSheetTotalAssets.RootEntries.Any()
                    ? reconciliationsModel.PresentationTreeBalanceSheetTotalAssets.RootEntries.First().Element.Name
                    : null;
            string prefixBalanceListLiabilities =
                reconciliationsModel.PresentationTreeBalanceSheetLiabilities.RootEntries.Any()
                    ? reconciliationsModel.PresentationTreeBalanceSheetLiabilities.RootEntries.First().Element.Name
                    : null;
            string prefixIncomeStatement = reconciliationsModel.PresentationTreeIncomeStatement.RootEntries.Any()
                                               ? reconciliationsModel.PresentationTreeIncomeStatement.RootEntries.
                                                     First().Element.Name
                                               : null;
            foreach (IReconciliation reconciliation in reconciliations) {
                MultiKeyId reconciliationKeyToIncomeStatement = new MultiKeyId {
                    Reconciliation = reconciliation,
                    PresentationTreeType =
                        ReconciliationPresentationTreeTypes.
                            IncomeStatement
                };
                MultiKeyId reconciliationKeyToSheetLiabilites = new MultiKeyId {
                    Reconciliation = reconciliation,
                    PresentationTreeType =
                        ReconciliationPresentationTreeTypes.
                            BalanceSheetLiabilities
                };
                MultiKeyId reconciliationKeyToSheetTotalAssets = new MultiKeyId {
                    Reconciliation = reconciliation,
                    PresentationTreeType =
                        ReconciliationPresentationTreeTypes.
                            BalanceSheetTotalAssets
                };
                // each key is connected to one reconciliation's one tab. For example 'A' reconciliation's liabilities tab.
                // 'A' reconciliation's assets tab is other key.
                ret[reconciliationKeyToIncomeStatement] = new List<IReconciliationTransaction>();
                ret[reconciliationKeyToSheetLiabilites] = new List<IReconciliationTransaction>();
                ret[reconciliationKeyToSheetTotalAssets] = new List<IReconciliationTransaction>();
                foreach (IReconciliationTransaction reconciliationTransaction in reconciliation.Transactions) {
                    if (reconciliationTransaction.Position == null) {
                        continue;
                    }
                    string id = reconciliationTransaction.Position.Id.Split(new[] {'_'}, 2)[1];
                    if (prefixBalanceListAssets != null && id.StartsWith(prefixBalanceListAssets)) {
                        ret[reconciliationKeyToSheetTotalAssets].Add(reconciliationTransaction);
                    } else if (prefixBalanceListLiabilities != null && id.StartsWith(prefixBalanceListLiabilities)) {
                        ret[reconciliationKeyToSheetLiabilites].Add(reconciliationTransaction);
                    } else if (prefixIncomeStatement != null && id.StartsWith(prefixIncomeStatement)) {
                        ret[reconciliationKeyToIncomeStatement].Add(reconciliationTransaction);
                    } else {
                        throw new NotImplementedException();
                    }
                }
            }
            return ret;
        }
        #endregion ExportReconciliations

        #region ExportHyperCube
        private void ExportHyperCube(string elementId) {
            foreach (IPresentationTree ptree in Config.Document.GaapPresentationTrees.Values) {
                if (ptree.HasNode(elementId)) {
                    eBalanceKitBusiness.Interfaces.PresentationTree.IPresentationTreeNode node =
                        ptree.GetNode(elementId) as
                        eBalanceKitBusiness.Interfaces.PresentationTree.IPresentationTreeNode;
                    PdfPTable table = new HyperCubeExporter(Config).GeneratePdfContentLimited(node);
                    if (table != null) {
                        pdf.AddHeadline(node.Element.Label);
                        pdf.CurrentChapter.Add(table);
                    }
                    break;
                }
            }
        }
        #endregion ExportHyperCube

        #region ExportUnassignedAccounts
        private void ExportUnassignedAccounts() {
            pdf.AddHeadline(eBalanceKitResources.Localisation.ResourcesPdfExport.UnassignedAccounts);

            foreach (var balanceList in Config.Document.BalanceLists) {
                pdf.AddSubHeadline(balanceList.DisplayString);
                PdfPTable addTable = new PdfPTable(new[] {70f, 30f}) {SpacingBefore = 5f, WidthPercentage = 100f};
                foreach (var unassignedItem in balanceList.UnassignedItems) {
                    AddDefaultCell(addTable, unassignedItem.Label);
                    AddValueCell(addTable, unassignedItem.ValueDisplayString);
                }
                pdf.CurrentSection.Add(addTable);
            }
        }
        #endregion ExportUnassignedAccounts

        #region IsExports
        /// <summary>
        /// Shows the specific balance list element is exported or not. If we don't add parameter returns the Config.ExportAccounts
        /// Most likely the current value node's DbValue.SendAccountBalances will be the parameter.
        /// </summary>
        /// <param name="sendAccountBalances">the current node's SendAccountBalances value</param>
        /// <returns></returns>
        private bool IsExportAccounts(bool sendAccountBalances = true) { return (Config.ExportAsXbrl && sendAccountBalances) || Config.ExportAccounts; }

        private bool IsExportComments() { return !Config.ExportAsXbrl && Config.ExportComments; }

        private bool IsExportReconciliationInfo() { return !Config.ExportAsXbrl && Config.ExportReconciliationInfo; }

        private bool IsExportMandatoryOnly() { return !Config.ExportAsXbrl && Config.ExportMandatoryOnly; }

        private bool IsExportNilValues(PdfTreeView selfNode) { return Config.ExportAsXbrl ? selfNode.IsMandatoryField : Config.ExportNILValues; }
        #endregion

        #region DefaultTable
        /// <summary>
        /// Make a default table with the given column number
        /// </summary>
        /// <param name="columns">how many columns</param>
        /// <returns>empty default table</returns>
        private static PdfPTable DefaultTable(int columns) {
            PdfPTable pdfPTable = new PdfPTable(columns) {HeaderRows = 0, WidthPercentage = 100f};
            pdfPTable.DefaultCell.Border = 0;
            return pdfPTable;
        }
        #endregion

        #region ExportDefault
        /// <summary>
        /// True if the valueType's children should be checked.
        /// </summary>
        /// <param name="valueType">input</param>
        /// <returns>output</returns>
        private static bool CheckChildren(XbrlElementValueTypes valueType) {
            return valueType != XbrlElementValueTypes.Tuple &&
                   // single choice can have child. TODO: implement to gather information from the optional value.
                   valueType != XbrlElementValueTypes.MultipleChoice && valueType != XbrlElementValueTypes.SingleChoice &&
                   valueType != XbrlElementValueTypes.None;
        }

        /// <summary>
        /// Export the element with the given uri ending.
        /// </summary>
        /// <param name="uriShort">the uri ending</param>
        private void ExportDefault(string uriShort) {
            // specify which node should be built. For more general the "notes" can be changed to a parameter
            string uri = ExportHelper.GetUri(uriShort, Config.Document);
            IPresentationTreeNode root = Config.Document.GaapPresentationTrees[uri].RootEntries.First();
            // result tree
            PdfTreeView treeView = new PdfTreeView();
            BuildTreeViewDefault(treeView, Config.Document.ValueTreeMain.Root, root);

            // nothing in notes
            if (treeView.Children.Count == 0) {
                return;
            }

            // the result tree have a head element which is not important. Just cut it.
            PdfTreeView firstNode = treeView.Children.First();
            firstNode.Parent = null;

            #region testExpand
            // TODO: add these as test
            //foreach (var child in firstNode.AsDepthFirstEnumerable(child => child.Children)) {
            //    Debug.Assert(ExportNilValues() || child.ExportElement);
            //}
            //foreach (var VARIABLE in CheckRootForMandatory(root)) {
            //    test if VARIABLE is in the result.
            //}
            //private IEnumerable CheckRootForMandatory(IPresentationTreeNode root) {
            //    if (root == null) {
            //        yield break;
            //    }
            //    if (root.Element.IsMandatoryField)
            //        yield return root;
            //    foreach (var child in root.Children) {
            //        foreach (var childValue in CheckRootForMandatory(child as IPresentationTreeNode)) {
            //            yield return childValue;
            //        }
            //    }
            //}
            #endregion

            if (!IsExportNilValues(treeView) &&
                (!treeView.ExportElement && (!Config.ExportAsXbrl || !treeView.HasMandatoryChild))) {
                return;
            }

            // exporting part
            IRoleType role = Config.Document.MainTaxonomy.GetRole(uri);
            pdf.AddHeadline(role.Name +
                            (IsExportAccounts()
                                 ? eBalanceKitResources.Localisation.ResourcesPdfExport.WithBalances
                                 : string.Empty));
            ExportTreeViewDefault(firstNode);
        }

        private PdfPTable ExportTreeViewByType(PdfTreeView child, bool haveHeadLine) {
            PdfPTable addedTable = DefaultTable(1);

            if (child.IsAccount) {
                addedTable = DefaultTable(2);
                AddDefaultCell(addedTable, child.Header);
                //addedTable.AddCell(child.Header);
                AddValueCell(addedTable, child.Value);
                //addedTable.AddCell(child.Value);
                return addedTable;
            }

            if (child.Element.IsMonetaryTree) {
                PdfPTable newTable = IsExportReconciliationInfo()
                                         ? AddTreeViewTableUeberleitung(false)
                                         : AddTreeViewTable(false);
                ExportTreeView(child, newTable, IsExportReconciliationInfo(), false, false);
                if (child.HasComputedValue) {
                    AddValueCell(addedTable, child.Value, bold: true);
                } else {
                    AddValueCell(addedTable, child.Value);
                }
                addedTable.AddCell(newTable);
                return addedTable;
            }

            if (!child.HasValue && !IsExportNilValues(child)) {
                return addedTable;
            }

            switch (child.Element.ValueType) {
                case XbrlElementValueTypes.HyperCubeContainerItem:

                    #region hyperCubeExportHelpingPart
                    // TODO: export of hyper cube is ignored right now
                    //ExportHyperCube(child.Element.Id);
                    /*string elementId = child.Element.Id;
                    addedTable = DefaultNotesTable(1);
                    var hyperCubeElement = from value in Config.Document.GaapPresentationTrees.Values
                                           where value.HasNode(elementId)
                                           select
                                               value.GetNode(elementId) as
                                               Interfaces.PresentationTree.IPresentationTreeNode;
                    foreach (
                        Interfaces.PresentationTree.IPresentationTreeNode presentationTreeNode in hyperCubeElement) {
                        addedTable.AddCell(new HyperCubeExporter(Config).GeneratePdfContentLimited(presentationTreeNode));
                    }
                    break;*/
                    #endregion hyperCubeExportHelpingPart

                    return addedTable;
                    /*case XbrlElementValueTypes.Monetary:
                    addedTable = DefaultNotesTable(2);
                    addedTable.AddCell(string.Empty);
                    addedTable.AddCell(child.Value);
                    if (ExportComments() && !string.IsNullOrEmpty(child.Comment)) {
                        AddDefaultCell(addedTable, eBalanceKitResources.Localisation.ResourcesPdfExport.CommentLabel + child.Comment,
                                       null, true);
                        addedTable.AddCell(string.Empty);
                    }
                    break;*/
                case XbrlElementValueTypes.Tuple:
                    // tuple's handled other way, than usual. First we add a comment, if needed,
                    // and set that every element of tuple should be bordered.
                    addedTable = DefaultTable(1);
                    if (IsExportComments() && !string.IsNullOrEmpty(child.Comment)) {
                        AddDefaultCell(addedTable,
                                       eBalanceKitResources.Localisation.ResourcesPdfExport.CommentLabel + child.Comment,
                                       italic: true);
                    }
                    addedTable.DefaultCell.BorderWidthTop = 1f;
                    addedTable.DefaultCell.BorderWidthLeft = 1f;
                    addedTable.DefaultCell.BorderWidthBottom = 1f;
                    addedTable.DefaultCell.BorderWidthRight = 1f;
                    addedTable.SpacingBefore = 5f;
                    // than we export every element, each in other box.
                    foreach (PdfTreeView tupleListEntry in child.Children) {
                        PdfPTable innerTable = DefaultTable(1);
                        // proceed with the elements in the tuple element.
                        ExportTreeViewDefault(tupleListEntry, parent: innerTable);
                        addedTable.AddCell(innerTable);
                    }
                    break;
                default:
                    // default data, that can be shown splits the actual depth in half in the output, and first half is the label of the element,
                    // and the second half is the value.
                    addedTable = DefaultTable(2);
                    addedTable.AddCell(haveHeadLine ? string.Empty : child.Header);
                    if (child.HasComputedValue) {
                        AddValueCell(addedTable, child.Value, bold: true);
                    } else {
                        AddValueCell(addedTable, child.Value);
                    }
                    if (IsExportComments() && !string.IsNullOrEmpty(child.Comment)) {
                        AddDefaultCell(addedTable,
                                       eBalanceKitResources.Localisation.ResourcesPdfExport.CommentLabel + child.Comment,
                                       null, true);
                        addedTable.AddCell(string.Empty);
                    }
                    break;
            }
            return addedTable;
        }

        /// <summary>
        /// export the given tree view in to the current pdfGenerator. Make head lines and fill the values.
        /// </summary>
        /// <param name="treeView">input tree view</param>
        /// <param name="level">the current level we are in</param>
        /// <param name="parent">when we are inside a tuple or some datastructure, that have more elements,
        ///  the export will put the values into that table. Default null means we add the table to the current pdfGenerator</param>
        private void ExportTreeViewDefault(PdfTreeView treeView, int level = 1, PdfPTable parent = null) {
            foreach (PdfTreeView child in treeView.Children) {
                // if we are in an element of tuple or some other datastructure, don't add header line.
                if (parent == null && !child.IsAccount) {
                    switch (level) {
                        case 1:
                            pdf.AddSubHeadline(child.Header);
                            break;
                        case 2:
                            pdf.AddSubSubHeadline(child.Header);
                            break;
                        default:
                            //pdf.AddSubSubHeadline(child.Header);
                            break;
                    }
                }

                PdfPTable addedTable = ExportTreeViewByType(child, parent == null && level < 3);

                // add the value of current level to the proper parent.
                if (parent != null) {
                    parent.AddCell(addedTable);

                    #region commentUnused
                    /*
                bool insideContainer = false;
                PdfTreeView actualParent = child.Parent.Parent;
                while (actualParent != null) {

                    if (actualParent.Element != null && CheckChildren(actualParent.Element.ValueType)) {
                        actualParent = actualParent.Parent;
                        continue;
                    }

                    Debug.Assert(actualParent.Element != null || !CheckChildren(actualParent.Parent.Element.ValueType));

                    insideContainer = true;
                    break;
                }

                if (insideContainer) {
                    break;
                }
                if (child.Parent == null) {
                    pdf.CurrentSection.Add(addedTable);
                } else {
                    pdf.CurrentSubSection.Add(addedTable);
                }*/
                    #endregion commentUnused
                } else {
                    if (treeView.Parent == null) {
                        pdf.CurrentSection.Add(addedTable);
                    } else {
                        pdf.CurrentSubSection.Add(addedTable);
                    }
                }

                // leave the special datastructures. They are proceeded already.
                if (!child.IsAccount && !child.Element.IsMonetaryTree && CheckChildren(child.Element.ValueType))
                    ExportTreeViewDefault(child, level + 1, parent);
            }
        }

        /// <summary>
        /// prepare a PdfTreeView node for the BuildTreeViewNotes. The node will contain proper values about if it have value, and if it have,
        /// than add Element, HasValue, Header, IsAccount, IsMandatoryField, HasComputedValue, ExportAccount(if it is a monetary), Comment, ReconciliationInfo to it.
        /// </summary>
        /// <param name="valueTreeRoot">the container of current available values of the depth</param>
        /// <param name="root">presentation tree node. The current node</param>
        /// <returns>The setted current node with values</returns>
        private PdfTreeView BuildSelfNode(ValueTreeNode valueTreeRoot, IPresentationTreeEntry root) {
            PdfTreeView selfNode = new PdfTreeView();

            Debug.Assert(root != null && root.Element != null, Localisation.ExceptionMessages.ShouldHaveElement);

            selfNode.HasValue = false;
            selfNode.Element = root.Element;
            selfNode.Header = root.Element.MandatoryLabel;
            // true if balance list entry. IsAccount will be set to true in BuildTreeViewNotes.
            selfNode.IsAccount = false;
            selfNode.IsMandatoryField = root.Element.IsMandatoryField;
            // changed in Monetary
            selfNode.HasComputedValue = false;
            selfNode.ExportAccount = false;

            if (!valueTreeRoot.Values.ContainsKey(root.Element.Id)) {
                Debug.Assert(root.Element.ValueType == XbrlElementValueTypes.Abstract);
                return selfNode;
            }

            IValueTreeEntry value = valueTreeRoot.Values[root.Element.Id];
            XbrlElementValueBase valueBase = value as XbrlElementValueBase;
            selfNode.Comment = valueBase != null ? valueBase.Comment : string.Empty;
            // default value. May change in process
            string temp = value.DisplayString;
            SetNullIfXbrl(ref temp);
            selfNode.Value = temp;

            if (IsExportComments() && !string.IsNullOrEmpty(selfNode.Comment)) {
                selfNode.HasValue = true;
            }

            switch (root.Element.ValueType) {
                case XbrlElementValueTypes.MultipleChoice:
                    throw new NotImplementedException();
                case XbrlElementValueTypes.Monetary:
                    Debug.Assert(!value.HasValue || value.MonetaryValue != null);
                    selfNode.HasComputedValue = value.MonetaryValue != null && value.MonetaryValue.HasComputedValue;
                    selfNode.ExportAccount = value.DbValue.SendAccountBalances;
                    if (!root.Element.IsReconciliationPosition) {
                        selfNode.HasValue = value.HasValue;
                        break;
                    }
                    selfNode.ReconciliationInfo = value.ReconciliationInfo;
                    Debug.Assert(selfNode.ReconciliationInfo != null);
                    selfNode.HasValue = value.HasValue || (selfNode.ReconciliationInfo.HasTransferValue &&
                                                           IsExportReconciliationInfo());
                    break;
                case XbrlElementValueTypes.SingleChoice:

                    XbrlElementValue_SingleChoice singleChoiceValue = value as XbrlElementValue_SingleChoice;

                    Debug.Assert(singleChoiceValue != null && string.IsNullOrEmpty(selfNode.Comment));

                    if (singleChoiceValue.SelectedValue == null)
                        break;

                    selfNode.Value = singleChoiceValue.SelectedValue.Label;
                    selfNode.HasValue = true;
                    break;

                case XbrlElementValueTypes.String:
                    if (value.Value != null && !string.IsNullOrEmpty(value.Value.ToString())) {
                        selfNode.Value = value.Value.ToString();
                        selfNode.HasValue = true;
                    }
                    break;
                    // if it's a tuple container element.
                case XbrlElementValueTypes.Tuple:

                    XbrlElementValue_Tuple tupleValue = value as XbrlElementValue_Tuple;
                    IPresentationTreeNode pchild = root as IPresentationTreeNode;

                    Debug.Assert(tupleValue != null && pchild != null);

                    // take all the children, that are added.
                    foreach (ValueTreeNode valueTreeNode in tupleValue.Items) {
                        // tupleChild = tuple element.
                        PdfTreeView tupleChild = new PdfTreeView {HasValue = false};
                        foreach (IPresentationTreeNode presentationTreeEntry in pchild) {
                            // if any element in the tuple have value, than this tuple element have value. The tuple element's all child will be added to the actual tuple element.
                            tupleChild.HasValue |= BuildTreeViewDefault(tupleChild, valueTreeNode, presentationTreeEntry);
                        }
                        // it that tuple element don't have a value, just ignore it.
                        if (!tupleChild.HasValue)
                            continue;
                        // add the tuple element as a child to the node itself.
                        tupleChild.Parent = selfNode;
                        // that tuple element had value, so the tuple have value.
                        selfNode.HasValue = true;
                    }
                    break;
                case XbrlElementValueTypes.HyperCubeContainerItem:
                    // export will take care of it
                    // TODO: how to check a hyperCube if it have a value?
                    break;
                default:
                    Debug.Assert(root.Element.ValueType != XbrlElementValueTypes.Abstract);
                    // have simple value. String or number. Monetary, Date, Int ... goes there
                    if ((value.IsNumeric && value.DecimalValue != 0) ||
                        (!value.IsNumeric && !string.IsNullOrEmpty(value.ToString()))) {
                        selfNode.HasValue = value.HasValue;
                    } else {
                        // no value set. If something new type implemented, than this assert will alert the programers, that type is special enough,
                        // to not export automatically.
                        Debug.Assert(!value.HasValue, Localisation.ExceptionMessages.ShouldntHaveValue);
                    }
                    break;
            }
            return selfNode;
        }

        private IEnumerable<IPresentationTreeEntry> GetRightChildrensForConfig(IPresentationTreeNode root,
                                                                               ValueTreeNode valueTreeRoot) {
            IEnumerable<IPresentationTreeEntry> childrensWithElement;
            if (IsExportMandatoryOnly()) {
                if (IsExportAccounts()) {
                    childrensWithElement = from child in root.Children
                                           where
                                               child is BalanceListEntryBase ||
                                               ((child.Element != null) && (child.Element.IsMandatoryField) &&
                                                valueTreeRoot.Values.ContainsKey(child.Element.Id))
                                           select child;
                } else {
                    childrensWithElement = from child in root.Children
                                           where
                                               child.Element != null && child.Element.IsMandatoryField &&
                                               valueTreeRoot.Values.ContainsKey(child.Element.Id)
                                           select child;
                }
            } else {
                if (IsExportAccounts()) {
                    childrensWithElement = from child in root.Children
                                           where
                                               child is BalanceListEntryBase || ((child.Element != null) &&
                                                                                 valueTreeRoot.Values.ContainsKey(
                                                                                     child.Element.Id))
                                           select child;
                } else {
                    childrensWithElement = from child in root.Children
                                           where
                                               child.Element != null &&
                                               valueTreeRoot.Values.ContainsKey(child.Element.Id)
                                           select child;
                }
            }
            return childrensWithElement;
        }

        /// <summary>
        /// make a tree view from the given value tree, and presentation tree.
        /// </summary>
        /// <param name="treeView">the output tree view</param>
        /// <param name="valueTreeRoot">the value tree, that contains all the values in the current level</param>
        /// <param name="root">the current node of the presentation tree</param>
        /// <returns>the return value is true, if the tree view is expanded. The checked branch of parameter root had a value somewhere inside</returns>
        private bool BuildTreeViewDefault(PdfTreeView treeView, ValueTreeNode valueTreeRoot, IPresentationTreeNode root) {
            PdfTreeView selfNode = BuildSelfNode(valueTreeRoot, root);

            if (selfNode.Element.IsMonetaryTree) {
                BuildTreeView(selfNode, valueTreeRoot.ValueTree, root);
                selfNode.Parent = treeView;
                return IsExportNilValues(selfNode) || selfNode.ExportElement ||
                       (Config.ExportAsXbrl && selfNode.HasMandatoryChild);
            }

            bool ret = selfNode.HasValue;

            // these element types' child are not checked
            if (!(root is BalanceListEntryBase) && CheckChildren(root.Element.ValueType)) {
                // enumerate on only childs that have element, and have value in value tree
                IEnumerable<IPresentationTreeEntry> childrensWithElement = GetRightChildrensForConfig(root,
                                                                                                      valueTreeRoot);

                foreach (IPresentationTreeEntry child in childrensWithElement) {
                    if (IsExportAccounts() && child is BalanceListEntryBase) {
                        Debug.Assert(selfNode.HasValue, Localisation.ExceptionMessages.MustHaveValue);

                        ret = true;

                        BalanceListEntryBase balanceListEntryBaseChild = child as BalanceListEntryBase;

                        if (!selfNode.ExportAccount && !IsExportAccounts()) {
                            continue;
                        }

                        PdfTreeView balanceListNode = new PdfTreeView {
                            IsAccount = true,
                            IsMandatoryField = false,
                            ExportAccount = true,
                            Header = balanceListEntryBaseChild.Label,
                            Value = balanceListEntryBaseChild.ValueDisplayString,
                            HasValue = true,
                            Comment =
                                string.IsNullOrEmpty(balanceListEntryBaseChild.Comment)
                                    ? string.Empty
                                    : balanceListEntryBaseChild.Comment,
                        };
                        balanceListNode.Parent = selfNode;

                        continue;
                    }

                    IPresentationTreeNode pchild = child as IPresentationTreeNode;

                    Debug.Assert(pchild != null);

                    // add every children's value to the selfNode by a recursive call. From now on the selfNode is a parent, if returns true.
                    if (!BuildTreeViewDefault(selfNode, valueTreeRoot, pchild))
                        continue;

                    ret = true;
                }
            }

            // ret is true if any child or self have value. If it is false than just skip it.
            if (!ret && !IsExportNilValues(selfNode)) {
                return false;
            }

            // self node is builded add it to the output variable treeView.
            selfNode.Parent = treeView;
            return true;
        }
        #endregion ExportDefault

        #region ExportPart
        private void ExportPart(string uri, bool GuV = false) {
            PdfPTable table = IsExportReconciliationInfo()
                                  ? AddTreeViewTableUeberleitung(GuV)
                                  : AddTreeViewTable(false);

            var root = Config.Document.GaapPresentationTrees[uri].RootEntries.First();

            PdfTreeView tv = new PdfTreeView();
            BuildTreeView(tv, Config.Document.ValueTreeMain, root);

            if (tv.Children.Count == 0) {
                return;
            }

            if (!IsExportNilValues(tv) && (!tv.ExportElement && (!Config.ExportAsXbrl || !tv.HasMandatoryChild))) {
                return;
            }

            ExportTreeView(tv, table, IsExportReconciliationInfo(), false, GuV);
            var role = Config.Document.MainTaxonomy.GetRole(uri);
            pdf.AddHeadline(role.Name +
                            (IsExportAccounts()
                                 ? eBalanceKitResources.Localisation.ResourcesPdfExport.WithBalances
                                 : string.Empty));
            pdf.CurrentChapter.Add(table);
        }
        #endregion ExportPart

        #region ExportIncomeStatements
        private void ExportIncomeStatements() {
            switch (Config.Document.MainTaxonomyInfo.Type) {
                case Taxonomy.Enums.TaxonomyType.GAAP:
                    //ExportIncomeStatement("http://www.xbrl.de/taxonomies/de-gaap-ci/role/incomeStatement");
                    ExportPart("http://www.xbrl.de/taxonomies/de-gaap-ci/role/incomeStatement", true);
                    break;
                case Taxonomy.Enums.TaxonomyType.OtherBusinessClass:
                    ExportPart("http://www.xbrl.de/taxonomies/de-gaap-ci/role/incomeStatement", true);
                    break;
                case Taxonomy.Enums.TaxonomyType.Financial:
                    ExportPart("http://www.xbrl.de/taxonomies/de-fi/role/incomeStatementStf", true);
                    break;
                case Taxonomy.Enums.TaxonomyType.Insurance:
                    ExportPart("http://www.xbrl.de/taxonomies/de-ins/role/incomeStatement", true);
                    break;
            }
        }
        #endregion ExportIncomeStatements

        #region ExportBalanceSheets
        private void ExportBalanceSheets() {
            switch (Config.Document.MainTaxonomyInfo.Type) {
                case Taxonomy.Enums.TaxonomyType.GAAP:
                    //ExportBalanceSheet("http://www.xbrl.de/taxonomies/de-gaap-ci/role/balanceSheet");
                    ExportPart("http://www.xbrl.de/taxonomies/de-gaap-ci/role/balanceSheet");
                    break;
                case Taxonomy.Enums.TaxonomyType.OtherBusinessClass:
                    ExportPart("http://www.xbrl.de/taxonomies/de-gaap-ci/role/balanceSheet");
                    break;
                case Taxonomy.Enums.TaxonomyType.Financial:
                    ExportPart("http://www.xbrl.de/taxonomies/de-fi/role/balanceSheet");
                    break;
                case Taxonomy.Enums.TaxonomyType.Insurance:
                    ExportPart("http://www.xbrl.de/taxonomies/de-ins/role/balanceSheet");
                    break;
                default:
                    break;
            }
        }
        #endregion ExportBalanceSheets

        #region ExportCompanyInformation
        private void ExportCompanyInformation() {
            pdf.AddHeadline("Unternehmensinformation");
            {
                pdf.AddSubHeadline("Allgemein");
                PdfPTable companyTable = CreateCompanyInfoTable();
                //Company name
                AddTaxonomyTableCell(companyTable, "de-gcd_genInfo.company.id.name", false);

                //Former company name (if available)
                if (AddTaxonomyTableCell(companyTable, "de-gcd_genInfo.company.id.name.formerName", true))
                    AddTaxonomyTableCell(companyTable, "de-gcd_genInfo.company.id.name.dateOfLastChange", true);

                //Address
                string address =
                    Config.Document.ValueTreeGcd.Root.Values["de-gcd_genInfo.company.id.location.street"].DisplayString +
                    " "
                    +
                    Config.Document.ValueTreeGcd.Root.Values["de-gcd_genInfo.company.id.location.houseNo"].DisplayString +
                    "\n"
                    +
                    Config.Document.ValueTreeGcd.Root.Values["de-gcd_genInfo.company.id.location.zipCode"].DisplayString +
                    " "
                    + Config.Document.ValueTreeGcd.Root.Values["de-gcd_genInfo.company.id.location.city"].DisplayString +
                    "\n"
                    +
                    Config.Document.ValueTreeGcd.Root.Values["de-gcd_genInfo.company.id.location.country"].DisplayString;
                companyTable.AddCell("Adresse");
                companyTable.AddCell(address);

                AddTaxonomyTableCell(companyTable, "de-gcd_genInfo.company.id.location", true);
                //Legalform
                AddTaxonomyTableCell(companyTable, "de-gcd_genInfo.company.id.legalStatus", true);

                //Former legalform (if available)
                if (AddTaxonomyTableCell(companyTable, "de-gcd_genInfo.company.id.legalStatus.formerStatus", true)) {
                    AddTaxonomyTableCell(companyTable, "de-gcd_genInfo.company.id.legalStatus.dateOfLastChange", true);
                }

                pdf.CurrentChapter.Add(companyTable);
            }

            {
                pdf.AddSubHeadline("Kennnummern");
                XbrlElementValue_Tuple tuple =
                    (XbrlElementValue_Tuple) Config.Document.ValueTreeGcd.Root.Values["de-gcd_genInfo.company.id.idNo"];
                pdf.CurrentChapter.Add(CreateTableFromTuple(tuple));
            }

            {
                pdf.AddSubHeadline("Gesellschafter");
                XbrlElementValue_Tuple tuple =
                    (XbrlElementValue_Tuple)
                    Config.Document.ValueTreeGcd.Root.Values["de-gcd_genInfo.company.id.shareholder"];
                pdf.CurrentChapter.Add(CreateTableFromTuple(tuple));
            }

            {
                pdf.AddSubHeadline("Registereintrag");
                if (Config.Document.ValueTreeGcd.Root.Values["de-gcd_genInfo.company.id.Incorporation.Type"].HasValue) {
                    string[] ids = new string[]
                                   {"Type", "prefix", "section", "number", "suffix", "court", "dateOfFirstRegistration"};
                    pdf.CurrentChapter.Add(CreateTableFromIds("de-gcd_genInfo.company.id.Incorporation.", ids));
                }
            }

            {
                pdf.AddSubHeadline("Börsennotierung");
                XbrlElementValue_Tuple tuple =
                    (XbrlElementValue_Tuple)
                    Config.Document.ValueTreeGcd.Root.Values["de-gcd_genInfo.company.id.stockExch"];
                pdf.CurrentChapter.Add(CreateTableFromTuple(tuple));
            }

            {
                pdf.AddSubHeadline("Kontaktperson");
                XbrlElementValue_Tuple tuple =
                    (XbrlElementValue_Tuple)
                    Config.Document.ValueTreeGcd.Root.Values["de-gcd_genInfo.company.id.contactAddress"];
                pdf.CurrentChapter.Add(CreateTableFromTuple(tuple));
            }
            //Add all other information
            {
                pdf.AddSubHeadline("Sonstige Informationen");
                string[] ids = new string[] {
                    "de-gcd_genInfo.company.id.lastTaxAudit", "de-gcd_genInfo.company.id.sizeClass",
                    "de-gcd_genInfo.company.id.business", "de-gcd_genInfo.company.id.CompanyStatus",
                    "de-gcd_genInfo.company.id.FoundationDate", "de-gcd_genInfo.company.id.internet",
                    "de-gcd_genInfo.company.id.internet.description", "de-gcd_genInfo.company.id.internet.url",
                    "de-gcd_genInfo.company.id.comingfrom", "de-gcd_genInfo.company.id.companyLogo",
                    "de-gcd_genInfo.company.userSpecific"
                };
                PdfPTable otherTable = CreateTableFromIds(string.Empty, ids);
                XbrlElementValue_Tuple tuple =
                    (XbrlElementValue_Tuple)
                    Config.Document.ValueTreeGcd.Root.Values["de-gcd_genInfo.company.id.industry"];
                foreach (var item in tuple.Items[0].Values) {
                    if (!item.Value.HasValue) continue;
                    otherTable.AddCell(item.Value.ToString());
                    otherTable.AddCell(item.Value.DisplayString);
                }

                pdf.CurrentChapter.Add(otherTable);
            }
        }
        #endregion ExportCompanyInformation

        #region ExportReportInformation
        private void ExportReportInformation() {
            pdf.AddHeadline("Informationen zum Bericht");

            {
                pdf.AddSubHeadline("Identifikationsmerkmale des Berichts");

                string[] ids = {
                    "de-gcd_genInfo.report.id.reportType", "de-gcd_genInfo.report.id.reportStatus",
                    "de-gcd_genInfo.report.id.revisionStatus", "de-gcd_genInfo.report.id.reportElement.allocation.ntAss"
                    ,
                    "de-gcd_genInfo.report.id.reportElement.allocation.incomeUse",
                    "de-gcd_genInfo.report.id.reportElement.allocation.payablesReceivablesAgeingReport",
                    "de-gcd_genInfo.report.id.statementType", "de-gcd_genInfo.report.id.statementType.tax",
                    "de-gcd_genInfo.report.id.accountingStandard",
                    "de-gcd_genInfo.report.id.specialAccountingStandard",
                    "de-gcd_genInfo.report.id.incomeStatementFormat", "de-gcd_genInfo.report.id.consolidationRange",
                    "de-gcd_genInfo.report.id.consideredInConsolidatedStatement"
                };
                PdfPTable reportTable = CreateTableFromIds(string.Empty, ids);

                //Two normal booleans
                AddTaxonomyTableCell(reportTable, "de-gcd_genInfo.report.id.statementType.restated", true);
                AddTaxonomyTableCell(reportTable, "de-gcd_genInfo.report.id.incomeStatementendswithBalProfit", true);

                //List of MultipleChoice elements
                XbrlElementValue_MultipleChoice reportElements =
                    Config.Document.ValueTreeGcd.Root.Values["de-gcd_genInfo.report.id.reportElement"] as
                    XbrlElementValue_MultipleChoice;
                for (int i = 0; i < reportElements.Elements.Count; ++i) {
                    bool? isChecked = reportElements.IsChecked[reportElements.Elements[i].Id].BoolValue;

                    if (isChecked.HasValue) {
                        reportTable.AddCell(reportElements.Elements[i].MandatoryLabel);
                        reportTable.AddCell(isChecked.Value
                                                ? eBalanceKitResources.Localisation.ResourcesCommon.Yes
                                                : eBalanceKitResources.Localisation.ResourcesCommon.No);
                    }
                }
                pdf.CurrentChapter.Add(reportTable);
            }
            {
                pdf.AddSubHeadline("Dokument gehört zu");

                XbrlElementValue_Tuple tuple =
                    (XbrlElementValue_Tuple)
                    Config.Document.ValueTreeGcd.Root.Values["de-gcd_genInfo.report.id.accordingTo"];
                PdfPTable table = CreateTableFromTuple(tuple);
                pdf.CurrentChapter.Add(table);
            }
            {
                pdf.AddSubHeadline("Berichtsperiode");
                string[] ids = {
                    "de-gcd_genInfo.report.period.reportPeriodBegin", "de-gcd_genInfo.report.period.reportPeriodEnd",
                    "de-gcd_genInfo.report.period.fiscalYearBegin", "de-gcd_genInfo.report.period.fiscalYearEnd",
                    "de-gcd_genInfo.report.period.balSheetClosingDate",
                    "de-gcd_genInfo.report.period.fiscalPreviousYearBegin",
                    "de-gcd_genInfo.report.period.fiscalPreviousYearEnd",
                    "de-gcd_genInfo.report.period.balSheetClosingDatePreviousYear"
                };

                PdfPTable reportTable = CreateTableFromIds(string.Empty, ids);
                pdf.CurrentChapter.Add(reportTable);
            }
        }
        #endregion ExportReportInformation

        #region ExportDocumentInformation
        private void ExportDocumentInformation() {
            pdf.AddHeadline("Dokumentinformationen");
            {
                pdf.AddSubHeadline("Identifikationsmerkmale des Dokuments");

                string[] ids = {
                    "de-gcd_genInfo.doc.id.generationDate",
                    "de-gcd_genInfo.doc.id.generationReason",
                    "de-gcd_genInfo.doc.id.contents",
                    "de-gcd_genInfo.doc.id.origLanguage",
                    "de-gcd_genInfo.doc.id.disclosable"
                };
                PdfPTable documentTable = CreateTableFromIds(string.Empty, ids);
                pdf.CurrentChapter.Add(documentTable);
            }
            {
                pdf.AddSubHeadline("Dokumentersteller");
                XbrlElementValue_Tuple tuple =
                    (XbrlElementValue_Tuple) Config.Document.ValueTreeGcd.Root.Values["de-gcd_genInfo.doc.author"];
                pdf.CurrentChapter.Add(CreateTableFromTuple(tuple));
            }
            {
                pdf.AddSubHeadline("Dokumentrevisionen");
                string[] ids = {
                    "de-gcd_genInfo.doc.rev.date",
                    "de-gcd_genInfo.doc.rev.versionNo",
                    "de-gcd_genInfo.doc.rev.dateOfChange",
                    "de-gcd_genInfo.doc.rev.changeInitiator"
                };
                PdfPTable documentTable = CreateTableFromIds(string.Empty, ids);
                pdf.CurrentChapter.Add(documentTable);
            }
        }
        #endregion ExportDocumentInformation

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
        private bool AddTaxonomyTableCell(PdfPTable table, string id, bool checkExistance = false) {
            if (checkExistance && !Config.Document.ValueTreeGcd.Root.Values[id].HasValue)
                return false;
            XbrlElementValueTypes valueType = Config.Document.ValueTreeGcd.Root.Values[id].Element.ValueType;
            IValueTreeEntry value = Config.Document.ValueTreeGcd.Root.Values[id];
            table.AddCell(Config.Document.GcdTaxonomy.Elements[id].MandatoryLabel);

            if (valueType == Taxonomy.Enums.XbrlElementValueTypes.Boolean
                && value.HasValue) {
                XbrlElementValue_Boolean element =
                    Config.Document.ValueTreeGcd.Root.Values[id] as XbrlElementValue_Boolean;
                table.AddCell((bool) element.Value
                                  ? eBalanceKitResources.Localisation.ResourcesCommon.Yes
                                  : eBalanceKitResources.Localisation.ResourcesCommon.No);
            } else if (valueType == XbrlElementValueTypes.Date && value.HasValue) {
                XbrlElementValue_Date element =
                    value as XbrlElementValue_Date;
                table.AddCell(element.DateValue.Value.ToString(LocalisationUtils.GermanCulture.DateTimeFormat.ShortDatePattern));
            } else 
                table.AddCell(value.DisplayString);
            return true;
        }
        #endregion

        #region CreateCompanyInfoTable
        /// <summary>
        /// Creates a new table with some nice properties
        /// </summary>
        /// <returns>new table</returns>
        private PdfPTable CreateCompanyInfoTable() {
            PdfPTable resultTable = new PdfPTable(new float[] {1.7f, 2f});
            resultTable.DefaultCell.Border = PdfPCell.NO_BORDER;
            resultTable.SpacingBefore = 5f;
            resultTable.SpacingAfter = 5f;
            resultTable.WidthPercentage = 95f;
            //resultTable.HorizontalAlignment = Element.ALIGN_LEFT;

            return resultTable;
        }
        #endregion CreateCompanyInfoTable

        #region CreateTableFromTuple
        /// <summary>
        /// Creates a table from a XbrlElementValue_Tuple by iterating over all items and all values
        /// </summary>
        /// <param name="tuple"></param>
        /// <returns></returns>
        private PdfPTable CreateTableFromTuple(XbrlElementValue_Tuple tuple) {
            PdfPTable resultTable = CreateCompanyInfoTable();
            AddTupleToTable(tuple, resultTable);
            return resultTable;
        }
        #endregion CreateTableFromTuple

        #region AddTupleToTable
        private void AddTupleToTable(XbrlElementValue_Tuple tuple, PdfPTable resultTable) {
            foreach (var person in tuple.Items) {
                foreach (var item in person.Values) {
                    //Check if ExportMandatoryOnly -- sev
                    if (IsExportMandatoryOnly() && !item.Value.Element.IsMandatoryField) {
                        continue;
                    }

                    if (item.Value.Element.ValueType == Taxonomy.Enums.XbrlElementValueTypes.Tuple) {
                        XbrlElementValue_Tuple localTuple = (XbrlElementValue_Tuple) item.Value;
                        if (item.Key == "de-gcd_genInfo.company.id.shareholder.ShareDivideKey") {
                            resultTable.AddCell(item.Value.ToString());
                            resultTable.AddCell(
                                localTuple.Items[0].Values[
                                    "de-gcd_genInfo.company.id.shareholder.ShareDivideKey.numerator"].DisplayString +
                                " / " +
                                localTuple.Items[0].Values[
                                    "de-gcd_genInfo.company.id.shareholder.ShareDivideKey.denominator"].DisplayString);
                        } else AddTupleToTable(localTuple, resultTable);
                        //System.Diagnostics.Debug.WriteLine(item.);
                    } else if (item.Value.Element.ValueType == Taxonomy.Enums.XbrlElementValueTypes.Boolean
                               && item.Value.HasValue) {
                        XbrlElementValue_Boolean flag = item.Value as XbrlElementValue_Boolean;
                        resultTable.AddCell(item.Value.ToString());
                        resultTable.AddCell((bool) flag.Value
                                                ? eBalanceKitResources.Localisation.ResourcesCommon.Yes
                                                : eBalanceKitResources.Localisation.ResourcesCommon.No);
                    } else if (item.Value.HasValue) {
                        resultTable.AddCell(item.Value.ToString());
                        resultTable.AddCell(item.Value.DisplayString);
                    }
                }
                if (person != tuple.Items[tuple.Items.Count - 1]) {
                    PdfPCell horizontalArrow = new PdfPCell();
                    horizontalArrow.Border = PdfPCell.BOTTOM_BORDER;
                    resultTable.AddCell(horizontalArrow);
                    resultTable.AddCell(horizontalArrow);
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
        private PdfPTable CreateTableFromIds(string prefix, string[] ids) {
            PdfPTable resultTable = CreateCompanyInfoTable();

            foreach (var id in ids) {
                //System.Diagnostics.Debug.WriteLine(prefix + id);
                AddTaxonomyTableCell(resultTable, prefix + id, true);
                //if (Config.Document.ValueTreeGcd.Root.Values[prefix + id].HasValue) {

                //    resultTable.AddCell(TaxonomyManager.GCD_Taxonomy.ElementsByName[prefix + id].ToString());
                //    resultTable.AddCell(Config.Document.ValueTreeGcd.Root.Values[prefix + id].DisplayString);
                //}
            }

            return resultTable;
        }
        #endregion CreateTableFromIds

        /*#region ExportIncomeStatement
        private void ExportIncomeStatement(string _uri) {
            PdfPTable table = IsExportReconciliationInfo()
                                  ? AddTreeViewTableUeberleitung(true)
                                  : AddTreeViewTable(false);

            var root = Config.Document.GaapPresentationTrees[_uri].RootEntries.First();

            PdfTreeView tv = new PdfTreeView();
            BuildTreeView(tv, Config.Document.ValueTreeMain, root);

            if (!IsExportNilValues(tv) && (!tv.ExportElement && (!Config.ExportAsXbrl || !tv.HasMandatoryChild))) {
                return;
            }

            ExportTreeView(tv, table, IsExportReconciliationInfo(), false, true);
            var role = Config.Document.MainTaxonomy.GetRole(_uri);
            pdf.AddHeadline(role.Name +
                            (IsExportAccounts()
                                 ? eBalanceKitResources.Localisation.ResourcesPdfExport.WithBalances
                                 : string.Empty));
            pdf.CurrentChapter.Add(table);
        }
        #endregion ExportIncomeStatement*/

        /*#region ExportBalanceSheet
        private void ExportBalanceSheet(string _uri) {
            PdfPTable table = IsExportReconciliationInfo()
                                  ? AddTreeViewTableUeberleitung(false)
                                  : AddTreeViewTable(false);

            var root = Config.Document.GaapPresentationTrees[_uri].RootEntries.First();
            PdfTreeView tv = new PdfTreeView();
            BuildTreeView(tv, Config.Document.ValueTreeMain, root);

            if (!IsExportNilValues(tv) && (!tv.ExportElement && (!Config.ExportAsXbrl || !tv.HasMandatoryChild))) {
                return;
            }
            IRoleType role = Config.Document.MainTaxonomy.GetRole(_uri);
            pdf.AddHeadline(role.Name +
                            (IsExportAccounts()
                                 ? eBalanceKitResources.Localisation.ResourcesPdfExport.WithBalances
                                 : string.Empty));
            ExportTreeView(tv, table, IsExportReconciliationInfo(), false, false);
            pdf.CurrentChapter.Add(table);
        }
        #endregion ExportBalanceSheet*/

        private void ExportManagementReport() {
            string uri = ExportHelper.GetUri("managementReport", Config.Document);
            IPresentationTreeNode rootEntry = Config.Document.GaapPresentationTrees[uri].RootEntries.First();
            ExportTreeViewManagementReport(Config.Document.ValueTreeMain, rootEntry);
        }

        private bool _isFirstAdd;

        /// <summary>
        /// Exports the TreeView for the ManagementReport
        /// </summary>
        /// <param name="vtree">Config.Document.ValueTreeMain</param>
        /// <param name="root">The first entry in the managementReport tag.</param>
        /// <author>Sebastian Vetter</author>
        private void ExportTreeViewManagementReport(ValueTree vtree,
                                                    Taxonomy.Interfaces.PresentationTree.IPresentationTreeNode root) {
            try {
                int pos = 1;
                PdfTreeView tv = new PdfTreeView();
                foreach (IPresentationTreeEntry child in root.Children) {
                    PdfTreeView tvChild = null;

                    //Skip if only mandatory fields wanted -- sev
                    if (child.Element != null && IsExportMandatoryOnly() && !child.Element.IsMandatoryField) {
                        continue;
                    }

                    Taxonomy.Interfaces.PresentationTree.IPresentationTreeNode pchild =
                        child as Taxonomy.Interfaces.PresentationTree.IPresentationTreeNode;

                    if (vtree.Root.Values.ContainsKey(pchild.Element.Id)) {
                        tvChild = tv.AddChild(pchild.Element, pchild.Element.MandatoryLabel, pos, false);
                        IValueTreeEntry value = vtree.Root.Values[pchild.Element.Id];
                        tvChild.Comment = ((XbrlElementValueBase) value).Comment ?? string.Empty;

                        if ((pchild.Element.ValueType == XbrlElementValueTypes.String) && (value.Value != null)) {
                            // We need the complete string and not the limited DisplayString --sev
                            tvChild.Value = value.Value.ToString();
                        } else {
                            tvChild.Value = value.DisplayString;
                        }

                        tvChild.ReconciliationInfo = value.ReconciliationInfo;
                        tvChild.HasValue = value.HasValue;
                    } else {
                        tvChild = tv.AddChild(pchild.Element, pchild.Element.MandatoryLabel, pos);
                        tvChild.Value = string.Empty;
                        tvChild.HasValue = false;
                    }


                    if (tvChild.Value == null || tvChild.HasValue == false || tvChild.Value.Trim().ToLower().Equals("-")) {
                        tvChild.Value = string.Empty;
                    }


                    bool exportElememt = false;
                    exportElememt = exportElement(pchild as Structures.Presentation.PresentationTreeNode);

                    if (exportElememt || IsExportNilValues(tvChild)) {
                        if (_isFirstAdd) {
                            pdf.AddHeadline("Lagebericht");
                            _isFirstAdd = false;
                        }
                        Taxonomy.Interfaces.PresentationTree.IPresentationTreeEntry tmp = child;
                        int spacer = 0;

                        while ((tmp.Parents.Count() == 1)) {
                            spacer++;
                            tmp = tmp.Parents.First();
                        }

                        switch (spacer) {
                            case 1:
                                pdf.AddSubHeadline(child.Element.Label);
                                break;
                            case 2:
                                pdf.AddSubSubHeadline(child.Element.Label);
                                break;
                            default:
                                pdf.AddSubSubHeadline(child.Element.Label);
                                break;
                        }

                        PdfPTable table = DefaultTable(1);

                        //AddDefaultCell(table, string.Empty);

                        //AddDefaultCell(table, tvChild.Value);
                        table.AddCell(new Phrase(tvChild.Value, font));

                        //AddDefaultCell(table, string.Empty);

                        // --mga rewrote the --sev's add
                        if (child.Parents.Any()) {
                            if (child.Parents.First().Parents.Any()) {
                                pdf.CurrentSubSection.Add(table);
                            } else {
                                pdf.CurrentSection.Add(table);
                            }
                        } else {
                            pdf.CurrentChapter.Add(table);
                        }
                        //if ((pdf.CurrentSubSection != null) && (child.Parents.Count() > 1))
                        //    pdf.CurrentSubSection.Add(table);
                        //else if (pdf.CurrentSection != null)
                        //    pdf.CurrentSection.Add(table);
                        //else
                        //    pdf.CurrentChapter.Add(table);
                    }

                    ExportTreeViewManagementReport(vtree,
                                                   child as Taxonomy.Interfaces.PresentationTree.IPresentationTreeNode);

                    pos++;
                }
            } catch (Exception ex) {
                throw new Exception(ex.Message, ex);
            }
        }


        private bool exportElement(Structures.Presentation.PresentationTreeNode pchild) {
            if (pchild.Value.HasValue || pchild.Element.IsMandatoryField)
                return true;
            foreach (var cChild in pchild.Children)
                if (exportElement(cChild as Structures.Presentation.PresentationTreeNode))
                    return true;
            return false;
        }

        /*
        #region ExportAccountBalances
        private void ExportAccountBalances() {
            XbrlElementValue_List accountBalanceList =
                //Config.Document.ValueTreeMain.Root.Values["detailedInformation.accountBalances"] as
                Config.Document.ValueTreeMain.Root.Values["de-gaap-ci_detailedInformation.accountBalances"] as
                XbrlElementValue_List;


            if (accountBalanceList != null)
                pdf.AddHeadline(accountBalanceList.Element.MandatoryLabel);

            Dictionary<string, PdfTreeView> nodeDict = new Dictionary<string, PdfTreeView>();
            string uri = string.Empty;
            IPresentationTreeNode rootEntry;
            PdfPTable table;

            // Check if export of balance sheet is wanted
            if (Config.ExportBalanceSheet) {
                // Lookup the uri for the balance sheet
                uri = ExportHelper.GetUri(ExportHelper.ExportInformations.BalanceSheet.Name, Config.Document);
                rootEntry = Config.Document.GaapPresentationTrees[uri].RootEntries.First();
                // New initialize otherwise we get the same entry more than once
                ExportHelper.ExportInformations.BalanceSheet.tv = new PdfTreeView();
                BuildTreeViewAccountBalances(nodeDict, ExportHelper.ExportInformations.BalanceSheet.tv, rootEntry);
            }

            if (Config.ExportIncomeStatement) {
                uri = ExportHelper.GetUri(ExportHelper.ExportInformations.IncomeStatement.Name, Config.Document);
                rootEntry = Config.Document.GaapPresentationTrees[uri].RootEntries.First();
                ExportHelper.ExportInformations.IncomeStatement.tv = new PdfTreeView();
                BuildTreeViewAccountBalances(nodeDict, ExportHelper.ExportInformations.IncomeStatement.tv, rootEntry);
            }

            if (Config.ExportAppropriationProfit) {
                uri = ExportHelper.GetUri(ExportHelper.ExportInformations.AppropriationProfits.Name, Config.Document);
                rootEntry = Config.Document.GaapPresentationTrees[uri].RootEntries.First();
                ExportHelper.ExportInformations.AppropriationProfits.tv = new PdfTreeView();
                BuildTreeViewAccountBalances(nodeDict, ExportHelper.ExportInformations.AppropriationProfits.tv,
                                             rootEntry);
            }

            if (Config.ExportContingentLiabilities) {
                uri = ExportHelper.GetUri(ExportHelper.ExportInformations.ContingentLiabilities.Name, Config.Document);
                rootEntry = Config.Document.GaapPresentationTrees[uri].RootEntries.First();
                ExportHelper.ExportInformations.ContingentLiabilities.tv = new PdfTreeView();
                BuildTreeViewAccountBalances(nodeDict, ExportHelper.ExportInformations.ContingentLiabilities.tv,
                                             rootEntry);
            }

            if (Config.ExportCashFlowStatement) {
                uri = ExportHelper.GetUri(ExportHelper.ExportInformations.CashFlowStatement.Name, Config.Document);
                rootEntry = Config.Document.GaapPresentationTrees[uri].RootEntries.First();
                ExportHelper.ExportInformations.CashFlowStatement.tv = new PdfTreeView();
                BuildTreeViewAccountBalances(nodeDict, ExportHelper.ExportInformations.CashFlowStatement.tv, rootEntry);
            }

            if (Config.ExportAdjustmentOfIncome) {
                uri = ExportHelper.GetUri(ExportHelper.ExportInformations.AdjustmentOfIncome.Name, Config.Document);
                rootEntry = Config.Document.GaapPresentationTrees[uri].RootEntries.First();
                ExportHelper.ExportInformations.AdjustmentOfIncome.tv = new PdfTreeView();
                BuildTreeViewAccountBalances(nodeDict, ExportHelper.ExportInformations.AdjustmentOfIncome.tv, rootEntry);
            }
            if (Config.ExportDeterminationOfTaxableIncome) {
                uri = ExportHelper.GetUri(ExportHelper.ExportInformations.DeterminationOfTaxableIncome.Name,
                                          Config.Document);
                rootEntry = Config.Document.GaapPresentationTrees[uri].RootEntries.First();
                ExportHelper.ExportInformations.DeterminationOfTaxableIncome.tv = new PdfTreeView();
                BuildTreeViewAccountBalances(nodeDict, ExportHelper.ExportInformations.DeterminationOfTaxableIncome.tv,
                                             rootEntry);
            }
            if (Config.ExportDeterminationOfTaxableIncomeBusinessPartnership) {
                uri =
                    ExportHelper.GetUri(
                        ExportHelper.ExportInformations.DeterminationOfTaxableIncomeBusinessPartnership.Name,
                        Config.Document);
                rootEntry = Config.Document.GaapPresentationTrees[uri].RootEntries.First();
                ExportHelper.ExportInformations.DeterminationOfTaxableIncomeBusinessPartnership.tv = new PdfTreeView();
                BuildTreeViewAccountBalances(nodeDict,
                                             ExportHelper.ExportInformations.
                                                 DeterminationOfTaxableIncomeBusinessPartnership.tv, rootEntry);
            }
            if (Config.ExportDeterminationOfTaxableIncomeSpecialCases) {
                uri = ExportHelper.GetUri(
                    ExportHelper.ExportInformations.DeterminationOfTaxableIncomeSpecialCases.Name, Config.Document);
                rootEntry = Config.Document.GaapPresentationTrees[uri].RootEntries.First();
                ExportHelper.ExportInformations.DeterminationOfTaxableIncomeSpecialCases.tv = new PdfTreeView();
                BuildTreeViewAccountBalances(nodeDict,
                                             ExportHelper.ExportInformations.DeterminationOfTaxableIncomeSpecialCases.tv,
                                             rootEntry);
            }

            foreach (var balanceList in Config.Document.BalanceLists) {
                foreach (var entry in balanceList.AssignedItems) {
                    if (entry is IAccountGroup) {
                        if (!entry.SendBalance) continue;
                        IAccountGroup group = entry as IAccountGroup;
                        foreach (var account in group.Items)
                            ExportAccountBalance(nodeDict, account, group.AssignedElement);
                    } else {
                        if (!entry.SendBalance) continue;
                        ExportAccountBalance(nodeDict, entry);
                    }
                }
            }

            // Entry only needed if export of balance is wanted
            if (Config.ExportBalanceSheet && ExportHelper.ExportInformations.BalanceSheet.tv.ExportElement) {
                // Add the headline that is stored in the struct
                pdf.AddSubHeadline(ExportHelper.ExportInformations.BalanceSheet.Headline);
                table = AddTreeViewTable(true);
                ExportTreeView(ExportHelper.ExportInformations.BalanceSheet.tv, table, false, true, false);
                pdf.CurrentChapter.Add(table);
            }

            if (Config.ExportIncomeStatement && ExportHelper.ExportInformations.IncomeStatement.tv.ExportElement) {
                pdf.AddSubHeadline(ExportHelper.ExportInformations.IncomeStatement.Headline);
                table = AddTreeViewTable(true);
                ExportTreeView(ExportHelper.ExportInformations.IncomeStatement.tv, table, false, true, true);
                pdf.CurrentChapter.Add(table);
            }

            if (Config.ExportAppropriationProfit &&
                ExportHelper.ExportInformations.AppropriationProfits.tv.ExportElement) {
                pdf.AddSubHeadline(ExportHelper.ExportInformations.AppropriationProfits.Headline);
                table = AddTreeViewTable(true);
                ExportTreeView(ExportHelper.ExportInformations.AppropriationProfits.tv, table, false, true, false);
                pdf.CurrentChapter.Add(table);
            }

            if (Config.ExportContingentLiabilities &&
                ExportHelper.ExportInformations.ContingentLiabilities.tv.ExportElement) {
                pdf.AddSubHeadline(ExportHelper.ExportInformations.ContingentLiabilities.Headline);
                table = AddTreeViewTable(true);
                ExportTreeView(ExportHelper.ExportInformations.ContingentLiabilities.tv, table, false, true, false);
                pdf.CurrentChapter.Add(table);
            }

            if (Config.ExportCashFlowStatement && ExportHelper.ExportInformations.CashFlowStatement.tv.ExportElement) {
                pdf.AddSubHeadline(ExportHelper.ExportInformations.CashFlowStatement.Headline);
                table = AddTreeViewTable(true);
                ExportTreeView(ExportHelper.ExportInformations.CashFlowStatement.tv, table, false, true, false);
                pdf.CurrentChapter.Add(table);
            }

            if (Config.ExportDeterminationOfTaxableIncomeSpecialCases &&
                ExportHelper.ExportInformations.DeterminationOfTaxableIncomeSpecialCases.tv.ExportElement) {
                pdf.AddSubHeadline(ExportHelper.ExportInformations.DeterminationOfTaxableIncomeSpecialCases.Headline);
                table = AddTreeViewTable(true);
                ExportTreeView(ExportHelper.ExportInformations.DeterminationOfTaxableIncomeSpecialCases.tv, table, false,
                               true, false);
                pdf.CurrentChapter.Add(table);
            }
            if (Config.ExportDeterminationOfTaxableIncomeBusinessPartnership &&
                ExportHelper.ExportInformations.DeterminationOfTaxableIncomeBusinessPartnership.tv.ExportElement) {
                pdf.AddSubHeadline(
                    ExportHelper.ExportInformations.DeterminationOfTaxableIncomeBusinessPartnership.Headline);
                table = AddTreeViewTable(true);
                ExportTreeView(ExportHelper.ExportInformations.DeterminationOfTaxableIncomeBusinessPartnership.tv, table,
                               false, true, false);
                pdf.CurrentChapter.Add(table);
            }
            if (Config.ExportDeterminationOfTaxableIncome &&
                ExportHelper.ExportInformations.DeterminationOfTaxableIncome.tv.ExportElement) {
                pdf.AddSubHeadline(ExportHelper.ExportInformations.DeterminationOfTaxableIncome.Headline);
                table = AddTreeViewTable(true);
                ExportTreeView(ExportHelper.ExportInformations.DeterminationOfTaxableIncome.tv, table, false, true,
                               false);
                pdf.CurrentChapter.Add(table);
            }
            if (Config.ExportAdjustmentOfIncome && ExportHelper.ExportInformations.AdjustmentOfIncome.tv.ExportElement) {
                pdf.AddSubHeadline(ExportHelper.ExportInformations.AdjustmentOfIncome.Headline);
                table = AddTreeViewTable(true);
                ExportTreeView(ExportHelper.ExportInformations.AdjustmentOfIncome.tv, table, false, true, false);
                pdf.CurrentChapter.Add(table);
            }
        }


        private static void ExportAccountBalance(Dictionary<string, PdfTreeView> nodeDict, IBalanceListEntry entry,
                                                 IElement elem = null) {
            var elemName = elem == null ? entry.AssignedElement.Name : elem.Name;
            var accNumber = entry.Number;
            var accName = entry.Name;
            var balance = entry.Amount;

            if (nodeDict.ContainsKey(elemName)) {
                var node = nodeDict[elemName];
                var child = node.AddChild(null, accNumber + " - " + accName, 0, true);
                child.Value = balance.ToString("#,0.00") + " €";
                child.HasValue = true;
            } else {
                // TODO
            }
        }
        #endregion ExportAccountBalances
        */

        #region AddTreeViewTable
        private PdfPTable AddTreeViewTable(bool accountBalances) {
            PdfPTable table = new PdfPTable(2);
            table.DefaultCell.Border = 0;
            table.WidthPercentage = 100;
            table.SetTotalWidth(new float[] {500f, 100f});

            AddHeaderCell(table, string.Empty, Element.ALIGN_LEFT);
            AddHeaderCell(table, accountBalances
                                     ? eBalanceKitResources.Localisation.ResourcesPdfExport.Amount
                                     : Config.Document.IsCommercialBalanceSheet
                                           ? eBalanceKitResources.Localisation.ResourcesPdfExport.Handelsbilanzwert
                                           : eBalanceKitResources.Localisation.ResourcesPdfExport.Steuerbilanzwert,
                          Element.ALIGN_RIGHT);
            table.HeaderRows = 1;
            return table;
        }
        #endregion AddTreeViewTable

        #region AddTreeViewTableUeberleitung
        private PdfPTable AddTreeViewTableUeberleitung(bool GuV) {
            PdfPTable table = new PdfPTable(4);
            table.DefaultCell.Border = 0;
            table.WidthPercentage = 100;
            table.SetTotalWidth(new float[] {100f, 37f, 33f, 33f});

            AddHeaderCell(table, string.Empty, Element.ALIGN_LEFT);
            AddHeaderCell(table,
                          GuV
                              ? eBalanceKitResources.Localisation.ResourcesPdfExport.Überleitungswert
                              : eBalanceKitResources.Localisation.ResourcesPdfExport.ReconciliationCurPer,
                          Element.ALIGN_RIGHT);

            AddHeaderCell(table, eBalanceKitResources.Localisation.ResourcesPdfExport.Handelsbilanzwert,
                          Element.ALIGN_RIGHT);
            AddHeaderCell(table, eBalanceKitResources.Localisation.ResourcesPdfExport.Steuerbilanzwert,
                          Element.ALIGN_RIGHT);
            table.HeaderRows = 1;
            return table;
        }
        #endregion AddTreeViewTableUeberleitung

        #region AddHeaderCell
        private void AddHeaderCell(PdfPTable table, string value, int hAlignment) {
            PdfPCell cell = new PdfPCell(new Phrase(value, fontBold));
            cell.HorizontalAlignment = hAlignment;
            cell.VerticalAlignment = Element.ALIGN_CENTER;
            cell.Border = table.DefaultCell.Border;
            table.AddCell(cell);
        }
        #endregion AddHeaderCell

        /*
        #region BuildTreeViewAccountBalances
        private void BuildTreeViewAccountBalances(Dictionary<string, PdfTreeView> nodeDict, PdfTreeView tv,
                                                  Taxonomy.Interfaces.PresentationTree.IPresentationTreeNode root) {
            try {
                int pos = 1;
                foreach (IPresentationTreeEntry child in root.Children) {
                    //Skip if only mandatory fields wanted -- sev
                    if (child.Element != null && IsExportMandatoryOnly && !child.Element.IsMandatoryField) {
                        continue;
                    }

                    PdfTreeView tvChild = null;

                    if (child is eBalanceKitBusiness.Structures.DbMapping.BalanceList.BalanceListEntryBase) {
                    } else {
                        Taxonomy.Interfaces.PresentationTree.IPresentationTreeNode pchild =
                            child as Taxonomy.Interfaces.PresentationTree.IPresentationTreeNode;

                        tvChild = tv.AddChild(pchild.Element, pchild.Element.MandatoryLabel, pos, false);
                        tvChild.Value = string.Empty;
                        tvChild.HasValue = false;
                        nodeDict[pchild.Element.Name] = tvChild;

                        if (tvChild != null)
                            BuildTreeViewAccountBalances(nodeDict, tvChild,
                                                         child as
                                                         Taxonomy.Interfaces.PresentationTree.IPresentationTreeNode);
                    }

                    pos++;
                }
            } catch (Exception ex) {
                throw new Exception(ex.Message, ex);
            }
        }
        #endregion BuildTreeViewAccountBalances
        */

        #region BuildTreeView
        private void BuildTreeView(PdfTreeView tv, ValueTree vtree,
                                   Taxonomy.Interfaces.PresentationTree.IPresentationTreeEntry root, int pos = 1) {
            PdfTreeView tvChild;

            //Skip if only mandatory fields wanted -- sev
            if (root.Element != null && IsExportMandatoryOnly() && !root.Element.IsMandatoryField) {
                return;
            }


            if (root is IBalanceListEntry) {
                IBalanceListEntry balanceListEntryChild = root as IBalanceListEntry;
                tvChild = tv.AddChild(null, balanceListEntryChild.Label, pos, true);
                tvChild.Value = balanceListEntryChild.ValueDisplayString;
                string comment = balanceListEntryChild.Comment;
                tvChild.Comment = comment ?? string.Empty;
            } else {
                if (vtree.Root.Values.ContainsKey(root.Element.Id)) {
                    var presentationTree =
                        DocumentManager.Instance.CurrentDocument.GaapPresentationTrees.Where(
                            tree => tree.Value.Nodes.Any(node => node.Element.Id == root.Element.Id)).ToList();

                    tvChild = tv.AddChild(root.Element, root.Element.MandatoryLabel, pos, false);
                    IValueTreeEntry value = vtree.Root.Values[root.Element.Id];
                    XbrlElementValueBase valueBase = value as XbrlElementValueBase;
                    tvChild.Comment = valueBase.Comment != null ? valueBase.Comment : string.Empty;
                    tvChild.IsMandatoryField = value.Element.IsMandatoryField;

                    string temp = null;
                    if ((value.IsNumeric && value.DecimalValue != 0) ||
                        (!value.IsNumeric && !string.IsNullOrEmpty(value.ToString()))
                        ||
                        (value.ReconciliationInfo != null && value.ReconciliationInfo.HasTransferValue &&
                         IsExportReconciliationInfo())
                        ) {
                        if ((root.Element.ValueType == XbrlElementValueTypes.String) && (value.Value != null)) {
                            // We need the complete string and not the limited DisplayString --sev
                            temp = value.Value.ToString();
                        } else {
                            temp = value.DisplayString;
                        }

                        tvChild.ReconciliationInfo = value.ReconciliationInfo;
                        tvChild.HasValue = value.HasValue ||
                                           (tvChild.ReconciliationInfo != null &&
                                            tvChild.ReconciliationInfo.HasTransferValue &&
                                            IsExportReconciliationInfo());
                        if (value is XbrlElementValue_Monetary) {
                            tvChild.ExportAccount = IsExportAccounts(value.DbValue.SendAccountBalances);
                            tvChild.HasComputedValue = value.HasComputedValue;
                        }
                    }
                    SetNullIfXbrl(ref temp);
                    tvChild.Value = temp;
                } else {
                    /*
                    // create a separate pdf for this HyperCube element
                    // ToDo Check only if IsHypercubeItem  -- sev
                    if (pchild.Element.IsHypercubeItem || pchild.Element.IsAbstract) {
                        if (pchild.Element.Label.Contains("Anlagespiegel") || pchild.Element.Label.Contains("Eigenkapitalspiegel")) {
                                
                        new HyperCubeExporter(Config).ExportPdfLimited(pchild.Element.Id, 2);
                        }
                    }
                    */
                    tvChild = tv.AddChild(root.Element, root.Element.MandatoryLabel, pos, false);
                    string temp = string.Empty;
                    SetNullIfXbrl(ref temp);
                    tvChild.Value = temp;
                    tvChild.HasValue = false;
                }
            }

            IPresentationTreeNode pchild = root as IPresentationTreeNode;

            if (pchild == null) {
                return;
            }

            int innerPos = 1;
            foreach (IPresentationTreeEntry child in pchild.Children) {
                BuildTreeView(tvChild, vtree,
                              child, innerPos);
                innerPos++;
            }
        }

        private void SetNullIfXbrl(ref string value) {
            if (Config.ExportAsXbrl && (string.IsNullOrEmpty(value) || value == "-")) {
                value = "NIL";
            }
        }

        #endregion BuildTreeView

        #region ExportTreeView
        private void ExportTreeView(PdfTreeView tv, PdfPTable table, bool includeUeberleitung, bool accountBalances,
                                    bool GuV) {
            foreach (var child in tv.Children) {
                try {
                    if (child.Level > 0) {
                        bool needHeadNumber;
                        if ((Config.Document.MainTaxonomyInfo.Type == TaxonomyType.GAAP) ||
                            (Config.Document.MainTaxonomyInfo.Type == TaxonomyType.OtherBusinessClass)) {
                            needHeadNumber = true;
                        } else {
                            needHeadNumber = false;
                        }

                        if (child.IsAccount) {
                            if (child.Parent.ExportAccount && (IsExportAccounts() || accountBalances)) {
                                //BaseColor bgColor = new BaseColor(200, 200, 200);

                                PdfPTable headertable = new PdfPTable(2);
                                headertable.DefaultCell.Border = 0;
                                headertable.WidthPercentage = 100;
                                headertable.TotalWidth = 500f;
                                float fillerWidth = (10f*child.Level + 1);
                                float headerWidth = headertable.TotalWidth - fillerWidth;
                                headertable.SetWidths(new float[] {fillerWidth, headerWidth});
                                headertable.AddCell(string.Empty); // filler
                                AddDefaultCell(headertable, child.Header, null, includeUeberleitung);

                                PdfPCell cell = new PdfPCell(headertable);
                                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                cell.Border = table.DefaultCell.Border;
                                table.AddCell(cell);

                                //When Ueberleitungswerte should be exported, then we have two more columns
                                if (includeUeberleitung) {
                                    AddValueCell(table, string.Empty);
                                    AddValueCell(table, string.Empty);
                                }
                                AddValueCell(table, child.Value, null, includeUeberleitung);
                            }
                        } else if (child.Element.ValueType == Taxonomy.Enums.XbrlElementValueTypes.Monetary) {
                            //Do not export NIL values for account balances
                            if ((!accountBalances && IsExportNilValues(child)) || child.ExportElement) {
                                PdfPTable headertable = new PdfPTable(3);
                                headertable.DefaultCell.Border = 0;
                                headertable.WidthPercentage = 100;
                                headertable.TotalWidth = 500f;
                                float fillerWidth = 10f*child.Level;
                                float numberWidth = (needHeadNumber
                                                         ? ComputeNumberWidth(child.Level, child.HeaderNumber,
                                                                              includeUeberleitung)
                                                         : ComputeNumberWidth(child.Level, string.Empty,
                                                                              includeUeberleitung));
                                //Phrase p = new Phrase(child.HeaderNumber);
                                float headerWidth = headertable.TotalWidth - fillerWidth - numberWidth;
                                headertable.SetWidths(new float[] {fillerWidth, numberWidth, headerWidth});
                                headertable.AddCell(string.Empty); // filler
                                AddDefaultCell(headertable, needHeadNumber ? child.HeaderNumber : string.Empty);

                                AddDefaultCell(headertable,
                                               IsExportComments()
                                                   ? child.Header + Environment.NewLine + child.Comment
                                                   : child.Header, bold: child.HasComputedValue);

                                table.AddCell(headertable);
                                if (includeUeberleitung) {
                                    string hbValue = string.Empty,
                                           transfer = string.Empty,
                                           stValue = string.Empty,
                                           transferPrevious = string.Empty;
                                    if (child.ReconciliationInfo != null) {
                                        if (child.ReconciliationInfo.HBValue != null) {
                                            hbValue =
                                                child.ReconciliationInfo.GetDisplayString(
                                                    child.ReconciliationInfo.HBValue);
                                            SetNullIfXbrl(ref hbValue);
                                        }
                                        //if (child.ReconciliationInfo.TransferValue != null)
                                        transfer =
                                            child.ReconciliationInfo.GetDisplayString(
                                                child.ReconciliationInfo.TransferValue);
                                        SetNullIfXbrl(ref transfer);
                                        //if (child.ReconciliationInfo.TransferValuePreviousYear != null)
                                        transferPrevious =
                                            child.ReconciliationInfo.GetDisplayString(
                                                child.ReconciliationInfo.TransferValuePreviousYear);
                                        SetNullIfXbrl(ref transferPrevious);
                                        if (child.ReconciliationInfo.STValue != null) {
                                            stValue =
                                                child.ReconciliationInfo.GetDisplayString(
                                                    child.ReconciliationInfo.STValue);
                                            SetNullIfXbrl(ref stValue);
                                        }
                                    } else {
                                        hbValue = child.Value;
                                        stValue = child.Value;
                                    }

                                    PdfPTable transferTable = new PdfPTable(1);
                                    transferTable.DefaultCell.Border = 0;
                                    transferTable.WidthPercentage = 100;
                                    AddValueCell(transferTable, transfer, bold: child.HasComputedValue);
                                    AddValueCell(transferTable, transferPrevious, bold: child.HasComputedValue);

                                    if (!GuV && !string.IsNullOrEmpty(transferPrevious)) {
                                        PdfPCell cell = new PdfPCell(transferTable);
                                        cell.VerticalAlignment = Element.ALIGN_CENTER;
                                        cell.Border = 0;
                                        table.AddCell(cell);
                                    } else
                                        AddValueCell(table, transfer, bold: child.HasComputedValue);
                                    AddValueCell(table, hbValue, bold: child.HasComputedValue);
                                    AddValueCell(table, stValue, bold: child.HasComputedValue);

                                    //Create a second line if it is a balance sheet and there is previous transfer value
                                    //if (!GuV && !string.IsNullOrEmpty(transferPrevious)) {
                                    //    table.AddCell(string.Empty);
                                    //    AddValueCell(table, transferPrevious);
                                    //    table.AddCell(string.Empty);
                                    //    table.AddCell(string.Empty);
                                    //}
                                } else {
                                    //AddDefaultCell(table, child.Value, bold: child.HasComputedValue);
                                    AddValueCell(table, child.Value, bold: child.HasComputedValue);
                                }
                            }
                        } else if (((child.Element.ValueType == Taxonomy.Enums.XbrlElementValueTypes.String) ||
                                    (child.Element.ValueType == Taxonomy.Enums.XbrlElementValueTypes.Abstract)) &&
                                   child.HasValue) {
                            // ToDo Add function to insert string fields
                            //AddHeaderCell(table, child.Value, 0);
                            if (child.ExportElement || IsExportNilValues(child)) {
                                PdfPTable headertable = new PdfPTable(3);
                                headertable.DefaultCell.Border = 0;
                                headertable.WidthPercentage = 100;
                                headertable.TotalWidth = 500f;
                                float fillerWidth = 10f*child.Level;
                                float numberWidth = (needHeadNumber
                                                         ? ComputeNumberWidth(child.Level, child.HeaderNumber,
                                                                              includeUeberleitung)
                                                         : ComputeNumberWidth(child.Level, string.Empty,
                                                                              includeUeberleitung));
                                //Phrase p = new Phrase(child.HeaderNumber);
                                float headerWidth = headertable.TotalWidth - fillerWidth;
                                headertable.SetWidths(new float[] {fillerWidth, numberWidth, headerWidth});
                                headertable.AddCell(string.Empty); // filler
                                AddDefaultCell(headertable, needHeadNumber ? child.HeaderNumber : string.Empty);

                                AddDefaultCell(headertable, child.Header + Environment.NewLine + child.Value);

                                table.AddCell(headertable);

                                AddDefaultCell(table, string.Empty);
                            }
                        }
                    }

                    ExportTreeView(child, table, includeUeberleitung, accountBalances, GuV);
                } catch (Exception ex) {
                    throw new Exception(ex.Message, ex);
                }
            }
        }
        #endregion ExportTreeView

        #region ComputeNumberWidth
        private static float ComputeNumberWidth(int level, string posNumber, bool includeUeberleitung) {
            if (includeUeberleitung)
                return (14f*posNumber.Replace(".", string.Empty).Length) + (1.2f*level);
            return (6f*posNumber.Replace(".", string.Empty).Length) + (1.2f*level);
        }
        #endregion ComputeNumberWidth

        #region ComputeNumberWidthUeberleitung
        private static float ComputeNumberWidthUeberleitung(int level, string posNumber) { return (14f*posNumber.Replace(".", string.Empty).Length) + (1.2f*level); }
        #endregion ComputeNumberWidthUeberleitung

        #region AddDefaultCell
        /// <summary>
        /// Adds a left alligned table cell.
        /// </summary>
        /// <param name="table"></param>
        /// <param name="value"></param>
        private void AddDefaultCell(PdfPTable table, string value, BaseColor backgroundColor = null,
                                    bool italic = false, bool bold = false) {
            PdfPCell cell = new PdfPCell(new Phrase(value, italic ? fontItalic : bold ? fontBold : font));
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            cell.Border = table.DefaultCell.Border;
            if (backgroundColor != null) cell.BackgroundColor = backgroundColor;
            table.AddCell(cell);
        }
        #endregion AddDefaultCell

        #region AddValueCell
        /// <summary>
        /// Adds a right alligned table cell.
        /// </summary>
        /// <param name="table"></param>
        /// <param name="value"></param>
        private void AddValueCell(PdfPTable table, string value, BaseColor backgroundColor = null,
                                  bool italic = false, bool bold = false) {
            PdfPCell cell = new PdfPCell(new Phrase(value, italic ? fontItalic : bold ? fontBold : font));
            cell.HorizontalAlignment = Element.ALIGN_RIGHT;
            cell.VerticalAlignment = Element.ALIGN_CENTER;
            cell.Border = table.DefaultCell.Border;
            if (backgroundColor != null) cell.BackgroundColor = backgroundColor;
            table.AddCell(cell);
        }
        #endregion AddValueCell

        /*
        #region ExportPresentationTree
        private void ExportPresentationTree(PresentationTree ptree, ValueTree vtree,
                                            eBalanceKitBusiness.Structures.DbMapping.Document document) {
            PdfPTable table = new PdfPTable(2);
            table.DefaultCell.Border = 0;
            table.WidthPercentage = 100;
            table.SetWidths(new float[] {500f, 100f});

            foreach (PresentationTreeNode root in ptree.RootEntries) {
                ExportPresentationTree(0, root, vtree, table);
            }

            pdf.CurrentChapter.Add(table);
        }

        private void ExportPresentationTree(int level, PresentationTreeNode root, ValueTree vtree, PdfPTable table,
                                            string header = string.Empty) {
            if (level > 0) {
                string h = string.Empty;
                for (int i = 0; i < level*8; i++) h += " ";

                if (Config.ExportNILValues) {
                    table.AddCell(new Phrase(h + header + " " + root.Element.Label, font));

                    if (vtree.Root.Values.ContainsKey(root.Element.Id)) {
                        IValueTreeEntry value = vtree.Root.Values[root.Element.Id];
                        table.AddCell(new Phrase(value.DisplayString, font));
                    } else {
                        table.AddCell(new Phrase(string.Empty, font));
                    }
                } else {
                    if (vtree.Root.Values.ContainsKey(root.Element.Id)) {
                        IValueTreeEntry value = vtree.Root.Values[root.Element.Id];

                        if ((value.IsNumeric && value.DecimalValue != 0) ||
                            (!value.IsNumeric && !string.IsNullOrEmpty(value.ToString()))) {
                            table.AddCell(new Phrase(h + header + ". " + root.Element.Label, font));
                            table.AddCell(new Phrase(value.DisplayString, font));
                        }
                    }
                }

                int pos = 1;
                foreach (Taxonomy.Interfaces.PresentationTree.IPresentationTreeNode child in root.Children) {
                    if (child is PresentationTreeNode) {
                        ExportPresentationTree(level + 1, (PresentationTreeNode) child, vtree, table, header + "." + pos);
                        pos++;
                    } else if (child is eBalanceKitBusiness.Structures.DbMapping.BalanceList.BalanceListEntryBase) {
                        eBalanceKitBusiness.Structures.DbMapping.BalanceList.BalanceListEntryBase account =
                            (eBalanceKitBusiness.Structures.DbMapping.BalanceList.BalanceListEntryBase) child;

                        if (IsExportAccounts() && (Config.ExportNILValues || account.Amount != 0)) {
                            string h1 = string.Empty;
                            for (int i = 0; i < (level + 1)*8; i++) h1 += " ";

                            table.AddCell(new Phrase(h1 + " " + account.Label, font));
                            table.AddCell(new Phrase(account.ValueDisplayString, font));
                        }
                    }
                }
            } else {
                int pos = 1;
                foreach (Taxonomy.Interfaces.PresentationTree.IPresentationTreeNode child in root.Children) {
                    if (child is PresentationTreeNode) {
                        ExportPresentationTree(level + 1, (PresentationTreeNode) child, vtree, table, pos.ToString());
                        pos++;
                    }
                }
            }
        }
        #endregion ExportPresentationTree
        */
    }
}