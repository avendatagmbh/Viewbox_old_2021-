/**************************************************************************************************************
 * author               date            comment
 * ------------------------------------------------------------------------------------------------------------
 * Mirko Dibbert        2010-10-16      initial implementation
 *************************************************************************************************************/

using System.ComponentModel;

namespace ViewValidatorLogic.Config {

    /// <summary>
    /// Enumeration of all avaliable config locations.
    /// </summary>
    public enum ConfigLocation {

        /// <summary>
        /// Use directory based config storage.
        /// </summary>
        Directory,

        /// <summary>
        /// Use database base config storage.
        /// </summary>
        Database
    }

    /// <summary>
    /// Config class for all global application options.
    /// </summary>
    public class ApplicationConfig : INotifyPropertyChanged {

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationConfig"/> class.
        /// </summary>
        public ApplicationConfig() {
            this.ConfigLocation = Config.ConfigLocation.Directory;
            this.LastUser = string.Empty;
            this.LastProfile = string.Empty;
            this.ConfigDirectory = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments) + "\\AvenDATA\\ViewValidator";
            //this.ConfigDbConfig = new ConfigDatabase();
        }

        /*****************************************************************************************************/

        #region events

        /// <summary>
        /// Occurs when a property changed
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion events

        /*****************************************************************************************************/

        #region eventTrigger

        /// <summary>
        /// Called when a property changed.
        /// </summary>
        /// <param name="property">The property.</param>
        private void OnPropertyChanged(string property) {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(property));
        }

        #endregion eventTrigger

        /*****************************************************************************************************/

        #region member varables

        /// <summary>
        /// See property ConfigDirectory.
        /// </summary>
        private string _configDirectory;

        /// <summary>
        /// See property LastUser.
        /// </summary>
        private string _lastUser;

        /// <summary>
        /// See property LastProfile.
        /// </summary>
        private string _lastProfile;

        #endregion

        /*****************************************************************************************************/

        #region properties

        /// <summary>
        /// Gets or sets the config location.
        /// </summary>
        /// <value>The config location.</value>
        public ConfigLocation ConfigLocation { get; set; }

        /// <summary>
        /// Gets or sets the last selected user.
        /// </summary>
        /// <value>The last selected user.</value>
        public string LastUser {
            get { return _lastUser; }
            set {
                if (_lastUser != value) {
                    _lastUser = value;
                    OnPropertyChanged("LastUser");
                }
            }
        }

        /// <summary>
        /// Gets or sets the last profile.
        /// </summary>
        /// <value>The last profile.</value>
        public string LastProfile {
            get { return _lastProfile; }
            set {
                if (_lastProfile != value) {
                    _lastProfile = value;
                    OnPropertyChanged("LastProfile");
                }
            }
        }

        public string LastTableMapping { get; set; }

        /// <summary>
        /// Gets the config directory.
        /// </summary>
        /// <value>The config directory.</value>
        public string ConfigDirectory {
            get { return _configDirectory; }
            set {
                if (_configDirectory != value.Trim()) {
                    _configDirectory = value.Trim();
                    if (!System.IO.Directory.Exists(_configDirectory)) {
                        System.IO.Directory.CreateDirectory(_configDirectory);
                    }
                }
            }
        }

        public string LastExcelFile { get; set; }
        //public bool AutomaticLogin { get; set; }

        #endregion properties
    }
}
