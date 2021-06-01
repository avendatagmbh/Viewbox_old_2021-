using System.Collections.Generic;

namespace DbAccess.Structures
{
	public class DbKey
	{
		public string TableID { get; set; }

		public List<string> IdxColumnID { get; set; }

		public double TimeToUsed { get; set; }
	}
}
