using System.Collections.Generic;

namespace DbAccess.Structures
{
	public class DbIndexInfo
	{
		public DbIndexType Type { get; set; }

		public string Name { get; set; }

		public string TableName { get; set; }

		public string OriginalType { get; set; }

		public bool? AllowNull { get; set; }

		public string Comment { get; set; }

		public List<string> Columns { get; set; }
	}
}
