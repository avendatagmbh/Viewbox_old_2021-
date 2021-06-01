using System;
using System.Collections.Generic;
using System.Linq;
using SystemDb;

namespace ViewboxDb.RowNoFilters
{
	public static class Parser
	{
		public enum ExtParameterTypes
		{
			None,
			LPAD,
			RPAD,
			START,
			END,
			BOTH,
			IFNULL
		}

		public enum Operators
		{
			Equal,
			Greater,
			GreaterOrEqual,
			Less,
			LessOrEqual,
			NotEqual,
			Like,
			None
		}

		public enum OperatorsForAssistant
		{
			None,
			Equal,
			Greater,
			GreaterOrEqual,
			Less,
			LessOrEqual,
			Between,
			Like
		}

		public class OperatorsExt
		{
			public OperatorsForAssistant OperatorsForAssistant { get; set; }

			public string Name { get; set; }

			public static List<OperatorsExt> GetOperatorsTexts()
			{
				return ((OperatorsForAssistant[])Enum.GetValues(typeof(OperatorsForAssistant))).Select((OperatorsForAssistant x) => new OperatorsExt
				{
					OperatorsForAssistant = x,
					Name = x.ToString()
				}).ToList();
			}
		}

		public static Dictionary<Operators, string> OperatorSql = new Dictionary<Operators, string>
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
			}
		};

		public static IRowNoFilter Parse(ITableObject table, string toParse)
		{
			IRowNoFilter filter = null;
			try
			{
				if (toParse.Length >= 3 && toParse.Substring(0, 3).ToLower().Equals("not") && toParse[3].Equals('('))
				{
					if (CheckOuterBrackets(toParse.Substring(3), '(', ')'))
					{
						string toParse6 = toParse.Substring(4);
						toParse6 = toParse6.Remove(toParse6.Length - 1);
						filter = GetFilterWithConditions(new NotFilter(), table, toParse6);
					}
					return filter;
				}
				if (toParse.Length >= 3 && toParse.Substring(0, 3).ToLower().Equals("and"))
				{
					if (CheckOuterBrackets(toParse.Substring(3), '(', ')'))
					{
						string toParse5 = toParse.Substring(4);
						toParse5 = toParse5.Remove(toParse5.Length - 1);
						filter = GetFilterWithConditions(new AndFilter(), table, toParse5);
					}
					return filter;
				}
				if (toParse.Length >= 2 && toParse.Substring(0, 2).ToLower().Equals("or"))
				{
					if (CheckOuterBrackets(toParse.Substring(2), '(', ')'))
					{
						string toParse4 = toParse.Substring(3);
						toParse4 = toParse4.Remove(toParse4.Length - 1);
						filter = GetFilterWithConditions(new OrFilter(), table, toParse4);
					}
					return filter;
				}
				if (toParse.Length >= 7 && toParse.Substring(0, 7).ToLower().Equals("emptyor"))
				{
					if (CheckOuterBrackets(toParse.Substring(7), '(', ')'))
					{
						string toParse3 = toParse.Substring(8);
						toParse3 = toParse3.Remove(toParse3.Length - 1);
						filter = GetEmptyOrFilter(new EmptyOrFilter(), table, toParse3);
					}
					return filter;
				}
				if (toParse.Length >= 10 && toParse.Substring(0, 10).ToLower().Equals("notemptyor"))
				{
					if (CheckOuterBrackets(toParse.Substring(10), '(', ')'))
					{
						string toParse2 = toParse.Substring(11);
						toParse2 = toParse2.Remove(toParse2.Length - 1);
						filter = GetNotEmptyOrFilter(new NotEmptyOrFilter(), table, toParse2);
					}
					return filter;
				}
				return GetColValueFilter(table, toParse);
			}
			catch
			{
				return null;
			}
		}

		public static IRowNoFilter GetColValueFilter(ITableObject table, string toParse)
		{
			ColValueFilter filter = null;
			List<string> words = new List<string>();
			string help = "";
			bool isInQuote = false;
			for (int i = 0; i < toParse.Length; i++)
			{
				char c = toParse[i];
				if (c.Equals('"'))
				{
					isInQuote = ((!isInQuote) ? true : false);
				}
				if (!isInQuote)
				{
					if (c.Equals('(') || c.Equals(',') || c.Equals(')'))
					{
						words.Add(help);
						help = "";
					}
					else
					{
						help += c;
					}
				}
				else
				{
					help += c;
				}
			}
			if (words.Count > 2)
			{
				string op = words.ElementAt(0);
				string colId = words.ElementAt(1);
				string value = words.ElementAt(2);
				string extParameterType = "";
				string extParameter1 = null;
				string extParameter2 = null;
				ExtParameterTypes extParamType = ExtParameterTypes.None;
				if (words.Count > 3)
				{
					extParameterType = words.ElementAt(3).ToUpper();
				}
				if (Enum.GetNames(typeof(ExtParameterTypes)).Contains(extParameterType))
				{
					extParamType = (ExtParameterTypes)Enum.Parse(typeof(ExtParameterTypes), extParameterType);
					if (words.Count > 4)
					{
						extParameter1 = words.ElementAt(4);
					}
					if (words.Count > 5)
					{
						extParameter2 = words.ElementAt(5);
					}
				}
				if (!colId.StartsWith("%"))
				{
					return null;
				}
				colId = colId.Remove(0, 1);
				IColumn col = null;
				if (!string.IsNullOrEmpty(colId))
				{
					try
					{
						int cid = int.Parse(colId);
						col = table.Columns[cid];
					}
					catch
					{
						return null;
					}
				}
				if (Enum.GetNames(typeof(Operators)).Contains(op))
				{
					try
					{
						object val = DoConvert(col, value);
						Operators oper = (Operators)Enum.Parse(typeof(Operators), op);
						if (oper == Operators.Like)
						{
							val = val.ToString().Replace('*', '%');
						}
						return new ColValueFilter(col, oper, val, extParamType, extParameter1, extParameter2);
					}
					catch
					{
						return null;
					}
				}
			}
			return filter;
		}

		public static object DoConvert(IColumn column, string value)
		{
			if (!value.StartsWith("\"") || !value.EndsWith("\""))
			{
				return null;
			}
			value = value.Substring(1);
			value = value.Substring(0, value.Length - 1);
			if (value.ToLower().Equals("dbnull"))
			{
				return value.Replace("\"", "");
			}
			if (string.IsNullOrEmpty(value))
			{
				return string.Empty;
			}
			return TryParse(column, value);
		}

		public static object TryParse(IColumn column, string value)
		{
			if (column.DataType == SqlType.Integer && long.TryParse(value, out var toLong))
			{
				return toLong;
			}
			if (column.DataType == SqlType.Decimal && double.TryParse(value, out var toDouble))
			{
				return toDouble;
			}
			if ((column.DataType == SqlType.Date || column.DataType == SqlType.Time || column.DataType == SqlType.DateTime) && DateTime.TryParse(value, out var toDateTime))
			{
				return toDateTime.ToString("yyyyMMddHHmmss");
			}
			return value;
		}

		public static IRowNoFilter GetFilterWithConditions(IRowNoFilter filter, ITableObject table, string toParse)
		{
			string help = "";
			int bracketsOpen = 0;
			int strOpen = 0;
			for (int i = 0; i < toParse.Length; i++)
			{
				char c = toParse[i];
				if (c.Equals('('))
				{
					bracketsOpen++;
				}
				if (c.Equals(')'))
				{
					bracketsOpen--;
				}
				if ('"'.Equals(c))
				{
					strOpen++;
				}
				if (bracketsOpen == 0 && strOpen % 2 == 0 && c.Equals(','))
				{
					IRowNoFilter filter2 = Parse(table, help);
					if (filter.GetType() == typeof(AndFilter))
					{
						(filter as AndFilter).AddCondition(filter2);
					}
					if (filter.GetType() == typeof(OrFilter))
					{
						(filter as OrFilter).AddCondition(filter2);
					}
					if (filter.GetType() == typeof(NotFilter))
					{
						(filter as NotFilter).Condition = filter2;
					}
					help = "";
				}
				else
				{
					help += c;
				}
			}
			if (!string.IsNullOrWhiteSpace(help))
			{
				IRowNoFilter filter3 = Parse(table, help);
				if (filter.GetType() == typeof(AndFilter))
				{
					(filter as AndFilter).AddCondition(filter3);
				}
				if (filter.GetType() == typeof(OrFilter))
				{
					(filter as OrFilter).AddCondition(filter3);
				}
				if (filter.GetType() == typeof(NotFilter))
				{
					(filter as NotFilter).Condition = filter3;
				}
			}
			return filter;
		}

		public static IRowNoFilter GetEmptyOrFilter(EmptyOrFilter filter, ITableObject table, string toParse)
		{
			string help = "";
			int bracketsOpen = 0;
			int strOpen = 0;
			bool hasValue = false;
			for (int i = 0; i < toParse.Length; i++)
			{
				char c = toParse[i];
				if (c.Equals('('))
				{
					bracketsOpen++;
				}
				if (c.Equals(')'))
				{
					bracketsOpen--;
				}
				if ('"'.Equals(c))
				{
					strOpen++;
				}
				if (!hasValue && bracketsOpen == 0 && strOpen % 2 == 0 && c.Equals(','))
				{
					if (!help.StartsWith("\"") || !help.EndsWith("\""))
					{
						return null;
					}
					help = help.Substring(1);
					help = (string)(filter.Value = help.Substring(0, help.Length - 1));
					help = "";
					hasValue = true;
				}
				else
				{
					help += c;
				}
			}
			if (filter.Value != null && "null".Equals(filter.Value.ToString().ToLower()))
			{
				filter.Value = null;
			}
			filter.Condition = Parse(table, help);
			return filter;
		}

		public static IRowNoFilter GetNotEmptyOrFilter(NotEmptyOrFilter filter, ITableObject table, string toParse)
		{
			string help = "";
			int bracketsOpen = 0;
			bool hasValue = false;
			int strOpen = 0;
			for (int i = 0; i < toParse.Length; i++)
			{
				char c = toParse[i];
				if (c.Equals('('))
				{
					bracketsOpen++;
				}
				if (c.Equals(')'))
				{
					bracketsOpen--;
				}
				if ('"'.Equals(c))
				{
					strOpen++;
				}
				if (!hasValue && bracketsOpen == 0 && strOpen % 2 == 0 && c.Equals(','))
				{
					if (!help.StartsWith("\"") || !help.EndsWith("\""))
					{
						return null;
					}
					help = help.Substring(1);
					help = (string)(filter.Value = help.Substring(0, help.Length - 1));
					help = "";
					hasValue = true;
				}
				else
				{
					help += c;
				}
			}
			if (filter.Value != null && "null".Equals(filter.Value.ToString().ToLower()))
			{
				filter.Value = null;
			}
			filter.Condition = Parse(table, help);
			return filter;
		}

		public static bool CheckOuterBrackets(string toCheck, char opening, char closing)
		{
			if (toCheck[0].Equals(opening))
			{
				return toCheck[toCheck.Length - 1].Equals(closing);
			}
			return false;
		}
	}
}
