using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using SystemDb;
using SystemDb.Internal;
using DbAccess;
using Viewbox.Exceptions;
using Viewbox.Job;
using Viewbox.Models;
using Viewbox.Models.Wertehilfe;
using Viewbox.Properties;
using ViewboxDb;

namespace Viewbox.Controllers
{
	public abstract class TableObjectControllerBase : BaseController
	{
		public JsonResult ChangeColumnVisibility(int id, bool visible)
		{
			try
			{
				ViewboxSession.Columns[id].IsVisible = visible;
				ViewboxSession.UpdateColumn(id, visible);
				return Json(false);
			}
			catch (Exception ex)
			{
				return Json(ex.Message);
			}
		}

		public PartialViewResult ViewOptions(int id)
		{
			ViewboxSession.SaveStartLoadingTime();
			return PartialView("_ViewOptionsPartial", ViewboxSession.TableObjects[id]);
		}

		protected ActionResult ResultView(ITableObjectList model, bool json)
		{
			return json ? ((ViewResultBase)PartialView("_TableObjectsOverviewListPartial", model)) : ((ViewResultBase)View("Index", model));
		}

		public PartialViewResult GetTableObjectsBase(string search = null, string type = "", bool showEmpty = false, bool showHidden = false, bool showArchived = false, int page = 0, int size = 25, string doselect = "", int sortColumn = 1, SortDirection direction = SortDirection.Ascending, bool showEmptyHidden = false)
		{
			if (!Enum.TryParse<TableType>(type, out var typeEnum))
			{
				return null;
			}
			IEnumerable<ITableObject> enumerable;
			if (ViewboxSession.Optimizations.Count() != 0)
			{
				enumerable = GetTablesFromQuery(search, typeEnum, showEmpty, showHidden, showArchived, sortColumn, direction, showEmptyHidden);
			}
			else
			{
				IEnumerable<ITableObject> enumerable2 = new List<ITableObject>();
				enumerable = enumerable2;
			}
			IEnumerable<ITableObject> tempobjs = enumerable;
			switch (doselect)
			{
			case "all":
				foreach (ITableObject tobj in tempobjs)
				{
					ViewboxSession.CheckedTableItems[tobj.Id] = true;
				}
				break;
			case "none":
				ViewboxSession.CheckedTableItems.Clear();
				break;
			}
			if (tempobjs != null)
			{
				int count = tempobjs.Count();
				List<ITableObject> currPageObjs = ((page >= 0 && size >= 0) ? tempobjs.Skip(page * size).Take(size).ToList() : tempobjs.ToList());
				ListOptionsModel model = new ListOptionsModel
				{
					Type = typeEnum,
					ShowEmpty = showEmpty,
					ShowHidden = showHidden,
					ShowArchived = showArchived,
					ShowEmptyHidden = showEmptyHidden,
					Page = page,
					Size = size,
					Count = count,
					TableObjects = currPageObjs,
					Search = search,
					SortColumn = sortColumn,
					Direction = direction
				};
				return PartialView("_ListArchiveOptionsPartial", model);
			}
			return null;
		}

		public void DoSelect(int id = -1, bool chk = false)
		{
			ViewboxSession.CheckedTableItems[id] = chk;
		}

