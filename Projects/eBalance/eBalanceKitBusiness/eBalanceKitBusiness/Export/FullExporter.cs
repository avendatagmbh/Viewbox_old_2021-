using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Taxonomy.Enums;
using Utils;
using eBalanceKitBusiness.Interfaces.BalanceList;
using eBalanceKitBusiness.Options;
using eBalanceKitBusiness.Structures.DbMapping;
using eBalanceKitBusiness.Structures.DbMapping.BalanceList;
using eBalanceKitBusiness.Structures.ValueTree;
using eBalanceKitBusiness.Structures.XbrlElementValue;
using log4net;
using AV.Log;

namespace eBalanceKitBusiness.Export {
    public class FullExporter
    {

        internal ILog _log = LogHelper.GetLogger();

        #region ExportFull
        /// <summary>
        /// Exports the full EBK data into the specified file.
        /// </summary>
        public void FullExport(string file, Document document) {

            _log.Log(LogLevelEnum.Info, "This is the entry point of FullExport()", true);
            
            try {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(file);

                //Inserting balance list info, account group info, splitted accounts info, and attaching them to each account

                XmlNode xnListRoot = xmlDoc.SelectSingleNode("//*[name()='xbrli:xbrl']");
                XmlNodeList xnList = xmlDoc.SelectNodes("//*[name()='de-gaap-ci:detailedInformation.accountBalances']");

                int balanceListId = 1;
                int accounGroupId = 1;
                int splitAccounGroupId = 1;

                XmlElement xmlNodeBalanceListDesc = xmlDoc.CreateElement("detailedInformation.accountbalances.balanceListDescription");
                XmlElement xmlNodeAccountGroupDesc = xmlDoc.CreateElement("detailedInformation.accountbalances.accountGroupDescription");
                XmlElement xmlNodeSplitAccountGroupDesc = xmlDoc.CreateElement("detailedInformation.accountbalances.splittedAccountGroupDescription");

                foreach (IBalanceList balancelist in document.BalanceLists) {
                    XmlElement xmlNodeBalanceListItem = xmlDoc.CreateElement("detailedInformation.accountbalances.balanceListItem");
                    xmlNodeBalanceListItem.SetAttribute("id", balanceListId.ToString(CultureInfo.InvariantCulture));
                    xmlNodeBalanceListItem.SetAttribute("isImported", balancelist.IsImported.ToString(CultureInfo.InvariantCulture));
                    //if virtual list
                    if (!balancelist.IsImported) {
                        xmlNodeBalanceListItem.SetAttribute("source", balancelist.Source);
                    }
                    if (balancelist.Comment != null) {
                        xmlNodeBalanceListItem.SetAttribute("comment", balancelist.Comment);
                    }
                    xmlNodeBalanceListItem.InnerText = balancelist.Name;
                    xmlNodeBalanceListDesc.AppendChild(xmlNodeBalanceListItem);
                    foreach (IAccount account in balancelist.Accounts) {
                        if (xnList == null) continue;
                        foreach (XmlNode xn in xnList) {
                            XmlNode selectSingleNode = null;
                            foreach (XmlNode child in xn.ChildNodes.Cast<XmlNode>().Where(child => child.Name.Split('.').Last() == "accountNumber")) {
                                selectSingleNode = child;
                            }
                            if (selectSingleNode == null) continue;
                            string accountNumber = selectSingleNode.InnerText;
                            if (account.Number == accountNumber) {
                                XmlElement xmlNodeBalanceList = xmlDoc.CreateElement("detailedInformation.accountbalances.balanceList");
                                if (!balancelist.IsImported) {
                                    xmlNodeBalanceList.SetAttribute("isVirtual", "True");
                                    //account = (VirtualAccount) account;
                                    VirtualAccount virtualAccount = account as VirtualAccount;
                                    if (virtualAccount != null) {
                                        xmlNodeBalanceList.SetAttribute("taxonomyPosition", virtualAccount.TaxonomyPosition);
                                    }
                                }
                                xmlNodeBalanceList.InnerText = balanceListId.ToString(CultureInfo.InvariantCulture);
                                xn.AppendChild(xmlNodeBalanceList);
                            }
                        }
                    }

                    foreach (IAccountGroup group in balancelist.AccountGroups) {
                        XmlElement xmlNodeAccountGroupItem = xmlDoc.CreateElement("detailedInformation.accountbalances.accountGroupItem");
                        xmlNodeAccountGroupItem.SetAttribute("id", accounGroupId.ToString(CultureInfo.InvariantCulture));
                        xmlNodeAccountGroupItem.SetAttribute("balanceListId", balanceListId.ToString(CultureInfo.InvariantCulture));
                        xmlNodeAccountGroupItem.SetAttribute("groupNumber", group.Number);
                        xmlNodeAccountGroupItem.SetAttribute("groupComment", group.Comment);
                        if (group.AssignedElement != null) {
                            xmlNodeAccountGroupItem.SetAttribute("assignedElement", group.AssignedElement.Id);
                        }
                        xmlNodeAccountGroupItem.InnerText = group.Name;
                        xmlNodeAccountGroupDesc.AppendChild(xmlNodeAccountGroupItem);
                        if (xnList != null) {
                            foreach (XmlNode xn in xnList) {
                                XmlNode selectSingleNode = null;
                                foreach (XmlNode child in xn.ChildNodes.Cast<XmlNode>().Where(child => child.Name.Split('.').Last() == "accountNumber")) {
                                    selectSingleNode = child;
                                }
                                if (selectSingleNode == null) continue;
                                string accountNumber = selectSingleNode.InnerText;
                                foreach (XmlElement xmlNodeBalanceList in from Account account in @group.Items where account.Number == accountNumber select xmlDoc.CreateElement("detailedInformation.accountbalances.balanceList")) {

                                    xmlNodeBalanceList.InnerText = balanceListId.ToString(CultureInfo.InvariantCulture);
                                    xn.AppendChild(xmlNodeBalanceList);
                                }
                                foreach (XmlElement xmlNodeAccountGroup in from Account account in @group.Items where account.Number == accountNumber select xmlDoc.CreateElement("detailedInformation.accountbalances.accountGroup")) {

                                    xmlNodeAccountGroup.InnerText = accounGroupId.ToString(CultureInfo.InvariantCulture);
                                    xn.AppendChild(xmlNodeAccountGroup);
                                }
                            }
                        }
                        accounGroupId++;
                    }

                    foreach (SplitAccountGroup splitAccountGroup in balancelist.SplitAccountGroups) {
                        XmlElement xmlNodeSplitAccountGroupItem = xmlDoc.CreateElement("detailedInformation.accountbalances.accountGroupItem");
                        xmlNodeSplitAccountGroupItem.SetAttribute("id", splitAccounGroupId.ToString(CultureInfo.InvariantCulture));
                        xmlNodeSplitAccountGroupItem.SetAttribute("balanceListId", balanceListId.ToString(CultureInfo.InvariantCulture));
                        xmlNodeSplitAccountGroupItem.SetAttribute("originalAccountNumber", splitAccountGroup.Account.Number);
                        xmlNodeSplitAccountGroupItem.SetAttribute("originalAccountAmount", splitAccountGroup.Account.Amount.ToString(CultureInfo.InvariantCulture));
                        xmlNodeSplitAccountGroupItem.InnerText = splitAccountGroup.Account.Name;
                        xmlNodeSplitAccountGroupDesc.AppendChild(xmlNodeSplitAccountGroupItem);
                        if (xnList != null) {
                            foreach (XmlNode xn in xnList) {
                                XmlNode selectSingleNode = null;
                                foreach (XmlNode child in xn.ChildNodes.Cast<XmlNode>().Where(child => child.Name.Split('.').Last() == "accountNumber")) {
                                    selectSingleNode = child;
                                }
                                if (selectSingleNode == null) continue;
                                string accountNumber = selectSingleNode.InnerText;
                                foreach (XmlElement xmlNodeBalanceList in from splitAccountGroupItem in splitAccountGroup.Items where accountNumber == splitAccountGroupItem.Number select xmlDoc.CreateElement("detailedInformation.accountbalances.balanceList")) {
                                    xmlNodeBalanceList.InnerText = balanceListId.ToString(CultureInfo.InvariantCulture);
                                    xn.AppendChild(xmlNodeBalanceList);
                                }
                                foreach (XmlElement xmlNodeSplitAccountGroup in from splitAccountGroupItem in splitAccountGroup.Items where accountNumber == splitAccountGroupItem.Number select xmlDoc.CreateElement("detailedInformation.accountbalances.splitAccountGroup")) {
                                    foreach (SplittedAccount item in splitAccountGroup.Items.Cast<SplittedAccount>().Where(item => item.Number == accountNumber && item.Comment != null)) {
                                        xmlNodeSplitAccountGroup.SetAttribute("splittedComment", item.Comment);
                                        break;
                                    }
                                    xmlNodeSplitAccountGroup.InnerText = splitAccounGroupId.ToString(CultureInfo.InvariantCulture);
                                    xn.AppendChild(xmlNodeSplitAccountGroup);
                                }
                            }
                        }
                        splitAccounGroupId++;
                    }

                    balanceListId++;
                }


                //Inserting taxonomy position comments and other values

                XmlElement xmlNodePositionComments = xmlDoc.CreateElement("commentsTaxonomyPositions");

                foreach (IValueTreeEntry entry in document.ValueTreeMain.Root.Values.Values)  {
                    
                    switch (entry.Element.ValueType) {
                        case XbrlElementValueTypes.Tuple:
                            XbrlElementValue_Tuple tupleValue = entry as XbrlElementValue_Tuple;
                            if (tupleValue != null) {
                                foreach (var item in tupleValue.Items) {
                                    foreach (var val in item.Values) {
                                        XbrlElementValueBase baseValue = val.Value as XbrlElementValueBase;
                                        if (baseValue != null && baseValue.Comment != null && xmlNodePositionComments[val.Value.Element.Id + ".comment"] == null) {
                                            XmlElement xmlNodePositionCommentItem = xmlDoc.CreateElement(val.Value.Element.Id + ".comment");
                                            xmlNodePositionCommentItem.InnerText = baseValue.Comment;
                                            xmlNodePositionComments.AppendChild(xmlNodePositionCommentItem);
                                        }
                                    }
                                }
                            }
                            break;
                    }

                    //if (xn.LocalName == entry.Element.Id.Split(new char[] { '_' }, 2).Last() && entry.DbValue != null && entry.DbValue.Comment != null) {
                    if (entry.DbValue != null && entry.DbValue.Comment != null)
                    {
                        bool hasAddedComment = xmlNodePositionComments.Cast<XmlNode>().Any(node => node.Name == entry.Element.Id + ".comment");
                        if (hasAddedComment)
                        {
                            continue;
                        }
                        //XmlElement xmlNodePositionCommentItem = xmlDoc.CreateElement(entry.Element.TargetNamespace.Name, xn.LocalName, entry.Element.TargetNamespace.Namespace);
                        XmlElement xmlNodePositionCommentItem = xmlDoc.CreateElement(entry.Element.Id + ".comment");
                        xmlNodePositionCommentItem.InnerText = entry.DbValue.Comment;
                        xmlNodePositionComments.AppendChild(xmlNodePositionCommentItem);
                    }
                }

                XmlElement xmlNodeOtherValues = xmlDoc.CreateElement("otherValues");

                foreach (IValueTreeEntry entry in document.ValueTreeGcd.Root.Values.Values) {
                    XbrlElementValueBase baseValue = entry as XbrlElementValueBase;
                    if (baseValue != null && baseValue.ValueOther != null) {
                        XmlElement xmlNodeValueOther = xmlDoc.CreateElement(entry.Element.Id + ".other");
                        xmlNodeValueOther.InnerText = baseValue.ValueOther;
                        xmlNodeOtherValues.AppendChild(xmlNodeValueOther);
                    }
                }

                //Inserting taxonomy position send account balances property
                /*
                XmlElement xmlNodeSendAccBalances = xmlDoc.CreateElement("sendAccountBalances");

                foreach (IValueTreeEntry entry in document.ValueTreeMain.Root.Values.Values)  {
                    //InsertSendAccountBalances(entry, document, xmlDoc, xmlNodeSendAccBalances);
                }
                */


                //Inserting system info
                XmlElement xmlNodeSystem = xmlDoc.CreateElement("system");
                xmlNodeSystem.SetAttribute("comment", document.System.Comment);
                xmlNodeSystem.InnerText = document.System.Name;

                //Inserting report info
                XmlElement xmlNodeReport = xmlDoc.CreateElement("report");
                xmlNodeReport.SetAttribute("name", document.Name);
                if (document.Comment != null) {
                    xmlNodeReport.SetAttribute("comment", document.Comment);
                }

                XmlElement xmlNodeSettings = xmlDoc.CreateElement("Settings");

                XmlElement setting1 = xmlDoc.CreateElement("ShowSelectedLegalForm");
                setting1.InnerText = document.Settings.ShowSelectedLegalForm.ToString(CultureInfo.InvariantCulture);
                xmlNodeSettings.AppendChild(setting1);

                XmlElement setting2 = xmlDoc.CreateElement("ShowSelectedTaxonomy");
                setting2.InnerText = document.Settings.ShowSelectedTaxonomy.ToString(CultureInfo.InvariantCulture);
                xmlNodeSettings.AppendChild(setting2);

                XmlElement setting3 = xmlDoc.CreateElement("ShowTypeOperatingResult");
                setting3.InnerText = document.Settings.ShowTypeOperatingResult.ToString(CultureInfo.InvariantCulture);
                xmlNodeSettings.AppendChild(setting3);

                XmlElement setting4 = xmlDoc.CreateElement("ShowOnlyMandatoryPostions");
                setting4.InnerText = document.Settings.ShowOnlyMandatoryPostions.ToString(CultureInfo.InvariantCulture);
                xmlNodeSettings.AppendChild(setting4);


                if (xnListRoot != null) {
                    xnListRoot.AppendChild(xmlNodeBalanceListDesc);
                    xnListRoot.AppendChild(xmlNodeAccountGroupDesc);
                    xnListRoot.AppendChild(xmlNodeSplitAccountGroupDesc);
                    xnListRoot.AppendChild(xmlNodePositionComments);
                    xnListRoot.AppendChild(xmlNodeOtherValues);
                    xnListRoot.AppendChild(xmlNodeSystem);
                    xnListRoot.AppendChild(xmlNodeReport);
                    xnListRoot.AppendChild(xmlNodeSettings);
                }

                xmlDoc.Save(file);
            }
            catch(Exception ex) {
                _log.ErrorWithCheck("Exception occurred while doing FullExport()", ex);
                throw;
            }

        }
        #endregion ExportXbrl

