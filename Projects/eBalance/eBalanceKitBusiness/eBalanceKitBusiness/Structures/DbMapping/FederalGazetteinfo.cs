using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DbAccess;
using Utils;
using eBalanceKitBusiness.Interfaces;
using eBalanceKitBusiness.Logs;

namespace eBalanceKitBusiness.Structures.DbMapping {
    [DbTable("federal_gazette_info", ForceInnoDb = true)]
    public class FederalGazetteInfo : NotifyPropertyChangedBase {


        
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

        #region Notes
        private bool _notes;

        [DbColumn("notes")]
        public bool Notes {
            get { return _notes; }
            set {
                if (_notes == value) return;
                _notes = value;
                OnPropertyChanged("Notes");
            }
        }
        #endregion

        #region Assets
        private bool _assets;

        [DbColumn("assets")]
        public bool Assets {
            get { return _assets; }
            set {
                if (_assets == value) return;
                _assets = value;
                OnPropertyChanged("Assets");
            }
        }
        #endregion

        #region Balance
        private bool _balance;

        [DbColumn("balance")]
        public bool Balance {
            get { return _balance; }
            set {
                if (_balance == value) return;
                _balance = value;
                OnPropertyChanged("Balance");
            }
        }
        #endregion

        #region IncomeUse
        private bool _incomeUse;

        [DbColumn("income_use")]
        public bool IncomeUse {
            get { return _incomeUse; }
            set {
                if (_incomeUse == value) return;
                _incomeUse = value;
                OnPropertyChanged("IncomeUse");
            }

        }
        #endregion

        #region IncomeStatement
        private bool _incomeStatement;

        [DbColumn("income_statement")]
        public bool IncomeStatement {
            get { return _incomeStatement; }
            set {
                if (_incomeStatement == value) return;
                _incomeStatement = value;
                OnPropertyChanged("IncomeStatement");
            }
        }
        #endregion

        #region MangementReport
        private bool _managementReport;

        [DbColumn("management_report")]
        public bool ManagementReport {
            get { return _managementReport; }
            set {
                if (_managementReport == value) return;
                _managementReport = value;
                OnPropertyChanged("ManagementReport");
            }
        }
        #endregion

        #region StartDate
        private DateTime _startDate;

        [DbColumn("start_date")]
        public DateTime StartDate {
            get { return _startDate; }
            set {
                if (_startDate == value) return;
                _startDate = value;
                OnPropertyChanged("StartDate");
            }
        }
        #endregion

        #region EndDate
        private DateTime _endDate;

        [DbColumn("end_date")]
        public DateTime EndDate {
            get { return _endDate; }
            set {
                if (_endDate == value) return;
                _endDate = value;
                OnPropertyChanged("EndDate");
            }
        }
        #endregion

        #region ExemptionDecision
        private bool _exemptionDecision;

        [DbColumn("exemption_decision")]
        public bool ExemptionDecision {
            get { return _exemptionDecision; }
            set {
                if (_exemptionDecision == value) return;
                _exemptionDecision = value;
                OnPropertyChanged("ExemptionDecision");
            }
        }
        #endregion

        #region ExemptionNote
        private bool _exemptionNote;

        [DbColumn("exemption_note")]
        public bool ExemptionNote {
            get { return _exemptionNote; }
            set {
                if (_exemptionNote == value) return;
                _exemptionNote = value;
                OnPropertyChanged("ExemptionNote");
            }
        }
        #endregion

        #region LossAssumption
        private bool _lossAssumption;

        [DbColumn("loss_assumption")]
        public bool LossAssumption {
            get { return _lossAssumption; }
            set {
                if (_lossAssumption == value) return;
                _lossAssumption = value;
                OnPropertyChanged("LossAssumption");
            }
        }
        #endregion

        #region OrderType
        private int _orderType;

        [DbColumn("oder_type")]
        public int OrderType {
            get { return _orderType; }
            set {
                if (_orderType == value) return;
                _orderType = value;
                OnPropertyChanged("OrderType");
            }
        }
        #endregion

        #region PublicationArea
        private int _publicationArea;

        [DbColumn("publication_area")]

        public int PublicationArea {
            get { return _publicationArea; }
            set {
                if (_publicationArea == value) return;
                _publicationArea = value;
                OnPropertyChanged("PublicationArea");
            }
        }
        #endregion

        #region PublicationCategory
        private int _publicationCategory;

        [DbColumn("publication_category")]
        public int PublicationCategory {
            get { return _publicationCategory; }
            set {
                if (_publicationCategory == value) return;
                _publicationCategory = value;
                OnPropertyChanged("PublicationCategory");
            }
        }
        #endregion

