using DbAccess.Attributes;

namespace SystemDb.Internal
{
	[DbTable("column_dictionary", ForceInnoDb = true)]
	public class ColumnDictionary
	{
		[DbColumn("id", AutoIncrement = true)]
		[DbPrimaryKey]
		public int Id { get; set; }

		[DbColumn("filter_type")]
		public string FilterType { get; set; }

		[DbColumn("type")]
		public string Type { get; set; }

		[DbColumn("schema")]
		public string Schema { get; set; }

		[DbColumn("table")]
		public string Table { get; set; }

		[DbColumn("column")]
		public string Column { get; set; }

		public override string ToString()
		{
			return "Id: " + Id + ", FilterType: " + FilterType + ", Type: " + Type + ", Schema: " + Schema + ", Table: " + Table + ", Column: " + Column;
		}
	}
}
