using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Threading;
using SystemDb;
using DbAccess;

namespace ViewboxDb.Filters
{
	public class ColValueFilter : IFilter
	{
		public static bool ToDate;

		public IColumn Column { get; private set; }

		public object Value { get; private set; }

		public Operators Op { get; private set; }

		public ColValueFilter(IColumn column, Operators op, object value = null)
		{
			Column = column;
			Op = op;
			Value = value;
		}

		public string ToString(DatabaseBase db)
		{
			string cname = db.Enquote(Column.Name);
			if (Value == null)
			{
				return $"{cname} IS NULL";
			}
			if (Value.ToString() == "")
			{
				if (Op == Operators.NotEqual)
				{
					return $"({cname} IS NULL OR {Op.ToSQL(cname, db.GetValueString(Value), Column.DataType)})";
				}
				return $"({Op.ToSQL(cname, db.GetValueString(Value), Column.DataType)})";
			}
			if (Value.ToString() == "DBNULL")
			{
				return $"{cname} IS NULL";
			}
			if (Op == Operators.Like)
			{
				Column.ExactMatchUnchecked = true;
			}
			if (Op == Operators.Equal && Column.DataType == SqlType.String && Column.MaxLength > Value.ToString().Length && int.TryParse(Value.ToString(), out var _))
			{
				string paddedValue = Value.ToString().PadLeft(Column.MaxLength, '0');
				return $"({Op.ToSQL(cname, db.GetValueString(Value), Column.DataType)} OR {Op.ToSQL(cname, db.GetValueString(paddedValue), Column.DataType)})";
			}
			return Op.ToSQL(cname, db.GetValueString(Value), Column.DataType);
		}

		public string ToString(DatabaseBase db, string prefix)
		{
			string pref = (string.IsNullOrEmpty(prefix) ? "" : (prefix + "."));
			string cname = db.Enquote(Column.Name);
			if ((Op == Operators.Like || Op == Operators.Equal) && Column.DataType == SqlType.Date)
			{
				string mySqlDateFormat2 = Thread.CurrentThread.CurrentCulture.DateTimeFormat.ShortDatePattern.Replace("dd", "%d").Replace("MM", "%m").Replace("yyyy", "%Y");
				cname = "DATE_FORMAT(" + pref + cname + ", '" + mySqlDateFormat2 + "')";
			}
			if ((Op == Operators.Like || Op == Operators.Equal) && Column.DataType == SqlType.DateTime)
			{
				CultureInfo cultureNfo = Thread.CurrentThread.CurrentCulture;
				string mySqlDateFormat = $"{cultureNfo.DateTimeFormat.ShortDatePattern} {cultureNfo.DateTimeFormat.LongTimePattern}".Replace("dd", "%d").Replace("MM", "%m").Replace("yyyy", "%Y")
					.Replace("HH", "%H")
					.Replace("mm", "%i")
					.Replace("ss", "%s");
				cname = "DATE_FORMAT(" + pref + cname + ", '" + mySqlDateFormat + "')";
			}
			if (Value == null)
			{
				return $"({pref + cname} IS NULL)";
			}
			if (Value.ToString() == "")
			{
				return $"({pref + cname} IS NULL OR {pref + Op.ToSQL(cname, db.GetValueString(Value), Column.DataType)})";
			}
			if (Value.ToString() == "DBNULL")
			{
				return $"({pref + cname} IS NULL)";
			}
			if (Op == Operators.Equal && Column.DataType == SqlType.String && Column.MaxLength > Value.ToString().Length && int.TryParse(Value.ToString(), out var _))
			{
				string paddedValue = Value.ToString().PadLeft(Column.MaxLength, '0');
				return $"({pref + Op.ToSQL(cname, db.GetValueString(Value), Column.DataType)} OR {pref + Op.ToSQL(cname, db.GetValueString(paddedValue), Column.DataType)})";
			}
			if (Op != Operators.Like || Column.DataType != SqlType.Date)
			{
				return $"({pref}{Op.ToSQL(cname, db.GetValueString(Value), Column.DataType)})";
			}
			if (Op == Operators.Like)
			{
				Column.ExactMatchUnchecked = true;
			}
			return $"({Op.ToSQL(cname, db.GetValueString(Value), Column.DataType)})";
		}

