// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-01-09
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.ComponentModel;
using Config.DbStructure;
using Config.Enums;
using Config.Interfaces.Config;

namespace Config.Interfaces.DbStructure {
    public interface IOutputConfig {
        event PropertyChangedEventHandler PropertyChanged;
        /// <summary>
        /// Type of the config.
        /// </summary>
        OutputConfigTypes Type { get; set; }

        IConfig Config { get; set; }

        void Save();
    }
}