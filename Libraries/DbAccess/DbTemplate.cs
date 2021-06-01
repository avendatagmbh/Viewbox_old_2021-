using System.Collections.Generic;

namespace DbAccess
{
	public class DbTemplate
	{
		public string Filename { get; set; }

		public bool IsUserDefined { get; set; }

		public string ServerName { get; set; }

		public string QuoteLeft { get; set; }

		public string QuoteRight { get; set; }

		public IEnumerable<ConnectionStringParam> Params { get; set; }
	}
}
