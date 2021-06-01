using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using SystemDb;

namespace ViewboxDb.Filters
{
	public static class Filter
	{
		private static Regex betweenRegex = new Regex("(ifnull\\(filter_param_[0-9]+\\,\\'\\'\\) = \\'\\' OR )?(ifnull\\(filter_param_[0-9]+\\,\\'\\'\\) = \\'\\' OR )?`[^`]+` (BETWEEN|NOT BETWEEN) filter_param_[0-9]+ AND filter_param_[0-9]+", RegexOptions.IgnoreCase | RegexOptions.Compiled);

		private static Regex colSetRegex = new Regex("(ifnull\\(filter_param_[0-9]+\\,\\'\\'\\) = \\'\\' OR )?`[^`]+` IN \\(filter_param_[0-9]+\\)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

		private static Regex colValueRegex = new Regex("(ifnull\\(filter_param_[0-9]+\\,\\'\\'\\) = \\'\\' OR )?`[^`]+` ([^ ]+) filter_param_[0-9]+", RegexOptions.IgnoreCase | RegexOptions.Compiled);

		private static Regex colValueRegex2 = new Regex("(ifnull\\(filter_param_[0-9]+\\,\\'\\'\\) = \\'\\' OR )?CAST\\(`[^`]+` AS CHAR\\) LIKE filter_param_[0-9]+", RegexOptions.IgnoreCase | RegexOptions.Compiled);

		private static Regex colValueRegex3 = new Regex("(ifnull\\(filter_param_[0-9]+\\,\\'\\'\\) = \\'\\' OR )?SUBSTRING\\(`[^`]+`\\, 1\\, LEAST\\(LENGTH\\(`[^`]+`\\)\\, LENGTH\\(filter_param_[0-9]+\\)\\)\\) = filter_param_[0-9]+", RegexOptions.IgnoreCase | RegexOptions.Compiled);

		private static Regex colValueRegex4 = new Regex("(ifnull\\(filter_param_[0-9]+\\,\\'\\'\\) = \\'\\' OR )?CAST\\(DATE_FORMAT\\(`[^`]+`, \\'[^\\']+\\'\\) AS CHAR\\) LIKE filter_param_[0-9]+", RegexOptions.IgnoreCase | RegexOptions.Compiled);

		public static IFilter GetFilter(ITableObject table, string toParse, bool decode, IColumnCollection cols = null)
		{
			try
			{
				StringBuilder parseSB = new StringBuilder(toParse);
				string filterType = GetFilterType(toParse, parseSB);
				return filterType switch
				{
					"and" => GetFilterWithConditions(new AndFilter(), table, parseSB, decode, cols), 
					"or" => GetFilterWithConditions(new OrFilter(), table, parseSB, decode, cols), 
					"between" => GetBetweenFilter(table, parseSB, decode, cols), 
					"in" => GetColSetFilter(table, parseSB, decode, cols), 
					_ => GetColValueFilter(filterType, table, parseSB, decode, cols), 
				};
			}
			catch (Exception)
			{
				return null;
			}
		}

		private static string GetFilterType(string toParse, StringBuilder parseSB)
		{
			int parenthesis = toParse.IndexOf('(');
			if (CheckOuterBrackets(toParse, parenthesis))
			{
				CutFilterAndOuterBrackets(parseSB, parenthesis);
				return toParse.Substring(0, parenthesis).ToLower();
			}
			return string.Empty;
		}

		public static void CutFilterAndOuterBrackets(StringBuilder parseSB, int skip = 0)
		{
			parseSB.Remove(0, skip + 1);
			parseSB.Length--;
		}

		public static bool CheckDataType(SqlType type, object value)
		{
			switch (type)
			{
			case SqlType.Integer:
			{
				if (!long.TryParse(value.ToString(), out var _))
				{
					return false;
				}
				break;
			}
			case SqlType.Decimal:
			{
				if (!double.TryParse(value.ToString(), out var _))
				{
					return false;
				}
				break;
			}
			case SqlType.DateTime:
				if (value.ToString().Length != 14 || !value.ToString().All((char c) => char.IsDigit(c)))
				{
					return false;
				}
				break;
			case SqlType.Date:
				if (value.ToString().Length != 14 || !value.ToString().All((char c) => char.IsDigit(c)))
				{
					return false;
				}
				break;
			}
			return true;
		}

		public static List<string> ParseParameters(StringBuilder parseSB)
		{
			List<string> parameters = new List<string>();
			StringBuilder parameterSB = new StringBuilder();
			bool readingString = false;
			//foreach (char c in parseSB)
			//{
			//	if (c == '"' && (parseSB.Length == 0 || parseSB[parseSB.Length - 1] != '\\'))
			//	{
			//		readingString = !readingString;
			//	}
			//	else if (!readingString && c == ',')
			//	{
			//		parameters.Add(parameterSB.ToString());
			//		parameterSB.Clear();
			//		continue;
			//	}
			//	parameterSB.Append(c);
			//}
			if (readingString)
			{
				throw new Exception("parse error");
			}
			if (parseSB.Length > 0)
			{
				parameters.Add(parameterSB.ToString());
			}
			return parameters;
		}

		public static IFilter GetBetweenFilter(ITableObject table, StringBuilder parseSB, bool decode, IColumnCollection cols = null)
		{
			List<string> parameters = ParseParameters(parseSB);
			if (parameters == null || parameters.Count != 3)
			{
				return null;
			}
			IColumn col = TryGetColumn(cols, parameters[1]);
			if (col == null)
			{
				col = TryGetColumn(table, parameters[1]);
			}
			return new BetweenFilter(col, DoConvert(col, parameters[0], decode), DoConvert(col, parameters[2], decode));
		}

		private static IColumn TryGetColumn(IColumnCollection cols, string columnParameter)
		{
			IColumn col = null;
			int colId = -1;
			if (columnParameter.StartsWith("%") && columnParameter.Length > 1 && int.TryParse(columnParameter.Substring(1), out colId))
			{
				try
				{
					col = cols[colId];
					return col;
				}
				catch (Exception)
				{
					return col;
				}
			}
			return col;
		}

		private static IColumn TryGetColumn(ITableObject table, string columnParameter)
		{
			return TryGetColumn(table.Columns, columnParameter);
		}

		public static IFilter GetColValueFilter(string filterType, ITableObject table, StringBuilder parseSB, bool decode, IColumnCollection cols)
		{
			string operStr = (from x in Enum.GetNames(typeof(Operators))
				where x.Equals(filterType, StringComparison.OrdinalIgnoreCase)
				select x).FirstOrDefault();
			if (operStr == null)
			{
				return null;
			}
			List<string> parameters = ParseParameters(parseSB);
			if (parameters == null || parameters.Count != 2)
			{
				return null;
			}
			IColumn col = TryGetColumn(cols, parameters[0]);
			if (cols == null)
			{
				col = TryGetColumn(table, parameters[0]);
			}
			Operators oper = (Operators)Enum.Parse(typeof(Operators), operStr);
			return new ColValueFilter(col, oper, DoConvert(col, parameters[1], decode));
		}

		public static string ByteToHex(byte[] val)
		{
			string hex = BitConverter.ToString(val);
			return "0x" + hex.Replace("-", "").ToLower();
		}

		public static byte[] StringToByteArrayFastest(string hex)
		{
			if (hex.StartsWith("0x") && hex.Length > 2)
			{
				hex = hex.Substring(2);
				if (hex.Length % 2 == 1)
				{
					throw new Exception("The binary key cannot have an odd number of digits");
				}
				byte[] arr = new byte[hex.Length >> 1];
				for (int i = 0; i < hex.Length >> 1; i++)
				{
					arr[i] = (byte)((GetHexVal(hex[i << 1]) << 4) + GetHexVal(hex[(i << 1) + 1]));
				}
				return arr;
			}
			return null;
		}

		public static int GetHexVal(char hex)
		{
			return hex - ((hex < ':') ? 48 : 87);
		}

		public static IFilter GetColSetFilter(ITableObject table, StringBuilder toParse, bool decode, IColumnCollection cols = null)
		{
			List<string> parameters = ParseParameters(toParse);
			if (parameters == null || parameters.Count < 2)
			{
				return null;
			}
			ColSetFilter filter = new ColSetFilter();
			filter.Column = TryGetColumn(cols, parameters[0]);
			if (filter.Column == null)
			{
				filter.Column = TryGetColumn(table, parameters[0]);
			}
			for (int i = 1; i < parameters.Count; i++)
			{
				filter.Values.Add(DoConvert(filter.Column, parameters[i], decode));
			}
			return filter;
		}

		public static IFilter GetFilterWithConditions(IFilter filter, ITableObject table, StringBuilder parseSB, bool decode, IColumnCollection cols)
		{
			StringBuilder nextParseSB = new StringBuilder();
			int level = 0;
			bool readingString = false;
			//foreach (char c in parseSB)
			//{
			//	if (c == '"' && (parseSB.Length == 0 || parseSB[parseSB.Length - 1] != '\\'))
			//	{
			//		readingString = !readingString;
			//	}
			//	else if (!readingString)
			//	{
			//		switch (c)
			//		{
			//		case '(':
			//			level++;
			//			break;
			//		case ')':
			//		{
			//			level--;
			//			if (level != 0)
			//			{
			//				break;
			//			}
			//			nextParseSB.Append(c);
			//			string nextParseString = nextParseSB.ToString();
			//			while (nextParseString[0] == ',')
			//			{
			//				nextParseString = nextParseString.Substring(1, nextParseString.Length - 1);
			//			}
			//			IFilter filter2 = GetFilter(table, nextParseString, decode, cols);
			//			if (filter2 != null)
			//			{
			//				if (filter is AndFilter)
			//				{
			//					((AndFilter)filter).AddCondition(filter2);
			//				}
			//				if (filter is OrFilter)
			//				{
			//					((OrFilter)filter).AddCondition(filter2);
			//				}
			//			}
			//			nextParseSB.Clear();
			//			continue;
			//		}
			//		}
			//	}
			//	nextParseSB.Append(c);
			//}
			if (readingString)
			{
				throw new Exception("parse error");
			}
			return filter;
		}

		private static object DoConvert(IColumn column, string value, bool decode)
		{
			if (value.Length >= 2 && value[0] == '"' && value[value.Length - 1] == '"')
			{
				value = value.Substring(1, value.Length - 2);
				if (decode)
				{
					value = Encoding.UTF8.GetString(Convert.FromBase64String(value));
				}
				return value;
			}
			if (value.StartsWith("0x") && column != null)
			{
				if (column.DataType == SqlType.Integer && long.TryParse(value, out var toLong))
				{
					return toLong;
				}
				if ((column.DataType == SqlType.Date || column.DataType == SqlType.Time || column.DataType == SqlType.DateTime) && DateTime.TryParse(value, out var toDateTime))
				{
					return toDateTime.ToString("yyyyMMddHHmmss");
				}
			}
			return null;
		}

		public static bool CheckOuterBrackets(string toParse, int skip)
		{
			if (toParse[skip] == '(')
			{
				return toParse[toParse.Length - 1] == ')';
			}
			return false;
		}

		public static List<string> GetFilterTypesFromIssueCommand(string toParse)
		{
			List<string> filterTypes = new List<string>();
			try
			{
				SortedDictionary<int, string> helper = new SortedDictionary<int, string>();
				ProcessMatches(betweenRegex.Matches(toParse), helper, "between");
				ProcessMatches(colSetRegex.Matches(toParse), helper, "in");
				ProcessMatches(colValueRegex.Matches(toParse), helper, "colvalue");
				ProcessMatches(colValueRegex2.Matches(toParse), helper, "like");
				ProcessMatches(colValueRegex3.Matches(toParse), helper, "startswith");
				ProcessMatches(colValueRegex4.Matches(toParse), helper, "like");
				foreach (KeyValuePair<int, string> item in helper)
				{
					filterTypes.Add(item.Value);
					if (item.Value == "between" || item.Value == "not between")
					{
						filterTypes.Add(item.Value);
					}
				}
				return filterTypes;
			}
			catch (Exception)
			{
				return filterTypes;
			}
		}

		private static void ProcessMatches(MatchCollection matches, SortedDictionary<int, string> helper, string filterType)
		{
			foreach (Match i in matches)
			{
				if (!(filterType == "between"))
				{
					if (filterType == "colvalue")
					{
						if (i.Groups[2].Value.Length <= 2)
						{
							helper.Add(i.Index, i.Groups[2].Value);
						}
					}
					else
					{
						helper.Add(i.Index, filterType);
					}
				}
				else
				{
					helper.Add(i.Index, i.Groups[3].Value.ToLower());
				}
			}
		}
	}
}
