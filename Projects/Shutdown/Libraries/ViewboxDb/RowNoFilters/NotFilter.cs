using System.Collections.Generic;
using SystemDb;
using DbAccess;

namespace ViewboxDb.RowNoFilters
{
	public class NotFilter : IRowNoFilter
	{
		public IRowNoFilter Condition { get; set; }

		public RangeList GetRowNoRanges(DatabaseBase connection, IndexDb indexDb, IColumn rowNoColumn, List<IOrderArea> areas, long from = -1L, long to = long.MaxValue)
		{
			RangeList ret = Condition.GetRowNoRanges(connection, indexDb, rowNoColumn, areas, from, to);
			ret?.Not(-1L);
			return ret;
		}

		public string ToConditionString(DatabaseBase connection, string prefix)
		{
			return "NOT " + Condition.ToConditionString(connection, prefix);
		}
	}
}
