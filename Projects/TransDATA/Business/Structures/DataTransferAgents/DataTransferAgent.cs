// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-01-11
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using AV.Log;
using Business.Enums;
using Business.Interfaces;
using Business.Structures.InputAgents;
using Business.Structures.OutputAgents;
using Business.Structures.Pdf;
using Config.Interfaces.DbStructure;
using Config.Enums;
using Config.Manager;
using Config.Structures;
using Logging;
using System.Threading;
using PdfGenerator;
using iTextSharp.text;
using System.Diagnostics;
using AvdCommon.Logging;


namespace Business.Structures.DataTransferAgents
{
    public class DataTransferAgent : DataTransferAgentBase
    {

        //private int TaskCount { get { return Math.Min(Profile.MaxThreadCount, Environment.ProcessorCount); } }
        private int TaskCount { get { return Profile.MaxThreadCount; } }

        public DataTransferAgent(IProfile profile)
            : base(profile)
        {
            
            OutputAgents = new IOutputAgent[TaskCount];

            #region generate input agent
            switch (profile.InputConfig.Type)
            {
                case InputConfigTypes.Database:
                    InputAgent = new InputAgentDatabase(profile.InputConfig);
                    break;

                //case InputConfigTypes.Binary:
                //    InputAgent = new InputAgentBinary(profile.InputConfig);
                //    break;

                case InputConfigTypes.Csv:
                    InputAgent = new InputAgentCsv(profile.InputConfig);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
            #endregion generate input agent

            #region generate output agent
            switch (profile.OutputConfig.Type)
            {
                case OutputConfigTypes.Database:
                    for (int i = 0; i < TaskCount; i++)
                    {
                        OutputAgents[i] = new OutputAgentDatabase(profile, InputAgent, profile.OutputConfig);
                    }
                    break;

                case OutputConfigTypes.Gdpdu:
                    for (int i = 0; i < TaskCount; i++)
                    {
                        OutputAgents[i] = new OutputAgentGdpdu(profile, InputAgent, profile.OutputConfig);
                    }
                    break;

                case OutputConfigTypes.Csv:
                    for (int i = 0; i < TaskCount; i++)
                    {
                        OutputAgents[i] = new OutputAgentCsv(profile, InputAgent, profile.OutputConfig);
                    }
                    break;

                //case OutputConfigTypes.Sql:
                //    for (int i = 0; i < TaskCount; i++) {
                //        OutputAgents[i] = new OutputAgentSql(profile, InputAgent, profile.OutputConfig);
                //    }
                //    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
            #endregion generate output agent

            //TransferProgress = new TransferProgress();
            UICulture = System.Threading.Thread.CurrentThread.CurrentUICulture;
        }

        private IInputAgent InputAgent { get; set; }
        private IOutputAgent[] OutputAgents { get; set; }
        private bool Cancelled { get; set; }
        private CultureInfo UICulture { get; set; }

        #region TransferProgress
        //private TransferProgress _transferProgress;
        //public override ITransferProgress TransferProgress {
        //    get { return _transferProgress; }
        //    set { _transferProgress = (TransferProgress) value; }
        //}
        #endregion TransferProgress

        private List<ITransferEntity> Entities { get; set; }

        public override void Start() {
            _log.ContextLog(LogLevelEnum.Debug, "");
            Task.Factory.StartNew(() => Transfer(), TaskCreationOptions.LongRunning);
        }

        public override void Cancel()
        {
            _log.ContextLog(LogLevelEnum.Debug, "");
            Cancelled = true;
            for (int i = 0; i < TaskCount; i++)
            {
                OutputAgents[i].Cancel();
            }
        }

        /// <summary>
        /// Transfers all selected tables from the specified source to the specified output.
        /// </summary>
        //private bool _onTransfer = false;
        DateTime startCheckTimeOut = DateTime.MinValue;
        private void Transfer(List<ITransferEntity> entitiesToDo = null)
        {
            _log.ContextLog( LogLevelEnum.Debug,"");

            OnTransfer = true;
            TransferStartTime = DateTime.Now;

            _log.ContextLog( LogLevelEnum.Debug,"Start Watchdog");
            var timer = new Thread(Watchdog);
            timer.Start();

            Thread.CurrentThread.CurrentUICulture = UICulture;
            Log = new LoggingDb(Config.ConfigDb.ConnectionManager);

            try
            {
                _log.ContextLog( LogLevelEnum.Debug,"InitTransfer TaskCount: {0}", TaskCount);
                for (int i = 0; i < TaskCount; i++)
                {
                    OutputAgents[i].InitTransfer();
                }

                if (entitiesToDo == null)
                {
                    // init table list
                    _log.ContextLog( LogLevelEnum.Debug,"generate Entities and init table list");
                    Entities = new List<ITransferEntity>();

                    foreach (var table in Profile.Tables.Where(table => table.DoExport))
                    {
                        Entities.Add(table);
                        table.TransferState.State = TransferStates.NotTransfered;
                    }

                    _log.ContextLog( LogLevelEnum.Debug,"TransferProgress.Step:ExportingTables");
                    TransferProgress.Step = TransferProgressSteps.ExportingTables;
                }
                else
                {
                    _log.ContextLog( LogLevelEnum.Debug,"entitiesToDo.Count: {0}", entitiesToDo.Count);
                    foreach (var table in entitiesToDo)
                    {
                        Entities.Add(table);
                        table.TransferState.State = TransferStates.NotTransfered;
                    }
                    _log.ContextLog( LogLevelEnum.Debug,"TransferProgress.Step:ExportingErrorTables");
                    TransferProgress.Step = TransferProgressSteps.ExportingErrorTables;
                }

                _log.ContextLog( LogLevelEnum.Debug,"Entities.Count: {0}", Entities.Count);


                TransferProgress.EntitiesTotal = Entities.Count;
                TransferProgress.EntityFinishedList.Clear();
                TransferProgress.WorkingTasks = TaskCount;

                if (Cancelled)
                    return;

                // init export tasks
                _log.ContextLog( LogLevelEnum.Debug,"init export tasks");
                var tasks = new Task[TaskCount];
                for (int i = 0; i < TaskCount; i++)
                    tasks[i] = Task.Factory.StartNew((index) => TransferEntity(OutputAgents[(int)index], Log), i,
                                                     TaskCreationOptions.LongRunning);

                // wait until all export tasks are finished or cancelled
                _log.ContextLog( LogLevelEnum.Debug,"wait until all export tasks are finished or cancelled");
                Task.WaitAll(tasks);
                _log.ContextLog( LogLevelEnum.Debug,"wait until all export tasks are finished or cancelled. DONE.");

                if (!Cancelled)
                {
                    _log.ContextLog( LogLevelEnum.Debug,"CompleteTransfer");
                    for (int i = 0; i < TaskCount; i++)
                    {
                        OutputAgents[i].CompleteTransfer();
                    }
                }
            }
            catch (Exception ex)
            {
                TransferProgress.AddErrorMessage(ex.Message);
            }
            finally
            {
                // check for error tables
                _log.ContextLog( LogLevelEnum.Debug,"check for error tables");
                var errorEntities =
                    TransferProgress.EntityFinishedList.Where(
                        table => table.TransferState.State == TransferStates.TransferedError).ToList();


                // if has any error, wait and try to reconnect
                _log.ContextLog( LogLevelEnum.Debug,"errorEntities.Count: {0}", errorEntities.Count);
                if (errorEntities.Count > 0)
                {
                    TransferProgress.Step = TransferProgressSteps.WaitForConnection;
                    _log.ContextLog( LogLevelEnum.Debug,"TransferProgress.Step:WaitForConnection");

                    bool timeOutOccured = false;
                    bool inputConnected = false;
                    bool outputConnected = false;

                    if (startCheckTimeOut == DateTime.MinValue) {
                        startCheckTimeOut = DateTime.Now;
                        _log.ContextLog( LogLevelEnum.Debug,"startCheckTimeOut: {0}", startCheckTimeOut);
                    }

                    do
                    {
                        // wait 
                        int waitTime = Math.Min(10 * 60, (int)(-startCheckTimeOut.Subtract(DateTime.Now).TotalMilliseconds / 1000));
                        _log.ContextLog( LogLevelEnum.Debug,"waitTime: {0}", waitTime);

                        for (int i = 0; i < waitTime && !Cancelled && !(timeOutOccured = startCheckTimeOut.Add(new TimeSpan(3, 0, 0)) < DateTime.Now); i++)
                        {
                            //_log.ContextLog( LogLevelEnum.Debug,"wait a sec");
                            Thread.Sleep(1000);
                        }

                        if (!Cancelled)
                        {
                            inputConnected = this.InputAgent.CheckDataAccess();
                            _log.ContextLog(LogLevelEnum.Warn, "InputAgent.CheckDataAccess: {0}", (inputConnected ? "Connected" : "Not Connected"));

                            outputConnected = OutputAgents[0].CheckDataAccess();
                            _log.ContextLog(LogLevelEnum.Warn, "OutputAgents.CheckDataAccess: {0}", (outputConnected ? "Connected" : "Not Connected"));
                        }
                    } while (!(timeOutOccured || Cancelled || (inputConnected && outputConnected)));

                    _log.ContextLog( LogLevelEnum.Warn,"End wait {0}", startCheckTimeOut.Subtract(DateTime.Now).ToString());


                    if (inputConnected && outputConnected)
                    {
                        // if connected succesfully, try again
                        TransferProgress.Step = TransferProgressSteps.ExportingErrorTables;
                        _log.ContextLog( LogLevelEnum.Debug,"TransferProgress.Step:ExportingErrorTables");

                        _log.ContextLog( LogLevelEnum.Debug,"connected succesfully, try again");
                        Transfer(errorEntities);

                        // wait for finnish Transfer
                        _log.ContextLog( LogLevelEnum.Debug,"wait for finnish Transfer");
                        while (OnTransfer)
                            Thread.Sleep(4 * 1000);
                        _log.ContextLog( LogLevelEnum.Debug,"wait for finnish Transfer. DONE.");
                    }
                }

                if (OnTransfer)
                {
                    var errorEntities2 =
                            TransferProgress.EntityFinishedList.Where(
                                table => table.TransferState.State == TransferStates.TransferedError).ToList();


                    // create documentation if finished
                    TransferProgress.Step = TransferProgressSteps.CreateDocumentation;
                    _log.ContextLog( LogLevelEnum.Debug,"TransferProgress.Step:CreateDocumentation");

                    if (!Cancelled)
                        OnFinished();
                    OnTransfer = false;
                }
            }

        }

        /// <summary>
        /// Starts a new table exports until no more tables remain.
        /// </summary>
        private void TransferEntity(IOutputAgent outputAgent, LoggingDb loggingDb)
        {
            _log.ContextLog(LogLevelEnum.Debug, "");

            Thread.CurrentThread.CurrentUICulture = UICulture;

            ITransferEntity entity;
            while (!Cancelled && GetNextEntity(out entity))
            {
                var entityProgress = TransferProgress.AddProcessedEntity(entity) as TransferTableProgress;

                try
                {
                    outputAgent.ProcessEntity(entity, entityProgress, loggingDb);
                }
                catch (Exception ex)
                {
                    _log.ContextLog(LogLevelEnum.Error, "Error: {0}", ex.Message);
                }
                finally
                {
                    TransferProgress.RemoveProcessedEntity(entity);
                }
            }
        }

        /// <summary>
        /// Gets the next table from table list.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private bool GetNextEntity(out ITransferEntity entity)
        {
            lock (Entities)
            {
                if (Entities.Count > 0)
                {
                    entity = Entities.First();
                    Entities.RemoveAt(0);
                }
                else
                {
                    entity = null;
                }
            }
            return entity != null;
        }

        private FileInfo CreateLogFile(LoggingDb log, bool writeCurrentStatus = false)
        {
            LogfileGenerator logFileGenerator = new LogfileGenerator(this, Profile.Name);
            return logFileGenerator.GenerateLogFile(log, Entities, writeCurrentStatus);
        }

        private void GenerateXMLExportFile(LoggingDb loggingDb)
        {
            var result = new StringBuilder();
            var settings = new XmlWriterSettings { Indent = true, IndentChars = "  ", NewLineChars = Environment.NewLine };
            var writer = XmlWriter.Create(result, settings);

            writer.WriteStartDocument();
            writer.WriteStartElement("DataTransferAgent_Export");
            writer.WriteElementString("DateTime", DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss"));

            var finished = TransferProgress.EntityFinishedList.ToList();
            foreach (var table in finished)
            {
                var log = loggingDb.GetLogTables(table.Id);
                var logItem = log[log.Count - 1];

                writer.WriteStartElement("Table");
                new XMLLoggingTable()
                {
                    Catalog = table.Catalog,
                    Schema = table.Schema,
                    Name = table.Name,
                    Comment = table.Comment,
                    Count = table.Count,
                    Filter = table.Filter,
                    TransferStateState = (int)table.TransferState.State,
                    TransferStateMessage = table.TransferState.Message,
                    LogCount = logItem.Count,
                    LogCountDest = logItem.CountDest,
                    LogDuration = logItem.Duration,
                    LogError = logItem.Error,
                    LogFilter = logItem.Filter,
                    LogTimestamp = logItem.Timestamp,
                    LogUsername = logItem.Username,
                    LogState = logItem.State.ToString()
                }.WriteToXML(writer);

                foreach (var col in logItem.Columns)
                {
                    foreach (var column in table.Columns)
                    {
                        if (col.ColumnId != column.Id) continue;
                        writer.WriteStartElement("Column");
                        new XMLLoggingColumn()
                        {
                            Name = column.Name,
                            Comment = column.Comment,
                            Type = column.Type.ToString(),
                            TypeName = column.TypeName,
                            DbType = column.DbType.ToString(),
                            MaxLength = column.MaxLength,
                            DefaultValue = column.DefaultValue,
                            AllowDBNull = column.AllowDBNull,
                            AutoIncrement = column.AutoIncrement,
                            NumericScale = column.NumericScale,
                            IsPrimaryKey = column.IsPrimaryKey,
                            IsUnsigned = column.IsUnsigned,
                            IsIdentity = column.IsIdentity,
                            OrdinalPosition = column.OrdinalPosition
                        }.WriteToXML(writer);
                        writer.WriteEndElement();
                    }
                }
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
            writer.WriteEndDocument();
            writer.Close();

            OutputAgents[0].SaveXMLMetaData(result.ToString());
        }

        private bool SendMail(string subject, FileInfo attachement)
        {
            _log.ContextLog(LogLevelEnum.Debug, "Subject: {0}", subject);

            bool res =MailManager.SendMailMessage(Profile.MailConfig, subject,
                                        Base.Localisation.ResourcesCommon.MailFileInAttachement,
                                        Profile.MailConfig.Host.ToLower() == "smtp.mail.yahoo.de" ? "rsadata7@yahoo.de" : "Transdata@avendata.de",
                //"rsadata7@yahoo.de",
                                        new List<FileInfo>() { attachement });


            _log.ContextLog(LogLevelEnum.Debug, "Success: {0}", res?"yes":"no");

            return res;
        }

        protected override bool SendStatusMail()
        {
            return SendMail(string.Format(Base.Localisation.ResourcesCommon.MailSubjectStatusmail, Profile.Name), CreateLogFile(Log, true));
        }

        private string TimespanToString(TimeSpan span)
        {
            if (span.TotalSeconds < 1) return "< 1 " + Base.Localisation.ResourcesCommon.Second;
            var sb = new StringBuilder();
            if (span.Days > 0)
            {
                sb.Append(span.Days + " ");
                sb.Append(span.Days > 1 ? Base.Localisation.ResourcesCommon.Days : Base.Localisation.ResourcesCommon.Day);
            }
            if (span.Hours > 0)
            {
                sb.Append(span.Hours + " ");
                sb.Append(span.Hours > 1 ? Base.Localisation.ResourcesCommon.Hours : Base.Localisation.ResourcesCommon.Hour);
            }
            if (span.Minutes > 0)
            {
                sb.Append(span.Minutes + " ");
                sb.Append(span.Minutes > 1 ? Base.Localisation.ResourcesCommon.Minutes : Base.Localisation.ResourcesCommon.Minute);
            }
            if (span.Seconds > 0)
            {
                sb.Append(span.Seconds + " ");
                sb.Append(span.Seconds > 1 ? Base.Localisation.ResourcesCommon.Seconds : Base.Localisation.ResourcesCommon.Second);
            }
            return sb.ToString();
       } 

        public DataTable GetPreview(ITransferEntity entity, long count) { return InputAgent.GetPreview(entity, count); }

        public override bool CheckDataAccessSource() { return InputAgent.CheckDataAccess(); }

        public override bool CheckDataAccessDestination() { return OutputAgents[0].CheckDataAccess(); }

        public override string GetSourceTooltip()
        {
            switch (Profile.InputConfig.Type)
            {
                case InputConfigTypes.Database:
                    return Base.Localisation.ResourcesCommon.Database;

                case InputConfigTypes.Csv:
                    return Base.Localisation.ResourcesCommon.CSV;

                default:
                    return string.Empty;
            }
        }

        public override string GetDestinationTooltip()
        {
            switch (Profile.OutputConfig.Type)
            {
                case OutputConfigTypes.Database:
                    return Base.Localisation.ResourcesCommon.Database;

                case OutputConfigTypes.Csv:
                    return Base.Localisation.ResourcesCommon.CSV;

                case OutputConfigTypes.Gdpdu:
                    return Base.Localisation.ResourcesCommon.GDPDU;

                default:
                    return string.Empty;
            }
        }

        public override IInputAgent GetFirstInputAgent()
        {
            return InputAgent;
        }

        public override IOutputAgent GetFirstOutputAgent()
        {
            return OutputAgents.FirstOrDefault();
        }

        public override string GetLogDirectory() { return OutputAgents[0].GetLogDirectory(); }
    }
}