// --------------------------------------------------------------------------------
// author: Istvan Balazsy
// since: 2012-09-05
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml;
using DbAccess;
using Taxonomy;
using Taxonomy.Enums;
using Taxonomy.Interfaces;
using Taxonomy.Interfaces.PresentationTree;
using Taxonomy.PresentationTree;
using Utils;
using eBalanceKitBase.Structures;
using eBalanceKitBusiness.Interfaces.BalanceList;
using eBalanceKitBusiness.Logs;
using eBalanceKitBusiness.Reconciliation;
using eBalanceKitBusiness.Reconciliation.DbMapping;
using eBalanceKitBusiness.Reconciliation.Enums;
using eBalanceKitBusiness.Reconciliation.Import;
using eBalanceKitBusiness.Reconciliation.Interfaces;
using eBalanceKitBusiness.Reconciliation.ReconciliationTypes;
using eBalanceKitBusiness.Reconciliation.ViewModels;
using eBalanceKitBusiness.Rights;
using eBalanceKitBusiness.Structures;
using eBalanceKitBusiness.Structures.DbMapping;
using System.Globalization;
using eBalanceKitBusiness.Manager;
using eBalanceKitBusiness.Structures.DbMapping.BalanceList;
using eBalanceKitBusiness.Structures.ValueTree;
using eBalanceKitBusiness.Structures.XbrlElementValue;
using eBalanceKitResources.Localisation;
using PresentationTreeNode = eBalanceKitBusiness.Structures.Presentation.PresentationTreeNode;
using PreviousYearValues = eBalanceKitBusiness.Reconciliation.Import.PreviousYearValues;
using log4net;
using AV.Log;

namespace eBalanceKitBusiness.Import {

    internal class XbrlImporter {
        #region constructor
        public XbrlImporter(FileStream stream) {

            XbrlImportErrors = new List<XbrlImportError>();
            if (stream != null) {
                _fileName = stream.Name;
            }

            try {
                if (stream != null) {
                    stream.Position = 0;
                    _xmlDoc.Load(stream);
                }
                //_company = new Company
                //           {TaxonomyInfo = TaxonomyManager.GetTaxonomyInfo(TaxonomyType.GCD, "2012-06-01")};
            } catch (Exception ex) {
                XbrlImportErrors.Add(new XbrlImportError {
                    FilePath = _fileName,
                    ErrorDescription = ex.Message
                });
            }
        }
        #endregion constructor

        #region constants

        internal ILog _log = LogHelper.GetLogger();

        private const string ReconciliationTransferNodeName = "hbst.transfer";
        private const string GenInfoNodeName = "genInfo";
        private const string PrefixNodeName1 = "de-gaap-ci";
        private const string PrefixNodeName2 = "de-bra";
        private const string PrefixNodeName3 = "de-fi";
        private const string PrefixNodeName4 = "de-ins";
        private const string TaxonomyPrefix = "de-gcd";
        private const string ActualPeriodLabel = "changeValueActualPeriod";
        private const string PreviousPeriodLabel = "changeValuePreviousPeriod";
        private const string ReconciliationKindNodeName = "kind";
        private const string ReconciliationNameNodeName = "name";
        private const string CommentNodeName = "comment";
        private const string MultipleChoiceisCheckedAttr = "xsi:nil";
        private const string MultipleChoiceisCheckedAttrTemplate = "nil";
        private const string DetailedInfoNode = "detailedInformation";
        private const string AccountBalanceInfoNode = "accountBalances";
        private const string TaxonomyNode = "link:schemaRef";
        private const string TaxonomyAttribute = "xlink:href";
        private const string CommentsTaxonomyPositions = "commentsTaxonomyPositions";
        private const string ViewConfigurations = "Settings";
        private const string FootNoteNode = "link:footnoteLink";


        #endregion constants


        #region members

        private XmlDocument _xmlDoc = new XmlDocument();
        private Document _document = new Document();
        private Company _company;
        private string _fileName;
        private string _reportName;
        private string _reportComment;
        private Structures.DbMapping.System _system;

        private Reconciliation.Import.PreviousYearValues previousYearValues = new Reconciliation.Import.PreviousYearValues();

        private ReconciliationsModel model;

        public List<XbrlImportError> XbrlImportErrors { get; private set; }
        public bool HasErrors { get { return XbrlImportErrors.Count > 0; } }
        public Document Document { get { return _document; } set { _document = value; } }
        public Company Company { get { return _company; } set { _company = value; } }

        private bool _isFullImport;
        private bool isShareholderDeleted;
        private bool isDocAuthorDeleted;
        private bool isContactPersonDeleted;
        private bool isSegmBusinessDeleted;

        #endregion members


        #region events
        #endregion events


        #region properties

        #endregion

        //Gets the company name and financial year of the report to the CtlImportXbrlDetails dialog

        public Tuple<string, string> GetXbrlDetailsForDialog() {
            XmlNode companyNode = _xmlDoc.SelectSingleNode("//*[name()='genInfo.company.id.name']");
            XmlNode financialYearNode = _xmlDoc.SelectSingleNode("//*[name()='xbrli:instant']");
            string company = null;
            string year = null;
            if(companyNode != null ) {
                company = companyNode.InnerText;
            }
            if (financialYearNode != null) {
                CultureInfo cu = new CultureInfo("de-DE");
                year = DateTime.Parse(financialYearNode.InnerText, cu).Year.ToString(cu);
            }
            return new Tuple<string, string>(company, year);
        }

        //Gets the company name and financial year of the report to the CtlImportFullDetails dialog in case of full import

        public Tuple<XmlNode, string, XmlNode, XmlNode> GetFullDetailsForDialog() {
            Tuple<string, string> getXbrlImportData = GetXbrlDetailsForDialog();
            string year = getXbrlImportData.Item2;

            XmlNode xnListRoot = _xmlDoc.SelectSingleNode("//*[name()='xbrli:xbrl']");

            if(xnListRoot == null) {
                return null;
            }

            XmlNode companyParent = _xmlDoc.CreateNode(XmlNodeType.Element, "company", null);
            List<XmlNode> nodeList = xnListRoot.ChildNodes.Cast<XmlNode>().Where(node => node.Name.StartsWith("genInfo.company.id")).ToList();
            foreach(XmlNode node in nodeList) {
                companyParent.AppendChild(node);   
            }

            XmlNode reportNode = _xmlDoc.SelectSingleNode("//*[name()='report']");
            XmlNode systemNode = _xmlDoc.SelectSingleNode("//*[name()='system']");

            return new Tuple<XmlNode, string, XmlNode, XmlNode>(companyParent, year, systemNode, reportNode);
        }

        //Does the import
        public void FullImport(Tuple<string, string> reportToImport, Structures.DbMapping.System system, Tuple<string, string> companyToImport, string financialYear) {
            _reportComment = reportToImport.Item2;
            Import(reportToImport.Item1, system, companyToImport, financialYear, true);
        }


