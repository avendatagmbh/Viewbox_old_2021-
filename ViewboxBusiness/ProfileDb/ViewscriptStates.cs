namespace ViewboxBusiness.ProfileDb
{
	public enum ViewscriptStates
	{
		Ready,
		CreatingIndex,
		CreateIndexError,
		CreatingTable,
		CreateTableError,
		CopyingTable,
		CopyError,
		Completed,
		Warning,
		GettingIndexInfo,
		GettingIndexInfoError,
		GeneratingDistinctValues,
		GeneratingDistinctValuesError,
		CheckingProcedureParameters,
		CheckingProcedureParametersError,
		CheckingReportParameters,
		CheckingReportParametersError,
		CheckingWhereCondition,
		CheckingWhereConditionError
	}
}
