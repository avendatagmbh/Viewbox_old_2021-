using System.Collections;
using System.Collections.Generic;
using SystemDb;
using DbAccess;

namespace ViewboxDb.Filters
{
	public class NotBetweenFilter : BetweenFilter
	{
		public NotBetweenFilter(IColumn column, object value1, object value2)
			: base(column, value1, value2)
		{
		}

		public override string ToString()
		{
			return $"{base.Column.Name} NOT BETWEEN {base.Value1} AND {base.Value2}";
		}

		public override string ToString(DatabaseBase db)
		{
			return $"{db.Enquote(base.Column.Name)} NOT BETWEEN {db.GetValueString(base.Value1)} AND {db.GetValueString(base.Value2)}";
		}

		public override string ToString(DatabaseBase db, string prefix)
		{
			return $"{prefix}.{db.Enquote(base.Column.Name)} NOT BETWEEN {db.GetValueString(base.Value1)} AND {db.GetValueString(base.Value2)}";
		}

		public override string ToCommandString(DatabaseBase db, Hashtable commandParameters)
		{
			commandParameters.Add("@filter_param_" + commandParameters.Count, base.Value1);
			commandParameters.Add("@filter_param_" + commandParameters.Count, base.Value2);
			return string.Format("{0} NOT BETWEEN {1} AND {2}", db.Enquote(base.Column.Name), "@filter_param_" + (commandParameters.Count - 2), "@filter_param_" + (commandParameters.Count - 1));
		}

		public override string GetReplacedFilter(DatabaseBase db, ref int i, List<bool> freeselection, ref int j)
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
			return string.Format("(ifnull({1},'') = '' OR ifnull({2},'') = '' OR {0} NOT BETWEEN {1} AND {2})", db.Enquote(base.Column.Name), param1Name, param2Name);
		}

		public override string ToOriginalString()
		{
			return string.Format("NotBetween({1},{0},{2})", "%" + ((base.Column != null) ? base.Column.Id : 0), string.Concat("\"", base.Value1, "\""), string.Concat("\"", base.Value2, "\""));
		}

		public override string GetToolTipText()
		{
			return $" ({base.Column} NOT BETWEEN {base.Value1} AND {base.Value2})";
		}
	}
}
