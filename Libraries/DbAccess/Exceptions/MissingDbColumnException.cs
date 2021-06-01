using System;

namespace DbAccess.Exceptions
{
	public class MissingDbColumnException : Exception
	{
		public MissingDbColumnException(string classname, string propertyName)
			: base("Missing DbColumn for property \"" + propertyName + "\" in class \"" + classname + "\".")
		{
		}
	}
}
