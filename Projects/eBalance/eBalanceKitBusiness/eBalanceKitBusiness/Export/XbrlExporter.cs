// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-09-12
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Xml;
using Taxonomy.Enums;
using eBalanceKitBusiness.HyperCubes.Interfaces.Structure;
using eBalanceKitBusiness.Logs;
using eBalanceKitBusiness.Reconciliation;
using eBalanceKitBusiness.Reconciliation.Enums;
using eBalanceKitBusiness.Reconciliation.Interfaces;
using eBalanceKitBusiness.Reconciliation.Interfaces.ReconciliationTypes;
using eBalanceKitBusiness.Structures.DbMapping;
using eBalanceKitBusiness.Structures.ValueTree;
using eBalanceKitBusiness.Structures.XbrlElementValue;
using System.Collections.Generic;
using eBalanceKitBase;
using System.Linq;
using System.Reflection;

namespace eBalanceKitBusiness.Export {

    public static class XbrlExporter {
        
        // XBRL
        private static readonly XmlQualifiedName Xbrli = new XmlQualifiedName("xbrli", "http://www.xbrl.org/2003/instance");
        private static readonly XmlQualifiedName Xbrldi = new XmlQualifiedName("xbrldi", "http://xbrl.org/2006/xbrldi");
        private static readonly XmlQualifiedName Xlink = new XmlQualifiedName("xlink", "http://www.w3.org/1999/xlink");
        private static readonly XmlQualifiedName Link = new XmlQualifiedName("link", "http://www.xbrl.org/2003/linkbase");
        private static readonly XmlQualifiedName Xsi = new XmlQualifiedName("xsi", "http://www.w3.org/2001/XMLSchema-instance");
        private static readonly XmlQualifiedName Iso4217 = new XmlQualifiedName("iso4217", "http://www.xbrl.org/2003/iso4217");
        private static readonly XmlQualifiedName Xhtml = new XmlQualifiedName("xhtml", "http://www.w3.org/1999/xhtml");
        
        // Elster
        private static readonly XmlQualifiedName Ebilanz = new XmlQualifiedName("ebilanz", "http://rzf.fin-nrw.de/RMS/EBilanz/2009/XMLSchema");

        private static string _contextNameInstant;
        private static string _contextNamePeriod;
        
        private const string UnitNameMonetary = "EUR";
        private const string UnitNamePure = "unit_pure";
        private const string ElsterEntity = "http://www.rzf-nrw.de/Steuernummer";

        private static readonly XmlWriterSettings XmlSettings = new XmlWriterSettings {
            Indent = true,
            IndentChars = ("    "),
            Encoding = Encoding.GetEncoding("iso-8859-15")
        };

        #region GetElsterXml
        /// <summary>
        /// Returns a StringBuilder with the elster XML data (contains the XBRL data in the "Nutzdaten" tag).
        /// </summary>
        public static StringBuilder GetElsterXml(Document document, bool generateTestHeader) {
            var result = new StringBuilder();
            using (var writer = XmlWriter.Create(result, XmlSettings)) {
                #region force iso-8859-15 encoding
                var innerWriterPropInfo = writer.GetType().GetProperty(
                    "InnerWriter", BindingFlags.NonPublic | BindingFlags.Instance);

                var innerWriter = (XmlWriter) innerWriterPropInfo.GetValue(writer, null);

                var encodingFieldInfo = innerWriter.GetType().GetField(
                    "encoding", BindingFlags.NonPublic | BindingFlags.Instance);

                Debug.Assert(encodingFieldInfo != null, "encodingFieldInfo != null");

                encodingFieldInfo.SetValue(innerWriter, Encoding.GetEncoding("iso-8859-15"));
                #endregion

                _contextNameInstant = "I-" + document.FinancialYear.FYear;
                _contextNamePeriod = "D-" + document.FinancialYear.FYear;

                string entityId = GetEntityId(document);

                // get data part, needed to receive size
                var sbData = new StringBuilder();
                using (XmlWriter wData = XmlWriter.Create(sbData, XmlSettings)) {
                    wData.WriteStartDocument();
                    WriteData(document, wData, ElsterEntity, entityId);
                    wData.WriteEndDocument();
                }

                writer.WriteStartDocument();
                writer.WriteStartElement("Elster", "http://www.elster.de/2002/XMLSchema");
                WriteTransferHeader(writer, document,
                                    sbData.Length - "<?xml version=\"1.0\" encoding=\"utf-16\"?>".Length,
                                    generateTestHeader);
                WriteData(document, writer, ElsterEntity, entityId);
                writer.WriteEndElement(); // Elster
                writer.WriteEndDocument();
            }

            return result;
        }
        #endregion GetElsterXml
        
        #region ExportXbrl
        /// <summary>
        /// Exports the XBRL data into the specified file.
        /// </summary>
        public static void ExportXbrl(string file, Document document, string entityScheme = ElsterEntity) {
            using (var writer = XmlWriter.Create(file, XmlSettings)) {
                _contextNameInstant = "I-" + document.FinancialYear.FYear;
                _contextNamePeriod = "D-" + document.FinancialYear.FYear;

                string entityId = GetEntityId(document);

                WriteXbrlPart(document, writer, entityScheme, entityId);
                writer.WriteEndDocument();
            }
        }
        #endregion ExportXbrl
        
        #region GetEntityId
        /// <summary>
        /// Returns the entity id from as fact 'genInfo.company.id.idNo.type.companyId.ST13'.
        /// </summary>
        private static string GetEntityId(Document document) {
            var idNo = document.ValueTreeGcd.GetValue("de-gcd_genInfo.company.id.idNo");
            var xbrlElementValueTuple = idNo as XbrlElementValue_Tuple;
            if (xbrlElementValueTuple == null) throw new Exception("Fact 'de-gcd_genInfo.company.id.idNo' is no tuple elment.");

            var st13 = xbrlElementValueTuple.Items[0].Values["de-gcd_genInfo.company.id.idNo.type.companyId.ST13"];
            if (st13 == null)
                throw new Exception("Could not find fact 'de-gcd_genInfo.company.id.idNo.type.companyId.ST13'.");
            return st13.HasValue ? st13.Value.ToString() : string.Empty;
        }
        #endregion GetEntityId
        
