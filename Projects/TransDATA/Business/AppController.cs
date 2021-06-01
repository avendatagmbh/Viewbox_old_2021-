// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2011-08-27
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using AV.Log;
using Base.EventArgs;
using Business.Structures;
using Business.Structures.DataTransferAgents;
using Config;
using Config.DbStructure;
using Config.Enums;
using Config.Interfaces.Config;
using Config.Interfaces.DbStructure;
using Config.Manager;
using DbAccess;
using GenericOdbc;
using Utils;
using Business.Interfaces;
using Business.Structures.DateReaders;
using log4net;

namespace Business {
    /// <summary>
    /// Application controler for TransDATA professional.
    /// </summary>
    public static class AppController {

        static AppController() {
            UserConfig.Error += UserConfigError;

            Languages = new List<Language> {
                new Language("de-DE", "Deutsch", "German"),
                new Language("en-EN", "Englisch", "English")
            };
            SelectedLanguage = Languages.First();
        }

        #region events
        public static event EventHandler<MessageEventArgs> Error;

        private static void OnError(string message) { if (Error != null) Error(null, new MessageEventArgs(message)); }
        #endregion events

        #region event handler
        private static void UserConfigError(object sender, MessageEventArgs e) { if (Error != null) Error(null, e); }
        #endregion event handler

        #region properties
        public static UserConfig UserConfig { get; private set; }
        public static bool IsInitialized { get; private set; }

        public static UserManager UserManager { get { return ConfigDb.UserManager; } }

        public static ProfileManager ProfileManager { get { return ConfigDb.ProfileManager; } }

        public static IEnumerable<IDbTemplate> DbTemplates { get { return DbTemplateManager.GetTemplates(); } }

        #region Languages
        private static Language _selectedLanguage;

        public static Language SelectedLanguage {
            get { return _selectedLanguage; }
            set {
                if (_selectedLanguage == value) return;
                _selectedLanguage = value;

                Thread.CurrentThread.CurrentUICulture = CultureInfo.CreateSpecificCulture(value.Culture.Name);

                LogHelper.GetLogger().ContextLog(LogLevelEnum.Debug, "Selected language: {0}", value.Culture.Name);
            }
        }

        public static IEnumerable<Language> Languages { get; private set; }
        #endregion Languages

        #endregion properties

        #region methods

        #region CheckDbConnection
        private static void CheckDbConnection(IProfile profile) {
            string error;
            if (!TestDbConnection(((IDatabaseInputConfig) profile.InputConfig).ConnectionString, out error)) {
                LogHelper.GetLogger().ContextLog(LogLevelEnum.Error, "{0} {1}", Base.Localisation.ResourcesCommon.DatabaseConnectionFailed, error);
                throw new Exception(Base.Localisation.ResourcesCommon.DatabaseConnectionFailed + " " + error);
            }
        }
        #endregion CheckDbConnection

        #region Init
        /// <summary>
        /// Initializes the application controller.
        /// </summary>
        public static void Init() {
            try {
                // init user config
                UserConfig = UserConfig.GetUserConfig();

                // init configuration database
                ConfigDb.Init();


                // load last used language
                if (ConfigDb.LastLanguage != null)
                {
                    foreach (Language lng in Languages)
                        if (lng.Culture.CompareInfo.Name == ConfigDb.LastLanguage)
                            SelectedLanguage = lng;
                }


                IsInitialized = true;

                LogHelper.GetLogger().ContextLog(LogLevelEnum.Debug, "Initialized");

            } catch (Exception ex) {
                LogHelper.GetLogger().ContextLog(LogLevelEnum.Error, "Error: {0}", ex.Message);
                OnError(ex.Message);
            }
        }
        #endregion Init

