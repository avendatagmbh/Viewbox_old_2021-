using System.Collections.Generic;
using Viewbox.Properties;

namespace Viewbox.Models
{
	public class ForcedToQuitModel : ErrorModel
	{
		public ForcedToQuitModel()
		{
			base.Dialog = new DialogModel
			{
				Title = Resources.ForcedToQuitTitle,
				Content = string.Format(Resources.ForcedToQuit),
				DialogType = DialogModel.Type.Info,
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
}
