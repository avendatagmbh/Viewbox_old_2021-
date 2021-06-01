using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Diagnostics;
using AvdCommon.Logging;
using DbSearchLogic.Manager;

namespace DbSearchLogic.SearchCore.Structures {

    /// <summary>
    /// Klasse zur Verwaltung der Statusinformationen eines Threads.
    /// </summary>
    /// <author>Mirko Dibbert</author>
    /// <company>AvenDATA GmbH</company>
    /// <since>27.01.2010</since>
    public class QueryState : INotifyPropertyChanged {
        #region Events
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName) { if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName)); }
        public event EventHandler NextColumnEvent;
        private void OnNextColumn() { if (NextColumnEvent != null) NextColumnEvent(this, null); }
        #endregion Events

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryState"/> class.
        /// </summary>
        public QueryState() {
            CurrentTablesDict = new Dictionary<string, QueryTableState>();
            LockObject = new object();
            this.Stopwatch = new Stopwatch();
        }

        /// <summary>
        /// Resets this instance.
        /// </summary>
        public void Reset() {
            lock (LockObject) {
                TablesTotal = 0;
                TablesFinished = 0;
                TablesProcessed = 0;
            }
        }

        /// <summary>
        /// Startzeitpunkt der Suche.
        /// </summary>
        public Stopwatch Stopwatch { get; private set; }

        /// <summary>
        /// Liste aller aktuell bearbeiteten Tabellen.
        /// </summary>
        private Dictionary<string, QueryTableState> CurrentTablesDict { get; set; }

        /// <summary>
        /// Sperrobjekt.
        /// </summary>
        private object LockObject { get; set; }

        /// <summary>
        /// Gesamtfortschritt der Datenbanksuche.
        /// </summary>
        float _progress;
        public float Progress {
            get { return _progress; }
        }

        public void Init(int nTablesTotal) {
            TablesTotal = nTablesTotal;
            Stopwatch.Start();
        }

        /// <summary>
        /// Gesamtanzahl aller Tabellen.
        /// </summary>
        private int _tablesTotal;
        public int TablesTotal {
            get { 
                return _tablesTotal; 
            }
            
            private set { 
                lock (LockObject) { 
                    _tablesTotal = value;
                    OnPropertyChanged("TablesTotal");
                } 
            }
        }

        /// <summary>
        /// Anzahl der bearbeiteten Tabellen (wird automatisch aktualisiert).
        /// </summary>
        int _tablesFinished;
        public int TablesFinished {
            get { return _tablesFinished; }
            private set {
                lock (LockObject) {
                    _tablesFinished = value;
                    OnPropertyChanged("TablesFinished");
                } 
            }

        }

        internal void TableStarted(string tableName, string hostname, int columns, long tableCount) {
            lock (LockObject) {
                CurrentTablesDict[tableName] = new QueryTableState(tableName, hostname, columns, tableCount);
                TablesProcessed++;
                UpdateProgress();
            }
        }

        private int TablesProcessed { get; set; }

        internal void TableFinished(string tableName) {
            lock (LockObject) {
                CurrentTablesDict.Remove(tableName);
                TablesFinished++;
                UpdateProgress();
                //LogManager.Status("Tabelle fertiggestellt: " + tableName);
            }
        }

        internal void NextColumn(string tableName, string columnName) {
            lock (LockObject) {
                CurrentTablesDict[tableName].NextColumn(columnName);
                UpdateProgress();
                OnNextColumn();
            }
        }

        internal void ColumnFinished(string sTableName) {
            lock (LockObject) {
                CurrentTablesDict[sTableName].ColumnFinished();
                UpdateProgress();
            }
        }

        private void UpdateProgress() {
            double nResult = 0;
            lock (LockObject) {
                double nProgressPerTable = 0;
                if (TablesTotal > 0) {
                    nProgressPerTable = ((double) 1/TablesTotal);

                    if (TablesFinished > 0) {
                        nResult = ((double) TablesFinished/(double) TablesTotal);
                    }
                }
                foreach (QueryTableState oTableState in CurrentTablesDict.Values) {
                    nResult += nProgressPerTable * oTableState.Progress;
                }
            }
            nResult *= 100;
            if(_progress != System.Math.Min(100, nResult)) {
                _progress = (float) System.Math.Min(100, nResult);
                OnPropertyChanged("Progress");
            }
        }

        public QueryTableState GetTableState(string tableName) {
            lock (LockObject) {
                if (CurrentTablesDict.ContainsKey(tableName)) return CurrentTablesDict[tableName];
            }
            return null;
        }
        /// <summary>
        /// Statusobjekte aller aktuell bearbeiteten Tabellen.
        /// </summary>
        public List<QueryTableState> CurrentTables {
            get {
                List<QueryTableState> oResult = null;
                lock (LockObject) {
                    oResult = CurrentTablesDict.Values.ToList();
                }
               
                return oResult;
               
            }
        }

    }
}
