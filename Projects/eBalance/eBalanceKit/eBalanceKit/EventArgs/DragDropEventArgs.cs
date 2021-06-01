/**************************************************************************************************************
 * author               date            comment
 * ------------------------------------------------------------------------------------------------------------
 * Mirko Dibbert        2010-12-08      initial implementation
 *************************************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace eBalanceKit.EventArgs {

    /// <summary>
    /// Event argument class for Drag&Drop events of the CtlTaxonomyTreeViewWithBalanceList control.
    /// </summary>
    internal class DragDropEventArgs : System.EventArgs {

        /// <summary>
        /// Initializes a new instance of the <see cref="DragDropEventArgs"/> class.
        /// </summary>
        /// <param name="args">The args.</param>
        public DragDropEventArgs(object args) {
            this.Args = args;
        }

        /// <summary>
        /// Gets or sets the event argument object.
        /// </summary>
        /// <value>The event argument object.</value>
        public object Args { get; set; }
    }
}
