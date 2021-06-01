// --------------------------------------------------------------------------------
// author: Benjamin Held
// since: 2011-10-06
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using DbSearchLogic.SearchCore.Result;
using DbSearchLogic.SearchCore.Structures.Result.DataMapper;
using DbSearchLogic.Structures.TableRelated;
using Utils;

namespace DbSearchLogic.SearchCore.Structures.Result {
    public class ResultHistory : INotifyPropertyChanged {
        #region Events
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName) { if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName)); }
        #endregion Events

        #region Constructor
        public ResultHistory(Query query) {
            Query = query;
            //ColumnToResults = new Dictionary<Column, ColumnResults>();
            Results = new ObservableCollectionAsync<ResultSet>();
            //ColumnResultsList = new ObservableCollectionAsync<ColumnResults>();
        }
        #endregion

        #region Properties
        public Query Query { get; set; }
        //Private dictionary to speed up finding results
        //private Dictionary<Column, ColumnResults> ColumnToResults { get; set; }
        //Stores results which are currectly edited by a search
        private Dictionary<Guid, ResultSet> _currentResults = new Dictionary<Guid, ResultSet>(); 

        public ObservableCollectionAsync<ResultSet> Results { get; set; }
        private bool _loaded = false;
        private ResultSet _recentResults;
        public ResultSet RecentResults {
            get { return _recentResults; }
            set {
                if (_recentResults != value) {
                    _recentResults = value;
                    OnPropertyChanged("RecentResults");
                }
            }
        }

        ResultSetLoader _resultSetLoader = new ResultSetLoader();
        #endregion Properties

        #region Methods
        public void LoadResults() {
            if (!_loaded) {
                Task.Factory.StartNew(() => _resultSetLoader.LoadResultSets(this));
                _loaded = true;
            } else if (RecentResults == null && Results.Count != 0) RecentResults = Results[Results.Count - 1];
        }

        public void AddResultSet(ResultSet resultSet) {
            lock (Results) {
                
                bool found = Results.Any(result => result.DisplayString == resultSet.DisplayString);
                if(!found) Results.Add(resultSet);
            }
        }

        #region Handling generation of results during a search
        public void AddNewResults(Query query, Guid guid) {
            Query snapshotQuery = (Query) query.Clone();
            ResultSet newResultSet = new ResultSet(snapshotQuery);
            lock (_currentResults) {
                _currentResults[guid] = newResultSet;
            }
            lock (Results) {
                Results.Add(newResultSet);
            }

            RecentResults = newResultSet;
        }

        public void FinishedSearch(Guid guid) {
            lock (_currentResults) {
                _currentResults[guid].ResultFinished();
                _currentResults.Remove(guid);
            }
        }

        public void AddTableResult(TableResultSet tableResult, Guid guid) {
            lock (_currentResults) {
                _currentResults[guid].AddTableResult(tableResult);
            }
        }
        #endregion Handling generation of results during a search

        public void DeleteResult(ResultSet result) {
            Query.DbQuery.DeleteResult(result.DbResultSet);
            Results.Remove(result);
            if (Results.Count > 0)
                RecentResults = Results[Results.Count - 1];
        }
        #endregion Methods


        public void UnloadResults()
        {
            foreach (var result in Results)
                if (!_currentResults.Values.Contains(result))
                    result.Unload();
            _loaded = false;
            Results.Clear();
        }
    }
}
