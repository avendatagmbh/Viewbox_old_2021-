// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-12-28
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using DbAccess;
using Utils;
using eBalanceKitBusiness.FederalGazette.Model;
using eBalanceKitBusiness.Interfaces;
using eBalanceKitBusiness.Logs;
using eBalanceKitBusiness.Manager;
using eBalanceKitBusiness.Rights;
using eBalanceKitBusiness.Structures.DbMapping.Rights;
using eBalanceKitResources.Localisation;

namespace eBalanceKitBusiness.Structures.DbMapping {
    [DbTable("users", Description = "all available users", ForceInnoDb = true)]
    public class User : NotifyPropertyChangedBase, IComparable, ILoggableObject
    {
        #region events
        public event LogHandler NewLog;
        private void OnNewLog(string element, object oldValue, object newValue) { if (NewLog != null) NewLog(this, new LogArgs(element, oldValue, newValue)); }

        public void OnAssignedCompaniesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.AssignedCompaniesAsXml = SerializeToXml(new List<CompanyInfo>(this.AssignedCompanies));
            this._notAllowedCompaniesSet = false;
        }
        public void OnEditableUsersCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.EditableUsers = GetEditableUsers();
        }
        public void OnRolesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.EditableRoles = GetEditableRoles();
        }
        
        #endregion events

        #region properties

        #region Id
        [DbColumn("id", AllowDbNull = false)]
        [DbPrimaryKey]
        public int Id { get; set; }
        #endregion Id

        #region UserName
        private string _userName;

        [DbColumn("username", Length = 128)]
        public string UserName {
            get { return _userName; }
            set {
                if (_userName == value) return;
                
                // log changed value
                OnNewLog("UserName", _userName, value);

                _userName = StringUtils.Left(value, 128);
                OnPropertyChanged("UserName");
                OnPropertyChanged("DisplayString");
            }
        }
        #endregion

        #region FullName
        private string _fullName;

        [DbColumn("fullname", AllowDbNull = true, Length = 256)]
        public string FullName {
            get { return _fullName; }
            set {
                if (_fullName == value) return;

                // log changed value
                OnNewLog("FullName", _fullName, value);
                    
                _fullName = StringUtils.Left(value, 256);
                OnPropertyChanged("FullName");
                OnPropertyChanged("DisplayString");
            }
        }
        #endregion

        #region Salt
        [DbColumn("salt", AllowDbNull = true, Length = 64)]
        public string Salt { get; set; }
        #endregion

        #region PasswordHash
        private string _passwordHash;

        [DbColumn("password", AllowDbNull = false, Length = 1024)]
        public string PasswordHash {
            get { return _passwordHash; }
            set {
                if (_passwordHash == value) return;
                
                // log changed value
                OnNewLog("PasswordHash", _passwordHash, value);
                
                _passwordHash = value;
            }
        }
        #endregion

        #region IsActive
        private bool _isActive = true;

        [DbColumn("is_active")]
        public bool IsActive {
            get { return _isActive; }
            set {
                // discard changes, if curent user has no admin rights
                if (UserManager.Instance.CurrentUser != null && !UserManager.Instance.CurrentUser.IsAdmin) return;

                // discard changes, if curent user tries to change it's own rights
                if (UserManager.Instance.CurrentUser != null && UserManager.Instance.CurrentUser == this) return;

                if (_isActive == value) return;
                
                // log changed active state
                OnNewLog("IsActive", _isActive, value);
                
                _isActive = value;
                OnPropertyChanged("IsActive");
            }
        }
        #endregion

        #region IsAdmin
        private bool _isAdmin;

        [DbColumn("is_admin")]
        public bool IsAdmin {
            get { return _isAdmin; }
            set {
                // discard changes, if curent user has no admin rights
                if (UserManager.Instance.CurrentUser != null && !UserManager.Instance.CurrentUser.IsAdmin) return;

                // discard changes, if curent user tries to change it's own rights
                if (UserManager.Instance.CurrentUser != null && UserManager.Instance.CurrentUser == this) return;

                if (_isAdmin == value) return;
                
                // log changed admin state
                OnNewLog("IsAdmin", _isAdmin, value);
                    
                _isAdmin = value;
                OnPropertyChanged("IsAdmin");
            }
        }
        #endregion

        #region LastLogin
        [DbColumn("last_login", AllowDbNull = true)]
        public DateTime? LastLogin { get; set; }
        #endregion

        #region IsCompanyAdmin
        private bool _isCompanyAdmin;

        [DbColumn("is_companyadmin")]
        public bool IsCompanyAdmin
        {
            get { return _isCompanyAdmin; }
            set
            {
                // discard changes, if curent user has no admin rights
                if (UserManager.Instance.CurrentUser != null && !UserManager.Instance.CurrentUser.IsAdmin) return;

                // discard changes, if curent user tries to change it's own rights
                if (UserManager.Instance.CurrentUser != null && UserManager.Instance.CurrentUser == this) return;

                if (_isCompanyAdmin == value) return;

                // log changed admin state
                OnNewLog("IsCompanyAdmin", _isCompanyAdmin, value);

                _isCompanyAdmin = value;
                OnPropertyChanged("IsCompanyAdmin");
            }
        }
        #endregion

        #region NotAllowedCompanies

        private bool _notAllowedCompaniesSet = false;
        private ObservableCollection<Company> _notAllowedCompanies = new ObservableCollection<Company>();
        public ObservableCollection<Company> NotAllowedCompanies {
            get {
                if (!_notAllowedCompaniesSet && CompanyManager.Instance.Companies.Count() > 0) {
                    if (this.AllowUserManagement) {
                        //_notAllowedCompanies = new ObservableCollection<Company>(CompanyManager.Companies.Where(c => this.IsCompanyAdmin && !this.AssignedCompanies.Any(ac => ac.Id == c.Id)));
                        if (this.IsAdmin)
                            _notAllowedCompanies = new ObservableCollection<Company>();
                        else
                            _notAllowedCompanies = new ObservableCollection<Company>(CompanyManager.Instance.Companies.Where(c => !this.AssignedCompanies.Any(ac => ac.Id == c.Id)));
                    } else
                        _notAllowedCompanies = CompanyManager.Instance.NotAllowedCompanies;
                    _notAllowedCompaniesSet = true;
                }
                return _notAllowedCompanies;
            }
        }

        #endregion NotAllowedCompanies

        #region AssignedCompanies
        private bool _isAssignedCompaniesSet = false;
        private ObservableCollection<CompanyInfo> _assignedCompanies = new ObservableCollection<CompanyInfo>();

        public ObservableCollection<CompanyInfo> AssignedCompanies {
            get {
                // Companies may not initialized, check whether they exists
                if (!_isAssignedCompaniesSet && CompanyManager.Instance.Companies.Count() > 0) {
                    CompanyInfoSerializer serializer = new CompanyInfoSerializer();
                    if (!string.IsNullOrEmpty(this.AssignedCompaniesAsXml)) {
                        using (StringReader sReader = new StringReader(this.AssignedCompaniesAsXml)) {
                            using (XmlReader xmlReader = XmlReader.Create(sReader)) {
                                serializer.ReadXml(xmlReader);
                                _assignedCompanies = new ObservableCollection<CompanyInfo>(serializer.Companies);
                            }
                        }
                    }
                    _isAssignedCompaniesSet = true;
                    this._assignedCompanies.CollectionChanged += OnAssignedCompaniesCollectionChanged;
                }
                return _assignedCompanies;
            }
            set {
                ObservableCollection<CompanyInfo> companies = value ?? new ObservableCollection<CompanyInfo>();
                this.AssignedCompaniesAsXml = SerializeToXml(new List<CompanyInfo>(companies));
                this._assignedCompanies = companies;
                OnPropertyChanged("AssignedCompanies");
            }
        }

        private string _assignedCompaniesAsXml = null;

        [DbColumn("assigned_companies", AllowDbNull = true, Length = 1024)]
        internal string AssignedCompaniesAsXml {
            get { return this._assignedCompaniesAsXml; }
            set {
                OnNewLog("AssignedCompanies", _assignedCompaniesAsXml, value);
                this._assignedCompaniesAsXml = value;
            }
        }

        #endregion 
        
        #region IsDeleted
        [DbColumn("is_deleted")]
        public bool IsDeleted { get; set; }
        #endregion

        #region IsDomainUser
        private bool _isDomainUser;

        [DbColumn("is_domain_user")]
        public bool IsDomainUser {
            get { return _isDomainUser; }
            set {
                // discard changes, if curent user has no admin rights
                if (UserManager.Instance.CurrentUser != null && !UserManager.Instance.CurrentUser.AllowUserManagement) return;

                // discard changes, if curent user tries to change it's own rights
                if (UserManager.Instance.CurrentUser != null && UserManager.Instance.CurrentUser == this) return;

                if (_isDomainUser == value) return;
                
                _isDomainUser = value;
                OnPropertyChanged("IsDomainUser");
            }
        }
        #endregion IsDomainUser

        #region DomainName
        private string _domain;

        [DbColumn("domain")]
        public string DomainName {
            get { return _domain; }
            set {
                // discard changes, if curent user has no admin rights
                if (UserManager.Instance.CurrentUser != null && !UserManager.Instance.CurrentUser.IsAdmin)
                    return;

                // discard changes, if curent user tries to change it's own rights
                if (UserManager.Instance.CurrentUser != null && UserManager.Instance.CurrentUser == this)
                    return;

                if (_domain == value)
                    return;

                _domain = value;
                OnPropertyChanged("DomainName");
            }
        }
        #endregion IsDomainUser

        #region AllowUserManagement
        /// <summary>
        /// Returns true, if this user is allowed to see the security config area.
        /// </summary>
        public bool AllowUserManagement { get { return IsAdmin || (IsCompanyAdmin && AssignedCompanies.Count > 0); } }
        #endregion AllowUserManagement

        #region DisplayString
        public string DisplayString {
            get {
                string result = UserName;
                if (string.IsNullOrEmpty(result)) return "<" + ResourcesCommon.EmptyName + ">";
                if (result.Length == 0) {
                    result = FullName;
                    if (string.IsNullOrEmpty(result)) return "<" + ResourcesCommon.EmptyName + ">";
                } else {
                    if (!string.IsNullOrEmpty(FullName)) result += " (" + FullName + ")";
                }
                return result;
            }
        }
        #endregion

        #region IsSelected
        private bool _isSelected;

        public bool IsSelected {
            get { return _isSelected; }
            set {
                _isSelected = value;
                OnPropertyChanged("IsSelected");
            }
        }
        #endregion

        #region AssignedRoles
        private readonly ObservableCollection<Role> _assignedRoles = new ObservableCollection<Role>();
        public ObservableCollection<Role> AssignedRoles { get { return _assignedRoles; } }
        #endregion AssignedRoles

        #region AutoLogin
        private bool _autoLogin;
        /// <summary>
        /// Requiered for check if the User activated auto login.
        /// The user has to login once before he can use the auto login.
        /// </summary>
        public bool AutoLogin {
            get { return _autoLogin; }
            set {
                if (_autoLogin != value) {
                    _autoLogin = value;
                    OnPropertyChanged("AutoLogin");
                }
            }
        }
        #endregion AutoLogin

        #region Editable users

        private ObservableCollection<User> _editableUsers; 
        public ObservableCollection<User> EditableUsers {
            get {
                if (_editableUsers == null) {
                    //DEVNOTE: called from CtlSecurityManagementModel
                    //RoleManager.Roles.CollectionChanged += OnEditableUsersCollectionChanged;
                    _editableUsers = GetEditableUsers();
                }
                return _editableUsers;
            }
            set {
                _editableUsers = value;
                OnPropertyChanged("EditableUsers");
            }
        }

        #endregion Editable users

        #region Editable roles

        private ObservableCollection<Role> _editableRoles;

        public ObservableCollection<Role> EditableRoles {
            get {
                if (_editableRoles == null) {
                    //DEVNOTE: called from CtlSecurityManagementModel
                    //RoleManager.Roles.CollectionChanged += OnRolesCollectionChanged;
                    _editableRoles = GetEditableRoles();
                }
                return _editableRoles;
            }
            set {
                _editableRoles = value;
                OnPropertyChanged("EditableRoles");
            }
        }

        #endregion Editable roles

        #region Options
        private Options.IOptions _options;

        public Options.IOptions Options {
            //get { return eBalanceKitBusiness.Options.GlobalUserOptions.UserOptions; }
            get {
                if (_options == null) {
                    eBalanceKitBusiness.Options.GlobalUserOptions.Load(this);
                }
                return _options;
            }
            set {
                _options = value;
            }
        }

        #endregion Options

        public UpgradeDataContext UpgradeDataContext { get; set; }

        #endregion properties

        #region methods

        #region IsCompanyAssigned

        public bool IsCompanyAssigned(Company company)
        {
            return AssignedCompanies.Any(c => c.Id == company.Id);
        }

        #endregion IsCompanyAssigned

        #region CompareTo
        public int CompareTo(object obj) {
            if (!(obj is User)) return 0;
            if (string.IsNullOrEmpty(UserName) || string.IsNullOrEmpty(((User) obj).UserName)) return 0;
            return UserName.CompareTo(((User) obj).UserName);
        }
        #endregion

        #region [ Serialize to XML ]

        internal string SerializeToXml(List<CompanyInfo> companies)
        {
            string serializedValue;
            CompanyInfoSerializer serializer = new CompanyInfoSerializer(companies);
            using (StringWriter sWriter = new StringWriter())
            {
                using (XmlWriter xWriter = XmlWriter.Create(sWriter))
                {
                    serializer.WriteXml(xWriter);
                }
                serializedValue = sWriter.ToString();
            }
            return serializedValue;
        }

        #endregion

        #region Editable users

        public void RefreshEditableUserCollection() {
            _editableUsers = GetEditableUsers();
            OnPropertyChanged("EditableUsers");
        }

        private ObservableCollection<User> GetEditableUsers () {
            if (!this.AllowUserManagement)
                return new ObservableCollection<User>();
            if (this.IsAdmin)
                return UserManager.Instance.EditableUsers;

            // user is company administrator
            ObservableCollection<User> editableUsers = new ObservableCollection<User>();
            
            foreach (User editableUser in UserManager.Instance.EditableUsers) {
                // company admin should not see admin
                if (editableUser.IsAdmin)
                    continue;

                bool revoked = false;
                RightDeducer rightDeducerForUser = new RightDeducer(editableUser);
                // check not allowed companies, if one of these companies is visible for the user, then do not show this user
                //foreach (Company company in CompanyManager.NotAllowedCompanies) {
                foreach (Company company in this.NotAllowedCompanies) {
                    if (IsCompanyVisibleForUser(rightDeducerForUser, company, editableUser)) {
                        revoked = true;
                        break;
                    }
                }
                if (revoked) continue;

                // check assigned roles if there is any right to an object in one of the not allowed companies
                foreach (Role assignedRole in editableUser.AssignedRoles) {
                    revoked = IsRightRevoked(assignedRole);
                    if (revoked) break;
                }
                
                if (!revoked) {
                    editableUsers.Add(editableUser);
                }
            }
            return editableUsers;
        }

        private bool IsCompanyVisibleForUser(RightDeducer rightDeducer, Company company, User user) { return rightDeducer.CompanyVisibleForUser(company, user); }
        
        #endregion Editable users

        #region Editable roles

        public void RefreshEditableRolesCollection() {
            _editableRoles = GetEditableRoles();
            OnPropertyChanged("EditableRoles");
        }

        private ObservableCollection<Role> GetEditableRoles() {
            if (!this.AllowUserManagement)
                return new ObservableCollection<Role>();
            if (this.IsAdmin)
                return RoleManager.Roles;

            // user is company administrator
            ObservableCollection<Role> editableRoles = new ObservableCollection<Role>();
            foreach (Role role in RoleManager.Roles) {

                role.LoadDetails();
                bool revoked = false;

                // check if any of assigned users has rights in one of the not allowed companies, then do not show this role
                foreach (User user in role.AssignedUsers) {
                    RightDeducer rightDeducerForUser = new RightDeducer(user);
                    foreach (Company company in this.NotAllowedCompanies) {
                        if (IsCompanyVisibleForUser(rightDeducerForUser, company, user)) {
                            revoked = true;
                            break;
                        }
                    }
                }
                if (revoked) continue;

                revoked = IsRightRevoked(role);

                if (!revoked) {
                    editableRoles.Add(role);
                }
            }
            return editableRoles;
        }

        /// <summary>
        /// Returns true if the current role has rights for an object in one of the not allowed companies
        /// </summary>
        /// <param name="role"></param>
        /// <returns></returns>
        private bool IsRightRevoked(Role role) {

            bool revoked = false;
            // check the rights to objects, if has rights to an object under a not allowed company, then do not show this role
            foreach (Company company in this.NotAllowedCompanies) {
                Right right = role.GetRightIfPresent(company);
                if (IsRightRevoked(right)) {
                    revoked = true;
                    break;
                }
                foreach (FinancialYear financialYear in company.FinancialYears) {
                    Right rightFY = role.GetRightIfPresent(financialYear);
                    if (IsRightRevoked(rightFY)) {
                        revoked = true;
                        break;
                    }
                }
                foreach (
                    Document document in
                        DocumentManager.Instance.Documents.Where(d => this.NotAllowedCompanies.Any(c => c.Id == d.CompanyId))) {
                    Right documentRight = role.GetRightIfPresent(document);
                    if (IsRightRevoked(documentRight) || HasAnyRight(role, document)) {
                        revoked = true;
                        break;
                    }
                }
                if (revoked) break;
            }
            return revoked;
        }

        private bool HasAnyRight(Role role, Document document) {
            Right rRest = role.GetRightIfPresent(DbRight.ContentTypes.DocumentSpecialRight_Rest, document);
            if (IsRightRevoked(rRest)) return true;
            
            Right rTransfer = role.GetRightIfPresent(DbRight.ContentTypes.DocumentSpecialRight_TransferValueCalculation, document);
            if (IsRightRevoked(rTransfer)) return true;
            
            // do not check this right, because by default all user gets right for financial years
            //Right rFY = role.GetRightIfPresent(DbRight.ContentTypes.FinancialYear, document);
            //if (IsRightRevoked(rFY)) return true;
            
            Right rDocument = role.GetRightIfPresent(DbRight.ContentTypes.Document, document);
            if (IsRightRevoked(rDocument)) return true;
            
            Right rCompany = role.GetRightIfPresent(DbRight.ContentTypes.Company, document);
            if (IsRightRevoked(rCompany)) return true;
            
            Right rAll = role.GetRightIfPresent(DbRight.ContentTypes.All, document);
            if (IsRightRevoked(rAll)) return true;
            
            Right rSystem = role.GetRightIfPresent(DbRight.ContentTypes.System, document);
            if (IsRightRevoked(rSystem)) return true;

            return false;
        }
        
        /// <summary>
        /// Returns true if any rights in right is allowed
        /// </summary>
        /// <param name="right"></param>
        /// <returns></returns>
        private bool IsRightRevoked(Right right) {
            if (right != null && right.IsAnyRightAllowed()) {
                return true;
            }
            return false;
        }

        #endregion Editable roles

        #endregion methods
    }

    
}