using DbAccess.Attributes;

namespace SystemDb.Internal
{
	[DbTable("order_areas", ForceInnoDb = true)]
	public class OrderArea : IOrderArea
	{
		[DbColumn("id", AutoIncrement = true)]
		[DbPrimaryKey]
		public int Id { get; set; }

		[DbColumn("table_id")]
		public int TableId { get; set; }

		[DbColumn("language_value", Length = 2, StoreAsVarBinary = true)]
		public string LanguageValue { get; set; }

		[DbColumn("index_value", Length = 128)]
		public string IndexValue { get; set; }

		[DbColumn("split_value", Length = 128)]
		public string SplitValue { get; set; }

		[DbColumn("sort_value", Length = 128)]
		public string SortValue { get; set; }

		[DbColumn("start")]
		public long Start { get; set; }

		[DbColumn("end")]
		public long End { get; set; }
	}
}
