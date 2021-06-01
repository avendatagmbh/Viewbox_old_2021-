// --------------------------------------------------------------------------------
// author: Solueman Hussain
// since: 2011-12-08
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using eBalanceKitBusiness.FederalGazette.Model;
using eBalanceKitBusiness.Manager;
using eBalanceKitBusiness.Structures.DbMapping;
using eBalanceKitBusiness.Structures.ValueTree;
using eBalanceKitBusiness.Structures.XbrlElementValue;

namespace eBalanceKitBusiness.FederalGazette {
    public class FederalGazetteXbrlExport {

        private class XmlNamespace {
            public XmlNamespace(string name, string url) {
                Name = name;
                Url = url;
            }

            public string Name { get; set; }
            public string Url { get; set; }
        }

        #region locals
        public FederalGazetteXbrlExport(FederalGazetteMainModel model) { Model = model; }
        private FederalGazetteMainModel Model { get; set; }
        // XBRL namespaces
        private static XmlNamespace xbrli = new XmlNamespace("xbrlin", "http://www.xbrl.org/2003/instance");
        private static XmlNamespace xbrldi = new XmlNamespace("xbrldi", "http://xbrl.org/2006/xbrldi");
        private static XmlNamespace xlink = new XmlNamespace("xlink", "http://www.w3.org/1999/xlink");
        private static XmlNamespace link = new XmlNamespace("link", "http://www.xbrl.org/2003/linkbase");
        private static XmlNamespace xsi = new XmlNamespace("xsi", "http://www.w3.org/2001/XMLSchema-instance");
        private static XmlNamespace iso4217 = new XmlNamespace("iso4217", "http://www.xbrl.org/2003/iso4217");
        private static XmlNamespace xhtml = new XmlNamespace("xhtml", "http://www.w3.org/1999/xhtml");
        private static string _contextNameInstant;
        private static string _contextNamePeriod;
        private const string UnitNameMonetary = "EUR";
        private const string UnitNamePure = "unit_pure";
        private List<string> _accounts = new List<string>();
        #endregion

        #region ExportXmlAnnualStatement
        public StringBuilder ExportXmlAnnualStatement(string fileName, bool writeXml, string account) {
            _accounts.Clear();
            _accounts.Add(account);
            return ExportXmlAnnualStatement(writeXml, fileName);
        }
        #endregion

        #region ExportXmlAnnualStatement
        public StringBuilder ExportXmlAnnualStatement(string fileName, bool writeXml, List<string> account) {
            _accounts.Clear();
            _accounts = account;
            return ExportXmlAnnualStatement(writeXml, fileName);
        }
        #endregion

        #region ExportXmlAnnualStatement
        private StringBuilder ExportXmlAnnualStatement(bool writeXml, string filename) {
            var xmlSettings = new XmlWriterSettings
            {Indent = true, IndentChars = ("    "), Encoding = Encoding.GetEncoding("iso-8859-15")};
            var builder = new StringBuilder();
            //var file = "C:\\Users\\soh\\Desktop\\testBA.xml";

            using (XmlWriter xmlWriter = XmlWriter.Create(builder, xmlSettings)) {
                _contextNameInstant = "I-" + Model.Document.FinancialYear.FYear;
                _contextNamePeriod = "D-" + Model.Document.FinancialYear.FYear;

                string entityId = null;
                string entityScheme = ""; //scheme name????

                if (!Model.Document.ValueTreeGcd.Root.Values.ContainsKey("genInfo.company.id.idNo")) throw new Exception("Could not find tuple 'genInfo.company.id.idNo'.");
                var idNoNode =
                    Model.Document.ValueTreeGcd.Root.Values["genInfo.company.id.idNo"] as XbrlElementValue_Tuple;
                if (!idNoNode.Items[0].Values.ContainsKey("genInfo.company.id.idNo.type.companyId.ST13")) throw new Exception("Could not find fact 'genInfo.company.id.idNo.type.companyId.ST13'.");
                var value = idNoNode.Items[0].Values["genInfo.company.id.idNo.type.companyId.ST13"];
                if (value.HasValue) entityId = value.Value.ToString();

                xmlWriter.WriteStartDocument();

                GenerateXmlContent(Model.Document, xmlWriter, entityScheme, entityId);

                xmlWriter.WriteEndDocument();
            }
            return builder;
        }
        #endregion