        /// <summary>
        /// 
        /// </summary>
        private static void WriteTransferHeader(XmlWriter w, Document document, int dataSize, bool generateTestHeader) {
            w.WriteStartElement("TransferHeader");
            w.WriteAttributeString("version", "8");

            w.WriteElementString("Verfahren", "ElsterBilanz");
            w.WriteElementString("DatenArt", "Bilanz");
            w.WriteElementString("Vorgang", "send-Auth");

            // ReSharper disable ConvertIfStatementToConditionalTernaryExpression
            if (generateTestHeader) {
                //w.WriteElementString("Testmerker", "700000004"); // nur Test der Datenübertragung (Annahmestelle), keine weitere Verarbeitung
                w.WriteElementString("Testmerker", "700000001"); // nur Test der Datenübertragung (Rechenzentrum), keine weitere Verarbeitung
                //w.WriteElementString("Testmerker", "700000004"); // Test mit gültiger Serverantwort
            } else {
                //w.WriteElementString("Testmerker", "700000001"); // Testmerker für Pilotierung
                w.WriteElementString("Testmerker", "000000000");
            }
            // ReSharper restore ConvertIfStatementToConditionalTernaryExpression

            w.WriteElementString("HerstellerID", "00855"); // Test Hersteller-ID 74931
            w.WriteElementString("DatenLieferant", GetSenderInfo(document));

            w.WriteStartElement("Datei");

            w.WriteElementString("Verschluesselung", "PKCS#7v1.5");
            w.WriteElementString("Kompression", "GZIP");
            w.WriteElementString("DatenGroesse", dataSize.ToString(CultureInfo.InvariantCulture));
            w.WriteElementString("TransportSchluessel", "hier ist ein individueller Transportschlüssel");

            w.WriteEndElement(); // Datei

            w.WriteElementString("VersionClient", VersionInfo.Instance.CurrentVersion);

            w.WriteEndElement(); // TransferHeader
        }

        /// <summary>
        /// Returns a string which contains information about the sender.
        /// </summary>
        private static string GetSenderInfo(Document document) {
            var value = document.ValueTreeGcd.Root.Values["de-gcd_genInfo.company.id.location.street"];
            string street = (value.HasValue ? value.Value.ToString() : "");

            value = document.ValueTreeGcd.Root.Values["de-gcd_genInfo.company.id.location.houseNo"];
            string houseNo = (value.HasValue ? value.Value.ToString() : "");

            value = document.ValueTreeGcd.Root.Values["de-gcd_genInfo.company.id.location.zipCode"];
            string zip = (value.HasValue ? value.Value.ToString() : "");

            value = document.ValueTreeGcd.Root.Values["de-gcd_genInfo.company.id.location.city"];
            string city = (value.HasValue ? value.Value.ToString() : "");

            // max 256 characters allowed for sender info
            return
                (!string.IsNullOrEmpty(document.Company.Name) ? Utils.StringUtils.Left(document.Company.Name, 64) : "") + ";" +
                (!string.IsNullOrEmpty(street) ? Utils.StringUtils.Left(street, 94) : "") + ";" +
                (!string.IsNullOrEmpty(houseNo) ? Utils.StringUtils.Left(houseNo, 9) : "") + ";" +
                (!string.IsNullOrEmpty(zip) ? Utils.StringUtils.Left(zip, 5) : "") + ";" +
                (!string.IsNullOrEmpty(city) ? Utils.StringUtils.Left(city, 64) : "");
        }

        /// <summary>
        /// Returns a string which contains information about the document creator.
        /// </summary>
        private static string GetCreatorInfo(Document document) {
            var value = document.ValueTreeGcd.Root.Values["de-gcd_genInfo.company.id.location.street"];
            string street = (value.HasValue ? value.Value.ToString() : "");

            value = document.ValueTreeGcd.Root.Values["de-gcd_genInfo.company.id.location.houseNo"];
            string houseNo = (value.HasValue ? value.Value.ToString() : "");

            value = document.ValueTreeGcd.Root.Values["de-gcd_genInfo.company.id.location.zipCode"];
            string zip = (value.HasValue ? value.Value.ToString() : "");

            value = document.ValueTreeGcd.Root.Values["de-gcd_genInfo.company.id.location.city"];
            string city = (value.HasValue ? value.Value.ToString() : "");

            // max 256 characters allowed for sender info
            return
                (!string.IsNullOrEmpty(document.Company.Name) ? Utils.StringUtils.Left(document.Company.Name, 64) : "") + ";" +
                (!string.IsNullOrEmpty(street) ? Utils.StringUtils.Left(street, 94) : "") + ";" +
                (!string.IsNullOrEmpty(houseNo) ? Utils.StringUtils.Left(houseNo, 9) : "") + ";" +
                (!string.IsNullOrEmpty(zip) ? Utils.StringUtils.Left(zip, 5) : "") + ";" +
                (!string.IsNullOrEmpty(city) ? Utils.StringUtils.Left(city, 64) : "");
        }

        /// <summary>
        /// 
        /// </summary>
        private static void WriteData(Document document, XmlWriter w, string entityScheme, string entityId) {
            w.WriteStartElement("DatenTeil");
            w.WriteStartElement("Nutzdatenblock");

            WriteDataHeader(document, w);

            w.WriteStartElement("Nutzdaten");
            w.WriteStartElement(Ebilanz.Name, "EBilanz", Ebilanz.Namespace);
            w.WriteAttributeString("version", "000001");
            w.WriteAttributeString("xmlns", Ebilanz.Name, null, Ebilanz.Namespace);

            var date = (document.ValueTreeGcd.Root.Values["de-gcd_genInfo.report.period.balSheetClosingDate"] as XbrlElementValue_Date).DateValue.Value;
            w.WriteElementString("stichtag", Ebilanz.Namespace, date.ToString("yyyyMMdd"));
            
            WriteXbrlPart(document, w, entityScheme, entityId);

            w.WriteEndElement(); // EBilanz

            w.WriteEndElement(); //  Nutzdaten
            
            w.WriteEndElement();// Nutzdatenblock
            w.WriteEndElement(); // DatenTeil
        }

        /// <summary>
        /// 
        /// </summary>
        private static void WriteDataHeader(Document document, XmlWriter w) {
            w.WriteStartElement("NutzdatenHeader");
            w.WriteAttributeString("version", "10");
            w.WriteElementString("NutzdatenTicket", "0001");

            w.WriteStartElement("Empfaenger");
            w.WriteAttributeString("id", "F");

            string fId = null;
            if (!document.ValueTreeGcd.Root.Values.ContainsKey("de-gcd_genInfo.company.id.idNo")) throw new Exception("Could not find tuple 'genInfo.company.id.idNo'.");
            var idNoNode = document.ValueTreeGcd.Root.Values["de-gcd_genInfo.company.id.idNo"] as XbrlElementValue_Tuple;
            if (!idNoNode.Items[0].Values.ContainsKey("de-gcd_genInfo.company.id.idNo.type.companyId.BF4")) throw new Exception("Could not find fact 'genInfo.company.id.idNo.type.companyId.BF4'.");
            var value = idNoNode.Items[0].Values["de-gcd_genInfo.company.id.idNo.type.companyId.BF4"];
            if (value.HasValue) {
                fId = value.Value.ToString();
            } else {
                // extract BF4 number from ST13 (first 4 characters)
                if (!idNoNode.Items[0].Values.ContainsKey("de-gcd_genInfo.company.id.idNo.type.companyId.ST13")) throw new Exception("Could not find fact 'genInfo.company.id.idNo.type.companyId.ST13'.");
                var stId = idNoNode.Items[0].Values["de-gcd_genInfo.company.id.idNo.type.companyId.ST13"];
                if (stId.HasValue && stId.Value.ToString().Length >= 4)
                    fId = stId.Value.ToString().Substring(0, 4);
            }
            w.WriteString(fId);
            
            w.WriteEndElement(); // Empfaenger

            w.WriteStartElement("Hersteller");
            w.WriteElementString("ProduktName", "eBilanz-Kit");
            w.WriteElementString("ProduktVersion", "V" + VersionInfo.Instance.CurrentVersion);
            w.WriteEndElement(); // Hersteller

            w.WriteElementString("DatenLieferant", GetSenderInfo(document));
            w.WriteEndElement(); // NutzdatenHeader
        }

