using System.Collections;
using System.Collections.Generic;
using SystemDb.Internal;

namespace SystemDb
{
	public interface ITableObjectCollection : IEnumerable<ITableObject>, IEnumerable
	{
		int Count { get; }

		ITableObject this[int id] { get; }

		ITableObject this[string name] { get; }

		event ObjectAddedHandler ObjectAdded;

		event ObjectRemovedHandler ObjectRemoved;

		bool Contains(int id);

		bool Contains(string name);

		void Add(ITableObject tobj);

		void AddRange(IEnumerable<ITableObject> tobjs);

		void Remove(TableObject tobj);

		void RemoveById(int id);
	}
}
