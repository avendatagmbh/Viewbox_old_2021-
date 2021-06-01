namespace SystemDb
{
	public interface IUserTableTransactionIdSettings
	{
		ITableObject Table { get; }

		IUser User { get; }

		string TransactionId { get; }
	}
}
