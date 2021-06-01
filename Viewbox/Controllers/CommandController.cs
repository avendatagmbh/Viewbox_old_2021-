using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web.Mvc;
using SystemDb;
using Viewbox.Command;
using Viewbox.Job;
using Viewbox.Models;
using ViewboxDb;

namespace Viewbox.Controllers
{
	public class CommandController : BaseController
	{
		public ActionResult GetUpdates(int maxEntriesLatest = 5, int maxEntriesHistory = 5)
		{
			if (ViewboxSession.IsAuthenticated && ViewboxSession.IsInitialized)
			{
				NotificationModel model = new NotificationModel();
				model.MaxEntriesHistory = maxEntriesHistory;
				model.MaxEntriesLatest = maxEntriesLatest;
				IEnumerable<Base> updates = ViewboxSession.GetFinishedJobs();
				foreach (Base job in updates)
				{
					IJobStatusHelper jobStatusHelper = JobStatusHelperFactory.CreateJobStatusHelper(job, this);
					switch (job.Status)
					{
					case Base.JobStatus.Finished:
						jobStatusHelper.InitializeNotificationModelOnFinish(model);
						break;
					case Base.JobStatus.Crashed:
						jobStatusHelper.InitializeNotificationModelOnCrashed(model);
						break;
					}
				}
				return PartialView("_NotificationPartial", model);
			}
			return null;
		}

		[Authorize]
		public void RemoveJob(string key)
		{
			ViewboxSession.RemoveJob(key);
		}

		public ActionResult Index()
		{
			while (!ViewboxApplication.Initialized)
			{
				Thread.Sleep(1000);
			}
			string result = string.Format("Server started: {1:F}{0}Server init finished: {2:F}{0}Server init time: {3:T}", Environment.NewLine, ViewboxApplication.ServerStart, ViewboxApplication.ServerInitializedTime, ViewboxApplication.InitTime);
			return Content(result, "text/plain");
		}

		public PartialViewResult SearchOptimization(string search, int optGroupId, string returnUrl)
		{
			IOptimization selectedOpt = ViewboxSession.Optimizations.FirstOrDefault((IOptimization o) => o.Group.Id == optGroupId);
			if (search == null || search == string.Empty)
			{
				OptimizationModel model2 = new OptimizationModel
				{
					SearchPhrase = search,
					SelectedOptimization = selectedOpt,
					Optimizations = ViewboxSession.AllowedOptimizations.Where((IOptimization o) => o.Group.Id == optGroupId && o.Parent.Id == selectedOpt.Parent.Id).ToList(),
					ReturnUrl = returnUrl
				};
				return PartialView("_OptimizationPartial", model2);
			}
			search = search.ToLower();
			List<IOptimization> searchedOpts = new List<IOptimization>();
			foreach (IOptimization opt in ViewboxSession.AllowedOptimizations.Where((IOptimization o) => o.Group.Id == optGroupId && o.Parent.Id == selectedOpt.Parent.Id))
			{
				string value = opt.GetValue().ToLower();
				string description = opt.GetDescription().ToLower();
				if (value.Contains(search) || description.Contains(search))
				{
					searchedOpts.Add(opt);
				}
			}
			OptimizationModel model = new OptimizationModel
			{
				SearchPhrase = search,
				SelectedOptimization = selectedOpt,
				Optimizations = searchedOpts,
				ReturnUrl = returnUrl
			};
			return PartialView("_OptimizationPartial", model);
		}

		public PartialViewResult RefreshBoxHeader(string returnUrl)
		{
			return PartialView("_BoxHeaderPartial", new BoxHeaderModel
			{
				ReturnUrl = returnUrl
			});
		}

		public ActionResult RefreshTableObjects(string search = null, bool showEmpty = true, int sortColumn = 0, SortDirection direction = SortDirection.Ascending, bool json = false, int page = 0, int size = 25, TableType type = TableType.Issue, bool catRefresh = true)
		{
			return type switch
			{
				TableType.Issue => RedirectToAction("Index", "IssueList", new { search, sortColumn, direction, json, page, size, catRefresh }), 
				TableType.View => RedirectToAction("Index", "ViewList", new { search, showEmpty, sortColumn, direction, json, page, size, catRefresh }), 
				TableType.Table => RedirectToAction("Index", "TableList", new { search, showEmpty, sortColumn, direction, json, page, size, catRefresh }), 
				_ => null, 
			};
		}

		public PartialViewResult RefreshViewOptions(int id)
		{
			return PartialView("_ViewOptionsPartial", (id < 0) ? ViewboxSession.TempTableObjects[id].Table : ViewboxSession.TableObjects[id]);
		}

		public PartialViewResult RefreshExtendedSort(int id)
		{
			ITableObject tobj = ((id < 0) ? ViewboxSession.TempTableObjects[id].Table : ViewboxSession.TableObjects[id]);
			return PartialView("_ExtendedSortPartial", tobj);
		}

		public PartialViewResult RefreshExtendedSum(int id)
		{
			ITableObject tobj = ((id < 0) ? ViewboxSession.TempTableObjects[id].Table : ViewboxSession.TableObjects[id]);
			return PartialView("_SummaPartial", tobj);
		}

		public PartialViewResult RefreshExtendedFilter(int id)
		{
			ITableObject tobj = ((id < 0) ? ViewboxSession.TempTableObjects[id].Table : ViewboxSession.TableObjects[id]);
			return PartialView("_FilterPartial", tobj);
		}

		public ActionResult GetNoteText(int id)
		{
			if (ViewboxApplication.Notes[ViewboxSession.User] != null)
			{
				INote note = ViewboxApplication.Notes[ViewboxSession.User].FirstOrDefault((INote n) => n.Id == id);
				return Content((note != null) ? note.Text : "", "text/plain");
			}
			return Content("", "text/plain");
		}

		public ActionResult GetNoteTitle(int id)
		{
			if (ViewboxApplication.Notes[ViewboxSession.User] != null)
			{
				INote note = ViewboxApplication.Notes[ViewboxSession.User].FirstOrDefault((INote n) => n.Id == id);
				return Content((note != null) ? note.Title : "", "text/plain");
			}
			return Content("", "text/plain");
		}

		[HttpPost]
		[ValidateInput(false)]
		public ActionResult SaveNote(int id, string text, string title)
		{
			INote note = ViewboxApplication.SaveNote(id, text, title);
			return Content(note.Id.ToString(), "text/plain");
		}

		public ActionResult UpdateTable(int id, bool json, string search, bool showHidden, bool showEmpty, bool catRefresh, string url, bool showEmptyHidden = false, int sortColumn = 0, SortDirection direction = SortDirection.Ascending, int page = 0, int size = 25)
		{
			ViewboxSession.TableObjects[id].IsVisible = showHidden || showEmptyHidden;
			foreach (IUser user in ViewboxApplication.Database.SystemDb.Users)
			{
				ViewboxApplication.Database.SystemDb.UpdateTableObject(user, ViewboxSession.TableObjects[id]);
			}
			ViewboxSession.SessionMarker.MarkAll();
			return RedirectToAction("Index", url, new { json, search, showHidden, showEmpty, showEmptyHidden, catRefresh, sortColumn, direction, page, size });
		}
	}
}
