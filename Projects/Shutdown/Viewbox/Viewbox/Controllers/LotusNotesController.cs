using System.Web.Mvc;
using Viewbox.Models;

namespace Viewbox.Controllers
{
	public class LotusNotesController : TableObjectControllerBase
	{
		public ActionResult Index(int page = 1, int size = 25, string filter = "")
		{
			base.ViewBag.Title = "LotusNotes";
			LotusNotesTableModel model = LotusNotesTableModel.Table;
			model.Page = page;
			model.Size = size;
			model.Filter = filter;
			model.BuildTreeView(page, size, filter);
			return View("Index", model);
		}

		public PartialViewResult EmailDetails(string unid = "")
		{
			return PartialView("_EmailDetailsPartial", new LotusEmailDetails(unid));
		}
	}
}
