using System.Collections;
using System.Collections.Generic;

namespace SystemDb
{
	public interface IExtendedColumnInformationCollection : IDictionary<string, IList<IColumnConnection>>, ICollection<KeyValuePair<string, IList<IColumnConnection>>>, IEnumerable<KeyValuePair<string, IList<IColumnConnection>>>, IEnumerable
	{
	}
}
