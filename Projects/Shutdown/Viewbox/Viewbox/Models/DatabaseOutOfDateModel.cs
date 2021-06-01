using System.Collections.Generic;
using SystemDb.Upgrader;
using Viewbox.Properties;

namespace Viewbox.Models
{
	public class DatabaseOutOfDateModel : ErrorModel
	{
		public DatabaseOutOfDateModel(DatabaseBaseOutOfDateInformation info)
		{
			base.Dialog = new DialogModel
			{
				Title = "Datenbank ist nicht mehr aktuell",
				Content = string.Format("Die installierte Datenbank Version ist {0}, aktuell ist Version {1}. Wollen Sie die Datenbank upgraden?", (info == null) ? "0" : info.InstalledDbVersion, (info == null) ? "0" : info.CurrentDbVersion),
				DialogType = DialogModel.Type.Warning,
				Buttons = new List<DialogModel.Button>
				{
					new DialogModel.Button
					{
						Caption = Resources.Yes
					}
				}
			};
		}
	}
}
