// --------------------------------------------------------------------------------
// author: Marcus Gerlach
// since:  2011-01-12
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;

namespace Business.Interfaces {
    public interface IDataReader : IDisposable {

        int FieldCount { get; }

        /// <summary>
        /// Loads the table data.
        /// </summary>
        void Load(bool loadAllColumns = false);

        /// <summary>
        /// Cancels the execution of the load method.
        /// </summary>
        void Cancel();

        void Close();

        /// <summary>
        /// Reads the next data row in the assigned table.
        /// </summary>
        /// <returns>True, if more data rows exists, false otherwhise.</returns>
        bool Read();

        /// <summary>
        /// Gets the actual data line as object array.
        /// </summary>
        /// <returns></returns>
        object[] GetData();

    }
}
