using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using SystemDb;
using ViewboxDb.RowNoFilters;

namespace ViewboxDb
{
	public static class OperatorHelper
	{
		private static readonly Dictionary<Operators, string> sql = new Dictionary<Operators, string>
		{
			{
				Operators.Equal,
				"{0} = {1}"
			},
			{
				Operators.Greater,
				"{0} > {1}"
			},
			{
				Operators.GreaterOrEqual,
				"{0} >= {1}"
			},
			{
				Operators.Less,
				"{0} < {1}"
			},
			{
				Operators.LessOrEqual,
				"{0} <= {1}"
			},
			{
				Operators.NotEqual,
				"{0} != {1}"
			},
			{
				Operators.Like,
				"CAST({0} AS CHAR) LIKE {1}"
			},
			{
				Operators.StartsWith,
				"SUBSTRING({0}, 1, LEAST(LENGTH({0}), LENGTH({1}))) = {1}"
			}
		};

		private static readonly Dictionary<Parser.Operators, string> sqlRowNo = new Dictionary<Parser.Operators, string>
		{
			{
				Parser.Operators.Equal,
				"{0} = {1}"
			},
			{
				Parser.Operators.Greater,
				"{0} > {1}"
			},
			{
				Parser.Operators.GreaterOrEqual,
				"{0} >= {1}"
			},
			{
				Parser.Operators.Less,
				"{0} < {1}"
			},
			{
				Parser.Operators.LessOrEqual,
				"{0} <= {1}"
			},
			{
				Parser.Operators.NotEqual,
				"{0} != {1}"
			},
			{
				Parser.Operators.Like,
				"CAST({0} AS CHAR) LIKE {1}"
			}
		};

		public static string ToSQL(this Operators op, object parameter1, object parameter2, SqlType type)
		{
			CultureInfo cultureNfo = Thread.CurrentThread.CurrentCulture;
			switch (type)
			{
			case SqlType.String:
				return string.Format(sql[op], parameter1, parameter2);
			case SqlType.Date:
			case SqlType.DateTime:
				if (!(parameter1 as string).Contains("STR_TO_DATE"))
				{
					string dateFormat = cultureNfo.DateTimeFormat.ShortDatePattern;
					if (type == SqlType.DateTime)
					{
						dateFormat = dateFormat + " " + cultureNfo.DateTimeFormat.LongTimePattern;
					}
					string mySqlDateFormat = dateFormat.Replace("dd", "%d").Replace("MM", "%m").Replace("yyyy", "%Y")
						.Replace("HH", "%H")
						.Replace("mm", "%i")
						.Replace("ss", "%s");
					return string.Format(sql[op], parameter1, $"STR_TO_DATE({parameter2}, '{mySqlDateFormat}')");
				}
				break;
			}
			return string.Format(sql[op], parameter1, parameter2);
		}

		public static string ToSQL(this Parser.Operators op, object parameter1, object parameter2, SqlType type)
		{
			return string.Format(sqlRowNo[op], parameter1, parameter2);
		}

		public static string GetOpString(this Operators op)
		{
			return op switch
			{
				Operators.Equal => "=", 
				Operators.Greater => ">", 
				Operators.GreaterOrEqual => "≥", 
				Operators.Less => "<", 
				Operators.LessOrEqual => "≤", 
				Operators.Like => "≈", 
				Operators.NotEqual => "!=", 
				Operators.StartsWith => "=|", 
				_ => throw new ArgumentException(), 
			};
		}
	}
}