        #region Shutdown
        /// <summary>
        /// Shuts down the application controller.
        /// </summary>
        public static void Shutdown() {
            LogHelper.GetLogger().ContextLog(LogLevelEnum.Debug, "");
            IsInitialized = false;

            try {
                ConfigDb.Shutdown();
            } catch (Exception ex) {
                OnError(ex.Message);
            }
        }
        #endregion Shutdown

        public static bool TestDbConnection(string connectionString, out string error) {
            LogHelper.GetLogger().ContextLog(LogLevelEnum.Debug, "");

            try {
                using (var conn = ConnectionManager.CreateConnection("GenericODBC", connectionString)) {
                    conn.Open();
                }
            } catch (Exception ex) {
                error = ex.Message;
                LogHelper.GetLogger().ContextLog(LogLevelEnum.Error, "Error: {0}", error);

                // workaround: ODBC (MySQL, other engines not tested) return the exception message twice
                try {
                    string tmp1 = error.Substring(0, error.Length / 2 - 1);
                    string tmp2 = error.Substring(tmp1.Length + 2);
                    if (tmp1 == tmp2) error = tmp1;
                } catch (Exception) { }

                return false;
            }

            error = string.Empty;
            return true;
        }

        #region GetSupportMailHRef
        public static string GetSupportMailHRef() {
            const string br = "%0A";
            string version =
                Assembly.GetEntryAssembly().GetName().Version.Major + "." +
                Assembly.GetEntryAssembly().GetName().Version.Minor +
                (Assembly.GetEntryAssembly().GetName().Version.Build > 0
                     ? "." + Assembly.GetEntryAssembly().GetName().Version.Build
                     : "");

            string mailtoBody = Base.Localisation.ResourcesCommon.SupportMailProgramVersion + version + br;
            mailtoBody += Base.Localisation.ResourcesCommon.SupportMailOperatingSystem + OSHelper.GetOSName() + " (" +
                          Environment.OSVersion.VersionString + ")" +
                          br;

            switch (OSHelper.GetPlatform()) {
                case OSHelper.Platform.X86:
                    mailtoBody += Base.Localisation.ResourcesCommon.SupportMailArchitecture + "32 Bit";
                    break;
                case OSHelper.Platform.X64:
                    mailtoBody += Base.Localisation.ResourcesCommon.SupportMailArchitecture + "64 Bit";
                    break;
                case OSHelper.Platform.Unknown:
                    mailtoBody += Base.Localisation.ResourcesCommon.SupportMailArchitecture +
                                  Base.Localisation.ResourcesCommon.SupportMailUnknownArchitecture;
                    break;
            }
            mailtoBody += br + br;

            mailtoBody += Base.Localisation.ResourcesCommon.SupportMailCompanyName + br;
            mailtoBody += Base.Localisation.ResourcesCommon.SupportMailContactPerson + br;
            mailtoBody += Base.Localisation.ResourcesCommon.SupportMailPhone + br;
            mailtoBody += Base.Localisation.ResourcesCommon.SupportMailEMail + br;

            mailtoBody += br + Base.Localisation.ResourcesCommon.SupportMailProblemDescription + br;

            return "mailto:support@avendata.de?subject=TransDATA Professional Support&body=" + mailtoBody;
        }
        #endregion GetSupportMailHRef

        #endregion methods

        public static IDataTransferAgent GetDataTransferAgent(IProfile profile) {
            LogHelper.GetLogger().ContextLog(LogLevelEnum.Debug, "Profile name: {0}", profile.Name);
            return new DataTransferAgent(profile);
        }

        public static IExporter GetExporter(IProfile profile) {
            LogHelper.GetLogger().ContextLog(LogLevelEnum.Debug, "Profile name: {0}", profile.Name);
            return new Exporter(profile);
        }
 
        public static IImporterDbStructure GetImporterDbStructure(IProfile profile) {
            LogHelper.GetLogger().ContextLog(LogLevelEnum.Debug, "Profile name: {0}", profile.Name);
            return new ImporterDbStructure(profile);
        }
    }
}