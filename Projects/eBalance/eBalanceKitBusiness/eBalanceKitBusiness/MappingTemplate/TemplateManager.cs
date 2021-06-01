// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-02-14
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Xml;
using DbAccess;
using Taxonomy.Enums;
using Utils;
using eBalanceKitBase.Structures;
using eBalanceKitBusiness.Interfaces.PresentationTree;
using eBalanceKitBusiness.Localisation;
using eBalanceKitBusiness.Logs;
using eBalanceKitBusiness.Manager;
using eBalanceKitBusiness.Structures;
using eBalanceKitBusiness.Structures.DbMapping;
using eBalanceKitBusiness.Structures.DbMapping.MappingTemplate;
using eBalanceKitBusiness.Structures.ValueTree;
using eBalanceKitBusiness.Structures.XbrlElementValue;
using eBalanceKitResources.Localisation;

namespace eBalanceKitBusiness.MappingTemplate {
    /// <summary>
    /// This class provides several template management functions.
    /// </summary>
    public static class TemplateManager {

        #region properties

        #region Templates
        public static ObservableCollectionAsync<MappingTemplateHead> _templates;

        public static ObservableCollectionAsync<MappingTemplateHead> Templates {
            get {
                if (_templates == null) {
                    _templates = new ObservableCollectionAsync<MappingTemplateHead>();
                    Init();
                }
                return _templates;
            }
        }
        #endregion

        private static readonly Dictionary<string, string> DictChangedTaxonomy = new Dictionary<string, string>();
        
        #endregion properties

        #region methods

        #region Init
        public static void Init() {
            Templates.Clear();

            using (var conn = AppConfig.ConnectionManager.GetConnection()) {
                var tmp = conn.DbMapping.Load<MappingTemplateHead>();
                tmp.Sort();
                foreach (var template in tmp) {
                    AddMissingTemplateElements(template);
                    Templates.Add(template);
                }
            }
        }
        #endregion Init

        private static void AddMissingTemplateElements(MappingTemplateHead template) {
            bool saveTemplate = false;
            foreach (var ptree in template.Taxonomy.PresentationTrees) {
                foreach (var root in ptree.RootEntries) {
                    saveTemplate |= AddMissingTemplateElements(template, root);
                }
            }

            if (saveTemplate) SaveTemplate(template);
        }

        private static void InitElementInfos(MappingTemplateHead template, Document document = null) {
            foreach (var ptree in template.Taxonomy.PresentationTrees) {
                foreach (var root in ptree.RootEntries) {
                    CreateTemplate(template, document, root);
                }
            }
        }

        private static bool AddMissingTemplateElements(MappingTemplateHead template,
                                                       Taxonomy.Interfaces.PresentationTree.IPresentationTreeNode root) {
            bool result = false;

            if (root.Element.IsList) return result;

            if (root.Element.ValueType == XbrlElementValueTypes.Monetary) {
                var elementInfo = template.GetElementInfo(root.Element.Id);
                if (elementInfo == null) {
                    result |= true;
                    Debug.WriteLine(root.Element);
                    template.ElementInfos.Add(new MappingTemplateElementInfo {
                        Template = template,
                        ElementId = root.Element.Id
                    });
                }
            }

            foreach (var child in root.Children.OfType<Taxonomy.Interfaces.PresentationTree.IPresentationTreeNode>()) {
                result |= AddMissingTemplateElements(template, child);
            }

            return result;
        }

        #region AddTemplate
        public static void AddTemplate(MappingTemplateHead template) {
            using (var conn = AppConfig.ConnectionManager.GetConnection()) {
                try {
                    SaveTemplate(template, conn);

                    // insert sorted into observable collection
                    var tmp = new List<MappingTemplateHead>(Templates) {template};
                    tmp.Sort();
                    Templates.Clear();
                    foreach (MappingTemplateHead t in tmp) Templates.Add(t);
                } catch (Exception ex) {
                    throw new Exception(ExceptionMessages.TemplateManagerAdd + ex.Message);
                }
            }
        }
        #endregion AddTemplate

