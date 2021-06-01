using System;
using System.ComponentModel;
using System.IO;
using DbAccess.Structures;

namespace ViewBuilderBusiness.Structures.Config
{
    /// <summary>
    ///   Enumeration of all avaliable config locations.
    /// </summary>
    public enum ConfigLocation
    {
        /// <summary>
        ///   Use directory based config storage.
        /// </summary>
        Directory,

        /// <summary>
        ///   Use database base config storage.
        /// </summary>
        Database
    }

    /// <summary>
    ///   Config class for all global application options.
    /// </summary>
    public class ApplicationConfig : INotifyPropertyChanged
    {
        /// <summary>
        ///   Initializes a new instance of the <see cref="ApplicationConfig" /> class.
        /// </summary>
        public ApplicationConfig()
        {
            ConfigLocation = ConfigLocation.Directory;
            LastUser = string.Empty;
            LastProfile = string.Empty;
            ConfigDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) +
                              "\\AvenDATA\\ViewBuilder";
            ConfigDbConfig = new DbConfig("MySQL");
            SmtpServer = new SmtpServerConfig();
        }

        #region events

        /// <summary>
        ///   Occurs when a property changed
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion events

        #region eventTrigger

        /// <summary>
        ///   Called when a property changed.
        /// </summary>
        /// <param name="property"> The property. </param>
        private void OnPropertyChanged(string property)
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(property));
        }

        #endregion eventTrigger

        #region member varables

        /// <summary>
        ///   See property ConfigDirectory.
        /// </summary>
        private string _configDirectory;

        /// <summary>
        ///   See property LastProfile.
        /// </summary>
        private string _lastProfile;

        /// <summary>
        ///   See property LastUser.
        /// </summary>
        private string _lastUser;

        #endregion

        #region properties

        /// <summary>
        ///   Gets or sets the config location.
        /// </summary>
        /// <value> The config location. </value>
        public ConfigLocation ConfigLocation { get; set; }

        /// <summary>
        ///   Gets or sets the last selected user.
        /// </summary>
        /// <value> The last selected user. </value>
        public string LastUser
        {
            get { return _lastUser; }
            set
            {
                if (_lastUser != value)
                {
                    _lastUser = value;
                    OnPropertyChanged("LastUser");
                }
            }
        }

        /// <summary>
        ///   Gets or sets the last profile.
        /// </summary>
        /// <value> The last profile. </value>
        public string LastProfile
        {
            get { return _lastProfile; }
            set
            {
                if (_lastProfile != value)
                {
                    _lastProfile = value;
                    OnPropertyChanged("LastProfile");
                }
            }
        }

        /// <summary>
        ///   Gets the config directory.
        /// </summary>
        /// <value> The config directory. </value>
        public string ConfigDirectory
        {
            get { return _configDirectory; }
            set
            {
                if (_configDirectory != value.Trim())
                {
                    _configDirectory = value.Trim();
                    if (!Directory.Exists(_configDirectory))
                    {
                        Directory.CreateDirectory(_configDirectory);
                    }
                }
            }
        }

        /// <summary>
        ///   Gets or sets the config db config.
        /// </summary>
        /// <value> The config db config. </value>
        public DbConfig ConfigDbConfig { get; set; }

        /// <summary>
        ///   Gets or sets the SMTP server config.
        /// </summary>
        /// <value> The SMTP server config. </value>
        public SmtpServerConfig SmtpServer { get; set; }

        #endregion properties

        /*****************************************************************************************************/

        /*****************************************************************************************************/

        /*****************************************************************************************************/

        /*****************************************************************************************************/
    }
}