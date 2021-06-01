using System.Collections.Generic;
using SystemDb;
using DbAccess;

namespace ViewboxDb.RowNoFilters
{
	public interface IRowNoFilter
	{
		RangeList GetRowNoRanges(DatabaseBase connection, IndexDb indexDb, IColumn rowNoColumn, List<IOrderArea> areas, long from = -1L, long to = long.MaxValue);

		string ToConditionString(DatabaseBase connection, string prefix);
	}
}
