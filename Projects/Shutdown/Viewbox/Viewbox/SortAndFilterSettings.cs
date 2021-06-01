using ViewboxDb;

namespace Viewbox
{
	public class SortAndFilterSettings
	{
		private string _filter;

		private int _sortColumn;

		private SortDirection _sortDirection;

		public string Filter
		{
			get
			{
				return _filter;
			}
			set
			{
				_filter = value;
			}
		}

		public int SortColumn
		{
			get
			{
				return _sortColumn;
			}
			set
			{
				_sortColumn = value;
			}
		}

		public SortDirection Sort
		{
			get
			{
				return _sortDirection;
			}
			set
			{
				_sortDirection = value;
			}
		}
	}
}
