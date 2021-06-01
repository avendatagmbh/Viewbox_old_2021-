using System;
using System.Collections;
using System.Collections.Generic;

namespace SystemDb
{
	public interface IObjectTypeRelationsCollection : IDictionary<int, Tuple<int, int>>, ICollection<KeyValuePair<int, Tuple<int, int>>>, IEnumerable<KeyValuePair<int, Tuple<int, int>>>, IEnumerable
	{
		bool IsEmpty { get; }
	}
}
