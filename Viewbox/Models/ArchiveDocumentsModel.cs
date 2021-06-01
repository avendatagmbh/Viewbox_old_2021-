using System;
using System.Data;
using SystemDb;
using Viewbox.Properties;

namespace Viewbox.Models
{
	public class ArchiveDocumentsModel : ViewboxModel, IColumnFilterModel
	{
		public ITableObject Table { get; set; }

		public DataTable Documents { get; set; }

		public string TemporaryDirectory { get; set; }

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

		public ArchiveDocumentsModel()
		{
			ColumnFilterEnabled = true;
		}
	}
}