        /// <summary>
        /// 
        /// </summary>
        private static void WriteXbrlPart(Document document, XmlWriter writer, string entityScheme, string entityId) {
            
            var namespaces = new HashSet<XmlQualifiedName>();
            
            writer.WriteStartElement("xbrli", "xbrl", Xbrli.Namespace);

            writer.WriteAttributeString("xmlns", Link.Name, null, Link.Namespace);
            writer.WriteAttributeString("xmlns", Xlink.Name, null, Xlink.Namespace);
            writer.WriteAttributeString("xmlns", Xhtml.Name, null, Xhtml.Namespace);
            writer.WriteAttributeString("xmlns", Xsi.Name, null, Xsi.Namespace);
            writer.WriteAttributeString("xmlns", Iso4217.Name, null, Iso4217.Namespace);
            writer.WriteAttributeString("xmlns", Xbrldi.Name, null, Xbrldi.Namespace);

            foreach (var elem in document.MainTaxonomy.Elements.Values) {
                if (!namespaces.Contains(elem.TargetNamespace)) {
                    namespaces.Add(elem.TargetNamespace);
                    writer.WriteAttributeString("xmlns", elem.TargetNamespace.Name, null, elem.TargetNamespace.Namespace);
                }
            }

            // insert schema refs
            InsertSchemaRef(writer, document.GcdTaxonomy.SchemaRef);
            InsertSchemaRef(writer, document.MainTaxonomy.SchemaRef);

            Debug.Assert(document.FinancialYear.BalSheetClosingDate != null, "document.FinancialYear.BalSheetClosingDate != null");
            Debug.Assert(document.FinancialYear.FiscalYearBegin != null, "document.FinancialYear.FiscalYearBegin != null");
            Debug.Assert(document.FinancialYear.FiscalYearEnd != null, "document.FinancialYear.FiscalYearEnd != null");

            var instantDate = document.FinancialYear.BalSheetClosingDate.Value;
            var periodStartDate = document.FinancialYear.FiscalYearBegin.Value;
            var periodEndDate = document.FinancialYear.FiscalYearEnd.Value;
            
            AddInstantContext(instantDate, writer, entityScheme, entityId, _contextNameInstant);
            AddPeriodContext(periodStartDate, periodEndDate, writer, entityScheme, entityId, _contextNamePeriod);

            foreach (var hyperCube in document.GetHyperCubes()) {
                var v = document.ValueTreeMain.GetValue(hyperCube.Root.Element.Id);

                if (v == null || !v.IsReportable || !document.TaxonomyPart.ReportParts.Values.Any(part => part.IsSelected == true && part.TaxonomyElements.Contains(v.Element.Id))) {
                    continue;
                }
                foreach (var context in hyperCube.GetScenarioContexts()) {
                    AddScenarioContext(periodStartDate, periodEndDate, writer, entityScheme, entityId, context);
                }
            }
            
            // add unit element "eur"
            writer.WriteStartElement("unit", Xbrli.Namespace);
            writer.WriteAttributeString("id", UnitNameMonetary);
            writer.WriteStartElement("measure", Xbrli.Namespace);
            writer.WriteString(Iso4217.Name + ":EUR");
            writer.WriteEndElement();
            writer.WriteEndElement();

            // add unit element "int"
            writer.WriteStartElement("unit", Xbrli.Namespace);
            writer.WriteAttributeString("id", UnitNamePure);
            writer.WriteStartElement("measure", Xbrli.Namespace);
            writer.WriteString(Xbrli.Name + ":pure");
            writer.WriteEndElement();
            writer.WriteEndElement();

            WriteHyperCubeItems(document, writer);
            WriteGAAP(writer, document);
            WriteGCD(writer, document);

            writer.WriteEndElement();
        }
        private static void WriteHyperCubeItems(Document document, XmlWriter w) {
            foreach (var hyperCube in document.GetHyperCubes()) {

                var v = document.ValueTreeMain.GetValue(hyperCube.Root.Element.Id);
                if (v == null || !v.IsReportable || !document.TaxonomyPart.ReportParts.Values.Any(part => part.IsSelected == true && part.TaxonomyElements.Contains(v.Element.Id))) {
                    continue;
                }


                foreach (var item in hyperCube.Items.Items) {
                    if (item.HasValue) {
                        var elem = document.MainTaxonomy.Elements[item.PrimaryDimensionValue.Id];
                        w.WriteStartElement(item.PrimaryDimensionValue.Name, elem.TargetNamespace.Namespace);
                        
                        switch (elem.ValueType) {
                            case Taxonomy.Enums.XbrlElementValueTypes.Monetary:
                                w.WriteAttributeString("decimals", "2");
                                w.WriteAttributeString("contextRef", item.Context.Name);
                                w.WriteAttributeString("unitRef", UnitNameMonetary);
                                w.WriteString(Convert.ToDecimal(item.Value).ToString("0.00").Replace(",", "."));
                                break;

                            case Taxonomy.Enums.XbrlElementValueTypes.String:
                                w.WriteAttributeString("contextRef", item.Context.Name);
                                w.WriteString(item.Value.ToString());
                                break;

                            default:
                                w.WriteAttributeString("contextRef", item.Context.Name);
                                w.WriteString(item.Value.ToString());
                                break;
                        }
       
                        w.WriteEndElement();
                    }
                }
            }
        }

