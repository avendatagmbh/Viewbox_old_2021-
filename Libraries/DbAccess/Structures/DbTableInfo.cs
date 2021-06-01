using System.Collections.Generic;

namespace DbAccess.Structures
{
	public class DbTableInfo
	{
		private readonly List<DbColumnInfo> _columns = new List<DbColumnInfo>();

		public string Catalog { get; private set; }

		public string Schema { get; private set; }

		public string Name { get; private set; }

		private string Type { get; set; }

		private string Remarks { get; set; }

		public string Filter { get; private set; }

		public List<DbIndexInfo> TableIndexes { get; set; }

		public List<DbColumnInfo> Columns => _columns;

		public DbTableInfo(string catalog, string schema, string name, string type, string remarks, string filter = "")
		{
			Catalog = catalog?.Trim();
			Schema = schema?.Trim();
			Name = name?.Trim();
			Type = type?.Trim();
			Remarks = remarks?.Trim();
			Filter = filter?.Trim();
		}

		public override string ToString()
		{
			string result = string.Empty;
			if (!string.IsNullOrEmpty(Catalog))
			{
				result += Catalog;
			}
			if (!string.IsNullOrEmpty(Schema))
			{
				result = result + ((result.Length > 0) ? "." : string.Empty) + Schema;
			}
			if (!string.IsNullOrEmpty(Name))
			{
				result = result + ((result.Length > 0) ? "." : string.Empty) + Name;
			}
			return result;
		}
	}
}
