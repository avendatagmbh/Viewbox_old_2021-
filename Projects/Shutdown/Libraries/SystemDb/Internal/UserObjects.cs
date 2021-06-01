namespace SystemDb.Internal
{
	public class UserObjects : IUserObjects
	{
		public ICategoryCollection Categories { get; set; }

		public ITableObjectCollection TableObjects { get; set; }

		public ICategoryCollection IssueCategories { get; set; }

		public IIssueCollection Issues { get; set; }

		public int IssueCount { get; set; }

		public ICategoryCollection ViewCategories { get; set; }

		public IViewCollection Views { get; set; }

		public int ViewCount { get; set; }

		public ICategoryCollection TableCategories { get; set; }

		public ITableCollection Tables { get; set; }

		public int TableCount { get; set; }

		public IArchiveCollection Archives { get; set; }

		public IArchiveDocumentCollection ArchiveDocuments { get; set; }

		public IFullColumnCollection Columns { get; set; }

		public IOptimizationGroupCollection OptimizationGroups { get; set; }

		public IOptimizationCollection Optimizations { get; set; }

		public RightObjectTree RightObjectTree { get; set; }

		public UserObjects()
		{
			Categories = new CategoryCollection();
			TableObjects = new TableObjectCollection();
			IssueCategories = new CategoryCollection();
			Issues = new IssueCollection();
			IssueCount = 0;
			ViewCategories = new CategoryCollection();
			Views = new ViewCollection();
			ViewCount = 0;
			TableCategories = new CategoryCollection();
			Tables = new TableCollection();
			TableCount = 0;
			Archives = new ArchiveCollection();
			ArchiveDocuments = new ArchiveDocumentCollection();
			Columns = new FullColumnCollection();
			OptimizationGroups = new OptimizationGroupCollection();
			Optimizations = new OptimizationCollection();
			RightObjectTree = new RightObjectTree();
		}
	}
}
