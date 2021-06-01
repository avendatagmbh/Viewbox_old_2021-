using System.Collections;
using System.Collections.Generic;

namespace SystemDb
{
	public interface IDataObjectCollection<out T> : IEnumerable<T>, IEnumerable where T : IDataObject
	{
		T this[int id] { get; }

		T this[string name] { get; }

		int Count { get; }

		event OrdinalChangedHandler OrdinalChanged;

		event ObjectAddedHandler ObjectAdded;

		event ObjectRemovedHandler ObjectRemoved;

		T At(int index);

		bool Contains(int id);

		bool Contains(string name);

		void RemoveHandler();
	}
}
