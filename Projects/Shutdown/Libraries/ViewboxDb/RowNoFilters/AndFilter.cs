using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SystemDb;
using DbAccess;

namespace ViewboxDb.RowNoFilters
{
	public class AndFilter : FilterWithConditions
	{
		public AndFilter()
		{
			_keyWord = "AND";
		}

		public override RangeList GetRowNoRanges(DatabaseBase connection, IndexDb indexDb, IColumn rowNoColumn, List<IOrderArea> areas, long from = -1L, long to = long.MaxValue)
		{
			Stopwatch watch = new Stopwatch();
			watch.Start();
			List<RangeList> ranges = new List<RangeList>();
			foreach (IRowNoFilter item in base.Conditions.ToList())
			{
				RangeList list = item.GetRowNoRanges(connection, indexDb, rowNoColumn, areas, from, to);
				if (list == null || list.Count == 0)
				{
					return null;
				}
				ranges.Add(list);
				from = Math.Max(from, list.Min((Range w) => w.From));
				to = Math.Min(to, list.Max((Range w) => w.To));
			}
			watch.Restart();
			ranges = ranges.OrderBy((RangeList w) => w.Count).ToList();
			RangeList ret = ranges.FirstOrDefault();
			if (ret != null)
			{
				foreach (RangeList other in ranges.Skip(1))
				{
					ret.InterSect(other);
				}
			}
			watch.Stop();
			return ret;
		}
	}
}