        public bool Import(string reportName, Structures.DbMapping.System system, Tuple<string, string> companyToImport = null, string financialYear = null, bool isFullImport = false){

            _log.Log(LogLevelEnum.Info, "This is the entry point of Import()", true);
            
            bool isNeedToAssignCompany = false;
            Company companyToAssign = null;
            int financialYearXbrl = 0;
            ITaxonomyInfo taxonomyInfo = null;
            ITaxonomyInfo gcdTaxonomyInfo = null;
            _reportName = reportName;
            _system = system;
            _isFullImport = isFullImport;

            if(isFullImport) {
                foreach (Structures.DbMapping.System sys in SystemManager.Instance.Systems.Where(sys => system.Name.ToLower() == sys.Name.ToLower())) {
                    _system = sys;
                }    
            }

            XmlNodeList taxonomyNodes = _xmlDoc.SelectNodes("//*[name()='" + TaxonomyNode + "']");
            if (taxonomyNodes == null) {
                return false;
            }
            foreach (XmlNode node in taxonomyNodes) {
                if (node.Attributes != null && node.Attributes[TaxonomyAttribute] != null && !node.Attributes[TaxonomyAttribute].Value.Contains(TaxonomyPrefix)) {
                    taxonomyInfo = TaxonomyManager.GetTaxonomyInfoByFileName(node.Attributes[TaxonomyAttribute].Value.Split('/').Last());
                } else if (node.Attributes != null && node.Attributes[TaxonomyAttribute] != null) {
                    gcdTaxonomyInfo = TaxonomyManager.GetTaxonomyInfoByFileName(node.Attributes[TaxonomyAttribute].Value.Split('/').Last());
                }
            }

            if (companyToImport == null) {
                XmlNode companyNode = _xmlDoc.SelectSingleNode("//*[name()='genInfo.company.id.name']");
                XmlNode financialYearNode = _xmlDoc.SelectSingleNode("//*[name()='xbrli:instant']");

                if (companyNode == null || financialYearNode == null) {
                    throw new Exception(ResourcesCommon.ImportXbrlNoCompanyOrFinancialYear);
                }

                financialYearXbrl = DateTime.Parse(financialYearNode.InnerText, new CultureInfo("de-DE")).Year;

                string companyNameXbrl = companyNode.InnerText;
                foreach (Company company in CompanyManager.Instance.Companies) {
                    if (company.Name.ToLower() == companyNameXbrl.ToLower()) {
                        isNeedToAssignCompany = true;
                        companyToAssign = company;
                        break;
                    }
                }
                if (!isNeedToAssignCompany) {
                    companyToAssign = new Company
                                      {TaxonomyInfo = TaxonomyManager.GetTaxonomyInfo(TaxonomyType.GCD, gcdTaxonomyInfo.Version)};
                    companyToAssign.SetFinancialYearIntervall(2009, 2030);
                    companyToAssign.SetVisibleFinancialYearIntervall(DateTime.Now.Year, DateTime.Now.Year);
                    companyToAssign.Name = companyNameXbrl;
                    CompanyManager.Instance.AddCompany(companyToAssign);
                }
            }
            else {

                bool isAssigned = false;
                financialYearXbrl = Convert.ToInt16(financialYear);

                string companySt13ToImport = companyToImport.Item1;
                string companyNameToImport = companyToImport.Item2;

                if (companySt13ToImport != string.Empty) {
                    foreach (Company company in CompanyManager.Instance.Companies) {
                        var tupleValue =
                            company.ValueTree.Root.Values["de-gcd_genInfo.company.id.idNo"] as XbrlElementValue_Tuple;
                        if (tupleValue != null &&
                            tupleValue.Items[0].Values["de-gcd_genInfo.company.id.idNo.type.companyId.ST13"].Value !=
                            null &&
                            tupleValue.Items[0].Values["de-gcd_genInfo.company.id.idNo.type.companyId.ST13"].Value.
                                ToString() == companySt13ToImport) {
                            companyToAssign = company;
                            isAssigned = true;
                            break;
                        }
                    }
                }
                if (!isAssigned && companyNameToImport != string.Empty) {
                    foreach (Company company in CompanyManager.Instance.Companies) {
                        if (company.ValueTree.Root.Values["de-gcd_genInfo.company.id.name"].Value != null && company.ValueTree.Root.Values["de-gcd_genInfo.company.id.name"].Value.ToString() == companyNameToImport) {
                            companyToAssign = company;
                            isAssigned = true;
                            break;
                        }
                    }
                }
                if(!isAssigned) {
                    CompanyManager.Instance.CurrentCompany = null;
                    companyToAssign = new Company
                                      {TaxonomyInfo = TaxonomyManager.GetTaxonomyInfo(TaxonomyType.GCD, gcdTaxonomyInfo.Version)};
                    companyToAssign.SetFinancialYearIntervall(2009, 2030);
                    companyToAssign.SetVisibleFinancialYearIntervall(DateTime.Now.Year, DateTime.Now.Year);
                    companyToAssign.Name = companyNameToImport;
                    CompanyManager.Instance.AddCompany(companyToAssign);
                }
            }

            _company = companyToAssign;

            bool isDocumentAssigned = false;

            if (isFullImport) {
                foreach (Document doc in DocumentManager.Instance.Documents) {
                    if (doc.System == system && doc.Company == companyToAssign && doc.FinancialYear.FYear == financialYearXbrl && doc.Name.ToLower() == reportName.ToLower()) {
                        _document = doc;
                        isDocumentAssigned = true;
                        break;
                    }
                }
            }

            if(!isDocumentAssigned || !isFullImport) {
                _document = DocumentManager.Instance.AddDocument(taxonomyInfo, gcdTaxonomyInfo);
            }

            _document.Company = companyToAssign;
            CompanyManager.Instance.CurrentCompany = companyToAssign;
            CompanyManager.Instance.SaveCompany(companyToAssign);

            foreach (FinancialYear year in CompanyManager.Instance.CurrentCompany.FinancialYears) {
                if (year.FYear == financialYearXbrl) {
                    _document.FinancialYear = year;
                    break;
                }
            }

            _document.ReportRights = new ReportRights(_document);
            _document.LoadDetails(new ProgressInfo());
            if (_reportName != null) {
                _document.Name = _reportName;
            }
            if (_reportComment != null) {
                _document.Comment = _reportComment;
            }
            if (_reportName != null) {
                _document.System = _system;
            }
            DocumentManager.Instance.CurrentDocument = _document;
            DocumentManager.Instance.SaveDocument(_document);
            model = new ReconciliationsModel(DocumentManager.Instance.CurrentDocument);
            
            //Clear ValueTree values
            foreach (KeyValuePair<string, IElement> element in DocumentManager.Instance.CurrentDocument.MainTaxonomy.Elements) {
                IValueTreeEntry entry = DocumentManager.Instance.CurrentDocument.ValueTreeMain.GetValue(element.Key);
                if (entry != null) {
                    entry.Value = null;
                }
            }

            try {
                ImportAccountBalances();
            }
            catch(Exception ex) {
                using (NDC.Push(LogHelper.GetNamespaceContext()))
                {
                    _log.ErrorWithCheck("Exception occurred while doing ImportAccountBalances()", ex);
                }
                throw;
            }

            try {
                ImportPositions(PrefixNodeName1);
                ImportPositions(PrefixNodeName2);
                ImportPositions(PrefixNodeName3);
                ImportPositions(PrefixNodeName4);
            }
            catch (Exception ex) {
                _log.ErrorWithCheck("Exception occurred while doing ImportPositions()", ex);
                throw;
            }

            try {
                ImportFootNotes(PrefixNodeName1);
                ImportFootNotes(PrefixNodeName2);
                ImportFootNotes(PrefixNodeName3);
                ImportFootNotes(PrefixNodeName4);
            }
            catch (Exception ex) {
                _log.ErrorWithCheck("Exception occurred while doing ImportFootNotes()", ex);
                throw;
            }


            if(isFullImport) {
                try {
                    ImportPositionComments();
                }
                catch (Exception ex) {
                    _log.ErrorWithCheck("Exception occurred while doing ImportPositionComments()", ex);
                    throw;
                }
                try {
                    ImportViewConfigurations();
                }
                catch (Exception ex) {
                    _log.ErrorWithCheck("Exception occurred while doing ImportViewConfigurations()", ex);
                    throw;
                }
            }

            try {
                ImportReconciliations();
            }
            catch (Exception ex) {
                _log.ErrorWithCheck("Exception occurred while doing ImportReconciliations()", ex);
                throw;
            }

            try {
                ImportGeneralInfo(isNeedToAssignCompany);
            }
            catch (Exception ex) {
                _log.ErrorWithCheck("Exception occurred while doing ImportGeneralInfo()", ex);
                throw;
            }

            var tmp = DocumentManager.Instance.CurrentDocument;
            DocumentManager.Instance.CurrentDocument = null;
            DocumentManager.Instance.CurrentDocument = tmp;

            DocumentManager.Instance.CurrentDocument.LoadDetails(null);

            return !HasErrors;
        }

