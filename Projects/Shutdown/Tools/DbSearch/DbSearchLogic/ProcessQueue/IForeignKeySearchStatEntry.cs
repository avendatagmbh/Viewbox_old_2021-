using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DbSearchLogic.ProcessQueue {

    /// <summary>
    /// Entry for providing statistics information about an initated foreign key search
    /// </summary>
    public interface IForeignKeySearchStatEntry : IKeySearchStatEntry {

        /// <summary>
        /// The foreign key table name
        /// </summary>
        string ForeignKeyTableName { get; }
    }
}