        #region DeleteTemplate
        public static void DeleteTemplate(MappingTemplateHead template) {
            using (var conn = AppConfig.ConnectionManager.GetConnection()) {
                try {
                    try {
                        conn.BeginTransaction();
                        LogManager.Instance.DeleteTemplate(template);
                        conn.DbMapping.Delete(template);
                        conn.CommitTransaction();
                    } catch (Exception ex) {
                        conn.RollbackTransaction();
                        throw new Exception(ex.Message, ex);
                    }

                    Templates.Remove(template);
                } catch (Exception ex) {
                    throw new Exception(ExceptionMessages.TemplateManagerDelete + ex.Message);
                }
            }
        }
        #endregion DeleteTemplate

        #region SaveTemplate
        public static void SaveTemplate(MappingTemplateHead template) {

            foreach (var elementInfo in template.ElementInfos) {
                if (elementInfo.PresentationTreeNode == null)
                    continue; // element is not displayed in presentation tree
                var value = elementInfo.PresentationTreeNode.Value;
                elementInfo.AutoComputeEnabled = value.AutoComputeEnabled;
                elementInfo.SupressWarningMessages = value.SupressWarningMessages;
                elementInfo.SendAccountBalances = value.SendAccountBalances;
            }

            using (var conn = AppConfig.ConnectionManager.GetConnection()) {
                try {
                    SaveTemplate(template, conn);
                } catch (Exception ex) {
                    throw new Exception(ExceptionMessages.TemplateManagerUpdate + ex.Message);
                }
            }

            // sort system list
            var tmp = new List<MappingTemplateHead>(Templates);
            tmp.Sort();
            Templates.Clear();
            foreach (MappingTemplateHead t in tmp) Templates.Add(t);
        }

        private static void SaveTemplate(MappingTemplateHead template, IDatabase conn) {
            try {
                var oldTemplate = template.Id > 0;
                // update last modifier and last modified date if the template is not a new created template
                if (oldTemplate) {
                    template.LastModifier = UserManager.Instance.CurrentUser;
                    template.LastModifyDate = DateTime.Now;
                    LogManager.Instance.UpdateTemplate(template);
                }

                conn.BeginTransaction();
                conn.DbMapping.Save(template);
                conn.CommitTransaction();
                // Logging after we save the new template to get the new created Id
                if (!oldTemplate) {
                    LogManager.Instance.NewTemplate(template);
                }
            } catch (Exception ex) {
                conn.RollbackTransaction();
                throw new Exception(ex.Message, ex);
            }
        }
        #endregion SaveTemplate