        private void ImportPositionComments() {
            XmlNode xnList = _xmlDoc.SelectSingleNode("//*[name()='" + CommentsTaxonomyPositions + "']");

            if (xnList != null) {
                foreach (XmlNode node in xnList) {
                    string nodeName = node.Name.Replace(".comment", string.Empty);
                    IValueTreeEntry valueEntry = DocumentManager.Instance.CurrentDocument.ValueTreeMain.GetValue(nodeName);
                    if(valueEntry != null) {
                        XbrlElementValueBase baseValue = valueEntry as XbrlElementValueBase;
                        if (baseValue != null) {
                            baseValue.Comment = node.InnerText;
                        }
                    }
                }
            }
        }

        private void ImportViewConfigurations() {
            XmlNode xnList = _xmlDoc.SelectSingleNode("//*[name()='" + ViewConfigurations + "']");

            if (xnList != null) {
                foreach (XmlNode node in xnList) {
                    switch(node.Name)
                    {
                        case "ShowSelectedLegalForm":
                                DocumentManager.Instance.CurrentDocument.Settings.ShowSelectedLegalForm = Boolean.Parse(node.InnerText);
                                break;
                        case "ShowSelectedTaxonomy":
                                DocumentManager.Instance.CurrentDocument.Settings.ShowSelectedTaxonomy = Boolean.Parse(node.InnerText);
                                break;
                        case "ShowTypeOperatingResult":
                                DocumentManager.Instance.CurrentDocument.Settings.ShowTypeOperatingResult = Boolean.Parse(node.InnerText);
                                break;
                        case "ShowOnlyMandatoryPostions":
                                DocumentManager.Instance.CurrentDocument.Settings.ShowOnlyMandatoryPostions = Boolean.Parse(node.InnerText);
                                break;
                    }
                }
                DocumentManager.Instance.CurrentDocument.Settings.SaveConfiguration();
            }
        }

        private void ImportPositions(string taxonomyNodeId) {

            if (HasErrors)
                return;

            ProgressInfo progress = new ProgressInfo();
            DocumentManager.Instance.CurrentDocument.LoadDetails(progress);         

            XmlNodeList xnList = _xmlDoc.SelectNodes("//*[starts-with(name(), '" + taxonomyNodeId + ":')]");
            if (xnList != null && xnList.Count > 0) {
                foreach (XmlNode xn in xnList) {
                    ImportXbrlPositionValue(taxonomyNodeId, xn, null);
                }
            }
        }

        private void ImportFootNotes(string taxonomyNodeId) {
            XmlNodeList xnList = _xmlDoc.SelectNodes("//*[starts-with(name(), '" + FootNoteNode +"')]");
            if (xnList != null && xnList.Count > 0) {
                foreach (XmlNode xn in xnList) {
                    XmlNode child = xn["link:footnote"];
                    if(child != null) {
                        string value = child.InnerText;
                        string id = null;
                        if(child.Attributes != null && child.Attributes["xlink:label"] != null) {
                            id = child.Attributes["xlink:label"].Value;
                        }
                        if(id == null) {continue;}
                        IValueTreeEntry entry = Document.ValueTreeMain.GetValue(taxonomyNodeId + "_" + id);
                        if(entry != null) {
                            entry.Value = System.Net.WebUtility.HtmlEncode(value);
                        }
                    }
                }
            }

        }

