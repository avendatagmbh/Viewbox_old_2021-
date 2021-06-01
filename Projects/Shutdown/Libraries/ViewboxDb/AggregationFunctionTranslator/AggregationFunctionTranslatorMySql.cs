namespace ViewboxDb.AggregationFunctionTranslator
{
	internal class AggregationFunctionTranslatorMySql : IAggregationFunctionTranslator
	{
		public string ConvertEnumToString(AggregationFunction aggregationFunction)
		{
			return aggregationFunction.ToString();
		}
	}
}
