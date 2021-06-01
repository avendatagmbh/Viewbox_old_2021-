// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2010-09-28
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using DbAccess;
using DbAccess.Structures;
using eBalanceKitBusiness.AuditCorrections.DbMapping;
using eBalanceKitBusiness.HyperCubes.DbMapping;
using eBalanceKitBusiness.Logs;
using eBalanceKitBusiness.Logs.DbMapping;
using eBalanceKitBusiness.Manager;
using eBalanceKitBusiness.Structures.DbMapping;
using eBalanceKitBusiness.Structures.DbMapping.GlobalSearch;
using eBalanceKitBusiness.Structures.DbMapping.MappingTemplate;
using eBalanceKitBusiness.Structures.DbMapping.ValueMappings;
using eBalanceKitBusiness.Structures.XbrlElementValue;
using eBalanceKitBusiness.Structures.DbMapping.BalanceList;
using eBalanceKitBase.Structures;
using eBalanceKitBase;
using eBalanceKitBusiness.Exceptions;
using eBalanceKitBusiness.Structures.DbMapping.Rights;
using eBalanceKitBusiness.Structures.DbMapping.Templates;

namespace eBalanceKitBusiness.Structures {

    /// <summary>
    /// Application config class.
    /// </summary>
    public static class AppConfig {

        #region constructor
        static AppConfig() {
            List<Language> languages = new List<Language> {
                new Language("de-DE", "Deutsch", "German"),
                new Language("en-EN", "Englisch", "English")
            };
            Languages = languages;

            // set last language if exist
            if (!string.IsNullOrEmpty(UserConfig.LastLanguage)) {
                switch (UserConfig.LastLanguage) {
                    case "en":
                        SelectedLanguage = languages[1];
                        break;

                    default:
                        SelectedLanguage = languages[0];
                        break;
                }
            } else {
                switch (Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName) {
                    case "en":
                        SelectedLanguage = languages[1];
                        break;

                    default:
                        SelectedLanguage = languages[0];
                        break;
                }
            }
            
            DatabaseConfig = new DatabaseConfig();

            //AppCultureInfo = CultureInfo.CreateSpecificCulture("de-DE");
            //AppCultureInfo.NumberFormat.NegativeSign = "-";
            AppCultureInfo = CultureInfo.InvariantCulture;
            AppUiCulture = CultureInfo.CreateSpecificCulture("de-DE");

            Thread.CurrentThread.CurrentCulture = AppCultureInfo;
            Thread.CurrentThread.CurrentUICulture = AppUiCulture;
        }
        #endregion

        #region events
        public static event ErrorEventHandler Error;
        private static void OnError(Exception ex) { if (Error != null) Error(null, new ErrorEventArgs(ex)); }

        public static event EventHandler InitFinished;
        private static void OnInitFinished() { if (InitFinished != null) InitFinished(null, new System.EventArgs()); }
        #endregion

        #region properties

        public static bool IsInitialized { get; private set; }
        public static ConnectionManager ConnectionManager { get; private set; }
        public static ProxyConfig ProxyConfig { get { return DatabaseConfig.ProxyConfig; } }
        public static DatabaseConfig DatabaseConfig { get; set; }
        public static CultureInfo AppCultureInfo { get; set; }
        public static CultureInfo AppUiCulture { get; set; }

        #region Languages
        private static Language _selectedLanguage;

        public static Language SelectedLanguage {
            get { return _selectedLanguage; }
            set {
                if (_selectedLanguage == value) return;
                _selectedLanguage = value;

                Thread.CurrentThread.CurrentUICulture =
                    CultureInfo.CreateSpecificCulture(value.Culture.Name);

                UserConfig.LastLanguage = value.Culture.TwoLetterISOLanguageName;
                //DEVNOTE: config will be saved on exit from application
                //UserConfig.Save();
            }
        }

        public static IEnumerable<Language> Languages { get; private set; }
        #endregion // Languages

        #endregion // properties

        #region methods

