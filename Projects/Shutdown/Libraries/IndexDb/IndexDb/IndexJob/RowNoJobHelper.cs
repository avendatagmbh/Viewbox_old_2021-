using System.Collections.Generic;
using DbAccess.Structures;

namespace IndexDb.IndexJob
{
	internal class RowNoJobHelper
	{
		public int OldValueId { get; set; }

		public int StartRowNo { get; set; }

		public List<DbColumnValues> ColumnValues { get; set; }
	}
}
