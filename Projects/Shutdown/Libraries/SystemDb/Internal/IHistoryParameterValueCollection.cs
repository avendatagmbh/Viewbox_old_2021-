using System.Collections;
using System.Collections.Generic;

namespace SystemDb.Internal
{
	public interface IHistoryParameterValueCollection : IDataObjectCollection<IHistoryParameterValue>, IEnumerable<IHistoryParameterValue>, IEnumerable
	{
	}
}
