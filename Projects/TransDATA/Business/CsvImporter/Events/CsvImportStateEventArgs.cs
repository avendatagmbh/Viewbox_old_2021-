using System;

namespace Business.CsvImporter.Events {
    
    public class CsvImportStateEventArgs : EventArgs {

        /// <summary>
        /// Initializes a new instance of the <see cref="CsvImportStateEventArgs"/> class.
        /// </summary>
        /// <param name="state">The state.</param>
        //internal CsvImportStateEventArgs(TableState_CsvImport state) {
        //    this.State = state;
        //}

        /// <summary>
        /// Gets or sets the state.
        /// </summary>
        /// <value>The state.</value>
        //public TableState_CsvImport State { get; private set; }
    }
}