        /// <summary>
        /// Inits this instance.
        /// </summary>
        public static void Init(ProgressInfo progress) {
            
            DbMappingBase.CheckForceInnoDbFlag = true;
            
            progress.Caption = eBalanceKitResources.Localisation.ResourcesCommon.Initialization;

            EricWrapper.Eric.Init();

            if (!DatabaseConfig.ExistsConfigfile) throw new FileNotFoundException(Localisation.ExceptionMessages.ConfigFileNotFound);

            try {
                Dispose();
                DatabaseConfig.LoadConfig();
                EricWrapper.Eric.SetProxy(DatabaseConfig.ProxyConfig.Host, DatabaseConfig.ProxyConfig.Port,
                    DatabaseConfig.ProxyConfig.Username, DatabaseConfig.ProxyConfig.Password);

                //For MySQL and SQLServer we need to create the database if it does not exist
                if (DatabaseConfig.DbConfig.DbType == "MySQL" || DatabaseConfig.DbConfig.DbType == "SQLServer" || DatabaseConfig.DbConfig.DbType=="Oracle") {
                    DbConfig dbConfig = new DbAccess.Structures.DbConfig(DatabaseConfig.DbConfig.DbType) {
                        Username = DatabaseConfig.DbConfig.Username,
                        Password = DatabaseConfig.DbConfig.Password,
                        Hostname = DatabaseConfig.DbConfig.Hostname,
                        SID= DatabaseConfig.DbConfig.SID
                    };
                    

                    // create database if it does not yet exist
                    using (IDatabase conn = ConnectionManager.CreateConnection(dbConfig)) {
                        conn.Open();
                        conn.CreateDatabaseIfNotExists(DatabaseConfig.DbConfig.DbName);
                    }
                }
            } catch (Exception ex) {
                throw new Exception(Localisation.ExceptionMessages.ReadAppConfigFailed + ":" + Environment.NewLine + ex.Message, ex);
            }

            try {
                progress.Caption = eBalanceKitResources.Localisation.ResourcesCommon.InitializingDatabase;
                InitConnectionManager(DatabaseConfig.DbConfig);

                InitDatabase(progress);

                // load taxonomies in background thread
                TaxonomyManager.Error += Error;
                TaxonomyManager.InitTaxonomyInfos();

                InitManager();

            } catch (DatabaseOutOfDateException) {
                throw;
            } catch (Exception ex) {
                throw new Exception(ex.Message, ex);
            }

            new Thread(LogManager.Instance.LogToDatabase) {
                CurrentCulture = Thread.CurrentThread.CurrentCulture,
                CurrentUICulture = Thread.CurrentThread.CurrentUICulture
            }.Start(1000);

            IsInitialized = true;
            OnInitFinished();
        }

        public static void InitConnectionManager(DbConfig dbConfig) {
            if (ConnectionManager != null) ConnectionManager.Dispose();
            ConnectionManager = new ConnectionManager(dbConfig, 12);
            ConnectionManager.Init();
        }

        #region init database methods

        #region InitDatabase
        private static void InitDatabase(ProgressInfo progress) {
            progress.Caption = eBalanceKitResources.Localisation.ResourcesCommon.InitializingDatabase;
            while (!AppConfig.ConnectionManager.IsInitialized) Thread.Sleep(10);

            string curDbVersion = Info.DbVerion;
            if (VersionInfo.Instance.NewerDbVersionExists(curDbVersion))
                throw new DatabaseOutOfDateException(
                    Localisation.ExceptionMessages.DatabaseOutOfDate + Environment.NewLine + Environment.NewLine +
                    string.Format(Localisation.ExceptionMessages.DatabaseOutOfDate1, curDbVersion,
                                  VersionInfo.Instance.CurrentDbVersion));

            if (VersionInfo.Instance.ProgramVersionToOld(curDbVersion))
                // TODO: add the new window.
                throw new ProgramOutOfDateException(string.Format(Localisation.ExceptionMessages.ProgramOutOfDate,
                                                                  curDbVersion));
            CreateTables();
        }

        static void Upgrader_Finished(object sender, System.EventArgs e) {
            OnInitFinished();
        }
        #endregion

        #region CreateTables
        internal static void CreateTables() {
            using (IDatabase conn = AppConfig.ConnectionManager.GetConnection()) {
                CreateTables(conn);
            }
        }

        internal static string TableSqlCreationCode(IDatabase conn) {
            var allTypes = System.Reflection.Assembly.GetExecutingAssembly().GetTypes();

            string result = "List<string> tableCreation = new List<string> {";
            foreach (var type in allTypes) {
                foreach (var attribute in System.Attribute.GetCustomAttributes(type, true)) {
                    if (attribute is DbTableAttribute) {
                        result += "\"" + conn.DbMapping.GetCreateTableSqlStatement(type) + "\"," + Environment.NewLine;
                    }
                }
            }
            result.Remove(result.Length - 1);
            result += "};";
            return result;
        }

