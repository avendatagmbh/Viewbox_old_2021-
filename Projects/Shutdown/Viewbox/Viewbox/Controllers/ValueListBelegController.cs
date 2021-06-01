using System.Web.Mvc;
using Viewbox.Models;

namespace Viewbox.Controllers
{
	public class ValueListBelegController : Controller
	{
		public JsonResult Index(int columnId = 0, int tableId = 0, string value = "", int limit = 5)
		{
			ValueListBelegModels model = new ValueListBelegModels(columnId, tableId, value, limit);
			return Json(model.Result, JsonRequestBehavior.AllowGet);
		}
	}
}
