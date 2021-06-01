// --------------------------------------------------------------------------------
// author: Benjamin Held
// since: 2011-11-16
// copyright 2011 AvenDATA GmbH
// refactored and unit tested by Attila Papp
// --------------------------------------------------------------------------------
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using SystemDb;
using SystemDb.Upgrader;
using DbAccess.Structures;


namespace ViewboxAdmin_ViewModel.Structures.Config {
    /// <summary>
    /// Profile configuration.
    /// </summary>
    public class Profile : NotifyBase, IProfile
    {
        #region Constructors
        public Profile(ISystemDb systemdb, IDbConfig dbconfig ) {
            this.SystemDb = systemdb;
            this.DbConfig = dbconfig;
        }
        #endregion Constructors

        #region Properties
        private string _name;
        /// <summary>
        /// Name of the profile
        /// </summary>
        public string Name {
            get { return _name; }
            set {
                if (_name != value) {
                    ValidateName(value);
                    _name = value;
                    OnPropertyChanged("Name");
                }
            }
        }

        private string _description;
        /// <summary>
        /// contains general description about the profile
        /// </summary>
        public string Description {
            get { return _description; }
            set {
                if (_description != value) {
                    _description = value;
                    OnPropertyChanged("Description");
                }
            }
        }

        

        #region DbConfig
        private IDbConfig _dbconfig;

        public IDbConfig DbConfig {
            get { return _dbconfig; }
            set {
                if (_dbconfig != value) {
                    _dbconfig = value;
                    OnPropertyChanged("DbConfig");
                }
            }
        }
        #endregion DbConfig


        public ISystemDb SystemDb { get; private set; }

        public string DisplayString { 
            get {
                if(IsLoading)
                    return Name + " (is loading)";
                if(DatabaseOutOfDateInformation != null)
                    return Name + " (Database is not up-to-date)";
                return Name;
            } 
        }

        private bool _loaded;
        public bool Loaded {
            get { return _loaded; }
            set {
                if (_loaded != value) {
                    _loaded = value;
                    OnPropertyChanged("Loaded");
                    OnPropertyChanged("DisplayString");
                }
            }
        }
        
        private volatile bool _isLoading;
        public bool IsLoading {
            get { return _isLoading; }
            set {
                if (_isLoading != value) {
                    _isLoading = value;
                    OnPropertyChanged("IsLoading");
                    OnPropertyChanged("DisplayString");
                }
            }
        }
        
        private IDatabaseOutOfDateInformation _databaseOutOfDateInformation;
        public IDatabaseOutOfDateInformation DatabaseOutOfDateInformation {
            get { return _databaseOutOfDateInformation; }
            set {
                _databaseOutOfDateInformation = value;
                OnPropertyChanged("DatabaseOutOfDateInformation");
                OnPropertyChanged("DisplayString");
            }
        }

#endregion Properties

        #region Public methods

        private object _lock = new object();

        public void Load() {
            lock (_lock) {
                if (Loaded || IsLoading) return;
                IsLoading = true;
                try {
                    SystemDb.LoadingFinished += () => {
                        IsLoading = false;};
                    SystemDb.DataBaseUpGradeChecked += () => {
                        DatabaseOutOfDateInformation = SystemDb.DatabaseOutOfDateInformation; 
                        //Loaded = DatabaseOutOfDateInformation == null;
                    };
                    SystemDb.Connect(DbConfig.DbType,DbConfig.ConnectionString,20);
                    
                }
                    catch {
                        Loaded = false;
                        IsLoading = false;
                        throw;
                    }
                
            }
        }

        public void UpgradeDatabase() {
            if (DatabaseOutOfDateInformation != null) {
                    SystemDb.UpgradeDatabase(SystemDb.DB);
                    DatabaseOutOfDateInformation = SystemDb.DatabaseOutOfDateInformation;
                IsLoading = false;
                Loaded = false;
            }
            }

        #endregion Public methods

        #region Private methods
        /// <summary>
        /// validating the name if contains invalid characters.  
        /// </summary>
        /// <param name="name">the name to validate</param>
        private void ValidateName(string name) {
            // a tight coupled way of validation...
            foreach (var character in Path.GetInvalidFileNameChars())
            {
                if (name.Contains(character))
                    throw new ArgumentException("Der Name darf keines der folgenden Zeichen enthalten: " +
                                                string.Join("", Path.GetInvalidPathChars()));
            }
        }
        #endregion Private methods
    }
}
