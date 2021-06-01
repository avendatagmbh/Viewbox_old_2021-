// --------------------------------------------------------------------------------
// author: Sebastian Vetter, Mirko Dibbert
// since: 2012-04-17
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Xml;
using DbAccess;
using eBalanceKitBusiness.Export;
using eBalanceKitBusiness.Structures.DbMapping;

namespace eBalanceKitBusiness.Options {
    [DbTable("GlobalOptions", ForceInnoDb = true)]
    internal class AvailableOptions : GlobalOptionsBase, IOptions {

        public AvailableOptions() {
            XmlDocument = new XmlDocument();
            PropertyChanged += AvailableOptionsPropertyChanged;
        }

        void AvailableOptionsPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            if (e.PropertyName.Equals("XmlContent")) {
                Init();
            }
        }

        public AvailableOptions(User user){
            XmlDocument = new XmlDocument();
            User = user;
            XmlContent = GetDefaultXml();
            Init();
        }

        #region properties

        #region ShowSelectedLegalForm
        private bool _showSelectedLegalForm;
        /// <summary>
        /// Hide or show only informations for the current selected legal form.
        /// </summary>
        public bool ShowSelectedLegalForm {
            get { return _showSelectedLegalForm; }
            set {
                //if (value == _showSelectedLegalForm) return;
                _showSelectedLegalForm = value;
                SetValue("ShowSelectedLegalForm", value);
                OnPropertyChanged("ShowSelectedLegalForm");
            }
        }
        #endregion ShowSelectedLegalForm

        #region HideChosenWarnings
        private bool _hideChosenWarnings;
        /// <summary>
        /// Hide or show the marked warnings after validation.
        /// </summary>
        public bool HideChosenWarnings {
            get { return _hideChosenWarnings; }
            set {
                //if (value == _hideChosenWarnings) return;
                _hideChosenWarnings = value;
                SetValue("HideChosenWarnings", value);
                OnPropertyChanged("HideChosenWarnings");
            }
        }
        #endregion ShowWarnings

        #region HideAllWarnings
        private bool _hideAllWarnings;
        /// <summary>
        /// Hide or show warnings after validation.
        /// </summary>
        public bool HideAllWarnings {
            get { return _hideAllWarnings; }
            set {
                //if (value == _hideAllWarnings) return;
                _hideAllWarnings = value;
                SetValue("HideAllWarnings", value);
                OnPropertyChanged("HideAllWarnings");
            }
        }

        #endregion HideAllWarnings

        #region ShowTypeOperatingResult
        private bool _showTypeOperatingResult;
        /// <summary>
        /// Hide or show only informations for the current selected TypeOperatingResult (GKV/UKV)
        /// </summary>
        public bool ShowTypeOperatingResult {
            get { return _showTypeOperatingResult; }
            set {
                //if (value == _showTypeOperatingResult) return;
                _showTypeOperatingResult = value;
                SetValue("ShowTypeOperatingResult", value);
                OnPropertyChanged("ShowTypeOperatingResult");
            }
        }
        #endregion ShowTypeOperatingResult

        #region ShowOnlyMandatoryPostions
        private bool _showOnlyMandatoryPostions;
        /// <summary>
        /// Hide or show only mandatory postions
        /// </summary>
        public bool ShowOnlyMandatoryPostions {
            get { return _showOnlyMandatoryPostions; }
            set {
                //if (value == _showTypeOperatingResult) return;
                _showOnlyMandatoryPostions = value;
                SetValue("ShowOnlyMandatoryPostions", value);
                OnPropertyChanged("ShowOnlyMandatoryPostions");
            }
        }
        #endregion ShowOnlyMandatoryPostions
        
        #region ShowSelectedTaxonomy
        private bool _showSelectedTaxonomy;
        /// <summary>
        /// Hide or show only informations for the current selected taxonomy.
        /// </summary>
        public bool ShowSelectedTaxonomy {
            get { return _showSelectedTaxonomy; }
            set {
                //if (value == _showSelectedTaxonomy) return;
                _showSelectedTaxonomy = value;
                SetValue("ShowSelectedTaxonomy", value);
                OnPropertyChanged("ShowSelectedTaxonomy");
            }
        }
        #endregion ShowSelectedTaxonomy

        #region AuditModeEnabled
        private bool _auditModeEnabled;

        public bool AuditModeEnabled {
            get { return _auditModeEnabled; }
            set {
                if (_auditModeEnabled == value) return;
                _auditModeEnabled = value;
                SetValue("AuditModeEnabled", value);
                OnPropertyChanged("AuditModeEnabled");
            }
        }
        #endregion // AuditModeEnabled

        #region ManagementAssistantAskAgain
        private bool _managementAssistantAskAgain;

        public bool ManagementAssistantAskAgain {
            get { return _managementAssistantAskAgain; }
            set {
                if (_managementAssistantAskAgain != value) {
                    _managementAssistantAskAgain = value;
                    SetValue("ManagementAssistantAskAgain", value);
                    OnPropertyChanged("ManagementAssistantAskAgain");
                }
            }
        }
        #endregion ManagementAssistantAskAgain


        #endregion // properties

        #region methods

        private void Init() {
            XmlDocument.InnerXml = XmlContent;
            ShowSelectedLegalForm = GetBoolValue("ShowSelectedLegalForm").GetValueOrDefault(true);
            ShowSelectedTaxonomy = GetBoolValue("ShowSelectedTaxonomy").GetValueOrDefault(true);
            HideChosenWarnings = GetBoolValue("HideChosenWarnings").GetValueOrDefault(false);
            HideAllWarnings = GetBoolValue("HideAllWarnings").GetValueOrDefault(false);
            ShowTypeOperatingResult = GetBoolValue("ShowTypeOperatingResult").GetValueOrDefault(true);
            ShowOnlyMandatoryPostions = GetBoolValue("ShowOnlyMandatoryPostions").GetValueOrDefault(false);
            AuditModeEnabled = GetBoolValue("AuditModeEnabled").GetValueOrDefault(false);
            ManagementAssistantAskAgain = GetBoolValue("ManagementAssistantAskAgain").GetValueOrDefault(true);

            _pdfOptions = new PdfOptions(this);
        }
        
        #region PdfExportOptionMethods
        private PdfOptions _pdfOptions;

        public void GetPdfExportOptionsFromDatabase(IExportConfig config) {
            _pdfOptions.GetPdfExportOptionsFromDatabase(config);
        }

        public void SetPdfExportOptionsValue(IExportConfig config) { _pdfOptions.SetPdfExportOptionsValue(config); }

        #endregion // PdfExportOptionMethods

        public override void Reset() { 
            base.Reset();
            Init();
        }

        #endregion // methods
    }
}