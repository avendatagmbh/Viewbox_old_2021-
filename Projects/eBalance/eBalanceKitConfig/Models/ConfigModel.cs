using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using DbAccess.Structures;
using System.ComponentModel;
using System.Security.Cryptography;
using System.Windows;
using eBalanceKitBase;

namespace eBalanceKitConfig.Models {

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

    public class ConfigModel : INotifyPropertyChanged {

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string property) {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(property));
        }

        public ConfigModel(Window owner) {
            if (File.Exists(System.Environment.CurrentDirectory + "\\" + ConfigFile))
                UsedConfigPath = System.Environment.CurrentDirectory + "\\";
            else {
                UsedConfigPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.CommonApplicationData) + "\\AvenDATA\\eBalanceKit\\";
                // create config directory
                try {
                    if (!Directory.Exists(UsedConfigPath)) Directory.CreateDirectory(UsedConfigPath);

                } catch (Exception ex) {
                    throw new Exception("Config directory '" + UsedConfigPath + "' could not be created: " + Environment.NewLine + ex.Message);
                }
            }

            this.Owner = owner;
            this.DbName = "eBalanceKit";
            this.HostnameSQLite = UsedConfigPath + "eBalanceKit.db3";
            this.DbNameSQLite = "Main";
            this.Hostname = "localhost";
            this.DbName = "e_balance_kit";
            this.ProxyConfig = new ProxyConfig();
            this.DatabaseConfig = new DatabaseConfig();
            this.SID = "orcl";

        }

        public string UsedConfigPath { get; private set; }
        //private static string ConfigPath { get { return System.Environment.GetFolderPath(System.Environment.SpecialFolder.CommonApplicationData) + "\\AvenDATA\\eBalanceKit\\"; } }
        private static string ConfigFile = "eBalanceKit.cfg";
        private static string DbPasswordSQLite = "4264g27S78HWP637JnS7RbH72fTjLsd";
        private static string ConfigPassword = "j54z8dj237S8357fOJse2093DZmXU39";

        private bool _useSQLiteEngine;
        public bool UseSQLiteEngine {
            get { return _useSQLiteEngine; }
            set {
                _useSQLiteEngine = value;
                OnPropertyChanged("UseSQLiteEngine");
            }
        }

        private bool _useMySQLEngine;
        public bool UseMySQLEngine {
            get { return _useMySQLEngine; }
            set {
                _useMySQLEngine = value;
                OnPropertyChanged("UseMySQLEngine");
            }
        }

        private bool _useSQLServerEngine;
        public bool UseSQLServerEngine {
            get { return _useSQLServerEngine; }
            set {
                _useSQLServerEngine = value;
                OnPropertyChanged("UseSQLServerEngine");
            }
        }

        private bool _useOracleServerEngine;
        public bool UseOracleServerEngine {
            get { return _useOracleServerEngine; }
            set {
                _useOracleServerEngine = value;
                OnPropertyChanged("UseOracleServerEngine");
            }
        }

        private string _hostname;
        public string Hostname {
            get { return _hostname; }
            set {
                _hostname = value;
                OnPropertyChanged("Hostname");
            }
        }

        private string _hostnameSQLite;
        public string HostnameSQLite {
            get { return _hostnameSQLite; }
            set {
                _hostnameSQLite = value;
                OnPropertyChanged("HostnameSQLite");
            }
        }

        private string _user;
        public string User {
            get { return _user; }
            set {
                _user = value;
                OnPropertyChanged("User");
            }
        }

        private string _password;
        public string Password {
            get { return _password; }
            set {
                _password = value;
                OnPropertyChanged("Password");
            }
        }

        private string _dbName;
        public string DbName {
            get { return _dbName; }
            set {
                _dbName = value;
                OnPropertyChanged("DbName");
            }
        }
        private string _sid;
        public string SID {
            get { return _sid; }
            set {
                _sid = value;
                OnPropertyChanged("SID");
            }
        }

        private string _dbNameSQLite;
        public string DbNameSQLite {
            get { return _dbNameSQLite; }
            set {
                _dbNameSQLite = value;
                OnPropertyChanged("DbNameSQLite");
            }
        }

        public ProxyConfig ProxyConfig { get; set; }
        private DatabaseConfig DatabaseConfig{get;set;}
        public void LoadConfig() {
            // init default values
            if (!File.Exists(UsedConfigPath + ConfigFile)) {
                this.UseSQLiteEngine = true;
                return;
            }

            DatabaseConfig = new DatabaseConfig();
            DatabaseConfig.LoadConfig();
            
            this.UseMySQLEngine = false;
            this.UseMySQLEngine = false;
            this.UseSQLServerEngine = false;

            switch (DatabaseConfig.DbConfig.DbType) {
                case "SQLite":
                    this.UseSQLiteEngine = true;
                    this.HostnameSQLite = DatabaseConfig.DbConfig.Hostname;
                    break;

                case "MySQL":
                    this.UseMySQLEngine = true;
                    this.Hostname = DatabaseConfig.DbConfig.Hostname;
                    this.User = DatabaseConfig.DbConfig.Username;
                    this.Password = DatabaseConfig.DbConfig.Password;
                    this.DbName = DatabaseConfig.DbConfig.DbName;
                    break;

                case "SQLServer":
                    this.UseSQLServerEngine = true;
                    this.Hostname = DatabaseConfig.DbConfig.Hostname;
                    this.User = DatabaseConfig.DbConfig.Username;
                    this.Password = DatabaseConfig.DbConfig.Password;
                    this.DbName = DatabaseConfig.DbConfig.DbName;
                    break;
                case "Oracle":
                    this.UseOracleServerEngine = true;
                    this.Hostname = DatabaseConfig.DbConfig.Hostname;
                    this.User = DatabaseConfig.DbConfig.Username;
                    this.Password = DatabaseConfig.DbConfig.Password;
                    this.DbName = DatabaseConfig.DbConfig.DbName;
                    this.SID = DatabaseConfig.DbConfig.SID;
                    break;

                default:
                    this.UseSQLiteEngine = true;
                    this.HostnameSQLite = UsedConfigPath + "eBalanceKit.db3";
                    break;
            }
            ProxyConfig.Host = DatabaseConfig.ProxyConfig.Host;
            ProxyConfig.Port = DatabaseConfig.ProxyConfig.Port;
            ProxyConfig.Username = DatabaseConfig.ProxyConfig.Username;
            ProxyConfig.Password = DatabaseConfig.ProxyConfig.Password;
            this.BackupDirectory = DatabaseConfig.BackupDirectory;
        }

        public void SaveConfig() {
            StreamWriter writer = null;
            try {
                writer = new StreamWriter(UsedConfigPath + ConfigFile);

                if (UseSQLiteEngine) {
                    writer.WriteLine("dbtype=SQLite");
                    writer.WriteLine("dbpassword=" + StringUtils.EncryptString(DbPasswordSQLite, ConfigPassword));
                    writer.WriteLine("dbhost=" + HostnameSQLite);

                } else if (UseMySQLEngine) {
                    writer.WriteLine("dbtype=MySQL");
                    if (Password != null) {
                        writer.WriteLine("dbpassword=" + StringUtils.EncryptString(Password, ConfigPassword));
                    }
                    writer.WriteLine("dbhost=" + Hostname);
                    writer.WriteLine("dbuser=" + User);
                    writer.WriteLine("dbname=" + DbName);

                } else if (UseSQLServerEngine) {
                    writer.WriteLine("dbtype=SQLServer");
                    if (Password != null) {
                        writer.WriteLine("dbpassword=" + StringUtils.EncryptString(Password, ConfigPassword));
                    }
                    writer.WriteLine("dbhost=" + Hostname);
                    writer.WriteLine("dbuser=" + User);
                    writer.WriteLine("dbname=" + DbName);

                } else if (UseOracleServerEngine) {
                    writer.WriteLine("dbtype=Oracle");
                    if (Password != null) {
                        writer.WriteLine("dbpassword=" + StringUtils.EncryptString(Password, ConfigPassword));
                    }
                    writer.WriteLine("dbhost=" + Hostname);
                    writer.WriteLine("dbuser=" + User);
                    writer.WriteLine("dbname=" + User);
                    writer.WriteLine("sid="+SID);

                } else {
                    throw new Exception("Unknown database interface!");
                }             
                
                writer.WriteLine("proxy_host=" + this.ProxyConfig.Host);
                writer.WriteLine("proxy_port=" + this.ProxyConfig.Port);
                writer.WriteLine("proxy_user=" + this.ProxyConfig.Username);
                writer.WriteLine("proxy_password=" + StringUtils.EncryptString(this.ProxyConfig.Password, ConfigPassword));
                writer.WriteLine("backup_directory=" + this.BackupDirectory);

            } finally {
                if (writer != null) writer.Close();
            }
        }

        internal void TestConnection() {
            DbConfig config = GetDbConfig();
            if(config.DbType!="SQLServer")
                config.DbName = null;
            else config.DbName = "";
            using (var conn = DbAccess.ConnectionManager.CreateConnection(config)) {
                conn.Open();
            }
        }

        public DbConfig GetDbConfig() {
            // write config parts which are persistated into the database
            DbConfig config;
            if (this.UseSQLiteEngine) {
                config = new DbConfig("SQLite");
                config.DbName = "Main";
                config.Hostname = this.HostnameSQLite;
                config.Password = "4264g27S78HWP637JnS7RbH72fTjLsd";

            } else if (this.UseMySQLEngine) {
                config = new DbConfig("MySQL");
                config.DbName = this.DbName;
                config.Hostname = this.Hostname;
                config.Password = this.Password;

            } else if (UseSQLServerEngine) {
                config = new DbConfig("SQLServer");
                config.DbName = this.DbName;
                config.Hostname = this.Hostname;
                config.Password = this.Password;
            } else if (UseOracleServerEngine) {
                config = new DbConfig("Oracle")
                {DbName = this.User, Hostname = this.Hostname, Password = this.Password , SID= this.SID};
            } else
                throw new NotImplementedException();

            config.Username = this.User;
            return config;
        }

        public Window Owner { get; set; }

        public string BackupDirectory { get; private set; }
    }
}
