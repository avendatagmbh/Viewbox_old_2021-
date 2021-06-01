using System;
using System.Web;

namespace Viewbox
{
	public class HttpApplicationFactory
	{
		private static HttpApplicationStateBase _context;

		public static HttpApplicationStateBase Current
		{
			get
			{
				if (_context != null)
				{
					return _context;
				}
				throw new InvalidOperationException("HttpApplicationStateBase not available");
			}
		}

		public static void SetCurrentApplication(HttpApplicationStateBase context)
		{
			_context = context;
		}
	}
}