        #region AddScenarioContext
        private static void AddScenarioContext(DateTime periodStart, DateTime periodEnd, XmlWriter writer,
                                               string entityScheme, string entityId, IScenarioContext context) {

            writer.WriteStartElement("context", Xbrli.Namespace);
            writer.WriteAttributeString("id", context.Name);

            writer.WriteStartElement("entity", Xbrli.Namespace);
            writer.WriteStartElement("identifier", Xbrli.Namespace);
            writer.WriteAttributeString("scheme", entityScheme);
            writer.WriteString(entityId);
            writer.WriteEndElement();
            writer.WriteEndElement();

            writer.WriteStartElement("period", Xbrli.Namespace);

            writer.WriteStartElement("startDate", Xbrli.Namespace);
            writer.WriteString(periodStart.ToString("yyyy-MM-dd"));
            writer.WriteEndElement();

            writer.WriteStartElement("endDate", Xbrli.Namespace);
            writer.WriteString(periodEnd.ToString("yyyy-MM-dd"));
            writer.WriteEndElement();

            writer.WriteEndElement(); // period

            writer.WriteStartElement("scenario", Xbrli.Namespace);
            foreach (var member in context.Members) {
                writer.WriteStartElement("explicitMember", Xbrldi.Namespace);
                writer.WriteAttributeString("dimension", member.Dimension.TargetNamespace.Name + ":" + member.Dimension.Name);
                writer.WriteString(member.Dimension.TargetNamespace.Name + ":" + member.Member.Name);
                writer.WriteEndElement();
            }
            writer.WriteEndElement(); // scenario

            writer.WriteEndElement(); // context

        }
        #endregion AddScenarioContext
        
        #region AddPeriodContext
        private static void AddPeriodContext(DateTime periodStart, DateTime periodEnd, XmlWriter writer,
                                             string entityScheme, string entityId, string contextName) {
            
            writer.WriteStartElement("context", Xbrli.Namespace);
            writer.WriteAttributeString("id", contextName);

            writer.WriteStartElement("entity", Xbrli.Namespace);
            writer.WriteStartElement("identifier", Xbrli.Namespace);
            writer.WriteAttributeString("scheme", entityScheme);
            writer.WriteString(entityId);
            writer.WriteEndElement();
            writer.WriteEndElement();

            writer.WriteStartElement("period", Xbrli.Namespace);

            writer.WriteStartElement("startDate", Xbrli.Namespace);
            writer.WriteString(periodStart.ToString("yyyy-MM-dd"));
            writer.WriteEndElement();

            writer.WriteStartElement("endDate", Xbrli.Namespace);
            writer.WriteString(periodEnd.ToString("yyyy-MM-dd"));
            writer.WriteEndElement();

            writer.WriteEndElement(); // period
            writer.WriteEndElement(); // context
        }
        #endregion AddPeriodContext
        
        #region AddInstantContext
        private static void AddInstantContext(DateTime instantDate, XmlWriter writer, string entityScheme,
                                              string entityId, string contextName) {
            writer.WriteStartElement("context", Xbrli.Namespace);
            writer.WriteAttributeString("id", contextName);

            writer.WriteStartElement("entity", Xbrli.Namespace);
            writer.WriteStartElement("identifier", Xbrli.Namespace);
            writer.WriteAttributeString("scheme", entityScheme);
            writer.WriteString(entityId);
            writer.WriteEndElement();
            writer.WriteEndElement();

            writer.WriteStartElement("period", Xbrli.Namespace);
            writer.WriteStartElement("instant", Xbrli.Namespace);
            writer.WriteString(instantDate.ToString("yyyy-MM-dd"));
            writer.WriteEndElement();
            writer.WriteEndElement();

            writer.WriteEndElement(); // context
        }
        #endregion AddInstantContext
        
        private static void InsertSchemaRef(XmlWriter w, string url) {
            w.WriteStartElement("schemaRef", Link.Namespace);
            w.WriteAttributeString("type", Xlink.Namespace, "simple");
            w.WriteAttributeString("href", Xlink.Namespace, url);
            w.WriteEndElement();
        }

        private static void SetNotReportable(Taxonomy.Interfaces.PresentationTree.IPresentationTreeNode presentationTreeNode, Document document) {
            var valueTreeEntry = document.ValueTreeMain.GetValue(presentationTreeNode.Element.Id);
            if (valueTreeEntry != null) {
                    valueTreeEntry.IsReportable = false;
            }

            if (presentationTreeNode.IsHypercubeContainer) {
                // Otherwise there would be a problem because HyperCube child nodes are named equal to the taxonomy nodes (eg. de-fi_bsBanks.ass.receivBanks)
                return;
            }

            foreach (Taxonomy.Interfaces.PresentationTree.IPresentationTreeNode child in presentationTreeNode.Children) {
                // set each child as not reportable
                SetNotReportable(child, document);
            }
        }

        private static void SetReportable(Taxonomy.Interfaces.PresentationTree.IPresentationTreeNode presentationTreeNode, Document document) {
            
            var valueTreeEntry = document.ValueTreeMain.GetValue(presentationTreeNode.Element.Id);
            if (valueTreeEntry != null) {
                    valueTreeEntry.IsReportable = true;
            }

            if (presentationTreeNode.IsHypercubeContainer || presentationTreeNode.Element.NotPermittedForFiscal) {
                // Otherwise there would be a problem because HyperCube child nodes are named equal to the taxonomy nodes (eg. de-fi_bsBanks.ass.receivBanks)
                return;
            }

            foreach (Taxonomy.Interfaces.PresentationTree.IPresentationTreeNode child in presentationTreeNode.Children) {
                // set each child as reportable
                SetReportable(child, document);
            }
        }

        private static void WriteGAAP(XmlWriter w, Document document) {
            ResetIsReportableState(document.ValueTreeMain.Root);

            // UpdateIsReportableState for the selected report parts
            foreach (var reportParts in document.TaxonomyPart.ReportPartsToSend) {
                

                if (reportParts.TaxonomyElements != null) {

                    // for each entry in reportPart.Value
                    foreach (var elementId in reportParts.TaxonomyElements) {

                        var presentationTrees =
                            document.MainTaxonomy.PresentationTrees.Where(
                                pTree => pTree.Nodes.Any(node => node.Element.Id.Equals(elementId))).ToList();

                        // Iterate all presentation trees (usualy only 1) and set the IsReportable flag
                        foreach (var presentationTree in presentationTrees) {
                            foreach (
                                var rootEntry in
                                    presentationTree.Nodes.Where(node => node.Element.Id.Equals(elementId))) {

                                UpdateIsReportableState(rootEntry, document.ValueTreeMain.Root);
                            }
                        }
                    }
                }
            }

            // UpdateIsReportableState for parts that MUST be sent (NIL)
            foreach (var ptree in document.GaapPresentationTrees.Values.Where(pTree => pTree.Nodes.Any(root => root.Element.IsMandatoryField)).ToList()) {
                foreach (var rootEntry in ptree.RootEntries) {
                    UpdateIsReportableState(rootEntry, document.ValueTreeMain.Root);
                }
            }

            InsertItems(w, document.ValueTreeMain.Root);

            if (document.TaxonomyPart.ReportPartsToSend.Contains(document.TaxonomyPart.PartAccountDetails)) {
                foreach (var balanceList in document.BalanceLists)
                    InsertAccountBalances(w, balanceList, document);
            }

            if (document.TaxonomyPart.ReportPartsToSend.Contains(document.TaxonomyPart.PartReconciliation)) {
                InsertReconciliationInfo(w, document.ValueTreeMain.Root, document);
            }
        }
        
