using DbAccess.Attributes;

namespace SystemDb.Internal
{
	[DbTable("relations", ForceInnoDb = true)]
	public class Relation : IRelationDatabaseObject
	{
		[DbColumn("id")]
		[DbPrimaryKey]
		public int Id { get; set; }

		[DbColumn("relation_id")]
		public int RelationId { get; set; }

		[DbColumn("parent")]
		public int ParentId { get; set; }

		[DbColumn("child")]
		public int ChildId { get; set; }

		[DbColumn("operator")]
		public int Operator { get; set; }

		[DbColumn("full_line")]
		public int FullLine { get; set; }

		[DbColumn("type")]
		public int Type { get; set; }

		[DbColumn("ext_info")]
		public string ExtInfo { get; set; }

		[DbColumn("column_ext_info")]
		public string ColumnExtInfo { get; set; }

		[DbColumn("user_defined", AllowDbNull = false, DefaultValue = "0")]
		public bool UserDefined { get; set; }
	}
}
