using System.Web;

namespace Viewbox
{
	public class HttpContextFactory
	{
		private static HttpContextBase _context;

		public static HttpContextBase Current
		{
			get
			{
				if (_context != null)
				{
					return _context;
				}
				return (HttpContext.Current == null) ? null : new HttpContextWrapper(HttpContext.Current);
			}
		}

		public static void SetCurrentContext(HttpContextBase context)
		{
			_context = context;
		}
	}
}
