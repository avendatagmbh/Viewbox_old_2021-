// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-01-09
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using Config.Enums;
using Config.Interfaces.Config;
using Config.Interfaces.DbStructure;
using Config.Config;
using DbAccess;
using Utils;

namespace Config.DbStructure {
    [DbTable("output_config")]
    public class OutputConfig : NotifyPropertyChangedBase, IOutputConfig {

        /// <summary>
        /// This constructor should only called from DbMapping using reflection.
        /// </summary>
        public OutputConfig() { }
        
        public OutputConfig(OutputConfigTypes type) { Type = type; }

        #region Id
        [DbColumn("id", AllowDbNull = false)]
        [DbPrimaryKey]
        public int Id { get; set; }
        #endregion

        #region Type
        private OutputConfigTypes _type;

        [DbColumn("type")]
        public OutputConfigTypes Type {
            get { return _type; }
            set {
                _type = value;
                switch (Type) {
                    case OutputConfigTypes.Database:
                        Config = new DatabaseOutputConfig();
                        break;

                    case OutputConfigTypes.Gdpdu:
                        Config = new GdpduOutputConfig();
                        break;
                    
                    case OutputConfigTypes.Csv:
                        Config = new CsvOutputConfig();
                        break;
                    
                    //case OutputConfigTypes.Sql:
                    //    Config = new SqlExportConfig();
                    //    break;
                    
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                OnPropertyChanged("Type");
            }
        }
        #endregion Type

        #region ConfigData
        private string _configData;

        /// <summary>
        /// XML representation of the config.
        /// </summary>
        [DbColumn("data")]
        private string ConfigData {
            get { return _configData; }
            set {
                _configData = value;
                _config = ConfigBase.GetOutputConfig(Type, _configData);
                _config.PropertyChanged += (sender, args) => UpdateConfigData();
            }
        }
        #endregion 

        #region Config
        private IConfig _config;

        public IConfig Config {
            get { return _config; }
            set {
                _config = value;
                _config.PropertyChanged += (sender, args) => UpdateConfigData();
                UpdateConfigData();
                OnPropertyChanged("Config");
            }
        }

        #endregion

        internal bool DoDbUpdate { get; set; }

        private void UpdateConfigData() {
            _configData = Config.GetXmlRepresentation();
            //Save();
        }

        #region Save
        public void Save() {
            if (DoDbUpdate) {
                (Config as ConfigBase).OnForceDataUpdate();
                ConfigDb.Save(this);
            }
        }
        #endregion Save

    }
}