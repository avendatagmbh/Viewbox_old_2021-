// --------------------------------------------------------------------------------
// author: Márton Garai
// since: 2012-08-02
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using Utils;
using eBalanceKitBusiness.Interfaces;
using eBalanceKitBusiness.Structures.DbMapping.BalanceList;

namespace eBalanceKitBusiness.MappingTemplate
{
    public class SplitAccountGroupInfo : NotifyPropertyChangedBase, IAccountOrSplitAccountGroupInfo
    {

        #region properties
        public ValueInputMode AccountValueInputMode { get; set; }
        public string AccountName { get; set; }
        public string AccountNumber { get; set; }
        public string GroupComment { get; set; }
        public List<SplittedChildInfo> ChildrenInfos { get; set; }

        // used in TaxonomyAndBalanceListBase.xaml
        public string AccountDisplayString { get { return (AccountNumber != null ? AccountNumber + " - " : "") + AccountName; } }

        public bool HasComment { get { return !string.IsNullOrEmpty(GroupComment); } }

        #region IsSelected
        private bool _isSelected;

        public bool IsSelected {
            get { return _isSelected; }
            set {
                if (_isSelected != value) {
                    _isSelected = value;
                    OnPropertyChanged("IsSelected");
                }
            }
        }
        #endregion IsSelected
        #endregion

        #region constructor
        public SplitAccountGroupInfo() {
            ChildrenInfos = new List<SplittedChildInfo>();
            IsSelected = false;
        }

        /// <summary>
        /// Constructor for load the details when coming from database
        /// </summary>
        /// <param name="parentRaw">The xml string that stores the values of the split account</param>
        /// <param name="childrenRaw">The xml string that stores the values of the accounts for the split account</param>
        public SplitAccountGroupInfo(string parentRaw, string childrenRaw) {
            if (!string.IsNullOrEmpty(parentRaw)) {
                using (XmlReader reader = new XmlTextReader(new StringReader(parentRaw))) {
                    reader.Read();
                    AccountName = reader.GetAttribute("Name");
                    AccountNumber = reader.GetAttribute("Number");
                    GroupComment = reader.GetAttribute("Comment");
                    string attribute = reader.GetAttribute("ValueInputMode");
                    Debug.Assert(attribute != null);
                    string valueInputMode = attribute.ToLower();
                    if (valueInputMode == "absolute") {
                        AccountValueInputMode = ValueInputMode.Absolute;
                    }
                    if (valueInputMode == "relative") {
                        AccountValueInputMode = ValueInputMode.Relative;
                    }
                }
            }

            if (!string.IsNullOrEmpty(childrenRaw)) {
                ChildrenInfos = new List<SplittedChildInfo>();
                using (XmlReader reader = new XmlTextReader(new StringReader(childrenRaw))) {
                    while (reader.Read()) {
                        if (reader.Name == "child" && reader.NodeType == XmlNodeType.Element) {
                            string name = reader.GetAttribute("Name");
                            string number = reader.GetAttribute("Number");
                            string percent = reader.GetAttribute("Percent");
                            string comment = reader.GetAttribute("Comment");
                            Debug.Assert(name != null);
                            Debug.Assert(number != null);
                            Debug.Assert(percent != null);
                            Debug.Assert(comment != null);
                            SplittedChildInfo temp = new SplittedChildInfo {
                                Name = name,
                                Number = number,
                                Comment = comment,
                                ValueInputMode = AccountValueInputMode
                            };
                            if (percent != "null")
                                temp.AmountPercent = decimal.Parse(percent, CultureInfo.InvariantCulture);
                            ChildrenInfos.Add(temp);
                        }
                    }
                }
                
            } else {
                ChildrenInfos = new List<SplittedChildInfo>();
            }

            IsSelected = false;
        }
        #endregion

        #region methods
        /// <summary>
        /// Converts the class into an xml string for database storage.
        /// </summary>
        public string GetChildrenRaw() {
            XElement root = new XElement("root", null);
            foreach (SplittedChildInfo splittedChildInfo in ChildrenInfos) {
                root.Add(new XElement("child",
                         new XAttribute("Name", splittedChildInfo.Name),
                         new XAttribute("Number", splittedChildInfo.Number),
                         new XAttribute("Comment", splittedChildInfo.Comment),
                         new XAttribute("Percent", splittedChildInfo.AmountPercent.HasValue ?
                             splittedChildInfo.AmountPercent.Value.ToString(CultureInfo.InvariantCulture) :
                             "null")));
            }
            return root.ToString(SaveOptions.DisableFormatting);
        }

        /// <summary>
        /// Converts the class into an xml string for database storage.
        /// </summary>
        public string GetParentRaw() {
            string accountInputModeString = null;
            // have to be updated if there is a new type of input mode all across the mapping template generation/ import/ export!
            if (AccountValueInputMode == ValueInputMode.Absolute) {
                accountInputModeString = "absolute";
            }
            if (AccountValueInputMode == ValueInputMode.Relative) {
                accountInputModeString = "relative";
            }
            Debug.Assert(accountInputModeString != null);
            XElement parent = new XElement("parent", new XAttribute("Name", AccountName),
                                           new XAttribute("Number", AccountNumber),
                                           new XAttribute("Comment", GroupComment),
                                           new XAttribute("ValueInputMode", accountInputModeString));
            return parent.ToString(SaveOptions.DisableFormatting);
        }
        #endregion
    }
}
