using System;
using System.Collections.Generic;
using SystemDb;
using ArchiveWebServiceInterface;
using Viewbox.Properties;

namespace Viewbox.Models
{
	public class DocumentsModelNew : ViewboxModel
	{
		public IArchive Table { get; set; }

		public List<Document> Documents { get; set; }

		public string TemporaryDirectory { get; set; }

		public override string LabelCaption => Resources.DocumentArchive;

		public ITableObject OriginalTable { get; set; }

		public long RowsCount { get; internal set; }

		public int RowsPerPage { get; internal set; }

		public long FromRow { get; internal set; }

		public long ToRow => Math.Min(RowsCount, FromRow + RowsPerPage - 1);

		public string LabelNumberOfRows => Resources.NumberOfRows;

		public string LabelNoData => Resources.NoData;

		public DocumentsModelNew()
		{
			Documents = new List<Document>();
		}
	}
}