        private static void WriteGCD(XmlWriter w, Document document) {
            ResetIsReportableState(document.ValueTreeGcd.Root);

            foreach (var ptree in document.GcdTaxonomy.PresentationTrees) {
                foreach (var rootEntry in ptree.RootEntries) {
                    UpdateIsReportableState(rootEntry, document.ValueTreeGcd.Root);
                }
            }

            InsertItems(w, document.ValueTreeGcd.Root);
        }

        private static void ResetIsReportableState(ValueTreeNode root) {
            foreach (var value in root.Values.Values) {
                value.IsReportable = false;
                if (value is XbrlElementValue_Tuple) {
                    foreach (ValueTreeNode child in (value as XbrlElementValue_Tuple).Items) {
                        ResetIsReportableState(child);
                    }
                }
            }
        }
        
        private static bool UpdateIsReportableState(Taxonomy.Interfaces.PresentationTree.IPresentationTreeNode root, ValueTreeNode vroot) {

            if (vroot.Values.ContainsKey(root.Element.Id)) {

                // value element
                var value = vroot.Values[root.Element.Id];
                value.IsReportable |= IsReportableItem(value);

                switch (root.Element.ValueType) {
                    case Taxonomy.Enums.XbrlElementValueTypes.Tuple:
                        foreach (var child in root.Children)
                            foreach (ValueTreeNode item in (value as XbrlElementValue_Tuple).Items)
                                value.IsReportable |= UpdateIsReportableState(child as Taxonomy.Interfaces.PresentationTree.IPresentationTreeNode, item);
                        break;

                    default:
                        foreach (var child in root.Children)
                            if (child is Taxonomy.Interfaces.PresentationTree.IPresentationTreeNode) 
                                value.IsReportable |= UpdateIsReportableState(child as Taxonomy.Interfaces.PresentationTree.IPresentationTreeNode, vroot);
                        break;
                }

                return value.IsReportable;

            } else {
                // abstract element
                bool isReportable = false;
                foreach (var child in root.Children) {
                    if (child is Taxonomy.Interfaces.PresentationTree.IPresentationTreeNode) {
                        isReportable |= UpdateIsReportableState(child as Taxonomy.Interfaces.PresentationTree.IPresentationTreeNode, vroot);
                    }
                }
                return isReportable;
            }
        }

        private static void InsertAccountBalances(XmlWriter w, IBalanceList balanceList, Document document) {

            if (!(document.MainTaxonomy.Elements.ContainsKey("de-gaap-ci_detailedInformation.accountBalances") &&
                document.MainTaxonomy.Elements.ContainsKey("de-gaap-ci_detailedInformation.accountbalances.positionName") &&
                document.MainTaxonomy.Elements.ContainsKey("de-gaap-ci_detailedInformation.accountbalances.accountNumber") &&
                document.MainTaxonomy.Elements.ContainsKey("de-gaap-ci_detailedInformation.accountbalances.accountDescription") &&
                document.MainTaxonomy.Elements.ContainsKey("de-gaap-ci_detailedInformation.accountbalances.amount"))) return;

            var ns =
                document.MainTaxonomy.Elements["de-gaap-ci_detailedInformation.accountBalances"].TargetNamespace;

            foreach (var entry in balanceList.AssignedItems) {
                if (entry is IAccountGroup) {
                    if (!entry.SendBalance) continue;
                    IAccountGroup group = entry as IAccountGroup;
                    foreach (var account in group.Items) 
                        WriteAccountBalanceLine(w, account, ns, document);
                } else {
                    if (!entry.SendBalance) continue;
                    WriteAccountBalanceLine(w, entry, ns, document);
                }
            }
        }

        private static void WriteAccountBalanceLine(XmlWriter w, IBalanceListEntry entry, XmlQualifiedName gaapNamespace, Document document) {
            var assignedElementId = (entry.AssignedElement == null && entry is IAccount) ? (entry as IAccount).AccountGroup.AssignedElementId : entry.AssignedElementId;
            string xbrlName = document.TaxonomyIdManager.GetElement(assignedElementId).Name;
            w.WriteStartElement("detailedInformation.accountBalances", gaapNamespace.Namespace);
            WriteElement(w, document.MainTaxonomy.Elements["de-gaap-ci_detailedInformation.accountbalances.positionName"], xbrlName);
            WriteElement(w, document.MainTaxonomy.Elements["de-gaap-ci_detailedInformation.accountbalances.accountNumber"], entry.Number);
            WriteElement(w, document.MainTaxonomy.Elements["de-gaap-ci_detailedInformation.accountbalances.accountDescription"], entry.Name);
            WriteElement(w, document.MainTaxonomy.Elements["de-gaap-ci_detailedInformation.accountbalances.amount"], entry.Amount);
            w.WriteEndElement();
        }

        private static void InsertReconciliationInfo(XmlWriter w, ValueTreeNode root, Document document) {

            // section is only allowed for commercial balances
            if (!document.IsCommercialBalanceSheet) return;

            Action<string> writeTransferKind = (choice) => {
                var elem = document.MainTaxonomy.Elements["de-gaap-ci_hbst.transfer.kind"];
                var itemElem = document.MainTaxonomy.Elements["de-gaap-ci_" + choice];
                if (choice != null) {
                    w.WriteStartElement(elem.Name, elem.TargetNamespace.Namespace);
                    WriteContextAttribute(w, elem);

                    w.WriteStartElement(choice, elem.TargetNamespace.Namespace);
                    WriteContextAttribute(w, itemElem);
                    w.WriteEndElement();

                    w.WriteEndElement();
                } else {
                    if (elem.IsMandatoryField) {
                        w.WriteStartElement(elem.Name, elem.TargetNamespace.Namespace);
                        w.WriteAttributeString("nil", Xsi.Namespace, "true");
                        WriteContextAttribute(w, elem);
                        w.WriteEndElement();
                    }
                }
            };

            var hbstNs = document.MainTaxonomy.Elements["de-gaap-ci_hbst.transfer"].TargetNamespace;
            foreach (var reconciliation in document.ReconciliationManager.Reconciliations) {
                WriteReconciliation(w, document, reconciliation, hbstNs, writeTransferKind);
            }
            WriteReconciliation(w, document, document.ReconciliationManager.PreviousYearValues, hbstNs, writeTransferKind);
        }