		internal IEnumerable<ITableObject> GetTablesFromQuery(string search = null, TableType type = TableType.All, bool showEmpty = false, bool showHidden = false, bool showArchived = false, int sortColumn = 0, SortDirection direction = SortDirection.Ascending, bool showEmptyHidden = false)
		{
			if (ViewboxSession.Optimizations.Count() == 0)
			{
				throw new ViewboxRightException($"User {ViewboxSession.User.UserName} has no rights assigned.");
			}
			ITableObjectCollection objects = ViewboxApplication.Database.SystemDb.Objects;
			string system = ViewboxSession.SelectedSystem ?? "";
			IOptimization optimization = ViewboxSession.Optimizations.LastOrDefault();
			ILanguage language = ViewboxSession.Language ?? ViewboxApplication.Database.SystemDb.DefaultLanguage;
			IUser user = ViewboxSession.User;
			IEnumerable<ITableObject> temptobjs = objects.Where((ITableObject tobj) => (string.IsNullOrEmpty(search) || (tobj.Descriptions[language] != null && tobj.Descriptions[language].ToLower().Contains(search.ToLower())) || tobj.TableName.ToLower().Contains(search.ToLower()) || tobj.TransactionNumber.ToString(CultureInfo.InvariantCulture).StartsWith(search)) && tobj.Database.ToLower() == system.ToLower() && tobj.Type == type);
			foreach (ITableObject t2 in temptobjs)
			{
				t2.RoleBasedOptimization = ViewboxSession.GetRoleBasedOptimizationFilter(t2);
			}
			IEnumerable<ITableObject> tobjs = (showEmptyHidden ? temptobjs.Where((ITableObject t) => (type == TableType.Issue || ViewboxApplication.Database.SystemDb.GetDataCount(t, optimization, t, user) <= 0) && !t.IsArchived && !ViewboxApplication.Database.SystemDb.GetIsVisibleOfTableByUser(t, user)) : (showEmpty ? temptobjs.Where((ITableObject t) => (type == TableType.Issue || ViewboxApplication.Database.SystemDb.GetDataCount(t, optimization, t, user) < 1) && !t.IsArchived && ViewboxApplication.Database.SystemDb.GetIsVisibleOfTableByUser(t, user)) : (showHidden ? temptobjs.Where((ITableObject t) => !t.IsArchived && !ViewboxApplication.Database.SystemDb.GetIsVisibleOfTableByUser(t, user)) : ((!showArchived) ? temptobjs.Where((ITableObject t) => (type == TableType.Issue || ViewboxApplication.Database.SystemDb.GetDataCount(t, optimization, t, user) > 0) && !t.IsArchived && ViewboxApplication.Database.SystemDb.GetIsVisibleOfTableByUser(t, user)) : temptobjs.Where((ITableObject t) => t.IsArchived)))));
			switch (sortColumn)
			{
			case 1:
				if (direction == SortDirection.Ascending)
				{
					return tobjs.OrderBy((ITableObject t) => t.TransactionNumber);
				}
				return tobjs.OrderByDescending((ITableObject t) => t.TransactionNumber);
			case 2:
				if (direction == SortDirection.Ascending)
				{
					return tobjs.OrderBy((ITableObject t) => t.Descriptions[language] ?? t.TableName);
				}
				return tobjs.OrderByDescending((ITableObject t) => t.Descriptions[language] ?? t.TableName);
			case 3:
				if (direction == SortDirection.Ascending)
				{
					return tobjs.OrderBy((ITableObject t) => ViewboxApplication.Database.SystemDb.GetDataCount(t, optimization, t, user));
				}
				return tobjs.OrderByDescending((ITableObject t) => ViewboxApplication.Database.SystemDb.GetDataCount(t, optimization, t, user));
			default:
				return tobjs.OrderBy((ITableObject t) => t.Ordinal);
			}
		}

		public void DoSelectNone(int id = -1, string search = null, int sortColumn = 0, SortDirection direction = SortDirection.Ascending, bool json = false, int page = 0, int size = 25, bool catRefresh = false, bool showEmpty = false, bool showHidden = false, bool showArchived = false)
		{
			ViewboxSession.CheckedTableItems.Clear();
		}

		public ActionResult DoArchiveSelected(TableType type = TableType.All, string archive = "")
		{
			if (string.IsNullOrEmpty(archive))
			{
				throw new ArgumentNullException("archive");
			}
			if (ViewboxSession.Optimizations.Count() == 0)
			{
				return RedirectToAction("Index", "Settings");
			}
			ArchiveType _archive = (ArchiveType)Enum.Parse(typeof(ArchiveType), archive);
			List<ITableObject> tobjs2 = ((ViewboxSession.Optimizations.Count() == 0) ? new List<ITableObject>() : GetTablesFromQuery("", type, showEmpty: false, showHidden: false, _archive != ArchiveType.Archive).ToList());
			List<ITableObject> tobjs = tobjs2.Where((ITableObject tobj) => ViewboxSession.CheckedTableItems.ContainsKey(tobj.Id) && ViewboxSession.CheckedTableItems[tobj.Id] && !tobj.IsUnderArchiving).ToList();
			if (tobjs.Count() == 0)
			{
				return PartialView("_DialogPartial", new DialogModel
				{
					DialogType = DialogModel.Type.Info,
					Title = Resources.PleaseSelectTables,
					Content = "Please select tables, if you selected tables, the ones you selected are being processed already.",
					Buttons = new List<DialogModel.Button>
					{
						new DialogModel.Button
						{
							Caption = Resources.OK
						}
					}
				});
			}
			long rowCount = 0L;
			rowCount = tobjs.Select((ITableObject t) => t.GetRowCount()).Sum();
			IList<Tuple<ITableObject, ArchiveType, long>> transformations = new List<Tuple<ITableObject, ArchiveType, long>>();
			foreach (ITableObject t2 in tobjs)
			{
				transformations.Add(new Tuple<ITableObject, ArchiveType, long>(t2, _archive, t2.GetRowCount()));
			}
			ViewboxSession.CheckedTableItems.Clear();
			string controller = ((type != TableType.View) ? "TableList" : "ViewList");
			Transformation.NotificationUrl url = new Transformation.NotificationUrl
			{
				Controller = controller,
				Method = "Index",
				Params = new Transformation.TableListParams
				{
					Search = "",
					ShowArchived = (_archive == ArchiveType.Archive),
					ShowEmpty = false,
					ShowHidden = false
				}
			};
			Transformation trans = Transformation.ArchiveTableList(transformations, url);
			return PartialView("_DialogPartial", new DialogModel
			{
				DialogType = DialogModel.Type.Info,
				Title = ((_archive == ArchiveType.Archive) ? Resources.ArchivationInProgress : Resources.RestorationInProgress),
				Key = trans.Key,
				Content = string.Format(Resources.LongRunningDialogText, string.Format(Resources.ArchivingInProgress, tobjs.Count, rowCount)),
				Buttons = new List<DialogModel.Button>
				{
					new DialogModel.Button
					{
						Caption = Resources.LongRunningDialogCaption
					}
				}
			});
		}

