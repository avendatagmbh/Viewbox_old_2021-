using System;
using DbAccess.Attributes;

namespace SystemDb.Internal
{
	[DbTable("issue_extensions", ForceInnoDb = true)]
	public class IssueExtension : ICloneable
	{
		[DbColumn("id", AutoIncrement = false)]
		[DbPrimaryKey]
		public int Id { get; set; }

		[DbColumn("ref_id")]
		public int RefId { get; set; }

		[DbColumn("command", AllowDbNull = true, Length = 1000000)]
		public string Command { get; set; }

		[DbColumn("obj_id")]
		public int TableObjectId { get; set; }

		[DbColumn("split_value")]
		public bool UseSplitValue { get; set; }

		[DbColumn("index_value")]
		public bool UseIndexValue { get; set; }

		[DbColumn("sort_value")]
		public bool UseSortValue { get; set; }

		[DbColumn("language_value")]
		public bool UseLanguageValue { get; set; }

		[DbColumn("flag")]
		public int Flag { get; set; }

		[DbColumn("row_no_filter", AllowDbNull = true, Length = 1000000)]
		public string RowNoFilter { get; set; }

		[DbColumn("need_bukrs")]
		public bool NeedBukrs { get; set; }

		[DbColumn("need_gjahr")]
		public bool NeedGJahr { get; set; }

		[DbColumn("checked")]
		public bool Checked { get; set; }

		public object Clone()
		{
			return MemberwiseClone();
		}
	}
}
