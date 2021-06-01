namespace SystemDb
{
	public interface IUserControllerSettingsCollection
	{
		IUserControllerSettings this[IUser user] { get; }
	}
}
