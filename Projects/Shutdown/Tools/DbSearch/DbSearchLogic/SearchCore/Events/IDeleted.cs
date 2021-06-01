using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DbSearchLogic.SearchCore.Events {
    /// <summary>
    /// Interface that for handling deleted event
    /// </summary>
    public  interface IDeleted {
        /// <summary>
        /// Deleted even handler for delete event
        /// </summary>
        event EventHandler Deleted;

        /// <summary>
        /// Flag which infgorms about the deleted state of the object
        /// </summary>
        bool IsDeleted { get; }
    }
}
