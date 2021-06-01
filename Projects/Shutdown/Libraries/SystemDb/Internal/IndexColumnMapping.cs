using System;
using DbAccess.Attributes;

namespace SystemDb.Internal
{
	[DbTable("index_columns", ForceInnoDb = true)]
	public class IndexColumnMapping : ICloneable
	{
		[DbColumn("id")]
		[DbPrimaryKey]
		public int Id { get; set; }

		[DbColumn("index_id")]
		[DbUniqueKey("uk_index_columns")]
		public int IndexId { get; set; }

		[DbColumn("column_id")]
		[DbUniqueKey("uk_index_columns")]
		public int ColumnId { get; set; }

		public object Clone()
		{
			return MemberwiseClone();
		}
	}
}
