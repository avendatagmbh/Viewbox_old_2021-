using System.Collections.Generic;
using SystemDb;
using DbAccess;

namespace ViewboxDb.RowNoFilters
{
	public class NotEmptyOrFilter : IRowNoFilter
	{
		public object Value { get; set; }

		public IRowNoFilter Condition { get; set; }

		public RangeList GetRowNoRanges(DatabaseBase connection, IndexDb indexDb, IColumn rowNoColumn, List<IOrderArea> areas, long from = -1L, long to = long.MaxValue)
		{
			if (Value != null && !string.IsNullOrEmpty(Value.ToString()) && !"''".Equals(Value.ToString()) && Value.ToString().ToLower() != "null")
			{
				return new RangeList
				{
					new Range
					{
						From = from,
						To = to
					}
				};
			}
			return Condition.GetRowNoRanges(connection, indexDb, rowNoColumn, areas, from, to);
		}

		public string ToConditionString(DatabaseBase connection, string prefix)
		{
			if (Value != null && !string.IsNullOrEmpty(Value.ToString()) && !"''".Equals(Value.ToString()) && Value.ToString().ToLower() != "null")
			{
				return "";
			}
			return Condition.ToConditionString(connection, prefix);
		}
	}
}