        private void ImportXbrlPositionValue(string taxonomyNodeId, XmlNode xn, ValueTreeNode valueTreeNode = null) {
            string posName = xn.Name.Split(':').Last();

            if (!posName.Contains(DetailedInfoNode) && !posName.StartsWith(ReconciliationTransferNodeName)) {
                string posValue = xn.InnerText;
                //var tree = DocumentManager.Instance.CurrentDocument.GaapPresentationTrees.Values.FirstOrDefault(t => t.Nodes.Any(node => node.Element.Id.Equals(taxonomyNodeId + "_" + posName)));

                PresentationTreeNode node = null;

                foreach (PresentationTree tree in DocumentManager.Instance.CurrentDocument.GaapPresentationTrees.Values) {
                    node = GetPresentationTreeItemList(tree, posName);
                    if (node != null){
                        break;
                    }
                }

                if (node == null){
                    return;
                }

                IValueTreeEntry valueEntry;

                if(valueTreeNode != null) {
                    valueEntry = valueTreeNode.Values.ContainsKey(xn.Name.Replace(":", "_")) ? valueTreeNode.Values[xn.Name.Replace(":", "_")] : valueTreeNode.Values[TaxonomyPrefix + "_" + xn.Name];
                }
                    
                else {
                    valueEntry = DocumentManager.Instance.CurrentDocument.ValueTreeMain.GetValue(taxonomyNodeId + "_" + posName);
                }

                if (valueEntry == null)
                    return;

                switch (valueEntry.Element.ValueType) {
                    case XbrlElementValueTypes.String:
                        XbrlElementValue_String stringValue = valueEntry as XbrlElementValue_String;
                        Debug.Assert(stringValue != null);
                        stringValue.Value = xn.InnerText;
                        break;
                    case XbrlElementValueTypes.Date:
                        XbrlElementValue_Date dateValue = valueEntry as XbrlElementValue_Date;
                        Debug.Assert(dateValue != null);
                        CultureInfo ci = Thread.CurrentThread.CurrentCulture;
                        if (xn.InnerText != string.Empty) {
                            dateValue.Value = Convert.ToDateTime(xn.InnerText, ci);
                            dateValue.SaveValueToDb();
                        }
                        break;
                    case XbrlElementValueTypes.Int:
                        XbrlElementValue_Int intValue = valueEntry as XbrlElementValue_Int;
                        Debug.Assert(intValue != null);
                        if (xn.InnerText != string.Empty) {
                            intValue.IntValue = Convert.ToInt32(xn.InnerText);
                        }
                        break;
                    case XbrlElementValueTypes.Boolean:
                        XbrlElementValue_Boolean boolValue = valueEntry as XbrlElementValue_Boolean;
                        Debug.Assert(boolValue != null);
                        if (xn.InnerText != string.Empty) {
                            boolValue.Value = Boolean.Parse(xn.InnerText);   
                        }
                        break;
                    case XbrlElementValueTypes.Numeric:
                        XbrlElementValue_Numeric numericValue = valueEntry as XbrlElementValue_Numeric;
                        Debug.Assert(numericValue != null);
                        Decimal dcm;
                        if (decimal.TryParse(xn.InnerText, out dcm)) {
                            numericValue.NumericValue = dcm;
                        }
                        break;
                    case XbrlElementValueTypes.Monetary:

                        if (xn.Attributes != null && xn.Attributes["sendacc"] != null) {
                            valueEntry.SendAccountBalances = bool.Parse(xn.Attributes["sendacc"].Value);
                        }
                        else {
                            valueEntry.SendAccountBalances = false;
                        }

                        if (xn.Attributes != null && xn.Attributes["autocompute"] != null) {
                            valueEntry.AutoComputeEnabled = bool.Parse(xn.Attributes["autocompute"].Value);
                        }
                        else {
                            valueEntry.AutoComputeEnabled = false;
                        }

                        if (posValue != string.Empty) {

                            double sumOfChildren = GetDirectChildSumOfPosition(taxonomyNodeId, posName);
                            double accountSumOfPosition = GetAccountSumOfPosition(posName, valueEntry.Element);
                            double xbrlPosValue = Convert.ToDouble(posValue);
                            object positionValueContainer;

                            if (xbrlPosValue.Equals(sumOfChildren) || xbrlPosValue.Equals(accountSumOfPosition) || xbrlPosValue.Equals(Math.Round(accountSumOfPosition + sumOfChildren, 2))) {
                                positionValueContainer = null;
                            }
                            else {
                                positionValueContainer = xbrlPosValue - sumOfChildren;
                            }

                            if (!xbrlPosValue.Equals(sumOfChildren)) {
                                XbrlElementValue_Monetary monetaryValue = valueEntry as XbrlElementValue_Monetary;
                                if (monetaryValue != null && !monetaryValue.Element.HasComputationTargets) {
                                    monetaryValue.Value = positionValueContainer;
                                }
                            }
                        }
                        break;
                    case XbrlElementValueTypes.Tuple:
                        
                        XbrlElementValue_Tuple tupleValue = valueEntry as XbrlElementValue_Tuple;
                        if (tupleValue == null) {
                            return;
                        }

                        /*
                        if (_isFullImport) {
                            valueTree = !tupleValue.Items.Any() ? tupleValue.AddValue() : tupleValue.Items.First();
                        }
                        else {
                            valueTree = tupleValue.AddValue();
                        }
                         */

                        if (valueEntry.Element.Id == "de-gaap-ci_nt.segmBusiness") {
                            if(!isSegmBusinessDeleted) {
                                clearTupleValue(tupleValue);
                                isSegmBusinessDeleted = true;
                            }
                        }
                        else if (valueEntry.Element.Parents.Count == 0) {
                            clearTupleValue(tupleValue);
                        }

                        ValueTreeNode valueTree = tupleValue.AddValue();

                        foreach (XmlNode child in xn.ChildNodes) {
                            if(!valueEntry.Element.IsList)
                                ImportXbrlPositionValue(taxonomyNodeId, child, valueTree);
                        }
                        break;
                    case XbrlElementValueTypes.SingleChoice:
                        XbrlElementValue_SingleChoice singleChoiceValue = valueEntry as XbrlElementValue_SingleChoice;
                        Debug.Assert(singleChoiceValue != null);
                        foreach (IElement item in singleChoiceValue.Elements) {
                            if (item.Name == xn.FirstChild.Name.Split(':').Last()){
                                singleChoiceValue.SelectedValue = item;
                            }
                        }
                        break;
                    case XbrlElementValueTypes.MultipleChoice:
                        XbrlElementValue_MultipleChoice multipleChoiceValue = valueEntry as XbrlElementValue_MultipleChoice;
                        Debug.Assert(multipleChoiceValue != null);
                        foreach (IElement item in (multipleChoiceValue.Elements)) {
                            multipleChoiceValue.IsChecked[item.Id].BoolValue = null;
                            foreach (XmlNode child in xn.ChildNodes) {
                                bool isChecked = true;
                                if (item.Id == TaxonomyPrefix + "_" + child.Name && child.Attributes != null){
                                    if (child.Attributes[MultipleChoiceisCheckedAttr] != null && child.Attributes[MultipleChoiceisCheckedAttr].Value == "true"){
                                        isChecked = false;
                                    }
                                    multipleChoiceValue.IsChecked[item.Id].BoolValue = isChecked;
                                    break;
                                }
                            }
                        }
                        break;
                    case XbrlElementValueTypes.HyperCubeContainerItem:
                        break;
                    default:
                        XbrlImportErrors.Add(new XbrlImportError
                        {
                            Position = posName,
                            FilePath = _fileName,
                            ErrorDescription =
                                                      "Unknown value entry type(" +
                                                      valueEntry.Element.ValueType.ToString() +
                                                      ") at this position: " + posName
                        });
                        throw new NotImplementedException();
                }
            }
        }

        private void clearTupleValue(XbrlElementValue_Tuple tupleValue) {
            while (tupleValue.Items.Count > 0) {
                tupleValue.DeleteValue(tupleValue.Items.FirstOrDefault());
            }
        }

        private PresentationTreeNode GetPresentationTreeItemList(PresentationTree tree, string posName) {
            
            PresentationTreeNode resultNode = null;
            foreach (PresentationTreeNode item in tree) {
                resultNode = GetPresentationTreeItem(item, posName);
                if(resultNode != null) {
                    break;
                }
            }
            return resultNode;
        }

        private PresentationTreeNode GetPresentationTreeItem(PresentationTreeNode node, string posName) {
           
            PresentationTreeNode foundItem = null;

            foreach (PresentationTreeNode item in node) {
                    if (item.Element.Name == posName) {
                        foundItem = item;
                        break;
                    }
                    if (item.FirstOrDefault() is PresentationTreeNode){
                        PresentationTreeNode nextItem = GetPresentationTreeItem(item, posName);
                        if (nextItem != null) {
                            foundItem = nextItem;
                            break;
                        }
                    }
            }
            return foundItem;
        }


        private double GetDirectChildSumOfPosition(string taxonomyNodeId, string positionName) {

            PresentationTreeNode node = null;

            foreach (PresentationTree tree in DocumentManager.Instance.CurrentDocument.GaapPresentationTrees.Values) {
                node = GetPresentationTreeItemList(tree, positionName);
                if (node != null) {
                    break;
                }
            }

            double amountOfDirectChildren = 0;

            if (node == null || node.Children == null) {
                return 0;
            }

            var summationTargets = node.Element.SummationTargets;
            foreach (SummationItem entry in summationTargets) {
                XmlNode nodeXbrl = _xmlDoc.SelectSingleNode("//*[name()='" + entry.Element.Id.Replace(taxonomyNodeId + "_", taxonomyNodeId + ":") + "']");
                IValueTreeEntry valueEntry = DocumentManager.Instance.CurrentDocument.ValueTreeMain.GetValue(entry.Element.Id);
                if (valueEntry == null) {
                    continue;
                }

                bool hasPositiveSources = valueEntry.Element.HasPositiveComputationSources;
                bool isDavonPosition = valueEntry.IsComputationOrphanedNode && valueEntry.HasMonetaryParent;
                bool isSumUp = valueEntry.AutoComputeEnabled;

                if (nodeXbrl == null) {
                    continue;
                }
                double outPosValue;
                if (double.TryParse(nodeXbrl.InnerText, out outPosValue)) {

                    if (hasPositiveSources || (isSumUp && isDavonPosition)) {
                        amountOfDirectChildren += outPosValue;
                    }
                    else {
                        amountOfDirectChildren -= outPosValue;
                    }
                }
            }

            /*
            foreach(IPresentationTreeEntry entry in node.Children) {
                if (entry is PresentationTreeNode) {
                    XmlNode nodeXbrl = _xmlDoc.SelectSingleNode("//*[name()='" + entry.Element.Id.Replace(taxonomyNodeId + "_", taxonomyNodeId + ":") + "']");

                    IValueTreeEntry valueEntry = DocumentManager.Instance.CurrentDocument.ValueTreeMain.GetValue(entry.Element.Id);
                    if(valueEntry == null) {
                        continue;
                    }
                    bool hasPositiveSources = valueEntry.Element.HasPositiveComputationSources;
                    bool isDavonPosition = valueEntry.IsComputationOrphanedNode && valueEntry.HasMonetaryParent;
                    bool isSumUp = valueEntry.AutoComputeEnabled;

                    if (nodeXbrl == null) {
                        continue;
                    }
                    double outPosValue;
                    if (double.TryParse(nodeXbrl.InnerText, out outPosValue)) {

                        if (hasPositiveSources || (isSumUp && isDavonPosition)) {
                            amountOfDirectChildren += outPosValue;   
                        }
                        else {
                            amountOfDirectChildren -= outPosValue;   
                        }
                    }
                }
            }
             */
            return Math.Round(amountOfDirectChildren, 2); 
        }

