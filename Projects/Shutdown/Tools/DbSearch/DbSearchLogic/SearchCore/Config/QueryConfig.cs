using System.Xml.Serialization;
using System;
using System.IO;
using DbAccess.Structures;
using DbSearchLogic.SearchCore.Result;
using DbSearchLogic.SearchCore.SearchMatrix;
using DbSearchLogic.SearchCore.Structures;

namespace DbSearchLogic.SearchCore.Config {

    /// <summary>
    /// Konfigurationseinstellungen für die Suche.
    /// </summary>
    /// <author>Mirko Dibbert</author>
    /// <company>AvenDATA GmbH</company>
    /// <since>22.01.2010</since>
    public class QueryConfig  {
        public QueryConfig(DbConfig viewConfig, string name, SearchValueMatrix matrix) {
            this.Name = name;
            
            SearchValueMatrix = matrix;
            ViewConfig = viewConfig;
            this.Init();
        }

        /// <summary>
        /// Inits this instance.
        /// </summary>
        public void Init() {
            SearchParams = new ConfigSearchParams();

            this.State = new QueryState();
        }

        #region Properties
        public DbConfig ViewConfig { get; set; }

        /// <summary>
        /// Name of the query.
        /// </summary>
        [XmlElement("Name")]
        public string Name { get; set; }

        /// <summary>
        /// Description of the query.
        /// </summary>
        [XmlElement("Beschreibung")]
        public string Description { get; set; }

        /// <summary>
        /// Search parameters.
        /// </summary>
        [XmlElement("Parameter")]
        public ConfigSearchParams SearchParams { get; set; }


        /// <summary>
        /// Gets the current SearchValueMatrix
        /// </summary>
        public SearchValueMatrix SearchValueMatrix { get; set; }

        /// <summary>
        /// State of the query execution.
        /// </summary>
        [XmlIgnore]
        public QueryState State { get; set; }
        #endregion Properties
    }
}
