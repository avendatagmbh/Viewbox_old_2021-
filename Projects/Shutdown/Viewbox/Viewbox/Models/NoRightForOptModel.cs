using Viewbox.Properties;

namespace Viewbox.Models
{
	public class NoRightForOptModel : ErrorModel
	{
		public new DialogModel Dialog = new DialogModel
		{
			Title = Resources.UserAlreadyInUseTitle,
			Content = Resources.NoRightsForContentText,
			DialogType = DialogModel.Type.Warning
		};
	}
}