        private static void WriteReconciliation(XmlWriter w, Document document, IReconciliation reconciliation,
                                                XmlQualifiedName hbstNs, Action<string> writeTransferKind) {
            w.WriteStartElement("hbst.transfer", hbstNs.Namespace);
            switch (reconciliation.TransferKind) {
                case TransferKinds.Reclassification:
                    writeTransferKind("hbst.transfer.kind.reclassification");
                    break;

                case TransferKinds.ValueChange:
                    writeTransferKind("hbst.transfer.kind.changeValue");
                    break;

                case TransferKinds.ReclassificationWithValueChange:
                    writeTransferKind("hbst.transfer.kind.reclassificationChangeValue");
                    break;
            }

            int transactionNumber = 0;

            // balance sheet total assets items 
            foreach (var transaction in reconciliation.Transactions.Where(t => t.Position != null && t.Position.IsBalanceSheetAssetsPosition)) {
                //w.WriteStartElement("hbst.transfer.bsAss", hbstNs.Namespace);
                //WriteElement(w, document.MainTaxonomy.Elements["de-gaap-ci_hbst.transfer.bsAss.name"], transaction.Position.Name);
                //WriteElement(w,
                //             document.MainTaxonomy.Elements["de-gaap-ci_hbst.transfer.bsAss.changeValueActualPeriod"],
                //             reconciliation is IPreviousYearValues ? null : transaction.Value);
                //WriteElement(w,
                //             document.MainTaxonomy.Elements["de-gaap-ci_hbst.transfer.bsAss.changeValuePreviousPeriod"],
                //             reconciliation is IPreviousYearValues ? transaction.Value : null);
                //w.WriteEndElement();

                WriteReconEntry(w, transaction.Position, transaction.Value, hbstNs, document, reconciliation is IPreviousYearValues, "bsAss", reconciliation, transactionNumber);
                transactionNumber++;
            }

            transactionNumber = 0;

            // balance sheet equity and liabilities items
            foreach (var transaction in reconciliation.Transactions.Where(t => t.Position != null && t.Position.IsBalanceSheetLiabilitiesPosition)) {
                WriteReconEntry(w, transaction.Position, transaction.Value, hbstNs, document, reconciliation is IPreviousYearValues, "bsEqLiab", reconciliation, transactionNumber);
                transactionNumber++;
            }
            transactionNumber = 0;

            if (document.TaxonomyPart.PartIncomeStatement.IsSelected == true) {
                // list income statement
                foreach (
                    var transaction in
                        reconciliation.Transactions.Where(
                            t => t.Position != null && t.Position.IsIncomeStatementPosition)) {
                    WriteReconEntry(w, transaction.Position, transaction.Value, hbstNs, document,
                                    reconciliation is IPreviousYearValues, "isChangeNetIncome", reconciliation, transactionNumber);
                    transactionNumber++;
                }
            }
            WriteElement(w, document.MainTaxonomy.Elements["de-gaap-ci_hbst.transfer.comment"], reconciliation.Comment);
            w.WriteEndElement();
        }

        private static void WriteReconEntry(XmlWriter w, Taxonomy.IElement elem, object value, XmlQualifiedName hbstNs, Document document, bool reconIsPrevYear, string type, IReconciliation reconciliation, int transactionNumber) {
            w.WriteStartElement("hbst.transfer." + type, hbstNs.Namespace);
            WriteElement(w, document.MainTaxonomy.Elements["de-gaap-ci_hbst.transfer." + type + ".name"],
                         elem.TargetNamespace.Name + ":" + elem.Name);
            WriteElement(w,
                         document.MainTaxonomy.Elements["de-gaap-ci_hbst.transfer." + type + ".changeValueActualPeriod"],
                         reconIsPrevYear ? null : value);
            if (!type.Equals("isChangeNetIncome")) {

                WriteElement(w,
                             document.MainTaxonomy.Elements[
                                 "de-gaap-ci_hbst.transfer." + type + ".changeValuePreviousPeriod"],
                             reconIsPrevYear ? value : null);
            }
            w.WriteEndElement();

            var elementNode = (from tree in document.GaapPresentationTrees
                     where tree.Value.Nodes.Any(node => node.Element.Id.Equals(elem.Id))
                     select tree.Value.Nodes.FirstOrDefault(node => node.Element.Id.Equals(elem.Id))).ToList();
            if (elementNode != null && elementNode.Any()) {
                Debug.Assert(elementNode.Count == 1, "found more than one IPresentationTreeNode");
                foreach (var parent in elementNode.First().Parents) {
                    if (parent.Element.ValueType != XbrlElementValueTypes.Monetary) {
                        continue;
                    }
                    object val =
                        document.ValueTreeMain.Root.Values[parent.Element.Id].ReconciliationInfo.ComputedValueTransfer.
                            ComputedValueByReconciliation.ContainsKey(reconciliation)
                            ? document.ValueTreeMain.Root.Values[parent.Element.Id].ReconciliationInfo.
                                  ComputedValueTransfer.ComputedValueByReconciliation[reconciliation]
                            : document.ValueTreeMain.Root.Values[parent.Element.Id].ReconciliationInfo.
                                  ComputedValueTransfer.Value;
                    //val =
                    //    document.ValueTreeMain.Root.Values[parent.Element.Id].ReconciliationInfo.ComputedValueTransfer.
                    //        Transactions.Any() ? document.ValueTreeMain.Root.Values[parent.Element.Id].ReconciliationInfo.ComputedValueTransfer.
                    //        Transactions.ToList()[transactionNumber].Value : val;
                    //val = document.ValueTreeMain.Root.Values[parent.Element.Id].ReconciliationInfo.
                    //    ComputedValueTransfer;
                    WriteReconEntry(w, parent.Element, val, hbstNs, document, reconIsPrevYear, type, reconciliation, transactionNumber);
                }
            }
        }

        #region WriteElement
        private static void WriteElement(XmlWriter w, Taxonomy.IElement elem, object value) {
            w.WriteStartElement(elem.Name, elem.TargetNamespace.Namespace);
            WriteContextAttribute(w, elem);

            switch (elem.ValueType) {
                case Taxonomy.Enums.XbrlElementValueTypes.Int:
                    w.WriteAttributeString("unitRef", UnitNamePure);

                    if (value == null) {
                        w.WriteAttributeString("nil", Xsi.Namespace, "true");
                    } else {
                        w.WriteAttributeString("decimals", "2");
                        w.WriteString(value.ToString());
                    }

                    break;

                case Taxonomy.Enums.XbrlElementValueTypes.Numeric:
                    w.WriteAttributeString("unitRef", UnitNamePure);

                    if (value == null) {
                        w.WriteAttributeString("nil", Xsi.Namespace, "true");
                    } else {
                        w.WriteAttributeString("precision", "INF");
                        w.WriteString(value.ToString());
                    }

                    //w.WriteString(value.ToString().Replace(",", "."));
                    break;

                case Taxonomy.Enums.XbrlElementValueTypes.Monetary:
                    w.WriteAttributeString("unitRef", UnitNameMonetary);

                    if (value == null) {
                        w.WriteAttributeString("nil", Xsi.Namespace, "true");
                    } else {
                        w.WriteAttributeString("decimals", "2");
                        w.WriteString(((Decimal)value).ToString("0.00").Replace(",", "."));
                    }

                    break;

                case Taxonomy.Enums.XbrlElementValueTypes.Date:

                    if (value == null) {
                        w.WriteAttributeString("nil", Xsi.Namespace, "true");
                    } else {
                        w.WriteString(((DateTime)value).ToString("yyyy-MM-dd"));
                    }

                    break;

                case Taxonomy.Enums.XbrlElementValueTypes.Boolean:

                    if (value == null) {
                        w.WriteAttributeString("nil", Xsi.Namespace, "true");
                    } else {
                        w.WriteString((bool)value ? "true" : "false");
                    }

                    break;

                case Taxonomy.Enums.XbrlElementValueTypes.String:

                    if (value == null) {
                        w.WriteAttributeString("nil", Xsi.Namespace, "true");
                    } else {
                        w.WriteString(value.ToString());
                    }

                    break;

                default:

                    if (value == null) {
                        w.WriteAttributeString("nil", Xsi.Namespace, "true");
                    } else {
                        w.WriteString(value.ToString());
                    }

                    break;
            }

            w.WriteEndElement();
        }
        #endregion WriteElement

