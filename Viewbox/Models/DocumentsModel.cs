using System;
using System.Collections.Generic;
using System.Data;
using SystemDb;
using Ionic.Zip;
using Viewbox.Properties;

namespace Viewbox.Models
{
	public class DocumentsModel : ViewboxModel, IColumnFilterModel
	{
		public int TempId { get; set; }

		public IArchive Table { get; set; }

		public ITableObject OriginalTable { get; set; }

		public DataTable Documents { get; set; }

		public List<string> Params { get; set; }

		public bool IsZipArchive { get; set; }

		public string TemporaryDirectory { get; set; }

		public List<ZipEntry> Files { get; set; }

		public override string LabelCaption => Resources.DocumentArchive;

		public long RowsCount { get; internal set; }

		public int RowsPerPage { get; internal set; }

		public long FromRow { get; internal set; }

		public long ToRow => Math.Min(RowsCount, FromRow + RowsPerPage - 1);

		public string LabelNumberOfRows => Resources.NumberOfRows;

		public string LabelNoData => Resources.NoData;

		public bool ColumnFilterEnabled { get; set; }

		public bool ExactMatch { get; set; }

		public IColumn[] FilterColumns { get; set; }

		public string[] FilterColumnDescriptions { get; set; }

		public string[] ParameterValues { get; set; }

		public DocumentsModel()
		{
			Params = new List<string>();
			Files = new List<ZipEntry>();
			ColumnFilterEnabled = true;
			ExactMatch = true;
		}
	}
}
