using DbAccess.Attributes;

namespace SystemDb.Internal
{
	[DbTable("about")]
	public class About
	{
		[DbColumn("id")]
		[DbPrimaryKey]
		public int Id { get; set; }

		[DbColumn("company_name")]
		public string CompanyName { get; set; }

		[DbColumn("version")]
		public string Version { get; set; }
	}
}
