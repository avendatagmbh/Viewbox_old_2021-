/**************************************************************************************************************
 * author               date            comment
 * ------------------------------------------------------------------------------------------------------------
 * Attila Papp        2012-09-03        refactored legacy code from Mirko, fully tested
 *************************************************************************************************************/

using System.ComponentModel;
using ViewboxAdminBusiness.Manager;

namespace ViewboxAdmin_ViewModel.Structures.Config {
    /// <summary>
    /// Config class for all global application options.
    /// </summary>
    public class ApplicationConfig : IApplicationConfig {

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationConfig"/> class.
        /// </summary>
        public ApplicationConfig(IFileManager filemanager) {
            this._filemanager = filemanager;
            this.ConfigLocationType = ConfigLocation.Directory;
            this.LastUser = string.Empty;
            this.LastProfile = string.Empty;
            this.ConfigDirectory = FileManager.GetFolderPath(System.Environment.SpecialFolder.MyDocuments) + relativepath;
        }

        private readonly string relativepath = "\\AvenDATA\\ViewboxAdmin";

        private IFileManager _filemanager = null;

        public IFileManager FileManager { get { return _filemanager; } }

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
        public ConfigLocation ConfigLocationType { get; set; }

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

        /// <summary>
        /// Gets the config directory.
        /// </summary>
        /// <value>The config directory.</value>
        public string ConfigDirectory {
            get { return _configDirectory; }
            set {
                _configDirectory = value.Trim();
                if (!FileManager.DirectoryExist(_configDirectory)) {
                        FileManager.CreateDirectory(_configDirectory);
                    }
            }
        }

        #endregion properties
    }
}
