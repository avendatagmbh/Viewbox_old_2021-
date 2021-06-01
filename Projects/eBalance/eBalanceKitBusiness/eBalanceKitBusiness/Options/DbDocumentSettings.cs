using System.Text;
using System.Xml;
using DbAccess;
using eBalanceKitBusiness.Structures.DbMapping;

namespace eBalanceKitBusiness.Options {

    [DbTable("document_settings", ForceInnoDb = true)]
    internal class DbDocumentSettings : GlobalOptionsBase, IDocumentSettings {
        public DbDocumentSettings() {
            XmlDocument = new XmlDocument();
            ShowOnlyMandatoryPostions = false;
            ShowSelectedLegalForm = true;
            ShowSelectedTaxonomy = true;
            ShowTypeOperatingResult = true;
        }

        public void Init(User user, Document document) {
            User = user;
            Document = document;
            ShowSelectedLegalForm = GetBoolValue("ShowSelectedLegalForm").GetValueOrDefault(true);
            ShowSelectedTaxonomy = GetBoolValue("ShowSelectedTaxonomy").GetValueOrDefault(true);
            ShowTypeOperatingResult = GetBoolValue("ShowTypeOperatingResult").GetValueOrDefault(true);
            ShowOnlyMandatoryPostions = GetBoolValue("ShowOnlyMandatoryPostions").GetValueOrDefault(false);
            
        }
        #region properties


        #region Document
        private int _documentId;
        private Document _document;

        [DbColumn("document_id")]
        public int DocumentId {
            get { return _documentId; }
            set {
                if (_documentId == value)
                    return;
                _documentId = value;
            }
        }

        public Document Document {
            get {
                return _document;
            }
            set {
                if (_document == value)
                    return;
                _document = value;
                DocumentId = value.Id;
            }
        }

        #endregion Document


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

        #endregion properties

        #region Methods

        #endregion Methods

        public DbDocumentSettings Clone() {
            var result = new DbDocumentSettings(){Id = 0, XmlContent = XmlContent};
            result.Init(User, Document);
            return result;
        }

        protected override string GetDefaultXml() {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("<c>");
            stringBuilder.Append("<ShowSelectedLegalForm>");
            stringBuilder.Append(true);
            stringBuilder.Append("</ShowSelectedLegalForm>");
            stringBuilder.Append("<ShowOnlyMandatoryPostions>");
            stringBuilder.Append(false);
            stringBuilder.Append("</ShowOnlyMandatoryPostions>");
            stringBuilder.Append("<ShowSelectedTaxonomy>");
            stringBuilder.Append(true);
            stringBuilder.Append("</ShowSelectedTaxonomy>");
            stringBuilder.Append("<ShowTypeOperatingResult>");
            stringBuilder.Append(true);
            stringBuilder.Append("</ShowTypeOperatingResult>");
            stringBuilder.Append("</c>");
            return stringBuilder.ToString();
        }
    }
}