        private double GetAccountSumOfPosition(string positionName, IElement element) {

            double accountSumOfPosition = 0;

            XmlNodeList xnList = _xmlDoc.SelectNodes("//*[name()='de-gaap-ci:" + DetailedInfoNode + "." + AccountBalanceInfoNode + "']");
            if (xnList == null || xnList.Count == 0) {
                return 0;
            }
            foreach (XmlNode xn in xnList) {
                string position = string.Empty;
                string amount = string.Empty;

                foreach (XmlNode item in xn.ChildNodes) {
                    switch (item.Name.Split('.').Last()) {
                        case "positionName":
                            position = item.InnerText;
                            break;
                        case "amount":
                            amount = item.InnerText;
                            break;
                    }
                }
                if(position == positionName) {
                    accountSumOfPosition += Convert.ToDouble(amount);
                }
            }

            accountSumOfPosition = Math.Round(accountSumOfPosition, 2); 
            
            if(element.IsCreditBalance) {
                if(accountSumOfPosition < 0) {
                    accountSumOfPosition = Math.Abs(accountSumOfPosition);   
                }
                else {
                    accountSumOfPosition = 0 - accountSumOfPosition;
                }
            }

            return accountSumOfPosition;
        }


        private void ImportReconciliations() {

            if (HasErrors)
                return;

            string taxonomyNodeId = PrefixNodeName1;
            string reconciliationNodeId = ReconciliationTransferNodeName;
            string transferNodeName = taxonomyNodeId + ":" + reconciliationNodeId;

            XmlNodeList xnList = _xmlDoc.SelectNodes("//*[name()='" + transferNodeName + "']");

            if (xnList != null && xnList.Count > 0) {
                DocumentManager.Instance.CurrentDocument.ReconciliationManager.DeleteAllReconciliations();
                foreach (XmlNode xn in xnList) {
                    XmlNodeList childs = xn.ChildNodes;

                    string reconciliationKind = xn[transferNodeName + "." + ReconciliationKindNodeName].FirstChild.Name.Split('.').Last();
                    TransferKinds transferKind;

                    switch (reconciliationKind.ToLower()) {
                        case "reclassificationchangevalue":
                            transferKind = TransferKinds.ReclassificationWithValueChange;
                            break;
                        case "changevalue":
                            transferKind = TransferKinds.ValueChange;
                            break;
                        case "reclassification":
                            transferKind = TransferKinds.Reclassification;
                            break;
                        default:
                            XbrlImportErrors.Add(new XbrlImportError {
                                FilePath = _fileName,
                                ErrorDescription = "Unknown reconciliation type in XBRL!"
                             });
                             throw new NotImplementedException();
                    }
                    bool isSuccessImport;
                    ImportReconciliation(taxonomyNodeId, transferKind, childs, transferNodeName, out isSuccessImport);
                }
            }
        }

        private void ImportGeneralInfo(bool isNeedToAssignCompany) {

            if (HasErrors)
                return;

            ValueTreeNode valueTreeRoot = DocumentManager.Instance.CurrentDocument.ValueTreeGcd.Root;

            XmlNodeList xnList = _xmlDoc.SelectNodes("//*[starts-with(name(), '" + GenInfoNodeName + "')]");

            if (xnList != null && xnList.Count > 0) {
                foreach (XmlNode xn in xnList) {
                    //if it is not a childnode
                    if(xn.Attributes != null && xn.Attributes["xmlns"] != null) {
                        if (!xn.Name.StartsWith("genInfo.company") || !isNeedToAssignCompany) {
                            ImportGeneralNode(xn, valueTreeRoot);      
                        }
                    }
                }
            }
        }

        private void ImportGeneralNode(XmlNode node, ValueTreeNode valueTreeNode) {

            IValueTreeEntry valueEntry;
            if(valueTreeNode.Values.ContainsKey(node.Name.Replace(":", "_"))) {
                valueEntry = valueTreeNode.Values[node.Name.Replace(":", "_")];
            }
            else {
                if (valueTreeNode.Values.ContainsKey(TaxonomyPrefix + "_" + node.Name)) {
                    valueEntry = valueTreeNode.Values[TaxonomyPrefix + "_" + node.Name];   
                }
                else {
                    return;
                }
            }

            if (!string.IsNullOrEmpty(node.InnerText) || node.HasChildNodes) {
                ImportXbrlValueTypes(valueEntry, node);
            }
        }

