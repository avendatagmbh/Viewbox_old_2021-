using System.Collections.Generic;

namespace Viewbox.Models
{
	public class Storage
	{
		public Dictionary<int, string> Info { get; set; }

		public Storage()
		{
			Info = new Dictionary<int, string>();
		}
	}
}
