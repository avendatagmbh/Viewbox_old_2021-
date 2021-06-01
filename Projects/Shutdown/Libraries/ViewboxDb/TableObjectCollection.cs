using System.Collections;
using System.Collections.Generic;

namespace ViewboxDb
{
	public class TableObjectCollection : IEnumerable<TableObject>, IEnumerable
	{
		private Dictionary<int, TableObject> _dic = new Dictionary<int, TableObject>();

		public TableObject this[int id]
		{
			get
			{
				if (!_dic.ContainsKey(id))
				{
					return null;
				}
				return _dic[id];
			}
		}

		public int Count => _dic.Count;

		public void Add(TableObject to)
		{
			if (!_dic.ContainsKey(to.Table.Id))
			{
				_dic.Add(to.Table.Id, to);
				to.TableObjectRemove += delegate(TableObject sender)
				{
					_dic.Remove(sender.Table.Id);
				};
			}
		}

		public void Remove(TableObject to)
		{
			if (_dic.ContainsKey(to.Table.Id))
			{
				_dic.Remove(to.Table.Id);
				to.TableObjectRemove += delegate(TableObject sender)
				{
					_dic.Add(sender.Table.Id, to);
				};
			}
		}

		public IEnumerator<TableObject> GetEnumerator()
		{
			foreach (KeyValuePair<int, TableObject> item in _dic)
			{
				yield return item.Value;
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
