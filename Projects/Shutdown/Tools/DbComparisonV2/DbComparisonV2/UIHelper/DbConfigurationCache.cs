using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DbComparisonV2.Models;
using System.IO;
using System.Xml;
using System.Runtime.Serialization.Formatters.Binary;
using System.ComponentModel;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using DBComparisonBusiness;
using DBComparisonBusiness.Business;
using DBComparisonV2.UIHelper;

namespace DbComparisonV2.UIHelper
{
    [DataContract]
    public class DbConfigurationCache : INotifyPropertyChanged
    {
        #region instance members
        public event PropertyChangedEventHandler PropertyChanged;
        #region fields
        List<DatabaseCompareConfig> _dbConfigs;
        #endregion
        #region properties
        [DataMember]
        public List<DatabaseCompareConfig> DbConfigs
        {
            get { return _dbConfigs; }
            set { 
                    _dbConfigs = value; 
            }
        }
        #endregion
        #region constructors
        public DbConfigurationCache()
        {
            _dbConfigs = new List<DatabaseCompareConfig>();
            _filePath = AppDomain.CurrentDomain.BaseDirectory + "dbcomparison_cache.xml";
        }
        #endregion
        #region methods
        private void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        /// <summary>
        /// FIFO list, max 3 entries
        /// </summary>
        /// <param name="comparConfig"></param>
        private void Add(DatabaseCompareConfig comparConfig)
        {
            var queue = new Queue<DatabaseCompareConfig>(_dbConfigs);
            if (queue.Count >= 3) queue.Dequeue();
            queue.Enqueue(comparConfig);
            _dbConfigs = queue.ToList();
        }
        #endregion
        #endregion

        #region class members
        static DbConfigurationCache _configCache;
        static string _filePath = "";

        #region class methods
        private static void InitializeConfigurations() 
        {
            _configCache = DesirializeConfigs();
        }
        public static  List<DatabaseCompareConfig> GetConfigurations() 
        {
            if (_configCache == null)
            {
                InitializeConfigurations();
            }
            return _configCache != null ? _configCache.DbConfigs : null;
        }

        private static DbConfigurationCache DecryptPasswordsInConfigs(DbConfigurationCache configCache)
        {
            if (configCache == null) return null;
            configCache.DbConfigs.ForEach(c =>
            {
                c.Object1.Password = EncryptionHelper.DecryptString(c.Object1.Password);
                c.Object2.Password = EncryptionHelper.DecryptString(c.Object2.Password);
            });
            return configCache;
        }

        private static DbConfigurationCache EncryptPasswordsInConfigs(DbConfigurationCache configCache)
        {
            if (configCache == null) return null;

            configCache.DbConfigs.ForEach(c =>
            {
                c.Object1.Password = EncryptionHelper.EncryptData(c.Object1.Password);
                c.Object2.Password = EncryptionHelper.EncryptData(c.Object2.Password);
            });
            return configCache;
        }
        /// <summary>
        /// returns true if the config is saved (new config only)
        /// </summary>
        /// <param name="comparConfig"></param>
        /// <returns></returns>
        public static bool SaveOrCreateConfiguration(DatabaseCompareConfig comparConfig)
        {
            if (comparConfig == null || string.IsNullOrEmpty(comparConfig.ConfigName)) throw new ArgumentNullException("DatabaseComparerConfig arg is null.");
            
            // check first if there is a config file before we create a new one
            _configCache = DesirializeConfigs();
            
            // skip if the config already exists
            if (_configCache != null && _configCache.DbConfigs.Find(c => c.ConfigName == comparConfig.ConfigName) != null)
            {
                return false;
            }
            if (_configCache == null) _configCache = new DbConfigurationCache();

            _configCache.Add(comparConfig);

            SerializeConfigurationCache(_configCache);
            InitializeConfigurations(); // refresh the configs (descrypts PWD on the way)
            return true;
        }

        private static void SerializeConfigurationCache(DbConfigurationCache configCache)
        {
            using (Stream stream = File.Open(_filePath, FileMode.OpenOrCreate))
            {
                DataContractSerializer serializer = new DataContractSerializer(typeof(DbConfigurationCache));
                EncryptPasswordsInConfigs(configCache);
                serializer.WriteObject(stream, configCache);
            }
        }
        private static  DbConfigurationCache DesirializeConfigs()
        {
            if (!File.Exists(_filePath)) return null;
            using (var stream = File.Open(_filePath, FileMode.Open))
            {
                DataContractSerializer serializer = new DataContractSerializer(typeof(DbConfigurationCache));
                return DecryptPasswordsInConfigs(serializer.ReadObject(stream) as DbConfigurationCache);
            }
        }
        #endregion
#endregion

    }
}