		public void ReplaceColumn(ITableObject tobj)
		{
			if (tobj.Columns.Contains(Column.Name))
			{
				IColumn column = (Column = tobj.Columns[Column.Name]);
			}
		}

		public List<Tuple<IFilter, string, object, IColumn>> GetParameters(List<Tuple<IFilter, string, object, IColumn>> parameters)
		{
			string paramName = "filter_param_" + parameters.Count;
			Tuple<IFilter, string, object, IColumn> tuple = new Tuple<IFilter, string, object, IColumn>(this, paramName, Value, Column);
			if (!parameters.Contains(tuple))
			{
				parameters.Add(tuple);
			}
			return parameters;
		}

		public string GetReplacedFilter(DatabaseBase db, ref int i, List<bool> freeselection, ref int j)
		{
			if (freeselection[j++])
			{
				i++;
				return string.Empty;
			}
			string cname = db.Enquote(Column.Name);
			if (Value == null)
			{
				return $"(ifnull({cname},'') = '')";
			}
			string paramName = "filter_param_" + i;
			i++;
			return $"(ifnull({paramName},'') = '' OR {Op.ToSQL(cname, paramName, Column.DataType)})";
		}

		public string ToCommandString(DatabaseBase db, Hashtable commandParameters)
		{
			string cname = db.Enquote(Column.Name);
			CultureInfo cultureNfo = Thread.CurrentThread.CurrentCulture;
			if (Op == Operators.Like && Column.DataType == SqlType.Date)
			{
				string mySqlDateFormat = cultureNfo.DateTimeFormat.ShortDatePattern.Replace("dd", "%d").Replace("MM", "%m").Replace("yyyy", "%Y");
				cname = "DATE_FORMAT(" + cname + ", '" + mySqlDateFormat + "')";
			}
			if (cultureNfo.TwoLetterISOLanguageName.Contains("de") && (Op == Operators.Greater || Op == Operators.GreaterOrEqual || Op == Operators.Less || Op == Operators.LessOrEqual || Op == Operators.Equal) && (Column.DataType == SqlType.Numeric || Column.DataType == SqlType.Decimal))
			{
				string filter2 = Value.ToString();
				if (filter2.Contains(","))
				{
					if (filter2.Contains(".") && filter2.Length > 1)
					{
						filter2 = filter2.Replace(".", "");
					}
					filter2 = filter2.Replace(",", ".");
					if (Op == Operators.Equal)
					{
						filter2 = filter2.TrimStart('0');
						if (filter2.Contains("."))
						{
							filter2 = filter2.TrimEnd('0');
						}
						if (filter2.Length == 0 || filter2.StartsWith("."))
						{
							filter2 = "0" + filter2;
						}
						if (filter2.EndsWith("."))
						{
							filter2 = filter2.Substring(0, filter2.Length - 1);
						}
					}
					Value = filter2;
				}
			}
			if ((Op == Operators.Equal || Op == Operators.Like) && Column.DataType == SqlType.String)
			{
				string filter = Value.ToString();
				if (filter.Contains("'") && !filter.Contains("\\'"))
				{
					filter = filter.Replace("'", "\\'");
				}
				Value = filter;
			}
			if (Op == Operators.Like)
			{
				Column.ExactMatchUnchecked = true;
			}
			if (Value == null)
			{
				return $"{cname} IS NULL";
			}
			if (Value.ToString() == "")
			{
				commandParameters.Add("@filter_param_" + commandParameters.Count, Value);
				return string.Format("({0} IS NULL OR {1})", cname, Op.ToSQL(cname, "@filter_param_" + (commandParameters.Count - 1), Column.DataType));
			}
			if (Value.ToString() == "DBNULL")
			{
				return $"{cname} IS NULL";
			}
			if (Op == Operators.Equal && Column.DataType == SqlType.String && Column.MaxLength > Value.ToString().Length && int.TryParse(Value.ToString(), out var _))
			{
				string paddedValue = Value.ToString().PadLeft(Column.MaxLength, '0');
				commandParameters.Add("@filter_param_" + commandParameters.Count, Value);
				commandParameters.Add("@filter_param_" + commandParameters.Count, paddedValue);
				return string.Format("({0} OR {1})", Op.ToSQL(cname, "@filter_param_" + (commandParameters.Count - 1), Column.DataType), Op.ToSQL(cname, "@filter_param_" + (commandParameters.Count - 2), Column.DataType));
			}
			if (Value.ToString().StartsWith("0x"))
			{
				return Op.ToSQL(cname, Value, Column.DataType);
			}
			commandParameters.Add("@filter_param_" + commandParameters.Count, Value);
			return Op.ToSQL("(" + cname, "@filter_param_" + (commandParameters.Count - 1) + ")", Column.DataType);
		}