        #region ImportTemplate
        public static void ImportTemplate(MappingTemplateHead template, string filename) {
            var doc = new XmlDocument();
            doc.Load(filename);

            var root = doc.SelectSingleNode("e_balance_kit_mapping_template");
            if (root == null) return;
            foreach (XmlNode child in root.ChildNodes) {
                switch (child.Name) {
                    case "name":
                        template.Name = child.InnerText;
                        break;

                    case "comment":
                        template.Comment = child.InnerText;
                        break;

                    case "account_structure":
                        template.AccountStructure = child.InnerText;
                        break;

                    case "taxonomy":
                        template.TaxonomyInfo = TaxonomyManager.GetTaxonomyInfo(child.InnerText);
                        break;

                    case "assignments":
                        foreach (XmlNode assignmentNode in child.ChildNodes) {
                            var line = new MappingTemplateLine();
                            if (assignmentNode.Attributes != null) {
                                foreach (XmlAttribute attr in assignmentNode.Attributes) {
                                    switch (attr.Name) {
                                        case "number":
                                            line.AccountNumber = attr.Value;
                                            break;

                                        case "name":
                                            line.AccountName = attr.Value;
                                            break;

                                        case "debit_element_id":
                                            line.DebitElementId = attr.Value;
                                            break;

                                        case "credit_element_id":
                                            line.CreditElementId = attr.Value;
                                            break;
                                    }
                                }
                            }

                            line.Template = template;
                            template.Assignments.Add(line);
                        }
                        break;
                }
            }

            InitElementInfos(template);
            foreach (XmlNode child in root.ChildNodes) {
                switch (child.Name) {

                    case "element_infos":
                        foreach (XmlNode elementInfoNode in child.ChildNodes) {
                            if (elementInfoNode.Attributes != null) {
                                string id = null;
                                bool autoComputeEnabled = false;
                                bool sendAccountBalances = false;
                                bool supressWarningMessages = false;
                                foreach (XmlAttribute attr in elementInfoNode.Attributes) {
                                    switch (attr.Name) {
                                        case "id":
                                            id = attr.Value;
                                            break;

                                        case "auto_compute_enabled":
                                            autoComputeEnabled = Boolean.Parse(attr.Value);
                                            break;

                                        case "send_account_balances":
                                            sendAccountBalances = Boolean.Parse(attr.Value);
                                            break;

                                        case "supress_warning_messages":
                                            supressWarningMessages = Boolean.Parse(attr.Value);
                                            break;
                                    }
                                }

                                if (String.IsNullOrEmpty(id)) {
                                    System.Diagnostics.Debug.WriteLine("Error in XML-Data: Found element_info without id attribute " + elementInfoNode.Name);
                                    continue;
                                    //throw new Exception("Fehler in XML-Datei: Found element_info without id attribute");
                                }
                                var elementInfo = template.GetElementInfo(id);

                                if (elementInfo == null) {
                                    //throw new Exception("Fehler in XML-Datei: Could not find element info with id '" + id + "'");
                                    System.Diagnostics.Debug.WriteLine("Error in XML-Data: Could not find element info with id '" + id + "'");
                                    continue;
                                }

                                elementInfo.AutoComputeEnabled = autoComputeEnabled;
                                elementInfo.SendAccountBalances = sendAccountBalances;
                                elementInfo.SupressWarningMessages = supressWarningMessages;
                            }

                        }
                        break;
                }
            }
            // Logging the import with file content and path
            LogManager.Instance.ImportTemplate(template, doc, filename);

            AddMissingTemplateElements(template);
        }
        #endregion ImportTemplate

        public static XmlDocument GetTemplateXml(MappingTemplateHead template) {
            var res = new XmlDocument();
            using (XmlWriter w = res.CreateNavigator().AppendChild()) {
                w.WriteStartDocument();
                w.WriteStartElement("e_balance_kit_mapping_template");

                w.WriteStartElement("name");
                w.WriteString(template.Name);
                w.WriteEndElement();

                w.WriteStartElement("comment");
                w.WriteString(template.Comment);
                w.WriteEndElement();

                w.WriteStartElement("account_structure");
                w.WriteString(template.AccountStructure);
                w.WriteEndElement();

                w.WriteStartElement("taxonomy");
                w.WriteString(template.TaxonomyInfo.Name);
                w.WriteEndElement();

                w.WriteStartElement("assignments");
                foreach (MappingTemplateLine assignment in template.Assignments) {
                    w.WriteStartElement("assignment");

                    w.WriteAttributeString("number", assignment.AccountNumber);
                    w.WriteAttributeString("name", assignment.AccountName);
                    if (assignment.DebitElementId != null)
                        w.WriteAttributeString("debit_element_id", assignment.DebitElementId);
                    if (assignment.CreditElementId != null)
                        w.WriteAttributeString("credit_element_id", assignment.CreditElementId);

                    w.WriteEndElement();
                }
                w.WriteEndElement();

                w.WriteStartElement("element_infos");
                foreach (var elementInfo in template.ElementInfos) {
                    w.WriteStartElement("element_info");
                    w.WriteAttributeString("id", elementInfo.ElementId);
                    w.WriteAttributeString("auto_compute_enabled", elementInfo.AutoComputeEnabled.ToString(System.Globalization.CultureInfo.InvariantCulture));
                    w.WriteAttributeString("send_account_balances", elementInfo.SendAccountBalances.ToString(System.Globalization.CultureInfo.InvariantCulture));
                    w.WriteAttributeString("supress_warning_messages", elementInfo.SupressWarningMessages.ToString(System.Globalization.CultureInfo.InvariantCulture));

                    w.WriteEndElement();
                }
                w.WriteEndElement();

                w.WriteEndElement();
                w.WriteEndDocument();
            }
            return res;
        }

