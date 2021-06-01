// --------------------------------------------------------------------------------
// author: Benjamin Held
// since: 2011-10-18
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DbSearchLogic.SearchCore.Structures.Result;
using DbSearchLogic.Structures.TableRelated;
using Utils;

namespace DbSearch.Structures.Results {
    public class ColumnResultView : NotifyPropertyChangedBase{
        public ColumnResultView(ColumnResult columnResult, int threshold, Query query) {
            this.ColumnResult = columnResult;
            _threshold = threshold;
            Query = query;
            this.ColumnResult.Column.MappedTo = Query.UserColumnMappings.ColumnMappings.FirstOrDefault(cm => cm.SearchColumn.Name == cm.SearchColumn.Name);
        }

        internal ColumnResult ColumnResult { get; set; }
        public Query Query { get; private set; }
        public string ColumnName { get { return ColumnResult.Column.Name; } }
        public int TableHitsCount { get { return ColumnResult.TableHits(_threshold / 100.0f); } }
        public int ColumnHitsCount { get { return ColumnResult.ColumnHits.Count(columnHitInfo => columnHitInfo.HitPercentage >= _threshold / 100.0f); } }
        public int SearchValueCount { get { return ColumnResult.Column.Query.SearchValuesForColumn(ColumnResult.Column); } }
        private int _threshold;
    }
}
