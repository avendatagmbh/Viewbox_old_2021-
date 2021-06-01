// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-01-11
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.IO;
using System.Threading;
using AV.Log;
using Business.Interfaces;
using Config.Interfaces.Config;
using Config.Interfaces.DbStructure;
using Logging;
using Logging.Interfaces.DbStructure;
using Business.Events;
using log4net;

namespace Business.Structures.DataTransferAgents {
    public abstract class DataTransferAgentBase : IDataTransferAgent {
        internal ILog _log = LogHelper.GetLogger();

        public DataTransferAgentBase(IProfile profile) {
            Profile = profile;
            TransferProgress = new TransferProgress();
            if(profile.OutputConfig.Config is ICsvOutputConfig) {
                string dirpath = ((ICsvOutputConfig) Profile.OutputConfig.Config).Folder;
                if (!string.IsNullOrEmpty(dirpath))
                    TransferProgress.InitHDDObservation(new DirectoryInfo(dirpath));
            }
        }

        #region Properties
        protected bool _onTransfer;
        protected bool OnTransfer { 
            get { return _onTransfer; }
            set { _onTransfer = value; }
        }
        
        protected IProfile Profile { get; set; }
        public ITransferProgress TransferProgress { get; set; }
        //public virtual ITransferProgress TransferProgress {
        //    get { throw new NotImplementedException(); }
        //    set { throw new NotImplementedException(); }
        //}

        protected LoggingDb Log { get; set; }

        public event EventHandler Finished;
        public DateTime TransferStartTime { get; set; }
        #endregion Properties

        #region Methods
        protected void OnFinished() {
            _log.ContextLog( LogLevelEnum.Debug,"");
            if (Finished != null) Finished(null, new EventArgs());
        }

        public abstract void Start();
        public abstract void Cancel();

        protected virtual bool SendStatusMail() { return false; }

        protected void Watchdog() {
            _log.ContextLog(LogLevelEnum.Debug, "");
            DateTime lastStatusMail = DateTime.MinValue;

            LoggingDb logDb = null;
            if (Profile.LogPerformance) {
                logDb = new LoggingDb(Config.ConfigDb.ConnectionManager);
            }
            while (OnTransfer) {
                TransferProgress.UpdatePerformace();
                // performance - log
                if (Profile.LogPerformance) {
                    IPerformance performance = logDb.CreatePerformanceLogEntity();
                    performance.CpuUsage = TransferProgress.CpuPerformance;
                    performance.RamLeft = TransferProgress.RamPerformance;
                    performance.FreeDriveSpace = TransferProgress.HDDPerformance;
                    performance.TimeStamp = DateTime.Now;
                    try { logDb.Save(performance); } catch (Exception) { }
                }
                // statusmail
                if (Profile.MailConfig.SendStatusmail) {
                    if (lastStatusMail.Date < System.DateTime.Now.Date && Profile.MailConfig.StatusmailSendTime.TimeOfDay < DateTime.Now.TimeOfDay) {
                        _log.ContextLog(LogLevelEnum.Debug, "SendStatusMail");
                        if (SendStatusMail()) lastStatusMail = DateTime.Now;
                    }
                }
                Thread.Sleep(1000);
            }
        }
        #endregion Methods

        public abstract bool CheckDataAccessSource();
        public abstract bool CheckDataAccessDestination();


        public abstract string GetSourceTooltip();
        public abstract string GetDestinationTooltip();
        

        public abstract string GetLogDirectory();

        public abstract IInputAgent GetFirstInputAgent();
        public abstract IOutputAgent GetFirstOutputAgent();

    }
}