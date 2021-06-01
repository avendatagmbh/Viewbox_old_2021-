// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-01-09
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using DbAccess.Structures;

namespace Config.Interfaces.Config {
    public interface IDatabaseOutputConfig : IConfig {
        /// <summary>
        /// Sets the database type.
        /// </summary>
        string DbType { get; set; }

        /// <summary>
        /// Sets the used database template (only used for DbType == "GenericODBC")
        /// </summary>
        string DbTemplateName { get; set; }

        /// <summary>
        /// Connection string for the database.
        /// </summary>
        string ConnectionString { get; set; }

        int CountInsertLines { get; set; }

        int QueuedInsertPackages { get; set; }

        DbConfig DbConfig { get; }

        bool UseImportDatabases { get; set; }
        
        bool UseDatabaseTablePrefix { get; set; }

        bool UseCompressDatabase { get; set; }

        bool IsMsSql { get; set; }

        int BatchSize{ get; set; }
    }
}