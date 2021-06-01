using DbAccess.Attributes;

namespace SystemDb.Internal
{
	[DbTable("extended_column_information", ForceInnoDb = true)]
	public class ExtendedColumnInformation : IExtendedColumnInformation
	{
		[DbColumn("information2")]
		public int InformationColumn2Id { get; set; }

		[DbColumn("reltype")]
		public string RelationType { get; set; }

		[DbColumn("id")]
		[DbPrimaryKey]
		public int Id { get; set; }

		[DbColumn("parent")]
		public int ParentColumnId { get; set; }

		[DbColumn("child")]
		public int ChildColumnId { get; set; }

		[DbColumn("information")]
		public int InformationColumnId { get; set; }
	}
}
