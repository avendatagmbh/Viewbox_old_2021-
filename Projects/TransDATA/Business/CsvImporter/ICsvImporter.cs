using System;
using Business.CsvImporter.Events;
using Business.CsvImporter.Structures;

namespace Business.CsvImporter {

    internal interface ICsvImporter {

        /// <summary>
        /// Occurs when a log message should be send to the GUI.
        /// </summary>
        event EventHandler<CsvImportMessageEventArgs> Log;

        /// <summary>
        /// Occurs when an info message should be send to the GUI.
        /// </summary>
        event EventHandler<CsvImportMessageEventArgs> Info;

        /// <summary>
        /// Occurs when a warning message should be send to the GUI.
        /// </summary>
        event EventHandler<CsvImportMessageEventArgs> Warn;

        /// <summary>
        /// Occurs when an error occurs.
        /// </summary>
        event EventHandler<CsvImportMessageEventArgs> Error;

        /// <summary>
        /// Occurs when the state is updated.
        /// </summary>
        //event EventHandler<CsvImportStateEventArgs> UpdateState;

        /// <summary>
        /// Imports the table, which is assigned to the specified stateId.
        /// </summary>
        /// <param name="stateId">The state id.</param>
        bool ImportTable(CsvTableInfo tableInfo);

        void Init();
    }
}
