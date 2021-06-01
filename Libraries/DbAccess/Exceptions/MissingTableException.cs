using System;

namespace DbAccess.Exceptions
{
	public class MissingTableException : Exception
	{
		public MissingTableException(string tableName, string message)
			: base("Table \"" + tableName + "\" is missing. " + message)
		{
		}
	}
}
