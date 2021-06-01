using DbAccess.Attributes;

namespace SystemDb.Internal
{
	[DbTable("tables", ForceInnoDb = true)]
	internal class FakeTable : TableObject
	{
		private string OriginalName { get; set; }

		public FakeTable()
			: base(TableType.FakeTable)
		{
		}

		public override object Clone()
		{
			TableObject clone = new FakeTable();
			Clone(ref clone);
			(clone as FakeTable).OriginalName = OriginalName;
			return clone;
		}
	}
}
