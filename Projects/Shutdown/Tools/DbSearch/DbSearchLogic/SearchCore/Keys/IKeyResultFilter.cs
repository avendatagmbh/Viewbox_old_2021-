using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DbSearchBase.Enums;
using DbSearchBase.Interfaces;

namespace DbSearchLogic.SearchCore.Keys {

    public enum ResultFilterTypeEnum {
        WhiteList = 0,
        BlackList = 1,
    }

    public enum ColumnFilterTypeEnum {
        ExactMatch = 0,
        Prefixed = 1,
        Postfixed = 2,
        Contains = 3,
    }

    public interface IKeyResultFilter {

        string ColumnBlackList { get; set; }
        string ColumnWhiteList { get; set; }
        ResultFilterTypeEnum ResultFilterType { get; }
        IEnumerable<IFilterColumn> ColumnFilter { get; }
        IEnumerable<BindableDbColumnType> ColumnTypesToShow { get; set; }
        bool IsVisible(IEnumerable<IColumn> columns);
    }

    public interface IFilterColumn {
        ColumnFilterTypeEnum ColumnFiterType { get; }
        string ColumnName { get; }
    }
}
