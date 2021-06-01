using System.Collections.Generic;

namespace ViewboxDb
{
	public class Description
	{
		public string CountryCode { get; set; }

		public List<string> Descriptions { get; set; }

		public Description()
		{
		}

		public Description(string countryCode, List<string> descriptions)
		{
			CountryCode = countryCode;
			Descriptions = descriptions;
		}
	}
}