        #region GenerateXmlContent
        private void GenerateXmlContent(Document document, XmlWriter xmlWriter, string entityScheme, string entityId) {
            //var namespaces = new Dictionary<string, XmlNamespace>();

            //xmlWriter.WriteStartElement("xbrli", "xbrl", xbrli.Url);
            //{
            //    xmlWriter.WriteAttributeString("xmlns", xbrli.Name, null, xbrli.Url);
            //    xmlWriter.WriteAttributeString("xmlns", xbrldi.Name, null, xbrldi.Url);
            //    xmlWriter.WriteAttributeString("xmlns", xlink.Name, null, xlink.Url);
            //    xmlWriter.WriteAttributeString("xmlns", link.Name, null, link.Url);
            //    xmlWriter.WriteAttributeString("xmlns", xsi.Name, null, xsi.Url);
            //    xmlWriter.WriteAttributeString("xmlns", iso4217.Name, null, iso4217.Url);
            //    xmlWriter.WriteAttributeString("xmlns", xhtml.Name, null, xhtml.Url);

            //    var namespaceToName = new Dictionary<string, string>();
            //    foreach (var elem in document.MainTaxonomy.Elements.Values) {
            //        string targetNamespaceName = null;
            //        if (elem.TargetNamespace.Contains("de-gcd")) targetNamespaceName = "gcd";
            //        else if (elem.TargetNamespace.Contains("de-gaap")) targetNamespaceName = "de-gaap-ci";
            //        else if (elem.TargetNamespace.Contains("de-bra")) targetNamespaceName = "de-bra";
            //        else if (elem.TargetNamespace.Contains("de-fi")) targetNamespaceName = "de-fi";
            //        else if (elem.TargetNamespace.Contains("de-ins")) targetNamespaceName = "de-ins";
            //        if (targetNamespaceName == null) {
            //            if (!namespaceToName.ContainsKey(elem.TargetNamespace)) namespaceToName[elem.TargetNamespace] = "ns" + (namespaceToName.Count + 1);
            //            targetNamespaceName = namespaceToName[elem.TargetNamespace];
            //        }

            //        if (!namespaces.ContainsKey(targetNamespaceName)) {
            //            var ns = new XmlNamespace(targetNamespaceName, elem.TargetNamespace);
            //            namespaces[targetNamespaceName] = ns;
            //            xmlWriter.WriteAttributeString("xmlns", ns.Name, null, ns.Url);
            //        }
            //    }
            //    InsertSchemaRef(xmlWriter, document.MainTaxonomy.SchemaRef);
            //    InsertSchemaRef(xmlWriter, document.GcdTaxonomy.SchemaRef);

            //    AddInstantContext(document, xmlWriter, entityScheme, entityId, _contextNameInstant);
            //    AddInstantContext(document, xmlWriter, entityScheme, entityId, _contextNamePeriod);

            //    xmlWriter.WriteStartElement("unit", xbrli.Url);
            //    xmlWriter.WriteAttributeString("id", UnitNameMonetary);
            //    xmlWriter.WriteStartElement("measure", xbrli.Url);
            //    xmlWriter.WriteString(iso4217.Name + ":EUR");
            //    xmlWriter.WriteEndElement();
            //    xmlWriter.WriteEndElement();

            //    xmlWriter.WriteStartElement("unit", xbrli.Url);
            //    xmlWriter.WriteAttributeString("id", UnitNamePure);
            //    xmlWriter.WriteStartElement("measure", xbrli.Url);
            //    xmlWriter.WriteString(xbrli.Name + ":pure");
            //    xmlWriter.WriteEndElement();
            //    xmlWriter.WriteEndElement();

            //    WriteGAAP(xmlWriter, document, namespaces["de-gaap-ci"]);
            //    //WriteGCD(xmlWriter, document);
            //}
            //xmlWriter.WriteEndElement();
        }
        #endregion