        internal static string SqlCreateIndices(IDatabase conn) {
            var allTypes = System.Reflection.Assembly.GetExecutingAssembly().GetTypes();

            string result = "#region indices" + Environment.NewLine + "IndexInformation ";
            foreach (var type in allTypes) {
                foreach (var attribute in System.Attribute.GetCustomAttributes(type, true)) {
                    if (attribute is DbTableAttribute) {
                        string index = conn.DbMapping.GetIndicesValuePair(type);
                        if (index != "")
                            result += index + Environment.NewLine;
                    }
                }
            }
            result += "#endregion" + Environment.NewLine;
            return result;
        }

        internal static void CreateTables(IDatabase conn) {
            //Execute this code in order to get the create tables statements for the DatabaseCreater
            #if DEBUG
            string result = TableSqlCreationCode(conn);
            string result2 = SqlCreateIndices(conn);
            #endif
            CreateTable<Info>(conn);

            CreateTable<TaxonomyIdAssignment>(conn);

            CreateTable<DbMapping.System>(conn);
            CreateTable<Company>(conn);
            CreateTable<FinancialYear>(conn);
            CreateTable<BalanceList>(conn);
            CreateTable<Document>(conn);

            CreateTable<Account>(conn);
            CreateTable<AccountGroup>(conn);
            CreateTable<SplitAccountGroup>(conn);
            CreateTable<SplittedAccount>(conn);

            //  templates
            CreateTable<MappingTemplateHead>(conn);
            CreateTable<MappingTemplateLine>(conn);
            CreateTable<MappingTemplateElementInfo>(conn);
            CreateTable<BalListTemplate>(conn);

            CreateTable<UpgradeInformation>(conn);

            CreateTable<User>(conn);
            CreateTable<Company>(conn);

            // value tables
            CreateTable<ValuesGAAP>(conn);
            CreateTable<ValuesGCD>(conn);
            CreateTable<ValuesGCD_Company>(conn);

            // transfer HBST (Überleitungsrechnung Handelsbilanz --> Steuerbilanz)
            CreateTable<Reconciliation.DbMapping.DbEntityReconciliation>(conn);
            CreateTable<Reconciliation.DbMapping.DbEntityReconciliationTransaction>(conn);

            // table for reference list
            CreateTable<Reconciliation.DbMapping.DbEntityReferenceList>(conn);
            CreateTable<Reconciliation.DbMapping.DbEntityReferenceListItem>(conn);
            CreateTable<DbMapping.BalanceList.DbEntityAccountReferenceList>(conn);
            CreateTable<DbMapping.BalanceList.DbEntityAccountReferenceListItem>(conn);

            //Logs
            CreateTable<DbSendLog>(conn);
            CreateTable<DbAdminLog>(conn);

            //Rights
            CreateTable<DbRole>(conn);
            CreateTable<DbRight>(conn);
            CreateTable<DbUserRole>(conn);

            // hyper cubes
            CreateTable<DbEntityHyperCube>(conn);
            CreateTable<DbEntityHyperCubeItem>(conn);
            CreateTable<DbEntityHyperCubeDimensionOrdinal>(conn);
            CreateTable<DbEntityHyperCubeDimension>(conn);
            CreateTable<DbEntityHyperCubeDimensionKey>(conn);
            CreateTable<DbEntityHyperCubeImport>(conn);

            // other
            CreateTable<TaxonomyInfo>(conn);
            CreateTable<GlobalSearchHistoryItem>(conn);

            //probably not necessary to save data due to redundancy
            //CreateTable<FederalGazetteInfo>(conn); 
            
            //federal gazette
            CreateTable<ValuesGAAPFG>(conn);

            //federal gazette
            CreateTable<ReportFederalGazette>(conn);

            CreateTable<AccountsInformationProfile>(conn);

            if (conn.CountTable(conn.DbMapping.GetTableName(typeof(TaxonomyInfo))) == 0) {                
                TaxonomyManager.InitTaxonomyTable(conn);
            }

            CreateTable<VirtualAccount>(conn);
            CreateTable<Options.AvailableOptions>(conn);

            // audit corrections
            CreateTable<DbEntityAuditCorrection>(conn);
            CreateTable<DbEntityAuditCorrectionTransaction>(conn);
            CreateTable<DbEntityAuditCorrectionSetEntry>(conn);

        }
        #endregion

