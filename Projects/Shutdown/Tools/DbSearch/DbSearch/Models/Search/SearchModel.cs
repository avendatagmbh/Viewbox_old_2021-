// --------------------------------------------------------------------------------
// author: Benjamin Held
// since: 2011-09-12
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Linq;
using DbSearch.Models.Result;
using DbSearch.Models.Rules;
using DbSearchLogic.SearchCore.Threading;
using DbSearchLogic.Structures.TableRelated;

namespace DbSearch.Models.Search {
    public class SearchModel {
        #region Constructor

        public SearchModel(Query query) {
            Query = query;
            TableMappingsModel = new TableMappingsModel(query);
            ColumnOverviewModel = new ColumnOverviewModel(query);
        }

        #endregion Constructor

        #region Properties

        public Query Query { get; set; }
        public TableMappingsModel TableMappingsModel { get; set; }
        public ColumnOverviewModel ColumnOverviewModel { get; private set; }
    
        public int NonUserdefinedColumnsCount {
            get { return Query.Columns.Count(column => !column.IsUserDefined); }
        }

        #endregion Properties

        internal void StartSearch() {
            ThreadManager.StartNewTableSearch(Query);
        }

        public void OptimalRows() {
            Query.OptimalRows(15);
        }

        public void SaveQuery() {
            Query.Save();
        }

        public void ChangeRows(int newRowCount) {
            if (Query.Rows.Count == newRowCount) return;
            if (newRowCount > Query.Rows.Count) {
                Query.AddRows(newRowCount - Query.Rows.Count);
            }else {
                while(Query.Rows.Count > newRowCount)
                    Query.RemoveRow(Query.Rows[Query.Rows.Count-1]);
            }
        }

        public bool ChangeColumns(int newColumnCount) {
            if (Query.Columns.Count == newColumnCount || newColumnCount < NonUserdefinedColumnsCount) return false;
            if (newColumnCount > Query.Columns.Count) {
                Query.AddColumns(newColumnCount - Query.Columns.Count);
            } else Query.DeleteColumns(Query.Columns.Count - newColumnCount);
            return true;
        }
    }
}