        //#region WriteGCD
        //private void WriteGCD(XmlWriter xmlWriter, Document document, string[] account) {
        //    ResetIsReportableState(document.ValueTreeGcd.Root);
        //    foreach (var presentationTree in document.GcdTaxonomy.PresentationTrees) UpdateIsReportableState(presentationTree.RootEntries.First(), document.ValueTreeGcd.Root);
        //    InsertItems(xmlWriter, document.ValueTreeGcd.Root);
        //}
        //#endregion

        //#region WriteGAAP
        //private void WriteGAAP(XmlWriter xmlWriter, Document document, XmlNamespace gaapNamespace) {
        //    ResetIsReportableState((document.ValueTreeMain.Root));

        //    foreach (var ptree in document.MainTaxonomy.PresentationTrees) {
        //        UpdateIsReportableState(ptree.RootEntries.First(), document.ValueTreeMain.Root);
        //    }
        //    InsertItems(xmlWriter, document.ValueTreeMain.Root);
        //    foreach (var balanceList in document.BalanceLists) InsertAccountBalances(xmlWriter, balanceList, gaapNamespace, document);

        //}
        //#endregion

        //#region InsertAccountBalances
        //private void InsertAccountBalances(XmlWriter xmlWriter, IBalanceList balanceList, XmlNamespace gaapNamespace,
        //                                   Document document) {

        //    if (!(document.MainTaxonomy.Elements.ContainsKey("de-gaap-ci_detailedInformation.accountBalances") &&
        //          document.MainTaxonomy.Elements.ContainsKey(
        //              "de-gaap-ci_detailedInformation.accountbalances.positionName") &&
        //          document.MainTaxonomy.Elements.ContainsKey(
        //              "de-gaap-ci_detailedInformation.accountbalances.accountNumber") &&
        //          document.MainTaxonomy.Elements.ContainsKey(
        //              "de-gaap-ci_detailedInformation.accountbalances.accountDescription") &&
        //          document.MainTaxonomy.Elements.ContainsKey("de-gaap-ci_detailedInformation.accountbalances.amount"))) return;

        //    foreach (var entry in balanceList.AssignedItems) {
        //        if (entry is IAccountGroup) {
        //            if (!entry.SendBalance) continue;
        //            var group = entry as IAccountGroup;
        //            foreach (var account in group.Items) WriteAccountBalanceLine(xmlWriter, account, gaapNamespace, document);
        //        } else {
        //            if (!entry.SendBalance) continue;
        //            WriteAccountBalanceLine(xmlWriter, entry, gaapNamespace, document);
        //        }
        //    }
        //}
        //#endregion

        //#region WriteAccountBalanceLine
        //private static void WriteAccountBalanceLine(XmlWriter xmlWriter, IBalanceListEntry entry,
        //                                            XmlNamespace gaapNamespace, Document document) {
        //    string xbrlName = document.TaxonomyIdManager.GetElement(entry.AssignedElementId).Name;
        //    xmlWriter.WriteStartElement("detailedInformation.accountBalances", gaapNamespace.Url);
        //    WriteElement(xmlWriter,
        //                 document.MainTaxonomy.Elements["de-gaap-ci_detailedInformation.accountbalances.positionName"],
        //                 xbrlName);
        //    WriteElement(xmlWriter,
        //                 document.MainTaxonomy.Elements["de-gaap-ci_detailedInformation.accountbalances.accountNumber"],
        //                 entry.Number);
        //    WriteElement(xmlWriter,
        //                 document.MainTaxonomy.Elements[
        //                     "de-gaap-ci_detailedInformation.accountbalances.accountDescription"], entry.Name);
        //    WriteElement(xmlWriter,
        //                 document.MainTaxonomy.Elements["de-gaap-ci_detailedInformation.accountbalances.amount"],
        //                 entry.Amount);
        //    xmlWriter.WriteEndElement();
        //}
        //#endregion

