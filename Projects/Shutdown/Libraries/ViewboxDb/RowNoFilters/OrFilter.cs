using System.Collections.Generic;
using System.Linq;
using SystemDb;
using DbAccess;

namespace ViewboxDb.RowNoFilters
{
	public class OrFilter : FilterWithConditions
	{
		public OrFilter()
		{
			_keyWord = "OR";
		}

		public override RangeList GetRowNoRanges(DatabaseBase connection, IndexDb indexDb, IColumn rowNoColumn, List<IOrderArea> areas, long from = -1L, long to = long.MaxValue)
		{
			RangeList ret = new RangeList();
			foreach (IRowNoFilter item in base.Conditions.ToList())
			{
				RangeList current = item.GetRowNoRanges(connection, indexDb, rowNoColumn, areas, from, to);
				ret.Join(current);
			}
			return ret;
		}
	}
}