        #region ExportTemplate
        public static void ExportTemplate(MappingTemplateHead template, string filename) {
            var settings = new XmlWriterSettings
            {Indent = true, IndentChars = ("    "), Encoding = Encoding.GetEncoding("utf-8")};

            using (XmlWriter w = XmlWriter.Create(filename, settings)) {
                GetTemplateXml(template).Save(w);
            }
        }
        #endregion ExportTemplate

        #region CreateTemplate
        public static MappingTemplateHead CreateTemplate(Document document = null) {
            MappingTemplateHead template;
            if (document != null) {
                template = new MappingTemplateHead {
                    TaxonomyInfo = document.MainTaxonomyInfo,
                    Name = document.Company.Name + " / " + document.System.Name,
                    CreationDate = DateTime.Now,
                    Creator = UserManager.Instance.CurrentUser
                };

            } else {
                template = new MappingTemplateHead {
                    CreationDate = DateTime.Now,
                    Creator = UserManager.Instance.CurrentUser
                };
            }

            return template;
        }

        private static void CreateTemplate(MappingTemplateHead template, Document document,
                                           Taxonomy.Interfaces.PresentationTree.IPresentationTreeNode root) {
            if (root.Element.ValueType == XbrlElementValueTypes.Monetary) {
                if (document != null) {
                    var value = document.ValueTreeMain.GetValue(root.Element.Id);
                    if (value != null) {
                        template.ElementInfos.Add(new MappingTemplateElementInfo {
                            Template = template,
                            ElementId = root.Element.Id,
                            AutoComputeEnabled = value.AutoComputeEnabled,
                            SupressWarningMessages = value.SupressWarningMessages,
                            SendAccountBalances = value.SendAccountBalances
                        });
                    }

                } else {
                    template.ElementInfos.Add(new MappingTemplateElementInfo {
                        Template = template,
                        ElementId = root.Element.Id,
                    });
                }
            }

            foreach (var child in root.Children.OfType<Taxonomy.Interfaces.PresentationTree.IPresentationTreeNode>()) {
                CreateTemplate(template, document, child);
            }
        }
        #endregion CreateTemplate

        #region InitTemplate
        public static void InitTemplate(CreateTemplateModel model) {

            var document = model.Document;
            var template = model.Template;

            foreach (var balanceList in model.SelectedBalanceLists) {
                foreach (var item in balanceList.AssignedItems) {
                    var assignment = template.GetAssignment(item.Number);
                    if (assignment == null) {
                        template.Assignments.Add(new MappingTemplateLine {
                            Template = template,
                            AccountNumber = item.Number,
                            AccountName = item.Name,
                            DebitElementId = (item.Amount >= 0 ? item.AssignedElement.Id : null),
                            CreditElementId = (item.Amount < 0 ? item.AssignedElement.Id : null),
                        });
                    } else {
                        // TODO1.5 TemplateManager.InitTemplate: handle conflict with user request
                        // options: replace assignment | keep assignment | allways keep assignment
                        if (item.Amount >= 0) assignment.DebitElementId = item.AssignedElement.Id;
                        else assignment.CreditElementId = item.AssignedElement.Id;

                        if (assignment.DebitElementId != null && assignment.CreditElementId != null) {
                            // TODO?
                        }
                    }
                }
            }

            InitElementInfos(template, document);

        }
        #endregion

