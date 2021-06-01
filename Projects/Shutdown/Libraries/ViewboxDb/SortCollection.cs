using System.Collections.Generic;

namespace ViewboxDb
{
	public class SortCollection : List<Sort>
	{
		public override int GetHashCode()
		{
			int hash = 0;
			using Enumerator enumerator = GetEnumerator();
			while (enumerator.MoveNext())
			{
				Sort i = enumerator.Current;
				hash += i.GetHashCode();
			}
			return hash;
		}

		public SortCollection Clone()
		{
			SortCollection sortCollection = new SortCollection();
			sortCollection.AddRange(this);
			return sortCollection;
		}
	}
}
