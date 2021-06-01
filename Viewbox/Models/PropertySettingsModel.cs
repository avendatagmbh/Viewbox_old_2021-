using System.Collections.Generic;
using SystemDb;

namespace Viewbox.Models
{
	public class PropertySettingsModel : SettingsModel
	{
		public class IdValuePair
		{
			public int Id { get; set; }

			public string Value { get; set; }
		}

		public override string Partial => "_PropertiesPartial";

		public IEnumerable<IProperty> Properties { get; set; }
	}
}
