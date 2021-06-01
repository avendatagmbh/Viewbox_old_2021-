// --------------------------------------------------------------------------------
// author: Benjamin Held
// since: 2011-09-08
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------

using DbSearchBase.Interfaces;

namespace DbSearchLogic.Config {

    /// <summary>
    /// Config class for all global application options.
    /// </summary>
    public class ApplicationConfig : IApplicationConfig {

        public ApplicationConfig() {
            this.LastProfile = string.Empty;
            LastQueryTable = string.Empty;
        }

        #region properties
        public string LastProfile { get; set; }

        public string LastQueryTable { get; set; }

        #endregion properties
    }
}
