

using System.ComponentModel;

namespace ViewboxAdmin_ViewModel.Structures.Config {
    public interface IApplicationConfig :INotifyPropertyChanged  {
        /// <summary>
        /// Gets or sets the config location.
        /// </summary>
        /// <value>The config location.</value>
        ConfigLocation ConfigLocationType { get; set; }

        /// <summary>
        /// Gets or sets the last selected user.
        /// </summary>
        /// <value>The last selected user.</value>
        string LastUser { get; set; }

        /// <summary>
        /// Gets or sets the last profile.
        /// </summary>
        /// <value>The last profile.</value>
        string LastProfile { get; set; }

        /// <summary>
        /// Gets the config directory.
        /// </summary>
        /// <value>The config directory.</value>
        string ConfigDirectory { get; set; }
    }
}
