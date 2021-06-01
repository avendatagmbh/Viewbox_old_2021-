using System.Web.Mvc;
using Viewbox.Models;

namespace Viewbox.Controllers
{
	[Authorize]
	[ViewboxFilter]
	public class DocumentationController : BaseController
	{
		public ActionResult Index()
		{
			DocumentationContent model = new DocumentationContent();
			return View(model);
		}
	}
}
