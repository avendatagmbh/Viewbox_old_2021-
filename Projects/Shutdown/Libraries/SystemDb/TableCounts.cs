using System.Collections.Generic;

namespace SystemDb
{
	public struct TableCounts
	{
		public int ArchivedTableCount;

		public IEnumerable<ITableObject> ArchivedTables;

		public int EmptyTableCount;

		public IEnumerable<ITableObject> EmptyTables;

		public int InVisibleTableCount;

		public IEnumerable<ITableObject> InVisibleTables;

		public int EmptyInVisibleTableCount;

		public IEnumerable<ITableObject> EmptyInVisibleTables;

		public int VisibleTableCount;

		public IEnumerable<ITableObject> VisibleTables;
	}
}
