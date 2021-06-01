using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using SystemDb;
using SystemDb.Internal;
using Utils;
using Viewbox.Enums;
using Viewbox.Job;
using Viewbox.Models;
using Viewbox.Properties;
using ViewboxDb;

namespace Viewbox.Controllers
{
	[Authorize]
	[ViewboxFilter]
	public class ViewListController : TableObjectControllerBase
	{
		public ActionResult Index(int id = -1, string search = null, bool showEmpty = false, int sortColumn = 0, SortDirection direction = SortDirection.Ascending, bool json = false, int page = 0, int size = 25, bool catRefresh = false, bool fullRefresh = false, bool showHidden = false, bool showArchived = false, int? objectTypeFilter = null, bool showEmptyHidden = false, bool isNoResult = false, bool tabChange = false, bool searchPartial = false, OperationEnum operation = OperationEnum.Sort)
		{
			if (ViewboxSession.LastPositionUserViewList == null)
			{
				ViewboxSession.LastPositionUserViewList = new Dictionary<string, string>();
			}
			string tab = (showEmpty ? "showEmpty" : (showHidden ? "showHidden" : (showArchived ? "showArchived" : (showEmptyHidden ? "showEmptyHidden" : ""))));
			if (ViewboxSession.LastPositionUserViewList.ContainsKey(ViewboxSession.User.SessionId) && tabChange)
			{
				ViewboxSession.LastPositionUserViewList[ViewboxSession.User.SessionId] = tab;
			}
			if (!ViewboxSession.LastPositionUserViewList.ContainsKey(ViewboxSession.User.SessionId))
			{
				if (tab != "")
				{
					ViewboxSession.LastPositionUserViewList.Add(ViewboxSession.User.SessionId, tab);
				}
			}
			else if (!base.ControllerContext.HttpContext.Request.Params.AllKeys.Contains("showEmpty") && !base.ControllerContext.HttpContext.Request.Params.AllKeys.Contains("showHidden") && !base.ControllerContext.HttpContext.Request.Params.AllKeys.Contains("showArchived") && !base.ControllerContext.HttpContext.Request.Params.AllKeys.Contains("showEmptyHidden"))
			{
				KeyValuePair<string, string> element = ViewboxSession.LastPositionUserViewList.FirstOrDefault((KeyValuePair<string, string> x) => x.Key == ViewboxSession.User.SessionId);
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
			if (isNoResult || operation == OperationEnum.Filter)
			{
				page = 0;
			}
			ViewboxSession.AllowedOpts = null;
			ViewboxSession.IssueOptimizationFilter.Clear();
			IOptimization opt = ViewboxSession.Optimizations.FirstOrDefault();
			if (opt == null || ViewboxSession.ViewCount == 0 || ViewboxSession.HideViewsButton)
			{
				return RedirectToAction("Index", "Home");
			}
			ViewboxSession.ClearSavedDocumentSettings("ViewList");
			if (search == null && ViewboxSession.ViewSortAndFilterSettings != null)
			{
				search = ViewboxSession.ViewSortAndFilterSettings.Filter;
			}
			if (sortColumn == 0 && ViewboxSession.ViewSortAndFilterSettings != null)
			{
				sortColumn = ViewboxSession.ViewSortAndFilterSettings.SortColumn;
				direction = ViewboxSession.ViewSortAndFilterSettings.Sort;
			}
			ViewList model = new ViewList(id, search, showEmpty || showHidden || showArchived || showEmptyHidden);
			model.PerPage = size;
			model.CurrentPage = page;
			if (objectTypeFilter.HasValue)
			{
				if (ViewboxApplication.Database.SystemDb.ObjectTypeRelationsCollection.IsEmpty)
				{
					ViewboxSession.User.Settings[UserSettingsType.View_ObjectType] = ((objectTypeFilter.Value != -1) ? objectTypeFilter.Value.ToString() : null);
				}
				else
				{
					ViewboxSession.User.Settings[UserSettingsType.View_ObjectType] = null;
					ViewboxSession.User.Settings[UserSettingsType.Extended_ObjectType] = ((objectTypeFilter.Value != -1) ? objectTypeFilter.Value.ToString() : null);
				}
			}
			ViewboxSession.TableObjectSetup(TableType.View, page, size, search, showEmpty, showHidden, showArchived, sortColumn, direction != SortDirection.Descending, out var fullTableListCount, showEmptyHidden, null, ViewboxSession.User.Settings[UserSettingsType.View_ObjectType].ToNullableInt(), ViewboxSession.User.Settings[UserSettingsType.Extended_ObjectType].ToNullableInt());
			model.TableCount = ViewboxSession.GetTableObjectCount(TableType.View, search, ViewboxSession.User.Settings[UserSettingsType.View_ObjectType].ToNullableInt(), ViewboxSession.User.Settings[UserSettingsType.Extended_ObjectType].ToNullableInt());
			model.SortColumn = sortColumn;
			model.Direction = direction;
			model.ShowEmpty = showEmpty;
			model.ShowHidden = showHidden;
			model.ShowArchived = showArchived;
			if (showArchived)
			{
				ViewboxSession.ViewOptionEnum = ViewOptionEnum.Archived;
			}
			else if (showHidden)
			{
				ViewboxSession.ViewOptionEnum = ViewOptionEnum.Hidden;
			}
			else if (showEmpty)
			{
				ViewboxSession.ViewOptionEnum = ViewOptionEnum.Empty;
			}
			else
			{
				ViewboxSession.ViewOptionEnum = ViewOptionEnum.Visible;
			}
			model.ShowEmptyHidden = showEmptyHidden;
			if (!catRefresh)
			{
				ViewboxSession.CheckedTableItems.Clear();
			}
			model.Views = model.GetListFromSessionViews(sortColumn, direction, showEmpty, ViewboxSession.User.Settings[UserSettingsType.View_ObjectType].ToNullableInt().HasValue);
			model.Count = fullTableListCount;
			IEnumerable<ITableObject> enumerable;
			if (ViewboxSession.Optimizations.Count() != 0)
			{
				enumerable = GetTablesFromQuery("", TableType.View);
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
				ViewboxSession.ViewSortAndFilterSettings = new SortAndFilterSettings();
				if (!string.IsNullOrEmpty(search))
				{
					ViewboxSession.ViewSortAndFilterSettings.Filter = search;
				}
				if (sortColumn != 0)
				{
					ViewboxSession.ViewSortAndFilterSettings.SortColumn = sortColumn;
				}
				ViewboxSession.ViewSortAndFilterSettings.Sort = direction;
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
			if (ViewboxApplication.Database.SystemDb.Issues.Any((IIssue i) => i.Database.ToLower() == opt.FindValue(OptimizationType.System).ToLower()) && !ViewboxSession.HideIssuesButton && (opt == null || !ViewboxApplication.Database.SystemDb.Views.Any((SystemDb.IView v) => v.Database.ToLower() == opt.FindValue(OptimizationType.System).ToLower()) || ViewboxSession.HideViewsButton))
			{
				return Redirect("/IssueList");
			}
			if (opt != null && ViewboxSession.TableCount > 0 && !ViewboxSession.HideTablesButton && (opt == null || !ViewboxApplication.Database.SystemDb.Views.Any((SystemDb.IView v) => v.Database.ToLower() == opt.FindValue(OptimizationType.System).ToLower()) || ViewboxSession.HideViewsButton))
			{
				return Redirect("/TableList");
			}
			if (!ViewboxApplication.Database.SystemDb.Issues.Any((IIssue i) => i.Database.ToLower() == opt.FindValue(OptimizationType.System).ToLower()) && !ViewboxSession.HideIssuesButton && (opt == null || ViewboxSession.TableCount <= 0 || ViewboxSession.HideTablesButton) && (opt == null || !ViewboxApplication.Database.SystemDb.Views.Any((SystemDb.IView v) => v.Database.ToLower() == opt.FindValue(OptimizationType.System).ToLower()) || ViewboxSession.HideViewsButton))
			{
				return Redirect("/Settings");
			}
			if (searchPartial)
			{
				return PartialView("_SearchPartial", model);
			}
			return ResultView(model, json);
		}

		public ActionResult ColumnValueList(string search = null, bool showEmpty = false, bool showHidden = false, bool showArchived = false, SortDirection direction = SortDirection.None, bool showEmptyHidden = false)
		{
			ViewList viewListModel = new ViewList();
			ViewboxSession.TableObjectSetup(TableType.View, 0, 0, search, showEmpty, showHidden, showArchived, 0, direction != SortDirection.Descending, out var _, showEmptyHidden);
			viewListModel.TableCount = ViewboxSession.GetTableObjectCount(TableType.View, search);
			viewListModel.Views = viewListModel.GetListFromSessionViews(0, direction);
			IEnumerable<ValueListElement> valueList = from x in ViewboxApplication.Database.SystemDb.ObjectTypesCollection
				where viewListModel.TableObjects.Any((ITableObject y) => y.Type == TableType.View && y.ObjectTypes.Any((KeyValuePair<string, string> z) => z.Value == GetObjectTypeTextCollection(x)))
				select new ValueListElement
				{
					Value = GetObjectTypeTextCollection(x),
					Id = x.Key
				};
			bool addDartValue = false;
			if (viewListModel.TableObjects.Any((ITableObject y) => y.Type == TableType.View && y.ObjectTypeCode == 0))
			{
				addDartValue = true;
			}
			if (direction != SortDirection.None)
			{
				valueList = ((direction == SortDirection.Ascending) ? valueList.OrderBy((ValueListElement v) => v.Value) : valueList.OrderByDescending((ValueListElement v) => v.Value));
			}
			object cachedObjectTypeText = (string.IsNullOrEmpty(ViewboxSession.User.Settings[UserSettingsType.View_ObjectType]) ? Resources.ShowAll : (from ot in valueList
				where ot.Id == ViewboxSession.User.Settings[UserSettingsType.View_ObjectType].ToNullableInt().Value
				select ot.Value).FirstOrDefault());
			ColumnValueListModels columnValueListModel = new ColumnValueListModels(valueList, addDartValue)
			{
				Direction = direction,
				PopupTitle = Resources.FilterOnField,
				AddShowAllItem = true
			};
			base.ViewBag.ListType = "ViewColumnValueList";
			return PartialView("_ColumnValueListPartial", columnValueListModel);
		}

		private string GetObjectTypeTextCollection(KeyValuePair<int, string> keyValuePair)
		{
			IObjectTypeText element = ViewboxApplication.Database.SystemDb.ObjectTypeTextCollection.FirstOrDefault((IObjectTypeText y) => y.RefId == keyValuePair.Key && y.CountryCode == ViewboxSession.Language.CountryCode);
			if (element == null)
			{
				return keyValuePair.Value;
			}
			return element.Text;
		}

		public PartialViewResult CategoryTabs(int id = -1, string search = null, bool showEmpty = false, int sortColumn = 0, SortDirection direction = SortDirection.Ascending, int page = 0, int size = 25, bool showHidden = false, bool showEmptyHidden = false)
		{
			ViewList model = new ViewList(id, search, showEmpty || showHidden);
			model.CurrentPage = page;
			model.PerPage = size;
			List<SystemDb.IView> list = model.GetViewList(showEmpty, showHidden, sortColumn, direction);
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
			if (ViewboxSession.ViewSortAndFilterSettings != null && ViewboxSession.ViewSortAndFilterSettings.SortColumn == sortColumn && ViewboxSession.ViewSortAndFilterSettings.Sort == direction)
			{
				ViewboxSession.ViewSortAndFilterSettings.SortColumn = (sortColumn = 0);
			}
			return Index(id, search, showEmpty, sortColumn, direction, json, page, size, catRefresh, fullRefresh, showHidden, showArchived, null, showEmptyHidden);
		}

		public ActionResult DeleteView(int id, string search = null, bool showEmpty = false, int sortColumn = 0, SortDirection direction = SortDirection.Ascending, bool json = false, int page = 0, int size = 25, bool catRefresh = false, bool showHidden = false)
		{
			ViewboxSession.DeleteView(id);
			if (ViewboxSession.ViewCount == 0)
			{
				return null;
			}
			return Index(-1, search, showEmpty, sortColumn, direction, json, 0, size, catRefresh, showHidden);
		}

		public ActionResult Archive(int id, ArchiveType archive)
		{
			if (ViewboxApplication.Database.SystemDb.Objects[id].IsUnderArchiving)
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
			ViewboxApplication.Database.SystemDb.Objects[id].IsUnderArchiving = true;
			string title = string.Empty;
			title = ((archive != 0) ? $"{Resources.RestoreTable} : {ViewboxSession.TableObjects[id].TableName}" : $"{Resources.ArchiveTable} : {ViewboxSession.TableObjects[id].TableName}");
			Transformation trans = Transformation.Create(ViewboxSession.TableObjects[id], archive, ViewboxSession.TableObjects[id].GetRowCount());
			long rowCount = 0L;
			rowCount = ViewboxSession.TableObjects[id].GetRowCount();
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

		public PartialViewResult GetTableObjects(string search = null, string type = "", bool showEmpty = false, bool showHidden = false, bool showArchived = false, int page = 0, int size = 25, string doselect = "", int sortColumn = 0, SortDirection direction = SortDirection.Ascending)
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

		public void ChangeTableOrder(int id, int ordinal)
		{
			ITableObject tobj = ((id < 0) ? ViewboxSession.TempTableObjects[id].Table : ViewboxSession.TableObjects[id]);
			tobj.Ordinal = ordinal;
			if (id > 0)
			{
				ViewboxApplication.Database.SystemDb.UpdateTableObjectOrder(ViewboxSession.User, TableType.View, tobj);
			}
		}

		public PartialViewResult DeleteConfirmationDialog()
		{
			return PartialView("_DialogPartial", new DialogModel
			{
				DialogType = DialogModel.Type.Info,
				Title = Resources.DeleteViewTitle,
				Content = string.Format(Resources.Proceed, Resources.DeleteViewTitle),
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
	}
}
