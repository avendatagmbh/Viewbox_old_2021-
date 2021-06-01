using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Web.Mvc;
using SystemDb.Internal;
using Viewbox.Properties;

namespace Viewbox.Models
{
	public class ModifyStartScreenModel : SettingsModel
	{
		public List<SelectListItem> Pics { get; private set; }

		public DialogModel DeleteConfirmDialog { get; private set; }

		public override string Partial => "_ModifyStartScreenPartial";

		public ModifyStartScreenModel()
			: base(SettingsType.ModifyStartScreen)
		{
			UpdatePicsList();
			DeleteConfirmDialog = new DialogModel
			{
				DialogType = DialogModel.Type.Warning,
				Title = Resources.Delete,
				Content = Resources.DeleteStartScreen,
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

		public void UpdatePicsList()
		{
			ViewboxApplication.Database.SystemDb.UpdateStartScreensList();
			Pics = new List<SelectListItem>();
			foreach (StartScreen file in ViewboxApplication.Database.SystemDb.StartScreens)
			{
				Pics.Add(new SelectListItem
				{
					Text = Path.GetFileName(file.Name),
					Value = Path.GetFileName(file.Name),
					Selected = file.IsDefault
				});
			}
		}
	}
}
