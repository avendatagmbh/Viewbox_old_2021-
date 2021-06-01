using DbAccess.Attributes;

namespace SystemDb.Internal
{
	[DbTable("table_transactions", ForceInnoDb = true)]
	public class TableTransactions
	{
		[DbColumn("id", AutoIncrement = true)]
		[DbPrimaryKey]
		public int Id { get; set; }

		[DbColumn("table_id")]
		[DbUniqueKey]
		public int TableId { get; set; }

		[DbColumn("transaction_number")]
		public string TransactionNumber { get; set; }
	}
}