        //#region UpdateIsReportableState

        //private bool UpdateIsReportableState(Taxonomy.Interfaces.PresentationTree.IPresentationTreeNode root,
        //                                     ValueTreeNode vroot) {
        //    if (vroot.Values.ContainsKey(root.Element.Name)) {

        //        // value element
        //        var value = vroot.Values[root.Element.Name];
        //        value.IsReportable |= IsReportableItem(value);

        //        switch (root.Element.ValueType) {
        //            case Taxonomy.Enums.XbrlElementValueTypes.Tuple:
        //                foreach (var child in root.Children)
        //                    foreach (ValueTreeNode item in (value as XbrlElementValue_Tuple).Items)
        //                        value.IsReportable |=
        //                            UpdateIsReportableState(
        //                                child as Taxonomy.Interfaces.PresentationTree.IPresentationTreeNode, item);
        //                break;

        //            default:
        //                foreach (var child in root.Children)
        //                    if (child is Taxonomy.Interfaces.PresentationTree.IPresentationTreeNode)
        //                        value.IsReportable |=
        //                            UpdateIsReportableState(
        //                                child as Taxonomy.Interfaces.PresentationTree.IPresentationTreeNode, vroot);
        //                break;
        //        }

        //        return value.IsReportable;

        //    } else {
        //        // abstract element
        //        bool isReportable = false;
        //        foreach (var child in root.Children) {
        //            if (child is Taxonomy.Interfaces.PresentationTree.IPresentationTreeNode) {
        //                isReportable |=
        //                    UpdateIsReportableState(
        //                        child as Taxonomy.Interfaces.PresentationTree.IPresentationTreeNode, vroot);
        //            }
        //        }
        //        return isReportable;
        //    }
        //}
        //#endregion

        //#region IsReportableItem

        //private bool IsReportableItem(IValueTreeEntry value) {
        //    // do not send nil value for disallowed elements
        //    if (!value.HasValue && (value.Element.NotPermittedForCommercial || value.Element.NotPermittedForFiscal)) return false;

        //    if (value.Element.IsMandatoryField) return true;

        //    switch (value.Element.ValueType) {
        //        case Taxonomy.Enums.XbrlElementValueTypes.MultipleChoice:
        //            XbrlElementValue_MultipleChoice mcValue = value as XbrlElementValue_MultipleChoice;
        //            foreach (var item in mcValue.Elements) {
        //                bool? isChecked = mcValue.IsChecked[item.Name].BoolValue;
        //                if (isChecked.HasValue) return true;
        //            }

        //            // no value selected
        //            return false;

        //        case Taxonomy.Enums.XbrlElementValueTypes.Tuple:
        //            XbrlElementValue_Tuple tuple = value as XbrlElementValue_Tuple;
        //            foreach (var item in tuple.Items) {
        //                foreach (var itemvalue in item.Values) {
        //                    if (IsReportableItem(itemvalue.Value)) return true;
        //                }
        //            }
        //            return false;

        //        default:
        //            return value.HasValue;
        //    }
        //}
        //#endregion

        //#region ResetIsReportableState

        //private void ResetIsReportableState(ValueTreeNode root) {
        //    foreach (var value in root.Values.Values) {
        //        value.IsReportable = false;
        //        if (value is XbrlElementValue_Tuple) {
        //            foreach (var child in (value as XbrlElementValue_Tuple).Items) {
        //                ResetIsReportableState(child);
        //            }
        //        }
        //    }
        //}
        //#endregion

        //#region InsertSchemaRef
        //private void InsertSchemaRef(XmlWriter xmlWriter, string url) {
        //    xmlWriter.WriteStartElement("schemaRef", link.Url);
        //    xmlWriter.WriteAttributeString("type", xlink.Url, "simple");
        //    xmlWriter.WriteAttributeString("href", xlink.Url, url);
        //    xmlWriter.WriteEndElement();
        //}
        //#endregion

