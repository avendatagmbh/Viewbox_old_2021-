using DbAccess.Attributes;

namespace Viewbox.DcwBalance
{
	[DbTable("_dcw_balance_structure_sign")]
	public class StructureSignRow
	{
		[DbColumn("_row_no_", AutoIncrement = true)]
		[DbPrimaryKey]
		public int RowNo { get; set; }

		[DbColumn("zdmnu", Length = 10)]
		public string RefMandant { get; set; }

		[DbColumn("zdkey", Length = 4)]
		public string Key { get; set; }

		[DbColumn("sign")]
		public int Sign { get; set; }
	}
}
