/**************************************************************************************************************
 * author               date            comment
 * ------------------------------------------------------------------------------------------------------------
 * Mirko Dibbert        2010-10-28      initial implementation
 *************************************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace ViewBuilder.Models {

    /// <summary>
    /// Model for the DlgScriptErrors windows
    /// </summary>
    internal class DlgScriptErrorsModel {

        /// <summary>
        /// Initializes a new instance of the <see cref="DlgScriptErrorsModel"/> class.
        /// </summary>
        public DlgScriptErrorsModel() {
            this.ScriptParseErrors = new ObservableCollection<object>();
            this.MultipleViewError = new ObservableCollection<object>();
        }

        /// <summary>
        /// Gets or sets the script parse errors.
        /// </summary>
        /// <value>The script parse errors.</value>
        public ObservableCollection<object> ScriptParseErrors { get; set; }

        /// <summary>
        /// Gets or sets the multiple view error.
        /// </summary>
        /// <value>The multiple view error.</value>
        public ObservableCollection<object> MultipleViewError { get; set; }
    }
}
