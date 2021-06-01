using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Viewbox.Models
{
	internal class EnumerableDataRowGroupList<T> : IEnumerable<T>, IEnumerable
	{
		private readonly IEnumerable _dataRows;

		private readonly StringBuilder _stringBuilder = new StringBuilder();

		public List<string> GroupColumnNameList { get; set; }

		public string GroupData(DataRow dataRow)
		{
			_stringBuilder.Remove(0, _stringBuilder.Length);
			foreach (string column in GroupColumnNameList)
			{
				_stringBuilder.Append(dataRow[column]);
			}
			return _stringBuilder.ToString();
		}

		internal EnumerableDataRowGroupList(IEnumerable items)
		{
			_dataRows = items;
		}

		IEnumerator<T> IEnumerable<T>.GetEnumerator()
		{
			return _dataRows.Cast<T>().GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<T>)this).GetEnumerator();
		}
	}
}
