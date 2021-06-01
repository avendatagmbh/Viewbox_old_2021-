using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using eBalanceKitBusiness.Structures.DbMapping;
using System.Xml;
using eBalanceKitBusiness.Structures;
using XbrlProcessor.Taxonomy;
using eBalanceKitBusiness.Structures.XbrlElementValue;
using eBalanceKitBusiness.Structures.ValueTree;
using System.Reflection;
using eBalanceKitBusiness.Attributes;
using XbrlProcessor.Taxonomy.PresentationTree;

namespace eBalanceKitBusiness.Export {

    public static class XbrlExport {

        private enum BalanceStandard {
            Fiscal,
            Commercial
        }

        private class XmlNamespace {

            /// <summary>
            /// Initializes a new instance of the <see cref="XmlNamespace"/> class.
            /// </summary>
            /// <param name="name">The name.</param>
            /// <param name="url">The URL.</param>
            public XmlNamespace(string name, string url) {
                this.Name = name;
                this.URL = url;
            }

            public string Name { get; set; }
            public string URL { get; set; }
        }

        // XBRL
        private static XmlNamespace xbrli = new XmlNamespace("xbrli", "http://www.xbrl.org/2003/instance");
        private static XmlNamespace xlink = new XmlNamespace("xlink", "http://www.w3.org/1999/xlink");
        private static XmlNamespace link = new XmlNamespace("link", "http://www.xbrl.org/2003/linkbase");
        private static XmlNamespace xsi = new XmlNamespace("xsi", "http://www.w3.org/2001/XMLSchema-instance");
        private static XmlNamespace gaap = new XmlNamespace("de-gaap-ci", "http://www.xbrl.de/taxonomies/de-gaap-ci-2010-12-16");
        private static XmlNamespace gcd = new XmlNamespace("de-gcd", "http://www.xbrl.de/taxonomies/de-gcd-2010-12-16");
        private static XmlNamespace iso4217 = new XmlNamespace("iso4217", "http://www.xbrl.org/2003/iso4217");
        private static XmlNamespace xhtml = new XmlNamespace("xhtml", "http://www.w3.org/1999/xhtml");
        
        // Elster
        private static XmlNamespace ebilanz = new XmlNamespace("ebilanz", "http://rzf.fin-nrw.de/RMS/EBilanz/2009/XMLSchema");

        private static string contextNameInstant;
        private static string contextNamePeriod;
        private static string unitNameMonetary = "EUR";
        private static string unitNamePure = "unit_pure";

        /// <summary>
        /// Gets the elster XML as StringBuilder.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <returns></returns>
        public static StringBuilder GetElsterXml(Document document) {
            StringBuilder result = new StringBuilder();
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars = ("    ");

            using (XmlWriter w = XmlWriter.Create(result, settings)) GenerateElsterXml(document, settings, w);

            // correct encoding specifikation, which is utf-8 by default when using StringBuilder for XmlWriter.
            result.Replace(Encoding.Unicode.WebName, Encoding.GetEncoding("iso-8859-15").WebName, 0, 80);

            return result;
        }

        /// <summary>
        /// Exports the elster XML data into the specified file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="document">The document.</param>
        public static void ExportElsterXml(string file, Document document) {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars = ("    ");
            settings.Encoding = Encoding.GetEncoding("iso-8859-15");

            using (XmlWriter w = XmlWriter.Create(file, settings)) GenerateElsterXml(document, settings, w);
        }

