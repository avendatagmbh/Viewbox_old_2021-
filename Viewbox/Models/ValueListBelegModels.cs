using System.Collections.Generic;
using System.Linq;
using SystemDb;
using ViewboxDb;

namespace Viewbox.Models
{
	public class ValueListBelegModels
	{
		private int _limit;

		private List<string> _result;

		private readonly global::ViewboxDb.ViewboxDb _db;

		private string _value;

		public int ColumnId { get; private set; }

		public string ColumnName { get; private set; }

		public List<string> Result
		{
			get
			{
				return _result;
			}
			set
			{
				_result = value;
			}
		}

		public ValueListBelegModels(int columnId = 0, int tableId = 0, string value = "", int limit = 5)
		{
			ITableObject obj = ((tableId < 0) ? ViewboxSession.TempTableObjects[tableId].Table : ViewboxSession.TableObjects[tableId]);
			ColumnId = columnId;
			ColumnName = ViewboxSession.Columns[columnId].Name;
			bool date = false;
			if (ViewboxSession.Columns[columnId] != null && ViewboxSession.Columns[columnId].DataType == SqlType.Decimal)
			{
				value = value.Replace(",", ".");
			}
			if (ViewboxSession.Columns[columnId] != null && (ViewboxSession.Columns[columnId].DataType == SqlType.Date || ViewboxSession.Columns[columnId].DataType == SqlType.Time || ViewboxSession.Columns[columnId].DataType == SqlType.DateTime))
			{
				date = true;
				int charCount = value.Split('.').Count();
				string[] values = value.PadRight(10 - charCount, '%').Split('.');
				for (int j = 0; j < values.Length - 1; j++)
				{
					for (int k = 1; k < values.Length; k++)
					{
						if (values[j].Length < values[k].Length)
						{
							string temp = values[j];
							values[j] = values[k];
							values[k] = temp;
						}
					}
				}
				value = string.Join("%", values);
				value = "%" + value.PadRight(8, '%');
			}
			_db = ViewboxApplication.Database;
			_value = value;
			_limit = limit;
			_result = _db.GetSearchResultBeleg(ColumnName, obj.Database, obj.TableName, value, limit, date);
			if (date)
			{
				for (int i = 0; i < _result.Count; i++)
				{
					_result[i] = _result[i].Replace(" 00:00:00", string.Empty);
				}
			}
		}
	}
}
