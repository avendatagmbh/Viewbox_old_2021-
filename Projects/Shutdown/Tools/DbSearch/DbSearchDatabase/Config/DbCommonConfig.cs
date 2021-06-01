// --------------------------------------------------------------------------------
// author: Benjamin Held
// since: 2011-09-09
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------

using System.ComponentModel;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;
using DbAccess;
using DbAccess.Structures;
using DbSearchDatabase.Structures;

namespace DbSearchDatabase.Config {
    [DataContract]
    public class ConfigData {
        public ConfigData() {
            //DbConfigValidation = new DbConfig("Access");
            DbConfigView = new DbConfig("MySQL") { Hostname = "localhost", Username = "root", Password = "avendata" };
        }
        //[DataMember]
        //public DbConfig DbConfigValidation { get; set; }
        [DataMember]
        public DbConfig DbConfigView { get; set; }
    }

    [DbTable("common_config", ForceInnoDb = true)]
    [DataContract(Name="CommonConfig")]
    public class DbCommonConfig : DatabaseObjectBase<int>{

        #region Contructor
        public DbCommonConfig() {
            Init();
        }

        private void Init() {
            ConfigData = new ConfigData();
        }

        //void  DbConfigValidation_PropertyChanged(object sender, PropertyChangedEventArgs e) {
        //    OnPropertyChanged("DbConfigValidation");
        //}

        internal DbCommonConfig(DbProfile dbProfile) {
            DbProfile = dbProfile;
            Init();
        }
        #endregion

        #region Properties
        #region ConfigData
        private ConfigData _configData;
        public ConfigData ConfigData {
            get { return _configData; }
            set {
                if (_configData != value) {
                    _configData = value;
                    //DbConfigValidation.PropertyChanged += new PropertyChangedEventHandler(DbConfigValidation_PropertyChanged);
                }
            }
        }
        #endregion

        //public DbConfig DbConfigValidation { get { return ConfigData.DbConfigValidation; } set { ConfigData.DbConfigValidation = value; } }
        public DbConfig DbConfigView { get { return ConfigData.DbConfigView; } set { ConfigData.DbConfigView = value; } }

        [DbColumn("profile_id", IsInverseMapping = true)]
        public DbProfile DbProfile { get; set; }

        [DbColumn("xml_config", AllowDbNull = true, Length = 65536)]
        public string XmlConfig {
            get {
                DataContractSerializer serializer = new DataContractSerializer(typeof(ConfigData));
                StringWriter stringWriter = new StringWriter();
                XmlTextWriter xmlWriter = new XmlTextWriter(stringWriter);

                serializer.WriteObject(xmlWriter, ConfigData);

                return stringWriter.ToString();
            }
            set {

                if (string.IsNullOrEmpty(value)) return;
                XmlTextReader reader = new XmlTextReader(new StringReader(value));
                DataContractSerializer ser = new DataContractSerializer(typeof(ConfigData));
                ConfigData = (ConfigData)ser.ReadObject(reader);
            }
        }

        #endregion
    }
}
