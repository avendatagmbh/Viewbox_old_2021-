using System.Collections;
using System.Collections.Generic;

namespace SystemDb
{
	public interface IObjectTypesCollection : IDictionary<int, string>, ICollection<KeyValuePair<int, string>>, IEnumerable<KeyValuePair<int, string>>, IEnumerable
	{
	}
}
