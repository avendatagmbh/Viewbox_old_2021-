using DbAccess.Attributes;

namespace Viewbox.DcwBalance
{
	[DbTable("_dcw_balance_structure")]
	public class StructureRow
	{
		[DbColumn("_row_no_", AutoIncrement = true)]
		[DbPrimaryKey]
		public int RowNo { get; set; }

		[DbColumn("zdmnu", Length = 10)]
		public string RefMandant { get; set; }

		[DbColumn("zdkey", Length = 4)]
		public string Key { get; set; }

		[DbColumn("zdparent", Length = 4)]
		public string Parent { get; set; }

		[DbColumn("zddsc")]
		public string Description { get; set; }

		[DbColumn("zdlevel", Length = 20)]
		public int Level { get; set; }

		[DbColumn("zdnumbering", Length = 20)]
		public int Numbering { get; set; }

		[DbColumn("bzuohfrom", Length = 4)]
		public string BzuohFrom { get; set; }

		[DbColumn("bzuohto", Length = 4)]
		public string BzuohTo { get; set; }

		[DbColumn("sign")]
		public int Sign { get; set; }

		[DbColumn("version")]
		public int Version { get; set; }

		[DbColumn("bilpos1")]
		public string BilPos1 { get; set; }

		[DbColumn("bilpos1_key")]
		public string BilPos1Key { get; set; }

		[DbColumn("bilpos2")]
		public string BilPos2 { get; set; }

		[DbColumn("bilpos2_key")]
		public string BilPos2Key { get; set; }

		[DbColumn("bilpos3")]
		public string BilPos3 { get; set; }

		[DbColumn("bilpos3_key")]
		public string BilPos3Key { get; set; }
	}
}
