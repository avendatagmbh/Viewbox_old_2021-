using System.Collections.Generic;

namespace ViewboxDb
{
	public class JoinColumnsCollection : List<JoinColumns>
	{
		public override int GetHashCode()
		{
			int hash = 0;
			using Enumerator enumerator = GetEnumerator();
			while (enumerator.MoveNext())
			{
				JoinColumns i = enumerator.Current;
				hash += i.GetHashCode();
			}
			return hash;
		}
	}
}
