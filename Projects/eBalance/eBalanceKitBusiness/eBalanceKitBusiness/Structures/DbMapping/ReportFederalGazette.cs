// --------------------------------------------------------------------------------
// author: Solueman Hussain
// since: 2012-01-03
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using DbAccess;
using Taxonomy;
using Taxonomy.Interfaces;
using Utils;
using eBalanceKitBase.Structures;
using eBalanceKitBusiness.Interfaces;
using eBalanceKitBusiness.Manager;
using eBalanceKitBusiness.Structures.DbMapping.ValueMappings;
using eBalanceKitBusiness.Structures.Presentation;
using eBalanceKitBusiness.Structures.XbrlElementValue;
using eBalanceKitBusiness.Validators;

namespace eBalanceKitBusiness.Structures.DbMapping {
    [DbTable("report_federal_gazette", ForceInnoDb = true)]
    public class ReportFederalGazette : NotifyPropertyChangedBase, ITaxonomyIdManagerProvider {

        #region properties

        #region Id
        private int _id;

        [DbColumn("id", AllowDbNull = false)]
        [DbPrimaryKey]
        public int Id {
            get { return _id; }
            set {
                if (_id == value) return;
                _id = value;
                OnPropertyChanged("Id");
            }
        }
        #endregion

        #region Name
        private string _name;

        [DbColumn("name", AllowDbNull = true, Length = 256)]
        public string Name {
            get { return _name; }
            set {
                if (_name == value) return;
                _name = value;
                OnPropertyChanged("Name");
            }
        }
        #endregion

        #region Comment
        private string _comment;

        [DbColumn("comment", AllowDbNull = true, Length = 1024)]
        public string Comment {
            get { return _comment; }
            set {
                if (_comment == value) return;
                _comment = value;
                OnPropertyChanged("Comment");
            }
        }
        #endregion

        #region CompanyId
        private int _companyId;

        [DbColumn("company_id", AllowDbNull = false)]
        public int CompanyId {
            get { return _companyId; }
            set {
                if (_companyId == value) return;
                _companyId = value;
                OnPropertyChanged("CompanyId");
            }
        }

        private Company _company;

        public Company Company {
            get { return _company; }
            set {
                if (_company == value) return;
                OnPropertyChanging("Company");
                _company = value;
                CompanyId = value.Id;
                OnPropertyChanged("Company");
            }
        }
        #endregion


        #region DocumentId
        private int _documentId;

        [DbColumn("document_id", AllowDbNull = false)]
        public int DocumentId {
            get { return _documentId; }
            set {
                if (_documentId == value) return;
                _documentId = value;
                OnPropertyChanged("DocumentId");
            }
        }

        private Document _document;

        public Document Document {
            get { return _document; }
            set {
                if (_document == value) return;
                _document = value;
                DocumentId = value.Id;
                OnPropertyChanged("Document");
            }
        }
        #endregion


        #region OwnerId
        private int _ownerId;

        [DbColumn("owner_id", AllowDbNull = false)]
        public int OwnerId {
            get { return _ownerId; }
            set {
                if (_ownerId == value) return;
                _ownerId = value;
                OnPropertyChanged("OwnerId");
            }
        }

        private User _owner;

        public User Owner {
            get { return _owner; }
            set {
                if (_owner == value) return;
                OwnerId = value.Id;
                _owner = value;
                OnPropertyChanged("Owner");
            }
        }
        #endregion


        #region GaapTaxonomyInfoId
        private string _gaapTaxonomyInfoId;

        [DbColumn("gaap_taxonomy_info_id", AllowDbNull = false)]
        public string GaapTaxonomyInfoId {
            get { return _gaapTaxonomyInfoId; }
            set {
                if (_gaapTaxonomyInfoId == value) return;
                _gaapTaxonomyInfoId = value;
                OnPropertyChanged("GaapTaxonomyInfoId");
            }
        }
        #endregion

        #region CreationDate
        [DbColumn("creation_date")]
        public DateTime CreationDate { get; set; }
        #endregion

        public IValidator ValidatorGaapFgTaxonomy { get; private set; }
        public Dictionary<string, IPresentationTree> GaapFgPresentationTrees { get; set; }
        private readonly ObservableCollectionAsync<IBalanceList> _items = new ObservableCollectionAsync<IBalanceList>();
        public ObservableCollectionAsync<IBalanceList> BalanceLists { get { return _items; } }
        #endregion


        #region FgGtapTaxonomyInfoId
        private int _fgGaapTaxonomyInfoId;

        public int FgGaapTaxonomyInfoId {
            get { return _fgGaapTaxonomyInfoId; }
            set {
                if (_fgGaapTaxonomyInfoId == value) return;
                _fgGaapTaxonomyInfoId = value;
            }
        }
        #endregion

        #region FgGaapTaxonomyInfo
        private ITaxonomyInfo _fgGaapTaxonomyInfo;

        public ITaxonomyInfo FgGaapTaxonomyInfo {
            get { return _fgGaapTaxonomyInfo; }
            set {
                if (_fgGaapTaxonomyInfo == value) return;
                ITaxonomyInfo oldValue = _fgGaapTaxonomyInfo;
                FgGaapTaxonomyInfoId = ((TaxonomyInfo) value).Id;

                _valueTreeFg = null;
                _taxonomyIdManager = null;
                OnPropertyChanged("FgGaapTaxonomyInfo");

            }
        }
        #endregion


        #region FgGaapTaxonomy
        public ITaxonomy FgGaapTaxonomy { get { return TaxonomyManager.GetTaxonomy(FgGaapTaxonomyInfo); } }
        #endregion


        #region ValueTree
        private ValueTree.ValueTree _valueTreeFg;

        public ValueTree.ValueTree ValueTreeFg {
            get { return _valueTreeFg; }
            set {
                _valueTreeFg = value;
                if (_valueTreeFg == value) return;
                InitValueMain();
                OnPropertyChanged("ValueTreeFg");

            }
        }
        #endregion


        #region Methods

        #region IniTValueTreeMain
        private void InitValueMain() {
            GaapFgPresentationTrees = PresentationTree.CreatePresentationTrees(FgGaapTaxonomy, ValueTreeFg, BalanceLists);
            ValueTreeFg.Root.ForceRecomputation();
            XbrlElementValueBase.InitStates(ValueTreeFg.Root);
        }
        #endregion


        #region LoadValueTree
        public void LoadValueTrees(ProgressInfo progressInfo) {
            ValueTreeFg = ValueTree.ValueTree.CreateValueTree(typeof (ValuesGAAPFG), this, FgGaapTaxonomy, progressInfo);
        }
        #endregion

        #endregion

        private TaxonomyIdManager _taxonomyIdManager;
        public TaxonomyIdManager TaxonomyIdManager { get { return _taxonomyIdManager ?? (_taxonomyIdManager = new TaxonomyIdManager(null, FgGaapTaxonomyInfo)); } }

    }
}