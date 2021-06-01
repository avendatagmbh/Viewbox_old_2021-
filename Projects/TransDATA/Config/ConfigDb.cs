// (C)opyright 2011 AvenDATA GmbH
// ----------------------------------------
// author: Mirko Dibbert
// since : 2011-08-27

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Base.Exceptions;
using Base.Localisation;
using Config.DbStructure;
using Config.Interfaces.DbStructure;
using Config.Manager;
using DbAccess;
using DbAccess.Structures;
using Utils;

namespace Config {
    /// <summary>
    /// Interface to the configuration database for TransDATA professional.
    /// </summary>
    public static class ConfigDb {
        #region properties

        #region ConnectionManager
        public static ConnectionManager ConnectionManager { get { return _cm; } }
        private static ConnectionManager _cm;
        #endregion ConnectionManager

        /// <summary>
        /// Returns the full filename of the configuration database.
        /// </summary>
        private static string DbFile {
            get {
                const string file = "transdata.db3";
                string path = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) +
                              "\\AvenDATA\\TransDATA";
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                return path + "\\" + file;
            }
        }

        /// <summary>
        /// Returns the password of the configuration database.
        /// </summary>
        private static string DbPwd {
            get { return "" /*"dRFjeZcv682g(se$gkbHJStHsJHR3%"*/; }
        }

        public static bool IsInitialized { get; private set; }

        public static string DbVersion {
            get { return Info.DbVerion; }
            set { Info.DbVerion = value; }
        }

        public static string LastLanguage
        {
            get { return Info.LastLanguage; }
            set { Info.LastLanguage = value; }
        }

        public static string LastProfile
        {
            get { return Info.LastProfile; }
            set { Info.LastProfile = value; }
        }

        //--------------------------------------------------------------------------------
        #endregion properties

        #region Init
        /// <summary>
        /// Initializes the config-db interface.
        /// </summary>
        public static void Init() {
            try {
                // init connection manager
                var dbConfig = new DbConfig("SQLite") {Hostname = DbFile, DbName = "Main", Password = DbPwd};
                //var dbConfig = new DbConfig("MySQL") { Hostname = "localhost", Username = "root", DbName = "TransDATADebug", Password = "avendata" };
                _cm = new ConnectionManager(dbConfig, 10);
                _cm.Init();

                using (IDatabase conn = _cm.GetConnection()) {
                    // create tables
                    CreateTable<Info>(conn);
                    CreateTable<User>(conn);
                    CreateTable<Profile>(conn);
                    CreateTable<Table>(conn);
                    CreateTable<TableColumn>(conn);
                    CreateTable<DbStructure.File>(conn);
                    CreateTable<InputConfig>(conn);
                    CreateTable<OutputConfig>(conn);
                    CreateTable<MailConfig>(conn);

                    IsInitialized = true;

                    // init DbVersion
                    string currentDbVersion = Info.DbVerion;
                    if (currentDbVersion == null) {
                        // init version with current databbase version
                        Info.DbVerion = VersionInfo.CurrentDbVersion;
                    }
                    else if (VersionInfo.NewerDbVersionExists(currentDbVersion)) {
                        // TODO: do database update                    
                    }
                    else if (VersionInfo.ProgramVersionToOld(currentDbVersion)) {
                        // TODO: show message, that the actual TransDATA version needs to be updated
                    }

                    // init manager
                    UserManager = new UserManager(conn);
                    ProfileManager = new ProfileManager(conn);
                    MailManager.Load(conn);
                }
            }
            catch (Exception ex) {
                throw new ConfigDbInitializationException(ex.Message, ex);
            }
        }
        #endregion Init

        #region Shutdown
        /// <summary>
        /// Shuts down the config-db interface.
        /// </summary>
        public static void Shutdown() {
            try {
                IsInitialized = false;
                if (_cm != null) _cm.Dispose();
            }
            catch (Exception ex) {
                throw new ConfigDbShutdownException(ex.Message, ex);
            }
        }
        #endregion Shutdown

        #region Manager
        //----------------------------------------
        public static UserManager UserManager { get; private set; }
        public static ProfileManager ProfileManager { get; private set; }

        //----------------------------------------
        #endregion Manager

        #region CreateTable
        private static void CreateTable<T>(IDatabase conn) {
            try {
                conn.DbMapping.CreateTableIfNotExists<T>();
            }
            catch (Exception ex) {
                throw new Exception(
                    string.Format(
                        ExceptionMessages.CreateTableFailed,
                        conn.DbMapping.GetTableName<T>(),
                        ex.Message),
                    ex);
            }
        }
        #endregion

        #region GetConnection
        internal static IDatabase GetConnection() {
            if (!IsInitialized) throw new ConfigDbNotInitializedException();
            return _cm.GetConnection();
        }
        #endregion GetConnection

        #region Save
        public static void Save(object entity) {
            using (IDatabase conn = GetConnection()) {
                try {
                    conn.BeginTransaction();
                    if (entity is IEnumerable) {
                        if (entity is IList) {
                            var list = (IList) entity;
                            if (list.Count != 0) {
                                conn.DbMapping.Save(list[0].GetType(), list);
                            }
                        } else {
                            foreach (var item in (entity as IEnumerable)) {
                                conn.DbMapping.Save(item);
                            }
                        }
                    } else {
                        conn.DbMapping.Save(entity);
                    }
                    conn.CommitTransaction();
                } catch (Exception) {
                    conn.RollbackTransaction();
                    throw;
                }
            }
        }
        #endregion Save

        #region Delete
        internal static void Delete(object entity) {
            using (IDatabase conn = GetConnection()) {
                try {
                    conn.BeginTransaction();
                    conn.DbMapping.Delete(entity);
                    conn.CommitTransaction();
                } catch (Exception) {
                    conn.RollbackTransaction();
                    throw;
                }
            }
        }
        #endregion

        public static void UpdateDoExport(IEnumerable<ITable> tables, bool value) {
            if(!tables.Any())
                return;

            using (IDatabase conn = GetConnection()) {
                StringBuilder sql =
                    new StringBuilder(string.Format("UPDATE {0} SET {1}={2} WHERE id in (",
                                                    conn.Enquote(conn.DbMapping.GetTableName<Table>()),
                                                    conn.Enquote(conn.DbMapping.GetColumnName<Table>("DoExport")), value ? 1:0));
                foreach (var table in tables) {
                    sql.Append(table.Id).Append(',');
                }
                //Remove last ,
                sql.Remove(sql.Length - 1, 1);
                sql.Append(")");

                conn.ExecuteNonQuery(sql.ToString());
            }
        }

        public static void Cleanup() {
            if (_cm != null)
                _cm.Dispose();
        }

    }
}