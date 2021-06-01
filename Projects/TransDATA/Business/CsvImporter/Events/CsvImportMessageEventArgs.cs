using System;
using System.IO;

namespace Business.CsvImporter.Events {
    public class CsvImportMessageEventArgs : EventArgs {

        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorEventArgs"/> class.
        /// </summary>
        /// <param name="fileName">Name of the table.</param>
        /// <param name="message">Name of the file.</param>
        /// <param name="message">The message.</param>
        internal CsvImportMessageEventArgs(string tableName, string fileName, string message) {
            this.TableName = tableName;
            this.FileName = fileName;
            this.Message = message;
        }

        /// <summary>
        /// Gets or sets the name of the table.
        /// </summary>
        /// <value>The name of the table.</value>
        public string TableName { get; private set; }

        /// <summary>
        /// Gets or sets the name of the file.
        /// </summary>
        /// <value>The name of the file.</value>
        public string FileName { get; private set; }

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        /// <value>The message.</value>
        public string Message { get; private set; }
    }
}
