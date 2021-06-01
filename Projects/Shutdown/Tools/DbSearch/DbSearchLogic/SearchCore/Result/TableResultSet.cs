using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using DbSearchLogic.SearchCore.Structures.Db;

namespace DbSearchLogic.SearchCore.Result {
    /// <summary>
    /// ResultSet stores information about full-, row- and column-hits of a table
    /// </summary>
    public class TableResultSet : IComparable<TableResultSet> {
        public TableResultSet(TableInfo tableInfo) {
            TableInfo = tableInfo;
            ColumnHits = new ObservableCollection<ColumnHit>();
        }

        #region Properties

        public TableInfo TableInfo { get; set; }

        public ObservableCollection<ColumnHit> ColumnHits { get; set; }

        public int NumberOfColumnHits {
            get { return ColumnHits.Count; }
        }

        #endregion Properties

        public override string ToString() {
            return this.TableInfo.Name;
        }

        public int CompareTo(TableResultSet oResultSet) {
            if (oResultSet == null) {
                return 1;
            }
            return TableInfo.Name.CompareTo(oResultSet.TableInfo.Name);
        }
    }
}