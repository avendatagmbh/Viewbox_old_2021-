using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using SystemDb;
using DbAccess.Enums;
using Viewbox.Job;
using Viewbox.Models;
using Viewbox.Properties;
using ViewboxDb;

namespace Viewbox.Controllers
{
	[Authorize]
	[ViewboxFilter]
	public class TableListController : TableObjectControllerBase
	{
		public ActionResult Index(int id = -1, string search = null, bool showEmpty = false, int sortColumn = 0, SortDirection direction = SortDirection.Ascending, bool json = false, int page = 0, int size = 25, bool catRefresh = false, bool fullRefresh = false, bool showHidden = false, bool showArchived = false, bool showEmptyHidden = false, bool isNoResult = false, bool tabChange = false, bool searchPartial = false)
		{
			ViewboxSession.AllowedOpts = null;
			ViewboxSession.IssueOptimizationFilter.Clear();
			if (ViewboxSession.LastPositionUserTableList == null)
			{
				ViewboxSession.LastPositionUserTableList = new Dictionary<string, string>();
			}
			string tab = (showEmpty ? "showEmpty" : (showHidden ? "showHidden" : (showArchived ? "showArchived" : (showEmptyHidden ? "showEmptyHidden" : ""))));
			if (ViewboxSession.LastPositionUserTableList.ContainsKey(ViewboxSession.User.SessionId) && tabChange)
			{
				ViewboxSession.LastPositionUserTableList[ViewboxSession.User.SessionId] = tab;
			}
			if (!ViewboxSession.LastPositionUserTableList.ContainsKey(ViewboxSession.User.SessionId))
			{
				if (tab != "")
				{
					ViewboxSession.LastPositionUserTableList.Add(ViewboxSession.User.SessionId, tab);
				}
			}
			else if (!base.ControllerContext.HttpContext.Request.Params.AllKeys.Contains("showEmpty") && !base.ControllerContext.HttpContext.Request.Params.AllKeys.Contains("showHidden") && !base.ControllerContext.HttpContext.Request.Params.AllKeys.Contains("showArchived") && !base.ControllerContext.HttpContext.Request.Params.AllKeys.Contains("showEmptyHidden"))
			{
				KeyValuePair<string, string> element = ViewboxSession.LastPositionUserTableList.FirstOrDefault((KeyValuePair<string, string> x) => x.Key == ViewboxSession.User.SessionId);
				if (element.Key != null)
				{
					switch (element.Value)
					{
					case "showEmpty":
						showEmpty = true;
						break;
					case "showHidden":
						showHidden = true;
						break;
					case "showArchived":
						showArchived = true;
						break;
					case "showEmptyHidden":
						showEmptyHidden = true;
						break;
					default:
						showEmpty = false;
						showHidden = false;
						showArchived = false;
						showEmptyHidden = false;
						break;
					}
				}
			}
			IOptimization opt = ViewboxSession.Optimizations.FirstOrDefault();
			if (opt == null || ViewboxSession.TableCount == 0 || ViewboxSession.HideTablesButton)
			{
				return RedirectToAction("Index", "Home");
			}
			ViewboxSession.ClearSavedDocumentSettings("TableList");
			if (search == null && ViewboxSession.TableSortAndFilterSettings != null)
			{
				search = ViewboxSession.TableSortAndFilterSettings.Filter;
			}
			if (sortColumn == 0 && ViewboxSession.TableSortAndFilterSettings != null)
			{
				sortColumn = ViewboxSession.TableSortAndFilterSettings.SortColumn;
				direction = ViewboxSession.TableSortAndFilterSettings.Sort;
			}
			if (isNoResult)
			{
				page = 0;
			}
			TableList model = new TableList(id, search, showEmpty || showHidden || showArchived || showEmptyHidden);
			model.CurrentPage = page;
			model.PerPage = size;
			ViewboxSession.TableObjectSetup(TableType.Table, page, size, search, showEmpty, showHidden, showArchived, sortColumn, direction != SortDirection.Descending, out var fullTableListCount, showEmptyHidden);
			model.TableCount = ViewboxSession.GetTableObjectCount(TableType.Table, search);
			model.SortColumn = sortColumn;
			model.Direction = direction;
			model.ShowEmpty = showEmpty;
			model.ShowHidden = showHidden;
			model.ShowArchived = showArchived;
			model.ShowEmptyHidden = showEmptyHidden;
			if (!catRefresh)
			{
				ViewboxSession.CheckedTableItems.Clear();
			}
			model.Tables = model.GetListFromSessionTables(sortColumn, direction, showEmpty);
			model.Count = fullTableListCount;
			IEnumerable<ITableObject> enumerable;
			if (ViewboxSession.Optimizations.Any())
			{
				enumerable = GetTablesFromQuery("", TableType.Table);
			}
			else
			{
				IEnumerable<ITableObject> enumerable2 = new List<ITableObject>();
				enumerable = enumerable2;
			}
			IEnumerable<ITableObject> tempobjs2 = enumerable;
			if (tempobjs2 != null)
			{
				int page2 = 0;
				int size2 = 25;
				int count2 = tempobjs2.Count();
				List<ITableObject> currPageObjs2 = tempobjs2.Skip(page2 * size2).Take(size2).ToList();
				model.ArchiveListOptions = new ListOptionsModel
				{
					Type = TableType.Table,
					ShowEmpty = false,
					ShowHidden = false,
					ShowArchived = false,
					ShowEmptyHidden = false,
					Page = page2,
					Size = size2,
					Count = count2,
					TableObjects = currPageObjs2,
					Search = "",
					SortColumn = 1,
					Direction = SortDirection.Ascending
				};
			}
			if (search != null || sortColumn != 0)
			{
				ViewboxSession.TableSortAndFilterSettings = new SortAndFilterSettings();
				if (!string.IsNullOrEmpty(search))
				{
					ViewboxSession.TableSortAndFilterSettings.Filter = search;
				}
				if (sortColumn != 0)
				{
					ViewboxSession.TableSortAndFilterSettings.SortColumn = sortColumn;
				}
				ViewboxSession.TableSortAndFilterSettings.Sort = direction;
			}
			if (fullRefresh)
			{
				string url = base.Request.UrlReferrer.AbsolutePath;
				return Redirect(url);
			}
			if (catRefresh)
			{
				return PartialView("_TableObjectsPartial", model);
			}
			if (ViewboxApplication.Database.SystemDb.Issues.Any((IIssue i) => i.Database.ToLower() == opt.FindValue(OptimizationType.System).ToLower()) && !ViewboxSession.HideIssuesButton && (opt == null || ViewboxSession.TableCount <= 0 || ViewboxSession.HideTablesButton))
			{
				return Redirect("/IssueList");
			}
			if (opt != null && ViewboxApplication.Database.SystemDb.Views.Any((SystemDb.IView v) => v.Database.ToLower() == opt.FindValue(OptimizationType.System).ToLower()) && !ViewboxSession.HideViewsButton && (opt == null || ViewboxSession.TableCount <= 0 || ViewboxSession.HideTablesButton))
			{
				return Redirect("/ViewList");
			}
			if (!ViewboxApplication.Database.SystemDb.Issues.Any((IIssue i) => i.Database.ToLower() == opt.FindValue(OptimizationType.System).ToLower()) && (opt == null || ViewboxSession.TableCount <= 0 || ViewboxSession.HideTablesButton) && (opt == null || !ViewboxApplication.Database.SystemDb.Views.Any((SystemDb.IView v) => v.Database.ToLower() == opt.FindValue(OptimizationType.System).ToLower()) || ViewboxSession.HideViewsButton))
			{
				return Redirect("/Settings");
			}
			if (searchPartial)
			{
				return PartialView("_SearchPartial", model);
			}
			return ResultView(model, json);
		}

