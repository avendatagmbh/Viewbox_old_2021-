namespace SystemDb
{
	public interface IUserPropertySettingCollection
	{
		IUserPropertySettings this[IUser user, IProperty property] { get; }
	}
}
