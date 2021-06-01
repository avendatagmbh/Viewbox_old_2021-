// -----------------------------------------------------------
// Created by Benjamin Held - 26.06.2011 13:54:34
// Copyright AvenDATA 2011
// -----------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DbAccess;
using XmlDatabaseBackup;
using System.Xml;
using eBalanceKitBase;
using System.IO;
using DbAccess.Structures;
using System.Security.Cryptography;
using System.Globalization;

namespace DatabaseManagement.DbUpgrade {
    public class eBalanceBackup {
        public class UserInfo{
            public string Comment { get; set; }
            public string Version { get; set; }
            public string DbVersion { get; set; }
            public string CreatedTime { get; set; }

            public UserInfo() {
                CreatedTime = DateTime.Now.ToString(CultureInfo.InvariantCulture);
            }
        }

        #region Events
        public event EventHandler Progress;
        public event EventHandler Finished;
        public event EventHandler<ErrorEventArgs> Error;
        private void OnError(Exception ex) { if (Error != null) Error(this, new ErrorEventArgs(ex)); }
        #endregion

        //Nearly the same password which has been used for encrypting user passwords, last digit is different
        string BackupPassword { get { return "j54z8dj237S8357fOJse2093DZmXU38"; } }

        public void ExportDatabase(IDatabase conn, string filename, UserInfo userInfo, bool encrypt) {
            Info info = new Info(conn);
            if (info.DbVerion == null)
                throw new Exception("keine gültige Datenbank.");
            DatabaseBackup backuper = new DatabaseBackup(conn.DbConfig.DbName);
            //XmlTextWriter writer = new XmlTextWriter(filename,Encoding.Unicode);

            XmlWriter writer = null;
            FileStream outFile = null;
            CryptoStream cs = null;

            if (encrypt) {
                PasswordDeriveBytes pdb = new PasswordDeriveBytes(BackupPassword, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                Rijndael alg = Rijndael.Create();
                alg.Key = pdb.GetBytes(32);
                alg.IV = pdb.GetBytes(16);

                outFile = new FileStream(filename, FileMode.Create, FileAccess.Write);
                cs = new CryptoStream(outFile,
                alg.CreateEncryptor(), CryptoStreamMode.Write);
                writer = XmlWriter.Create(cs);
            } else {
                writer = new XmlTextWriter(filename,Encoding.Unicode);
                (writer as XmlTextWriter).Formatting = Formatting.Indented;
            }

            //writer.Formatting = Formatting.Indented;
            
            writer.WriteStartDocument();
                writer.WriteStartElement("Backup");
                writer.WriteAttributeString("Name", "eBilanz-Kit");
                writer.WriteAttributeString("Version", VersionInfo.Instance.CurrentVersion);
                writer.WriteAttributeString("DbVersion", info.DbVerion);
                writer.WriteAttributeString("Created", userInfo.CreatedTime);
                writer.WriteAttributeString("Comment", userInfo.Comment);
                    writer.WriteStartElement("Database");
                        writer.WriteAttributeString("Name", conn.DbConfig.DbName);
                        writer.WriteAttributeString("Type", conn.DbConfig.DbType);
                        backuper.BackupDatabase(conn, writer);
                    writer.WriteEndElement();
                writer.WriteEndElement();
            writer.WriteEndDocument();
            writer.Close();

            if (encrypt) {
                if(cs != null) cs.Close();
                if(outFile != null) outFile.Close();
            }
        }

        XmlReader GetEncryptedReader(string filename) {
            PasswordDeriveBytes pdb = new PasswordDeriveBytes(BackupPassword, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
            Rijndael alg = Rijndael.Create();
            alg.Key = pdb.GetBytes(32);
            alg.IV = pdb.GetBytes(16);

            FileStream inFile = new FileStream(filename, FileMode.Open, FileAccess.Read);
            CryptoStream cs = new CryptoStream(inFile, alg.CreateDecryptor(), CryptoStreamMode.Read);
            return XmlReader.Create(cs);
        }

        XmlReader GetUnencryptedReader(string filename) {
            return XmlReader.Create(filename);
        }

        public UserInfo ReadUserInfoFromFile(string filename) {
            UserInfo result = new UserInfo();
            try {
                result = ReadInformation(filename);
                if (string.IsNullOrEmpty(result.DbVersion))
                    result.Comment = "Kein gültiges Backup: Es konnte keine Datenbank Version gelesen werden.";
            }
            catch(Exception ex){
                result.Comment = "Kein gültiges Backup: " + ex.Message;
            }
            
            return result;
        }

        public UserInfo ReadInformation(XmlReader reader) {
            //XmlDocument doc = new XmlDocument();
            //doc.Load(reader);
            //doc.Save("test.bak");
            UserInfo userInfo = new UserInfo();
            string program = "";
            while (reader.Read()) {
                if (reader.NodeType == XmlNodeType.Element) {
                    if (reader.Name == "Backup") {
                        if (reader.HasAttributes) {
                            reader.MoveToFirstAttribute();

                            do {
                                if (reader.Name == "DbVersion")
                                    userInfo.DbVersion = reader.Value;
                                else if (reader.Name == "Name")
                                    program = reader.Value;
                                else if (reader.Name == "Comment")
                                    userInfo.Comment = reader.Value;
                                else if (reader.Name == "Created")
                                    userInfo.CreatedTime = reader.Value;
                                else if (reader.Name == "Version")
                                    userInfo.Version = reader.Value;
                                else if (reader.Name == "DbVersion")
                                    userInfo.DbVersion = reader.Value;
                            }
                            while (reader.MoveToNextAttribute());
                        }
                    }
                }
            }
            if (program != "eBilanz-Kit")
                throw new Exception("Kein valides Datenbank-Backup.");
            return userInfo;
        }

        public UserInfo ReadInformation(string filename) {
            try {
                return ReadInformation(GetEncryptedReader(filename));
            } catch (Exception) {
                return ReadInformation(GetUnencryptedReader(filename));
            }
        }

        public void ImportDatabase(object data) {
            try {
                Tuple<DbConfig, string> tuple = data as Tuple<DbConfig, string>;
                ImportDatabase(tuple.Item1, tuple.Item2);
            } catch (Exception ex) {
                OnError(ex);
            }
        }

        private string backuperDbVersion;
        private DbConfig backuperDbConfig;

        public void ImportDatabase(DbConfig dbConfig, string filename) {
            //First try to create database
            if (dbConfig.DbType == "MySQL" || dbConfig.DbType == "SQLServer") {
                DbConfig dbConfigNoDatabase = (DbConfig)dbConfig.Clone();
                dbConfigNoDatabase.DbName = null;
                using (var conn = DbAccess.ConnectionManager.CreateConnection(dbConfigNoDatabase)) {
                    conn.Open();
                    conn.CreateDatabaseIfNotExists(dbConfig.DbName);
                }
            }

            XmlReader reader = GetEncryptedReader(filename);
            //Try to read backup with the help of an encrypted and an unencrypted filestream if the prior fails
            try {
                ImportHelper(filename, dbConfig, GetEncryptedReader);
            } catch (Exception) {
                ImportHelper(filename, dbConfig, GetUnencryptedReader);
            }
        }

        /// <summary>
        /// Uncrypt an database backup and store in plain XML as "<i>backupname</i>.backup.xml"
        /// </summary>
        /// <author>Sebastian Vetter</author>
        public void UncryptDatabase(DbConfig dbConfig, string filename) {
            XmlReader reader;
            //Try to read backup with the help of an encrypted and an unencrypted filestream if the prior fails
            try {
                reader = GetEncryptedReader(filename);
                Console.Write(reader.ReadInnerXml());
            }
            catch (Exception) {
                reader = GetUnencryptedReader(filename);
                
                Console.Write(reader.ReadInnerXml());
            }
            HashSet<string> a = new HashSet<string>();
            while (reader.Read()) {
                reader.ReadInnerXml();
                var x = reader.ReadOuterXml();
                a.Add(x);

            }
            System.IO.File.WriteAllLines(filename + ".backup.xml", a);
        }

        private delegate XmlReader GetXmlReader(string filename);
        private void ImportHelper(string filename, DbConfig dbConfig, GetXmlReader getReader) {
            using (var conn = DbAccess.ConnectionManager.CreateConnection(dbConfig)) {
                conn.Open();
                //UserInfo userInfo = ReadInformation(filename);
                UserInfo userInfo = ReadInformation(getReader(filename));

                string dbVersion = VersionInfo.Instance.GetLastDbVersion(userInfo.DbVersion);
                //Get the DB Version which is in the history
                dbVersion = VersionInfo.Instance.GetLastDbVersion(dbVersion);

                // drop existing tables
                foreach (var table in conn.GetTableList()) {

                    if (table == "sqlite_sequence") continue;

                    conn.DropTable(table);
                }

                // create new tables
                DatabaseCreator.Instance.CreateDatabase(dbVersion, conn.DbConfig.DbType, conn);
                
                DatabaseBackup backuper = new DatabaseBackup(conn.DbConfig.DbName);
                backuper.Progress += Progress;
                backuper.Error += Error;
                backuper.Finished += Finished;

                backuperDbVersion = dbVersion;
                backuperDbConfig = dbConfig;


                XmlReader reader = getReader(filename);
                backuper.ImportDatabase(conn, reader, ValidTableName);
            }
        }


        private bool ValidTableName(string tableName) {
            List<string> tables = DatabaseCreator.Instance.GetTableNames(backuperDbVersion, backuperDbConfig.DbType);
            
            if (VersionInfo.Instance.VersionToDouble(backuperDbVersion) >= VersionInfo.Instance.VersionToDouble("1.3.0") &&
                tableName.StartsWith("log_report_")) {
                using (var conn = DbAccess.ConnectionManager.CreateConnection(backuperDbConfig)) {
                    conn.Open();
                    if (tableName.EndsWith("_value_change")) {
                        DatabaseCreator.Instance.CreateTable(backuperDbVersion, backuperDbConfig.DbType, conn, "log_report_1_value_change", tableName);
                    } else {
                        DatabaseCreator.Instance.CreateTable(backuperDbVersion, backuperDbConfig.DbType, conn, "log_report_1", tableName);
                    }
                    return true;
                }
            }
            
            if (tables.Contains(tableName))
                return true;
            return false;
        }
    }
}
