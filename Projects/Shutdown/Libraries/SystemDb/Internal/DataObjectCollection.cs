using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SystemDb.Internal
{
	internal class DataObjectCollection<T, U> : IDataObjectCollection<U>, IEnumerable<U>, IEnumerable where T : DataObject, U where U : IDataObject
	{
		private readonly Dictionary<int, T> _byId = new Dictionary<int, T>();

		private readonly Dictionary<string, T> _byName = new Dictionary<string, T>(StringComparer.OrdinalIgnoreCase);

		private readonly SortedDictionary<int, T> _byOrdinal = new SortedDictionary<int, T>();

		public int Count => Math.Max(Math.Max(_byId.Count, _byName.Count), _byOrdinal.Count);

		public U this[int id] => (U)(_byId.ContainsKey(id) ? _byId[id] : null);

		public U this[string name] => (U)_byName[name];

		public event OrdinalChangedHandler OrdinalChanged;

		public event ObjectAddedHandler ObjectAdded;

		public event ObjectRemovedHandler ObjectRemoved;

		private void NotifyObjectRemoved(IDataObject sender)
		{
			if (this.ObjectRemoved != null)
			{
				this.ObjectRemoved(sender);
			}
		}

		public void RemoveHandler()
		{
			foreach (KeyValuePair<int, T> obj in _byId)
			{
				obj.Value.NameChanged -= obj_NameChanged;
				obj.Value.OrdinalChanged -= obj_OrdinalChanged;
			}
		}

		public IEnumerator<U> GetEnumerator()
		{
			return _byOrdinal.Values.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _byOrdinal.Values.GetEnumerator();
		}

		public void Add(U obj)
		{
			T o = obj as T;
			_byId[o.Id] = o;
			if (_byOrdinal.ContainsKey(o.Ordinal))
			{
				o.Ordinal = _byOrdinal.Keys.Max((int key) => key) + 1;
			}
			_byOrdinal[o.Ordinal] = o;
			_byName[o.Name] = o;
			obj.NameChanged += obj_NameChanged;
			obj.OrdinalChanged += obj_OrdinalChanged;
		}

		private void obj_NameChanged(IDataObject sender, string old_name)
		{
			_byName.Remove(old_name);
			_byName.Add(sender.Name, sender as T);
		}

		private void obj_OrdinalChanged(IDataObject sender, int old_ordinal)
		{
			_byOrdinal.Remove(old_ordinal);
			if (sender.Ordinal < old_ordinal)
			{
				foreach (int ordinal2 in _byOrdinal.Where(delegate(KeyValuePair<int, T> o)
				{
					KeyValuePair<int, T> keyValuePair6 = o;
					if (keyValuePair6.Key < old_ordinal)
					{
						keyValuePair6 = o;
						return keyValuePair6.Key >= sender.Ordinal;
					}
					return false;
				}).OrderByDescending(delegate(KeyValuePair<int, T> o)
				{
					KeyValuePair<int, T> keyValuePair5 = o;
					return keyValuePair5.Key;
				}).Select(delegate(KeyValuePair<int, T> o)
				{
					KeyValuePair<int, T> keyValuePair4 = o;
					return keyValuePair4.Key;
				}))
				{
					DataObject obj2 = _byOrdinal[ordinal2];
					_byOrdinal.Remove(ordinal2);
					obj2.SetOrdinalWithoutNotify(ordinal2 + 1);
					_byOrdinal.Add(ordinal2 + 1, obj2 as T);
				}
			}
			else
			{
				foreach (int ordinal in _byOrdinal.Where(delegate(KeyValuePair<int, T> o)
				{
					KeyValuePair<int, T> keyValuePair3 = o;
					if (keyValuePair3.Key > old_ordinal)
					{
						keyValuePair3 = o;
						return keyValuePair3.Key <= sender.Ordinal;
					}
					return false;
				}).OrderBy(delegate(KeyValuePair<int, T> o)
				{
					KeyValuePair<int, T> keyValuePair2 = o;
					return keyValuePair2.Key;
				}).Select(delegate(KeyValuePair<int, T> o)
				{
					KeyValuePair<int, T> keyValuePair = o;
					return keyValuePair.Key;
				}))
				{
					DataObject obj = _byOrdinal[ordinal];
					_byOrdinal.Remove(ordinal);
					obj.SetOrdinalWithoutNotify(ordinal - 1);
					_byOrdinal.Add(ordinal - 1, obj as T);
				}
			}
			if (!_byOrdinal.Keys.Contains(sender.Ordinal))
			{
				_byOrdinal.Add(sender.Ordinal, sender as T);
			}
		}

		public U At(int index)
		{
			try
			{
				return (U)_byOrdinal[index];
			}
			catch
			{
				return (U)_byOrdinal[_byOrdinal.ElementAt(index).Key];
			}
		}

		public void Remove(T obj)
		{
			_byId.Remove(obj.Id);
			_byOrdinal.Remove(obj.Ordinal);
			_byName.Remove(obj.Name);
			NotifyObjectRemoved(obj);
		}

		public void RemoveById(int id)
		{
			Remove(_byId[id]);
		}

		public void RemoveByName(string name)
		{
			Remove(_byName[name]);
		}

		public void RemoveAt(int index)
		{
			Remove(_byOrdinal[index]);
		}

		public void Clear()
		{
			T[] list = new T[Count];
			_byOrdinal.Values.CopyTo(list, 0);
			_byId.Clear();
			_byOrdinal.Clear();
			_byName.Clear();
			for (int i = Count; i > 0; i--)
			{
				NotifyObjectRemoved(list[i - 1]);
			}
		}

		public bool Contains(int id)
		{
			return _byId.ContainsKey(id);
		}

		public bool Contains(string name)
		{
			return _byName.ContainsKey(name);
		}

		public bool ContainsOrdinal(int ordinal)
		{
			return _byOrdinal.ContainsKey(ordinal);
		}
	}
}
