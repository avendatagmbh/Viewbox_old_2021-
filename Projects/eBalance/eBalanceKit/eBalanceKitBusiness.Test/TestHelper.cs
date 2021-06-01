using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using eBalanceKit.Windows.Security.Models;
using eBalanceKitBase.Structures;
using eBalanceKitBusiness.Manager;
using eBalanceKitBusiness.Rights;
using eBalanceKitBusiness.Structures;
using eBalanceKitBusiness.Structures.DbMapping;
using System = eBalanceKitBusiness.Structures.DbMapping.System;
using eBalanceKitBusiness.Structures.ValueTree;
using eBalanceKitBusiness.Structures.DbMapping.ValueMappings;
using eBalanceKitBusiness.Reconciliation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace eBalanceKitBusiness.Test
{
    internal class TestHelper
    {
        #region [ Members ]

        internal const string adminUserName = "admin";
        internal const string adminUserPw = "admin";
        internal const string defaultUserPw = "x";

        #endregion [ Members ]

        #region [ Constructor ]

        static TestHelper() { AppConfig.Init(new ProgressInfo()); }

        #endregion [ Methods ]

        #region [ Methods ]

        public static void Init() { }

        public static void InitCompanyManager() { CompanyManager.Init(true); }

        public static void InitSystem() {
            CompanyManager.Init(true);
            UserManager.Instance.Init();
            SystemManager.Init();
            DocumentManager.Instance.Init();
            RoleManager.Init();
            RightManager.Init();
            RightManager.InitAllowedDetails();
            //CompanyManager.InitAllowedCompanies(RightManager.RightDeducer);
            // for the purpose of handling Role changes
            CtlSecurityManagementModel model = new CtlSecurityManagementModel();
        }

        #region [ Init document ]

        public static void InitDocument(Document document) {
            DocumentManager.Instance.CurrentDocument = document;
            document.ReportRights = new ReportRights(document);
            ValueTree valueTreeGcd = ValueTree.CreateValueTree(typeof(ValuesGCD), document, document.GcdTaxonomy, null);
            ValueTree valueTreeMain = ValueTree.CreateValueTree(typeof(ValuesGAAP), document, document.MainTaxonomy, null);
            ReconciliationManager reconciliationManager = new ReconciliationManager(document);
            PrivateObject privateDocument = new PrivateObject(document);
            privateDocument.SetFieldOrProperty("ReconciliationManager", reconciliationManager);
            privateDocument.SetFieldOrProperty("ValueTreeGcd", valueTreeGcd);
            privateDocument.SetFieldOrProperty("ValueTreeMain", valueTreeMain);
            Type[] paramTypesDoc = new Type[] { };
            Object[] paramObjectsDoc = new Object[] { };
            object retValInvokeDoc = privateDocument.Invoke("InitValueTreeMain", paramTypesDoc, paramObjectsDoc);
            document.LoadDetails(null);
        }

        #endregion [ Init document ]

        public static void Logoff() {
            if (UserManager.Instance.CurrentUser != null) {
                RoleManager.Roles.CollectionChanged -= UserManager.Instance.CurrentUser.OnEditableUsersCollectionChanged;
                RoleManager.Roles.CollectionChanged -= UserManager.Instance.CurrentUser.OnRolesCollectionChanged;
            }
            UserManager.Instance.Logoff();
        }

        public static User LogonAsAdmin() {
            Logoff();
            UserManager.Instance.Init();
            User adminUser = UserManager.Instance.Users.FirstOrDefault(u => string.Compare(u.UserName, adminUserName, StringComparison.InvariantCultureIgnoreCase) == 0);
            if (adminUser == null)
                throw new Exception("User not found: admin");
            UserManager.Instance.SetPassword(adminUser, adminUserPw);
            if (!UserManager.Instance.Logon(adminUser, adminUserPw)) {
                throw new Exception("Login failed for user: admin");
            }
            InitSystem();
            return adminUser;
        }

        public static void LogonAsUser(User user) {
            LogonAsUser(user, defaultUserPw);
        }

        public static void LogonAsUser(User user, string password) {
            Logoff();
            //DEVNOTE: to get the newborn object without the possible customization of the parameter user
            //User userFound = UserManager.Instance.Users.FirstOrDefault(u => string.Compare(u.UserName, adminUserName, StringComparison.InvariantCultureIgnoreCase) == 0);
            //if (userFound == null)
            //    throw new Exception(string.Format("User not found: {0}", user.UserName));
            user.EditableRoles = null;
            user.EditableUsers = null;
            if (!UserManager.Instance.Logon(user, password)) {
                throw new Exception(string.Format("Login failed for user: {0}", user.UserName));
            }
            InitSystem();
        }

        #region [ Object factory methods ]

        public static Structures.DbMapping.System CreateSystem()
        {
            Structures.DbMapping.System system = new Structures.DbMapping.System() { Name = "testSystem_" + Guid.NewGuid().ToString("N") };
            SystemManager.Instance.AddSystem(system);
            system = SystemManager.Instance.Systems.FirstOrDefault(s => s.Name == system.Name);
            if (system == null)
                throw new NullReferenceException();
            return system;
        }

        public static void DeleteSystem(Structures.DbMapping.System system) { SystemManager.Instance.DeleteSystem(system); }

        public static Company CreateCompany() {
            Company company = new Company() {Name = "company_" + Guid.NewGuid().ToString("N"), Owner = UserManager.Instance.CurrentUser};
            company.SetFinancialYearIntervall(2012, 2020);
            company.SetVisibleFinancialYearIntervall(2012, 2020);
            CompanyManager.Instance.AddCompany(company);
            return company;
        }

        public static void DeleteCompany(Company company) { CompanyManager.Instance.DeleteCompany(company); }

        public static void AssignUserToCompany(Company company) {
            CompanyManager.Instance.AllowedCompanies.Add(company);
            for (int i = 0; i < CompanyManager.Instance.NotAllowedCompanies.Count; i++) {
                if (CompanyManager.Instance.NotAllowedCompanies[i].Id == company.Id) {
                    CompanyManager.Instance.NotAllowedCompanies.RemoveAt(i);
                    break;
                }
            }
        }
        
        public static User CreateUser(bool isCompanyAdmin) {
            string userName = "user_" + Guid.NewGuid().ToString("N");
            User user = new User() { UserName = userName, IsCompanyAdmin = isCompanyAdmin};
            UserManager.Instance.SetPassword(user, defaultUserPw);
            UserManager.Instance.AddUser(user);
            return user;
        }

        public static User CreateUser(Company companyAdminOf)
        {
            return CreateUser(companyAdminOf == null ? null : new List<Company>() { companyAdminOf });
        }

        public static User CreateUser(IEnumerable<Company> companiesAdminOf)
        {
            string userName = "user_" + Guid.NewGuid().ToString("N");
            User user = new User() { UserName = userName, IsCompanyAdmin = true };
            UserManager.Instance.SetPassword(user, defaultUserPw);
            UserManager.Instance.AddUser(user);
            if (companiesAdminOf != null) {
                foreach (Company company in companiesAdminOf) {
                    user.AssignedCompanies.Add(new CompanyInfo(company));
                }
            }
            return user;
        }

        public static void DeleteUser(User user) {
            UserManager.Instance.DeleteUser(user);
        }

        public static Document CreateDocument(Company company) {
            Document documentCreated = CreateDocument();
            documentCreated.Company = company;
            documentCreated.CompanyId = company.Id;
            return documentCreated;
        }

        public static Document CreateDocument() {
            Document doc = DocumentManager.Instance.AddDocument();
            if (doc == null)
                throw new NullReferenceException();
            return doc;
        }

        public static void DeleteDocument(Document document) {
            DocumentManager.Instance.DeleteDocument(document);
        }

        public static void DeleteRole(Role role) {
            RoleManager.DeleteRole(role);
        }

        #endregion [ Object factory methods ]

        #endregion [ Methods ]
    }
}
