using System.Collections.Generic;
using System.Web.Mvc;
using Viewbox.Properties;

namespace Viewbox.Models
{
	public class RequiredSelection : SelectionType
	{
		public RequiredSelection(int selected = 0)
		{
			selections.Add(new SelectListItem
			{
				Text = Resources.DisplayEqualsFilter,
				Value = "0"
			});
		}

		public override List<SelectListItem> GetSelection()
		{
			return selections;
		}
	}
}
