// -----------------------------------------------------------
// Created by Benjamin Held - 22.07.2011 15:45:10
// Copyright AvenDATA 2011
// -----------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.Cryptography;
using DbAccess.Structures;

namespace eBalanceKitBase {
    /// <summary>
    /// Utility class for string functions.
    /// </summary>
    public static class StringUtils {

        /// <summary>
        /// Returns the hash value for the specified password.
        /// </summary>
        /// <param name="password">The password.</param>
        /// <param name="salt">The salt.</param>
        /// <returns></returns>
        public static string GetPasswordHash(string password, string salt) {
            System.Security.Cryptography.SHA1 cspSha512 = // workaround - SHA512 not supported for WinXP
                new System.Security.Cryptography.SHA1CryptoServiceProvider();

            System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();

            string hash = ByteArrayToString(cspSha512.ComputeHash(enc.GetBytes(password + salt)));

            cspSha512.Dispose();

            return hash;
        }

        /// <summary>
        /// Verifies if the specified password/salt combination fits to the given hash value.
        /// </summary>
        /// <param name="password">The password.</param>
        /// <param name="salt">The salt.</param>
        /// <param name="hashValue">The hash value.</param>
        /// <returns></returns>
        public static bool VerifyPassword(string password, string salt, string hashValue) {
            return hashValue.Equals(GetPasswordHash(password, salt));
        }

        /// <summary>
        /// Encrypts the string.
        /// </summary>
        /// <param name="clearText">The clear text.</param>
        /// <param name="Key">The key.</param>
        /// <param name="IV">The IV.</param>
        /// <returns></returns>
        private static byte[] EncryptString(byte[] clearText, byte[] Key, byte[] IV) {
            MemoryStream ms = new MemoryStream();
            Rijndael alg = Rijndael.Create();
            alg.Key = Key;
            alg.IV = IV;
            CryptoStream cs = new CryptoStream(ms, alg.CreateEncryptor(), CryptoStreamMode.Write);
            cs.Write(clearText, 0, clearText.Length);
            cs.Close();
            byte[] encryptedData = ms.ToArray();
            return encryptedData;
        }

