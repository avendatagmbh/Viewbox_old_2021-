using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using DbSearch.Structures.Results;
using DbSearchLogic.SearchCore.Result;
using DbSearchLogic.SearchCore.Structures.Result;
using DbSearchLogic.Structures.TableRelated;

namespace DbSearch.Models.Result {

    /// <summary>
    /// MApping changed event handler
    /// </summary>
    /// <param name="sender">Sender</param>
    /// <param name="e">Event args</param>
    public delegate void MappingChangedEventHandler(object sender, MappingChangedEventArgs e);
    /// <summary>
    /// MApping changed event args
    /// </summary>
    public class MappingChangedEventArgs : EventArgs {
        public MappingChangedEventArgs(bool mapped, ColumnHitInfoView columnHitInfoView) {
            Mapped = mapped;
            ColumnHitInfoView = columnHitInfoView;
        }

        public ColumnHitInfoView ColumnHitInfoView { get; internal set; }

        public bool Mapped { get; internal set; }
    }

    public class ResultsModel : INotifyPropertyChanged {
        public ResultsModel() {
        }

        #region Events
        public event EventHandler ColumnResultViewChanged;

        public void OnColumnResultViewChanged(EventArgs e) {
            EventHandler handler = ColumnResultViewChanged;
            if (handler != null) handler(this, e);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName) { if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName)); }
        #endregion Events

        public event MappingChangedEventHandler ColumnMappingChanged;
        private void OnColumnMappingChanged(MappingChangedEventArgs e) {
            MappingChangedEventHandler handler = ColumnMappingChanged; if (handler != null) handler(this, e);
        }

        #region Properties
        private object _lock = new object();

        #region Query
        private Query _query;
        public Query Query {
            get { return _query; }
            set {
                if (_query != value) {
                    if (_query != null)
                    {
                        _query.ResultHistory.UnloadResults();
                    }

                    _query = value;
                    if (_query != null) {
                        _query.ResultHistory.PropertyChanged -= ResultHistory_PropertyChanged;
                        _query.ResultHistory.LoadResults();
                        _query.ResultHistory.PropertyChanged += ResultHistory_PropertyChanged;
                        lock (_lock) {
                            SelectedResults = _query.ResultHistory.RecentResults;
                        }
                    }
                    OnPropertyChanged("Query");
                }
            }
        }
        #endregion Query

        #region SelectedResults
        private ResultSet _selectedResults;
        public ResultSet SelectedResults {
            get { return _selectedResults; }
            set {
                if (_selectedResults != value) {
                    if (_selectedResults != null)
                    {
                        _selectedResults.ResultsChanged -= _selectedResults_ResultsChanged;
                        _selectedResults.Unload();
                    }
                    _selectedResults = value;
                    if (_selectedResults != null) {
                        _selectedResults.ResultsChanged -= _selectedResults_ResultsChanged;
                        _selectedResults.ResultsChanged += _selectedResults_ResultsChanged;
                        Task task = _selectedResults.Load();
                        if (task != null) task.ContinueWith(tmp => CreateColumnResults()).ContinueWith(lastTask => NewResults = false);
                        else CreateColumnResults();
                    } else ColumnResultsView = null;
                    ResultView = null;
                    OnPropertyChanged("ResultView");
                    OnPropertyChanged("SearchConfigString");
                    OnPropertyChanged("SelectedResults");
                    NewResults = false;
                }
            }
        }

        void _selectedResults_ResultsChanged(object sender, ResultSet.ResultsChangedEventArgs e) {
            NewResults = true;
            //CreateColumnResults();
        }
        #endregion SelectedResults

        #region NewResults
        private bool _newResults;
        public bool NewResults {
            get { return _newResults; }
            set {
                if (_newResults != value) {
                    _newResults = value;
                    OnPropertyChanged("NewResults");
                }
            }
        }
        #endregion NewResults

        public string SearchConfigString {
            get {
                if (SelectedResults == null)
                    return string.Empty;
                return SelectedResults.SearchConfigString;
            }
        }

        public IEnumerable<ColumnHitInfoView> ResultView { get; set; }

        #region ColumnResultsView
        private IEnumerable<ColumnResultView> _columnResultsView;
        public IEnumerable<ColumnResultView> ColumnResultsView {
            get { return _columnResultsView; }
            set {
                if (_columnResultsView != value) {
                    _columnResultsView = value;
                    OnPropertyChanged("ColumnResultsView");
                    OnColumnResultViewChanged(null);
                }
            }
        }
        #endregion ColumnResultsView

        #region Threshold
        private int _threshold = 100;
        public int Threshold {
            get { return _threshold; }
            set {
                if (_threshold != value) {
                    _threshold = value;
                    CreateColumnResults();
                    OnPropertyChanged("Threshold");
                }
            }
        }

        #endregion Threshold

        #endregion

        #region Methods
        internal void CreateColumnResults() {
            if (_selectedResults != null) {
                ColumnResultsView = (from columnResult in _selectedResults.ColumnResults
                                        select new ColumnResultView(columnResult, Threshold, Query)).ToList();
            }
        }

        public IEnumerable<ColumnHitInfoView> CreateResultView(IList selectedItems) {
            List<ColumnHitInfoView> result = null;
            //foreach (var selectedItem in selectedItems) {
            //ColumnResult selected = ((ColumnResultView)selectedItem).ColumnResult;
            foreach (ColumnResultView selectedItem in selectedItems)
            {
                ColumnResult selected = selectedItem.ColumnResult;
                selected.Load();
                int id = 1;
                IEnumerable<ColumnHitInfoView> temp =
                         from columnHitInfo in selected.ColumnHits
                         where columnHitInfo.HitPercentage >= Threshold / 100.0f
                         select new ColumnHitInfoView(
                             id++,
                             columnHitInfo.TableInfo, columnHitInfo,
                             selected.Column,
                             //selected.Column.Query)
                             selectedItem.Query)
                         ;
                if (selectedItem == selectedItems[0])
                    result = temp.ToList();
                else {
                    //Get tables which the result shall contain
                    var tablesIntersection = result.Intersect(temp, new ColumnResultViewComparer()).ToList();
                    
                    //Add all columns
                    foreach (var element in temp)
                        if (result.Contains(element, new ColumnResultViewComparer()) && !result.Contains(element, new ColumnResultViewComparer { IncludeColumn = true }))
                            result.Add(element);
                    
                    var resultCopy = result.ToList();
                    //Delete all tables which are not in result and temp
                    foreach (var element in resultCopy) {
                        if (!tablesIntersection.Contains(element, new ColumnResultViewComparer()))
                            result.Remove(element);
                    }
                }
            }
            return result;
        }

        public void SelectionChanged(IList selectedItems) {
            var result = CreateResultView(selectedItems);
            ResultView = result == null ? null : result.ToList();
            OnPropertyChanged("ResultView");
        }

        public void AddMapping(ColumnHitInfoView view) {
            view.AddMapping();
            OnColumnMappingChanged(new MappingChangedEventArgs(true, view));
        }

        public void DeleteMapping(ColumnHitInfoView view) {
            view.RemoveMapping();
            OnColumnMappingChanged(new MappingChangedEventArgs(false, view));
        }
        public void DeleteSelectedResult() {
            if (SelectedResults == null) return;
            Query.ResultHistory.DeleteResult(SelectedResults);

        }

        public List<ColumnResultView> SelectAllWithResultsInTable(string tableName) {
            List<ColumnResultView> result = new List<ColumnResultView>();
            foreach (var columnResultView in ColumnResultsView) {
                IEnumerable<ColumnHitInfoView> hitInfos = CreateResultView(new List<ColumnResultView> {columnResultView});
                foreach(var hitInfo in hitInfos)
                    if (hitInfo.TableName == tableName) {
                        result.Add(columnResultView);
                        break;
                    }
            }
            return result;
        }

        internal void Reload() {
            ResultSet currentResultSet = SelectedResults;
            SelectedResults = null;
            SelectedResults = currentResultSet;
            NewResults = false;
        }
        #endregion Methods

        #region EventHandler
        void ResultHistory_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            if (e.PropertyName == "RecentResults") {
                ResultHistory history = sender as ResultHistory;
                lock (_lock) {
                    if (history != null) SelectedResults = history.RecentResults;
                }
            }
        }
        #endregion EventHandler


    }
}
