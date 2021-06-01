using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SystemDb;
using DbAccess;

namespace ViewboxDb.Filters
{
	public class ColSetFilter : IFilter
	{
		public IColumn Column { get; set; }

		public ISet<object> Values { get; private set; }

		public ColSetFilter()
		{
			Values = new HashSet<object>();
		}

		public string ToString(DatabaseBase db)
		{
			string cname = db.Enquote(Column.Name);
			if (Values.Any((object v) => v.ToString().ToLower() == "dbnull"))
			{
				return string.Format("({0} IN ({1}) OR {0} IS NULL)", cname, string.Join("', '", db.GetValueString(Values.All((object v) => v != null))));
			}
			return string.Format("{0} IN ({1})", cname, string.Join(", ", Values.Select((object v) => db.GetValueString(v))));
		}

		public string ToString(DatabaseBase db, string prefix)
		{
			string cname = db.Enquote(Column.Name);
			if (Values.Any((object v) => v.ToString().ToLower() == "dbnull"))
			{
				return string.Format("({0} IN ({1}) OR {0} IS NULL)", prefix + "." + cname, string.Join("', '", db.GetValueString(Values.All((object v) => v != null))));
			}
			return string.Format("{0}.{1} IN ({2})", prefix, cname, string.Join(", ", Values.Select((object v) => db.GetValueString(v))));
		}

		public string ToCommandString(DatabaseBase db, Hashtable commandParameters)
		{
			string result = "";
			foreach (object val in Values.Where((object w) => w != null))
			{
				commandParameters.Add("@filter_param_" + commandParameters.Count, db.GetValueString(val));
				result = result + (string.IsNullOrEmpty(result) ? "" : ", ") + "'@filter_param_" + (commandParameters.Count - 1) + "'";
			}
			return string.Format(Values.Any((object w) => w == null) ? "(({0} IN ({1}) OR {0} IS NULL))" : "({0} IN ({1})", db.Enquote(Column.Name), result);
		}

		public void ReplaceColumn(ITableObject tobj)
		{
			IColumn column = (Column = tobj.Columns[Column.Name]);
		}

		public List<Tuple<IFilter, string, object, IColumn>> GetParameters(List<Tuple<IFilter, string, object, IColumn>> parameters)
		{
			foreach (object v in Values)
			{
				string paramName = "filter_param_" + parameters.Count;
				Tuple<IFilter, string, object, IColumn> tuple = new Tuple<IFilter, string, object, IColumn>(this, paramName, v, Column);
				if (!parameters.Contains(tuple))
				{
					parameters.Add(tuple);
				}
			}
			return parameters;
		}

		public string GetReplacedFilter(DatabaseBase db, ref int i, List<bool> freeselection, ref int j)
		{
			if (freeselection[j++])
			{
				foreach (object value in Values)
				{
					if (value != null)
					{
						i++;
					}
				}
				return string.Empty;
			}
			string result = string.Empty;
			foreach (object value2 in Values)
			{
				if (value2 != null)
				{
					if (!string.IsNullOrWhiteSpace(result))
					{
						result += ", ";
					}
					result = result + "filter_param_" + i;
					i++;
				}
			}
			return string.Format(Values.Any((object v) => v == null) ? "(ifnull({0},'') = '' OR {0} IN ({1}))" : "({0} IN ({1}))", db.Enquote(Column.Name), result);
		}

		public List<Tuple<IFilter, IColumn, string, object, string>> GetParameters(List<Tuple<IFilter, IColumn, string, object, string>> parameters)
		{
			foreach (object v in Values)
			{
				string paramName = "filter_param_" + parameters.Count;
				Tuple<IFilter, IColumn, string, object, string> tuple = new Tuple<IFilter, IColumn, string, object, string>(this, Column, paramName, v, "∈");
				if (!parameters.Contains(tuple))
				{
					parameters.Add(tuple);
				}
			}
			return parameters;
		}

		public string ToOriginalString()
		{
			return string.Format("In({0},{1})", "%" + Column.Id, string.Join(",", Values.Select((object v) => string.Concat("\"", v, "\""))));
		}

		public bool CheckFilter()
		{
			bool ret = true;
			foreach (object v in Values)
			{
				ret &= Filter.CheckDataType(Column.DataType, v);
			}
			return ret;
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
			return Values.FirstOrDefault();
		}

		public override string ToString()
		{
			if (Values.Any((object v) => v.ToString().ToLower() == "dbnull"))
			{
				return string.Format("({0} IN ({1}) OR {0} IS NULL)", Column.Name, string.Join("', '", Values.All((object v) => v != null)));
			}
			return string.Format("{0} IN ({1})", Column.Name, string.Join(", ", Values.Select((object v) => v)));
		}

		public override int GetHashCode()
		{
			return (((Column != null) ? Column.Name : null) + "IN" + string.Join("$", Values.Select((object v) => v.ToString()))).GetHashCode();
		}

		public List<string> GetParameters(List<string> list)
		{
			foreach (object value in Values)
			{
				_ = value;
				string paramName = "filter_param_" + list.Count;
				if (!list.Contains(paramName))
				{
					list.Add(paramName);
				}
			}
			return list;
		}

		public string GetToolTipText()
		{
			return "    (" + ToString() + ")";
		}
	}
}
