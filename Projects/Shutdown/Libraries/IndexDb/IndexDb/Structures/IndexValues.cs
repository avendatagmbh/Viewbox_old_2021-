using System.Collections.Generic;

namespace IndexDb.Structures
{
	public class IndexValues
	{
		public int Id { get; set; }

		public int Start { get; set; }

		public int End { get; set; }

		private List<object> DistinctValues { get; set; }

		public IndexValues()
		{
			DistinctValues = new List<object>();
		}
	}
}
