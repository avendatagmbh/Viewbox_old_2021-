using System;

namespace Viewbox.Exceptions
{
	public class BadPasswordException : Exception
	{
		public BadPasswordException(string message)
			: base(message)
		{
		}
	}
}
