using System;
using System.Threading.Tasks;
using System.Timers;
using DbAccess;
using DbSearchLogic.SearchCore.QueryExecution;
using DbSearchLogic.SearchCore.Result;
using DbSearchLogic.SearchCore.Structures;
using DbSearchLogic.SearchCore.Structures.Db;
using Utils;
using log4net;
using AV.Log;

namespace DbSearchLogic.SearchCore.Threading {
    internal class TableSearchThread : NotifyPropertyChangedBase, IThreadExecutor {

        internal ILog _log = LogHelper.GetLogger();

        public TableSearchThread(ThreadManager.QueryData data, TableInfo tableInfo, int priority) {
            _data = data;
            _tableInfo = tableInfo;
            _data.QueryInfo.State.NextColumnEvent += State_NextColumnEvent;
            ThreadPriority = priority;
        }

        ~TableSearchThread() {
            _data.QueryInfo.State.NextColumnEvent -= State_NextColumnEvent;
            if(_timer != null) _timer.Elapsed -= timer_Elapsed;
        }
        #region Methods

        public void CreateThread(TaskCreationOptions options) {
            _task = new Task(DoWork, options);
        }

        private void DoWork() {
            if (_abort) return;
            _timer.Elapsed += timer_Elapsed;
            _timer.Start();
            
            using (IDatabase conn = ConnectionManager.CreateConnection(QueryData.QueryInfo.QueryConfig.ViewConfig)) {
                conn.Open();
                conn.SetHighTimeout();
                try {
                    TableResultSet tableResult;
                    TableSearcher.SearchTable(_tableInfo, conn, QueryData.QueryInfo, QueryData.GlobalSearchConfig, out tableResult);
                    this.TableResult = tableResult;
                } catch (Exception ex) {
                    _log.Log(LogLevelEnum.Error, "Fehler bei der Suche in Tabelle " + CurrentTable + "." + CurrentColumn + ": " + ex.Message);
                }
            }
            ThreadManager.TableSearchFinished(this);
        }

        public void StartThread() {
            _task.Start();
        }

        public void Abort() {
            _abort = true;
            QueryData.QueryInfo.Cancel = true;
        }

        public override string ToString() {
            return "TableSearchThread: " + _tableInfo.Name;
        }
        #endregion Methods

        #region Properties
        private bool _abort = false;
        private ThreadManager.QueryData _data;
        private TableInfo _tableInfo;
        public ThreadManager.QueryData QueryData { get { return _data; } }
        public TableResultSet TableResult { get; set; }

        private Task _task;
        public Task Task {
            get { return _task; }
        }

        public int ThreadPriority { get; set; }

        public string QueryName {
            get { return _data.Query.Name; }
        }

        public string Description {
            get { return "Durchsuche"; }
        }

        public string CurrentTable {
            get {
                return _tableInfo.Name;
            }
        }

        public string CurrentColumn {
            get {
                QueryTableState state = QueryData.QueryInfo.State.GetTableState(_tableInfo.Name);
                if (state == null) return "";
                return state.CurrentColumnName;
            }
        }

        public string TableCount {
            get {
                QueryTableState state = QueryData.QueryInfo.State.GetTableState(_tableInfo.Name);
                if (state == null) return "";
                return ""+ state.TableCount;
            }
        }

        public string TimeWorked {
            get {
                QueryTableState state = QueryData.QueryInfo.State.GetTableState(_tableInfo.Name);
                if (state == null) return "";
                TimeSpan span = state.Stopwatch.Elapsed.Duration();
                return string.Format("{0:D2}:{1:D2}", span.Minutes, span.Seconds);
            }
        }

        Timer _timer = new Timer(1000.0);
        private bool _tableCountReady;
        #endregion Properties

        #region EventHandler
        void timer_Elapsed(object sender, ElapsedEventArgs e) {
            OnPropertyChanged("TimeWorked");
        }

        void State_NextColumnEvent(object sender, EventArgs e) {
            //OnPropertyChanged("CurrentQuery");
            if (!_tableCountReady && QueryData.QueryInfo.State.GetTableState(_tableInfo.Name) != null) {
                _tableCountReady = true;
                OnPropertyChanged("TableCount");
            }
            OnPropertyChanged("CurrentColumn");
            //OnPropertyChanged("TableCount");
        }
        #endregion EventHandler
    }
}