        private void ImportAccountBalances() {

            if (HasErrors)
                return;

            //Clear all account assignments   
            DocumentManager.Instance.CurrentDocument.ClearAssignments(new ProgressInfo());
            DocumentManager.Instance.CurrentDocument.ClearImportedAssignments(new ProgressInfo());

            //Clear all balance lists
            var balLists = DocumentManager.Instance.CurrentDocument.BalanceLists.ToList();
            foreach (var balList in balLists) {
                if (balList.IsImported)
                    BalanceListManager.Instance.RemoveBalanceList(balList);
            }

            Dictionary<string, BalanceList> balanceLists = new Dictionary<string, BalanceList>();
            Dictionary<string, VirtualBalanceList> virtualBalanceLists = new Dictionary<string, VirtualBalanceList>();
            Dictionary<string, IAccountGroup> accountGroups = new Dictionary<string, IAccountGroup>();
            Dictionary<string, ISplitAccountGroup> splittedAccountGroups = new Dictionary<string, ISplitAccountGroup>();
            BalanceList balanceList = null;


            if(_isFullImport) {
                XmlNode xnBalaceLists = _xmlDoc.SelectSingleNode("//*[name()='detailedInformation.accountbalances.balanceListDescription']");
                XmlNode xnAccountGroups = _xmlDoc.SelectSingleNode("//*[name()='detailedInformation.accountbalances.accountGroupDescription']");
                XmlNode xnSplittedAccountGroups = _xmlDoc.SelectSingleNode("//*[name()='detailedInformation.accountbalances.splittedAccountGroupDescription']");
                if (xnBalaceLists != null) {
                    foreach (XmlNode list in xnBalaceLists.ChildNodes) {
                        if (list.Attributes["isImported"].Value.ToLower() == "true") {
                            balanceList = new BalanceList {
                                Name = list.InnerText,
                                ImportedFrom = UserManager.Instance.CurrentUser,
                                Document = DocumentManager.Instance.CurrentDocument,
                                ImportDate = DateTime.Now
                            };
                            if (list.Attributes == null) {
                                continue;
                            }
                            if (list.Attributes["comment"] != null) {
                                balanceList.Comment = list.Attributes["comment"].Value;
                            }
                            BalanceListManager.Instance.AddBalanceList(DocumentManager.Instance.CurrentDocument, balanceList);
                            balanceLists.Add(list.Attributes["id"].Value, balanceList);
                        }
                        else {
                            VirtualBalanceList virtualBalanceList = new VirtualBalanceList();
                            virtualBalanceList.Name = list.InnerText;
                            virtualBalanceList.Comment = list.Attributes["comment"].Value;
                            virtualBalanceList.Source = list.Attributes["source"].Value;
                            virtualBalanceList.ImportedFrom = UserManager.Instance.CurrentUser;
                            virtualBalanceList.Document = DocumentManager.Instance.CurrentDocument;
                            virtualBalanceList.ImportDate = DateTime.Now;
                            virtualBalanceLists.Add(list.Attributes["id"].Value, virtualBalanceList);
                        }
                    }
                }
                if (xnAccountGroups != null) {
                    foreach (XmlNode group in xnAccountGroups.ChildNodes) {
                        if(group.Attributes == null) {
                            continue;
                        }
                        IAccountGroup accountGroup = null;
                        
                        foreach (KeyValuePair<string, BalanceList> bl in balanceLists) {
                            if(bl.Key == group.Attributes["balanceListId"].Value) {
                                accountGroup = AccountGroupManager.CreateAccountGroup(bl.Value);
                                accountGroup.Name = group.InnerText;
                                if (group.Attributes["groupNumber"] != null) {
                                    accountGroup.Number = group.Attributes["groupNumber"].Value;
                                }
                                if (group.Attributes["groupComment"] != null) {
                                    accountGroup.Comment = group.Attributes["groupComment"].Value;
                                }
                                if (accountGroup.BalanceList == bl.Value) {
                                    bl.Value.AddAccountGroup(accountGroup);
                                }

                                if (group.Attributes["assignedElement"] != null) {
                                    string element = Document.MainTaxonomy.Elements[group.Attributes["assignedElement"].Value].Id;
                                    bl.Value.AddAssignment(accountGroup, element);
                                }

                                break;
                            }
                        }                        
                        accountGroups.Add(group.Attributes["id"].Value, accountGroup);
                    }
                }

                if(xnSplittedAccountGroups != null) {
                    foreach (XmlNode group in xnSplittedAccountGroups.ChildNodes) {
                        if(group.Attributes == null) {
                            continue;
                        }
                        SplitAccountGroup splitAccountGroup = null;
                        Account splitAccount = new Account();
                        foreach (KeyValuePair<string, BalanceList> bl in balanceLists) {
                            if (bl.Key == group.Attributes["balanceListId"].Value) {
                                splitAccountGroup = new SplitAccountGroup();
                                splitAccountGroup.BalanceList = bl.Value;                   

                                splitAccount.BalanceList = bl.Value;
                                splitAccount.Number = group.Attributes["originalAccountNumber"].Value;
                                splitAccount.Amount = Convert.ToDecimal(group.Attributes["originalAccountAmount"].Value);
                                splitAccount.Name = group.InnerText;

                                bl.Value.AddItem(splitAccount);
                                using (IDatabase conn = AppConfig.ConnectionManager.GetConnection()){
                                    conn.DbMapping.Save(splitAccount);
                                }
                                splitAccountGroup.Account = splitAccount;
                                bl.Value.AddSplitAccountGroup(splitAccountGroup);
                            }
                        }
                        splittedAccountGroups.Add(group.Attributes["id"].Value, splitAccountGroup);
                    }
                }
            }
            else {
                balanceList = new BalanceList {
                    Name = ResourcesCommon.ImportedBalanceListName,
                    ImportedFrom = UserManager.Instance.CurrentUser,
                    Document = DocumentManager.Instance.CurrentDocument,
                    ImportDate = DateTime.Now
                };
                balanceLists.Add("1", balanceList);
                BalanceListManager.Instance.AddBalanceList(DocumentManager.Instance.CurrentDocument, balanceList);
            }

            XmlNodeList xnList = _xmlDoc.SelectNodes("//*[name()='de-gaap-ci:" + DetailedInfoNode + "." + AccountBalanceInfoNode + "']");

            if (xnList != null && xnList.Count > 0) {
                foreach (XmlNode xn in xnList) {

                    bool isVirtual = false;
                    string virtualTaxonomyPosition = string.Empty;

                    if(_isFullImport) {
                        balanceList = null;
                        string balanceListId = null;
                        foreach (XmlNode child in xn.ChildNodes.Cast<XmlNode>().Where(child => child.Name == "detailedInformation.accountbalances.balanceList")) {
                            balanceListId = child.InnerText;
                            if (child.Attributes.Count > 0) {
                                isVirtual = Boolean.Parse(child.Attributes["isVirtual"].Value);
                                virtualTaxonomyPosition = child.Attributes["taxonomyPosition"].Value;
                            }
                        }

                        if (isVirtual) {
                            foreach (KeyValuePair<string, VirtualBalanceList> vbl in virtualBalanceLists) {
                                if (vbl.Key == balanceListId) {
                                    balanceList = vbl.Value;
                                }
                            }
                        }
                        else {
                            foreach (KeyValuePair<string, BalanceList> bl in balanceLists) {
                                if (bl.Key == balanceListId) {
                                    balanceList = bl.Value;
                                }
                            }
                        }
                    }                  

                    string positionName = string.Empty;
                    string accountNumber = string.Empty;
                    string accountDescription = string.Empty;
                    string amount = string.Empty;
                    string accountGroup = string.Empty;
                    string splittedAccountGroup = string.Empty;
                    string splittedAccountComment = string.Empty;
                    foreach (XmlNode item in xn.ChildNodes) {
                        switch(item.Name.Split('.').Last()) {
                            case "positionName":
                                positionName = item.InnerText;
                                break;
                            case "accountNumber":
                                accountNumber = item.InnerText;
                                break;
                            case "accountDescription":
                                accountDescription = item.InnerText;
                                break;
                            case "amount":
                                amount = item.InnerText;
                                break;
                            case "accountGroup":
                                accountGroup = item.InnerText;
                                break;
                            case "splitAccountGroup":
                                splittedAccountGroup = item.InnerText;
                                if(item.Attributes != null) {
                                    splittedAccountComment = item.Attributes["splittedComment"].Value;
                                }
                                break;
                        }
                    }

                    string taxonomyId = "de-gaap-ci";

                    if (positionName != string.Empty) {
                        XmlNode assignedNode= _xmlDoc.SelectSingleNode("//*[contains(name(), ':" + positionName + "')]");
                        taxonomyId = assignedNode.Name.Split(':').First();
                    }

                    IElement accountPosition = null;

                    if (splittedAccountGroup != string.Empty) {

                        foreach (KeyValuePair<string, ISplitAccountGroup> group in splittedAccountGroups) {
                            if (group.Key == splittedAccountGroup) {
                                using (IDatabase conn = AppConfig.ConnectionManager.GetConnection()) {
                                    conn.DbMapping.Save(group.Value);
                                }
                                ISplittedAccount sAcc = group.Value.AddNewItem();
                                sAcc.Name = accountDescription;
                                sAcc.Number = accountNumber;
                                sAcc.Amount = Convert.ToDecimal(amount);
                                if(splittedAccountComment != string.Empty) {
                                    sAcc.Comment = splittedAccountComment;
                                }
                                if (positionName != string.Empty && DocumentManager.Instance.CurrentDocument.MainTaxonomy.Elements.ContainsKey(taxonomyId + "_" + positionName)) {
                                    accountPosition = DocumentManager.Instance.CurrentDocument.MainTaxonomy.Elements[taxonomyId + "_" + positionName];
                                }
                                if (accountPosition != null) {
                                    sAcc.AssignedElement = accountPosition;
                                    sAcc.AssignedElementId = Document.TaxonomyIdManager.GetId(accountPosition);
                                }
                                using (IDatabase conn = AppConfig.ConnectionManager.GetConnection()) {
                                    conn.DbMapping.Save(sAcc);
                                }
                            }
                        }
                        continue;
                    }

                    if(balanceList == null && _isFullImport) {
                        continue;
                    }

                    if (isVirtual) {
                        //VirtualAccount vaccount = new VirtualAccount(positionName, balanceList);
                        VirtualAccount vaccount = new VirtualAccount(virtualTaxonomyPosition, balanceList);
                        balanceList.AddItem(vaccount);
                        //vaccount.TaxonomyPosition = virtualTaxonomyPosition;
                        if (positionName != string.Empty && DocumentManager.Instance.CurrentDocument.MainTaxonomy.Elements.ContainsKey(taxonomyId + "_" + positionName)){
                            accountPosition = DocumentManager.Instance.CurrentDocument.MainTaxonomy.Elements[taxonomyId + "_" + positionName];
                        }
                        vaccount.AssignedElement = accountPosition;
                        continue;
                    }

                    Account account = new Account {
                        BalanceList = balanceList,
                        Document = DocumentManager.Instance.CurrentDocument
                    };
                    balanceList.AddItem(account);

                    if (positionName != string.Empty && DocumentManager.Instance.CurrentDocument.MainTaxonomy.Elements.ContainsKey(taxonomyId + "_" + positionName)) {
                        accountPosition = DocumentManager.Instance.CurrentDocument.MainTaxonomy.Elements[taxonomyId + "_" + positionName];
                    }

                    account.Name = accountDescription;
                    account.Amount = Convert.ToDecimal(amount);
                    account.Number = accountNumber;

                    if (accountPosition != null) {
                        account.AssignedElement = accountPosition;
                    }

                    
                    if (accountGroup != string.Empty) {
                        foreach (KeyValuePair<string, IAccountGroup> group in accountGroups) {
                            if(group.Key == accountGroup) {
                                using (IDatabase conn = AppConfig.ConnectionManager.GetConnection()) {
                                    conn.DbMapping.Save(group.Value);
                                }
                                group.Value.AddAccount(account);
                                break;
                            }
                        }
                    }
                    
                    if (accountPosition != null) {
                        balanceList.AddAssignment(account, taxonomyId + "_" + positionName);  
                    }
                    else {
                        using (IDatabase conn = AppConfig.ConnectionManager.GetConnection()) {
                            conn.DbMapping.Save(account);
                        }
                   }
                }
                ImportVirtualAccounts(virtualBalanceLists);
            }
        }

