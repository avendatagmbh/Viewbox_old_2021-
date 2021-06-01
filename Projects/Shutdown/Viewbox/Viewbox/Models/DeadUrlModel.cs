using System.Collections.Generic;
using Viewbox.Properties;

namespace Viewbox.Models
{
	public class DeadUrlModel : ErrorModel
	{
		public new DialogModel Dialog = new DialogModel
		{
			Title = Resources.DeadUrlCaption,
			Content = Resources.DeadUrlText,
			DialogType = DialogModel.Type.Warning,
			Buttons = new List<DialogModel.Button>
			{
				new DialogModel.Button
				{
					Caption = Resources.OK
				}
			}
		};
	}
}
