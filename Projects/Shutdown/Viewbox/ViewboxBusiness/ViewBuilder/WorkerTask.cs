namespace ViewboxBusiness.ViewBuilder
{
	public enum WorkerTask
	{
		Preparing,
		Working,
		Cleanup,
		CreatingIndex,
		WaitingForNextViewscript,
		CheckingReportParameters,
		CheckingProcedureParameters,
		CheckingWhereCondition,
		CopyTable,
		GetIndexInfo,
		DropExistingMetadata,
		GetColumnInfo,
		DeletingTableEntries,
		SavingIssue,
		CalculatingOrderArea,
		GeneratingDistinctValues
	}
}
