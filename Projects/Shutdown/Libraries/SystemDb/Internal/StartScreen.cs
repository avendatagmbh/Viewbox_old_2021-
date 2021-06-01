using DbAccess.Attributes;

namespace SystemDb.Internal
{
	[DbTable("startscreens")]
	public class StartScreen : IStartScreen
	{
		[DbColumn("id", AutoIncrement = true)]
		[DbPrimaryKey]
		public int Id { get; set; }

		[DbColumn("name")]
		public string Name { get; set; }

		[DbColumn("value")]
		public string ImgBase64 { get; set; }

		[DbColumn("default")]
		public bool IsDefault { get; set; }
	}
}