        /*
        private void InsertSendAccountBalances(IValueTreeEntry entry, Document document, XmlDocument xmlDoc, XmlElement xmlNodeSendAccBalances) {

            XmlElement xmlNodeAccountBalanceItem = xmlDoc.CreateElement(entry.Element.Id);

            switch (entry.Element.ValueType) {
                case XbrlElementValueTypes.Tuple:
                    XbrlElementValue_Tuple tupleValue = entry as XbrlElementValue_Tuple;               
                    xmlNodeAccountBalanceItem.InnerText = entry.SendAccountBalances.ToString(CultureInfo.InvariantCulture);
                    xmlNodeSendAccBalances.AppendChild(xmlNodeAccountBalanceItem);
                    if (tupleValue != null) {
                        foreach (ValueTreeNode item in tupleValue.Items) {
                            foreach (var itemEntry in item.Values) {
                                InsertSendAccountBalances(itemEntry.Value, document, xmlDoc, xmlNodeSendAccBalances);
                            }
                        }
                    }
                    break;
                default:
                    //XbrlElementValueBase baseValue = entry as XbrlElementValueBase;
                    xmlNodeAccountBalanceItem.InnerText = entry.SendAccountBalances.ToString(CultureInfo.InvariantCulture);
                    xmlNodeSendAccBalances.AppendChild(xmlNodeAccountBalanceItem);
                    break;
            }      
        }
         */

        public ObservableCollectionAsync<LogEntry> LogEntries {
            get { return LogHelper.LogEntries; }
        }
    }
}
