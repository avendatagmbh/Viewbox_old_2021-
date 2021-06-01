using System.Collections.Generic;
using System.Linq;
using SystemDb;
using DbAccess;

namespace ViewboxDb.RowNoFilters
{
	public abstract class FilterWithConditions : IRowNoFilter
	{
		protected string _keyWord;

		public ISet<IRowNoFilter> Conditions { get; set; }

		public string KeyWord => _keyWord;

		public FilterWithConditions()
		{
			Conditions = new HashSet<IRowNoFilter>();
		}

		public abstract RangeList GetRowNoRanges(DatabaseBase connection, IndexDb indexDb, IColumn rowNoColumn, List<IOrderArea> areas, long from = -1L, long to = long.MaxValue);

		public string ToConditionString(DatabaseBase connection, string prefix)
		{
			List<string> conds = (from w in Conditions
				select w.ToConditionString(connection, prefix) into w
				where !string.IsNullOrEmpty(w)
				select w).ToList();
			if (conds.Count != 0)
			{
				return string.Format("({0})", string.Join(" " + KeyWord + " ", conds));
			}
			return "";
		}

		public void AddCondition(IRowNoFilter filter)
		{
			Conditions.Add(filter);
		}
	}
}