        #region PublicationType
        private int _publicationType;

        [DbColumn("publication_type")]
        public int PublicationType {
            get { return _publicationType; }
            set {
                if (_publicationType == value) return;
                _publicationType = value;
                OnPropertyChanged("PublicationType");
            }
        }
        #endregion

        #region PublicationDescription
        private int _publicationDescription;

        [DbColumn("publication_description")]
        public int PublicationDescription {
            get { return _publicationDescription; }
            set {
                if (_publicationDescription == value) return;
                _publicationDescription = value;
                OnPropertyChanged("PublicationDescription");
            }
        }
        #endregion

        #region CompanyType
        private int _companyType;

        [DbColumn("company_type")]
        public int CompanyType {
            get { return _companyType; }
            set {
                if (_companyType == value) return;
                _companyType = value;
                OnPropertyChanged("CompanyType");
            }
        }
        #endregion

        #region CompanySize
        private string _companySize;

        [DbColumn("company_size", Length = 64)]
        public string CompanySize {
            get { return _companySize; }
            set {
                if (_companySize == value) return;
                _companySize = value;
                OnPropertyChanged("CompanySize");
            }
        }
        #endregion

        #region CompanyName
        private string _companyName;

        [DbColumn("company_name", Length = 512)]
        public string CompanyName {
            get { return _companyName; }
            set {
                if (_companyName == value) return;
                _companyName = value;
                OnPropertyChanged("CompanyName");
            }
        }
        #endregion

        #region CompanySign
        private string _companySign;

        [DbColumn("company_sign", Length = 64)]
        public string CompanySign {
            get { return _companySign; }
            set {
                if (_companySign == value) return;
                _companySign = value;
                OnPropertyChanged("CompanySign");
            }
        }
        #endregion

        #region RegistrationType
        private string _registrationType;

        [DbColumn("registration_type", Length = 64)]
        public string RegistrationType {
            get { return _registrationType; }
            set {
                if (_registrationType == value) return;
                _registrationType = value;
                OnPropertyChanged("RegistrationType");
            }
        }
        #endregion

        #region LegaLForm
        private string _legalForm;

        [DbColumn("legal_form", Length = 64)]
        public string LegalForm {
            get { return _legalForm; }
            set {
                if (_legalForm == value) return;
                _legalForm = value;
                OnPropertyChanged("LegalForm");
            }

        }
        #endregion

        #region Domicile
        private string _domicile;

        [DbColumn("domicile", Length = 256)]
        public string Domicile {
            get { return _domicile; }
            set {
                if (_domicile == value) return;
                _domicile = value;
                OnPropertyChanged("Domicile");
            }
        }
        #endregion

        #region CompanyRegistrationType
        private string _companyRegistrationType;

        [DbColumn("company_registration_type", Length = 256)]
        public string CompanyRegistrationType {
            get { return _companyRegistrationType; }
            set {
                if (_companyRegistrationType == value) return;
                _companyRegistrationType = value;
                OnPropertyChanged("CompanyRegistrationType");
            }
        }
        #endregion

        #region RegisterationCourt
        private string _registerationCourt;

        [DbColumn("registeration_court", Length = 64)]
        public string RegistrationCourt {
            get { return _registerationCourt; }
            set {
                if (_registerationCourt == value) return;
                _registerationCourt = value;
                OnPropertyChanged("RegistrationCourt");
            }
        }
        #endregion

        #region registrationNumber
        private string _registrationNumber;

        [DbColumn("registration_number", Length = 256)]
        public string RegistraionNumber {
            get { return _registrationNumber; }
            set {
                if (_registrationNumber == value) return;
                _registrationNumber = value;
                OnPropertyChanged("RegistraionNumber");
            }
        }
        #endregion

        #region CompanyId
        private string _companyId;

        [DbColumn("company_id", Length = 256)]
        public string CompanyId {
            get { return _companyId; }
            set {
                if (_companyId == value) return;
                _companyId = value;
                OnPropertyChanged("CompanyId");
            }
        }
        #endregion

        #region ClientId
        private string _clientId;

        [DbColumn("client_id", Length = 64)]
        public string ClientId {
            get { return _clientId; }
            set {
                if (_clientId == value) return;
                _clientId = value;
                OnPropertyChanged("ClientId");
            }
        }
        #endregion

        #region CompanyNameAddress
        private string _companyNameAddress;

