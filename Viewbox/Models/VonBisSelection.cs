using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Viewbox.Properties;

namespace Viewbox.Models
{
	public class VonBisSelection : SelectionType
	{
		public VonBisSelection(int selected = 0)
		{
			selections.Add(new SelectListItem
			{
				Text = Resources.DisplayEqualsFilter,
				Value = "0"
			});
			selections.Add(new SelectListItem
			{
				Text = Resources.DisplayNotEqualsFilter,
				Value = "1"
			});
			if (selected < 2)
			{
				SelectListItem selectedItem = selections.SingleOrDefault((SelectListItem s) => s.Value == selected.ToString());
				selectedItem.Selected = true;
			}
		}

		public override List<SelectListItem> GetSelection()
		{
			return selections;
		}
	}
}
