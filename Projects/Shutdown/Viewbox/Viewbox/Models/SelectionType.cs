using System.Collections.Generic;
using System.Web.Mvc;

namespace Viewbox.Models
{
	public abstract class SelectionType
	{
		protected List<SelectListItem> selections = new List<SelectListItem>();

		public abstract List<SelectListItem> GetSelection();
	}
}
