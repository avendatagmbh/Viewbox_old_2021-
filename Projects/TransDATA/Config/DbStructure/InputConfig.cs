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
    [DbTable("Input_config")]
    public class InputConfig : NotifyPropertyChangedBase, IInputConfig {

        /// <summary>
        /// This constructor should only called from DbMapping using reflection.
        /// </summary>
        public InputConfig() { }

        public InputConfig(InputConfigTypes type) { Type = type; }

        #region Id
        [DbColumn("id", AllowDbNull = false)]
        [DbPrimaryKey]
        public int Id { get; set; }
        #endregion

        #region Type
        private InputConfigTypes _type;

        [DbColumn("type")]
        public InputConfigTypes Type {
            get { return _type; }
            set {
                _type = value;
                switch (_type) {
                    case InputConfigTypes.Database:
                        Config = new DatabaseInputConfig();
                        break;
                    
                    //case InputConfigTypes.Binary:
                    //    Config = new BinaryConfig();
                    //    break;
                    
                    case InputConfigTypes.Csv:
                        Config = new CsvInputConfig();
                        break;
                    
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                OnPropertyChanged("Type");
                //OnPropertyChanged("");
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
                Config = ConfigBase.GetInputConfig(Type, _configData);
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

        public bool Validate(out string error) {
            //error = string.Empty;
            return Config.Validate(out error);

            //return false;
        }
        #endregion Save
    }
}