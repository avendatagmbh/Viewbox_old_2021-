// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-01-11
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Collections.Generic;
using System.Data;
using Config.Structures;
using DbAccess;

namespace Config.Interfaces.DbStructure {
    public interface 
        ITransferEntity {
        
        long Id { get; set; }

        string Name { get; set; }

        string DestinationName { get; set; }

        string Comment { get; set; }

        string Filter { get; set; }

        long Count { get; set; }

        string Catalog { get; set; }

        string Schema { get; set; }

        /// <summary>
        /// True, if this file has a comment.
        /// </summary>
        bool HasComment { get; }

        /// <summary>
        /// True, if this file should be exported.
        /// </summary>
        bool DoExport { get; set; }

        bool DoDbUpdate { get; set; }

        string DisplayString { get; }

        TransferState TransferState { get; set; }
        
        /// <summary>
        /// Saves this config to database.
        /// </summary>
        void Save();

        IEnumerable<IColumn> Columns { get; }
    }

}