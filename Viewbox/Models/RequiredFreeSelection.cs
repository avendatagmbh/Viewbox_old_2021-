using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Viewbox.Properties;

namespace Viewbox.Models
{
	public class RequiredFreeSelection : RequiredSelection
	{
		public RequiredFreeSelection(int selected = 0)
			: base(selected)
		{
			selections.Add(new SelectListItem
			{
				Text = Resources.DisplayNotEqualsFilter,
				Value = "1"
			});
			selections.Add(new SelectListItem
			{
				Text = Resources.DisplayContainsFilter,
				Value = "2"
			});
			selections.Add(new SelectListItem
			{
				Text = Resources.DisplayNotContainsFilter,
				Value = "3"
			});
			SelectListItem selectedItem = selections.SingleOrDefault((SelectListItem s) => s.Value == selected.ToString());
			selectedItem.Selected = true;
		}

		public override List<SelectListItem> GetSelection()
		{
			return selections;
		}
	}
}
