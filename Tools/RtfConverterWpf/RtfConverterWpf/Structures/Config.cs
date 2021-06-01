using System.Xml.Serialization;

namespace RtfConverterWpf.Structures {

    [XmlRoot("Config", Namespace = "", IsNullable = false)]
    public class Config {

        #region Properties
        public string Hostname { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string DbName { get; set; }
        public string TargetDbName { get; set; }
        #endregion Properties
    }
}
