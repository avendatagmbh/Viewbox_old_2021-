using System;
using DbAccess.Attributes;

namespace SystemDb.Internal
{
	[DbTable("user_table_transactionid_settings")]
	public class UserTableTransactionIdSettings : IUserTableTransactionIdSettings, ICloneable
	{
		[DbColumn("id")]
		[DbPrimaryKey]
		public int Id { get; set; }

		[DbColumn("table_id")]
		public int TableId { get; set; }

		[DbColumn("user_id")]
		public int UserId { get; set; }

		public ITableObject Table { get; set; }

		public IUser User { get; set; }

		[DbColumn("transaction_id")]
		public string TransactionId { get; set; }

		public object Clone()
		{
			return new UserTableTransactionIdSettings
			{
				Id = Id,
				Table = Table,
				TableId = TableId,
				TransactionId = TransactionId,
				UserId = UserId,
				User = User
			};
		}
	}
}
