// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-01-09
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
namespace Config.Interfaces.Config {
    public interface IDatabaseInputConfig : IConfig {

        /// <summary>
        /// Sets the used database template (only used for DbType == "GenericODBC")
        /// </summary>
        string DbTemplateName { get; set; }

        /// <summary>
        /// Connection string for the database.
        /// </summary>
        string ConnectionString { get; set; }

        /// <summary>
        /// True, if tables should be exported.
        /// </summary>
        bool ProcessTables { get; set; }

        /// <summary>
        /// True, if views should be exported.
        /// </summary>
        bool ProcessViews { get; set; }

        bool UseAdo { get; set; }

        bool UseSchema { get; set; }

        bool UseCatalog { get; set; }

        string TableWhitelist { get; set; }

        string DatabaseWhitelist { get; set; }
    }
}