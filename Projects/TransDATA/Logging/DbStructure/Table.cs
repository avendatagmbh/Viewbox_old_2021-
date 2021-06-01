using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Config;
using Logging.Interfaces.DbStructure;
using Utils;
using DbAccess;

namespace Logging.DbStructure {
    /// <summary>
    /// This class represents the structure for a table.
    /// </summary>
    [DbTable("log_tables")]
    internal class Table : NotifyPropertyChangedBase, ITable {

        #region Id
        [DbColumn("id", AllowDbNull = false)]
        [DbPrimaryKey]
        internal long Id { get; set; }
        #endregion

        #region TableId
        [DbColumn("table_id", AllowDbNull = false)]
        [DbIndex("idx_table_id")]
        public long TableId { get; set; }
        #endregion

        #region ProfileId
        [DbColumn("profile_id", AllowDbNull = false)]
        public long ProfileId { get; set; }
        #endregion

        #region Count
        [DbColumn("count")]
        public long Count { get; set; }
        #endregion

        #region Filter
        private string _filter;

        [DbColumn("filter", Length = 1024)]
        public string Filter {
            get { return _filter; }
            set {
                if (_filter == value) return;
                _filter = value;
                OnPropertyChanged("Filter");
            }
        }
        #endregion Filter
        
        #region Columns
        private ObservableCollectionAsync<Column> _columns;

        public IEnumerable<IColumn> Columns { get { return ColumnsDbMapping; } }

        [DbCollection("TableDbMapping", LazyLoad = true)]
        internal ICollection<Column> ColumnsDbMapping {
            get {
                if (_columns == null) LoadColumns();
                return _columns;
            }
        }
        #endregion Columns

        #region InputConfig
        private string _inputConfig;

        [DbColumn("inputconfig")]
        public string InputConfig {
            get { return _inputConfig; }
            set {
                if (_inputConfig == value) return;
                _inputConfig = value;
                OnPropertyChanged("Username");
            }
        }
        #endregion InputConfig

        #region OutputConfig
        private string _outputConfig;

        [DbColumn("outputConfig")]
        public string OutputConfig {
            get { return _outputConfig; }
            set {
                if (_outputConfig == value) return;
                _outputConfig = value;
                OnPropertyChanged("OutputConfig");
            }
        }
        #endregion OutputConfig

        #region Username
        private string _username;
        
        [DbColumn("username", Length = 256)]
        public string Username {
            get { return _username; }
            set {
                if (_username == value) return;
                _username = value;
                if (_username != null && _username.Length > 256) _username = _username.Substring(0, 256);
                OnPropertyChanged("Username");
            }
        }
        #endregion Username

        #region Timestamp
        private DateTime _timestamp;
        [DbColumn("timestamp")]
        public DateTime Timestamp {
            get { return _timestamp; }
            set {
                if (_timestamp == value) return;
                _timestamp = value;
               
                OnPropertyChanged("Timestamp");
            }
        }
        #endregion Timestamp

        #region CountDest
        private long _countdest;
        [DbColumn("count_dest")]
        public long CountDest {
            get { return _countdest; }
            set {
                if (_countdest == value) return;
                _countdest = value;

                OnPropertyChanged("CountDest");
            }
        }
        #endregion CountDest

        #region Duration
        private TimeSpan _duration;
        [DbColumn("duration")]
        public TimeSpan Duration {
            get { return _duration; }
            set {
                if (_duration == value) return;
                _duration = value;

                OnPropertyChanged("Duration");
            }
        }
        #endregion Duration

        #region State
        private ExportStates _state;
        [DbColumn("state")]
        public ExportStates State {
            get { return _state; }
            set {
                if (_state == value) return;
                _state = value;

                OnPropertyChanged("State");
            }
        }
        #endregion State

        #region Error
        private string _error;
        [DbColumn("error")]
        public string Error {
            get { return _error; }
            set {
                if (_error == value) return;
                _error = value;

                OnPropertyChanged("Error");
            }
        }
        #endregion Error

        #region methods

        #region LoadColumns
        private void LoadColumns() {
            _columns = new ObservableCollectionAsync<Column>();
            using (var conn = LoggingDb.ConnectionManager.GetConnection()) {
                foreach (var column in conn.DbMapping.Load<Column>(conn.Enquote("log_table_id") + "=" + Id)) {
                    _columns.Add(column);
                }
            }
        }
        #endregion

        #region AddColumn
        public void AddColumn(IColumn column) {
            if (_columns == null) _columns = new ObservableCollectionAsync<Column>();
            _columns.Add((Column)column);
        }
        #endregion

        #endregion methods
    }
}