		protected ActionResult SingleResultView(ITableObjectList model, bool json)
		{
			return json ? ((ViewResultBase)PartialView("_ShowOneTableObjectOverviewPartial", model)) : ((ViewResultBase)View("Index", model));
		}

		protected IssueList GetModel(bool isSingleResult, int id = -1, string search = null, int sortColumn = 0, SortDirection direction = SortDirection.Ascending, int page = 0, int size = 25, bool showHidden = false, int? objectTypeFilter = null, int? extended_ObjectType = null, IIssue selectedIssue = null)
		{
			if (search == null && ViewboxSession.IssueSortAndFilterSettings != null)
			{
				search = ViewboxSession.IssueSortAndFilterSettings.Filter;
			}
			if (sortColumn == 0 && ViewboxSession.IssueSortAndFilterSettings != null)
			{
				sortColumn = ViewboxSession.IssueSortAndFilterSettings.SortColumn;
				direction = ViewboxSession.IssueSortAndFilterSettings.Sort;
			}
			IssueList model = new IssueList(id, search, showHidden);
			model.PerPage = size;
			IEnumerable<int> favIds = ViewboxApplication.Database.SystemDb.GetUserFavoriteIssues(ViewboxSession.User);
			ViewboxSession.TableObjectSetup(TableType.Issue, page, -1, null, showEmpty: false, showHidden, showArchived: false, sortColumn, direction != SortDirection.Descending, out var fullTableListCount, showEmptyHidden: false, null, objectTypeFilter, extended_ObjectType);
			ViewboxSession.LoadFavoriteIssues(favIds);
			ViewboxApplication.Database.SystemDb.ChangeSequenceOfIssueParameterValues(ViewboxSession.Issues, ViewboxSession.User);
			model.FavoriteIssues = model.GetListFromSessionIssues(favIds, exclude: false, null, ViewboxSession.Language, out fullTableListCount, sortColumn, direction, 0, 1000);
			model.TableCount = ViewboxSession.GetTableObjectCount(TableType.Issue, search, null, extended_ObjectType);
			int filteredTableListCount = model.TableCount.ArchivedTableCount + model.TableCount.EmptyTableCount + model.TableCount.InVisibleTableCount + model.TableCount.VisibleTableCount + model.TableCount.EmptyInVisibleTableCount;
			model.CurrentPage = page;
			model.SortColumn = sortColumn;
			model.Direction = direction;
			model.ShowHidden = showHidden;
			model.Issues = model.GetListFromSessionIssues(favIds, exclude: true, search, ViewboxSession.Language, out fullTableListCount, sortColumn, direction, page, size);
			model.Count = fullTableListCount;
			model.SelectedIssues = null;
			if (isSingleResult)
			{
				model.SelectedIssues = model.GetIssueFromSessionIssues(id);
			}
			else if (model.SelectedIssues == null)
			{
				if (model.FavoriteIssues != null && model.FavoriteIssues.Any())
				{
					model.SelectedIssues = model.GetIssueFromSessionIssues(model.FavoriteIssues.First().Id);
				}
				else if (model.Issues != null && model.Issues.Any())
				{
					model.SelectedIssues = model.GetIssueFromSessionIssues(model.Issues.First().Id);
				}
				else if (model.SelectedIssues == null)
				{
					model.SelectedIssues = new List<IIssue>();
				}
			}
			bool parameterRefreshRequired = false;
			if (ViewboxApplication.Database.SystemDb.UserLastIssueSettingsCollection[ViewboxSession.User] != null && !ViewboxSession.Issues.Contains(ViewboxApplication.Database.SystemDb.UserLastIssueSettingsCollection[ViewboxSession.User].LastUsedIssue) && ViewboxApplication.Database.SystemDb.Issues.Contains(ViewboxApplication.Database.SystemDb.UserLastIssueSettingsCollection[ViewboxSession.User].LastUsedIssue) && string.CompareOrdinal(ViewboxSession.SelectedSystem, ViewboxApplication.Database.SystemDb.Issues[ViewboxApplication.Database.SystemDb.UserLastIssueSettingsCollection[ViewboxSession.User].LastUsedIssue].Database) == 0)
			{
				ViewboxApplication.Database.SystemDb.AddTableToIssue(ViewboxSession.User, ViewboxSession.TableObjects, ViewboxSession.Issues, ViewboxApplication.Database.SystemDb.UserLastIssueSettingsCollection[ViewboxSession.User].LastUsedIssue);
				model.Issues = model.GetListFromSessionIssues(favIds, exclude: true, search, ViewboxSession.Language, out fullTableListCount, sortColumn, direction, page, size);
				model.Count = fullTableListCount;
			}
			ViewboxApplication.Database.SystemDb.SaveLastExecutedIssue(ViewboxSession.User, id);
			int lastUsedIssue = ViewboxApplication.Database.SystemDb.UserLastIssueSettingsCollection[ViewboxSession.User].LastUsedIssue;
			if (ViewboxSession.Issues.Contains(lastUsedIssue))
			{
				model.SelectedIssues = new List<IIssue> { ViewboxSession.Issues[lastUsedIssue] };
			}
			else
			{
				parameterRefreshRequired = true;
			}
			if (model.SelectedIssues == null)
			{
				if (model.FavoriteIssues != null && model.FavoriteIssues.Any())
				{
					model.SelectedIssues = model.GetIssueFromSessionIssues(model.FavoriteIssues.First().Id);
				}
				else if (model.Issues != null && model.Issues.Any())
				{
					using DatabaseBase db = ViewboxApplication.Database.ConnectionManager.GetConnection();
					db.DbMapping.Delete<UserLastIssueSettings>($"user_id = {ViewboxSession.User.Id}");
					db.DbMapping.Save(lastUsedIssue);
				}
				if (parameterRefreshRequired)
				{
					IIssue issue2 = model.SelectedIssues.FirstOrDefault();
					ViewboxSession.IssueOptimizationFilter.Clear();
					if (issue2 != null && issue2.Parameters != null && issue2.Parameters.Any((IParameter x) => x.OptimizationType != OptimizationType.NotSet))
					{
						foreach (IParameter param2 in issue2.Parameters)
						{
							if (param2.OptimizationType != 0 && !ViewboxSession.IssueOptimizationFilter.Contains(param2.OptimizationType))
							{
								ViewboxSession.IssueOptimizationFilter.Add(param2.OptimizationType);
							}
						}
						if (ViewboxSession.AllowedOpts == null)
						{
							string allowedIndex = "";
							string allowedSplit = "";
							string allowedSort = "";
							foreach (IOptimization item2 in ViewboxSession.AllowedOptimizations)
							{
								if (item2.Value != null)
								{
									if (item2.Group.Type == OptimizationType.IndexTable)
									{
										allowedIndex = allowedIndex + item2.Value + ",";
									}
									else if (item2.Group.Type == OptimizationType.SplitTable)
									{
										allowedSplit = allowedSplit + item2.Value + ",";
									}
									else if (item2.Group.Type == OptimizationType.SortColumn)
									{
										allowedSort = allowedSort + item2.Value + ",";
									}
								}
							}
							ViewboxSession.AllowedOpts = new List<Tuple<OptimizationType, string>>();
							if (allowedIndex != "")
							{
								ViewboxSession.AllowedOpts.Add(Tuple.Create(OptimizationType.IndexTable, allowedIndex));
							}
							if (allowedSplit != "")
							{
								ViewboxSession.AllowedOpts.Add(Tuple.Create(OptimizationType.SplitTable, allowedSplit));
							}
							if (allowedSort != "")
							{
								ViewboxSession.AllowedOpts.Add(Tuple.Create(OptimizationType.SortColumn, allowedSort));
							}
						}
					}
				}
			}
			if (search != null || sortColumn != 0)
			{
				ViewboxSession.IssueSortAndFilterSettings = new SortAndFilterSettings();
				if (!string.IsNullOrEmpty(search))
				{
					ViewboxSession.IssueSortAndFilterSettings.Filter = search;
				}
				if (sortColumn != 0)
				{
					ViewboxSession.IssueSortAndFilterSettings.SortColumn = sortColumn;
				}
				ViewboxSession.IssueSortAndFilterSettings.Sort = direction;
				ViewboxSession.User.Settings[UserSettingsType.ReportLastFilterCondition] = search;
			}
			if (model.SelectedIssues.Any())
			{
				ViewboxApplication.Database.SystemDb.SaveLastExecutedIssue(ViewboxSession.User, model.SelectedIssues.ElementAt(0).Id);
				IOptimization opt = ViewboxSession.Optimizations.LastOrDefault();
				string languageCode = LanguageKeyTransformer.Transformer(ViewboxSession.User.CurrentLanguage);
				IIssue issue = model.SelectedIssues.FirstOrDefault();
				Dictionary<IParameter, IWertehilfe> helper = new Dictionary<IParameter, IWertehilfe>();
				foreach (IParameter param in issue.Parameters)
				{
					if (param.TableName != null && param.DatabaseName != null)
					{
						IWertehilfe wertehilfeModel = WertehilfeFactory.Create(param.Id, "", isExact: true, onlyCheck: true);
						wertehilfeModel.Language = LanguageKeyTransformer.Transformer(languageCode);
						wertehilfeModel.Optimization = opt;
						wertehilfeModel.RowsPerPage = ViewboxApplication.WertehilfeSize;
						helper.Add(param, wertehilfeModel);
					}
				}
				Parallel.ForEach(helper, delegate(KeyValuePair<IParameter, IWertehilfe> item)
				{
					item.Value.BuildValuesCollection();
					item.Key.HasIndexData = item.Value.ValueListCollection.Count > 0;
					item.Key.IndexDataGenerated = item.Value.HasIndex;
				});
			}
			if (selectedIssue != null)
			{
				model.SelectedIssues = new List<IIssue> { selectedIssue };
			}
			return model;
		}

