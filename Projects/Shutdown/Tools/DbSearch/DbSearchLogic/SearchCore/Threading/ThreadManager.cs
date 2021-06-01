using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using DbAccess;
using DbAccess.Structures;
using DbSearchDatabase.DistinctDb;
using DbSearchLogic.Config;
using DbSearchLogic.SearchCore.Config;
using DbSearchLogic.SearchCore.Result;
using DbSearchLogic.SearchCore.SearchMatrix;
using DbSearchLogic.SearchCore.Structures;
using DbSearchLogic.Structures;
using DbSearchLogic.Structures.TableRelated;
using Utils;
using log4net;
using ErrorEventArgs = DbSearchLogic.SearchCore.Events.ErrorEventArgs;
using AV.Log;

namespace DbSearchLogic.SearchCore.Threading {
    public static class ThreadManager {

        internal static ILog _log = LogHelper.GetLogger();

        #region QueryData
        internal class QueryData {
            internal QueryData() {
                Guid = Guid.NewGuid();
                GlobalSearchConfig = new GlobalSearchConfig();
            }

            public TableList FreeTables { get; set; }
            public QueryInfo QueryInfo { get; set; }
            public Task<TableResultSet> Task { get; set; }
            public Query Query { get; set; }
            public Guid Guid { get; private set; }
            public GlobalSearchConfig GlobalSearchConfig { get; set; }
        }
        #endregion QueryData

        #region Constructor
        static ThreadManager() {
            QueryInfos = new ObservableCollectionAsync<QueryInfo>();
            MaxThreads = 2;
        }
        #endregion Constructor

        #region Properties
        //public static int MaxThreads = (int)(Environment.ProcessorCount * 1.5);

        #region MaxThreads
        private static int _maxThreads;

        public static int MaxThreads {
            get { return _maxThreads; }
            set {
                if (_maxThreads != value) {
                    int oldValue = _maxThreads;
                    _maxThreads = value;
                    if(value > oldValue)
                        TryToStartNewThreads();
                }
            }
        }
        #endregion MaxThreads

        //public static int MaxThreads = 1;
        private static object _lock = new object();
        private static readonly Queue<IThreadExecutor> _threadQueue = new Queue<IThreadExecutor>();
        //private static readonly PriorityQueue<IThreadExecutor,int> _threadQueue = new PriorityQueue<IThreadExecutor, int>();
        private static readonly ObservableCollectionAsync<IThreadExecutor> _runningThreads = new ObservableCollectionAsync<IThreadExecutor>();
        public static ObservableCollectionAsync<IThreadExecutor> RunningThreads { get { return _runningThreads; } }
        public static EventHandler<ErrorEventArgs> ErrorHandler;
        public static ObservableCollectionAsync<QueryInfo> QueryInfos { get; private set; }
        private static int _queryNumber = 100000;
        #endregion Properties

        #region Methods
        private static void OnError(Exception ex) {if (ErrorHandler != null) ErrorHandler(null, new ErrorEventArgs(ex));}

        private static bool CheckError(Task task) {
            if (task.Exception != null) {
                OnError(task.Exception);
                return true;
            }
            return false;
        }

        #region StartNewQuery
        private static QueryThread StartNewQuery(QueryInfo info, Query query) {
            _log.Log(LogLevelEnum.Info, "Suche begonnen des Queries " + query.Name, true);

            Interlocked.Decrement(ref _queryNumber); 
            QueryThread queryThread = new QueryThread(info, query, _queryNumber, query.Profile.GlobalSearchConfig);
            queryThread.CreateThread();
            queryThread.Task.ContinueWith(tmp => AddTableThreads(queryThread));
            lock (_lock) {
                //_threadQueue.Enqueue(queryThread,1);
                _threadQueue.Enqueue(queryThread);
                QueryInfos.Add(queryThread.QueryData.QueryInfo);
            }
            //TryToStartNewThreads();
            return queryThread;
        }
        #endregion StartNewQuery

        #region TryToStartNewThread
        private static void TryToStartNewThreads(int max = 0) {
            lock(_lock) {
                int started = 0;
                while (_runningThreads.Count < MaxThreads && _threadQueue.Count > 0 && (started < max || max == 0)) {
                    //IThreadExecutor current = _threadQueue.Dequeue().Value;
                    IThreadExecutor current = _threadQueue.Dequeue();
                    //Console.WriteLine("Query: " + current.QueryName  + ", Thread: " + current.CurrentTable + ", Priority: " + current.ThreadPriority);
                    _runningThreads.Add(current);
                    current.StartThread();
                    started++;
                }
            }
        }
        #endregion TryToStartNewThread

        #region AddTableThreads
        private static void AddTableThreads(QueryThread queryThread) {
            lock(_lock){
                _runningThreads.Remove(queryThread);
            }
            if (CheckError(queryThread.Task)) {
                FinishedQuery(queryThread);
                return;
            }

            for(int i = 0; i < MaxThreads; ++i)
                TryToStartNewThreads();
        }
        #endregion AddTableThreads