        private void ImportVirtualAccounts(Dictionary<string, VirtualBalanceList> virtualBalanceLists) {

            foreach (IBalanceList list in Document.BalanceLists) {
                if(list.IsImported == false) {
                    string source = list.Source;
                    foreach (var savedList in virtualBalanceLists) {
                        if(savedList.Value.Source == source) {
                            foreach (VirtualAccount acc in list.Accounts) {
                                foreach(VirtualAccount accSaved in savedList.Value.Accounts) {
                                    if(acc.TaxonomyPosition == accSaved.TaxonomyPosition) {
                                        Document.TaxonomyIdManager.SetElementAssignment(acc, accSaved.AssignedElement);
                                        acc.Document = Document;
                                        using (IDatabase conn = AppConfig.ConnectionManager.GetConnection()) {
                                            conn.DbMapping.Save(acc);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public void ImportXbrlValueTypes(IValueTreeEntry valueEntry, XmlNode node, bool isTemplate = false) {

            if (valueEntry != null) {

                XmlNode xnOthers = _xmlDoc.SelectSingleNode("//*[name()='otherValues']");

                switch (valueEntry.Element.ValueType) {
                    case XbrlElementValueTypes.String:
                        XbrlElementValue_String stringValue = valueEntry as XbrlElementValue_String;
                        Debug.Assert(stringValue != null);
                        stringValue.Value = node.InnerText;
                        break;
                    case XbrlElementValueTypes.Date:
                        XbrlElementValue_Date dateValue = valueEntry as XbrlElementValue_Date;
                        Debug.Assert(dateValue != null);
                        CultureInfo ci = Thread.CurrentThread.CurrentCulture;
                        dateValue.Value = Convert.ToDateTime(node.InnerText, ci);
                        dateValue.SaveValueToDb();
                        break;
                    case XbrlElementValueTypes.Int:
                        XbrlElementValue_Int intValue = valueEntry as XbrlElementValue_Int;
                        Debug.Assert(intValue != null);
                        intValue.IntValue = Convert.ToInt32(node.InnerText);
                        break;
                    case XbrlElementValueTypes.Boolean:
                        XbrlElementValue_Boolean boolValue = valueEntry as XbrlElementValue_Boolean;
                        Debug.Assert(boolValue != null);
                        boolValue.Value = Boolean.Parse(node.InnerText);
                        break;
                    case XbrlElementValueTypes.Numeric:
                        XbrlElementValue_Numeric numericValue = valueEntry as XbrlElementValue_Numeric;
                        Debug.Assert(numericValue != null);
                        Decimal dcm;
                        if (decimal.TryParse(node.InnerText, out dcm)) {
                            numericValue.NumericValue = dcm;
                        }
                        break;
                    case XbrlElementValueTypes.SingleChoice:
                        XbrlElementValue_SingleChoice singleChoiceValue = valueEntry as XbrlElementValue_SingleChoice;
                        Debug.Assert(singleChoiceValue != null);
                        foreach (IElement item in singleChoiceValue.Elements) {
                            if (item.Name == node.FirstChild.Name) {
                                singleChoiceValue.SelectedValue = item;
                            }
                        }
                        if (_isFullImport) {
                            if (xnOthers != null && xnOthers.ChildNodes.Count > 0) {
                                if (xnOthers[valueEntry.Element.Id + ".other"] != null) {
                                    singleChoiceValue.ValueOther = xnOthers[valueEntry.Element.Id + ".other"].InnerText;
                                }
                            }
                        }
                        if (isTemplate && node.Name == "otherValue") {
                            singleChoiceValue.ValueOther = node.InnerText;
                            return;
                        }
                        break;
                    case XbrlElementValueTypes.MultipleChoice:
                        XbrlElementValue_MultipleChoice multipleChoiceValue = valueEntry as XbrlElementValue_MultipleChoice;
                        Debug.Assert(multipleChoiceValue != null);

                        if (_isFullImport) {
                            if (xnOthers != null && xnOthers.ChildNodes.Count > 0) {
                                if (xnOthers[valueEntry.Element.Id + ".other"] != null) {
                                    multipleChoiceValue.ValueOther = xnOthers[valueEntry.Element.Id + ".other"].InnerText;
                                }
                            }
                        }

                        foreach (IElement item in (multipleChoiceValue.Elements)) {
                            multipleChoiceValue.IsChecked[item.Id].BoolValue = null;
                            foreach (XmlNode child in node.ChildNodes) {
                                object isChecked = true;

                                if (child.Attributes != null && item.Id == TaxonomyPrefix + "_" + child.Name) {
                                    string attrKey = MultipleChoiceisCheckedAttr;
                                    if(isTemplate) {
                                        attrKey = MultipleChoiceisCheckedAttrTemplate;
                                    }
                                    if (child.Attributes[attrKey] != null) {
                                        if (child.Attributes[attrKey].Value == "true") {
                                            isChecked = false;
                                        }
                                        else {
                                            isChecked = null;
                                        }
                                    }
                                    if (isChecked == null) {
                                        multipleChoiceValue.IsChecked[item.Id].BoolValue = null;
                                    }
                                    else {
                                        multipleChoiceValue.IsChecked[item.Id].BoolValue = (bool)isChecked;
                                    }
                                    break;
                                }
                            }
                        }
                        if (isTemplate && node.Name == "otherValue") {
                            multipleChoiceValue.ValueOther = node.InnerText;
                            return;
                        }
                        break;
                    case XbrlElementValueTypes.Tuple:
                        XbrlElementValue_Tuple tupleValue = valueEntry as XbrlElementValue_Tuple;
                        if(tupleValue != null) {
                            //ValueTreeNode valueTree = !tupleValue.Items.Any() ? tupleValue.AddValue() : tupleValue.Items.First();

                            if (valueEntry.Element.Id == TaxonomyPrefix + "_genInfo.company.id.shareholder" && !isShareholderDeleted) {
                                clearTupleValue(tupleValue);
                                isShareholderDeleted = true;
                            }
                            if (valueEntry.Element.Id == TaxonomyPrefix + "_genInfo.company.id.contactAddress" && !isContactPersonDeleted) {
                                clearTupleValue(tupleValue);
                                isContactPersonDeleted = true;
                            }
                            if (valueEntry.Element.Id == TaxonomyPrefix + "_genInfo.doc.author" && !isDocAuthorDeleted) {
                                clearTupleValue(tupleValue);
                                isDocAuthorDeleted = true;
                            }

                            
                            ValueTreeNode valueTree = tupleValue.AddValue();
                            foreach (XmlNode child in node.ChildNodes)
                            {
                                ImportGeneralNode(child, valueTree);
                            }
                        }
                        break;
                    case XbrlElementValueTypes.Monetary:
                        XbrlElementValue_Monetary monetaryValue = valueEntry as XbrlElementValue_Monetary;
                        if(monetaryValue != null) {
                            monetaryValue.Value = Convert.ToDecimal(node.InnerText);   
                        }
                        break;
                    case XbrlElementValueTypes.HyperCubeContainerItem:
                        break;
                    default:
                        XbrlImportErrors.Add(new XbrlImportError {
                            Position = node.Name,
                            FilePath = _fileName,
                            ErrorDescription = "Unknown value entry type(" + valueEntry.Element.ValueType.ToString() + ") at this position: " + node.Name
                        });
                        throw new NotImplementedException();
                }
            }
        }

        private void GetPositionData(XmlNode node, out string positionName, out string positionValue, out bool isPreviousPeriod) {

            isPreviousPeriod = false;
            positionName = string.Empty;
            positionValue = string.Empty;

            if (node[node.Name + "." + ReconciliationNameNodeName] != null) {
                positionName = node[node.Name + "." + ReconciliationNameNodeName].InnerText.Replace(":", "_");
                if (string.IsNullOrEmpty(node[node.Name + "." + ActualPeriodLabel].InnerText)) {
                    if (string.IsNullOrEmpty(node[node.Name + "." + PreviousPeriodLabel].InnerText)) {
                        positionValue = string.Empty;
                    }
                    else {
                        positionValue = node[node.Name + "." + PreviousPeriodLabel].InnerText;
                        isPreviousPeriod = true;
                    }
                }
                else {
                    positionValue = node[node.Name + "." + ActualPeriodLabel].InnerText;
                }
            }
        }

        private bool IsNodeName(XmlNode node, string nodeName) {
            if (node.Name == nodeName) {
                return true;
            }
            return false;
        }

        private void ImportReconciliation(string taxonomyNodeId, TransferKinds transferKind, XmlNodeList nodes, string nodeRootName, out bool isSuccess, DeltaReconciliation valueChange = null) {
            string previousNodeName = string.Empty;
            bool hasPreviousYearValue = false;
            IElement elem = null;

            switch (transferKind) {
                case TransferKinds.Reclassification:
                case TransferKinds.ReclassificationWithValueChange:
                case TransferKinds.ValueChange:
                    string comment = string.Empty;
                    string recName = string.Empty;
                    if (nodes.Count > 0) {
                        //getting comment node
                        foreach (XmlNode node in nodes.Cast<XmlNode>().Where(node => node.Name.Split('.').Last() == CommentNodeName)) {
                            comment = node.InnerText;
                        }
                        if (_isFullImport) {
                            foreach (
                                XmlNode node in
                                    nodes.Cast<XmlNode>().Where(node => node.Name.Split('.').Last() == ReconciliationNameNodeName)) {
                                recName = node.InnerText;
                            }
                        }

                        foreach (XmlNode child in nodes) {
                            if (!IsNodeName(child, nodeRootName + "." + ReconciliationKindNodeName)) {
                                bool isPreviousPeriod;
                                string positionName;
                                string positionValue;
                                GetPositionData(child, out positionName, out positionValue, out isPreviousPeriod);
                                if (previousNodeName.Contains(positionName)) {
                                    continue;
                                }
                                if (DocumentManager.Instance.CurrentDocument.MainTaxonomy.Elements.ContainsKey(positionName)) {
                                    elem = DocumentManager.Instance.CurrentDocument.MainTaxonomy.Elements[positionName];
                                }
                                if (elem == null && !positionName.Contains(taxonomyNodeId)) {
                                    positionName = taxonomyNodeId + "_" + positionName;
                                }
                                previousNodeName = positionName;
                                if (isPreviousPeriod) {
                                    hasPreviousYearValue = true;
                                    if (DocumentManager.Instance.CurrentDocument.MainTaxonomy.Elements.ContainsKey(positionName)) {
                                        elem = DocumentManager.Instance.CurrentDocument.MainTaxonomy.Elements[positionName];
                                        previousYearValues.AddValue(Convert.ToDecimal(positionValue), elem);  
                                    }
                                    continue;
                                }
                                if (elem == null && DocumentManager.Instance.CurrentDocument.MainTaxonomy.Elements.ContainsKey(positionName)) {
                                    elem = DocumentManager.Instance.CurrentDocument.MainTaxonomy.Elements[positionName];
                                }
                                if (elem != null) {
                                    if (valueChange == null) {
                                        model.Document = DocumentManager.Instance.CurrentDocument;
                                        valueChange = (DeltaReconciliation)model.AddReconciliation(ReconciliationTypes.Delta);   
                                    }
                                    valueChange.Comment = comment;
                                    valueChange.AddTransaction(elem);
                                    if(_isFullImport && recName != string.Empty) {
                                        valueChange.Name = recName;    
                                    }
                                    IReconciliationTransaction transaction = valueChange.GetTransaction(elem);
                                    transaction.Value = Convert.ToDecimal(positionValue);
                                    valueChange.Save();
                                }
                            }
                        }
                    }
                    if (hasPreviousYearValue) {
                        var prevYearValues = new Reconciliation.ReconciliationTypes.PreviousYearValues(_document);
                        prevYearValues.Save();
                        _document.ReconciliationManager.PreviousYearValues = prevYearValues;
                        DocumentManager.Instance.CurrentDocument.ReconciliationManager.ImportPreviousYearValues(previousYearValues);
                    }

                    isSuccess = true;
                    break;
                default:
                    isSuccess = false;
                    break;
            }
        }

        public ObservableCollectionAsync<LogEntry> LogEntries {
            get { return LogHelper.LogEntries; }
        }
    }
}