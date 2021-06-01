using DbAccess.Attributes;

namespace SystemDb.Compatibility
{
	[DbTable("user")]
	internal class User
	{
		[DbColumn("user_id")]
		[DbPrimaryKey]
		public int Id { get; set; }

		[DbColumn("username", Length = 32)]
		public string Username { get; set; }

		[DbColumn("forename", Length = 256)]
		public string Forename { get; set; }

		[DbColumn("surename", Length = 256)]
		public string Surname { get; set; }

		[DbColumn("password", Length = 32)]
		public string Password { get; set; }
	}
}
