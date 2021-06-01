// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-09-12
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Base.Localisation;
using Business;
using Config.Interfaces.Config;
using Config.Interfaces.DbStructure;
using DbAccess;
using DbAccess.Structures;
using Utils;

namespace TransDATA.Models.ConfigModels {
    public class SelectDatabaseInputModel : NotifyPropertyChangedBase {
        public SelectDatabaseInputModel(IProfile profile) {
            if (!(profile.InputConfig.Config is IDatabaseInputConfig))
                return;
            _profile = profile;
            profile.InputConfig.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(InputConfig_PropertyChanged);
            // init driver list
            DriverList = new ObservableCollection<string>(ConnectionManager.GetOdbcDrivers());

            // init dsn list
            var tmp = new List<string>(ConnectionManager.GetUserDsnList());
            foreach (string dsn in ConnectionManager.GetSystemDsnList()) if (!tmp.Contains(dsn)) tmp.Add(dsn);
            tmp.Sort();

            foreach (string dsn in tmp) DsnList.Add(dsn);

            InputConfig = profile.InputConfig.Config as IDatabaseInputConfig;

            Templates = AppController.DbTemplates.ToList();

            foreach (var template in Templates) {
                if (template.Filename == null)
                    continue; // ignore default template
                if (template.Filename == InputConfig.DbTemplateName) {
                    SelectedDbTemplate = template;
                    break;
                }
            }

            if (SelectedDbTemplate == null)
                SelectedDbTemplate = Templates[0];

            ConnectionStringBuilder.ConnectionString = InputConfig.ConnectionString;
            IsDriverSelected = !string.IsNullOrEmpty(SelectedDriver);
            //IsDriverSelected = SelectedDriverIndex != -1;
            ConnectionStringBuilder.PropertyChanged += (sender,e) => InputConfig.ConnectionString = ConnectionStringBuilder.ConnectionString;
        }

        void InputConfig_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            if (e.PropertyName == "Config")
                InputConfig = _profile.InputConfig.Config as IDatabaseInputConfig;
        }


        #region Properties
        public List<IDbTemplate> Templates { get; private set; }

        #region Profile
        private IDatabaseInputConfig _inputConfig;
        private IDatabaseInputConfig InputConfig {
            get { return _inputConfig; } 
            set { _inputConfig = value;
                if(value != null)
                    value.ForceDataUpdate += (sender, e) => { UpdateProfileData(); };
                OnPropertyChanged("InputConfig");
            }
        }

        #endregion

        #region DriverList
        public ObservableCollection<string> DriverList { get; private set; }
        #endregion

        #region SelectedDriver
        public string SelectedDriver {
            get {
                // remove braces around driver name
                return _connectionStringBuilder.Driver != null ? (_connectionStringBuilder.Driver.Length > 2
                           ? _connectionStringBuilder.Driver.Substring(1, _connectionStringBuilder.Driver.Length - 2)
                           : _connectionStringBuilder.Driver) : "";

            }
            set {
                _connectionStringBuilder.Driver = value;
                OnPropertyChanged("SelectedDriver");
            }
        }
        #endregion

        #region DsnList
        private readonly ObservableCollection<string> _dsnList = new ObservableCollection<string>();

        public ObservableCollection<string> DsnList {
            get { return _dsnList; }
        }
        #endregion DsnList

        #region SelectedDsn
        public string SelectedDsn {
            get { return _connectionStringBuilder.Dsn; }
            set {
                _connectionStringBuilder.Dsn = value;
                OnPropertyChanged("SelectedDsn");
            }
        }
        #endregion

        #region UseCatalog
        public bool UseCatalog {
            get { return InputConfig.UseCatalog; }
            set {
                if (InputConfig.UseCatalog != value) {
                    InputConfig.UseCatalog = value;
                    OnPropertyChanged("UseCatalog");
                }
            }
        }
        #endregion UseCatalog

        #region UseSchema
        public bool UseSchema {
            get { return InputConfig.UseSchema; }
            set {
                if (InputConfig.UseSchema != value) {
                    InputConfig.UseSchema = value;
                    OnPropertyChanged("UseSchema");
                }
            }
        }
        #endregion UseSchema

        #region UseAdo
        public bool UseAdo {
            get { return InputConfig.UseAdo; }
            set {
                InputConfig.UseAdo = value;
                OnPropertyChanged("UseAdo");
            }
        }
        #endregion

        #region TableWhitelist
        public string TableWhitelist {
            get { return InputConfig.TableWhitelist; }
            set {
                InputConfig.TableWhitelist = value;
                OnPropertyChanged("TableWhitelist");
            }
        }
        #endregion TableWhitelist

        #region DatabaseWhitelist
        public string DatabaseWhitelist {
            get { return InputConfig.DatabaseWhitelist; }
            set {
                InputConfig.DatabaseWhitelist = value;
                OnPropertyChanged("DatabaseWhitelist");
            }
        }
        #endregion DatabaseWhitelist

        #region IsDriverSelected
        private bool _isDriverSelected;
        public bool IsDriverSelected {
            get { return _isDriverSelected; } 
            set {
                if (_isDriverSelected != value) {
                    _isDriverSelected = value;
                    if (_isDriverSelected)
                        SelectedDsn = string.Empty;
                    else
                        SelectedDriver = "";
                        //SelectedDriverIndex = -1;
                    OnPropertyChanged("IsDriverSelected");
                    //OnPropertyChanged("SelectedDriver");
                    //OnPropertyChanged("SelectedDsn");
                }
            }
        }
        #endregion

        #region ConnectionStringBuilder
        private readonly IConnectionStringBuilder _connectionStringBuilder = ConnectionStringBuilderFactory.GetConnectionStringBuilder("GenericODBC");

        public IConnectionStringBuilder ConnectionStringBuilder {
            get { return _connectionStringBuilder; }
        }
        #endregion

        #region UpdateProfileData
        public void UpdateProfileData() {
            InputConfig.ConnectionString = ConnectionStringBuilder.ConnectionString;
            InputConfig.DbTemplateName = SelectedDbTemplate.Filename;
        }
        #endregion UpdateConnectionString

        private IProfile _profile;

        #region SelectedDbTemplate
        private IDbTemplate _selectedDbTemplate;

        public IDbTemplate SelectedDbTemplate {
            get { return _selectedDbTemplate; }
            set {
                _selectedDbTemplate = value;
                _connectionStringBuilder.SetTemplateParams(_selectedDbTemplate.Params);
                OnPropertyChanged("SelectedDbTemplate");
            }
        }

        #endregion SelectedDbTemplate
        #endregion Properties

        #region Methods
        #region TestDbConnection
        public void TestDbConnection(Window owner) {
            string error;
            if (ConnectionManager.TestDbConnection(new DbConfig("GenericODBC") {ConnectionString = _connectionStringBuilder.ConnectionString}, out error))
                MessageBox.Show(owner, ResourcesCommon.DatabaseConnectionOk,
                                ResourcesCommon.ConnectionSucceed, MessageBoxButton.OK, MessageBoxImage.Information);
            else
                MessageBox.Show(owner, error, ResourcesCommon.ConnectionFailed, MessageBoxButton.OK, MessageBoxImage.Exclamation);
            
        }
        #endregion TestDbConnection

        #endregion Methods
    }
}