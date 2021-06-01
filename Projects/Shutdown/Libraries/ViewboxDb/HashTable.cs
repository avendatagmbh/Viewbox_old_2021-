using DbAccess.Attributes;

namespace ViewboxDb
{
	[DbTable("viewbox_hashes")]
	public class HashTable
	{
		[DbColumn("id")]
		[DbPrimaryKey]
		public int Id { get; set; }

		[DbColumn("hashCode")]
		public int HashCode { get; set; }

		[DbColumn("tablename", Length = 32)]
		public string TableName { get; set; }
	}
}
