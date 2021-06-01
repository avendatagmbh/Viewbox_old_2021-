using System.Collections.Generic;
using Viewbox.Properties;

namespace Viewbox.Models
{
	public class MaintenanceModel : ErrorModel
	{
		public new DialogModel Dialog = new DialogModel
		{
			Title = Resources.MaintenanceModelCaption,
			Content = Resources.MaintenanceModelText,
			DialogType = DialogModel.Type.Warning,
			Buttons = new List<DialogModel.Button>
			{
				new DialogModel.Button
				{
					Caption = Resources.TryAgain
				}
			}
		};
	}
}
