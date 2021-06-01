using System;
using System.Collections.Generic;
using System.Linq;

namespace ViewboxDb.RowNoFilters
{
	public class RangeList : List<Range>
	{
		public long MinRange => this.Select((Range w) => w.From).Min();

		public long MaxRange => this.Select((Range w) => w.To).Max();

		public void InterSect(RangeList other)
		{
			List<Range> source = this.ToList();
			Clear();
			int i = 0;
			int j = 0;
			while (i < source.Count && j < other.Count)
			{
				long from1 = source[i].From;
				long to1 = source[i].To;
				long from2 = other[j].From;
				long to2 = other[j].To;
				if (from1 < from2 && to1 < from2)
				{
					i++;
					continue;
				}
				if (from2 < from1 && to2 < from1)
				{
					j++;
					continue;
				}
				Add(new Range
				{
					From = Math.Max(from1, from2),
					To = Math.Min(to1, to2)
				});
				if (to1 > to2)
				{
					j++;
					continue;
				}
				if (to1 < to2)
				{
					i++;
					continue;
				}
				i++;
				j++;
			}
		}

		public void Join(RangeList other)
		{
			List<Range> source = this.ToList();
			source.AddRange(other);
			Clear();
			AddRange(from w in source
				orderby w.To, w.From
				select w);
			for (int i = base.Count - 1; i > 0; i--)
			{
				if (base[i].From - base[i - 1].To < 2)
				{
					Range value;
					if (base[i].To > base[i - 1].To)
					{
						int index = i - 1;
						value = new Range
						{
							From = base[i - 1].From,
							To = base[i].To
						};
						base[index] = value;
					}
					if (base[i].From < base[i - 1].From)
					{
						int index2 = i - 1;
						value = new Range
						{
							From = base[i].From,
							To = base[i - 1].To
						};
						base[index2] = value;
					}
					RemoveAt(i);
				}
			}
		}

		public void Not(long from = -1L, long to = long.MaxValue)
		{
		}
	}
}
