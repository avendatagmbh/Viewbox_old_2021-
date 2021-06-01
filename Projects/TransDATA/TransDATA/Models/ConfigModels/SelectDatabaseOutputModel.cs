// --------------------------------------------------------------------------------
// author: Marcus Gerlach
// since: 2012-01-19
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------

using System;
using System.Linq;
using System.Windows;
using AV.Log;
using Base.Localisation;
using Business;
using Config.Interfaces.Config;
using Config.Interfaces.DbStructure;
using DbAccess;
using DbAccess.Structures;
using Utils;
using log4net;

namespace TransDATA.Models.ConfigModels {
    public class SelectDatabaseOutputModel : NotifyPropertyChangedBase {
        internal ILog _log = LogHelper.GetLogger();

        public SelectDatabaseOutputModel(IProfile profile) {
            _profile = profile;
            _profile.OutputConfig.PropertyChanged += OutputConfig_PropertyChanged;
            OutputConfig = profile.OutputConfig.Config as IDatabaseOutputConfig;
            
            IDbTemplate mysqlTemplate = AppController.DbTemplates.FirstOrDefault((template) => template.Filename != null && template.Filename.ToLower().Contains("mysql"));
            if(mysqlTemplate == null) {
                InvalidOperationException ex = new InvalidOperationException("MySQL Template is missing");
                using (NDC.Push(LogHelper.GetNamespaceContext()))
                {
                    _log.Error(ex.Message, ex);
                }

                throw ex;
            }
            
            _connectionStringBuilder.SetTemplateParams(mysqlTemplate.Params);
            if(OutputConfig != null)
                _connectionStringBuilder.ConnectionString = OutputConfig.ConnectionString;
            _connectionStringBuilder.Driver = "";
            _connectionStringBuilder.PropertyChanged += _connectionStringBuilder_PropertyChanged;
        }

        void _connectionStringBuilder_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            if (_outputConfig != null)
                _outputConfig.ConnectionString = ConnectionStringBuilder.ConnectionString;
        }

        void OutputConfig_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            if(e.PropertyName == "Config")
                OutputConfig = _profile.OutputConfig.Config as IDatabaseOutputConfig;
        }

        //void _connectionStringBuilder_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) { OutputConfig.ConnectionString = _connectionStringBuilder.ConnectionString; }
        #region Properties
        #region OutputConfig
        private IDatabaseOutputConfig _outputConfig;

        public IDatabaseOutputConfig OutputConfig {
            get { return _outputConfig; }
            set {
                if (_outputConfig != value) {
                    _outputConfig = value;
                    if(value != null)
                        value.ForceDataUpdate += (sender, e) => { OutputConfig.ConnectionString = ConnectionStringBuilder.ConnectionString;  };
                    OnPropertyChanged("OutputConfig");
                }
            }
        }
        #endregion OutputConfig

        private readonly IProfile _profile;
        #region ConnectionStringBuilder
        private readonly IConnectionStringBuilder _connectionStringBuilder = ConnectionStringBuilderFactory.GetConnectionStringBuilder("GenericODBC");

        public IConnectionStringBuilder ConnectionStringBuilder {
            get { return _connectionStringBuilder; }
        }

        #endregion

        #region TestDbConnection
        public void TestDbConnection(Window owner) {
            string error;
            DbConfig _config = null;

            if (((IDatabaseOutputConfig)_profile.OutputConfig.Config).IsMsSql)
            {
                _config=new DbConfig("SQLServer") { ConnectionString = _connectionStringBuilder.ConnectionString, DbName = "" };
            }
            else
            {
                _config=new DbConfig("MySQL") { ConnectionString = _connectionStringBuilder.ConnectionString, DbName = "" };
            }
            if (ConnectionManager.TestDbConnection(_config, out error))
                MessageBox.Show(owner, ResourcesCommon.DatabaseConnectionOk,
                                ResourcesCommon.ConnectionSucceed, MessageBoxButton.OK, MessageBoxImage.Information);
            else
                MessageBox.Show(owner, error, ResourcesCommon.ConnectionFailed, MessageBoxButton.OK, MessageBoxImage.Exclamation);
            
        }
        #endregion TestDbConnection
        #endregion Properties
    }
}