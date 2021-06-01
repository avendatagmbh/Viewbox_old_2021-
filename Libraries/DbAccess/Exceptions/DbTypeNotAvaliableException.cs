using System;

namespace DbAccess.Exceptions
{
	public class DbTypeNotAvaliableException : Exception
	{
		internal DbTypeNotAvaliableException(string dbType)
			: base("The database type '" + dbType + "' is not avaliable.")
		{
		}
	}
}
