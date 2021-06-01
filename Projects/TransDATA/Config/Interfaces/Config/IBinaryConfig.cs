// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-01-12
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
namespace Config.Interfaces.Config {
    public interface IBinaryConfig : IConfig {
        string Folder { get; set; }    
    }
}