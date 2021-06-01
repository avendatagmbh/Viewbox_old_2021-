// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-01-09
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
namespace Config.Interfaces.Config {
    public interface ISqlExportConfig : IConfig {
        string Folder { get; set; }
    }
}