using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
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
	public class ExportController : BaseController
	{
		public ICategoryCollection GetTableCollection(TableType type)
		{
			return type switch
			{
				TableType.Table => ViewboxSession.TableCategories, 
				TableType.View => ViewboxSession.ViewCategories, 
				_ => throw new ArgumentException("type"), 
			};
		}

		public ActionResult Index(TableType type = TableType.View, int cat_id = -1, int table_id = -1, bool finished = false, bool urlNotFound = false)
		{
			if (!ViewboxSession.User.AllowedExport && !ViewboxSession.User.IsSuper)
			{
				return RedirectToAction("Index", "Home");
			}
			ViewboxSession.ClearSavedDocumentSettings("Export");
			ICategoryCollection collection = GetTableCollection(type);
			DialogModel dialog = null;
			if (urlNotFound)
			{
				dialog = new DialogModel
				{
					DialogType = DialogModel.Type.Warning,
					Title = Resources.Error,
					Content = Resources.ExportFileDoesntExistsAnymore,
					Buttons = new List<DialogModel.Button>
					{
						new DialogModel.Button
						{
							Caption = Resources.OK
						}
					}
				};
			}
			if (collection.Count == 0)
			{
				TableType otherType = ((type == TableType.View) ? TableType.Table : TableType.View);
				return View(new ExportModel(otherType, GetTableCollection(otherType), collection, table_id, finished)
				{
					Dialog = dialog
				});
			}
			return View(new ExportModel(type, collection, GetTableCollection((type == TableType.View) ? TableType.Table : TableType.View), table_id, finished)
			{
				Dialog = dialog
			});
		}

		public PartialViewResult ExportTableOverview(TableType type = TableType.View, int id = -1, string search = null, bool showEmpty = false, int sortColumn = 0, SortDirection direction = SortDirection.Ascending, int page = 0, int size = 25, bool showHidden = false, bool showEmptyHidden = false, bool showArchived = false)
		{
			search = ViewboxSession.SearchTablePdfExport;
			IEnumerable<ITableObject> list = GetListSync(type, id, search, showEmpty, sortColumn, direction, showHidden, showEmptyHidden);
			List<Tuple<string, string, string>> tableOptions = new List<Tuple<string, string, string>>();
			switch (type)
			{
			case TableType.Table:
			{
				TableList tableListModel = new TableList();
				tableListModel.Tables = tableListModel.GetListFromSessionTables(sortColumn, direction);
				bool showArchive = ViewboxSession.ViewOptionEnum == ViewOptionEnum.Archived;
				tableListModel.TableCount = ViewboxSession.GetTableObjectCount(TableType.Table, search);
				if (showEmptyHidden)
				{
					list = tableListModel.TableCount.EmptyInVisibleTables;
					tableOptions.Add(new Tuple<string, string, string>("visibility", string.Empty, string.Format(Resources.EmptyHiddenTableObjects, Resources.Tables)));
				}
				else if (showEmpty)
				{
					list = tableListModel.TableCount.EmptyTables;
					tableOptions.Add(new Tuple<string, string, string>("visibility", string.Empty, string.Format(Resources.EmptyTables, Resources.Tables)));
				}
				else if (showHidden)
				{
					list = tableListModel.TableCount.InVisibleTables;
					tableOptions.Add(new Tuple<string, string, string>("visibility", string.Empty, string.Format(Resources.HiddenTableObjects, Resources.Tables)));
				}
				else if (showArchived)
				{
					list = tableListModel.TableCount.ArchivedTables;
					tableOptions.Add(new Tuple<string, string, string>("visibility", string.Empty, string.Format(Resources.ArchivedTableObjects, Resources.Tables)));
				}
				else
				{
					list = tableListModel.TableCount.VisibleTables;
					tableOptions.Add(new Tuple<string, string, string>("visibility", string.Empty, string.Format(Resources.VisibleTables, Resources.Tables)));
				}
				break;
			}
			case TableType.View:
			{
				ViewList viewListModel = new ViewList();
				viewListModel.Views = viewListModel.GetListFromSessionViews(sortColumn, direction);
				if (type == TableType.View && ViewboxSession.ViewSortAndFilterSettings != null && search == null)
				{
					search = ViewboxSession.ViewSortAndFilterSettings.Filter;
				}
				viewListModel.TableCount = ViewboxSession.GetTableObjectCount(TableType.View, search);
				IEnumerable<ValueListElement> valueList = from x in ViewboxApplication.Database.SystemDb.ObjectTypesCollection
					where viewListModel.TableObjects.Any((ITableObject y) => y.Type == TableType.View && y.ObjectTypes.Any((KeyValuePair<string, string> z) => z.Value == GetObjectTypeTextCollection(x)))
					select new ValueListElement
					{
						Value = GetObjectTypeTextCollection(x),
						Id = x.Key
					};
				valueList = ((direction == SortDirection.Ascending) ? valueList.OrderBy((ValueListElement v) => v.Value) : valueList.OrderByDescending((ValueListElement v) => v.Value));
				int? nullableInt = ViewboxSession.User.Settings[UserSettingsType.View_ObjectType].ToNullableInt();
				object cachedObjectTypeText = (string.IsNullOrEmpty(ViewboxSession.User.Settings[UserSettingsType.View_ObjectType]) ? Resources.ShowAll : (from ot in valueList
					where nullableInt.HasValue && ot.Id == nullableInt.Value
					select ot.Value).FirstOrDefault());
				if (showEmptyHidden)
				{
					list = viewListModel.TableCount.EmptyInVisibleTables;
					tableOptions.Add(new Tuple<string, string, string>("visibility", string.Empty, string.Format(Resources.EmptyHiddenTableObjects, Resources.Views)));
				}
				else if (showEmpty)
				{
					list = viewListModel.TableCount.EmptyTables;
					tableOptions.Add(new Tuple<string, string, string>("visibility", string.Empty, string.Format(Resources.EmptyTables, Resources.Views)));
				}
				else if (showHidden)
				{
					list = viewListModel.TableCount.InVisibleTables;
					tableOptions.Add(new Tuple<string, string, string>("visibility", string.Empty, string.Format(Resources.HiddenTableObjects, Resources.Views)));
				}
				else if (showArchived)
				{
					list = viewListModel.TableCount.ArchivedTables;
					tableOptions.Add(new Tuple<string, string, string>("visibility", string.Empty, string.Format(Resources.ArchivedTableObjects, Resources.Views)));
				}
				else
				{
					list = viewListModel.TableCount.VisibleTables;
					tableOptions.Add(new Tuple<string, string, string>("visibility", string.Empty, string.Format(Resources.VisibleTables, Resources.Views)));
				}
				if ((string)cachedObjectTypeText != Resources.ShowAll)
				{
					list = list.Where((ITableObject x) => x.GetObjectType() == (string)cachedObjectTypeText);
					tableOptions.Add(new Tuple<string, string, string>("module", Resources.Module, (string)cachedObjectTypeText));
				}
				break;
			}
			}
			if (!string.IsNullOrWhiteSpace(search))
			{
				tableOptions.Add(new Tuple<string, string, string>("search", Resources.SearchPdf, search));
			}
			List<ITableObject> templist = new List<ITableObject>();
			switch (sortColumn)
			{
			case 1:
				if (direction == SortDirection.Ascending)
				{
					templist = new List<ITableObject>(list.OrderBy((ITableObject l) => l.TransactionNumber));
				}
				if (direction == SortDirection.Descending)
				{
					templist = new List<ITableObject>(list.OrderByDescending((ITableObject l) => l.TransactionNumber));
				}
				break;
			case 2:
				if (direction == SortDirection.Ascending)
				{
					templist = new List<ITableObject>(list.OrderBy((ITableObject t) => t.GetDescription()));
				}
				if (direction == SortDirection.Descending)
				{
					templist = new List<ITableObject>(list.OrderByDescending((ITableObject t) => t.GetDescription()));
				}
				break;
			case 3:
				if (direction == SortDirection.Ascending)
				{
					templist = new List<ITableObject>(list.OrderBy((ITableObject t) => t.GetRowCount()));
				}
				if (direction == SortDirection.Descending)
				{
					templist = new List<ITableObject>(list.OrderByDescending((ITableObject t) => t.GetRowCount()));
				}
				break;
			case 4:
				if (direction == SortDirection.Ascending)
				{
					templist = new List<ITableObject>(from t in list
						orderby t.GetObjectType(), t.GetDescription()
						select t);
				}
				if (direction == SortDirection.Descending)
				{
					templist = new List<ITableObject>(from t in list
						orderby t.GetObjectType() descending, t.GetDescription()
						select t);
				}
				break;
			default:
				templist = ((type != TableType.View) ? new List<ITableObject>(list.OrderBy((ITableObject t) => t.Ordinal)) : new List<ITableObject>(from t in list
					orderby t.GetObjectType(), t.Ordinal
					select t));
				break;
			}
			ITableObjectCollection objects = ViewboxApplication.Database.SystemDb.CreateTableObjectCollection();
			objects.AddRange(templist);
			Export job = Export.Create(objects, ExportType.PDF, tableOptions);
			string ttype = "";
			ttype += ((type == TableType.Table) ? Resources.Tables : "");
			ttype += ((type == TableType.View) ? Resources.Views : "");
			ttype += ((type == TableType.Issue) ? Resources.Issues : "");
			return PartialView("_DialogPartial", new DialogModel
			{
				DialogType = DialogModel.Type.Info,
				Title = string.Format(Resources.ExportOverview, ExportType.PDF, Resources.Overview, ttype),
				Key = job.Key,
				Content = string.Format(Resources.ShortRunningDialogText, Resources.ExportData),
				Buttons = new List<DialogModel.Button>
				{
					new DialogModel.Button
					{
						Caption = Resources.LongRunningDialogCaption
					}
				}
			});
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

		private static IEnumerable<ITableObject> GetListSync(TableType type, int id, string search, bool showEmpty, int sortColumn, SortDirection direction, bool showHidden, bool showEmptyHidden)
		{
			IEnumerable<ITableObject> list = null;
			switch (type)
			{
			case TableType.Issue:
			{
				IssueList model3 = new IssueList(id, search, showHidden);
				list = model3.GetIssueList(showHidden, sortColumn, direction, isPdfExport: true);
				break;
			}
			case TableType.Table:
			{
				TableList model = new TableList(id, search, showEmpty || showHidden);
				list = model.GetTableList(showEmpty, showHidden, sortColumn, direction, isPDFexport: true, showEmptyHidden);
				break;
			}
			case TableType.View:
			{
				ViewList model2 = new ViewList(id, search, showEmpty || showHidden);
				list = model2.GetViewList(showEmpty, showHidden, sortColumn, direction, isPdfExport: true, showEmptyHidden);
				break;
			}
			}
			return list;
		}

		public ActionResult TableObjects(TableType type, int id = -1, int page = 0, bool json = false, string search = null, int sortColumn = 1, SortDirection direction = SortDirection.Ascending)
		{
			if (json)
			{
				IOptimization opt = ViewboxSession.Optimizations.LastOrDefault();
				string optValue = string.Empty;
				if (opt != null)
				{
					optValue = opt.GetOptimizationValue(OptimizationType.System);
				}
				int perPage = ViewboxSession.User.DisplayRowCount;
				bool directionSimplified = direction == SortDirection.Ascending;
				int count = 0;
				ViewboxSession.SetupTableObjectsForExport(type, page, perPage, search, sortColumn, directionSimplified, optValue, out count);
				List<object> list = new List<object>();
				switch (type)
				{
				case TableType.Table:
				{
					List<ITable> templist = new List<ITable>();
					switch (sortColumn)
					{
					case 1:
						if (direction == SortDirection.Ascending)
						{
							templist = new List<ITable>(ViewboxSession.Tables.OrderBy((ITable l) => l.GetDescription()));
						}
						if (direction == SortDirection.Descending)
						{
							templist = new List<ITable>(ViewboxSession.Tables.OrderByDescending((ITable l) => l.GetDescription()));
						}
						break;
					case 2:
						if (direction == SortDirection.Ascending)
						{
							templist = new List<ITable>(ViewboxSession.Tables.OrderBy((ITable t) => t.GetRowCount()));
						}
						if (direction == SortDirection.Descending)
						{
							templist = new List<ITable>(ViewboxSession.Tables.OrderByDescending((ITable t) => t.GetRowCount()));
						}
						break;
					default:
						templist = new List<ITable>(ViewboxSession.Tables.OrderBy((ITable t) => t.Ordinal));
						break;
					}
					list.AddRange(templist.Select((ITable table) => new
					{
						Id = table.Id,
						Name = table.Name,
						Type = table.Type,
						Description = table.GetDescription(),
						RowCount = table.GetRowCount().ToString("#,0"),
						Filter = string.Empty,
						Sort = string.Empty,
						Selected = false,
						CategoryId = table.Category.Id
					}));
					break;
				}
				case TableType.View:
				{
					List<SystemDb.IView> templist2 = new List<SystemDb.IView>();
					switch (sortColumn)
					{
					case 1:
						if (direction == SortDirection.Ascending)
						{
							templist2 = new List<SystemDb.IView>(ViewboxSession.Views.OrderBy((SystemDb.IView l) => l.GetDescription()));
						}
						if (direction == SortDirection.Descending)
						{
							templist2 = new List<SystemDb.IView>(ViewboxSession.Views.OrderByDescending((SystemDb.IView l) => l.GetDescription()));
						}
						break;
					case 2:
						if (direction == SortDirection.Ascending)
						{
							templist2 = new List<SystemDb.IView>(ViewboxSession.Views.OrderBy((SystemDb.IView t) => t.GetRowCount()));
						}
						if (direction == SortDirection.Descending)
						{
							templist2 = new List<SystemDb.IView>(ViewboxSession.Views.OrderByDescending((SystemDb.IView t) => t.GetRowCount()));
						}
						break;
					default:
						templist2 = new List<SystemDb.IView>(ViewboxSession.Views.OrderBy((SystemDb.IView t) => t.Ordinal));
						break;
					}
					list.AddRange(templist2.Select((SystemDb.IView table) => new
					{
						Id = table.Id,
						Name = table.Name,
						Type = table.Type,
						Description = table.GetDescription(),
						RowCount = table.GetRowCount().ToString("#,0"),
						Filter = string.Empty,
						Sort = string.Empty,
						Selected = false,
						CategoryId = table.Category.Id
					}));
					break;
				}
				}
				list.Insert(0, count);
				return Json(list, JsonRequestBehavior.AllowGet);
			}
			return View("Index", new ExportModel(type, GetTableCollection(type), GetTableCollection((type == TableType.View) ? TableType.Table : TableType.View)));
		}

		public JsonResult GetAllTableIdsByCatagoryJson(TableType type, int categoryId, string search = "")
		{
			List<ITableObject> objList = null;
			objList = ((!(search != string.Empty)) ? ViewboxApplication.Database.SystemDb.Objects.Where((ITableObject obj) => obj.Category.Id == categoryId && obj.Type == type && obj.IsVisible).ToList() : ViewboxApplication.Database.SystemDb.Objects.Where((ITableObject obj) => obj.Category.Id == categoryId && obj.Type == type && obj.IsVisible && obj.Descriptions[ViewboxSession.Language].ToLower().Contains(search.ToLower())).ToList());
			IEnumerable<int> idList = objList.Select((ITableObject obj) => obj.Id);
			return Json(new { categoryId, idList });
		}

		public ActionResult Columns(int id, bool json = false, string search = null)
		{
			ViewboxSession.SetupTableColumns(id);
			ITableObject tobj = ViewboxSession.TableObjects[id];
			IColumnCollection columns = (ViewboxSession.User.IsSuper ? ViewboxApplication.Database.SystemDb.Objects.SingleOrDefault((ITableObject t) => t.Id == id).Columns : tobj.Columns);
			if (tobj.RowCount != 0)
			{
				if (json)
				{
					return Json(columns.Select((IColumn column) => new
					{
						Id = column.Id,
						Name = column.Name,
						Type = column.Type,
						Description = column.GetDescription(),
						Selected = column.IsVisible
					}), JsonRequestBehavior.AllowGet);
				}
			}
			else if (json)
			{
				List<object> emptyColList = new List<object>();
				emptyColList.Add(new
				{
					Name = "warning",
					Description = Resources.EmptyTableExpand
				});
				return Json(emptyColList, JsonRequestBehavior.AllowGet);
			}
			return Index(tobj.Type);
		}

		private ActionResult NeedAllColumnsDialog()
		{
			return PartialView("_DialogPartial", new DialogModel
			{
				DialogType = DialogModel.Type.Warning,
				Title = Resources.Warning,
				Content = Resources.GetAllFields,
				Buttons = new List<DialogModel.Button>
				{
					new DialogModel.Button
					{
						Caption = Resources.Yes,
						Data = "needall"
					},
					new DialogModel.Button
					{
						Caption = Resources.No,
						Data = "notall"
					}
				}
			});
		}

		private ActionResult CheckSizeDialog(int type)
		{
			return type switch
			{
				0 => PartialView("_DialogPartial", new DialogModel
				{
					DialogType = DialogModel.Type.Warning,
					Title = Resources.Warning,
					Content = string.Format(Resources.PdfWarning, string.Join(" ", Resources.PdfVeryBig, Resources.And, Resources.PdfUnclear), string.Join(" ", Resources.PdfFilterResult, Resources.And, Resources.PdfHideColumns)),
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
				}), 
				1 => PartialView("_DialogPartial", new DialogModel
				{
					DialogType = DialogModel.Type.Warning,
					Title = Resources.Warning,
					Content = string.Format(Resources.PdfWarning, Resources.PdfVeryBig, Resources.PdfFilterResult),
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
				}), 
				2 => PartialView("_DialogPartial", new DialogModel
				{
					DialogType = DialogModel.Type.Warning,
					Title = Resources.Warning,
					Content = string.Format(Resources.PdfWarning, Resources.PdfUnclear, Resources.PdfHideColumns),
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
				}), 
				_ => null, 
			};
		}

		public ActionResult Start(int id, ExportType type, bool checkSize = true, bool checkAllFields = false, bool getAllFields = false, string fromTab = "")
		{
			string tname = "";
			string ttype = "";
			if (id < 0)
			{
				List<string> allowedSchemes2 = new List<string>();
				allowedSchemes2.Add("dsv_013");
				allowedSchemes2.Add("dsv_030");
				allowedSchemes2.Add("dsv_nettethal");
				ViewboxDb.TableObject tobj2 = ViewboxSession.TempTableObjects[id];
				if (allowedSchemes2.Contains(tobj2.OriginalTable.Database) || !ViewboxApplication.HasAnyDSVBug)
				{
					if (checkAllFields && tobj2.Table.Columns.Any((IColumn c) => c.IsEmpty))
					{
						return NeedAllColumnsDialog();
					}
					if (checkSize && type == ExportType.PDF)
					{
						long rowCount = ((tobj2.GroupSubTotal != null && !string.IsNullOrEmpty(tobj2.GroupSubTotal.sql)) ? ViewboxSession.GetSubtotalDataCount(tobj2.GroupSubTotal.sql) : tobj2.Table.GetRowCount());
						bool suitableVisibles = tobj2.Table.Columns.Count((IColumn c) => !c.IsEmpty && c.IsVisible) > 7;
						if (rowCount > 50000)
						{
							return PartialView("_DialogPartial", new DialogModel
							{
								DialogType = DialogModel.Type.Warning,
								Content = string.Format(Resources.TooManyRowsExportProblem, type),
								Buttons = new List<DialogModel.Button>
								{
									new DialogModel.Button
									{
										Caption = Resources.OK
									}
								}
							});
						}
						if (rowCount > 50000 && suitableVisibles)
						{
							return CheckSizeDialog(0);
						}
						if (rowCount > 50000)
						{
							return CheckSizeDialog(1);
						}
						if (suitableVisibles)
						{
							return CheckSizeDialog(2);
						}
					}
					ttype += ((tobj2.Table.Type == TableType.Table) ? Resources.Table : "");
					ttype += ((tobj2.Table.Type == TableType.View) ? Resources.View : "");
					ttype += ((tobj2.Table.Type == TableType.Issue) ? Resources.Issue : "");
					tname = tobj2.Table.GetDescription();
					ViewboxDb.TableObjectCollection tempTableObjects = new ViewboxDb.TableObjectCollection();
					tempTableObjects.Add(tobj2);
					Base job = Export.Create(tempTableObjects, ViewboxSession.Optimizations.LastOrDefault(), type, getAllFields, fromTab, ViewboxSession.Optimizations);
					return PartialView("_DialogPartial", new DialogModel
					{
						DialogType = DialogModel.Type.Info,
						Key = job.Key,
						Title = string.Format(Resources.ExportTableLabel, type.ToString(), ttype, tname.Substring(0, Math.Min(tname.Length, 30))),
						Content = string.Format(Resources.LongRunningDialogText, Resources.ExportData),
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
					Title = string.Format(Resources.ExportTableLabel, type.ToString(), ttype, tname.Substring(0, Math.Min(tname.Length, 30))),
					Content = string.Format(Resources.LongRunningDialogText, Resources.ExportData),
					Buttons = new List<DialogModel.Button>
					{
						new DialogModel.Button
						{
							Caption = Resources.LongRunningDialogCaption
						}
					}
				});
			}
			List<string> allowedSchemes = new List<string>();
			allowedSchemes.Add("dsv_013");
			allowedSchemes.Add("dsv_030");
			allowedSchemes.Add("dsv_nettethal");
			ITableObject tobj = ViewboxSession.TableObjects[id];
			if (allowedSchemes.Contains(tobj.Database) || !ViewboxApplication.HasAnyDSVBug)
			{
				if (checkAllFields && tobj.Columns.Any((IColumn c) => c.IsEmpty))
				{
					return NeedAllColumnsDialog();
				}
				if (checkSize && type == ExportType.PDF)
				{
					if (tobj.GetRowCount() > 50000)
					{
						return PartialView("_DialogPartial", new DialogModel
						{
							DialogType = DialogModel.Type.Warning,
							Content = string.Format(Resources.TooManyRowsExportProblem, type),
							Buttons = new List<DialogModel.Button>
							{
								new DialogModel.Button
								{
									Caption = Resources.OK
								}
							}
						});
					}
					if (tobj.GetRowCount() > 50000 && tobj.Columns.Where((IColumn c) => !c.IsEmpty && c.IsVisible).Count() > 7)
					{
						return CheckSizeDialog(0);
					}
					if (tobj.GetRowCount() > 50000)
					{
						return CheckSizeDialog(1);
					}
					if (tobj.Columns.Where((IColumn c) => !c.IsEmpty && c.IsVisible).Count() > 7)
					{
						return CheckSizeDialog(2);
					}
				}
				ttype += ((tobj.Type == TableType.Table) ? Resources.Table : "");
				ttype += ((tobj.Type == TableType.View) ? Resources.View : "");
				ttype += ((tobj.Type == TableType.Issue) ? Resources.Issue : "");
				tname = tobj.GetDescription();
				ITableObjectCollection objects = ViewboxApplication.Database.SystemDb.CreateTableObjectCollection();
				objects.Add(tobj);
				Base job = Export.Create(objects, ViewboxSession.Optimizations.LastOrDefault(), type, getAllFields);
				return PartialView("_DialogPartial", new DialogModel
				{
					DialogType = DialogModel.Type.Info,
					Key = job.Key,
					Title = string.Format(Resources.ExportTableLabel, type.ToString(), ttype, tname.Substring(0, Math.Min(tname.Length, 30))),
					Content = string.Format(Resources.LongRunningDialogText, Resources.ExportData),
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
				Title = string.Format(Resources.ExportTableLabel, type.ToString(), ttype, tname.Substring(0, Math.Min(tname.Length, 30))),
				Content = string.Format(Resources.LongRunningDialogText, Resources.ExportData),
				Buttons = new List<DialogModel.Button>
				{
					new DialogModel.Button
					{
						Caption = Resources.LongRunningDialogCaption
					}
				}
			});
		}

		public ActionResult StartPdfUserDefined(int id, Export.UserDefinedExportProjects project)
		{
			string tname = "";
			string ttype = "";
			Base job;
			if (id < 0)
			{
				ViewboxDb.TableObject tobj2 = ViewboxSession.TempTableObjects[id];
				ttype += ((tobj2.Table.Type == TableType.Table) ? Resources.Table : "");
				ttype += ((tobj2.Table.Type == TableType.View) ? Resources.View : "");
				ttype += ((tobj2.Table.Type == TableType.Issue) ? Resources.Issue : "");
				tname = tobj2.Table.GetDescription();
				ViewboxDb.TableObjectCollection tempTableObjects = new ViewboxDb.TableObjectCollection();
				tempTableObjects.Add(tobj2);
				job = Export.CreateUserDefined(tempTableObjects, ViewboxSession.Optimizations.LastOrDefault(), project);
			}
			else
			{
				ITableObject tobj = ViewboxSession.TableObjects[id];
				ttype += ((tobj.Type == TableType.Table) ? Resources.Table : "");
				ttype += ((tobj.Type == TableType.View) ? Resources.View : "");
				ttype += ((tobj.Type == TableType.Issue) ? Resources.Issue : "");
				tname = tobj.GetDescription();
				ITableObjectCollection objects = ViewboxApplication.Database.SystemDb.CreateTableObjectCollection();
				objects.Add(tobj);
				job = Export.CreateUserDefined(objects, ViewboxSession.Optimizations.LastOrDefault(), project);
			}
			return PartialView("_DialogPartial", new DialogModel
			{
				DialogType = DialogModel.Type.Info,
				Key = job.Key,
				Title = string.Format(Resources.ExportTableLabel, ExportType.PDF.ToString(), ttype, tname),
				Content = string.Format(Resources.LongRunningDialogText, Resources.ExportData),
				Buttons = new List<DialogModel.Button>
				{
					new DialogModel.Button
					{
						Caption = Resources.LongRunningDialogCaption
					}
				}
			});
		}

		public PartialViewResult MassExport(string exports, ExportType type, string search, bool getAllFields, string filters = "")
		{
			if (filters != string.Empty)
			{
				ITableObjectCollection objects = ViewboxSession.GetMassExportObjects(exports);
				if (objects.Count > 0)
				{
					Export.Create(objects, ViewboxSession.Optimizations.LastOrDefault(), type, getAllFields, filters);
				}
				return ExportJobs(search);
			}
			ITableObjectCollection objects2 = ViewboxSession.GetMassExportObjects(exports);
			if (objects2.Count > 0)
			{
				Dictionary<int, IColumnCollection> dict = new Dictionary<int, IColumnCollection>();
				foreach (ITableObject to in objects2)
				{
					if (to.RowCount == 0)
					{
						IColumnCollection colList = ViewboxSession.GetColumnList(to, to.Id);
						dict.Add(to.Id, colList);
					}
				}
				foreach (KeyValuePair<int, IColumnCollection> item in dict)
				{
					if (objects2[item.Key].ColumnCount != 0)
					{
						continue;
					}
					foreach (IColumn coll in item.Value)
					{
						objects2[item.Key].Columns.Add(coll);
					}
				}
				if (objects2.Count == 1 && objects2.First().RowCount == 0)
				{
					Export.Create(objects2, ViewboxSession.Optimizations.LastOrDefault(), type, getAllFields);
				}
				else
				{
					Export.Create(objects2, ViewboxSession.Optimizations.LastOrDefault(), type, getAllFields);
				}
			}
			return ExportJobs(search);
		}

		public PartialViewResult RefreshJobs(string search)
		{
			return ExportJobs(search);
		}

		public PartialViewResult ExportTypeSelection(string exports)
		{
			List<string> allowedSchemes = new List<string>();
			allowedSchemes.Add("dsv_013");
			allowedSchemes.Add("dsv_030");
			allowedSchemes.Add("dsv_nettethal");
			ITableObjectCollection objects = ViewboxSession.GetMassExportObjects(exports);
			List<DialogModel.Button> buttons = new List<DialogModel.Button>();
			bool allowed = true;
			foreach (ITableObject item in objects)
			{
				if (!allowedSchemes.Contains(item.Database))
				{
					allowed = false;
				}
			}
			long maxRow = 0L;
			if (allowed || !ViewboxApplication.HasAnyDSVBug)
			{
				foreach (ITableObject o in objects)
				{
					long actRowCount = o.GetRowCount();
					if (actRowCount > maxRow)
					{
						maxRow = actRowCount;
					}
				}
				if (maxRow <= 50000)
				{
					buttons.Add(new DialogModel.Button
					{
						Caption = "PDF",
						Data = ExportType.PDF.ToString()
					});
				}
				buttons.Add(new DialogModel.Button
				{
					Caption = "Excel (CSV)",
					Data = ExportType.Excel.ToString()
				});
				buttons.Add(new DialogModel.Button
				{
					Caption = "GDPdU-Format",
					Data = ExportType.GDPdU.ToString()
				});
			}
			buttons.Add(new DialogModel.Button
			{
				Caption = Resources.Cancel
			});
			return PartialView("_DialogPartial", new DialogModel
			{
				DialogType = DialogModel.Type.Info,
				Title = Resources.ExportData,
				Content = ((maxRow > 50000) ? Resources.ExportTypeSelectionNoPDF : Resources.ExportTypeSelection),
				Buttons = buttons
			});
		}

		public ActionResult Download(string key)
		{
			Export export = Export.Find(key);
			if (export == null)
			{
				return RedirectToAction("Index", new
				{
					urlNotFound = true
				});
			}
			ViewboxSession.RemoveJob(key);
			CultureInfo culture = new CultureInfo(ViewboxSession.Language.CountryCode);
			string fname = "";
			if (string.IsNullOrEmpty(export.FileName))
			{
				string filename = "";
				filename = (export.IsOverviewPdf ? $"{Resources.Overview} {Resources.From} {export.StartTime.ToString(culture)}" : ((export.ExportObjects.Count <= 1) ? export.ExportObjects.First().GetDescription(ViewboxSession.Language) : $"{export.ExportObjects.Count} {Resources.Tables} {Resources.From} {export.StartTime.ToString(culture)}"));
				string regex = "[" + string.Join(",", Path.GetInvalidFileNameChars()) + "]";
				fname = export.ReplaceSpecialChars(Regex.Replace(filename, (regex ?? "") ?? "", "_")) + ".zip";
				Janitor.RegisterDownload(key);
				try
				{
					string filePath2 = ViewboxApplication.TemporaryDirectory + key + ".zip";
					if (!System.IO.File.Exists(filePath2))
					{
						return RedirectToAction("Index", new
						{
							urlNotFound = true
						});
					}
					return File(filePath2, "application/octet-stream", fname);
				}
				catch
				{
					return RedirectToAction("Index", new
					{
						urlNotFound = true
					});
				}
			}
			fname = export.FileName;
			Janitor.RegisterDownload(key);
			try
			{
				string filePath = ViewboxApplication.TemporaryDirectory + key + ".pdf";
				if (!System.IO.File.Exists(filePath))
				{
					return RedirectToAction("Index", new
					{
						urlNotFound = true
					});
				}
				return File(filePath, "application/pdf", fname);
			}
			catch
			{
				return RedirectToAction("Index", new
				{
					urlNotFound = true
				});
			}
		}

		public ActionResult RunningJobs()
		{
			var list = Export.List.Select((Export job) => new
			{
				key = job.Key,
				user = ViewboxSession.User.Name,
				start = $"{job.StartTime}",
				end = ((job.EndTime != DateTime.MinValue) ? $"{job.EndTime}" : null),
				time = $"{job.Runtime.Hours}:{job.Runtime.Minutes:00}:{job.Runtime.Seconds:00}",
				status = job.Status.ToString()
			});
			return Json(list, JsonRequestBehavior.AllowGet);
		}

		public ActionResult CancelOtherUserJob(string key, bool json = false)
		{
			Export job = Export.Find(key);
			if (job != null)
			{
				job.Owners.Clear();
				job.Cancel();
			}
			if (json)
			{
				return Json(job == null, JsonRequestBehavior.AllowGet);
			}
			return RedirectToAction("Index");
		}

		public ActionResult CancelJob(string key, bool json = false)
		{
			Export job = Export.Find(key);
			if (job != null)
			{
				if (job.Owners.Count == 1)
				{
					job.Cancel();
				}
				else
				{
					job.Owners.Remove(ViewboxSession.User);
				}
			}
			if (json)
			{
				return Json(job == null, JsonRequestBehavior.AllowGet);
			}
			return RedirectToAction("Index");
		}

		public PartialViewResult ExportJobs(string search)
		{
			ExportJobModel model = new ExportJobModel();
			if (search != null)
			{
				model.ExportJobs = Export.List.Where((Export j) => j.Descriptions[ViewboxSession.Language.CountryCode].ToLower().Contains(search.ToLower()));
			}
			else
			{
				model.ExportJobs = Export.List.Select((Export j) => j);
			}
			return PartialView("_ExportJobsPartial", model);
		}

		public void DeleteExport(string key)
		{
			Export export = Export.Find(key);
			if (export.Owners.Contains(ViewboxSession.User))
			{
				export.Owners.Remove(ViewboxSession.User);
			}
			if (export.Listeners.Contains(ViewboxSession.User))
			{
				export.Listeners.Remove(ViewboxSession.User);
			}
			if (export.JobShown.Contains(ViewboxSession.User))
			{
				export.JobShown.Remove(ViewboxSession.User);
			}
			if (ViewboxSession.User.IsSuper)
			{
				export.Owners.Clear();
				export.Listeners.Clear();
				export.JobShown.Clear();
			}
		}
	}
}
