using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using SystemDb;
using Viewbox.Properties;

namespace Viewbox.Models
{
	public class RightSettingsModel : SettingsModel
	{
		public bool RightsMode => base.Header.RightsMode;

		public new CredentialType Type { get; internal set; }

		public IEnumerable<IUser> Users => ViewboxApplication.Users.Where((IUser user) => ViewboxSession.User.CanGrant(user));

		public IEnumerable<IRole> Roles => ViewboxApplication.Roles.Where((IRole role) => ViewboxSession.User.CanGrant(role));

		public override string Partial => "_RightsPartial";

		public RightSettingsModel()
			: base(SettingsType.Rights)
		{
		}

		public DialogModel GetWaitDialog(string action)
		{
			return new DialogModel
			{
				Title = Resources.PleaseWait,
				Content = string.Format(Resources.ExecutingAction, action),
				DialogType = DialogModel.Type.Info
			};
		}

		public DialogModel GetConfirmDialog(string message)
		{
			return new DialogModel
			{
				DialogType = DialogModel.Type.Info,
				Title = message,
				Content = string.Format(Resources.Proceed, message),
				Buttons = new List<DialogModel.Button>
				{
					new DialogModel.Button
					{
						Caption = Resources.Yes,
						Data = true.ToString(CultureInfo.InvariantCulture)
					},
					new DialogModel.Button
					{
						Caption = Resources.No,
						Data = false.ToString(CultureInfo.InvariantCulture)
					}
				}
			};
		}
	}
}