        #region ExtendTemplates
        public static void ExtendTemplates(ExtendTemplateModel model) {

            model.AssignmentErrors.Clear();
            model.AssignmentConflicts.Clear();

            // merge selected template with actual assignments
            foreach (var balanceList in model.BalanceLists.Where(balanceList => balanceList.IsChecked)) {
                var assignmentErrorListEntry = new AssignmentErrorListEntry(balanceList.BalanceList);
                model.AssignmentErrors.Add(assignmentErrorListEntry);

                var assignmentConflictListEntry = new AssignmentConflictListEntry(balanceList.BalanceList);
                model.AssignmentConflicts.Add(assignmentConflictListEntry);

                foreach (
                    var item in
                        balanceList.BalanceList.AssignedItems.Where(
                            item => CheckIfMergeIsAllowed(model, item, assignmentErrorListEntry))) {

                    if (!model.Template.HasAssignment(item.Number)) {

                        // insert new assignment
                        var line = new MappingTemplateLine {
                            Template = model.Template,
                            AccountNumber = item.Number,
                            AccountName = item.Name
                        };

                        var mappingHeaderGui = new MappingHeaderGui(line);
                        model.Assignments.Add(mappingHeaderGui);
                        var node = FindPresentationTreeNode(model, item);
                        node.AddChildren(mappingHeaderGui);

                        if (item.Amount >= 0) line.DebitElementId = item.AssignedElement.Id;
                        else line.CreditElementId = item.AssignedElement.Id;

                        model.Template.Assignments.Add(line);

                    } else {
                        // update existing assignment
                        var line = model.Template.GetAssignment(item.Number);
                        var node = FindPresentationTreeNode(model, item);

                        if (line.IsAccountOfExchange) {
                            // exchange account
                            if (item.Amount >= 0) {
                                if (line.DebitElementId != item.AssignedElement.Id) {
                                    if (line.DebitElementId != null)
                                        assignmentConflictListEntry.Conflicts.Add(
                                            new AssignmentConflict(node.Element, line, item.AssignedElement.Id,
                                                                   TemplateAssignmentPosition.Debit,
                                                                   TemplateAssignmentConfictResolveMode.Replace));
                                    else
                                        // add new assignment
                                        line.DebitElementId = item.AssignedElement.Id;
                                }
                            } else {
                                if (line.CreditElementId != item.AssignedElement.Id) {
                                    if (line.CreditElementId != null)
                                        assignmentConflictListEntry.Conflicts.Add(
                                            new AssignmentConflict(node.Element, line, item.AssignedElement.Id,
                                                                   TemplateAssignmentPosition.Credit,
                                                                   TemplateAssignmentConfictResolveMode.Replace));
                                    else
                                        // add new assignment
                                        line.CreditElementId = item.AssignedElement.Id;
                                }
                            }

                        } else {
                            // regular account
                            if (line.ElementId == null) {
                                // add new assignment
                                if (item.Amount >= 0) line.CreditElementId = item.AssignedElement.Id;
                                else line.DebitElementId = item.AssignedElement.Id;

                            } else if (line.CreditElementId == line.DebitElementId) {
                                if (line.ElementId != item.AssignedElement.Id)
                                    if (item.Amount >= 0) {
                                        assignmentConflictListEntry.Conflicts.Add(
                                            new AssignmentConflict(node.Element, line, item.AssignedElement.Id,
                                                                   TemplateAssignmentPosition.Debit,
                                                                   TemplateAssignmentConfictResolveMode.
                                                                       ConvertToAccountOfExchange));
                                    } else {
                                        assignmentConflictListEntry.Conflicts.Add(
                                            new AssignmentConflict(node.Element, line, item.AssignedElement.Id,
                                                                   TemplateAssignmentPosition.Credit,
                                                                   TemplateAssignmentConfictResolveMode.
                                                                       ConvertToAccountOfExchange));
                                    }
                            } else {
                                if (item.Amount >= 0) {
                                    if (line.DebitElementId != null) {
                                        if (line.ElementId != item.AssignedElement.Id) {
                                            assignmentConflictListEntry.Conflicts.Add(
                                                new AssignmentConflict(node.Element, line, item.AssignedElement.Id,
                                                                       TemplateAssignmentPosition.Debit,
                                                                       TemplateAssignmentConfictResolveMode.Replace));
                                        }
                                    } else {
                                        assignmentConflictListEntry.Conflicts.Add(
                                            new AssignmentConflict(node.Element, line, item.AssignedElement.Id,
                                                                   TemplateAssignmentPosition.Debit,
                                                                   TemplateAssignmentConfictResolveMode.
                                                                       ConvertToAccountOfExchange));
                                    }
                                } else {
                                    if (line.CreditElementId != null) {
                                        if (line.ElementId != item.AssignedElement.Id) {
                                            assignmentConflictListEntry.Conflicts.Add(
                                                new AssignmentConflict(node.Element, line, item.AssignedElement.Id,
                                                                       TemplateAssignmentPosition.Credit,
                                                                       TemplateAssignmentConfictResolveMode.Replace));
                                        }
                                    } else {
                                            assignmentConflictListEntry.Conflicts.Add(
                                                new AssignmentConflict(node.Element, line, item.AssignedElement.Id,
                                                                       TemplateAssignmentPosition.Credit,
                                                                       TemplateAssignmentConfictResolveMode.
                                                                           ConvertToAccountOfExchange));                                        
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        
        private static bool CheckIfMergeIsAllowed(ExtendTemplateModel model, IBalanceListEntry item, AssignmentErrorListEntry assignmentErrorListEntry) {
            var node = FindPresentationTreeNode(model, item);

            if (node == null)
                throw new Exception(ResourcesCommon.CouldNotFindNodePresentationTree);

            foreach (IPresentationTreeNode n in node.Element.PresentationTreeNodes) {
                var value = n.Value;
                if (value.IsEnabled && !value.HasComputedValue) continue;
                var msg = !value.IsEnabled
                              ? ResourcesCommon.ColudNotAddTaxonomyNotActive
                              : ResourcesCommon.ColudNotAddTaxonomyCalculated;

                assignmentErrorListEntry.Errors.Add(new AssignmentError(value.Element, item.Label, msg));
                return false;
            }
            
            return true;
        }

        private static IPresentationTreeNode FindPresentationTreeNode(ExtendTemplateModel model, IBalanceListEntry item) {
            IPresentationTreeNode node = null;
            foreach (var ptree in model.PresentationTrees) {
                node = (IPresentationTreeNode) ptree.GetNode(item.AssignedElement.Id);
                if (node != null) break;
            }
            return node;
        }
        #endregion ExtendTemplates

        #region ApplyTemplate
        public static void ApplyTemplate(object paramArray) {
            var pi = (ProgressInfo) (((object[]) paramArray)[0]);
            var model = (ApplyTemplateModel) (((object[]) paramArray)[1]);

            model.AssignmentErrors.Clear();
            
            pi.Maximum = model.Template.Assignments.Count;
            pi.Value = 0;

            try {
                LogManager.Instance.Log = false;

                pi.Caption = ResourcesCommon.ChangeTemplate1;
                foreach (var balanceList in model.SelectedBalanceLists)
                    model.Document.ClearAssignments(pi, balanceList);

                pi.Caption = ResourcesCommon.ChangeTemplate2;
                var newAssignments = new Dictionary<IBalanceList, List<Tuple<IBalanceListEntry, string>>>();
                foreach (var balanceList in model.SelectedBalanceLists) {
                    var assignmentErrorListEntry = new AssignmentErrorListEntry(balanceList);
                    newAssignments[balanceList] =
                        ApplyTemplate(model.Template, model.Document, pi, balanceList, assignmentErrorListEntry);

                    if (assignmentErrorListEntry.Errors.Count > 0) model.AssignmentErrors.Add(assignmentErrorListEntry);
                }

                pi.Caption = ResourcesCommon.ChangeTemplate3;
                foreach (var balanceList in model.SelectedBalanceLists)
                    model.Document.AddAccountAssignments(newAssignments[balanceList], pi, balanceList);

                LogManager.Instance.Log = true;

                foreach (var balanceList in model.SelectedBalanceLists)
                    LogManager.Instance.UseTemplate(model.Template, model.Document, newAssignments[balanceList]);

                // update flags from template
                if (model.ReplaceAutoComputeEnabledFlag || model.ReplaceSendAccountBalanceFlag ||
                    model.ReplaceIgnoreWarningMessageFlag) {

                    int selectedOptions = 0;
                    if (model.ReplaceAutoComputeEnabledFlag) selectedOptions++;
                    if (model.ReplaceSendAccountBalanceFlag) selectedOptions++;
                    if (model.ReplaceIgnoreWarningMessageFlag) selectedOptions++;
                    string caption = ResourcesCommon.ChangeTemplate4 + " " +
                                     (selectedOptions == 1 ? ResourcesCommon.Option + " " : ResourcesCommon.Options + " ");

                    int usedOptions = 0;
                    if (model.ReplaceAutoComputeEnabledFlag) {
                        usedOptions++;
                        caption += "'" + ResourcesCommon.PositionSumUp + "'";
                        if (selectedOptions - usedOptions == 1) caption += " " + ResourcesCommon.And + " ";
                        else if (selectedOptions > 1) caption += ", ";
                    }
                    if (model.ReplaceSendAccountBalanceFlag) {
                        usedOptions++;
                        caption += "'" + ResourcesCommon.SendAccountBalance + "'";
                        if (selectedOptions - usedOptions == 1) caption += " " + ResourcesCommon.And + " ";
                        else if (selectedOptions > 1) caption += ", ";
                    }
                    if (model.ReplaceIgnoreWarningMessageFlag) {
                        caption += "'" + ResourcesCommon.IgnoreWarning + "'";
                    }

                    pi.Caption = caption;

                    Action<ValueTreeNode> setFlags = null;
                    setFlags = root => {
                        foreach (var value in root.Values.Values) {

                            var elementInfo = model.Template.GetElementInfo(value.Element.Name);

                            if (model.ReplaceAutoComputeEnabledFlag) {
                                if (value.AutoComputeAllowed) value.AutoComputeEnabled = elementInfo != null && elementInfo.AutoComputeEnabled;
                                //else ; 
                                // TODO: show error in GUI
                            }

                            if (model.ReplaceSendAccountBalanceFlag)
                                value.SendAccountBalances = elementInfo != null && elementInfo.SendAccountBalances;

                            if (model.ReplaceIgnoreWarningMessageFlag)
                                value.SupressWarningMessages = elementInfo != null && elementInfo.SupressWarningMessages;

                            if (value is XbrlElementValue_Tuple && !value.Element.IsList) {
                                var tupleValue = value as XbrlElementValue_Tuple;
                                setFlags(tupleValue.Items.First());
                            }
                        }
                    };

                    XbrlElementValueBase.DoDbUpdate = false;
                    setFlags(model.Document.ValueTreeMain.Root);
                    XbrlElementValueBase.DoDbUpdate = true;

                    model.Document.ValueTreeMain.Save();
                }

            } finally {
                LogManager.Instance.Log = true;
            }
        }

        public static List<Tuple<IBalanceListEntry, string>> ApplyTemplate(
            MappingTemplateHead template,
            Document document,
            ProgressInfo progressInfo,
            IBalanceList balanceList,
            AssignmentErrorListEntry assignmentErrorListEntry) {

            var newAssignments = new List<Tuple<IBalanceListEntry, string>>();
            
            var entryDict = new Dictionary<string, IBalanceListEntry>();
            foreach (var item in balanceList.UnassignedItems) entryDict[item.Number] = item;

            // set new assignments            
            foreach (var assignment in template.Assignments) {
                // account not contained in balance list
                if (!entryDict.ContainsKey(assignment.AccountNumber)) continue;

                var item = entryDict[assignment.AccountNumber];

                Tuple<IBalanceListEntry, string> newAssignment = null;

                // get new assignment
                if (assignment.DebitElementId != null || assignment.CreditElementId != null) {
                    if (assignment.IsAccountOfExchange) {
                        // exchange account (Wechselkonto)
                        if (item.Amount >= 0) {
                            if (assignment.DebitElementId != null) {
                                newAssignment = new Tuple<IBalanceListEntry, string>(item, assignment.DebitElementId);
                                newAssignments.Add(newAssignment);
                            }

                        } else {
                            // account.Amount < 0
                            if (assignment.CreditElementId != null) {
                                newAssignment = new Tuple<IBalanceListEntry, string>(item, assignment.CreditElementId);
                                newAssignments.Add(newAssignment);
                            }
                        }

                    } else {
                        // regular account
                        newAssignment = new Tuple<IBalanceListEntry, string>(item, assignment.ElementId);
                        newAssignments.Add(newAssignment);
                    }
                }

                if (newAssignment != null) {

                    // search node
                    var node = GetPresentationTreeNode(document, newAssignment.Item2);
                    if (node != null) {
                        // check if assignment is valied
                        bool isValid = true;
                        foreach (IPresentationTreeNode n in node.Element.PresentationTreeNodes) {
                            var value = n.Value;
                            if (value.IsEnabled && !value.HasComputedValue) continue;
                            var msg = !value.IsEnabled
                                          ? ResourcesCommon.ColudNotAssignTaxonomyNotActive
                                          : ResourcesCommon.ColudNotAssignTaxonomyCalculated;

                            isValid = false;
                            assignmentErrorListEntry.Errors.Add(
                                new AssignmentError(value.Element, assignment.AccountLabel, msg));
                            break;
                        }

                        // add assignment
                        if (isValid) {
                            foreach (IPresentationTreeNode n in node.Element.PresentationTreeNodes) {
                                n.AddChildren(newAssignment.Item1);
                                ExpandNodeAndarents(n);
                            }
                        }
                    }
                }

                progressInfo.Value++;
            }

            return newAssignments;
        }

        private static IPresentationTreeNode GetPresentationTreeNode(Document document, string id) {
            return document.GaapPresentationTrees.Values.Select(
                ptree => ptree.GetNode(id)).OfType<IPresentationTreeNode>().FirstOrDefault();
        }

        private static void ExpandNodeAndarents(IPresentationTreeNode node) {
            while (node != null) {
                node.IsExpanded = true;
                if (!node.IsRoot) node = node.Parents.First() as IPresentationTreeNode;
                else node = null;
            }
        }
        #endregion ApplyTemplate

        #region ResetTemplate
        public static void ResetTemplate(MappingTemplateHead template) {
            var templateToDelete = Templates.Where(t => t.Id == template.Id).FirstOrDefault();
            Debug.Assert(templateToDelete != null);
            Templates.Remove(templateToDelete);

            using (var conn = AppConfig.ConnectionManager.GetConnection()) {
                var tmp = new List<MappingTemplateHead>(Templates);
                var newTemplate = conn.DbMapping.Load<MappingTemplateHead>(template.Id);
                AddMissingTemplateElements(newTemplate);

                tmp.Add(newTemplate);
                tmp.Sort();

                Templates.Clear();
                foreach (var t in tmp) Templates.Add(t);
            }
        }
        #endregion ResetTemplate

        #endregion methods

    }
}