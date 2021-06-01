using System.Collections.Generic;

namespace ViewboxDb
{
	public class SubTotalParameters
	{
		public List<string> GroupList { get; set; }

		public List<string> ColumnList { get; set; }

		public string sql { get; set; }

		public bool OnlyExpectedResult { get; set; }
	}
}
