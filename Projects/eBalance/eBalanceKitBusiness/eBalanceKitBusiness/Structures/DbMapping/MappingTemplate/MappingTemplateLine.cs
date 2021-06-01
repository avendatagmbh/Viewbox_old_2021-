// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-02-14
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using DbAccess;
using Taxonomy;
using Utils;

namespace eBalanceKitBusiness.Structures.DbMapping.MappingTemplate {
    /// <summary>
    /// This class represents a line of an account assignment template.
    /// </summary>
    [DbTable("mapping_template_lines", ForceInnoDb = true)]
    public class MappingTemplateLine : NotifyPropertyChangedBase {
        #region Id
        [DbColumn("id", AllowDbNull = false)]
        [DbPrimaryKey]
        public int Id { get; set; }
        #endregion

        #region Template
        [DbColumn("template_id", IsInverseMapping = true)]
        public MappingTemplateHead Template { get; set; }
        #endregion

        #region AccountNumber
        [DbColumn("account_number", Length = 32)]
        public string AccountNumber { get; set; }
        #endregion

        #region AccountName
        [DbColumn("account_name", Length = 1024)]
        public string AccountName { get; set; }
        #endregion

        #region IsAccountOfExchange
        private bool _isAccountOfExchange;

        [DbColumn("is_account_of_exchange")]
        public bool IsAccountOfExchange {
            get { return _isAccountOfExchange; }
            set {
                _isAccountOfExchange = value;
                OnPropertyChanged("IsAccountOfExchange");
            }
        }
        #endregion

        #region ElementId
        public string ElementId {
            get { return DebitElementId ?? CreditElementId; }
            set {
                if ((DebitElementId == null) && (CreditElementId == null)) {
                    DebitElementId = value;
                    CreditElementId = value;
                } else {
                    if (DebitElementId != null) DebitElementId = value;
                    if (CreditElementId != null) CreditElementId = value;
                }
            }
        }
        #endregion

        #region Element
        public IElement DebitElement {
            get {
                if (DebitElementId == null) return null; 
                IElement element;
                Template.Taxonomy.Elements.TryGetValue(DebitElementId, out element);
                return element;
            }
        }
        
        public IElement CreditElement {
            get {
                if (CreditElementId == null) return null;
                IElement element;
                Template.Taxonomy.Elements.TryGetValue(CreditElementId, out element);
                return element;
            }            
        }
        #endregion

        #region DebitElementId
        private string _debitElementId;

        [DbColumn("debit_element_id", Length = 2048)]
        public string DebitElementId {
            get { return _debitElementId; }
            set {
                _debitElementId = value;
                OnPropertyChanged("DebitElementId");
                OnPropertyChanged("ElementId");
            }
        }
        #endregion

        #region CreditElementId
        private string _creditElementId;

        [DbColumn("credit_element_id", Length = 2048)]
        public string CreditElementId {
            get { return _creditElementId; }
            set {
                _creditElementId = value;
                OnPropertyChanged("CreditElementId");
                OnPropertyChanged("ElementId");
            }
        }
        #endregion

        #region AccountLabel
        public string AccountLabel { get { return AccountNumber + " - " + AccountName; } }
        #endregion
    }
}