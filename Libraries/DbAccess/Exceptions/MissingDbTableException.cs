using System;

namespace DbAccess.Exceptions
{
	public class MissingDbTableException : Exception
	{
		public MissingDbTableException(string classname)
			: base("Missing DbTable for class \"" + classname + "\".")
		{
		}
	}
}