		public PartialViewResult CategoryTabs(int id = -1, string search = null, bool showEmpty = false, int sortColumn = 0, SortDirection direction = SortDirection.Ascending, int page = 0, int size = 25, bool showHidden = false, bool showEmptyHidden = false)
		{
			TableList model = new TableList(id, search, showEmpty || showHidden);
			model.CurrentPage = page;
			model.PerPage = size;
			List<ITable> list = model.GetTableList(showEmpty, showHidden, sortColumn, direction);
			model.SortColumn = sortColumn;
			model.Direction = direction;
			model.ShowEmpty = showEmpty;
			model.ShowHidden = showHidden;
			model.ShowEmptyHidden = showEmptyHidden;
			model.Count = list.Count;
			return PartialView("_CategoryTabsPartial", model);
		}

		public ActionResult Sort(int id = -1, string search = null, bool showEmpty = false, int sortColumn = 0, SortDirection direction = SortDirection.Ascending, bool json = false, int page = 0, int size = 25, bool catRefresh = false, bool fullRefresh = false, bool showHidden = false, bool showArchived = false, bool showEmptyHidden = false)
		{
			page = 0;
			if (ViewboxSession.TableSortAndFilterSettings != null && ViewboxSession.TableSortAndFilterSettings.SortColumn == sortColumn && ViewboxSession.TableSortAndFilterSettings.Sort == direction)
			{
				ViewboxSession.TableSortAndFilterSettings.SortColumn = (sortColumn = 0);
			}
			return Index(id, search, showEmpty, sortColumn, direction, json, page, size, catRefresh, fullRefresh, showHidden, showArchived, showEmptyHidden);
		}

