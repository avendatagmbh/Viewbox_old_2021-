using DbAccess.Attributes;

namespace ViewboxDb
{
	[DbTable("")]
	public class Temp
	{
		[DbColumn("id", AutoIncrement = true)]
		[DbPrimaryKey]
		public int id { get; set; }

		[DbColumn("_row_no_")]
		[DbUniqueKey]
		public int _row_no_ { get; set; }
	}
}
