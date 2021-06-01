using System;
using System.Linq;
using System.Web.Mvc;
using SystemDb;

namespace Viewbox.Controllers
{
	[Authorize]
	[ViewboxFilter]
	[HandleError(View = "Error")]
	public class HomeController : BaseController
	{
		private IOptimization _firstOptimalisation;

		public RedirectToRouteResult Index()
		{
			if (ViewboxSession.User == null)
			{
				return RedirectToAction("LogOff", "Account");
			}
			ViewboxSession.ClearSavedDocumentSettings("DataGrid");
			if (ViewboxSession.Optimizations == null)
			{
				return RedirectToAction("NoRightForOpt", "Error");
			}
			_firstOptimalisation = ViewboxSession.Optimizations.FirstOrDefault();
			string controller = ViewboxApplication.Database.SystemDb.GetUserController(ViewboxSession.User);
			if (!string.IsNullOrWhiteSpace(controller) && IsEnabledRedirection(controller))
			{
				if (!(controller.ToLower() == "documents"))
				{
					return RedirectToAction("Index", controller);
				}
				if (!ViewboxApplication.HideDocumentsMenu && ViewboxSession.Archives.Count > 0)
				{
					IOptimization opt = ViewboxSession.Optimizations.FirstOrDefault();
					if (opt != null && ViewboxSession.Archives.Any((IArchive a) => a.Database.ToLower() == opt.FindValue(OptimizationType.System).ToLower()))
					{
						return RedirectToAction("Index", controller);
					}
				}
			}
			if (ViewboxSession.GetIssueViewTableCount(TableType.Issue) > 0 && IsEnabledRedirection("IssueList"))
			{
				return RedirectToAction("Index", "IssueList");
			}
			if (ViewboxSession.GetIssueViewTableCount(TableType.View) > 0 && IsEnabledRedirection("ViewList"))
			{
				return RedirectToAction("Index", "ViewList");
			}
			if (IsEnabledRedirection("TableList"))
			{
				return RedirectToAction("Index", "TableList");
			}
			return RedirectToAction("Index", "Settings");
		}

		public ActionResult Test()
		{
			throw new Exception("MUH");
		}

		private bool IsEnabledRedirection(string controller)
		{
			return controller.ToLower() switch
			{
				"issuelist" => _firstOptimalisation != null && ViewboxSession.GetIssueViewTableCount(TableType.Issue) > 0 && !ViewboxSession.HideIssuesButton, 
				"viewlist" => _firstOptimalisation != null && ViewboxSession.GetIssueViewTableCount(TableType.View) > 0 && !ViewboxSession.HideViewsButton, 
				"tablelist" => _firstOptimalisation != null && ViewboxSession.GetIssueViewTableCount(TableType.Table) > 0 && !ViewboxSession.HideTablesButton, 
				"export" => ViewboxSession.User != null && (ViewboxSession.User.AllowedExport || ViewboxSession.User.IsSuper), 
				"archivedocuments" => ViewboxSession.ArchiveDocuments.Count > 0 && _firstOptimalisation != null && ViewboxSession.ArchiveDocuments.Any((IArchiveDocument a) => a.Database.ToLower() == _firstOptimalisation.FindValue(OptimizationType.System).ToLower()), 
				"documents" => ViewboxSession.Archives.Count > 0 && _firstOptimalisation != null && ViewboxSession.Archives.Any((IArchive a) => a.Database.ToLower() == _firstOptimalisation.FindValue(OptimizationType.System).ToLower()), 
				_ => true, 
			};
		}
	}
}
