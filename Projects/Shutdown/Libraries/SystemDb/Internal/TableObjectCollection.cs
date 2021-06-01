using System;
using System.Collections;
using System.Collections.Generic;

namespace SystemDb.Internal
{
	internal class TableObjectCollection : ITableObjectCollection, IEnumerable<ITableObject>, IEnumerable
	{
		private readonly Dictionary<int, TableObject> _byId = new Dictionary<int, TableObject>();

		private readonly Dictionary<string, TableObject> _byName = new Dictionary<string, TableObject>(StringComparer.OrdinalIgnoreCase);

		public int Count => _byId.Count;

		public ITableObject this[int id]
		{
			get
			{
				if (!Contains(id))
				{
					return null;
				}
				return _byId[id];
			}
		}

		public ITableObject this[string name]
		{
			get
			{
				if (!Contains(name))
				{
					return null;
				}
				return _byName[name];
			}
		}

		public event ObjectAddedHandler ObjectAdded;

		public event ObjectRemovedHandler ObjectRemoved;

		public bool Contains(int id)
		{
			return _byId.ContainsKey(id);
		}

		public bool Contains(string name)
		{
			return _byName.ContainsKey(name);
		}

		private void NotifyObjectRemoved(ITableObject sender)
		{
			if (this.ObjectRemoved != null)
			{
				this.ObjectRemoved(sender);
			}
		}

		public IEnumerator<ITableObject> GetEnumerator()
		{
			return _byId.Values.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public void Add(ITableObject tobj)
		{
			lock (this)
			{
				_byId.Add(tobj.Id, tobj as TableObject);
				_byName.Add(tobj.Name, tobj as TableObject);
			}
		}

		public void AddRange(IEnumerable<ITableObject> tobjs)
		{
			foreach (ITableObject tobj in tobjs)
			{
				Add(tobj);
			}
		}

		public void Remove(TableObject tobj)
		{
			_byId.Remove(tobj.Id);
			_byName.Remove(tobj.Name);
			NotifyObjectRemoved(tobj);
		}

		public void RemoveById(int id)
		{
			Remove(_byId[id]);
		}

		public void RemoveByName(string name)
		{
			Remove(_byName[name]);
		}

		public void Clear()
		{
			TableObject[] list = new TableObject[Count];
			_byId.Values.CopyTo(list, 0);
			_byId.Clear();
			_byName.Clear();
			for (int i = Count; i > 0; i--)
			{
				NotifyObjectRemoved(list[i - 1]);
			}
		}
	}
}
