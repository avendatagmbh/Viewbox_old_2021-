// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-01-11
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.IO;
using Business.Events;
using Business.Interfaces;
using Config.Interfaces.DbStructure;

namespace Business.Structures.OutputAgents {
    internal abstract class OutputAgentBase : IOutputAgent {
        protected OutputAgentBase(IProfile profile, IInputAgent inputAgent, IOutputConfig config) {
            Profile = profile;
            InputAgent = inputAgent;
            Config = config;
        }

        #region DataReaderAdded
        public event EventHandler<DataReaderEvent> DataReaderAdded;
        protected void OnDataReaderAdded(IDataReader dataReader) { if (DataReaderAdded != null) DataReaderAdded(this, new DataReaderEvent(dataReader)); }
        #endregion

        #region DataReaderRemoved
        public event EventHandler<DataReaderEvent> DataReaderRemoved;
        protected void OnDataReaderRemoved(IDataReader dataReader) { if (DataReaderRemoved != null) DataReaderRemoved(this, new DataReaderEvent(dataReader)); }
        #endregion

        protected IProfile Profile { get; set; }
        protected IInputAgent InputAgent { get; set; }
        protected IOutputConfig Config { get; set; }

        /// <summary>
        /// Indicates if the export process has been cancelled.
        /// </summary>
        protected bool Cancelled { get; set; }

        public abstract void InitTransfer();

        public virtual void ProcessEntity(ITransferEntity entity, TransferTableProgress progress, Logging.LoggingDb loggingDb, bool useAdo = false) { throw new NotImplementedException(); }
        public virtual void ProcessFile(IFile file) { throw new NotImplementedException(); }

        public virtual void SaveLogFile(FileInfo fi)
        {
            var fiDest = new FileInfo(System.Reflection.Assembly.GetExecutingAssembly().Location);
            if (!Directory.Exists(fiDest.DirectoryName + "\\log\\"))
                Directory.CreateDirectory(fiDest.DirectoryName + "\\log\\");
            File.Copy(fi.FullName, fiDest.DirectoryName + "\\log\\" + fi.Name, true);
        }

        public virtual void SaveXMLMetaData(string XMLString)
        {
            var fi = new FileInfo(System.Reflection.Assembly.GetExecutingAssembly().Location);
            if (!Directory.Exists(fi.DirectoryName + "\\log\\"))
                Directory.CreateDirectory(fi.DirectoryName + "\\log\\");
            using (var writer = new StreamWriter(fi.DirectoryName + "\\log\\transdata_export_" + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + ".xml"))
            {
                writer.Write(XMLString);
                writer.Close();
            }
        }

        public abstract void CompleteTransfer();
        
        public virtual void Cancel() { Cancelled = true; }

        public abstract bool CheckDataAccess();


        public virtual string GetLogDirectory()
        {
            var fi = new FileInfo(System.Reflection.Assembly.GetExecutingAssembly().Location);
            if (!Directory.Exists(fi.DirectoryName + "\\log\\"))
                Directory.CreateDirectory(fi.DirectoryName + "\\log\\");
            return fi.DirectoryName + "\\log\\";
        }

        public abstract string GetDescription();
    }
}