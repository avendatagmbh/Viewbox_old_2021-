using System.Collections;
using System.Collections.Generic;

namespace SystemDb.Internal
{
	public class TableArchiveInformationCollection : Dictionary<int, TableArchiveInformation>, ITableArchiveInformationCollection, IEnumerable<ITableArchiveInformation>, IEnumerable
	{
		private readonly Dictionary<int, ITableArchiveInformation> _dic = new Dictionary<int, ITableArchiveInformation>();

		public new ITableArchiveInformation this[int tableId]
		{
			get
			{
				if (!_dic.ContainsKey(tableId))
				{
					return null;
				}
				return _dic[tableId];
			}
		}

		public new IEnumerator<ITableArchiveInformation> GetEnumerator()
		{
			return _dic.Values.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public void Add(ITableArchiveInformation settings)
		{
			if (_dic != null && _dic.ContainsKey(settings.TableId))
			{
				_dic[settings.TableId] = settings;
			}
			else
			{
				_dic.Add(settings.TableId, settings);
			}
		}

		public void Remove(ITableArchiveInformation settings)
		{
			_dic.Remove(settings.TableId);
		}
	}
}
