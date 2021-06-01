namespace Viewbox.Models
{
	public class RoleSettingsModel : SettingsModel
	{
		public override string Partial => "_RoleSettingsPartial";

		public RoleSettingsModel()
			: base(SettingsType.RoleSettings)
		{
		}
	}
}
