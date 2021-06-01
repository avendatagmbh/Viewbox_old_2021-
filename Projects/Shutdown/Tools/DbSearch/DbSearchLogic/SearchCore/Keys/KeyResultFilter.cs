using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using DbSearchBase.Enums;
using DbSearchBase.Interfaces;

namespace DbSearchLogic.SearchCore.Keys
{
    public class KeyResultFilter : IKeyResultFilter {

        public const string delimiter = ";";

        private string columnBlackList;
        public string ColumnBlackList {
            get { return columnBlackList; } 
            set { 
                if (columnBlackList == value) return;
                columnBlackList = value.Trim();
                ResultFilterType = ResultFilterTypeEnum.BlackList;
                columnWhiteList = null;
                columnFilter = null;
            } 
        }

        private string columnWhiteList;
        public string ColumnWhiteList {
            get { return columnWhiteList; } 
            set {
                if (columnWhiteList == value) return;
                columnWhiteList = value.Trim();
                ResultFilterType = ResultFilterTypeEnum.WhiteList;
                columnBlackList = null;
                columnFilter = null;
            } 
        }

        public ResultFilterTypeEnum ResultFilterType { get; internal set; }
        public IEnumerable<BindableDbColumnType> ColumnTypesToShow { get; set; }
        private IEnumerable<IFilterColumn> columnFilter;
        public IEnumerable<IFilterColumn> ColumnFilter { 
            get {
                if (columnFilter == null) {
                    string source = ResultFilterType == ResultFilterTypeEnum.WhiteList ? ColumnWhiteList : ColumnBlackList;
                    if (string.IsNullOrEmpty(source)) return new List<IFilterColumn>();
                    columnFilter = source.Split(new string[] {delimiter}, StringSplitOptions.RemoveEmptyEntries).Select(s => new ColumnFilter(s.ToLower()));
                }
                return columnFilter;
            } 
        }

        public bool IsVisible(IEnumerable<IColumn> columns) {
            bool result = false;
            result = columns.Any(c => ColumnTypesToShow.Any(df => df.IsSelected && df.DbType == c.ColumnType));
            
            if (ColumnFilter == null || ColumnFilter.Count() == 0) return result;

            if (result) {
                foreach (IColumn column in columns) {
                    if (ResultFilterType == ResultFilterTypeEnum.BlackList && IsMatch(column)) return false;
                    else {
                        result = IsMatch(column);
                        if (result) break;
                    }
                }
            }
            return result;
        }

        private bool IsMatch(IColumn column) {
            bool result = false;

            foreach (IFilterColumn filterColumn in ColumnFilter) {
                switch (filterColumn.ColumnFiterType) {
                    case ColumnFilterTypeEnum.Contains:
                        result = column.ColumnName.ToLower().Contains(filterColumn.ColumnName);
                        break;
                    case ColumnFilterTypeEnum.ExactMatch:
                        result = string.Compare(column.ColumnName,filterColumn.ColumnName, StringComparison.InvariantCultureIgnoreCase) == 0;
                        break;
                    case ColumnFilterTypeEnum.Postfixed:
                        result = column.ColumnName.ToLower().StartsWith(filterColumn.ColumnName);
                        break;
                    case ColumnFilterTypeEnum.Prefixed:
                        result = column.ColumnName.ToLower().EndsWith(filterColumn.ColumnName);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                if (result) return true;
            }
            return result;
        }
    }

    public class ColumnFilter : IFilterColumn {

        public const string wildcard = "*";

        public ColumnFilter(string columnName) {
            columnName = columnName.Trim();
            ColumnFiterType = ColumnFilterTypeEnum.ExactMatch;
            ColumnName = columnName;
            if (columnName.StartsWith(wildcard)) {
                ColumnFiterType = ColumnFilterTypeEnum.Prefixed;
                ColumnName = columnName.Substring(1);
            } 
            if (columnName.EndsWith(wildcard)) {
                if (ColumnFiterType == ColumnFilterTypeEnum.Prefixed) {
                    ColumnFiterType = ColumnFilterTypeEnum.Contains;
                    ColumnName = ColumnName.Substring(0, ColumnName.Length - 1);
                } else {
                    ColumnFiterType = ColumnFilterTypeEnum.Postfixed;
                    ColumnName = columnName.Substring(0, ColumnName.Length - 1);
                }
            }
        }

        public ColumnFilterTypeEnum ColumnFiterType { get; internal set; }
        public string ColumnName { get; internal set; }
    }

}