		public List<Tuple<IFilter, IColumn, string, object, string>> GetParameters(List<Tuple<IFilter, IColumn, string, object, string>> parameters)
		{
			string paramName = "filter_param_" + parameters.Count;
			Tuple<IFilter, IColumn, string, object, string> tuple = new Tuple<IFilter, IColumn, string, object, string>(this, Column, paramName, Value, Op.GetOpString());
			if (!parameters.Contains(tuple))
			{
				parameters.Add(tuple);
			}
			return parameters;
		}

		public string ToOriginalString()
		{
			string value = Value.ToString();
			return string.Format(Op.ToString() + "({0},{1})", "%" + Column.Id, "\"" + value + "\"");
		}

		public bool CheckFilter()
		{
			if (Column == null)
			{
				return true;
			}
			if (Value.ToString() == "DBNULL")
			{
				return true;
			}
			return Filter.CheckDataType((ToDate && Column.DataType == SqlType.DateTime) ? SqlType.Date : Column.DataType, Value);
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
			BinaryFilter binaryParent = parent as BinaryFilter;
			if (binaryParent == null)
			{
				return;
			}
			try
			{
				object id = connection.ExecuteScalar("SELECT ID FROM " + connection.Enquote(indexDb.DbName, IndexDb.ValueTablePrefix + Column.Id) + " WHERE VALUE = " + ((Value == null) ? "" : Value.ToString()));
				if (id == null)
				{
					Column = rowNoColumn;
					Value = "0";
					return;
				}
				int realId = 0;
				if (!int.TryParse(id.ToString(), out realId))
				{
					return;
				}
				using IDataReader reader = connection.ExecuteReader("SELECT FROM_ROW, TO_ROW FROM " + connection.Enquote(indexDb.DbName, IndexDb.RowNoTablePrefix + Column.Id) + " WHERE VALUE_ID = " + realId);
				OrFilter orFilter = new OrFilter();
				while (reader.Read())
				{
					object value1 = reader.GetValue(0);
					object value2 = reader.GetValue(1);
					if (value1 != null && value1.Equals(value2))
					{
						orFilter.AddCondition(new ColValueFilter(rowNoColumn, Operators.Equal, value1));
					}
					else
					{
						orFilter.AddCondition(new BetweenFilter(rowNoColumn, value1, value2));
					}
				}
				if (orFilter.Conditions.Count < 2000)
				{
					binaryParent.Conditions.Remove(this);
					binaryParent.Conditions.Add(orFilter);
				}
			}
			catch
			{
			}
		}

		public object GetColumnValue(string column)
		{
			if (!(Column.Name == column))
			{
				return null;
			}
			return Value;
		}

		public override int GetHashCode()
		{
			return $"{((Column == null) ? null : Column.Name)}{Op}{Value}".GetHashCode();
		}

		public override string ToString()
		{
			if (Value == null)
			{
				return $"{Column.Name} IS NULL";
			}
			if (Value.ToString() == "")
			{
				return $"({Column.Name} IS NULL OR {Op.ToSQL(Column.Name, Value, Column.DataType)})";
			}
			if (Value.ToString() == "DBNULL")
			{
				return $"{Column.Name} IS NULL";
			}
			return Op.ToSQL(Column.Name, Value, Column.DataType);
		}

		public string GetToolTipText()
		{
			string value = Value.ToString();
			if (Op == Operators.Like && value.Contains("%"))
			{
				return $"    ({Column} {Op.GetOpString()} {value.Substring(1, value.Length - 1)})";
			}
			return $" ({Column} {Op.GetOpString()} {value})";
		}
	}
}
