using System;

namespace ViewboxBusiness.Exceptions
{
	public class InvalidScriptException : Exception
	{
		public InvalidScriptException(string message)
			: base(message)
		{
		}
	}
}
