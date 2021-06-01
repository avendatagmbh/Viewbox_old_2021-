// --------------------------------------------------------------------------------
// author: Benjamin Held
// since: 2011-09-14
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------

using System.Collections.ObjectModel;
using System.ComponentModel;
using DbSearchLogic.Structures.TableRelated;
using System.Collections.Generic;
using Utils;

namespace DbSearchLogic.SearchCore.Result
{
    /// <summary>
    /// ResultSet stores the search matrix, search parameters and all of the results
    /// </summary>
    //public class ResultSet : INotifyPropertyChanged {
    //    #region Events
    //    public event PropertyChangedEventHandler PropertyChanged;
    //    private void OnPropertyChanged(string propertyName) { if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName)); }
    //    #endregion Events

    //    #region Constructor

    //    public ResultSet(Query query) {
    //        this.TableResultSets = new ObservableCollection<TableResultSet>();
    //        this.Query = query;
    //        //ColumnResults = new SortedDictionary<string, ColumnResult>();
    //        ColumnResults = new ObservableCollectionAsync<ColumnResult>();
    //    }


    //    #endregion Constructor

    //    #region Properties
    //    public ObservableCollection<TableResultSet> TableResultSets { get; set; }
    //    //public SortedDictionary<string, ColumnResult> ColumnResults { get; set; }
    //    public ObservableCollectionAsync<ColumnResult> ColumnResults { get; set; }
    //    protected Query Query { get; set; }
    //    #endregion Properties

    //    public void AddTableResult(TableResultSet tableResult) {
    //        if (tableResult == null) return;

    //        TableResultSets.Add(tableResult);

    //        lock (ColumnResults) {
    //            if (ColumnResults.Count == 0) {
    //                foreach (var column in Query.Columns)
    //                    ColumnResults.Add(new ColumnResult(column));
    //            }
    //        }
    //        foreach(var columnResult in ColumnResults)
    //            columnResult.AddTableResult(tableResult);
    //    }

    //    public void Clear() {
    //        TableResultSets.Clear();
    //        ColumnResults.Clear();
    //    }
    //}
}