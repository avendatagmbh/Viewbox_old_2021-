using DbAccess.Attributes;
using Utils;

namespace IndexDb.Structures
{
	[DbTable("info")]
	internal class Info : NotifyPropertyChangedBase
	{
		[DbColumn("id", AllowDbNull = false)]
		[DbPrimaryKey]
		public int Id { get; set; }

		[DbColumn("key", AllowDbNull = false)]
		[DbUniqueKey]
		public string Key { get; set; }

		[DbColumn("value", AllowDbNull = true)]
		public string Value { get; set; }
	}
}
