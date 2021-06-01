using System;

namespace ViewboxDb.AggregationFunctionTranslator
{
	public static class DatabaseSpecificFactory
	{
		private static string _dbType;

		public static void Init(string dbType)
		{
			_dbType = dbType;
		}

		public static IAggregationFunctionTranslator GetAggregationFunctionTranslator()
		{
			string dbType = _dbType;
			if (!(dbType == "MySQL"))
			{
				if (dbType == "SQLServer")
				{
					return new AggregationFunctionTranslatorSqlServer();
				}
				throw new NotImplementedException();
			}
			return new AggregationFunctionTranslatorMySql();
		}
	}
}
