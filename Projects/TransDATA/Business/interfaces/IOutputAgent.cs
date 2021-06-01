// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-01-11
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.IO;
using Business.Events;
using Business.Structures;
using Config.Interfaces.DbStructure;

namespace Business.Interfaces {
    public interface IOutputAgent {

        event EventHandler<DataReaderEvent> DataReaderAdded;
        event EventHandler<DataReaderEvent> DataReaderRemoved;
        
        /// <summary>
        /// Prepares the transfer process, e.g. create databases or directories.
        /// </summary>
        void InitTransfer();

        /// <summary>
        /// Transfers the specified table.
        /// </summary>
        void ProcessEntity(ITransferEntity entity, TransferTableProgress progress, Logging.LoggingDb loggingDb, bool useAdo = false);

        /// <summary>
        /// Finishes the transfer process, e.g. writing the xml file for the GDPdU-Export.
        /// </summary>
        void CompleteTransfer();

        /// <summary>
        /// Cancels the currently running transfer process.
        /// </summary>
        void Cancel();

        void SaveLogFile(FileInfo fi);

        void SaveXMLMetaData(string XMLString);

        bool CheckDataAccess();

        string GetLogDirectory();
        string GetDescription();
    }
}