        #region CreateTable
        internal static void CreateTable<T>(IDatabase conn) {
            try {
                conn.DbMapping.CreateTableIfNotExists<T>();
            } catch (Exception ex) {
                throw new Exception(
                    string.Format(
                        Localisation.ExceptionMessages.CreateTableFailed,
                        conn.DbMapping.GetTableName<T>()) +
                        ":" + Environment.NewLine + ex.Message, ex);
            }
        }
        #endregion

        #region InitManager
        private static void InitManager() {
            Info.Init();
            UserManager.Instance.Init();
        }
        #endregion

        #endregion

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        public static void Dispose() {
            if (ConnectionManager == null) return;
            LogManager.Instance.Dispose = true;
            ConnectionManager.Dispose();
            ConnectionManager = null;
        }

        public static void IsRegistered() {

        }


        /// <summary>
        /// Saves this instance to the config file.
        /// </summary>
        public static void Save() {

            // connection manager initialized?
            if (ConnectionManager == null) return;
            DatabaseConfig.Save();
        }

        public static void DisableDbUpdates() { XbrlElementValueBase.DoDbUpdate = false; }
        public static void EnableDbUpdates() { XbrlElementValueBase.DoDbUpdate = true; }

        public static string GetSupportMailHRef() {
            const string br = "%0A";
            var version =
                System.Reflection.Assembly.GetEntryAssembly().GetName().Version.Major + "." +
                System.Reflection.Assembly.GetEntryAssembly().GetName().Version.Minor +
                (System.Reflection.Assembly.GetEntryAssembly().GetName().Version.Build > 0 ? "." + System.Reflection.Assembly.GetEntryAssembly().GetName().Version.Build : "");

            var mailtoBody = "Programmversion: " + version + br;
            mailtoBody += "Betriebssystem: " + Utils.OSHelper.GetOSName() + " (" + System.Environment.OSVersion.VersionString + ")" + br;

            switch (Utils.OSHelper.GetPlatform()) {
                case Utils.OSHelper.Platform.X86: 
                    mailtoBody += "Architektur: " + "32 Bit";
                    break;
                case Utils.OSHelper.Platform.X64:
                    mailtoBody += "Architektur: " + "64 Bit";
                    break;
                case Utils.OSHelper.Platform.Unknown:
                    mailtoBody += "Architektur: " + "Unbekannt";
                    break;
                default:
                    break;
            }
            mailtoBody += br;            

            using (var conn = ConnectionManager.GetConnection()) {
                mailtoBody += "Datenbank: ";
                switch (conn.DbConfig.DbType) {
                    case "SQLServer":
                        mailtoBody += "SQL-Server";
                        break;

                    default:
                        mailtoBody += conn.DbConfig.DbType;
                        break;
                }
                mailtoBody += " (Version " + ((DbConnection)conn.Connection).ServerVersion + ")" + br;
            }

            mailtoBody += br + "Seriennummer:" + Info.Serial + br + br;

            mailtoBody += "Firmenname:" + br;
            mailtoBody += "Ansprechpartner:" + br;
            mailtoBody += "Telefon:" + br;
            mailtoBody += "E-Mail:" + br;

            mailtoBody += br + "Problembeschreibung:" + br;

            return "mailto:" + CustomResources.CustomStrings.SupportMail + "?subject=" + CustomResources.CustomStrings.ProductName + " Support&body=" + mailtoBody;
        }

        public static string GetRegistrationMailHRef() {
            const string br = "%0A";
            var version =
                System.Reflection.Assembly.GetEntryAssembly().GetName().Version.Major + "." +
                System.Reflection.Assembly.GetEntryAssembly().GetName().Version.Minor +
                (System.Reflection.Assembly.GetEntryAssembly().GetName().Version.Build > 0 ? "." + System.Reflection.Assembly.GetEntryAssembly().GetName().Version.Build : "");

            string mailtoBody = "Programmversion: " + version + br;
            mailtoBody += "Seriennummer: " + br;
            mailtoBody += "Firmenname: " + br + br;
            mailtoBody += "Ansprechpartner:" + br;
            mailtoBody += "Telefon:" + br;
            mailtoBody += "E-Mail:" + br;

            return "mailto:" + CustomResources.CustomStrings.RegistrationMail + "?subject=eBilanz-Kit Registrierung&body=" + mailtoBody;
        }

        #endregion methods
    }
}