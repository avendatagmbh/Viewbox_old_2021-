using System.Collections.Generic;
using SystemDb;

namespace Viewbox.Models
{
	public class UserSettingsModel : SettingsModel
	{
		public bool RightsMode => ViewboxSession.RightsMode;

		public IUser RightsModeUser => (ViewboxSession.RightsModeCredential.Type == CredentialType.User) ? ViewboxApplication.Users[ViewboxSession.RightsModeCredential.Id] : null;

		public IEnumerable<IProperty> Properties { get; set; }

		public override string Partial => "_UserPartial";

		public UserSettingsModel()
			: base(SettingsType.User)
		{
			Properties = new List<IProperty>();
		}
	}
}
