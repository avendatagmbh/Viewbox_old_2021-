// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-01-09
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using DbAccess;

namespace Config.Interfaces.Config {
    public interface IGdpduConfig : IConfig {
        string XmlName { get; set; }
        string XmlLocation { get; set; }
        string XmlComment { get; set; }
        string Folder { get; set; }
    }
}