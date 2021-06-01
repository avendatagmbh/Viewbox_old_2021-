using System.Collections;
using System.Collections.Generic;

namespace SystemDb.Internal
{
	public class ObjectTypesCollection : Dictionary<int, string>, IObjectTypesCollection, IDictionary<int, string>, ICollection<KeyValuePair<int, string>>, IEnumerable<KeyValuePair<int, string>>, IEnumerable
	{
	}
}