        #region TableSearchFinished
        internal static void TableSearchFinished(TableSearchThread thread) {
            CheckError(thread.Task);
            lock (_lock) {
                _runningThreads.Remove(thread);
            }
            try {
                //Set results
                //lock (thread.QueryData.Query.ResultHistory) {
                thread.QueryData.Query.ResultHistory.AddTableResult(thread.TableResult, thread.QueryData.Guid);
                //Set to null so that the garbage collector can dispose the tableresult
                thread.TableResult = null;
                //}
            } catch (Exception ex) {
                OnError(ex);
            }
            TryToStartNewThreads();
        }

        #endregion TableSearchFinished

        #region StartNewTableSearchThreaded
        private static QueryThread StartNewTableSearchThreaded(Query query) {
            try {
                
                //Load data of query in case it hasnt been done
                query.LoadData(15);

                SearchValueMatrix svm = SearchValueMatrix.CreateFromQuery(query);
                //SearchValueMatrix svm = SearchValueMatrix.CreateFromQuery(query.CreateDistinctQuery());

                //DbDistincter distincter = new DbDistincter(query.Profile.DbConfigView);
                //distincter.Start();
                //Check if distinctDatabase Exists
                DbConfig dbNoDatabase = (DbConfig)query.Profile.DbConfigView.Clone();
                dbNoDatabase.DbName = string.Empty;
                bool distinctDbExists = false;
                using (IDatabase conn = ConnectionManager.CreateConnection(dbNoDatabase)) {
                    conn.Open();
                    if (conn.DatabaseExists(query.Profile.DbConfigView.DbName + "_idx")) distinctDbExists = true;
                    //else {
                    //    try {
                    //        LogManager.Status("Starte das Distincten der Datenbank");
                    //        DbDistincter distincter = new DbDistincter(query.Profile.DbConfigView);
                    //        distincter.Start();
                    //        distinctDbExists = true;
                    //        LogManager.Status("Distincten der Datenbank fertiggestellt");
                    //    } catch (Exception ex) {
                    //        LogManager.Error("Distinct der Datenbank ist fehlgeschlagen: " + ex.Message);
                    //    }
                    //}
                }

                QueryInfo info = new QueryInfo(
                    new QueryConfig(query.Profile.DbConfigView, query.Name, svm),
                    distinctDbExists
                    );

                //Save query to store all information
                query.Save();
                return StartNewQuery(info, query);
            } catch (Exception ex) {
                OnError(ex);
                return null;
            }
        }
        #endregion StartNewTableSearchThreaded

        public static Task StartNewTableSearch(Query query) {
            return Task.Factory.StartNew(() => { StartNewTableSearchThreaded(query); TryToStartNewThreads(); }).ContinueWith((task) => CheckError(task));
        }

        public static void StartNewTableSearch(ObservableCollection<Query> queries) {
            Task.Factory.StartNew(() => {
                                      foreach (var query in queries) {
                                          //if (query.Name == "01_CJ20N_Statusanzeige_Liste_der_Aenderungen")
                                          //    continue;
                                          Task<QueryThread> task = new Task<QueryThread>(() => StartNewTableSearchThreaded(query),TaskCreationOptions.AttachedToParent);
                                          task.ContinueWith((antTask) => CheckError(antTask));
                                          task.Start();
                                          task.Wait();
                                      }
                                      TryToStartNewThreads(1);
                                  });
        }

        public static void NewThread(IThreadExecutor thread) {
            lock (_lock) {
                //_threadQueue.Enqueue(thread,thread.ThreadPriority);
                _threadQueue.Enqueue(thread);
            }
            TryToStartNewThreads(1);
        }

        internal static void FinishedQuery(QueryThread queryThread) {
            QueryInfos.Remove(queryThread.QueryData.QueryInfo);
        }

        internal static void DeleteFromActiveThreads(QueryThread queryThread) {
            lock (_lock) {
                _runningThreads.Remove(queryThread);
            }
            TryToStartNewThreads();
        }

        public static void AbortThreads() {
            lock (_lock) {
                foreach (var runningThread in _runningThreads) {
                    runningThread.Abort();
                }
                foreach(var thread in _threadQueue)
                    thread.Abort();
                    //thread.Value.Abort();

                _runningThreads.Clear();
                _threadQueue.Clear();
                QueryInfos.Clear();
            }            
        }

        public static void DistinctDb(Profile profile, int threadCount) {
            Task.Factory.StartNew(() => {
                                      DistinctDbImpl(profile, threadCount);
                                  }).ContinueWith((ant) => CheckError(ant));
        }

        public static void DistinctDbImpl(Profile profile, int threadCount) {
            DbConfig dbNoDatabase = (DbConfig)profile.DbConfigView.Clone();
            dbNoDatabase.DbName = string.Empty;
            using (IDatabase conn = ConnectionManager.CreateConnection(dbNoDatabase)) {
                conn.Open();
                conn.SetHighTimeout();
                _log.Log(LogLevelEnum.Info, "Starte das Distincten der Datenbank", true);
                DbDistincter distincter = new DbDistincter(profile.DbConfigView, threadCount);
                distincter.Start(false);
                _log.Log(LogLevelEnum.Info, "Distincten der Datenbank fertiggestellt", true);
            }
        }
        #endregion Methods
    }
}
