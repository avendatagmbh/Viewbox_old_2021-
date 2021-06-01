namespace ViewboxDb.AggregationFunctionTranslator
{
	internal class AggregationFunctionTranslatorSqlServer : IAggregationFunctionTranslator
	{
		public string ConvertEnumToString(AggregationFunction aggregationFunction)
		{
			return aggregationFunction switch
			{
				AggregationFunction.Std => "STDEV", 
				AggregationFunction.Variance => "VAR", 
				_ => aggregationFunction.ToString(), 
			};
		}
	}
}
