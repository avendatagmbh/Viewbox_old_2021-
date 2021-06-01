namespace SystemDb
{
	public interface IUserObjects
	{
		ICategoryCollection Categories { get; }

		ITableObjectCollection TableObjects { get; }

		ICategoryCollection IssueCategories { get; }

		IIssueCollection Issues { get; }

		int IssueCount { get; }

		ICategoryCollection ViewCategories { get; }

		IViewCollection Views { get; }

		int ViewCount { get; }

		ICategoryCollection TableCategories { get; }

		ITableCollection Tables { get; }

		int TableCount { get; }

		IArchiveCollection Archives { get; }

		IArchiveDocumentCollection ArchiveDocuments { get; }

		IFullColumnCollection Columns { get; }

		IOptimizationGroupCollection OptimizationGroups { get; }

		IOptimizationCollection Optimizations { get; }

		RightObjectTree RightObjectTree { get; }
	}
}
