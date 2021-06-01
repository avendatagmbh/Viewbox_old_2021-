using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using DbSearchLogic.Manager;
using DbSearchLogic.SearchCore.Config;
using DbSearchLogic.SearchCore.Structures;
using DbSearchLogic.SearchCore.Structures.Db;
using DbSearchLogic.Structures.TableRelated;
using log4net;
using AV.Log;

namespace DbSearchLogic.SearchCore.Threading {
    class QueryThread : IThreadExecutor{

        internal ILog _log = LogHelper.GetLogger();

        public QueryThread(QueryInfo info, Query query, int tableThreadPriority, GlobalSearchConfig globalSearchConfig) {
            _queryData = new ThreadManager.QueryData() { FreeTables = new TableList(), QueryInfo = info, Query = query, GlobalSearchConfig = globalSearchConfig };
            query.ResultHistory.AddNewResults(query, _queryData.Guid);
            _tableThreadPriority = tableThreadPriority;
        }

        #region Properties

        private bool _abort = false;
        private int _tableThreadPriority;

        private Task _task;
        public Task Task {
            get { return _task; }
        }

        public int ThreadPriority {
            get { return 1; }
        }

        public string QueryName {
            get { return _queryData.Query.Name; }
        }

        public string Description {
            get { return "Lese Tabellen der MySQL Datenbank"; }
        }

        public string CurrentTable {
            get { return "-"; }
        }

        public string CurrentColumn {
            get { return "-"; }
        }

        public string TableCount {
            get { return "-"; }
        }

        public string TimeWorked {
            get { return "-"; }
        }

        private ThreadManager.QueryData _queryData;
        public ThreadManager.QueryData QueryData { get { return _queryData; } }
        private static object _staticLock = new object();
        #endregion

        private void DoWork() {
            _queryData.FreeTables = TableListManager.TableList(_queryData.QueryInfo.QueryConfig.ViewConfig, _queryData.Query.Profile.TableInfoManager, _queryData.Query.SearchTableDecider);
            
            _queryData.QueryInfo.State.Init(_queryData.FreeTables.RemainingTables);

            TableInfo currentTable;
            ThreadManager.QueryData data = QueryData;
            Random random = new Random();

            List<Task> childTasks = new List<Task>();
            //10% chance to get a huge table
            lock (_staticLock) {
                while (data.FreeTables.GetFreeTable(random.Next(1, 10) != 1, out currentTable)) {
                //while (data.FreeTables.GetFreeTable(random.Next(1, 1) != 1, out currentTable)) {
                    TableSearchThread thread = new TableSearchThread(data, currentTable, _tableThreadPriority);
                    thread.CreateThread(TaskCreationOptions.AttachedToParent);
                    childTasks.Add(thread.Task);
                    ThreadManager.NewThread(thread);
                }
            }
            ThreadManager.DeleteFromActiveThreads(this);
            foreach (var childTask in childTasks) {
                try {
                    childTask.Wait();
                    if (_abort) break;
                }
                catch (AggregateException aggregateException) {
                    string message = "Fehler in bei der Tabellensuche: ";
                    foreach (var ex in aggregateException.InnerExceptions)
                        message += Environment.NewLine + ex.Message;
                    _log.Log(LogLevelEnum.Error, message, true);
                }
            }
            QueryData.Query.ResultHistory.FinishedSearch(QueryData.Guid);
            ThreadManager.FinishedQuery(this);
            _log.Log(LogLevelEnum.Info, "Suche beendet des Queries " + QueryData.Query.Name, true);

        }

        public void CreateThread(TaskCreationOptions options = TaskCreationOptions.LongRunning) {
            _task = new Task(DoWork, options);
        }

        public void StartThread() {
            _task.Start();
        }

        public void Abort() {
            _abort = true;
        }

        public override string ToString() {
            return "QueryThread: " + QueryName;
        }

    }
}
