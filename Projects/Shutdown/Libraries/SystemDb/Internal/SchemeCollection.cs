using System.Collections;
using System.Collections.Generic;

namespace SystemDb.Internal
{
	public class SchemeCollection : Dictionary<int, Scheme>, ISchemeCollection, IEnumerable<IScheme>, IEnumerable
	{
		public new IScheme this[int id]
		{
			get
			{
				if (!ContainsKey(id))
				{
					return null;
				}
				return base[id];
			}
		}

		public new IEnumerator<IScheme> GetEnumerator()
		{
			return base.Values.GetEnumerator();
		}

		public void Add(Scheme scheme)
		{
			Add(scheme.Id, scheme);
		}
	}
}
