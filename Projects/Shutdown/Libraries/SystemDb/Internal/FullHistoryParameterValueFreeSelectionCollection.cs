using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace SystemDb.Internal
{
	internal class FullHistoryParameterValueFreeSelectionCollection : ConcurrentDictionary<int, IHistoryParameterValueFreeSelection>, IFullHistoryParameterValueFreeSelectionCollection, IEnumerable<IHistoryParameterValueFreeSelection>, IEnumerable
	{
		IHistoryParameterValueFreeSelection IFullHistoryParameterValueFreeSelectionCollection.this[int id]
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

		public new IEnumerator<IHistoryParameterValueFreeSelection> GetEnumerator()
		{
			return base.Values.GetEnumerator();
		}

		public void Add(IHistoryParameterValueFreeSelection historyValue)
		{
			base[historyValue.Id] = historyValue;
		}

		public IHistoryParameterValueFreeSelection HistoryValue(int userId, int parameterId, string value, int selectionType)
		{
			using (IEnumerator<IHistoryParameterValueFreeSelection> enumerator = GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					IHistoryParameterValueFreeSelection historyParameterValue = enumerator.Current;
					if (userId == historyParameterValue.UserId && parameterId == historyParameterValue.ParameterId && value == historyParameterValue.Value && historyParameterValue.SelectionType == selectionType)
					{
						return historyParameterValue;
					}
				}
			}
			return null;
		}

		public void RemoveValues(int userId, int issueId)
		{
			List<int> idsToRemove = new List<int>();
			using (IEnumerator<IHistoryParameterValueFreeSelection> enumerator = GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					IHistoryParameterValueFreeSelection h = enumerator.Current;
					if (h.UserId == userId && h.IssueId == issueId)
					{
						idsToRemove.Add(h.Id);
					}
				}
			}
			foreach (int id in idsToRemove)
			{
				TryRemove(id, out var _);
			}
		}
	}
}