        //#region Insertitems
        //private void InsertItems(XmlWriter xmlWriter, ValueTreeNode root) {
        //    foreach (var value in root.Values.Values) {
        //        if (value.Element.Name.StartsWith("mgmtRep")) {
        //            var test = value.Element.Name.ToString();
        //        }
        //        if (value.Element.IsSelectionListEntry) continue;
        //        if (value.Element.Name.StartsWith("genInfo.company.id.parent")) continue;

        //        foreach (var account in _accounts) {
        //            if (value.Element.Name.StartsWith(account) && !value.Element.Name.StartsWith("bs.contingLiab"))
        //                if (value is XbrlElementValue_Tuple) {
        //                    InsertTupel(xmlWriter, value);
        //                } else {
        //                    InsertItem(xmlWriter, value);
        //                }
        //        }
        //    }
        //}
        //#endregion

        //#region InsertTupel
        //private void InsertTupel(XmlWriter xmlWriter, IValueTreeEntry value) {
        //    var tuple = value as XbrlElementValue_Tuple;
        //    if (tuple.Items.Count == 0) {
        //        if (tuple.Element.Name == "genInfo.company.id.shareholder")
        //            try {
        //                LogManager.Instance.Log = false;
        //                tuple.AddValue();
        //                var item = tuple.Items[0];
        //                //if its the same case with as elster shareholder no present. a matter of testing?!
        //                foreach (var x in item.Values.Values) {
        //                    if (x.Element.IsMandatoryField) x.IsReportable = true;
        //                    if (x.Element.Name == "genInfo.company.id.shareholder.ShareDivideKey") {
        //                        var keyTupel = x as XbrlElementValue_Tuple;
        //                        foreach (var entry in keyTupel.Items[0].Values) {
        //                            if (entry.Key == "genInfo.company.id.shareholder.ShareDivideKey.denominator") entry.Value.Value = 1;
        //                        }
        //                    }
        //                }
        //                xmlWriter.WriteStartElement(value.Element.Name, value.Element.TargetNamespace);
        //                InsertItems(xmlWriter, item);
        //                xmlWriter.WriteEndElement();
        //                tuple.DeleteValue(item);
        //            } finally {
        //                LogManager.Instance.Log = true;
        //            }
        //    }
        //}
        //#endregion

        //#region InsertItem
        //private void InsertItem(XmlWriter xmlWriter, IValueTreeEntry value) {
        //    if (!value.HasValue && (value.Element.NotPermittedForCommercial || value.Element.NotPermittedForFinancial)) return;
        //    switch (value.Element.ValueType) {
        //        case Taxonomy.Enums.XbrlElementValueTypes.SingleChoice:

        //            #region SingleChoice
        //            if (value.Value != null) {
        //                xmlWriter.WriteStartElement(value.Element.Name, value.Element.TargetNamespace);

        //                WriteContextAttribute(xmlWriter, value);

        //                foreach (var item in ((XbrlElementValue_SingleChoice) value).Elements) {
        //                    if (item.Name == value.Value.ToString()) {
        //                        xmlWriter.WriteStartElement(item.Name, item.TargetNamespace);
        //                        WriteContextAttribute(xmlWriter, item);
        //                        xmlWriter.WriteEndElement();
        //                    }
        //                }
        //                xmlWriter.WriteEndElement();
        //            } else {
        //                if (value.Element.IsMandatoryField) {
        //                    xmlWriter.WriteStartElement(value.Element.Name, value.Element.TargetNamespace);

