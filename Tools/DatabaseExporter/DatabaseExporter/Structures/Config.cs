using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace DatabaseExporter.Structures {

    [XmlRoot("Config", Namespace = "", IsNullable = false)]
    public class Config {

        #region Properties
        public string Hostname { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string DbName { get; set; }
        public string OutputDir { get; set; }
        #endregion Properties

        #region Methods
        #endregion Methods
    }
}
