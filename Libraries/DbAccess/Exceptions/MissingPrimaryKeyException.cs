using System;

namespace DbAccess.Exceptions
{
	public class MissingPrimaryKeyException : Exception
	{
		public MissingPrimaryKeyException(string classname)
			: base("Missing primary key for class \"" + classname + "\".")
		{
		}
	}
}
