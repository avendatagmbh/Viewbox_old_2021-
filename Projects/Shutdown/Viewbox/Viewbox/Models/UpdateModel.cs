using Viewbox.Properties;

namespace Viewbox.Models
{
	public class UpdateModel : ErrorModel
	{
		public new DialogModel Dialog = new DialogModel
		{
			Title = Resources.UpdateModelCaption,
			Content = Resources.UpdateModelText,
			DialogType = DialogModel.Type.Warning
		};
	}
}