        //                    //Dirty hack for elster client if no shareholder is present. whether its important in case of Federal Gazette.
        //                    if (value.Element.Name == "genInfo.company.id.shareholder.legalStatus" &&
        //                        ((XbrlElementValue_SingleChoice) value).Elements.Count > 0) {
        //                        var item = ((XbrlElementValue_SingleChoice) value).Elements[0];
        //                        xmlWriter.WriteStartElement(item.Name, item.TargetNamespace);
        //                        xmlWriter.WriteAttributeString("nil", xsi.Url, "true");
        //                        WriteContextAttribute(xmlWriter, item);
        //                        xmlWriter.WriteEndElement();
        //                    } else xmlWriter.WriteAttributeString("nil", xsi.Url, "true");
        //                    WriteContextAttribute(xmlWriter, value);
        //                    xmlWriter.WriteEndElement();
        //                }
        //            }

        //            break;
        //            #endregion SingleChoice

        //        case Taxonomy.Enums.XbrlElementValueTypes.MultipleChoice:

        //            #region MultipleChoice
        //            xmlWriter.WriteStartElement(value.Element.Name, value.Element.TargetNamespace);

        //            WriteContextAttribute(xmlWriter, value);

        //            var mcValue = value as XbrlElementValue_MultipleChoice;
        //            foreach (var item in mcValue.Elements) {
        //                bool? isChecked = mcValue.IsChecked[item.Name].BoolValue;

        //                if (isChecked.HasValue || item.IsMandatoryField) {

        //                    xmlWriter.WriteStartElement(item.Name, item.TargetNamespace);

        //                    WriteContextAttribute(xmlWriter, item);

        //                    if (isChecked.HasValue) {
        //                        if (isChecked.Value == true) {
        //                            if (item.Name.EndsWith(".S")) {
        //                                // values "other"
        //                                //w.WriteString(((XbrlElementValueBase)value).ValueOther);
        //                            } else {
        //                                // true
        //                            }
        //                        } else {
        //                            //w.WriteString("false");
        //                            xmlWriter.WriteAttributeString("nil", xsi.Url, "true");
        //                        }
        //                    } else {
        //                        xmlWriter.WriteAttributeString("nil", xsi.Url, "true");
        //                    }

        //                    xmlWriter.WriteEndElement();
        //                }
        //            }
        //            xmlWriter.WriteEndElement();
        //            break;
        //            #endregion MultipleChoice

        //        case Taxonomy.Enums.XbrlElementValueTypes.Monetary:
        //            if (value.MonetaryValue.Value != null || value.IsReportable ||
        //                value.Element.Name.StartsWith("genInfo.company.id.shareholder.ShareDivideKey.")) WriteElement(xmlWriter, value.Element, value.MonetaryValue.Value);
        //            break;

        //        default:
        //            if (value.Value != null || value.IsReportable ||
        //                value.Element.Name.StartsWith("genInfo.company.id.shareholder.ShareDivideKey.")) WriteElement(xmlWriter, value.Element, value.Value);
        //            break;
        //    }
        //}
        //#endregion

        //#region WriteElement
        //private static void WriteElement(XmlWriter xmlWriter, Taxonomy.IElement elem, object value) {
        //    xmlWriter.WriteStartElement(elem.Name, elem.TargetNamespace);
        //    //w.WriteAttributeString("xmlns",elem.TargetNamespace);
        //    WriteContextAttribute(xmlWriter, elem);
        //    switch (elem.ValueType) {
        //        case Taxonomy.Enums.XbrlElementValueTypes.Int:
        //            xmlWriter.WriteAttributeString("unitRef", UnitNamePure);

        //            if (value == null) {
        //                xmlWriter.WriteAttributeString("nil", xsi.Url, "true");
        //            } else {
        //                xmlWriter.WriteAttributeString("decimals", "2");
        //                xmlWriter.WriteString(value.ToString());
        //            }

        //            break;

        //        case Taxonomy.Enums.XbrlElementValueTypes.Numeric:
        //            xmlWriter.WriteAttributeString("unitRef", UnitNamePure);

        //            if (value == null) {
        //                xmlWriter.WriteAttributeString("nil", xsi.Url, "true");
        //            } else {
        //                xmlWriter.WriteAttributeString("precision", "INF");
        //                xmlWriter.WriteString(value.ToString());
        //            }