        private static void GenerateElsterXml(Document document, XmlWriterSettings settings, XmlWriter w) {
            UpdateCompanyData(document);

            BalanceStandard balanceStandard;
            var balanceStandardValue = document.ValueTreeGCD.Root.Values["genInfo.report.id.accountingStandard"];
            if (balanceStandardValue.HasValue && balanceStandardValue.Value.ToString().EndsWith(".AO")) {
                balanceStandard = XbrlExport.BalanceStandard.Fiscal;
            } else {
                balanceStandard = XbrlExport.BalanceStandard.Commercial;
            }

            contextNameInstant = "I-" + document.FinancialYear.FYear;
            contextNamePeriod = "D-" + document.FinancialYear.FYear;

            string entityScheme = "http://www.rzf-nrw.de/Steuernummer";
            string entityId = document.Company.IdNumber_ST13;

            // get data part, needed to receive size
            StringBuilder sbData = new StringBuilder();
            using (XmlWriter wData = XmlWriter.Create(sbData, settings)) {
                wData.WriteStartDocument();
                WriteData(document, wData, entityScheme, entityId, balanceStandard);
                wData.WriteEndDocument();
            }
            
            w.WriteStartDocument();
            w.WriteStartElement("Elster", "http://www.elster.de/2002/XMLSchema");
            WriteTransferHeader(w, document, sbData.Length - "<?xml version=\"1.0\" encoding=\"utf-16\"?>".Length);
            WriteData(document, w, entityScheme, entityId, balanceStandard);
            w.WriteEndElement(); // Elster
            w.WriteEndDocument();
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="w"></param>
        /// <param name="document"></param>
        /// <param name="dataSize"></param>
        private static void WriteTransferHeader(XmlWriter w, Document document, int dataSize) {
            w.WriteStartElement("TransferHeader");
            w.WriteAttributeString("version", "8");

            w.WriteElementString("Verfahren", "ElsterBilanz");
            w.WriteElementString("DatenArt", "Bilanz");
            w.WriteElementString("Vorgang", "send-Auth");
            //w.WriteElementString("Testmerker", "700000004"); // nur Test der Datenübertragung (Annahmestelle), keine weitere Verarbeitung
            //w.WriteElementString("Testmerker", "700000001"); // nur Test der Datenübertragung (Rechenzentrum), keine weitere Verarbeitung
            w.WriteElementString("Testmerker", "010000001"); // Test mit gültiger Serverantwort
            w.WriteElementString("HerstellerID", "00855"); // Test Hersteller-ID 74931
            w.WriteElementString("DatenLieferant", GetSenderInfo(document));

            w.WriteStartElement("Datei");

            w.WriteElementString("Verschluesselung", "PKCS#7v1.5");
            w.WriteElementString("Kompression", "GZIP");
            w.WriteElementString("DatenGroesse", dataSize.ToString());
            w.WriteElementString("TransportSchluessel", "hier ist ein individueller Transportschlüssel");

            w.WriteEndElement(); // Datei

            w.WriteElementString("VersionClient", Structures.DbMapping.Info.CurrentVersion);

            w.WriteEndElement(); // TransferHeader
        }

        /// <summary>
        /// Returns a string which contains information about the sender.
        /// <param name="document"></param>
        /// <returns></returns>
        private static string GetSenderInfo(Document document) {
            // max 256 characters allowed for sender info
            return
                Utils.StringUtils.Left(document.Company.Name, 64) + ";" +
                Utils.StringUtils.Left(document.Company.Location_Street, 94) + ";" +
                Utils.StringUtils.Left(document.Company.Location_HouseNumber, 9) + ";" +
                Utils.StringUtils.Left(document.Company.Location_ZIP, 5) + ";" +
                Utils.StringUtils.Left(document.Company.Location_City, 64);
        }

        /// <summary>
        /// Returns a string which contains information about the document creator.
        /// </summary>
        /// <param name="document"></param>
        /// <returns></returns>
        private static string GetCreatorInfo(Document document) {
            // max 256 characters allowed for sender info
            return 
                Utils.StringUtils.Left(document.Company.Name, 64) + ";" + 
                Utils.StringUtils.Left(document.Company.Location_Street, 94) + ";" + 
                Utils.StringUtils.Left(document.Company.Location_HouseNumber, 9) + ";" + 
                Utils.StringUtils.Left(document.Company.Location_ZIP, 5) + ";" + 
                Utils.StringUtils.Left(document.Company.Location_City, 64);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="document"></param>
        /// <param name="w"></param>
        /// <param name="entityScheme"></param>
        /// <param name="entityId"></param>
        /// <param name="w"></param>
        /// <param name="balanceStandard"></param>
        private static void WriteData(Document document, XmlWriter w, string entityScheme, string entityId, BalanceStandard balanceStandard) {
            w.WriteStartElement("DatenTeil");
            w.WriteStartElement("Nutzdatenblock");

            WriteDataHeader(document, w);

            w.WriteStartElement("Nutzdaten");
            w.WriteStartElement(ebilanz.Name, "EBilanz", ebilanz.URL);
            w.WriteAttributeString("version", "000001");
            w.WriteAttributeString("xmlns", ebilanz.Name, null, ebilanz.URL);
            w.WriteElementString("stichtag", ebilanz.URL, "20121231");
            
            WriteXbrlPart(document, w, entityScheme, entityId, balanceStandard);

            w.WriteEndElement(); // EBilanz

            w.WriteEndElement(); //  Nutzdaten
            
            w.WriteEndElement();// Nutzdatenblock
            w.WriteEndElement(); // DatenTeil
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="document"></param>
        /// <param name="w"></param>
        private static void WriteDataHeader(Document document, XmlWriter w) {
            w.WriteStartElement("NutzdatenHeader");
            w.WriteAttributeString("version", "10");
            w.WriteElementString("NutzdatenTicket", "0001");

            w.WriteStartElement("Empfaenger");
            w.WriteAttributeString("id", "F");
            w.WriteString(document.Company.IdNumber_BF4);
            w.WriteEndElement(); // Empfaenger

            w.WriteStartElement("Hersteller");
            w.WriteElementString("ProduktName", "eBilanz-Kit");
            w.WriteElementString("ProduktVersion", "V" + Structures.DbMapping.Info.CurrentVersion);
            w.WriteEndElement(); // Hersteller

            w.WriteElementString("DatenLieferant", GetSenderInfo(document));
            w.WriteEndElement(); // NutzdatenHeader
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="document"></param>
        /// <param name="w"></param>
        /// <param name="entityScheme"></param>
        /// <param name="entityId"></param>
        /// <param name="balanceStandard"></param>
        private static void WriteXbrlPart(Document document, XmlWriter w, string entityScheme, string entityId, BalanceStandard balanceStandard) {
            w.WriteStartElement("xbrli", "xbrl", xbrli.URL);

            w.WriteAttributeString("xmlns", link.Name, null, link.URL);
            w.WriteAttributeString("xmlns", xlink.Name, null, xlink.URL);
            w.WriteAttributeString("xmlns", xhtml.Name, null, xhtml.URL);
            w.WriteAttributeString("xmlns", xsi.Name, null, xsi.URL);
            w.WriteAttributeString("xmlns", gaap.Name, null, gaap.URL);
            w.WriteAttributeString("xmlns", gcd.Name, null, gcd.URL);
            w.WriteAttributeString("xmlns", iso4217.Name, null, iso4217.URL);

            // insert schema refs
            InsertSchemaRef(w, "http://www.xbrl.de/de/gcd/2010-12-16/de-gcd-2010-12-16-shell.xsd");
            InsertSchemaRef(w, "http://www.xbrl.de/de/gaap/2010-12-16/de-gaap-ci-2010-12-16-shell-fiscal.xsd");

            // add "instant" context
            w.WriteStartElement("context", xbrli.URL);
            w.WriteAttributeString("id", contextNameInstant);

            w.WriteStartElement("entity", xbrli.URL);
            w.WriteStartElement("identifier", xbrli.URL);
            w.WriteAttributeString("scheme", entityScheme);
            w.WriteString(entityId);
            w.WriteEndElement();
            //w.WriteStartElement("segment", xbrli.URL);
            //w.WriteEndElement();
            w.WriteEndElement();

            w.WriteStartElement("period", xbrli.URL);

            w.WriteStartElement("instant", xbrli.URL);

            if (document.FinancialYear.BalSheetClosingDate.HasValue) {
                w.WriteString(document.FinancialYear.BalSheetClosingDate.Value.ToString("yyyy-MM-dd"));
            }
            w.WriteEndElement();

            w.WriteEndElement();
            w.WriteEndElement();

            // add "period" context
            w.WriteStartElement("context", xbrli.URL);
            w.WriteAttributeString("id", contextNamePeriod);

            w.WriteStartElement("entity", xbrli.URL);
            w.WriteStartElement("identifier", xbrli.URL);
            w.WriteAttributeString("scheme", entityScheme);
            w.WriteString(entityId);
            w.WriteEndElement();
            //w.WriteStartElement("segment", xbrli.URL);
            //w.WriteEndElement();
            w.WriteEndElement();

            w.WriteStartElement("period", xbrli.URL);

            w.WriteStartElement("startDate", xbrli.URL);
            if (document.FinancialYear.FiscalYearBegin.HasValue) {
                w.WriteString(document.FinancialYear.FiscalYearBegin.Value.ToString("yyyy-MM-dd"));
            }
            w.WriteEndElement();

            w.WriteStartElement("endDate", xbrli.URL);
            if (document.FinancialYear.FiscalYearEnd.HasValue) {
                w.WriteString(document.FinancialYear.FiscalYearEnd.Value.ToString("yyyy-MM-dd"));
            }
            w.WriteEndElement();

            w.WriteEndElement();
            w.WriteEndElement();


            // add unit element "eur"
            w.WriteStartElement("unit", xbrli.URL);
            w.WriteAttributeString("id", unitNameMonetary);
            w.WriteStartElement("measure", xbrli.URL);
            w.WriteString(iso4217.Name + ":EUR");
            w.WriteEndElement();
            w.WriteEndElement();

            // add unit element "int"
            w.WriteStartElement("unit", xbrli.URL);
            w.WriteAttributeString("id", unitNamePure);
            w.WriteStartElement("measure", xbrli.URL);
            w.WriteString(xbrli.Name + ":pure");
            w.WriteEndElement();
            w.WriteEndElement();

            WriteGAAP(w, document, balanceStandard);
            WriteGCD(w, document, balanceStandard);

            w.WriteEndElement();
        }

        private static void InsertSchemaRef(XmlWriter w, string url) {
            w.WriteStartElement("schemaRef", link.URL);
            w.WriteAttributeString("type", xlink.URL, "simple");
            w.WriteAttributeString("href", xlink.URL, url);
            w.WriteEndElement();
        }

        private static void WriteGAAP(XmlWriter w, Document document, BalanceStandard balanceStandard) {
            InsertItems(w, document.ValueTreeGAAP.Root, gaap, balanceStandard);
        }

        private static void WriteGCD(XmlWriter w, Document document, BalanceStandard balanceStandard) {
            InsertItems(w, document.ValueTreeGCD.Root, gcd, balanceStandard);
        }

        private static void InsertItems(XmlWriter w, ValueTreeNode root, XmlNamespace ns, BalanceStandard balanceStandard) {
            foreach (var value in root.Values.Values) {

                if (value.Element.IsSelectionListEntry) continue;

                // TODO (DEBUG) genInfo.company.id.parent (Mutterunternehmen) wird wegen Probleme derzeit noch ignoriert
                if (value.Element.Name.StartsWith("genInfo.company.id.parent")) continue;
                
                if (value is XbrlElementValue_Tuple) {
                    InsertTuple(w, value, ns, balanceStandard);
                } else {
                    InsertItem(w, value, ns, balanceStandard);
                }
            }
        }

        private static void InsertTuple(XmlWriter w, IXbrlElementValue value, XmlNamespace ns, BalanceStandard balanceStandard) {

            XbrlElementValue_Tuple tuple = value as XbrlElementValue_Tuple;

            // do not report empty lists or tuples
            if (!IsReportableItem(value, balanceStandard)) return;

            foreach (ValueTreeNode item in tuple.Items) {
                w.WriteStartElement(value.Element.Name, ns.URL);
                InsertItems(w, item, ns, balanceStandard);
                w.WriteEndElement();
            }
        }

        private static bool IsReportableItem(IPresentationTreeNode root, ValueTreeNode vroot, BalanceStandard balanceStandard) {

            // function not yet tested!

            PresentationTreeNode pnode = root as PresentationTreeNode;
            IXbrlElementValue value = vroot.Values[pnode.Element.Name];

            switch (pnode.Element.ValueType) {
                case XbrlProcessor.Taxonomy.XbrlElementValue.XbrlElementValueTypes.Tuple:
                case XbrlProcessor.Taxonomy.XbrlElementValue.XbrlElementValueTypes.List:
                    foreach (var child in root.Children) {
                        XbrlElementValue_Tuple tvalue = value as XbrlElementValue_Tuple;
                        
                        // check tuple / each list entry for reportable elements
                        foreach (ValueTreeNode item in tvalue.Items) {
                            if (IsReportableItem(child, item, balanceStandard)) return true;
                        }
                    }
                    break;

                default:
                    foreach (var child in root.Children) {
                        if (IsReportableItem(child, vroot, balanceStandard)) return true;
                    }                    
                    break;
            }

            if (IsReportableItem(pnode.Value, balanceStandard)) return true;
            
            return false;
        }

        private static bool IsReportableItem(IXbrlElementValue value, BalanceStandard balanceStandard) {
            
            // do not send nil value for disallowed elements
            if (balanceStandard == BalanceStandard.Fiscal) {
                if (!value.HasValue && (value.Element.NotPermittedForCommercial || value.Element.NotPermittedForFiscal)) return false;
            } else { // balanceStandard == BalanceStandard.Commercial
                if (!value.HasValue && (value.Element.NotPermittedForCommercial)) return false;
            }

            if (value.Element.IsMandatoryField) return true;

            switch (value.Element.ValueType) {
                case XbrlProcessor.Taxonomy.XbrlElementValue.XbrlElementValueTypes.MultipleChoice:
                    XbrlElementValue_MultipleChoice mcValue = value as XbrlElementValue_MultipleChoice;
                    foreach (var item in mcValue.Elements) {
                        bool? isChecked = mcValue.IsChecked[item.Name].BoolValue;
                        if (isChecked.HasValue) return true;
                    }

                    // no value selected
                    return false;

                case XbrlProcessor.Taxonomy.XbrlElementValue.XbrlElementValueTypes.Tuple:
                case XbrlProcessor.Taxonomy.XbrlElementValue.XbrlElementValueTypes.List:
                    XbrlElementValue_Tuple tuple = value as XbrlElementValue_Tuple;
                    foreach (var item in tuple.Items) {
                        foreach (var itemvalue in item.Values) {
                            if (IsReportableItem(itemvalue.Value, balanceStandard)) return true;
                        }
                    }
                    return false;

                default:
                    return value.HasValue;
            }
        }

        private static void InsertItem(XmlWriter w, IXbrlElementValue value, XmlNamespace ns, BalanceStandard balanceStandard) {
             
            // do not send nil value for disallowed elements
            if (balanceStandard == BalanceStandard.Fiscal) {
                if (!value.HasValue && (value.Element.NotPermittedForCommercial || value.Element.NotPermittedForFiscal)) return;
            } else { // balanceStandard == BalanceStandard.Commercial
                if (!value.HasValue && (value.Element.NotPermittedForCommercial)) return;
            }

            switch (value.Element.ValueType) {

                // single choice lists
                case XbrlProcessor.Taxonomy.XbrlElementValue.XbrlElementValueTypes.SingleChoice:
                    if (value.Value != null) {
                        w.WriteStartElement(value.Element.Name, ns.URL);

                        WriteContextAttribute(w, value);

                        foreach (var item in ((XbrlElementValue_SingleChoice)value).Elements) {
                            if (item.Name == value.Value.ToString()) {
                                w.WriteStartElement(item.Name, ns.URL);
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
                            w.WriteStartElement(value.Element.Name, ns.URL);
                            w.WriteAttributeString("nil", xsi.URL, "true");
                            WriteContextAttribute(w, value);
                            w.WriteEndElement();
                        }
                    }

                    break;

                // multiple choice lists
                case XbrlProcessor.Taxonomy.XbrlElementValue.XbrlElementValueTypes.MultipleChoice:
                    w.WriteStartElement(value.Element.Name, ns.URL);

                    WriteContextAttribute(w, value);

                    XbrlElementValue_MultipleChoice mcValue = value as XbrlElementValue_MultipleChoice;
                    foreach (var item in mcValue.Elements) {
                        bool? isChecked = mcValue.IsChecked[item.Name].BoolValue;

                        if (isChecked.HasValue || item.IsMandatoryField) {

                            w.WriteStartElement(item.Name, ns.URL);
                            
                            WriteContextAttribute(w, item);
                            
                            if (isChecked.HasValue) {                                
                                if (isChecked.Value == true) {
                                    if (item.Name.EndsWith(".S")) { // values "other"
                                        //w.WriteString(((XbrlElementValueBase)value).ValueOther);
                                    } else {
                                        // true
                                    }
                                } else {
                                    w.WriteString("false");
                                    //w.WriteAttributeString("nil", xsi.URL, "true");
                                }
                            } else {
                                w.WriteAttributeString("nil", xsi.URL, "true");
                            }

                            w.WriteEndElement();
                        }
                    }
                    w.WriteEndElement();
                    break;

                // other
                default:
                    if (value.Value != null || value.Element.IsMandatoryField) {
                        w.WriteStartElement(value.Element.Name, ns.URL);

                        WriteContextAttribute(w, value);

                        switch (value.Element.ValueType) {
                            case XbrlProcessor.Taxonomy.XbrlElementValue.XbrlElementValueTypes.Int:
                                w.WriteAttributeString("unitRef", unitNamePure);

                                if (value.Value == null) {
                                    w.WriteAttributeString("nil", xsi.URL, "true");                                    
                                } else {
                                    w.WriteAttributeString("decimals", "0");
                                    w.WriteString(value.Value.ToString());
                                }
                                
                                break;

                            case XbrlProcessor.Taxonomy.XbrlElementValue.XbrlElementValueTypes.Numeric:
                                w.WriteAttributeString("unitRef", unitNamePure);

                                if (value.Value == null) {
                                    w.WriteAttributeString("nil", xsi.URL, "true");
                                } else {
                                    w.WriteAttributeString("precision", "INF");
                                    w.WriteString(value.Value.ToString());
                                }
                                
                                w.WriteString(value.Value.ToString().Replace(",", "."));
                                break;

                            case XbrlProcessor.Taxonomy.XbrlElementValue.XbrlElementValueTypes.Monetary:
                                w.WriteAttributeString("unitRef", unitNameMonetary);

                                if (value.Value == null) {
                                    w.WriteAttributeString("nil", xsi.URL, "true");
                                } else {
                                    w.WriteAttributeString("decimals", "2");
                                    w.WriteString((value as XbrlElementValue_Monetary).MonetaryValue.Value.ToString("0.00").Replace(",", "."));
                                }
                                                                
                                break;

                            case XbrlProcessor.Taxonomy.XbrlElementValue.XbrlElementValueTypes.Date:

                                if (value.Value == null) {
                                    w.WriteAttributeString("nil", xsi.URL, "true");
                                } else {
                                    w.WriteString(((DateTime)value.Value).ToString("yyyy-MM-dd"));
                                }
                                
                                break;

                            case XbrlProcessor.Taxonomy.XbrlElementValue.XbrlElementValueTypes.Boolean:

                                if (value.Value == null) {
                                    w.WriteAttributeString("nil", xsi.URL, "true");
                                } else {
                                    w.WriteString((bool)value.Value ? "true" : "false");
                                }

                                break;

                            case XbrlProcessor.Taxonomy.XbrlElementValue.XbrlElementValueTypes.String:

                                if (value.Value == null) {
                                    w.WriteAttributeString("nil", xsi.URL, "true");
                                } else {
                                    w.WriteString(value.Value.ToString());
                                }
                                
                                break;

                            default:

                                if (value.Value == null) {
                                    w.WriteAttributeString("nil", xsi.URL, "true");
                                } else {
                                    w.WriteString(value.Value.ToString());
                                }
                                
                                break;
                        }

                        w.WriteEndElement();
                    }
                    break;
            }
        }

        private static void WriteContextAttribute(XmlWriter w, IXbrlElementValue value) {
            WriteContextAttribute(w, value.Element);
        }

        private static void WriteContextAttribute(XmlWriter w, Element elem) {

            if (elem.PeriodType == null) return;

            // write contextRef attribute
            if (elem.PeriodType == "instant") {
                w.WriteAttributeString("contextRef", contextNameInstant);
            } else if (elem.PeriodType == "duration") {
                w.WriteAttributeString("contextRef", contextNamePeriod);
            } else {
                System.Diagnostics.Debug.WriteLine("unknown xbrl element type: " + elem.PeriodType);
            }
        }

        private static IXbrlElementValue SearchCompanyValue(ValueTreeNode root, string xbrlId) {
            if (root.Values.ContainsKey(xbrlId)) {
                return root.Values[xbrlId];
            } else {
                foreach (IXbrlElementValue value in root.Values.Values) {
                    if (value.Element.Name.StartsWith("genInfo.company") && value is XbrlElementValue_Tuple) {
                        IXbrlElementValue result = SearchCompanyValue((value as XbrlElementValue_Tuple).Items[0], xbrlId);
                        if (result != null) return result;
                    }
                }
            }

            return null;
        }

        private static void UpdateCompanyData(Document document) {

            document.ValueTreeGCD.Root.Values["genInfo.company.id.location"].Value = "Firmensitz";
            document.ValueTreeGCD.Root.Values["genInfo.company.id.location.country"].Value = "Deutschland";
            
            // update company data
            foreach (PropertyInfo pi in typeof(Company).GetProperties()) {                   
                foreach (System.Attribute attr in System.Attribute.GetCustomAttributes(pi)) {
                    if (attr is XbrlId) {
                        string xbrlId = (attr as XbrlId).Id;
                        IXbrlElementValue value = SearchCompanyValue(document.ValueTreeGCD.Root, xbrlId);
                        if (value != null) value.Value = pi.GetValue(document.Company, null);
                    }
                }
            }

            // update contacts
            XbrlElementValue_List contactList = document.ValueTreeGCD.Root.Values["genInfo.company.id.contactAddress"] as XbrlElementValue_List;
            List<ValueTreeNode> deleteContactList = new List<ValueTreeNode>();
            foreach (ValueTreeNode node in contactList.Items) deleteContactList.Add(node); 
            foreach (ValueTreeNode node in deleteContactList) contactList.DeleteValue(node);
            foreach (CompanyContact contact in document.Company.Contacts) {
                ValueTreeNode newContactNode = contactList.AddValue();

                foreach (PropertyInfo pi in typeof(CompanyContact).GetProperties()) {
                    foreach (System.Attribute attr in System.Attribute.GetCustomAttributes(pi)) {
                        if (attr is XbrlId) {                           
                            string xbrlId = (attr as XbrlId).Id;
                            IXbrlElementValue value = SearchCompanyValue(newContactNode, xbrlId);
                            if (value != null) value.Value = pi.GetValue(contact, null);
                        }
                    }

                    IXbrlElementValue value1 = SearchCompanyValue(newContactNode, "genInfo.company.id.contactAddress.person");
                    if (value1 != null) value1.Value = "Kontaktperson"; // must be set, currently not available in GUI
                }
            }

            // update shareholders
            XbrlElementValue_List shareholderList = document.ValueTreeGCD.Root.Values["genInfo.company.id.shareholder"] as XbrlElementValue_List;
            List<ValueTreeNode> deleteShareholderList = new List<ValueTreeNode>();
            foreach (ValueTreeNode node in shareholderList.Items) deleteShareholderList.Add(node);
            foreach (ValueTreeNode node in deleteShareholderList) shareholderList.DeleteValue(node);
            foreach (Shareholder shareholder in document.Company.Shareholders) {
                ValueTreeNode newShareholderNode = shareholderList.AddValue();

                foreach (PropertyInfo pi in typeof(Shareholder).GetProperties()) {
                    foreach (System.Attribute attr in System.Attribute.GetCustomAttributes(pi)) {
                        if (attr is XbrlId) {
                            string xbrlId = (attr as XbrlId).Id;
                            IXbrlElementValue value = SearchCompanyValue(newShareholderNode, xbrlId);
                            if (value != null) value.Value = pi.GetValue(shareholder, null);
                        }
                    }
                }
            }

            // TODO: (DEBUG) document.Company.Parent = document.Company;
            // Fehler bei Ausgabe von xxx.BF4
            if (document.Company.Parent != null) {

                var parentNode = (SearchCompanyValue(document.ValueTreeGCD.Root, "genInfo.company.id.parent") as XbrlElementValue_Tuple).Items[0];
                
                // update company data
                foreach (PropertyInfo pi in typeof(Company).GetProperties()) {
                    foreach (System.Attribute attr in System.Attribute.GetCustomAttributes(pi)) {
                        if (attr is XbrlId) {
                            string xbrlId = (attr as XbrlId).Id;
                             IXbrlElementValue value = SearchCompanyValue(parentNode, xbrlId.Replace("genInfo.company.id", "genInfo.company.id.parent"));
                            if (value != null) value.Value = pi.GetValue(document.Company.Parent, null);
                        }
                    }
                }
            }
        }
    }
}