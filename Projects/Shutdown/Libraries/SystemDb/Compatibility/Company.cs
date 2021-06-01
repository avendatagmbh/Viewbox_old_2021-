using DbAccess.Attributes;

namespace SystemDb.Compatibility
{
	[DbTable("company")]
	internal class Company
	{
		[DbColumn("company_id")]
		[DbPrimaryKey]
		public int Id { get; set; }

		[DbColumn("name", Length = 256)]
		public string Name { get; set; }

		[DbColumn("description", Length = 4096, AllowDbNull = true)]
		public string Description { get; set; }

		[DbColumn("bezeichnung", Length = 1024)]
		public string Comment { get; set; }

		[DbColumn("ort", Length = 1024)]
		public string Location { get; set; }
	}
}
