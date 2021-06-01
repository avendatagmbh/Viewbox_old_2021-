using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using SystemDb;
using DbAccess;

namespace ViewboxDb.Filters
{
	public class BetweenFilter : IFilter
	{
		public IColumn Column { get; private set; }

		public object Value1 { get; private set; }

		public object Value2 { get; private set; }

		public BetweenFilter(IColumn column, object value1, object value2)
		{
			if (value1 == null)
			{
				throw new ArgumentNullException("value1");
			}
			if (value2 == null)
			{
				throw new ArgumentNullException("value2");
			}
			Column = column;
			if (column != null && column.DataType == SqlType.Decimal)
			{
				string filter = value1.ToString();
				if (filter.Contains(","))
				{
					if (filter.Contains(".") && filter.Length > 1)
					{
						filter = filter.Replace(".", "");
					}
					filter = filter.Replace(",", ".");
					value1 = filter;
				}
				filter = value2.ToString();
				if (filter.Contains(","))
				{
					if (filter.Contains(".") && filter.Length > 1)
					{
						filter = filter.Replace(".", "");
					}
					filter = filter.Replace(",", ".");
					value2 = filter;
				}
			}
			Value1 = value1;
			Value2 = value2;
		}

		public virtual string ToString(DatabaseBase db)
		{
			if (Column.DataType == SqlType.Date)
			{
				string format2 = ((Thread.CurrentThread.CurrentCulture.Name == "de-DE") ? "'%d.%m.%Y'" : "'%d/%m/%Y'");
				return string.Format("{0} BETWEEN STR_TO_DATE({1}, {3}) AND STR_TO_DATE({2}, {3})", db.Enquote(Column.Name), db.GetValueString(Value1), db.GetValueString(Value2), format2);
			}
			if (Column.DataType == SqlType.DateTime)
			{
				string format = ((Thread.CurrentThread.CurrentCulture.Name == "de-DE") ? "'%d.%m.%Y %H:%i:%S'" : "'%d/%m/%Y %H:%i:%S'");
				return string.Format("{0} BETWEEN STR_TO_DATE({1}, {3}) AND STR_TO_DATE({2}, {3})", db.Enquote(Column.Name), db.GetValueString(Value1), db.GetValueString(Value2), format);
			}
			return $"{db.Enquote(Column.Name)} BETWEEN {db.GetValueString(Value1)} AND {db.GetValueString(Value2)}";
		}

		public virtual string ToString(DatabaseBase db, string prefix)
		{
			return $"{prefix}.{db.Enquote(Column.Name)} BETWEEN {db.GetValueString(Value1)} AND {db.GetValueString(Value2)}";
		}

		public virtual string ToCommandString(DatabaseBase db, Hashtable commandParameters)
		{
			commandParameters.Add("@filter_param_" + commandParameters.Count, Value1);
			commandParameters.Add("@filter_param_" + commandParameters.Count, Value2);
			return string.Format("{0} BETWEEN {1} AND {2}", db.Enquote(Column.Name), "@filter_param_" + (commandParameters.Count - 2), "@filter_param_" + (commandParameters.Count - 1));
		}

		public void ReplaceColumn(ITableObject tobj)
		{
			Column = tobj.Columns[Column.Name];
		}

		public List<Tuple<IFilter, string, object, IColumn>> GetParameters(List<Tuple<IFilter, string, object, IColumn>> parameters)
		{
			string param1Name = "filter_param_" + parameters.Count;
			Tuple<IFilter, string, object, IColumn> tuple1 = new Tuple<IFilter, string, object, IColumn>(this, param1Name, Value1, Column);
			if (!parameters.Contains(tuple1))
			{
				parameters.Add(tuple1);
			}
			string param2Name = "filter_param_" + parameters.Count;
			Tuple<IFilter, string, object, IColumn> tuple2 = new Tuple<IFilter, string, object, IColumn>(this, param2Name, Value2, Column);
			if (!parameters.Contains(tuple2))
			{
				parameters.Add(tuple2);
			}
			return parameters;
		}

		public virtual string GetReplacedFilter(DatabaseBase db, ref int i, List<bool> freeselection, ref int j)
		{
			if (freeselection[j++])
			{
				i += 2;
				return string.Empty;
			}
			string param1Name = "filter_param_" + i;
			i++;
			string param2Name = "filter_param_" + i;
			i++;
			return string.Format("(ifnull({1},'') = '' OR ifnull({2},'') = '' OR {0} BETWEEN {1} AND {2})", db.Enquote(Column.Name), param1Name, param2Name);
		}

		public List<Tuple<IFilter, IColumn, string, object, string>> GetParameters(List<Tuple<IFilter, IColumn, string, object, string>> parameters)
		{
			string param1Name = "filter_param_" + parameters.Count;
			Tuple<IFilter, IColumn, string, object, string> tuple1 = new Tuple<IFilter, IColumn, string, object, string>(this, Column, param1Name, Value1, "von");
			if (!parameters.Contains(tuple1))
			{
				parameters.Add(tuple1);
			}
			string param2Name = "filter_param_" + parameters.Count;
			Tuple<IFilter, IColumn, string, object, string> tuple2 = new Tuple<IFilter, IColumn, string, object, string>(this, Column, param2Name, Value2, "");
			if (!parameters.Contains(tuple2))
			{
				parameters.Add(tuple2);
			}
			return parameters;
		}

		public virtual string ToOriginalString()
		{
			return string.Format("Between({1},{0},{2})", "%" + ((Column != null) ? Column.Id : 0), string.Concat("\"", Value1, "\""), string.Concat("\"", Value2, "\""));
		}

		public bool CheckFilter()
		{
			if (Filter.CheckDataType(Column.DataType, Value1))
			{
				return Filter.CheckDataType(Column.DataType, Value2);
			}
			return false;
		}

		public bool HasIncorrectColumn()
		{
			return Column == null;
		}

		public bool HasValue()
		{
			return true;
		}

		public IFilter Clone()
		{
			return (IFilter)MemberwiseClone();
		}

		public void ReplaceRowNoFilters(IFilter parent, DatabaseBase connection, IndexDb indexDb, IColumn rowNoColumn)
		{
		}

		public object GetColumnValue(string column)
		{
			if (!(Column.Name == column))
			{
				return null;
			}
			return Value1;
		}

		public override string ToString()
		{
			return $"{Column.Name} BETWEEN {Value1} AND {Value2}";
		}

		public override int GetHashCode()
		{
			return $"{((Column != null) ? Column.Name : null)}${Value1}${Value2}".GetHashCode();
		}

		public virtual string GetToolTipText()
		{
			return $" ({Column} BETWEEN {Value1} AND {Value2})";
		}
	}
}