		public ActionResult Archive(int id, ArchiveType archive)
		{
			ITableObject tableObject = ViewboxSession.TableObjects[id];
			if (tableObject.IsUnderArchiving)
			{
				return PartialView("_DialogPartial", new DialogModel
				{
					DialogType = DialogModel.Type.Warning,
					Title = Resources.TableBeingProcessed,
					Content = string.Format(Resources.TableIsUnderPrecessing, ViewboxSession.TableObjects[id].TableName),
					Buttons = new List<DialogModel.Button>
					{
						new DialogModel.Button
						{
							Caption = Resources.OK
						}
					}
				});
			}
			tableObject.IsUnderArchiving = true;
			string title = string.Empty;
			title = ((archive != 0) ? $"{Resources.RestoreTable} : {ViewboxSession.TableObjects[id].TableName}" : $"{Resources.ArchiveTable} : {ViewboxSession.TableObjects[id].TableName}");
			long rowCount = 0L;
			rowCount = tableObject.GetRowCount();
			Transformation trans = Transformation.Create(tableObject, archive, rowCount);
			if (rowCount < 50000)
			{
				return PartialView("_DialogPartial", new DialogModel
				{
					DialogType = DialogModel.Type.Info,
					Title = title,
					Key = trans.Key,
					Content = string.Format(Resources.ShortRunningDialogText, title)
				});
			}
			if (rowCount > 10000000)
			{
				return PartialView("_DialogPartial", new DialogModel
				{
					DialogType = DialogModel.Type.Info,
					Title = title,
					Key = trans.Key,
					Content = string.Format(Resources.VeryLongRunningDialogText, title),
					Buttons = new List<DialogModel.Button>
					{
						new DialogModel.Button
						{
							Caption = Resources.LongRunningDialogCaption
						}
					}
				});
			}
			return PartialView("_DialogPartial", new DialogModel
			{
				DialogType = DialogModel.Type.Info,
				Title = title,
				Key = trans.Key,
				Content = string.Format(Resources.LongRunningDialogText, title),
				Buttons = new List<DialogModel.Button>
				{
					new DialogModel.Button
					{
						Caption = Resources.LongRunningDialogCaption
					}
				}
			});
		}

		public ActionResult ArchiveSelected(TableType type = TableType.All, string archive = "")
		{
			return DoArchiveSelected(type, archive);
		}

		public void Select(int id = -1, bool chk = false)
		{
			DoSelect(id, chk);
		}

		public void SelectNone(int id = -1, string search = null, int sortColumn = 0, SortDirection direction = SortDirection.Ascending, bool json = false, int page = 0, int size = 25, bool catRefresh = false, bool showEmpty = false, bool showHidden = false, bool showArchived = false)
		{
			DoSelectNone(id, search, sortColumn, direction, json, page, size, catRefresh, showEmpty, showHidden, showArchived);
		}

		public PartialViewResult GetTableObjects(string search = null, string type = "", bool showEmpty = false, bool showHidden = false, bool showArchived = false, int page = 0, int size = 25, string doselect = "", int sortColumn = 1, SortDirection direction = SortDirection.Ascending)
		{
			return GetTableObjectsBase(search, type, showEmpty, showHidden, showArchived, page, size, doselect, sortColumn, direction);
		}

		public PartialViewResult GetConfirmationDialog(string archive)
		{
			if (string.IsNullOrEmpty(archive))
			{
				throw new ArgumentNullException("archive");
			}
			ArchiveType _archive = (ArchiveType)Enum.Parse(typeof(ArchiveType), archive);
			string title = ((_archive == ArchiveType.Restore) ? Resources.ExecuteRestoringTitle : Resources.ExecuteArchiveTitle);
			return PartialView("_DialogPartial", new DialogModel
			{
				DialogType = DialogModel.Type.Info,
				Title = title,
				Content = string.Format(Resources.Proceed, title),
				Buttons = new List<DialogModel.Button>
				{
					new DialogModel.Button
					{
						Caption = Resources.Yes,
						Data = true.ToString()
					},
					new DialogModel.Button
					{
						Caption = Resources.No,
						Data = false.ToString()
					}
				}
			});
		}

		public ActionResult GetInnoDBWarningDialog()
		{
			return PartialView("_DialogPartial", new DialogModel
			{
				DialogType = DialogModel.Type.Warning,
				Title = Resources.ArchivingInnoDBCaption,
				Content = Resources.ArchivingInnoDBText,
				Buttons = new List<DialogModel.Button>
				{
					new DialogModel.Button
					{
						Caption = Resources.Yes,
						Data = true.ToString()
					},
					new DialogModel.Button
					{
						Caption = Resources.No,
						Data = false.ToString()
					}
				}
			});
		}

		public JsonResult CheckEngineType(int id)
		{
			ITableObject tableObject = ViewboxSession.TableObjects[id];
			if (tableObject.EngineType == EngineTypes.Undefined)
			{
				ViewboxApplication.Database.SystemDb.SetTableEngineForTableObject(tableObject);
			}
			bool isInnoDB = tableObject.EngineType == EngineTypes.InnoDB;
			string url = (isInnoDB ? string.Empty : base.Url.Action("GetInnoDBWarningDialog"));
			return new JsonResult
			{
				Data = new { isInnoDB, url },
				JsonRequestBehavior = JsonRequestBehavior.AllowGet
			};
		}

		public ActionResult TryArchive()
		{
			return PartialView("_DialogPartial", new DialogModel
			{
				DialogType = DialogModel.Type.Info,
				Title = Resources.ExecuteArchiveTitle,
				Content = string.Format(Resources.ToDearchiveContactAnAdmin),
				Buttons = new List<DialogModel.Button>
				{
					new DialogModel.Button
					{
						Caption = Resources.OK
					}
				}
			});
		}
	}
}
