using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Text;
using SystemDb;
using DbAccess;

namespace ViewboxDb.RowNoFilters
{
	public class ColValueFilter : IRowNoFilter
	{
		public IColumn Column { get; private set; }

		public object Value { get; private set; }

		public Parser.Operators Op { get; private set; }

		public Parser.ExtParameterTypes ExtParameterType { get; private set; }

		public object ExtParameter1 { get; private set; }

		public object ExtParameter2 { get; private set; }

		public ColValueFilter(IColumn column, Parser.Operators op, object value = null, Parser.ExtParameterTypes extParameterType = Parser.ExtParameterTypes.None, object extParameter1 = null, object extParameter2 = null)
		{
			Column = column;
			Op = op;
			Value = value;
			ExtParameterType = extParameterType;
			ExtParameter1 = extParameter1;
			ExtParameter2 = extParameter2;
		}

		public RangeList GetRowNoRanges(DatabaseBase connection, IndexDb indexDb, IColumn rowNoColumn, List<IOrderArea> areas, long from = -1L, long to = long.MaxValue)
		{
			Stopwatch watch = new Stopwatch();
			watch.Start();
			RangeList ret = new RangeList();
			object value = ProcessExtParameter();
			try
			{
				StringBuilder builder = new StringBuilder();
				builder.Append("SELECT FROM_ROW, TO_ROW FROM ").Append(connection.Enquote(indexDb.DbName, IndexDb.ValueTablePrefix + Column.Id)).Append(" V ")
					.Append(" JOIN (");
				bool first = true;
				foreach (IOrderArea area in areas)
				{
					if (first)
					{
						first = false;
					}
					else
					{
						builder.Append(" UNION ");
					}
					builder.Append("SELECT ").Append(connection.Enquote("VALUE_ID")).Append(", FROM_ROW + ")
						.Append(area.Start)
						.Append(" AS FROM_ROW")
						.Append(", TO_ROW + ")
						.Append(area.Start)
						.Append(" AS TO_ROW")
						.Append(" FROM ")
						.Append(connection.Enquote(indexDb.DbName, IndexDb.RowNoTablePrefix + Column.Id + "_" + area.Id));
				}
				builder.Append(" ) T ON T.").Append(connection.Enquote("VALUE_ID")).Append(" = V.")
					.Append(connection.Enquote("ID"))
					.Append(" WHERE ")
					.AppendFormat(Parser.OperatorSql[Op], connection.Enquote("VALUE"), (value == null) ? "" : value.ToString())
					.Append((to == long.MaxValue) ? "" : (" AND T.FROM_ROW < " + (to + 1)))
					.Append((from == -1) ? "" : (" AND T.TO_ROW > " + (from - 1)))
					.Append(" ORDER BY FROM_ROW, TO_ROW ");
				string sql = builder.ToString();
				using IDataReader reader = connection.ExecuteReader(sql);
				while (reader.Read())
				{
					ret.Add(new Range
					{
						From = reader.GetInt64(0),
						To = reader.GetInt64(1)
					});
				}
			}
			catch (Exception ex)
			{
				throw new Exception("", ex);
			}
			watch.Stop();
			return ret;
		}

		public string ToConditionString(DatabaseBase db, string prefix)
		{
			string cname = db.Enquote(Column.Name);
			if (Value == null)
			{
				return string.Format("{0} IS NULL", prefix + "." + cname);
			}
			if (Value.ToString() == "")
			{
				return string.Format("({0} IS NULL OR {1})", prefix + "." + cname, prefix + "." + Op.ToSQL(cname, Value, Column.DataType));
			}
			if (Value.ToString() == "DBNULL")
			{
				return string.Format("{0} IS NULL", prefix + "." + cname);
			}
			if (Op == Parser.Operators.Equal && Column.DataType == SqlType.String && Column.MaxLength > Value.ToString().Length && int.TryParse(Value.ToString(), out var _))
			{
				string paddedValue = Value.ToString().PadLeft(Column.MaxLength, '0');
				return string.Format("({0} OR {1})", prefix + "." + Op.ToSQL(cname, Value, Column.DataType), prefix + "." + Op.ToSQL(cname, paddedValue, Column.DataType));
			}
			return prefix + "." + Op.ToSQL(cname, Value, Column.DataType);
		}

		private object ProcessExtParameter()
		{
			if (Value == null)
			{
				return Value;
			}
			string val = Value.ToString();
			Parser.ExtParameterTypes extParameterType = ExtParameterType;
			if ((uint)(extParameterType - 1) <= 4u)
			{
				if (val.StartsWith("'"))
				{
					val = val.Remove(0, 1);
				}
				if (val.EndsWith("'"))
				{
					val = val.Remove(val.Length - 1, 1);
				}
			}
			switch (ExtParameterType)
			{
			case Parser.ExtParameterTypes.BOTH:
				return "'%" + val + "%'";
			case Parser.ExtParameterTypes.END:
				return val + "%";
			case Parser.ExtParameterTypes.START:
				return "'%" + val + "'";
			case Parser.ExtParameterTypes.LPAD:
			{
				if (ExtParameter1 != null && ExtParameter2 != null && ExtParameter2.ToString().Length == 1 && int.TryParse(ExtParameter1.ToString(), out var totalWidth))
				{
					return "'" + val.PadLeft(totalWidth, ExtParameter2.ToString()[0]) + "'";
				}
				break;
			}
			case Parser.ExtParameterTypes.RPAD:
			{
				if (ExtParameter1 != null && ExtParameter2 != null && ExtParameter2.ToString().Length == 1 && int.TryParse(ExtParameter1.ToString(), out var totalWidth2))
				{
					return "'" + val.PadRight(totalWidth2, ExtParameter2.ToString()[0]) + "'";
				}
				break;
			}
			case Parser.ExtParameterTypes.IFNULL:
				if ((ExtParameter1 != null && Value == null) || string.IsNullOrEmpty(Value.ToString()) || "''".Equals(Value.ToString()) || Value.ToString().ToLower() == "null")
				{
					return ExtParameter1;
				}
				break;
			}
			return Value;
		}
	}
}
