using System.Collections.Generic;

namespace Viewbox.Structures
{
	public class StructureDyn : StructureGraph
	{
		public Dictionary<int, string> FIELD { get; set; }

		public Dictionary<int, double> VALUE { get; set; }

		public Dictionary<int, string> ADDITIONAL_FIELDS { get; set; }

		public StructureDyn()
		{
			VALUE = new Dictionary<int, double>();
			FIELD = new Dictionary<int, string>();
			ADDITIONAL_FIELDS = new Dictionary<int, string>();
		}

		public double GetValue(int id)
		{
			if (VALUE.ContainsKey(id))
			{
				return VALUE[id];
			}
			return 0.0;
		}
	}
}