        /// <summary>
        /// Encrypts the string.
        /// </summary>
        /// <param name="clearText">The clear text.</param>
        /// <param name="Password">The password.</param>
        /// <returns></returns>
        public static string EncryptString(string clearText, string Password) {
            byte[] clearBytes = System.Text.Encoding.Unicode.GetBytes(clearText);
            PasswordDeriveBytes pdb = new PasswordDeriveBytes(Password, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
            byte[] encryptedData = EncryptString(clearBytes, pdb.GetBytes(32), pdb.GetBytes(16));
            return Convert.ToBase64String(encryptedData);
        }

        /// <summary>
        /// Encrypts the string.
        /// </summary>
        /// <param name="clearText">The clear text.</param>
        /// <param name="Password">The password.</param>
        /// <returns></returns>
        public static string EncryptString(string clearText, string salt, string Password) {
            byte[] clearBytes = System.Text.Encoding.Unicode.GetBytes(clearText + salt);
            PasswordDeriveBytes pdb = new PasswordDeriveBytes(Password, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
            byte[] encryptedData = EncryptString(clearBytes, pdb.GetBytes(32), pdb.GetBytes(16));
            return Convert.ToBase64String(encryptedData);
        }

        /// <summary>
        /// Decrypts the string.
        /// </summary>
        /// <param name="cipherData">The cipher data.</param>
        /// <param name="Key">The key.</param>
        /// <param name="IV">The IV.</param>
        /// <returns></returns>
        private static byte[] DecryptString(byte[] cipherData, byte[] Key, byte[] IV) {
            MemoryStream ms = new MemoryStream();
            Rijndael alg = Rijndael.Create();
            alg.Key = Key;
            alg.IV = IV;
            CryptoStream cs = new CryptoStream(ms, alg.CreateDecryptor(), CryptoStreamMode.Write);
            cs.Write(cipherData, 0, cipherData.Length);
            cs.Close();
            byte[] decryptedData = ms.ToArray();
            return decryptedData;
        }

        /// <summary>
        /// Decrypts the string.
        /// </summary>
        /// <param name="cipherText">The cipher text.</param>
        /// <param name="Password">The password.</param>
        /// <returns></returns>
        public static string DecryptString(string cipherText, string Password) {
            byte[] cipherBytes = Convert.FromBase64String(cipherText);
            PasswordDeriveBytes pdb = new PasswordDeriveBytes(Password, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
            byte[] decryptedData = DecryptString(cipherBytes, pdb.GetBytes(32), pdb.GetBytes(16));
            return System.Text.Encoding.Unicode.GetString(decryptedData);
        }

        /// <summary>
        /// Decrypts the string.
        /// </summary>
        /// <param name="cipherText">The cipher text.</param>
        /// <param name="Password">The password.</param>
        /// <returns></returns>
        public static string DecryptString(string cipherText, string salt, string Password) {
            byte[] cipherBytes = Convert.FromBase64String(cipherText);
            PasswordDeriveBytes pdb = new PasswordDeriveBytes(Password, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
            byte[] decryptedData = DecryptString(cipherBytes, pdb.GetBytes(32), pdb.GetBytes(16));

            string sDecrypted = System.Text.Encoding.Unicode.GetString(decryptedData);
            return sDecrypted.Substring(0, sDecrypted.Length - salt.Length);
        }


        /// <summary>
        /// Returs the hexadecimal representation for the specified byte array.
        /// </summary>
        /// <param name="arrInput">The byte array.</param>
        /// <returns></returns>
        public static string ByteArrayToString(byte[] arrInput) {
            int i;
            StringBuilder sOutput = new StringBuilder(arrInput.Length * 2);
            for (i = 0; i < arrInput.Length; i++) {
                sOutput.Append(arrInput[i].ToString("X2"));
            }
            return sOutput.ToString();
        }

        public static string CreateKey(string company, string serial) {
            string tmp =
                company.ToLower().Replace(" ", "") +
                serial.ToLower().Replace(" ", "");

            string tmpKey = StringUtils.EncryptString(tmp, "dog82cg!&f2§gksovFGJHOETvwt8$631g");

            System.Security.Cryptography.MD5 csp = new System.Security.Cryptography.MD5CryptoServiceProvider();
            System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
            string hash = StringUtils.ByteArrayToString(csp.ComputeHash(enc.GetBytes(tmpKey)));
            csp.Dispose();

            string key = string.Empty;
            for (int i = 0; i < 32; i += 2) {
                key += hash[i];
                if (i == 6) key += "-";
                if (i == 14) key += "-";
                if (i == 22) key += "-";
            }
            return key;
        }
    }

    public class DatabaseConfig {
        public DatabaseConfig() {
            if (File.Exists(System.Environment.CurrentDirectory + "\\" + ConfigFile))
                UsedConfigPath = System.Environment.CurrentDirectory + "\\";
            else
                UsedConfigPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.CommonApplicationData) + "\\AvenDATA\\eBalanceKit\\";
            UsedConfigFile = UsedConfigPath + ConfigFile;
        }

        public string UsedConfigPath { get; private set; }
        private static string UsedConfigFile { get; set; }
        public static string ConfigFile = "eBalanceKit.cfg";
        private static string ConfigPassword = "j54z8dj237S8357fOJse2093DZmXU39";

        public DbConfig DbConfig { get; set; }

        private ProxyConfig _proxyConfig = new ProxyConfig();
        public ProxyConfig ProxyConfig {
            get { return _proxyConfig; }
            set { _proxyConfig = value; }
        }
        public string BackupDirectory{get;set;}
        public bool ExistsConfigfile {
            get {
                return File.Exists(ConfigFile) || File.Exists(System.Environment.GetFolderPath(
                    System.Environment.SpecialFolder.CommonApplicationData) + "\\AvenDATA\\eBalanceKit\\" + ConfigFile);
            }
        }

        public Structures.Backup.AutomaticBackupConfig AutomaticBackupConfig { get; set; }

        public void LoadConfig() {
            // init default values
            if (!File.Exists(UsedConfigPath + ConfigFile)) {
                //this.UseSQLiteEngine = true;
                throw new Exception("Configdatei existiert nicht.");
            }

            StreamReader reader = null;
            try{
                string dbType = null;
                string dbHost = null;
                string dbName = null;
                string dbUser = null;
                string dbPassword = null;
                string dbSid = null;

                string proxy_host = null;
                string proxy_port = null;
                string proxy_user = null;
                string proxy_password = null;
                
                reader = new StreamReader(UsedConfigFile);
                while (!reader.EndOfStream) {
                    string line = reader.ReadLine().Trim();
                    if (line.Length == 0) continue;
                    if (!line.Contains("=")) continue;

                    string[] parts = new string[2];
                    parts[0] = line.Substring(0, line.IndexOf('='));
                    parts[1] = line.Substring(line.IndexOf('=') + 1, line.Length - line.IndexOf('=') - 1);
                    switch (parts[0].Trim().ToLower()) {
                        case "dbtype":
                            dbType = parts[1].Trim();
                            break;

                        case "dbhost":
                            dbHost = parts[1].Trim();
                            break;

                        case "dbuser":
                            dbUser = parts[1].Trim();
                            break;
                            
                        case "dbpassword":
                            string pwd = parts[1].Trim();
                            dbPassword = StringUtils.DecryptString(pwd, ConfigPassword);
                            break;

                        case "dbname":
                            dbName = parts[1].Trim();
                            break;

                        case "proxy_host":
                            proxy_host = parts[1].Trim();
                            break;

                        case "proxy_port":
                            proxy_port = parts[1].Trim();
                            break;

                        case "proxy_user":
                            proxy_user = parts[1].Trim();
                            break;

                        case "proxy_password":
                            proxy_password = StringUtils.DecryptString(parts[1].Trim(), ConfigPassword);
                            break;

                        case "backup_directory":
                            BackupDirectory = parts[1].Trim();
                            break;
                
                        case "sid":
                            dbSid = parts[1].Trim();
                            break;

                        case "auto_backup":
                            AutomaticBackupConfig = Structures.Backup.AutomaticBackupConfig.Load(parts[1].Trim());
                            break;

                    }
                }

                ProxyConfig.Host = proxy_host;
                ProxyConfig.Port = proxy_port;
                ProxyConfig.Username = proxy_user;
                ProxyConfig.Password = proxy_password;


                
                if (dbType == "MySQL" || dbType == "SQLServer" || dbType == "Oracle") {
                    DbConfig = new DbAccess.Structures.DbConfig(dbType) {
                        Username = dbUser,
                        Password = dbPassword,
                        Hostname = dbHost,
                        DbName = dbName,
                        SID = dbSid
                        
                    };
                } else {
                    // SQLite
                    dbName = "Main";
                    DbConfig = new DbAccess.Structures.DbConfig(dbType) {
                        DbName = dbName,
                        Username = dbUser,
                        Password = dbPassword,
                        Hostname = dbHost
                    };
                }

                if (string.IsNullOrEmpty(BackupDirectory))
                    BackupDirectory = System.Environment.GetFolderPath(System.Environment.SpecialFolder.CommonApplicationData) + "\\AvenDATA\\eBalanceKit\\";
            } finally {
                if (reader != null) reader.Close();
            }
        }

        public void Save() {
            StreamWriter writer = null;
            try {
                writer = new StreamWriter(UsedConfigFile);
                writer.WriteLine("dbtype=" + DbConfig.DbType);
                writer.WriteLine("dbpassword=" + StringUtils.EncryptString(DbConfig.Password, ConfigPassword));

                if (DbConfig.DbType == "MySQL" || DbConfig.DbType == "SQLServer" || DbConfig.DbType == "Oracle") {
                    writer.WriteLine("dbhost=" + DbConfig.Hostname);
                    writer.WriteLine("dbuser=" + DbConfig.Username);
                    writer.WriteLine("dbname=" + DbConfig.DbName);
                    if (!string.IsNullOrEmpty(DbConfig.SID)) writer.WriteLine("sid=" + DbConfig.SID);
                } else if (DbConfig.DbType == "SQLite") {
                    writer.WriteLine("dbhost=" + DbConfig.Hostname);
                }

                writer.WriteLine("proxy_host=" + ProxyConfig.Host);
                writer.WriteLine("proxy_port=" + ProxyConfig.Port);
                writer.WriteLine("proxy_user=" + ProxyConfig.Username);
                writer.WriteLine("proxy_password=" + StringUtils.EncryptString(ProxyConfig.Password, ConfigPassword));
                writer.WriteLine("backup_directory=" + BackupDirectory);
                writer.WriteLine("auto_backup=" + AutomaticBackupConfig);
            } catch (Exception ex) {
                throw new Exception("Application config could not be saved: " + Environment.NewLine + ex.Message);

            } finally {
                if (writer != null) writer.Close();
            }
        }

        public DbConfig GetDbConfig() {
            // write config parts which are persistated into the database
            //DbConfig config;
            //if (this.UseSQLiteEngine) {
            //    config = new DbConfig("SQLite");
            //    config.DbName = "Main";
            //    config.Hostname = this.HostnameSQLite;
            //    config.Password = "4264g27S78HWP637JnS7RbH72fTjLsd";

            //} else if (this.UseMySQLEngine) {
            //    config = new DbConfig("MySQL");
            //    config.DbName = this.DbName;
            //    config.Hostname = this.Hostname;
            //    config.Password = this.Password;

            //} else if (UseSQLServerEngine) {
            //    config = new DbConfig("SQLServer");
            //    config.DbName = this.DbName;
            //    config.Hostname = this.Hostname;
            //    config.Password = this.Password;
            //} else throw new NotImplementedException();

            //config.Username = this.User;

            //return config;
            return DbConfig;
        }
    }
}
