using System;
using DbAccess.Attributes;

namespace ViewboxDb
{
	[DbTable("archive_documents")]
	public class ArchiveDocuments
	{
		[DbColumn("id", AutoIncrement = true)]
		[DbPrimaryKey]
		public int Id { get; set; }

		[DbColumn("_row_no_")]
		public int RowNo { get; set; }

		[DbColumn("identifier", Length = 128)]
		public string Identifier { get; set; }

		[DbColumn("doc_id", Length = 128)]
		public string DocId { get; set; }

		[DbColumn("type")]
		public int Type { get; set; }

		[DbColumn("created")]
		public DateTime Created { get; set; }

		[DbColumn("modified")]
		public DateTime Modified { get; set; }

		[DbColumn("size")]
		public int Size { get; set; }

		[DbColumn("content_type")]
		public string ContentType { get; set; }

		[DbColumn("belegart")]
		public string Belegart { get; set; }
	}
}
