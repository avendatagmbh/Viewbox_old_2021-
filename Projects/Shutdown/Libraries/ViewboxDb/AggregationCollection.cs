using System.Collections.Generic;

namespace ViewboxDb
{
	public class AggregationCollection : List<Aggregation>
	{
		public override int GetHashCode()
		{
			int hash = 0;
			using Enumerator enumerator = GetEnumerator();
			while (enumerator.MoveNext())
			{
				Aggregation i = enumerator.Current;
				hash += i.GetHashCode();
			}
			return hash;
		}
	}
}
