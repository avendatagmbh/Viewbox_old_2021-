// --------------------------------------------------------------------------------
// author: Márton Garai
// since: 2012-08-07
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Xml;
using System.Xml.Linq;
using Utils;
using Utils.Commands;
using eBalanceKitBusiness.Interfaces;
using eBalanceKitResources.Localisation;

namespace eBalanceKitBusiness.MappingTemplate
{
    /// <summary>
    /// class for store visibility informations for an account group at the mapping template applying.
    /// </summary>
    public class AccountGroupInfo : NotifyPropertyChangedBase, IAccountGroupOrAccountGroupInfo
    {
        #region properties
        public ObservableCollection<AccountGroupChildInfo> ChildrenInfos { get; set; }
        /// <summary>
        /// these accounts will be shown in a tool tip if they are missing or already assigned. Not stored. Used only in the AccountGroupingModel.cs
        /// </summary>
        public ObservableCollection<AccountGroupChildInfo> MissingChildrenInfos { get; set; }

        /// <summary>
        /// At the end of applying a mapping template for positions
        ///  we save the reference of the actual balance list, to append the changes, if we click on the "apply template" button.
        /// </summary>
        public IBalanceList MaybeBalanceList { get; set; }

        /// <summary>
        /// for knowing which list should remove from
        /// </summary>
        private ObservableCollection<AccountGroupInfo> _containingList;

        public ObservableCollection<AccountGroupInfo> ContainingList { get { return _containingList; } 
            set {
                DeleteAccountGroupCommand = new DelegateCommand(o => true, o => {
                    string msg = string.Format(ResourcesBalanceList.DeleteAccountGroupSure, AccountDisplayString);
                    if (MessageBox.Show(msg, ResourcesBalanceList.DeleteAccountGroupTitle , MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.Yes) {
                        value.Remove(this);
                    }
                });
                _containingList = value;
            } 
        }

        public DelegateCommand DeleteAccountGroupCommand { get; set; }
        public DelegateCommand EditAccountGroupCommand { get; set; }

        #region AccountNumber
        private string _accountNumber;

        public string AccountNumber {
            get { return _accountNumber; }
            set {
                if (_accountNumber != value) {
                    _accountNumber = value;
                    OnPropertyChanged("AccountNumber");
                    OnPropertyChanged("AccountDisplayString");
                }
            }
        }
        #endregion AccountNumber

        #region AccountName
        private string _accountName;

        public string AccountName {
            get { return _accountName; }
            set {
                if (_accountName != value) {
                    _accountName = value;
                    OnPropertyChanged("AccountName");
                    OnPropertyChanged("AccountDisplayString");
                }
            }
        }
        #endregion AccountName

        #region GroupComment
        private string _groupComment;

        public string GroupComment {
            get { return _groupComment; }
            set {
                if (_groupComment != value) {
                    _groupComment = value;
                    OnPropertyChanged("GroupComment");
                    OnPropertyChanged("HasComment");
                }
            }
        }
        #endregion GroupComment

        // used in TaxonomyAndBalanceListBase.xaml
        public bool HasComment { get { return !string.IsNullOrEmpty(GroupComment); } }
        public string AccountDisplayString { get { return (AccountNumber != null ? AccountNumber + " - " : "") + AccountName; } }
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
        #endregion

        #region constructor
        public AccountGroupInfo() {
            ChildrenInfos = new ObservableCollection<AccountGroupChildInfo>();
            MissingChildrenInfos = new ObservableCollection<AccountGroupChildInfo>();
        }

        /// <summary>
        /// Constructor for load the details when coming from database
        /// </summary>
        /// <param name="parentRaw">The xml string that stores the values of the account group</param>
        /// <param name="childrenRaw">The xml string that stores the values of the accounts for the account group</param>
        public AccountGroupInfo(string parentRaw, string childrenRaw) {
            if (!string.IsNullOrEmpty(parentRaw)) {
                using (XmlReader reader = new XmlTextReader(new StringReader(parentRaw))) {
                    reader.Read();
                    AccountNumber = reader.GetAttribute("Number");
                    GroupComment = reader.GetAttribute("Comment");
                    AccountName = reader.GetAttribute("Name");
                }
            }

            if (!string.IsNullOrEmpty(childrenRaw)) {
                ChildrenInfos = new ObservableCollection<AccountGroupChildInfo>();
                using (XmlReader reader = new XmlTextReader(new StringReader(childrenRaw))) {
                    while (reader.Read()) {
                        if (reader.Name == "child" && reader.NodeType == XmlNodeType.Element) {
                            string name = reader.GetAttribute("Name");
                            string number = reader.GetAttribute("Number");
                            string comment = reader.GetAttribute("Comment");
                            Debug.Assert(name != null);
                            Debug.Assert(number != null);
                            Debug.Assert(comment != null);
                            ChildrenInfos.Add(new AccountGroupChildInfo {
                                Name = name,
                                Number = number,
                                Comment = comment, 
                                Parent = this
                            });
                        }
                    }
                }
            } else {
                ChildrenInfos = new ObservableCollection<AccountGroupChildInfo>();
            }
            MissingChildrenInfos = new ObservableCollection<AccountGroupChildInfo>();
        }
        #endregion

        #region methods
        /// <summary>
        /// Converts the class into an xml string for database storage.
        /// </summary>
        public string GetChildrenRaw() {
            XElement root = new XElement("root", null);
            foreach (AccountGroupChildInfo accountGroupChildInfo in ChildrenInfos) {
                root.Add(new XElement("child", new XAttribute("Name", accountGroupChildInfo.Name),
                                      new XAttribute("Number", accountGroupChildInfo.Number),
                                      new XAttribute("Comment", accountGroupChildInfo.Comment ?? string.Empty)));
            }
            return root.ToString(SaveOptions.DisableFormatting);
        }

        /// <summary>
        /// Converts the class into an xml string for database storage.
        /// </summary>
        public string GetParentRaw() {
            XElement parent = new XElement("parent", new XAttribute("Number", AccountNumber),
                                           new XAttribute("Comment", GroupComment),
                                           new XAttribute("Name", AccountName));
            return parent.ToString(SaveOptions.DisableFormatting);
        }

        public void RefreshCount() { OnPropertyChanged("ChildrenInfos"); }
        #endregion
    }
}