		[HttpGet]
		public void AddOrUpdateTransactionNumber(int id, string transactionNumber)
		{
			ITableObject table = ViewboxApplication.Database.SystemDb.GetTableObjectNeeded(id);
			if (table == null)
			{
				return;
			}
			table.TransactionNumber = transactionNumber;
			using DatabaseBase db = ViewboxApplication.Database.ConnectionManager.GetConnection();
			string sql = string.Format("SELECT COUNT(*) FROM `{0}`.`{1}` WHERE `table_id` = {2};", db.DbConfig.DbName, "table_transactions", id);
			if (int.Parse(db.ExecuteScalar(sql).ToString()) != 0)
			{
				sql = string.Format("UPDATE `{0}`.`{1}` SET `transaction_number` = '{2}' WHERE `table_id` = {3};", db.DbConfig.DbName, "table_transactions", transactionNumber, id);
				db.ExecuteNonQuery(sql);
			}
			else
			{
				sql = string.Format("INSERT INTO `{0}`.`{1}` VALUES (null, {2}, '{3}');", db.DbConfig.DbName, "table_transactions", id, transactionNumber);
				db.ExecuteNonQuery(sql);
			}
			IEnumerable<TableTransactions> trans = ViewboxApplication.Database.SystemDb.TableTransactions.Where((TableTransactions x) => x.TableId == id);
			for (int i = 0; i < trans.Count(); i++)
			{
				TableTransactions tran = trans.ElementAt(i);
				if (tran == null)
				{
					tran = new TableTransactions();
					tran.TableId = id;
					tran.TransactionNumber = transactionNumber;
					ViewboxApplication.Database.SystemDb.TableTransactions.Add(tran);
					db.DbMapping.Save(tran);
				}
				else
				{
					tran.TransactionNumber = transactionNumber;
					db.DbMapping.Save(tran);
				}
			}
			IUser user = ViewboxSession.User;
			IUserTableTransactionIdSettings usertran = ViewboxApplication.Database.SystemDb.UserTableTransactionIdSettings[table, user];
			if (usertran == null)
			{
				usertran = new UserTableTransactionIdSettings
				{
					TableId = table.Id,
					Table = table,
					TransactionId = transactionNumber,
					User = user,
					UserId = user.Id
				};
				db.DbMapping.Save(usertran);
				((UserTableTransactionIdSettingsCollection)ViewboxApplication.Database.SystemDb.UserTableTransactionIdSettings).Add(usertran);
			}
			else
			{
				((UserTableTransactionIdSettings)usertran).TransactionId = transactionNumber;
				db.DbMapping.Save(usertran);
			}
		}
	}
}
