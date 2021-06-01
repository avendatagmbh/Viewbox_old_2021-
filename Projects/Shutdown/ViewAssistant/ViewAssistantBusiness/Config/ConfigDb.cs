using System;
using System.IO;
using System.Configuration;
using System.Text;
using AV.Log;
using Base.Localisation;
using DbAccess;
using DbAccess.Structures;
using System.Collections;
using System.Threading;
using System.Reflection;

namespace ViewAssistantBusiness.Config
{
    public static class ConfigDb
    {
        #region ConnectionManager
        public static ConnectionManager ConnectionManager { get { return _cm; } }
        private static ConnectionManager _cm;
        #endregion ConnectionManager

        public static bool IsInitialized { get; private set; }

        /// <summary>
        /// Returns the full filename of the configuration database.
        /// </summary>
        private static string DbFile
        {
            get
            {
                string path;

                //If app settings exists...
                if (ConfigurationManager.AppSettings != null &&
                    !string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["ConfigDBPath"]))
                {
                    //Get the full path from the config.
                    path = ConfigurationManager.AppSettings["ConfigDBPath"];
                }
                else
                {
                    //Use default values.
                    path = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\AvenDATA\\ViewAssistant";
                }

                try
                {
                    //If the folder does not exist, try to create it.
                    if (!Directory.Exists(path))
                        Directory.CreateDirectory(path);
                }
                catch (Exception ex)
                {
                    //If any error happens, throw an init exception with the exception's data.
                    throw new ConfigDbInitializationException(ex.Message, ex);
                }

                string file = "viewassistant.db3";

                return path + "\\" + file;
            }
        }
        public static ProfileManager ProfileManager { get; private set; }


        #region Init
        /// <summary>
        /// Initializes the config-db interface.
        /// </summary>
        public static void Init()
        {
            try
            {
                LogHelper.ConfigureLogger(LogHelper.GetLogger(), GetActualLogFilePath());

                // init connection manager
                var dbConfig = new DbConfig("SQLite") { Hostname = DbFile, DbName = "Main", Password = "" };
                
                _cm = new ConnectionManager(dbConfig, 10);
                _cm.Init();
                while (!_cm.IsInitialized)
                {
                    Thread.Sleep(100);
                }
                using (IDatabase conn = _cm.GetConnection())
                {
                    // create tables
                    try
                    {
                        conn.DbMapping.CreateTableIfNotExists<ProfileConfig>();
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(
                            string.Format(
                                ExceptionMessages.CreateTableFailed,
                                conn.DbMapping.GetTableName<ProfileConfig>(),
                                ex.Message),
                            ex);
                    }

                    try
                    {
                        conn.DbMapping.AddColumn("ProfileConfig", "Boolean", new DbColumnAttribute("HideRowCounts"), null);
                    }
                    catch{}

                    IsInitialized = true;
                }

                ProfileManager = new ProfileManager(_cm);
            }
            catch (Exception ex)
            {
                throw new ConfigDbInitializationException(ex.Message, ex);
            }
        }
        #endregion Init

        #region GetConnection
        internal static IDatabase GetConnection()
        {
            if (!IsInitialized) throw new ConfigDbNotInitializedException();
            return _cm.GetConnection();
        }
        #endregion GetConnection

        #region Save
        public static void Save(object entity)
        {
            using (IDatabase conn = GetConnection())
            {
                try
                {
                    conn.BeginTransaction();
                    conn.DbMapping.Save(entity);
                    conn.CommitTransaction();
                }
                catch (Exception)
                {
                    if (conn.HasTransaction())
                        conn.RollbackTransaction();
                    throw;
                }
            }
        }
        #endregion Save

        #region Delete
        internal static void Delete(object entity)
        {
            using (IDatabase conn = GetConnection())
            {
                try
                {
                    conn.BeginTransaction();
                    conn.DbMapping.Delete(entity);
                    conn.CommitTransaction();
                }
                catch (Exception)
                {

                    if (conn.HasTransaction())
                        conn.RollbackTransaction();
                    throw;
                }
            }
        }
        #endregion

        public static void Cleanup()
        {
            if (_cm != null)
                _cm.Dispose();
        }

        public static string GetActualLogFilePath()
        {
            var date = DateTime.Now;
            var sb = new StringBuilder();
            sb.Append(GetLogDirectoryPath());
            sb.Append("log");
            sb.Append(date.Year);
            if(date.Month < 10)
            {
                sb.Append("0");
            }
            sb.Append(date.Month);
            if (date.Day < 10)
            {
                sb.Append("0");
            }
            sb.Append(date.Day);
            sb.Append(".txt");
            return sb.ToString();
        }

        public static string GetLogDirectoryPath()
        {
            var fi = new FileInfo(System.Reflection.Assembly.GetExecutingAssembly().Location);
            var str = fi.DirectoryName + "\\log\\";
            if (!Directory.Exists(str))
                Directory.CreateDirectory(str);
            return str;
        }
    }
}