        [DbColumn("copmany_name_address", Length = 512)]
        public string CompanyNameAddress {
            get { return _companyNameAddress; }
            set {
                if (_companyNameAddress == value) return;
                _companyNameAddress = value;
                OnPropertyChanged("CompanyNameAddress");
            }
        }
        #endregion

        #region CopmanyDevision
        private string _companyDevision;

        [DbColumn("company_devision", Length = 512)]
        public string CompanyDevision {
            get { return _companyDevision; }
            set {
                if (_companyDevision == value) return;
                _companyDevision = value;
                OnPropertyChanged("CompanyDevision");
            }
        }
        #endregion

        #region CompanyStreet
        private string _companyStreet;

        [DbColumn("company_street", Length = 256)]
        public string CompanyStreet {
            get { return _companyStreet; }
            set {
                if (_companyStreet == value) return;
                _companyStreet = value;
                OnPropertyChanged("CompanyStreet");
            }
        }
        #endregion

        #region CompanyPostcode
        private string _companyPostcode;

        [DbColumn("company_postcode", Length = 64)]
        public string CompanyPostcode {
            get { return _companyPostcode; }
            set {
                if (_companyPostcode == value) return;
                _companyPostcode = value;
                OnPropertyChanged("CompanyPostcode");
            }
        }
        #endregion

        #region CompanyCity
        private string _companyCity;

        [DbColumn("company_city", Length = 256)]
        public string CompanyCity {
            get { return _companyCity; }
            set {
                if (_companyCity == value) return;
                _companyCity = value;
                OnPropertyChanged("CompanyCity");
            }
        }
        #endregion

        #region CompanyState
        private string _companyState;

        [DbColumn("company_State", Length = 256)]
        public string CompanyState {
            get { return _companyState; }
            set {
                if (_companyState == value) return;
                _companyState = value;
                OnPropertyChanged("CompanyState");
            }
        }
        #endregion

        #region Salutation
        private string _salutation;

        [DbColumn("salutation", Length = 64)]
        public string Salutation {
            get { return _salutation; }
            set {
                if (_salutation == value) return;
                _salutation = value;
                OnPropertyChanged("Salutation");
            }
        }
        #endregion

        #region Titel
        private string _title;

        [DbColumn("title", Length = 64)]
        public string Titel {
            get { return _title; }
            set {
                if (_title == value) return;
                _title = value;
                OnPropertyChanged("Titel");
            }
        }
        #endregion

        #region FirstName
        private string _firstName;

        [DbColumn("firstname", Length = 256)]
        public string FirstName {
            get { return _firstName; }
            set {
                if (_firstName == value) return;
                _firstName = value;
                OnPropertyChanged("FirstName");
            }
        }
        #endregion

        #region LastName
        private string _lastName;

        [DbColumn("lastname", Length = 256)]
        public string LastName {
            get { return _lastName; }
            set {
                if (_lastName == value) return;
                _lastName = value;
                OnPropertyChanged("LastName");
            }
        }
        #endregion

        #region Telephone
        private string _telephone;

        [DbColumn("telephone", Length = 64)]
        public string Telephone {
            get { return _telephone; }
            set {
                if (_telephone == value) return;
                _telephone = value;
                OnPropertyChanged("Telephone");
            }
        }
        #endregion

        #region Cell
        private string _cell;

        [DbColumn("cell", Length = 64)]
        public string Cell {
            get { return _cell; }
            set {
                if (_cell == value) return;
                _cell = value;
                OnPropertyChanged("Cell");
            }
        }
        #endregion

        #region Email
        private string _email;

        [DbColumn("email", Length = 256)]
        public string Email {
            get { return _email; }
            set {
                if (_email == value) return;
                _email = value;
                OnPropertyChanged("Email");
            }
        }
        #endregion

        #region DataSent
        private bool _dataSent;

        [DbColumn("data_sent")]
        public bool DataSent {
            get { return _dataSent; }
            set {
                if (_dataSent == value) return;
                _dataSent = value;
                OnPropertyChanged("DataSent");
            }
        }
        #endregion

        #region AccountDeleted
        private bool _accountDeleted;

        [DbColumn("account_deleted")]
        public bool AccountDeleted {
            get { return _accountDeleted; }
            set {
                if (_accountDeleted == value) return;
                _accountDeleted = value;
                OnPropertyChanged("AccountDeleted");
            }
        }
        #endregion

        #region TicketId
        private string _ticketId;

        [DbColumn("ticket_id", Length = 64)]
        public string TicketId {
            get { return _ticketId; }
            set {
                if (_ticketId == value) return;
                _ticketId = value;
                OnPropertyChanged("TicketId");
            }
        }
        #endregion


    }
}