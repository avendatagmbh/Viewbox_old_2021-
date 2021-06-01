using System.Collections.Generic;
using Viewbox.Properties;

namespace Viewbox.Models
{
	public class NoRightsForContentModel : ErrorModel
	{
		public new DialogModel Dialog = new DialogModel
		{
			Title = Resources.NoRightsForContentCaption,
			Content = Resources.NoRightsForContentText,
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
