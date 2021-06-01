using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using DbAccess;

namespace ViewboxBusiness.Common
{
	public class Index
	{
		public string Name { get; set; }

		public string Table { get; set; }

		public List<string> Columns { get; set; }

		public List<string> UnsortedColumns { get; set; }

		public long Complexity { get; set; }

		public Index()
		{
			Columns = new List<string>();
			UnsortedColumns = new List<string>();
		}

		public Index(string table, IEnumerable<string> columns)
		{
			Columns = new List<string>();
			UnsortedColumns = new List<string>();
			foreach (string c in columns)
			{
				AddColumn(c.ToLower());
			}
			Table = table.ToLower();
			SetDefaultName();
		}

		public Index(string sql)
		{
			Columns = new List<string>();
			UnsortedColumns = new List<string>();
			Regex createIndexCmdRegex = new Regex("CREATE +INDEX +([^ ]+)[ |\\t]+ON[ ]+`(.+)`[ ]*\\((.+)\\)", RegexOptions.IgnoreCase);
			Regex createIndexCmdRegex2 = new Regex("CREATE +INDEX +([^ ]+)[ |\\t]+ON[ ]+([^ ]+)[ ]*\\((.+)\\)", RegexOptions.IgnoreCase);
			string columns;
			if (createIndexCmdRegex.IsMatch(sql))
			{
				Match match3 = createIndexCmdRegex.Match(sql);
				Table = match3.Groups[2].Value.ToLower();
				Name = match3.Groups[1].Value;
				columns = match3.Groups[3].Value;
			}
			else if (createIndexCmdRegex2.IsMatch(sql))
			{
				Match match2 = createIndexCmdRegex2.Match(sql);
				Table = match2.Groups[2].Value.ToLower();
				Name = match2.Groups[1].Value;
				columns = match2.Groups[3].Value;
			}
			else
			{
				createIndexCmdRegex = new Regex("ALTER[ ]+TABLE[ ]+([^ ]+)[ ]+ADD[ ]+INDEX[ ]+([^ ]+)[ ]+\\((.+)\\)", RegexOptions.IgnoreCase);
				if (!createIndexCmdRegex.IsMatch(sql))
				{
					throw new Exception("Der Index konnte nicht korrekt aus dem Befehl ausgelesen werden: " + Environment.NewLine + sql);
				}
				Match match = createIndexCmdRegex.Match(sql);
				Table = match.Groups[1].Value.ToLower();
				Name = match.Groups[2].Value;
				columns = match.Groups[3].Value;
			}
			string[] array = columns.Split(',');
			foreach (string column in array)
			{
				if (column.Trim().Length > 0)
				{
					AddColumn(column.Trim().ToLower());
				}
			}
			SetDefaultName();
		}

		public static bool CheckSql(string sql)
		{
			if (new Regex("CREATE[ ]+INDEX[ ]+([^ ]+)[ ]+ON[ ]+([^ ]+)[ ]*\\((.+)\\)", RegexOptions.IgnoreCase).IsMatch(sql))
			{
				return true;
			}
			return new Regex("ALTER[ ]+TABLE[ ]+([^ ]+)[ ]+ADD[ ]+INDEX[ ]+([^ ]+)[ ]+\\((.+)\\)", RegexOptions.IgnoreCase).IsMatch(sql);
		}

		public void SetDefaultName()
		{
			if (Table != null)
			{
				Name = "_" + Table.ToLower().Replace(" ", "_");
			}
			foreach (string column in UnsortedColumns)
			{
				Name = Name + "_" + column.ToLower().Replace(" ", "_");
			}
		}

		public override int GetHashCode()
		{
			return Name.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			Index i = obj as Index;
			if (i != null && i.GetHashCode() == GetHashCode())
			{
				return true;
			}
			return false;
		}

		public void AddColumn(string name)
		{
			name = name.Replace("`", "");
			if (!Columns.Contains(name))
			{
				Columns.Add(name);
				Columns.Sort();
				UnsortedColumns.Add(name);
				SetDefaultName();
			}
		}

		public bool Exists(DatabaseBase conn)
		{
			return conn.IsIndexExisting(Table, Columns);
		}
	}
}
