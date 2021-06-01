using System.Collections;
using System.Collections.Generic;

namespace SystemDb.Internal
{
	internal class HistoryParameterValueCollection : DataObjectCollection<HistoryParameterValue, IHistoryParameterValue>, IHistoryParameterValueCollection, IDataObjectCollection<IHistoryParameterValue>, IEnumerable<IHistoryParameterValue>, IEnumerable
	{
	}
}
