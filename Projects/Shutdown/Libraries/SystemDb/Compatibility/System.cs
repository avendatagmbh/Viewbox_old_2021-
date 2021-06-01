using DbAccess.Attributes;

namespace SystemDb.Compatibility
{
	[DbTable("system")]
	internal class System
	{
		[DbColumn("system_id")]
		[DbPrimaryKey]
		public int Id { get; set; }

		[DbColumn("company_id")]
		[DbIndex("company_id")]
		public int CompanyId { get; set; }

		[DbColumn("name", Length = 256)]
		public string Name { get; set; }

		[DbColumn("description", Length = 4096, AllowDbNull = true)]
		public string Description { get; set; }
	}
}
