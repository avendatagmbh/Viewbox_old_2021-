using DbAccess.Attributes;

namespace SystemDb.Internal
{
	[DbTable("tables", ForceInnoDb = true)]
	internal class FakeProcedure : TableObject
	{
		private string OriginalName { get; set; }

		public FakeProcedure()
			: base(TableType.FakeProcedure)
		{
		}

		public override object Clone()
		{
			TableObject clone = new FakeProcedure();
			Clone(ref clone);
			(clone as FakeProcedure).OriginalName = OriginalName;
			return clone;
		}
	}
}
