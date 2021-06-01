// --------------------------------------------------------------------------------
// author: Benjamin Held
// since: 2011-10-06
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.IO;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Xml;
using DbSearchDatabase.Factories;
using DbSearchDatabase.Interfaces;
using DbSearchLogic.SearchCore.Result;
using DbSearchLogic.SearchCore.Structures.Result.DataMapper;
using DbSearchLogic.Structures.TableRelated;
using Utils;

namespace DbSearchLogic.SearchCore.Structures.Result {
    public class ResultSet : NotifyPropertyChangedBase{
        #region Events
        public class ResultsChangedEventArgs : EventArgs {
        }
        public delegate void ResultChangedEvent(object sender, ResultsChangedEventArgs e);
        public event ResultChangedEvent ResultsChanged;
        public void OnResultsChanged(ResultsChangedEventArgs e) {
            ResultChangedEvent handler = ResultsChanged;
            if (handler != null) handler(this, e);
        }
        #endregion Events

        #region Constructor
        public ResultSet(Query snapshotQuery, IDbResultSet dbResultSet) {
            Init(snapshotQuery, dbResultSet);
        }

        public ResultSet(Query snapshotQuery) {

            IDbResultSet dbResultSet = DatabaseObjectFactory.CreateResultSet(snapshotQuery.DbQuery);
            dbResultSet.Timestamp = DateTime.Now;
            Init(snapshotQuery, dbResultSet);
        }

        private void Init(Query snapshotQuery, IDbResultSet dbResultSet) {
            SnapshotQuery = snapshotQuery;
            ColumnResults = new ObservableCollectionAsync<ColumnResult>();
            foreach (var column in snapshotQuery.Columns)
                if(column.IsUsedInSearch) ColumnResults.Add(new ColumnResult(column));
            DbResultSet = dbResultSet;
        }
        #endregion

        #region Properties
        internal IDbResultSet DbResultSet { get; set; }

        public string Comment { get { return DbResultSet.Comment; } }
        public DateTime Timestamp { get { return DbResultSet.Timestamp; } }
        internal Query SnapshotQuery { get; private set; }
        public ObservableCollectionAsync<ColumnResult> ColumnResults { get; set; }
        public string DisplayString { get { return "Ergebnisse vom " + Timestamp.ToString() + (!Finished ? " (nicht zu Ende gesucht)" : string.Empty); } }
        private ResultSaver _resultSaver;
        private object _lock = new object();
        private bool _loaded = false;
        public bool Finished {
            get { return DbResultSet.Finished; }
            set { 
                DbResultSet.Finished = value;
                OnPropertyChanged("Finished");
                OnPropertyChanged("DisplayString");
            }
        }

        public string SearchConfigString {
            get {
                if (string.IsNullOrEmpty(SnapshotQuery.SearchTableDecider.BlacklistTablesAsString()) && string.IsNullOrEmpty(SnapshotQuery.SearchTableDecider.WhitelistTablesAsString()))
                    return null;
                string result = "";
                if(!string.IsNullOrEmpty(SnapshotQuery.SearchTableDecider.WhitelistTablesAsString()))
                    result = "Whitelist: " + SnapshotQuery.SearchTableDecider.WhitelistTablesAsString();
                else if (!string.IsNullOrEmpty(SnapshotQuery.SearchTableDecider.BlacklistTablesAsString()))
                    result = "Blacklist: " + SnapshotQuery.SearchTableDecider.BlacklistTablesAsString();
                return result;
            }
        }
        #endregion Properties

        #region Methods

        public Task Load() {
            if (!_loaded) {
                //ResultLoader resultLoader = new ResultLoader();
                Task task = Task.Factory.StartNew(() => ColumnResultsLoader.LoadColumnResults(this));
                _loaded = true;
                return task;
            }
            return null;
            //ResultSet resultSet = new ResultSet(Query);
        }

        public void AddTableResult(TableResultSet tableResult) {
            lock (_lock) {
                if (_resultSaver == null) {
                    _resultSaver = new ResultSaver(this, SnapshotQuery.Profile.DbProfile.GetProfileConfig());
                    //_loaded = true;
                }
            }
            foreach (var columnResult in ColumnResults)
                columnResult.AddTableResult(tableResult);
            OnResultsChanged(new ResultsChangedEventArgs());
        }

        public void ResultFinished() {
            Finished = true;
            if (_resultSaver != null)
                _resultSaver.Finished();
        }

        public override string ToString() {
            return DisplayString;
        }

        public void Unload() {
            _loaded = false;
            foreach (var columnResult in ColumnResults)
                columnResult.ColumnHits.Clear();
            GC.Collect();
        }
        #endregion Methods

    }
}
