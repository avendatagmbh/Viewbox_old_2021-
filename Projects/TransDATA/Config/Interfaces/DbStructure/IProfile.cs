// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2011-08-27
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Collections.Generic;
using Config.Interfaces.Mail;

namespace Config.Interfaces.DbStructure {
    /// <summary>
    /// Interface for a profile objects.
    /// </summary>
    public interface IProfile {
        long Id { get; set; }
        string DisplayString { get; }
        string Name { get; set; }
        IInputConfig InputConfig { get; }
        IOutputConfig OutputConfig { get; set; }
        int MaxThreadCount { get; set; }
        bool LogPerformance { get; set; }
        bool DoDbUpdate { get; set; }
        bool IsDatabaseAnalysed { get; set; }

        bool DataSourceAvailable { get; set; }
        bool DataDestinationAvailable { get; set; }
        string DataSourceTooltip { get; set; }
        string DataDestinationTooltip { get; set; }

        #region Mail
        IMailConfig MailConfig { get; }
        #endregion
        /// <summary>
        /// Enumeration of all assigned tables.
        /// </summary>
        IEnumerable<ITable> Tables { get; }
        
        /// <summary>
        /// Saves this profile. Assigned tables must be explicitly saved by calling the SaveTables() method.
        /// </summary>
        void Save(bool saveAll = true);
        
        /// <summary>
        /// Saves the table collection.
        /// </summary>
        void SaveTables();

        void SaveTable(ITable table);
        /// <summary>
        /// Clears all tables/files which are assigned to this profile.
        /// </summary>
        void ClearTransferEntities();
        
        /// <summary>
        /// Creates a new table object, which is assigned to tis profile.
        /// </summary>
        /// <returns></returns>
        ITable CreateTable();
        //ITableWithParts CreateTableWithParts();

        /// <summary>
        /// Adds the specified table object to the table collections. This method should not be called
        /// bevore the count property has been initialized, otherwhise the table will not be added to 
        /// the correct detail collection (FilledTables or EmptyTables).
        /// </summary>
        /// <param name="table"></param>
        void AddTable(ITable table);

        IProfile Clone();
    }
}