        private static void InsertItems(XmlWriter w, ValueTreeNode root) {
            
            foreach (var value in root.Values.Values) {
                if (value.Element.IsSelectionListEntry) continue;


                // TODO (DEBUG) genInfo.company.id.parent (Mutterunternehmen) wird wegen Problemen derzeit noch ignoriert
                //if (value.Element.Id.StartsWith("de-gcd_genInfo.company.id.parent")) continue;
                
                if (value is XbrlElementValue_Tuple) {
                    InsertTuple(w, value);
                } else {
                    InsertItem(w, value);
                }
            }
        }

        private static void InsertTuple(XmlWriter w, IValueTreeEntry value) {

            XbrlElementValue_Tuple tuple = value as XbrlElementValue_Tuple;

            // do not report empty lists or tuples
            if (!IsReportableItem(value)) return;
            //if (!value.IsReportable) return;

            if (tuple.Items.Count == 0) {
                if (tuple.Element.Name == "genInfo.company.id.shareholder") {
                    try {
                        LogManager.Instance.Log = false;
                        tuple.AddValue();
                        
                        
                        var item = tuple.Items[0];
                        //hack for elster client if no shareholder is present
                        foreach (var x in item.Values.Values) {
                            if (x.Element.IsMandatoryField) x.IsReportable = true;
                            if (x.Element.Name == "genInfo.company.id.shareholder.ShareDivideKey") {
                                XbrlElementValue_Tuple keyTuple = x as XbrlElementValue_Tuple;
                                foreach (var key in keyTuple.Items[0].Values) {
                                    if (key.Key == "genInfo.company.id.shareholder.ShareDivideKey.denominator") {
                                        key.Value.Value = 1;
                                    }
                                }
                            }
                        }

                        w.WriteStartElement(value.Element.Name, value.Element.TargetNamespace.Namespace);
                        InsertItems(w, item);
                        w.WriteEndElement();
                        tuple.DeleteValue(item);
                    } finally {
                        LogManager.Instance.Log = true;
                    }
                }
                else if (tuple.Element.Name.Equals("kke.unlimitedPartners") || tuple.Element.Name.Equals("kke.limitedPartners")) {
                    try {
                        LogManager.Instance.Log = false;
                        tuple.AddValue();


                        var item = tuple.Items[0];
                        foreach (var x in item.Values.Values) {
                            if (x.Element.IsMandatoryField) x.IsReportable = true;
                            if ((x as XbrlElementValue_Tuple) != null &&
                                x.Element.Name.Equals(tuple.Element.Name + ".sumEquityAccounts.kinds")) {
                                XbrlElementValue_Tuple keyTuple = x as XbrlElementValue_Tuple;
                                keyTuple.AddValue();
                                foreach (var key in keyTuple.Items[0].Values.Values) {
                                    if (key.Element.IsMandatoryField) key.IsReportable = true;
                                }
                            }
                        }

                        w.WriteStartElement(value.Element.Name, value.Element.TargetNamespace.Namespace);
                        InsertItems(w, item);
                        w.WriteEndElement();
                        tuple.DeleteValue(item);
                    } finally {
                        LogManager.Instance.Log = true;
                    }
                }

                //}
                //if (tuple.Element.Name == "genInfo.company.id.shareholder") {
                //    tuple.AddValue();

                //    var item = tuple.Items[0];
                    
                //    foreach (var x in item.Values.Values) {
                //        if (x.Element.IsMandatoryField) x.IsReportable = true;
                //    }

                //    w.WriteStartElement(value.Element.Name, ns.Namespace);
                //    w.WriteAttributeString("nil", xsi.Namespace, "true");
                //    InsertItems(w, item, ns);
                //    tuple.DeleteValue(item);
                //    w.WriteEndElement();

                //}

                //ValueTreeNode dummyItem = tuple.GetPlaceholderValue();
                
                //w.WriteStartElement(value.Element.Name, ns.Namespace);
                //w.WriteAttributeString("nil", xsi.Namespace, "true");
                //InsertItems(w, dummyItem, ns);
                //w.WriteEndElement();
            } else {
                foreach (ValueTreeNode item in tuple.Items) {
                    w.WriteStartElement(value.Element.Name, value.Element.TargetNamespace.Namespace);
                    InsertItems(w, item);
                    w.WriteEndElement();
                }
            }
        }



        private static bool IsReportableItem(IValueTreeEntry value) {
            // do not send nil value for disallowed elements
            //if (!value.HasValue && (value.Element.NotPermittedForCommercial || value.Element.NotPermittedForFiscal)) return false;
            if (!value.HasValue && (value.Element.NotPermittedForCommercial)) return false;
            
            // only for reconciliation stuff allowed
            if (value.Element.Id.Equals("de-gaap-ci_is.netIncome.collItemChangeProfitHbst") || value.Element.Id.Equals("de-fi_isBanks.netIncome.collItemChangeProfitHbst") || value.Element.Id.Equals("de-ins_isIns.nonTechnicalAccount.collItemChangeProfitHbst")) {
                value.IsReportable = false;
                return false;
            }
            
            if (value.Element.IsMandatoryField) return true;

            switch (value.Element.ValueType) {
                case Taxonomy.Enums.XbrlElementValueTypes.MultipleChoice:
                    XbrlElementValue_MultipleChoice mcValue = value as XbrlElementValue_MultipleChoice;
                    foreach (var item in mcValue.Elements) {
                        bool? isChecked = mcValue.IsChecked[item.Name].BoolValue;
                        if (isChecked.HasValue) return true;
                    }

                    // no value selected
                    return false;

                case Taxonomy.Enums.XbrlElementValueTypes.Tuple:
                    XbrlElementValue_Tuple tuple = value as XbrlElementValue_Tuple;
                    foreach (var item in tuple.Items) {
                        foreach (var itemvalue in item.Values) {
                            if (IsReportableItem(itemvalue.Value)) return true;
                        }
                    }
                    return false;

                default:
                    return value.HasValue;
            }
        }

