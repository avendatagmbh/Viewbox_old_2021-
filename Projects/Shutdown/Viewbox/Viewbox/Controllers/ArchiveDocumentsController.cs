using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using SystemDb;
using SystemDb.Internal;
using DbAccess;
using Viewbox.Job;
using Viewbox.Models;
using Viewbox.Properties;
using ViewboxDb;
using ViewboxDb.Filters;

namespace Viewbox.Controllers
{
	[Authorize]
	[ViewboxFilter]
	public class ArchiveDocumentsController : BaseController
	{
		public ActionResult Index(int id = 0, long start = 0L)
		{
			IOptimization opt = ViewboxSession.Optimizations.FirstOrDefault();
			if (ViewboxSession.Archives.Count == 0 || opt == null || ViewboxApplication.HideDocumentsMenu || ViewboxSession.Archives.All((IArchive a) => a.Database.ToLower() != opt.FindValue(OptimizationType.System).ToLower()))
			{
				return RedirectToAction("Index", "Home");
			}
			ViewboxSession.ClearSavedDocumentSettings("ArchiveDocuments");
			ArchiveDocumentsModel model = new ArchiveDocumentsModel();
			model.RowsPerPage = 35;
			model.FromRow = start;
			PrepareParameters(model);
			if (id >= 0)
			{
				model.Table = ViewboxSession.ArchiveDocuments.FirstOrDefault();
				if (model.Table != null)
				{
					if (!ViewboxSession.TableObjects.Contains(model.Table.Id))
					{
						ViewboxSession.AddTableObject(model.Table.Id);
					}
					ViewboxSession.SetupTableColumns(model.Table.Id);
					model.Table = ViewboxSession.TableObjects[model.Table.Id];
					model.Documents = ViewboxApplication.Database.GetArchiveDocumentData(model.Table as IArchiveDocument, start);
					using DatabaseBase conn = ViewboxApplication.Database.ConnectionManager.GetConnection();
					try
					{
						model.RowsCount = conn.GetRowCount(conn.DbConfig.DbName, model.Table.TableName);
					}
					catch (Exception)
					{
					}
				}
			}
			else if (ViewboxSession.TempTableObjects[id] != null)
			{
				Janitor.RegisterTempObject(ViewboxSession.TempTableObjects[id].Key);
				model.Table = ViewboxSession.TempTableObjects[id].OriginalTable as IArchiveDocument;
				model.Documents = ViewboxApplication.Database.GetArchiveDocumentData(ViewboxSession.TempTableObjects[id], start);
				model.RowsCount = ViewboxSession.TempTableObjects[id].Table.RowCount;
				bool likeFilter = false;
				bool hasParameter = false;
				AndFilter filter = ViewboxSession.TempTableObjects[id].Filter as AndFilter;
				if (filter != null && model.FilterColumns != null)
				{
					int i;
					for (i = 0; i < model.FilterColumns.Length; i++)
					{
						ColValueFilter colFilter = filter.Conditions.FirstOrDefault((IFilter w) => w is ColValueFilter && ((ColValueFilter)w).Column.Name == model.FilterColumns[i].Name) as ColValueFilter;
						model.ParameterValues[i] = ((colFilter == null || colFilter.Value == null) ? "" : colFilter.Value.ToString());
						hasParameter |= !string.IsNullOrEmpty(model.ParameterValues[i]);
						if (colFilter != null && colFilter.Column.DataType == SqlType.DateTime)
						{
							if (model.ParameterValues[i].EndsWith("%"))
							{
								model.ParameterValues[i].Remove(model.ParameterValues[i].Length - 2);
							}
							if (DateTime.TryParseExact(model.ParameterValues[i], "yyyyMMddHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
							{
								model.ParameterValues[i] = date.ToString("dd/MM/yyyy");
							}
							else
							{
								model.ParameterValues[i] = "";
							}
						}
						if (colFilter != null && colFilter.Op == Operators.Like)
						{
							model.ParameterValues[i] = model.ParameterValues[i].TrimStart('%');
							model.ParameterValues[i] = model.ParameterValues[i].TrimEnd('%');
							likeFilter = true;
						}
					}
				}
				model.ExactMatch = hasParameter && !likeFilter;
			}
			if (model.Table != null && model.Table.Columns != null)
			{
				foreach (IColumn col in model.Table.Columns)
				{
					col.IsVisible = ViewboxApplication.Database.SystemDb.Columns[col.Id].IsVisible;
				}
			}
			ColValueFilter.ToDate = false;
			return View(model);
		}

		public ActionResult SortAndFilter(string[] parameterValues, bool ExactMatch, int sortColumnId = 0, SortDirection direction = SortDirection.Ascending)
		{
			ColValueFilter.ToDate = true;
			IOptimization opt = ViewboxSession.Optimizations.FirstOrDefault();
			if (opt == null || !ViewboxSession.ArchiveDocuments.Any())
			{
				return RedirectToAction("Index", "Home");
			}
			IArchiveDocument archive = ViewboxSession.ArchiveDocuments.FirstOrDefault();
			ArchiveDocumentsModel model = new ArchiveDocumentsModel();
			PrepareParameters(model);
			bool hasParameter = false;
			if (model.FilterColumns != null)
			{
				for (int i = 0; i < parameterValues.Length; i++)
				{
					parameterValues[i] = ((parameterValues[i] == null || parameterValues[i] == model.FilterColumnDescriptions[i]) ? "" : parameterValues[i]);
					hasParameter = hasParameter || !string.IsNullOrEmpty(parameterValues[i]);
				}
			}
			SortCollection sort = new SortCollection();
			if (sortColumnId > 0)
			{
				sort.Add(new Sort
				{
					cid = sortColumnId,
					dir = direction
				});
			}
			return RedirectToAction("SortAndFilter", "DataGrid", new
			{
				id = archive.Id,
				filter = ViewboxApplication.Database.PrepareArchiveDataParameters(parameterValues, ExactMatch),
				canReturnDialogPartial = false,
				sortListString = ((sortColumnId > 0) ? (sortColumnId + "|" + (int)direction) : "")
			});
		}

		private void PrepareParameters(ArchiveDocumentsModel model)
		{
			IArchiveDocument archive = ViewboxApplication.Database.SystemDb.ArchiveDocuments.FirstOrDefault();
			if (archive == null)
			{
				return;
			}
			using DatabaseBase connection = ViewboxApplication.Database.SystemDb.ConnectionManager.GetConnection();
			List<ColumnFilters> filterColumns = connection.DbMapping.Load<ColumnFilters>("table_id = " + archive.Id);
			model.FilterColumns = new IColumn[filterColumns.Count];
			int i = 0;
			foreach (ColumnFilters relationVisibleColumn in filterColumns)
			{
				IColumn relationColumn = ViewboxApplication.Database.SystemDb.Columns.FirstOrDefault((IColumn w) => w.Id == relationVisibleColumn.ColumnId);
				model.FilterColumns[i] = relationColumn;
				i++;
			}
			model.FilterColumnDescriptions = model.FilterColumns.Select((IColumn w) => w.GetDescription()).ToArray();
			model.ParameterValues = new string[model.FilterColumns.Length];
		}

		public ActionResult Filter(int id, int column, string filter)
		{
			return RedirectToAction("Filter", "DataGrid", new { id, column, filter });
		}

		public ActionResult Sort(int id, int tableId, SortDirection direction = SortDirection.Ascending)
		{
			return RedirectToAction("Sort", "DataGrid", new { id, tableId, direction });
		}

		public ActionResult Download(int id)
		{
			Transformation trans = Transformation.Create(id);
			return PartialView("_DialogPartial", new DialogModel
			{
				DialogType = DialogModel.Type.Info,
				Title = "Downloading blueline documents",
				Key = trans.Key,
				Content = string.Format(Resources.VeryLongRunningDialogText, "Downloading blueline documents"),
				Buttons = new List<DialogModel.Button>
				{
					new DialogModel.Button
					{
						Caption = Resources.LongRunningDialogCaption
					}
				}
			});
		}

		public FileResult OpenDocument(string search)
		{
			return File(search, "application/octet-stream", search);
		}
	}
}
