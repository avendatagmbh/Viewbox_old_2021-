using System.Linq;
using System.Web.Mvc;
using SystemDb.Internal;
using Viewbox.Models.Wertehilfe;

namespace Viewbox.Controllers
{
	[Authorize]
	[ViewboxFilter]
	public class WertehilfeController : Controller
	{
		public PartialViewResult Index(int parameterId, string search = "", bool isExact = false, int start = 1, string[] sortColumns = null, string[] directions = null, string[] currentValues = null)
		{
			IWertehilfe model = (ViewboxApplication.DynamicWertehilfe ? WertehilfeFactory.Create(parameterId, search, isExact, onlyCheck: false, sortColumns, directions, WertehilfeType.DynamicWertehilfe, currentValues) : WertehilfeFactory.Create(parameterId, search, isExact, onlyCheck: false, sortColumns, directions));
			model.Start = start;
			model.RowsPerPage = ViewboxApplication.WertehilfeSize;
			model.MaxRowCount = ViewboxApplication.WertehilfeLimit;
			model.Language = LanguageKeyTransformer.Transformer(ViewboxSession.User.CurrentLanguage);
			model.Optimization = ViewboxSession.Optimizations.LastOrDefault();
			model.BuildValuesCollection();
			return PartialView("_WertehilfePartial", model);
		}
	}
}
