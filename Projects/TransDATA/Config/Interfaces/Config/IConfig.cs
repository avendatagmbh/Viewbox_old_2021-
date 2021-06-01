// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-01-09
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.ComponentModel;

namespace Config.Interfaces.Config {
    /// <summary>
    /// Base interface for all import/export configs.
    /// </summary>
    public interface IConfig : INotifyPropertyChanged {
        event EventHandler ForceDataUpdate;
        string GetXmlRepresentation();
        bool Validate(out string error);
    }
}