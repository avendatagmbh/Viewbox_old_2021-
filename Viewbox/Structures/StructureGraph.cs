using System.Collections.Generic;

namespace Viewbox.Structures
{
	public class StructureGraph
	{
		public string ID { get; set; }

		public string PARENT { get; set; }

		public int ORDINAL { get; set; }

		public Dictionary<int, string> DESC { get; set; }

		public string LANG_KEY { get; set; }

		public string MANDT { get; set; }

		public string BUKRS { get; set; }

		public string GJAHR { get; set; }

		public StructureGraph()
		{
			DESC = new Dictionary<int, string>();
		}

		public string GetFullDesc()
		{
			return string.Join(" ", DESC.Values);
		}
	}
}
