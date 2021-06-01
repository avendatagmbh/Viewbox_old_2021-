using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Viewbox.Models;
using ViewboxDb;

namespace Viewbox.Controllers
{
	[Authorize]
	public class ValueListController : TableObjectControllerBase
	{
		public ActionResult Index(int parameterId, int columnId, int id = -1, string search = null, bool exactMatch = false, bool showEmpty = false, int sortColumn = 0, SortDirection direction = SortDirection.Ascending, bool json = false, int page = 0, string[] selectedParameters = null)
		{
			ValueListModels model = new ValueListModels(parameterId, columnId, ViewboxSession.Optimizations.LastOrDefault(), ViewboxSession.User.Id, id, search, exactMatch, showEmpty, sortColumn, direction, json, page, ViewboxApplication.WertehilfeSize, ViewboxApplication.IsEnabledPreviewOnWhiteBooks, selectedParameters);
			return PartialView("_ValueListPartial", model);
		}

		public ActionResult ExcelValues(int id, int columnId, string search = null, bool exactMatch = false, bool showEmpty = false, int sortColumn = 0, SortDirection direction = SortDirection.Ascending, bool json = false, int page = 0, int size = 25)
		{
			List<string> listOfValues = ViewboxApplication.Database.SystemDb.ReadDistinctData(columnId);
			return PartialView("_ExcelLikeFilterContent", listOfValues);
		}
	}
}
