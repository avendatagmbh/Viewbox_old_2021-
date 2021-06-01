using System;
using System.Collections.Generic;
using System.Linq;
using SystemDb;
using Viewbox.Properties;
using ViewboxDb;

namespace Viewbox.Models
{
	public class ListOptionsModel : BaseModel
	{
		public SortDirection Direction;

		public int SortColumn;

		public List<ITableObject> TableObjects { get; set; }

		public bool ShowEmpty { get; set; }

		public bool ShowHidden { get; set; }

		public bool ShowArchived { get; set; }

		public bool ShowEmptyHidden { get; set; }

		public TableType Type { get; set; }

		public string Search { get; set; }

		public int CheckedTableItemsCount => ViewboxSession.CheckedTableItems.Count((KeyValuePair<int, bool> itm) => itm.Value);

		public int Page { get; set; }

		public int Size { get; set; }

		public int LastPage => (Size != 0) ? ((Count - Count % Size) / Size) : 0;

		public int Count { get; set; }

		public string LabelNumberOfRows => Resources.NumberOfRows;

		public string LabelNoData => Resources.NoData;

		public int From => Size * Page;

		public int To => Math.Min(Count, From + Size);

		public bool ListArchiveOptionsAelectAllState => Count == ViewboxSession.CheckedTableItems.Count;

		public ListOptionsModel()
		{
			TableObjects = new List<ITableObject>();
			ShowEmpty = false;
			ShowHidden = false;
			ShowArchived = false;
			ShowEmptyHidden = false;
			Type = TableType.All;
			Search = "";
			Page = 0;
			Size = 25;
			Count = 0;
			SortColumn = 1;
			Direction = SortDirection.Ascending;
		}

		public bool CheckedTableItems(int key)
		{
			return ViewboxSession.CheckedTableItems.ContainsKey(key) && ViewboxSession.CheckedTableItems[key];
		}
	}
}
