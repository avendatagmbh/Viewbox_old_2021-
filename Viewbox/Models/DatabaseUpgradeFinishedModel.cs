using System.Collections.Generic;
using Viewbox.Properties;

namespace Viewbox.Models
{
	public class DatabaseUpgradeFinishedModel : ErrorModel
	{
		public bool ErrorOccured { get; private set; }

		public DatabaseUpgradeFinishedModel(string errorMessage)
		{
			ErrorOccured = !string.IsNullOrEmpty(errorMessage);
			if (string.IsNullOrEmpty(errorMessage))
			{
				base.Dialog = new DialogModel
				{
					Title = "Datenbankupgrade erfolgreich",
					Content = "Die Datenbank wurde erfolgreich upgegradet.",
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
			else
			{
				base.Dialog = new DialogModel
				{
					Title = "Datenbankupgrade fehlgeschlagen",
					Content = $"Es ist ein Fehler aufgetreten beim Datenbankupgrade: {errorMessage}",
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
	}
}
