using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using DbAccess;
using Taxonomy;
using Taxonomy.Interfaces;
using System.Xml;
using eBalanceKitBusiness.Manager;

namespace eBalanceKitBusiness.Structures.DbMapping.MappingTemplate
{

    /// <summary>
    /// This class represents the header of an account assignment template.
    /// </summary>
    [DbTable("mapping_template_heads_gcd", ForceInnoDb = true)]
    public class MappingTemplateHeadGCD : INotifyPropertyChanged, IComparable {

        public void InitMappingTemplateDictionaries()
        {
            _elementInfosByName = new Dictionary<string, MappingTemplateElementInfoGCD>();
            foreach (var elementInfo in ElementInfos)
                _elementInfosByName[elementInfo.ElementId] = elementInfo;
        }

        public MappingTemplateHeadGCD()
        {
            //AccountGroups = new ObservableCollection<MappingTemplateAccountGroup>();
            //SplitAccounts = new ObservableCollection<MappingTemplateSplitAccount>();
        }

        #region events
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string property)
        {
            if (PropertyChanged != null)
            {
                ClassArgumentHelper.Instance.OnPropertyChangedWithStringValidater(property, this);
                //base.OnPropertyChanged(propertyName);
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }
        #endregion events

        #region properties

        #region Id
        [DbColumn("id", AllowDbNull = false)]
        [DbPrimaryKey]
        public int Id { get; set; }
        #endregion

        #region Name
        [DbColumn("name", Length = 64)]
        public string Name
        {
            get { return _name; }
            set
            {
                if (_name != value)
                {
                    _name = value;
                    //if (_name != null && _name.Length > 62)
                    //    _name = _name.Substring(0, 62);
                    OnPropertyChanged("Name");
                }
            }
        }

        private string _name;
        #endregion

        #region AccountStructure
        [DbColumn("account_structure", Length = 128)]
        public string AccountStructure { get; set; }
        #endregion

        #region Comment
        private string _comment;

        [DbColumn("comment", Length = 1024, AllowDbNull = true)]
        public string Comment
        {
            get { return _comment; }
            set
            {
                _comment = value;
                //if (_comment != null && _comment.Length > 1022)
                //    _comment = _comment.Substring(0, 1022);
                OnPropertyChanged("Comment");
            }
        }
        #endregion

        #region Creator
        private User _creator;

        private int _creatorId;

        [DbColumn("creator_id")]
        public int CreatorId
        {
            get { return _creatorId; }
            set
            {
                _creatorId = value;
                Creator = UserManager.Instance.GetUser(value);
            }
        }

        #region CreationDate
        [DbColumn("creation_date")]
        public DateTime CreationDate { get; set; }
        #endregion

        public User Creator
        {
            get { return _creator; }
            set
            {
                if (_creator == value) return;
                _creator = value;
                CreatorId = value.Id;
            }
        }

        public string CreatorDisplayString
        {
            get
            {
                if (Creator != null)
                    return "Erstellt von " + Creator.DisplayString + " am " +
                           CreationDate.ToString("dd.MM.yyyy") +
                           " um " + CreationDate.ToString("HH:mm:ss") + ".";
                return "Erstellt am " +
                       CreationDate.ToString("dd.MM.yyyy") +
                       " um " + CreationDate.ToString("HH:mm:ss") + ".";
            }
        }
        #endregion Creator

        #region LastModifier
        private User _lastModifier;

        private int _lastModifierId;

        [DbColumn("last_modifier_id")]
        public int LastModifierId
        {
            get { return _lastModifierId; }
            set
            {
                _lastModifierId = value;
                LastModifier = UserManager.Instance.GetUser(value);
            }
        }

        public User LastModifier
        {
            get { return _lastModifier; }
            set
            {
                if (_lastModifier == value) return;
                if (value == null) return;
                _lastModifier = value;
                LastModifierId = value.Id;
                OnPropertyChanged("LastModifier");
                OnPropertyChanged("LastModifierDisplayString");
            }
        }

        #region LastModifyDate
        private DateTime? _lastModifyDate;

        [DbColumn("modify_date")]
        public DateTime? LastModifyDate
        {
            get { return _lastModifyDate; }
            set
            {
                _lastModifyDate = value;
                OnPropertyChanged("LastModifyDate");
                OnPropertyChanged("LastModifierDisplayString");
            }
        }
        #endregion

        public string LastModifierDisplayString
        {
            get
            {
                if (LastModifier == null || !LastModifyDate.HasValue) return null;
                return "Zuletzt geändert von " + LastModifier.DisplayString + " am " +
                       LastModifyDate.Value.ToString("dd.MM.yyyy") +
                       " um " + LastModifyDate.Value.ToString("HH:mm:ss") + ".";
            }
        }

        #endregion last Modifier

        /*

        #region SplitAccounts
        [DbCollection("Head")]
        public ObservableCollection<MappingTemplateSplitAccount> SplitAccounts { get; set; }
        #endregion
         */

        //#region SplitAccounts
        //ObservableCollection<MappingTemplateSplitAccount> _splitAccounts;

        //[DbCollection("Head")]
        //public ObservableCollection<MappingTemplateSplitAccount> SplitAccounts {
        //    get {
        //        if (_splitAccounts == null) {
        //            _splitAccounts = new ObservableCollection<MappingTemplateSplitAccount>();
        //            OnPropertyChanged("Items");
        //        }
        //        return _splitAccounts;
        //    }
        //    set {
        //        if (_splitAccounts != value) {
        //            _splitAccounts = value;
        //            OnPropertyChanged("Items");
        //            OnPropertyChanged("Assignments");
        //        }
        //    }
        //}
        //#endregion

        /*
        #region AccountGroups
        [DbCollection("Head")]
        public ObservableCollection<MappingTemplateAccountGroup> AccountGroups { get; set; }
        #endregion
         */

        //#region AccountGroups
        //ObservableCollection<MappingTemplateAccountGroup> _accountGroups;

        //[DbCollection("Head")]
        //public ObservableCollection<MappingTemplateAccountGroup> AccountGroups {
        //    get {
        //        if (_accountGroups == null) {
        //            _accountGroups = new ObservableCollection<MappingTemplateAccountGroup>();
        //            OnPropertyChanged("Items");
        //        }
        //        return _accountGroups;
        //    }
        //    set {
        //        if (_accountGroups != value) {
        //            _accountGroups = value;
        //            OnPropertyChanged("Items");
        //            OnPropertyChanged("Assignments");
        //        }
        //    }
        //}
        //#endregion

        /*
        #region Assignments
        [DbCollection("Template")]
        public ObservableCollection<MappingTemplateLine> Assignments
        {
            get
            {
                if (_items == null)
                {
                    _items = new ObservableCollection<MappingTemplateLine>();
                    _items.CollectionChanged += ItemsCollectionChanged;
                    OnPropertyChanged("Items");
                }
                return _items;
            }
            set
            {
                if (_items != value)
                {
                    _items = value;
                    OnPropertyChanged("Items");
                    OnPropertyChanged("Assignments");
                }
            }
        }

        void ItemsCollectionChanged(object sender, global::System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (MappingTemplateLine line in from object item in e.NewItems select item as MappingTemplateLine)
                {
                    _assignmentDict[line.AccountNumber] = line;
                }
            }

            if (e.OldItems != null)
            {
                foreach (MappingTemplateLine line in from object item in e.OldItems select item as MappingTemplateLine)
                {
                    _assignmentDict.Remove(line.AccountNumber);
                }
            }
        }
        ObservableCollection<MappingTemplateLine> _items;
        private readonly Dictionary<string, MappingTemplateLine> _assignmentDict = new Dictionary<string, MappingTemplateLine>();
        #endregion
        */

        #region ElementInfos
        [DbCollection("Template")]
        internal IList<MappingTemplateElementInfoGCD> ElementInfos { get { return _elementInfos; } }
        private readonly List<MappingTemplateElementInfoGCD> _elementInfos = new List<MappingTemplateElementInfoGCD>();
        private Dictionary<string, MappingTemplateElementInfoGCD> _elementInfosByName;
        #endregion

        #region TaxonomyInfoId
        private int _taxonomyInfoId;

        [DbColumn("taxonomy_info_id")]
        public int TaxonomyInfoId
        {
            get { return _taxonomyInfoId; }
            private set
            {
                if (_taxonomyInfoId == value) return;
                _taxonomyInfoId = value;
                TaxonomyInfo = TaxonomyManager.GetTaxonomyInfo(TaxonomyInfoId);
            }
        }
        #endregion

        #region TaxonomyInfo
        private ITaxonomyInfo _taxonomyInfo;

        public ITaxonomyInfo TaxonomyInfo
        {
            get { return _taxonomyInfo; }
            set
            {
                if (_taxonomyInfo == value) return;
                _taxonomyInfo = value;
                TaxonomyInfoId = ((TaxonomyInfo)value).Id;

                // reset _taxonomyIdManager
                _taxonomyIdManager = null;

                OnPropertyChanged("TaxonomyInfo");
            }
        }
        #endregion

        #region Taxonomy
        public ITaxonomy Taxonomy { get { return TaxonomyManager.GetTaxonomy(TaxonomyInfo); } }
        #endregion

        #region TaxonomyIdManager
        private TaxonomyIdManager _taxonomyIdManager;
        internal TaxonomyIdManager TaxonomyIdManager { get { return _taxonomyIdManager ?? (_taxonomyIdManager = new TaxonomyIdManager(null, TaxonomyInfo)); } }
        #endregion

        #region ToXml
        public XmlDocument ToXml()
        {
            var doc = new XmlDocument();
            var root = doc.CreateElement("root"); doc.AppendChild(root);
            root.Attributes.Append(doc.CreateAttribute("Name")).InnerText = Name;
            root.Attributes.Append(doc.CreateAttribute("Comment")).InnerText = Comment;
            root.Attributes.Append(doc.CreateAttribute("Id")).InnerText = Convert.ToString(Id);
            return doc;

        }
        #endregion

        #region IsSelected
        private bool _isSelected;

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                OnPropertyChanged("IsSelected");
            }
        }
        #endregion IsSelected

        #region IsActualTaxonomyVersion
        public bool IsActualTaxonomyVersion { get { return TaxonomyInfo.Version == TaxonomyManager.GetLatestTaxonomyInfo(TaxonomyInfo.Type).Version; } }
        #endregion

        #endregion properties

        #region methods

        #region GetElementInfo
        internal MappingTemplateElementInfoGCD GetElementInfo(string id)
        {
            if (_elementInfosByName == null) InitMappingTemplateDictionaries();
            MappingTemplateElementInfoGCD elementInfo = null;
            if (_elementInfosByName != null) {
                _elementInfosByName.TryGetValue(id, out elementInfo);
                if (elementInfo == null) _elementInfosByName.TryGetValue("de-gcd_" + id, out elementInfo);
            }
            return elementInfo;
        }
        #endregion

        #region CompareTo
        public int CompareTo(object obj)
        {
            if (!(obj is MappingTemplateHeadGCD)) return 0;
            var templ = (MappingTemplateHeadGCD)obj;
            if (Name == null) return templ.Name == null ? 0 : 1;
            return templ.Name == null ? -1 : Name.CompareTo(templ.Name);
        }
        #endregion

        /*
        public bool HasAssignment(string accountNumber) { return _assignmentDict.ContainsKey(accountNumber); }

        public MappingTemplateLine GetAssignment(string accountNumber)
        {
            MappingTemplateLine result;
            _assignmentDict.TryGetValue(accountNumber, out result);
            return result;
        }
         */
        #endregion methods
    }
}