        private static void InsertItem(XmlWriter w, IValueTreeEntry value) {

            // do not send nil value for disallowed elements
            //if (!value.HasValue && (value.Element.NotPermittedForCommercial || value.Element.NotPermittedForFiscal)) return;
            if (!value.HasValue && (value.Element.NotPermittedForCommercial)) return;
            
            switch (value.Element.ValueType) {

                // hyper cube items
                case Taxonomy.Enums.XbrlElementValueTypes.HyperCubeContainerItem:
                    // ignore
                    break;

                // single choice lists
                case Taxonomy.Enums.XbrlElementValueTypes.SingleChoice:
                    #region SingleChoice
                    if (value.Value != null) {
                        w.WriteStartElement(value.Element.Name, value.Element.TargetNamespace.Namespace);

                        WriteContextAttribute(w, value);

                        foreach (var item in ((XbrlElementValue_SingleChoice)value).Elements) {
                            if (item.Name == value.Value.ToString()) {
                                w.WriteStartElement(item.Name, item.TargetNamespace.Namespace);
                                WriteContextAttribute(w, item);
                                //if (item.Name.EndsWith(".S")) { // values "other"
                                //    w.WriteString(((XbrlElementValueBase)value).ValueOther);
                                //}
                                w.WriteEndElement();
                            }
                        }
                        w.WriteEndElement();
                    } else {
                        if (value.Element.IsMandatoryField) {
                            w.WriteStartElement(value.Element.Name, value.Element.TargetNamespace.Namespace);
                            
                            //Dirty hack for elster client if no shareholder is present
                            //if ((value.Element.Id == "de-gcd_genInfo.company.id.shareholder.legalStatus" || value.Element.Id.Equals("de-gcd_genInfo.report.id.statementType.tax")) && ((XbrlElementValue_SingleChoice)value).Elements.Count > 0) {
                            if (((XbrlElementValue_SingleChoice)value).Elements.Count > 0) {
                                var item = ((XbrlElementValue_SingleChoice)value).Elements[0];
                                w.WriteStartElement(item.Name, item.TargetNamespace.Namespace);
                                w.WriteAttributeString("nil", Xsi.Namespace, "true");
                                WriteContextAttribute(w, item);
                                w.WriteEndElement();
                            } else w.WriteAttributeString("nil", Xsi.Namespace, "true");
                            WriteContextAttribute(w, value);
                            w.WriteEndElement();
                        }
                    }

                    break;
                    #endregion SingleChoice

                // multiple choice lists
                case Taxonomy.Enums.XbrlElementValueTypes.MultipleChoice:
                    #region MultipleChoice
                    w.WriteStartElement(value.Element.Name, value.Element.TargetNamespace.Namespace);

                    WriteContextAttribute(w, value);

                    XbrlElementValue_MultipleChoice mcValue = value as XbrlElementValue_MultipleChoice;
                    foreach (var item in mcValue.Elements) {
                        bool? isChecked = mcValue.IsChecked[item.Id].BoolValue;

                        if (isChecked.HasValue || item.IsMandatoryField) {

                            w.WriteStartElement(item.Name, item.TargetNamespace.Namespace);
                            
                            WriteContextAttribute(w, item);
                            
                            if (isChecked.HasValue) {                                
                                if (isChecked.Value == true) {
                                    if (item.Id.EndsWith(".S")) { // values "other"
                                        //w.WriteString(((XbrlElementValueBase)value).ValueOther);
                                    } else {
                                        // true
                                    }
                                } else {
                                    //w.WriteString("false");
                                    w.WriteAttributeString("nil", Xsi.Namespace, "true");
                                }
                            } else {
                                w.WriteAttributeString("nil", Xsi.Namespace, "true");
                            }

                            w.WriteEndElement();
                        }
                    }
                    w.WriteEndElement();
                    break;
                    #endregion MultipleChoice

                case Taxonomy.Enums.XbrlElementValueTypes.Monetary:
                        if (value.IsReportable || value.Element.Id.StartsWith("de-gcd_genInfo.company.id.shareholder.ShareDivideKey.")) 
                            WriteElement(w, value.Element, ReportWithRealValue(value) ? value.MonetaryValue.Value : null);
                    break;

                // other
                default:
                    if (value.IsReportable || value.Element.Id.StartsWith("de-gcd_genInfo.company.id.parent.name") || value.Element.Id.StartsWith("de-gcd_genInfo.company.id.shareholder.ShareDivideKey.")) 
                        WriteElement(w, value.Element, ReportWithRealValue(value) ? value.Value : null);
                    //else if (value.Element.IsMandatoryField && !value.IsReportable) { WriteElement(w, value.Element, null);}
                    break;
            }

        }

        private static bool ReportWithRealValue(IValueTreeEntry value) {
            
            if (value.IsReportable && value.Element.IsMandatoryField && !value.Element.Id.StartsWith("de-gcd")) {
                //(value.Element.PresentationTreeNodes.First().PresentationTree as eBalanceKitBusiness.Structures.Presentation.PresentationTree).Document.TaxonomyPart.ReportPartsToSend.Where(part => part.TaxonomyElements.c)

                    var document = Manager.DocumentManager.Instance.CurrentDocument;
                    var result = false;

                    // for each part that is selected for sending
                    foreach (var reportParts in document.TaxonomyPart.ReportPartsToSend) {
                        if (reportParts.TaxonomyElements != null) {
                            // for each entry in TaxonomyElements
                            foreach (var elementId in reportParts.TaxonomyElements) {
                                // find the matching PresentationTrees
                                var presentationTrees =
                                    document.MainTaxonomy.PresentationTrees.Where(
                                        pTree => pTree.Nodes.Any(node => node.Element.Id.Equals(elementId)));
                                foreach (var matchingPresentationTree in presentationTrees) {
                                    // check if it 
                                    result |= matchingPresentationTree.Nodes.Any(node => node.Element.Id.Equals(value.Element.Id));
                                }
                            }
                        }

                    }
                    return result;
            }
            return true;
        }

        private static void WriteContextAttribute(XmlWriter w, IValueTreeEntry value) {
            WriteContextAttribute(w, value.Element);
        }

        private static void WriteContextAttribute(XmlWriter w, Taxonomy.IElement elem) {

            if (elem.PeriodType == null) return;

            // write contextRef attribute
            if (elem.PeriodType == "instant") {
                w.WriteAttributeString("contextRef", _contextNameInstant);
            } else if (elem.PeriodType == "duration") {
                w.WriteAttributeString("contextRef", _contextNamePeriod);
            } else {
                Debug.WriteLine("unknown xbrl element type: " + elem.PeriodType);
            }
        }


    }
}