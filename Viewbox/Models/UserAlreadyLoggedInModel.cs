using System.Collections.Generic;
using Viewbox.Properties;

namespace Viewbox.Models
{
	public class UserAlreadyLoggedInModel : ErrorModel
	{
		public string UserName { get; set; }

		public string Password { get; set; }

		public UserAlreadyLoggedInModel()
		{
			base.Dialog = new DialogModel
			{
				Title = Resources.UserAlreadyInUseTitle,
				Content = string.Format(Resources.UserAlreadyInUse),
				DialogType = DialogModel.Type.Info,
				Buttons = new List<DialogModel.Button>
				{
					new DialogModel.Button
					{
						Caption = Resources.OK
					},
					new DialogModel.Button
					{
						//Caption = Resources.Force
					}
				}
			};
		}
	}
}
