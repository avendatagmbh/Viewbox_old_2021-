using System.Collections;
using System.Collections.Generic;

namespace SystemDb.Internal
{
	public class ExtendedColumnInformationCollection : Dictionary<string, IList<IColumnConnection>>, IExtendedColumnInformationCollection, IDictionary<string, IList<IColumnConnection>>, ICollection<KeyValuePair<string, IList<IColumnConnection>>>, IEnumerable<KeyValuePair<string, IList<IColumnConnection>>>, IEnumerable
	{
	}
}
