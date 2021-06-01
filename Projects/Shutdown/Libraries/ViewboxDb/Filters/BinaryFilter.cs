using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SystemDb;
using DbAccess;

namespace ViewboxDb.Filters
{
	public abstract class BinaryFilter : IFilter
	{
		public ISet<IFilter> Conditions { get; set; }

		protected string KeyWord { get; set; }

		public BinaryFilter()
		{
			Conditions = new HashSet<IFilter>();
		}

		public string ToString(DatabaseBase db)
		{
			return string.Format("({0})", string.Join(" " + KeyWord + " ", Conditions.Select((IFilter c) => c.ToString(db))));
		}

		public string ToString(DatabaseBase connection, string prefix)
		{
			return string.Format("({0})", string.Join(" " + KeyWord + " ", Conditions.Select((IFilter c) => c.ToString(connection, prefix))));
		}

		public string ToCommandString(DatabaseBase db, Hashtable commandParameters)
		{
			string result = "";
			foreach (IFilter condition in Conditions)
			{
				result = result + (string.IsNullOrEmpty(result) ? "" : (" " + KeyWord + " ")) + condition.ToCommandString(db, commandParameters);
			}
			return result;
		}

		public void ReplaceColumn(ITableObject tobj)
		{
			foreach (IFilter condition in Conditions)
			{
				condition?.ReplaceColumn(tobj);
			}
		}

		public List<Tuple<IFilter, string, object, IColumn>> GetParameters(List<Tuple<IFilter, string, object, IColumn>> parameters)
		{
			foreach (IFilter condition in Conditions)
			{
				parameters = condition.GetParameters(parameters);
			}
			return parameters;
		}

		public string GetReplacedFilter(DatabaseBase db, ref int i, List<bool> freeselection, ref int j)
		{
			if (Conditions.Count == 0)
			{
				return string.Empty;
			}
			string output = string.Empty;
			foreach (IFilter condition in Conditions)
			{
				string filter = condition.GetReplacedFilter(db, ref i, freeselection, ref j);
				if (filter != string.Empty)
				{
					if (!string.IsNullOrWhiteSpace(output))
					{
						output = output + " " + KeyWord + " ";
					}
					output += filter;
				}
			}
			if (output != string.Empty)
			{
				return $"({output})";
			}
			return string.Empty;
		}

		public List<Tuple<IFilter, IColumn, string, object, string>> GetParameters(List<Tuple<IFilter, IColumn, string, object, string>> parameters)
		{
			foreach (IFilter condition in Conditions)
			{
				parameters = condition.GetParameters(parameters);
			}
			return parameters;
		}

		public string ToOriginalString()
		{
			return string.Format("{1}({0})", string.Join(",", Conditions.Select((IFilter c) => c.ToOriginalString())), KeyWord);
		}

		public bool CheckFilter()
		{
			bool ret = true;
			foreach (IFilter c in Conditions)
			{
				if (c != null)
				{
					ret = ret && c.CheckFilter();
				}
			}
			return ret;
		}

		public bool HasIncorrectColumn()
		{
			return Conditions.Any((IFilter c) => c?.HasIncorrectColumn() ?? false);
		}

		public bool HasValue()
		{
			if (Conditions != null && Conditions.Count != 0)
			{
				return Conditions.Any((IFilter c) => c?.HasValue() ?? false);
			}
			return false;
		}

		public IFilter Clone()
		{
			return (IFilter)MemberwiseClone();
		}

		public void ReplaceRowNoFilters(IFilter parent, DatabaseBase connection, IndexDb indexDb, IColumn rowNoColumn)
		{
			foreach (IFilter item in Conditions.ToList())
			{
				item.ReplaceRowNoFilters(this, connection, indexDb, rowNoColumn);
			}
		}

		public object GetColumnValue(string column)
		{
			return (from filter in Conditions.ToList()
				select filter.GetColumnValue(column)).FirstOrDefault((object value) => value != null);
		}

		public override string ToString()
		{
			return string.Format("({0})", string.Join(" " + KeyWord + " ", Conditions.Select((IFilter c) => c.ToString())));
		}

		public override int GetHashCode()
		{
			return Conditions.GetHashCode() + KeyWord.GetHashCode();
		}

		public void AddCondition(IFilter filter)
		{
			if (filter != null)
			{
				Conditions.Add(filter);
			}
		}

		public string GetToolTipText()
		{
			return "    (" + ToString() + ")";
		}
	}
}
