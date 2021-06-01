// --------------------------------------------------------------------------------
// author: Marcus Gerlach
// since:  2011-01-10
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AV.Log;
using Base.Exceptions;
using Business.Interfaces;
using Business.Structures.MetadataAgents;
using Config.Interfaces.Config;
using DbAccess.Structures;
using Config.DbStructure;
using Config.Interfaces.DbStructure;
using Business.Structures;
using System.Globalization;
using System.Threading.Tasks;
using DbAccess;
using Business.Structures.InputAgents;
using GenericOdbc;
using log4net;

namespace Business {
    class ImporterDbStructure : IImporterDbStructure {
        internal ILog _log = LogHelper.GetLogger();

        private int TaskCount { get { return Math.Min(Profile.MaxThreadCount, Environment.ProcessorCount); } }

        public ImporterDbStructure(IProfile profile) {
            Profile = profile;
            ImportProgress = new ImportDbStructureProgress();
            UICulture = System.Threading.Thread.CurrentThread.CurrentUICulture;
            ImportAgent = new InputAgentDatabase(profile.InputConfig);
        }

        #region events
        public event EventHandler Finished;
        public void OnFinished() { if (Finished != null) Finished(this, new EventArgs()); }
        #endregion

        private CultureInfo UICulture { get; set; }
        public IImportDbStructureProgress ImportProgress { get; set; }
        private bool IsCancelled { get; set; }
        private IProfile Profile { get; set; }
        private List<DbTableInfo> Tables { get; set; }
        private IInputAgent ImportAgent { get; set; }

        public void Start() {
            _log.ContextLog( LogLevelEnum.Debug,"");
            Task.Factory.StartNew(Import);
        }

        private void Import() {
            _log.ContextLog( LogLevelEnum.Debug,"" );

            System.Threading.Thread.CurrentThread.CurrentUICulture = UICulture;
            ImportProgress.WorkingTasks = TaskCount;
            try {
                IMetadataAgent agent = MetadataAgentFactory.CreateAgent(Profile.InputConfig.Config, TaskCount);

                Tables = new List<DbTableInfo>();

                _log.ContextLog( LogLevelEnum.Info,"ImportProgress.Step:GeneratingMeta");
                ImportProgress.Step = Enums.ImportDbStructureSteps.GeneratingMeta;

                agent.AddTables(ImportAgent, Tables, ImportProgress);

                if (IsCancelled) {
                    _log.ContextLog( LogLevelEnum.Debug,"Cancelled #1");
                    return;
                }

                ImportProgress.TablesTotal = Tables.Count;
                _log.ContextLog( LogLevelEnum.Info,"ImportProgress.Step:CountingTables");
                ImportProgress.Step = Enums.ImportDbStructureSteps.CountingTables;
                Profile.ClearTransferEntities();

                // init tasks
                _log.ContextLog( LogLevelEnum.Debug,"init tasks");
                int taskCount = Profile.MaxThreadCount;
                var tasks = new Task[taskCount];
                for (int i = 0; i < taskCount; i++) {
                    tasks[i] = Task.Factory.StartNew(() => ImportTable(agent), TaskCreationOptions.None);
                }

                // wait until all export tasks are finished or cancelled
                _log.ContextLog( LogLevelEnum.Debug,"wait until all export tasks are finished or cancelled");
                Task.WaitAll(tasks);

                if (IsCancelled){
                    _log.ContextLog( LogLevelEnum.Debug,"Cancelled #2");
                    return;
                }

                _log.ContextLog( LogLevelEnum.Info,"ImportProgress.Step:SavingData");
                ImportProgress.Step = Enums.ImportDbStructureSteps.SavingData;

                _log.ContextLog( LogLevelEnum.Debug,"Call Profile.SaveTables");
                Profile.SaveTables();
            } catch (Exception ex) {
                _log.ContextLog( LogLevelEnum.Error, "Error: {0}", ex.Message);
                ImportProgress.AddErrorMessage(ex.Message);
            } finally {
                if (!IsCancelled) {
                    _log.ContextLog( LogLevelEnum.Debug,"Call OnFinished");
                    OnFinished();
                }else {
                    _log.ContextLog( LogLevelEnum.Debug,"Cancelled #3");
                }
            }
            _log.ContextLog( LogLevelEnum.Debug,"Done");

        }

        private void ImportTable(IMetadataAgent agent) {
            System.Threading.Thread.CurrentThread.CurrentUICulture = UICulture;

            DbTableInfo table;
            while (!IsCancelled && GetNextTable(out table)) {
                try {
                    _log.ContextLog( LogLevelEnum.Debug,"Table:{0}", table);
                    agent.ImportTable(table, Profile, ImportProgress);
                } catch (Exception ex) {
                    string errorMessage = string.Format("Table: {0} Error: {1}", table.Name, ex.Message);
                    _log.ContextLog( LogLevelEnum.Error, "{0}", errorMessage);
                    ImportProgress.AddErrorMessage(errorMessage);
                }
            }
        }

        /// <summary>
        /// Gets the next table from table list.
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        private bool GetNextTable(out DbTableInfo table) {
            lock (Tables) {
                if (Tables.Count > 0) {
                    table = Tables.First();
                    Tables.RemoveAt(0);
                } else {
                    table = null;
                }
            }
            return table != null;
        }

        public void Cancel() {
            _log.ContextLog( LogLevelEnum.Debug,"");
            IsCancelled = true;
        }
    }
}
