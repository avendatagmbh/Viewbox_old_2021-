using System;
using System.Globalization;
using System.Linq;
using SystemDb;
using DbAccess;

namespace Viewbox.Models
{
	public class TableCrudColumnModel
	{
		private bool _hasIndex;

		private bool _indexDataGenerated;

		private ITableCrudColumn _tableCrudColumn;

		private object _value;

		public ITableCrudColumn TableCrudColumn
		{
			get
			{
				return _tableCrudColumn;
			}
			set
			{
				_tableCrudColumn = value;
			}
		}

		public string Name { get; set; }

		public string DisplayName { get; set; }

		public int CalculateType { get; set; }

		public int FromColumnId { get; set; }

		public object Value
		{
			get
			{
				return _value;
			}
			set
			{
				_value = value;
			}
		}

		public SqlType Type { get; set; }

		public bool Calculated => CalculateType == 1;

		public bool HasIndex
		{
			get
			{
				return _hasIndex;
			}
			set
			{
				_hasIndex = value;
			}
		}

		public bool IndexDataGenerated
		{
			get
			{
				return _indexDataGenerated;
			}
			set
			{
				_indexDataGenerated = value;
			}
		}

		public string FromColumnName { get; set; }

		public string FromColumnTableDatabase { get; set; }

		public string FromColumnTableTableName { get; set; }

		public string ColumnTableDatabase { get; set; }

		public string ColumnTableTableName { get; set; }

		public string ValueString
		{
			get
			{
				return (_value == null) ? null : _value.ToString();
			}
			set
			{
				_value = value;
			}
		}

		public bool ValueBool
		{
			get
			{
				if (_value != null && bool.TryParse(_value.ToString(), out var boolOutput))
				{
					return boolOutput;
				}
				if (ValueInt32.HasValue && ValueInt32.Value == 1)
				{
					return true;
				}
				return false;
			}
			set
			{
				_value = (value ? 1 : 0);
			}
		}

		public long? ValueInt32
		{
			get
			{
				if (_value != null && long.TryParse(_value.ToString(), out var longOutput))
				{
					return longOutput;
				}
				return null;
			}
			set
			{
				_value = value;
			}
		}

		public double? ValueDecimal
		{
			get
			{
				if (_value != null && double.TryParse(_value.ToString(), out var doubleOutput))
				{
					return doubleOutput;
				}
				return null;
			}
			set
			{
				_value = value;
			}
		}

		public string ValueDecimalStr
		{
			get
			{
				return (_value == null) ? "" : _value.ToString();
			}
			set
			{
				_value = null;
				if (value != null)
				{
					string val = value.Replace(",", "");
					val = val.Replace(".", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);
					if (double.TryParse(val, out var doubleOutput))
					{
						_value = doubleOutput;
					}
				}
			}
		}

		public decimal? ValueNumeric
		{
			get
			{
				if (_value != null && decimal.TryParse(_value.ToString(), out var decimalOutput))
				{
					return decimalOutput;
				}
				return null;
			}
			set
			{
				_value = value;
			}
		}

		public string ValueNumericStr
		{
			get
			{
				return (_value == null) ? "" : _value.ToString();
			}
			set
			{
				_value = null;
				if (value != null)
				{
					string val = value.Replace(",", "");
					val = val.Replace(".", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);
					if (double.TryParse(val, out var doubleOutput))
					{
						_value = doubleOutput;
					}
				}
			}
		}

		public DateTime? ValueDateTime
		{
			get
			{
				if (_value != null && DateTime.TryParse(_value.ToString(), out var dateOutput))
				{
					return dateOutput;
				}
				return null;
			}
			set
			{
				_value = value;
			}
		}

		public TableCrudColumnModel()
		{
		}

		public TableCrudColumnModel(ITableCrudColumn col, object value, DatabaseBase conn)
		{
			_tableCrudColumn = col;
			Name = col.Column.Name;
			CalculateType = col.CalculateType;
			_value = col.Column.ConstValue;
			Type = col.Column.DataType;
			_value = value;
			_hasIndex = false;
			_indexDataGenerated = false;
			if (col.FromColumn != null)
			{
				IOptimization opt = ViewboxSession.Optimizations.LastOrDefault();
				ViewboxApplication.Database.IndexDatabase.HasColumnIndexData(col.FromColumn.Id, col.FromColumn.Table.Id, conn, opt, out _indexDataGenerated, out _hasIndex, col.FromColumn.Table.OrderAreas.Count, ViewboxSession.IssueOptimizationFilter);
			}
			DisplayName = col.Column.Descriptions[ViewboxApplication.Database.SystemDb.DefaultLanguage] ?? col.Column.Name;
			if (col.FromColumn != null)
			{
				FromColumnName = col.FromColumn.Name;
				FromColumnTableDatabase = col.FromColumn.Table.Database;
				FromColumnTableTableName = col.FromColumn.Table.TableName;
			}
			if (col.Column != null)
			{
				ColumnTableDatabase = col.Column.Table.Database;
				ColumnTableTableName = col.Column.Table.TableName;
			}
			FromColumnId = ((_tableCrudColumn != null && _tableCrudColumn.FromColumn != null) ? _tableCrudColumn.FromColumn.Id : 0);
		}
	}
}