        //            xmlWriter.WriteString(value.ToString().Replace(",", "."));
        //            break;

        //        case Taxonomy.Enums.XbrlElementValueTypes.Monetary:
        //            xmlWriter.WriteAttributeString("unitRef", UnitNameMonetary);

        //            if (value == null) {
        //                xmlWriter.WriteAttributeString("nil", xsi.Url, "true");
        //            } else {
        //                xmlWriter.WriteAttributeString("decimals", "2");
        //                xmlWriter.WriteString(((Decimal) value).ToString("0.00").Replace(",", "."));
        //            }

        //            break;

        //        case Taxonomy.Enums.XbrlElementValueTypes.Date:

        //            if (value == null) {
        //                xmlWriter.WriteAttributeString("nil", xsi.Url, "true");
        //            } else {
        //                xmlWriter.WriteString(((DateTime) value).ToString("yyyy-MM-dd"));
        //            }

        //            break;

        //        case Taxonomy.Enums.XbrlElementValueTypes.Boolean:

        //            if (value == null) {
        //                xmlWriter.WriteAttributeString("nil", xsi.Url, "true");
        //            } else {
        //                xmlWriter.WriteString((bool) value ? "true" : "false");
        //            }

        //            break;

        //        case Taxonomy.Enums.XbrlElementValueTypes.String:

        //            if (value == null) {
        //                xmlWriter.WriteAttributeString("nil", xsi.Url, "true");
        //            } else {
        //                xmlWriter.WriteString(value.ToString());
        //            }
        //            break;

        //        default:

        //            if (value == null) {
        //                xmlWriter.WriteAttributeString("nil", xsi.Url, "true");
        //            } else {
        //                xmlWriter.WriteString(value.ToString());
        //            }
        //            break;
        //    }

        //    xmlWriter.WriteEndElement();
        //}
        //#endregion

        //#region WriteContextAttribute
        //private static void WriteContextAttribute(XmlWriter xmlWriter, IValueTreeEntry value) { WriteContextAttribute(xmlWriter, value.Element); }
        //#endregion

        //#region WriteContextAttribute
        //private static void WriteContextAttribute(XmlWriter xmlWriter, Taxonomy.IElement elem) {

        //    if (elem.PeriodType == null) return;
        //    // write contextRef attribute
        //    if (elem.PeriodType == "instant") {
        //        xmlWriter.WriteAttributeString("contextRef", _contextNameInstant);
        //    } else if (elem.PeriodType == "duration") {
        //        xmlWriter.WriteAttributeString("contextRef", _contextNamePeriod);
        //    } else {
        //        System.Diagnostics.Debug.WriteLine("unknown xbrl element type: " + elem.PeriodType);
        //    }
        //}
        //#endregion

        //#region AddInstantContext
        //private static void AddInstantContext(Document document, XmlWriter xmlWriter, string entityScheme,
        //                                      string entityId, string contextName) {
        //    xmlWriter.WriteStartElement("context", xbrli.Url);
        //    xmlWriter.WriteAttributeString("id", contextName);

        //    xmlWriter.WriteStartElement("entity", xbrli.Url);
        //    xmlWriter.WriteStartElement("identifier", xbrli.Url);
        //    xmlWriter.WriteAttributeString("scheme", entityScheme);
        //    xmlWriter.WriteString(entityId);
        //    xmlWriter.WriteEndElement();
        //    xmlWriter.WriteEndElement();

        //    xmlWriter.WriteStartElement("period", xbrli.Url);

        //    xmlWriter.WriteStartElement("instant", xbrli.Url);

        //    if (document.FinancialYear.BalSheetClosingDate.HasValue) {
        //        xmlWriter.WriteString(document.FinancialYear.BalSheetClosingDate.Value.ToString("yyyy-MM-dd"));
        //    }
        //    xmlWriter.WriteEndElement();
        //    xmlWriter.WriteEndElement();
        //    xmlWriter.WriteEndElement();
        //}
        //#endregion
    }
}