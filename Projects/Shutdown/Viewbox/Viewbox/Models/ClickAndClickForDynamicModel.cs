using System.Collections.Generic;
using SystemDb;

namespace Viewbox.Models
{
	public class ClickAndClickForDynamicModel
	{
		public IIssue Issue { get; set; }

		public Dictionary<int, string> CnCParameter { get; set; }

		public List<string> CnCParameterName { get; set; }

		public List<int> TargerColumnId { get; set; }
	}
}
