using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace SystemDb.Internal
{
	internal class FullHistoryParameterValueCollection : ConcurrentDictionary<int, IHistoryParameterValue>, IFullHistoryParameterValueCollection, IEnumerable<IHistoryParameterValue>, IEnumerable
	{
		IHistoryParameterValue IFullHistoryParameterValueCollection.this[int id]
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

		public new IEnumerator<IHistoryParameterValue> GetEnumerator()
		{
			return base.Values.GetEnumerator();
		}

		public void Add(IHistoryParameterValue historyValue)
		{
			base[historyValue.Id] = historyValue;
		}

		public IHistoryParameterValue HistoryValue(int userId, int parameterId, string value, int selectionType)
		{
			using (IEnumerator<IHistoryParameterValue> enumerator = GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					IHistoryParameterValue historyParameterValue = enumerator.Current;
					if (userId == historyParameterValue.UserId && parameterId == historyParameterValue.ParameterId && value == historyParameterValue.Value && historyParameterValue.SelectionType == selectionType)
					{
						return historyParameterValue;
					}
				}
			}
			return null;
		}
	}
}
