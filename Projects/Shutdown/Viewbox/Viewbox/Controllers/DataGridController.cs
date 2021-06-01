using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using SystemDb;
using SystemDb.Internal;
using AV.Log;
using DbAccess;
using log4net;
//using Microsoft.Ajax.Utilities;
using Viewbox.Job;
using Viewbox.Models;
using Viewbox.Properties;
using ViewboxDb;
using ViewboxDb.Filters;

namespace Viewbox.Controllers
{
	[Authorize]
	[ViewboxFilter]
	public class DataGridController : TableObjectControllerBase
	{
		private enum ParamError
		{
			NoError,
			CouldNotSave,
			LengthTooLong
		}

		private static readonly string[] _possibleColNameTemplates = new string[3] { "{0}", "SUM(`{0}`)", "CAST(`{0}` AS CHAR(255))" };

		internal ILog _log = LogHelper.GetLogger();

		public ActionResult TestOptimizationDelete(int optid)
		{
			OptimizationJob myJob = new OptimizationJob(optid);
			myJob.StartJob();
			return PartialView("_DialogPartial", new DialogModel
			{
				DialogType = DialogModel.Type.Info,
				Key = myJob.Key,
				Title = Resources.RemoveOptimization,
				Content = string.Format(Resources.LongRunningDialogText, Resources.AddingRelation),
				Buttons = new List<DialogModel.Button>
				{
					new DialogModel.Button
					{
						Caption = Resources.LongRunningDialogCaption
					}
				}
			});
		}

		public ActionResult TableIndexes(bool json = true, int id = 0, string columnName = "", string tableName = "", string databaseName = "")
		{
			return PartialView("_TableIndexPartial", TableIndexModel.Create(ViewboxApplication.Database.SystemDb.Objects.SingleOrDefault((ITableObject o) => o.Id == id), columnName));
		}

		[BackBtnFilter]
		[ViewboxFilter]
		public ActionResult Index(int id, int scheme = -1, int start = 0, int size = 0, bool json = false, bool useEnlargeProperty = true, bool resetColumnOrder = false, bool reset = false)
		{
			DataGridModel model = new DataGridModel();
			bool tmpOptChange = ViewboxSession.HasOptChanged;
			int temp_id = id;
			if (temp_id < 0)
			{
				temp_id = ((ViewboxSession.TempTableObjects[id].BaseTableId < 0) ? ViewboxSession.TempTableObjects[id].OriginalTable.Id : ViewboxSession.TempTableObjects[id].BaseTableId);
				if (temp_id > 0)
				{
					if (!ViewboxSession.TableObjects.Contains(temp_id))
					{
						ViewboxSession.AddTableObject(temp_id);
					}
					ViewboxSession.SetupTableColumns(temp_id);
				}
			}
			bool isEmpty;
			do
			{
				model = GenerateDataGridModel(id, out isEmpty, scheme, start, size, json, useEnlargeProperty, reset);
				if (isEmpty && !string.IsNullOrEmpty(ViewboxSession.CurrentTransactionKey) && ViewboxSession.CurrentlyRunningProcedures.Keys.Contains(ViewboxSession.CurrentTransactionKey))
				{
					Thread.Sleep(1000);
				}
			}
			while (isEmpty && !string.IsNullOrEmpty(ViewboxSession.CurrentTransactionKey) && ViewboxSession.CurrentlyRunningProcedures.Keys.Contains(ViewboxSession.CurrentTransactionKey));
			if (model.TableObject != null && model.TableObject.Sum != null)
			{
				if (model.TableObject.Sum.Count > 0)
				{
					model.FromRow = 1L;
					model.RowsCount = 1L;
					model.Summa = true;
				}
				else
				{
					model.Summa = false;
				}
			}
			if (isEmpty)
			{
				string lastUrl = ((base.Request.UrlReferrer == null) ? "/" : base.Request.UrlReferrer.AbsoluteUri);
				int originalId2 = 0;
				if (id < 0)
				{
					if (ViewboxSession.TempTableObjects[id].OriginalTable is Issue && (ViewboxSession.TempTableObjects[id].OriginalTable as Issue).IssueType == IssueType.StoredProcedure)
					{
						originalId2 = id + 1;
					}
					else if (ViewboxSession.TempTableObjects[id].OriginalTable is Table)
					{
						originalId2 = ViewboxSession.TempTableObjects[id].PreviousTableId;
					}
					else if (ViewboxSession.TempTableObjects[id].OriginalTable is View)
					{
						originalId2 = ViewboxSession.TempTableObjects[id].PreviousTableId;
						if (originalId2 > 0 && ViewboxSession.TempTableObjects[id].Sum != null && ViewboxSession.TempTableObjects[id].Sum.Count > 0 && ViewboxSession.TempTableObjects[id].Filter == null)
						{
							originalId2 = id;
						}
					}
					else
					{
						originalId2 = id;
					}
				}
				else
				{
					originalId2 = id;
				}
				if (lastUrl.Contains("DataGrid") && lastUrl.Contains("Index") && originalId2 != 0 && !ViewboxSession.RedirectionByCandCHappened)
				{
					if (originalId2 < 0)
					{
						if (!ViewboxSession.TempTableObjects[id].MultiOptimization)
						{
							OptimizationManager.CheckAfterExecution(ViewboxSession.TempTableObjects[originalId2]);
						}
					}
					else
					{
						OptimizationManager.SetBack();
					}
					if (model.Dialog != null)
					{
						Base.AddUserToJobShows(model.Dialog.Key);
					}
					if (model.TableObject != null)
					{
						IIssue iissueTemp = model.TableObject.Table as IIssue;
						if (tmpOptChange && iissueTemp != null && iissueTemp.IssueType == IssueType.StoredProcedure && ViewboxSession.TempTableObjects[originalId2] != null && ViewboxSession.TempTableObjects[originalId2].Table.RowCount == 0)
						{
							originalId2++;
						}
					}
					model = GenerateDataGridModel(originalId2, out isEmpty, scheme, start, size, json, useEnlargeProperty);
					if (model.TableObject != null)
					{
						IIssue iissueTemp2 = model.TableObject.Table as IIssue;
						if (tmpOptChange && iissueTemp2 != null && iissueTemp2.IssueType == IssueType.StoredProcedure && model.Dialog != null && model.RowsCount > 0)
						{
							model.IsThereEmptyMessage = false;
							ViewboxSession.IsThereOptChangeEmptyMessage = true;
							return (json || base.Request.IsAjaxRequest()) ? ((ViewResultBase)PartialView("_DataGridPartial", model)) : ((ViewResultBase)View(model));
						}
					}
					if (model.Dialog == null)
					{
						model.IsThereEmptyMessage = true;
					}
					else
					{
						ViewboxSession.IsThereEmptyMessage = true;
					}
				}
				else
				{
					if (!lastUrl.Contains("IssueList"))
					{
						if (originalId2 < 0)
						{
							OptimizationManager.CheckAfterExecution(ViewboxSession.TempTableObjects[originalId2]);
						}
						else
						{
							OptimizationManager.SetBack();
						}
					}
					ViewboxSession.IsThereEmptyMessage = true;
					if (lastUrl.Contains("IssueList") && lastUrl.Contains("optid"))
					{
						lastUrl = RegexReplace(lastUrl, "optid=(\\d+)&", "");
						lastUrl = RegexReplace(lastUrl, "\\?optid=(\\d+)", "");
					}
				}
				if (lastUrl.Contains("optid"))
				{
					lastUrl = RegexReplace(lastUrl, "optid=(\\d+)&", "");
					lastUrl = RegexReplace(lastUrl, "\\?optid=(\\d+)", "");
				}
				if (!lastUrl.Contains("?isNoResult=true") && !lastUrl.Contains("&isNoResult=true"))
				{
					lastUrl = (lastUrl.Contains("isNoResult=false") ? lastUrl.Replace("isNoResult=false", "isNoResult=true") : ((!lastUrl.Contains("?")) ? (lastUrl + "?isNoResult=true") : (lastUrl + "&isNoResult=true")));
				}
				ViewboxSession.RedirectionByCandCHappened = false;
				if (id < 0 && ViewboxSession.TempTableObjects[id].OriginalTable is Issue)
				{
					ViewboxSession.RedirectionByCandCHappened = true;
				}
				return Redirect(lastUrl);
			}
			ViewboxSession.RedirectionByCandCHappened = false;
			if (ViewboxSession.IsThereEmptyMessage.HasValue && ViewboxSession.IsThereEmptyMessage.Value)
			{
				if (model.RowsCount == 0)
				{
					model.IsThereEmptyMessage = true;
				}
				else
				{
					ViewboxSession.IsThereEmptyMessage = false;
				}
			}
			if (ViewboxSession.IsThereOptChangeEmptyMessage.HasValue && ViewboxSession.IsThereOptChangeEmptyMessage.Value)
			{
				ViewboxSession.IsThereOptChangeEmptyMessage = false;
				model.IsThereEmptyMessage = true;
			}
			foreach (Tuple<IFilter, IColumn, string, object, string> parameter in model.FilterParameters)
			{
				if (!(parameter.Item5 == "=") && !(parameter.Item5 == "≈"))
				{
					continue;
				}
				foreach (IColumn column in ViewboxSession.Columns.Where((IColumn c) => c.Name == parameter.Item2.Name))
				{
					if (parameter.Item5 == "=")
					{
						column.ExactMatchUnchecked = false;
					}
					else if (parameter.Item5 == "≈")
					{
						column.ExactMatchUnchecked = true;
					}
				}
			}
			if (model.SubtotalColumns != null && model.TableObject.Sum == null)
			{
				model = SubtotalGroupCalculation(model);
			}
			if (ViewboxSession.LastPositionUserViewList != null && ViewboxSession.User != null && ViewboxSession.User.SessionId != null && ViewboxSession.LastPositionUserViewList.ContainsKey(ViewboxSession.User.SessionId))
			{
				ViewboxSession.LastPositionUserViewList.Remove(ViewboxSession.User.SessionId);
			}
			if (ViewboxSession.LastPositionUserTableList != null && ViewboxSession.User != null && ViewboxSession.User.SessionId != null && ViewboxSession.LastPositionUserTableList.ContainsKey(ViewboxSession.User.SessionId))
			{
				ViewboxSession.LastPositionUserTableList.Remove(ViewboxSession.User.SessionId);
			}
			if (resetColumnOrder)
			{
				ViewboxDb.TableObject tobj = ViewboxSession.TempTableObjects[id];
				ITableObject obj = ((tobj != null) ? tobj.Table : ViewboxSession.TableObjects[id]);
				int originalId = ((tobj != null && tobj.OriginalTable != null) ? tobj.OriginalTable.Id : 0);
				ITableObject orTable = ((id > 0) ? ViewboxSession.TableObjects[id] : ViewboxSession.TempTableObjects[id].Table);
				List<Tuple<int, string>> columnOrder = ViewboxApplication.Database.SystemDb.ResetColumnOrder(obj, ViewboxSession.User, orTable, originalId);
				DataColumnCollection modelColumns = model.DataTable.Columns;
				if (model != null && model.TableObject != null && model.TableObject.Table != null && model.TableObject.Table.Columns != null && model.TableObject.OriginalColumns != null)
				{
					IColumnCollection tableObjectColumns = model.TableObject.Table.Columns;
					IColumnCollection originalColumns = model.TableObject.OriginalColumns;
					for (int i = 0; i < columnOrder.Count; i++)
					{
						string columnName = columnOrder[i].Item2;
						for (int k = 0; k < tableObjectColumns.Count; k++)
						{
							IColumn modelColumn2 = tableObjectColumns.ElementAt(k);
							if (columnName == modelColumn2.Name)
							{
								modelColumn2.Ordinal = columnOrder[i].Item1;
								break;
							}
						}
						for (int j = 0; j < originalColumns.Count; j++)
						{
							IColumn modelColumn = originalColumns.ElementAt(j);
							if (columnName == modelColumn.Name)
							{
								modelColumn.Ordinal = columnOrder[i].Item1;
								break;
							}
						}
					}
				}
			}
			return (json || base.Request.IsAjaxRequest()) ? ((ViewResultBase)PartialView("_DataGridPartial", model)) : ((ViewResultBase)View(model));
		}

		private string RegexReplace(string url, string pattern, string replacement)
		{
			Regex rgx = new Regex(pattern);
			return rgx.Replace(url, "");
		}

		private DataColumn GetColumnOrdinalSetForDataModel(DataColumnCollection cols, string colName)
		{
			int i;
			for (i = 0; i < _possibleColNameTemplates.Length && cols[string.Format(_possibleColNameTemplates[i], colName)] == null; i++)
			{
			}
			if (i < _possibleColNameTemplates.Length)
			{
				return cols[string.Format(_possibleColNameTemplates[i], colName)];
			}
			return null;
		}

		private DataGridModel GenerateDataGridModel(int id, out bool isEmpty, int scheme = -1, int start = 0, int size = 0, bool json = false, bool useEnlargeProperty = true, bool reset = false)
		{
			ViewboxSession.ClearSavedDocumentSettings("DataGrid");
			if (size != ViewboxSession.User.DisplayRowCount)
			{
				if (size != 0)
				{
					ViewboxSession.User.DisplayRowCount = size;
				}
				else
				{
					size = ViewboxSession.User.DisplayRowCount;
				}
			}
			ITableObject obj = ((id < 0) ? ViewboxSession.TempTableObjects[id].Table : ViewboxSession.TableObjects[id]);
			if (id < 0 && obj.Type == TableType.Issue)
			{
			}
			if (id < 0 && (ViewboxSession.TempTableObjects[id].Filter == null || (ViewboxSession.TempTableObjects[id].Filter as BinaryFilter).Conditions.Count == 0) && (ViewboxSession.TempTableObjects[id].Sort == null || (ViewboxSession.TempTableObjects[id].Sort != null && ViewboxSession.TempTableObjects[id].Sort.Count == 0)) && ViewboxSession.TempTableObjects[id].OriginalTable.Type == TableType.Issue)
			{
				Issue ot = ViewboxSession.TempTableObjects[id].OriginalTable as Issue;
				ot.ExecutedIssueId = id;
				if (ViewboxSession.TempTableObjects[id].Sum == null)
				{
					ot.OriginalId = id;
				}
			}
			if (!json)
			{
				ViewboxSession.SetupTableColumns(id);
			}
			if (id > 0 && (obj.Type == TableType.View || obj.Type == TableType.Table))
			{
				IProperty property = ViewboxApplication.FindProperty("table_order");
				if (property != null && property.Value.ToLower() == "true")
				{
					ITableObject tableObject3;
					if (obj.Type != TableType.View)
					{
						ITableObject tableObject2 = ViewboxSession.Tables[id];
						tableObject3 = tableObject2;
					}
					else
					{
						ITableObject tableObject2 = ViewboxSession.Views[id];
						tableObject3 = tableObject2;
					}
					obj = tableObject3;
					ViewboxApplication.Database.SystemDb.UpdateTableObjectOrder(ViewboxSession.User, obj.Type, obj);
				}
				obj = ViewboxSession.TableObjects[id];
			}
			if (id < 0)
			{
				Janitor.RegisterTempObject(ViewboxSession.TempTableObjects[id].Key);
			}
			DataGridModel model = new DataGridModel();
			model.TableInfo = obj;
			model.SelectedScheme = ((scheme > -1) ? obj.Schemes[scheme] : obj.DefaultScheme);
			if (useEnlargeProperty)
			{
				IProperty p = ViewboxApplication.FindProperty("enlarge_datagrid");
				if (p != null && p.Value == "true" && size < 40)
				{
					size = 40;
				}
			}
			model.RowsPerPage = size;
			model.FromRow = start;
			ViewboxDb.TableObject tobj = ViewboxSession.TempTableObjects[id];
			model.Sort = null;
			model.Filter = null;
			if (tobj != null)
			{
				model.TableObject = tobj;
				if (model.TableObject.Sum != null)
				{
					foreach (IColumn column4 in model.TableInfo.Columns)
					{
						if (model.TableObject.Sum.Contains(column4))
						{
							column4.IsVisible = true;
						}
						else
						{
							column4.IsVisible = false;
						}
					}
				}
				model.SapBalance = tobj.Additional as SapBalanceModel;
				model.SimpleTable = tobj.Additional as SimpleTableModel;
				model.StructuedTable = tobj.Additional as StructuedTableModel;
				model.UniversalTable = tobj.Additional as UniversalTableModel;
				if (model.UniversalTable != null && model.UniversalTable.IsDataGridNeeded)
				{
					model.SelectedScheme = ViewboxApplication.Database.SystemDb.Schemes[0];
					model.UniversalTable = null;
				}
				if (tobj.GroupSubTotal != null)
				{
					model.SubtotalColumns = new List<IColumn>();
					foreach (string pair2 in tobj.GroupSubTotal.ColumnList)
					{
						ITableObject table = tobj.Table;
						if (table != null)
						{
							IColumn column5 = table.Columns.FirstOrDefault((IColumn x) => x.Name == pair2);
							model.SubtotalColumns.Add((pair2 != string.Empty) ? column5 : null);
						}
					}
					model.GroupColumn = new List<IColumn>();
					foreach (string pair in tobj.GroupSubTotal.GroupList)
					{
						ITableObject table2 = tobj.Table;
						if (table2 != null)
						{
							IColumn column6 = table2.Columns.FirstOrDefault((IColumn x) => x.Name == pair);
							model.GroupColumn.Add((pair != string.Empty) ? column6 : null);
						}
					}
					tobj.Table.PageSystem.CountStep = 0L;
					model.RowsCount = ViewboxApplication.Database.TempDatabase.ComputePagedDataCount(tobj.Table);
					model.SubtotalOnlyExpectedResult = tobj.GroupSubTotal.OnlyExpectedResult;
				}
				PageSystem pageSystem = null;
				if (tobj.Table != null && tobj.Table.PageSystem != null)
				{
					pageSystem = tobj.Table.PageSystem;
					string orderBy = pageSystem.OrderBy;
					string[] orderByParts = orderBy.Split(',');
					if (orderByParts.Length == 2 && (orderByParts[0] == "" || orderByParts[0] == string.Empty))
					{
						pageSystem.OrderBy = orderBy.Replace(",", "");
					}
				}
				if (model.TableObject != null && model.TableObject.Sort != null)
				{
					SortCollection sortNow = model.TableObject.Sort;
					foreach (Sort s2 in sortNow)
					{
						IColumn temp = ViewboxSession.Columns[s2.cid];
						if (temp != null)
						{
							s2.cid = model.TableObject.Table.Columns[temp.Name].Id;
						}
					}
					tobj.Sort = sortNow;
					model.TableObject.Sort = tobj.Sort;
				}
				IIssue iissueTemp = model.TableObject.Table as IIssue;
				if (model.TableObject.ParamValues != null && iissueTemp != null && iissueTemp.IssueType == IssueType.StoredProcedure && tobj.HasOptimizationChanged())
				{
					model.Dialog = IssueReexecution(tobj);
					model.DataTable = ViewboxSession.LoadDataTable(tobj, ViewboxSession.Optimizations.LastOrDefault(), start, size, pageSystem);
					if (model.DataTable != null)
					{
						model.RowsCount = tobj.Table.RowCount;
					}
				}
				else if ((model.TableObject.Filter != null || model.TableObject.Sort != null) && ViewboxSession.HasOptChanged)
				{
					model.Sort = tobj.Sort;
					model.Filter = tobj.Filter;
					model.Dialog = SortAndFilterRetransformation(id, model.Sort, tobj.Filter);
					model.DataTable = ViewboxSession.LoadDataTable(tobj, tobj.Optimization, start, size, pageSystem);
					if (model.DataTable != null)
					{
						model.RowsCount = tobj.Table.RowCount;
					}
				}
				else if (tobj.GroupSubTotal != null)
				{
					model.DataTable = ViewboxSession.LoadDataTable(tobj, ViewboxSession.Optimizations.LastOrDefault(), start, size, pageSystem);
					if (model.DataTable != null)
					{
						model.RowsCount = ViewboxSession.GetDataCount(tobj.Table, model.TableObject.OriginalTable.Id, tobj.MultiOptimization, tobj.OptimizationSelected);
						model.RowsCount = ViewboxSession.GetSubtotalDataCount(tobj.GroupSubTotal.sql);
					}
				}
				else
				{
					model.DataTable = ViewboxSession.LoadDataTable(tobj, ViewboxSession.Optimizations.LastOrDefault(), start, size, pageSystem);
					if (model.DataTable != null)
					{
						model.RowsCount = ViewboxSession.GetDataCount(tobj.Table, model.TableObject.OriginalTable.Id, tobj.MultiOptimization, tobj.OptimizationSelected);
					}
				}
			}
			else
			{
				model.DataTable = ViewboxSession.LoadDataTable(obj, start, size);
				if (model.DataTable != null)
				{
					if (obj.Type != TableType.Issue)
					{
						model.RowsCount = ViewboxSession.GetDataCount(obj, model.TableInfo.Id);
					}
					else
					{
						model.RowsCount = ViewboxApplication.Database.TempDatabase.ComputeDataCount(obj);
					}
				}
			}
			if (model.DataTable == null)
			{
				model.DataTable = new DataTable();
				model.RowsCount = 0L;
			}
			else
			{
				if (model.RowsCount != 0)
				{
					model.RowsCount = Math.Max(model.DataTable.Rows.Count, model.RowsCount);
				}
				if (model.TableObject != null)
				{
					model.TableObject.Optimization = ViewboxSession.Optimizations.LastOrDefault();
				}
			}
			if (model.TableObject != null && model.TableObject.Table != null)
			{
				int tableId = model.TableObject.Table.Id;
				if (obj != null)
				{
					model.Crud = ViewboxApplication.TableCruds.GetCrudForTable(obj.Id);
					if (model.Crud == null)
					{
						IIssue issue = obj as IIssue;
						if (issue != null && issue.FilterTableObject != null)
						{
							model.Crud = ViewboxApplication.TableCruds.GetCrudForTable(issue.FilterTableObject.Id);
						}
					}
				}
			}
			if (model.TableObject != null && model.TableObject.OriginalTable != null)
			{
				model.TableDescription = model.TableObject.OriginalTable.GetDescription();
			}
			else
			{
				model.TableDescription = model.TableInfo.GetDescription();
			}
			if (model.TableObject != null && model.TableObject.Filter != null)
			{
				model.FilterParameters = model.TableObject.Filter.GetParameters(new List<Tuple<IFilter, IColumn, string, object, string>>());
			}
			string columnWidths = null;
			if (id >= 0 || ViewboxSession.TempTableObjects[id].Join == null)
			{
				int objId = ((id < 0) ? ViewboxSession.TempTableObjects[id].OriginalTable.Id : id);
				if (objId > 0)
				{
					if (!ViewboxSession.TableObjects.Contains(objId))
					{
						ViewboxApplication.Database.SystemDb.AddTableToTableObjects(ViewboxSession.User, ViewboxSession.TableObjects, objId);
					}
					columnWidths = ViewboxSession.GetColumnWidths(objId);
				}
			}
			model.ColumnWidths = columnWidths?.Split(',');
			if (model.ColumnWidths != null && model.ColumnWidths.Length <= model.TableInfo.Columns.Max((IColumn w) => w.Ordinal))
			{
				model.ColumnWidths = null;
			}
			int dynTableId = 0;
			int otherTableId = 0;
			ViewboxDb.TableObject tableobj = tobj;
			if (tobj != null)
			{
				dynTableId = tableobj.OriginalTable.Id;
				try
				{
					if (tableobj != null && tableobj.OriginalColumns != null)
					{
						IColumn column7 = tableobj.OriginalColumns.FirstOrDefault();
						otherTableId = ((column7 == null || column7.Table == null) ? (-1) : column7.Table.Id);
					}
					else
					{
						otherTableId = -1;
					}
				}
				catch (Exception)
				{
					otherTableId = -1;
				}
				if (otherTableId < 0)
				{
					otherTableId = dynTableId;
				}
			}
			ColumnCollection Columns = new ColumnCollection();
			List<IColumn> subtotalColumns = new List<IColumn>();
			if (tobj != null && tobj.GroupSubTotal != null && tobj.GroupSubTotal.ColumnList != null && tobj.GroupSubTotal.GroupList != null)
			{
				foreach (string item4 in tobj.GroupSubTotal.GroupList)
				{
					subtotalColumns.Add(obj.Columns.First((IColumn x) => x.Name == item4));
				}
				foreach (string item3 in tobj.GroupSubTotal.ColumnList)
				{
					subtotalColumns.Add(obj.Columns.First((IColumn x) => x.Name == item3));
				}
			}
			else
			{
				ITableObject tableObject = null;
				tableObject = ((tobj != null) ? ((tobj.Table.Type == TableType.Issue) ? ViewboxApplication.Database.SystemDb.Objects[dynTableId] : ViewboxApplication.Database.SystemDb.Objects[otherTableId]) : ((id >= 0) ? ViewboxApplication.Database.SystemDb.Objects[obj.Id] : ViewboxApplication.Database.SystemDb.Objects[dynTableId]));
				if (tableObject != null && tableObject.Columns != null)
				{
					Columns = (ColumnCollection)tableObject.Columns;
				}
			}
			IUserColumnOrderSettings userColOrder;
			if (id > 0)
			{
				userColOrder = ViewboxApplication.Database.SystemDb.UserColumnOrderSettings[ViewboxSession.User, model.TableInfo];
			}
			else if (tobj.Table.Type != TableType.Issue)
			{
				ITableObject tabObject2 = ViewboxApplication.Database.SystemDb.Objects[otherTableId];
				if (reset)
				{
					ViewboxApplication.Database.SystemDb.ReserColumnOrderOriginalTable(tabObject2, ViewboxSession.User);
				}
				userColOrder = ((tabObject2 == null) ? null : ViewboxApplication.Database.SystemDb.UserColumnOrderSettings[ViewboxSession.User, tabObject2]);
			}
			else
			{
				ITableObject tabObject = ViewboxApplication.Database.SystemDb.Objects[dynTableId];
				if (reset)
				{
					ViewboxApplication.Database.SystemDb.ReserColumnOrderOriginalTable(tabObject, ViewboxSession.User);
				}
				userColOrder = ((tabObject == null) ? null : ViewboxApplication.Database.SystemDb.UserColumnOrderSettings[ViewboxSession.User, tabObject]);
			}
			List<string> listOforder = userColOrder?.ColumnOrder.Split(',').ToList();
			List<Tuple<string, int>> columnName = new List<Tuple<string, int>>();
			if (tobj == null || tobj.GroupSubTotal == null || tobj.GroupSubTotal.ColumnList == null || tobj.GroupSubTotal.GroupList == null)
			{
				if (listOforder != null)
				{
					foreach (string item5 in listOforder)
					{
						IColumn column3 = Columns[int.Parse(item5)];
						if (column3 != null)
						{
							column3.Ordinal = listOforder.IndexOf(item5);
							columnName.Add(new Tuple<string, int>(column3.Name, column3.Ordinal));
							GetColumnOrdinalSetForDataModel(model.DataTable.Columns, column3.Name)?.SetOrdinal(column3.Ordinal);
						}
					}
				}
				Columns = (ColumnCollection)obj.Columns;
				foreach (Tuple<string, int> item2 in columnName)
				{
					for (int index = 0; index < Columns.Count; index++)
					{
						IColumn column2 = Columns.FirstOrDefault((IColumn c) => c.Name == item2.Item1);
						if (column2 != null)
						{
							column2.Ordinal = item2.Item2;
							GetColumnOrdinalSetForDataModel(model.DataTable.Columns, column2.Name)?.SetOrdinal(column2.Ordinal);
						}
					}
				}
			}
			int i = 1;
			try
			{
				i = ((listOforder == null || (tobj != null && tobj.GroupSubTotal != null)) ? 1 : 0);
			}
			catch (Exception)
			{
				i = 1;
			}
			List<IColumn> widthCalcColumns = ((Columns.Count > 0) ? Columns.ToList() : subtotalColumns);
			foreach (IColumn column in widthCalcColumns)
			{
				column.HeaderClass = "col_" + column.Ordinal + ((!column.IsVisible) ? " hide" : "");
				float genColWidth = 0f;
				if (ViewboxSession.UserDefinedColumnWidths != null)
				{
					UserDefinedWidth currentColumnWidthInfo = ViewboxSession.UserDefinedColumnWidths.Find((UserDefinedWidth item) => item.tableName == column.Table.Name && item.colName == column.Name);
					if (currentColumnWidthInfo != null)
					{
						column.UserDefinedWidth = currentColumnWidthInfo.newWidth;
					}
				}
				string s = "[" + column.Ordinal + "] " + (column.GetDescription() ?? column.Name);
				genColWidth = GetWidthOfString(s, new Font(new FontFamily("arial"), 10f, FontStyle.Bold, GraphicsUnit.Pixel)) + 50f;
				for (int j = 0; j < model.DataTable.Rows.Count; j++)
				{
					string content = model.DataTable.Rows[j][i].ToString();
					if (column.DataType == SqlType.Decimal)
					{
						content = Helper.GetDecimalFormat(column.MaxLength, content);
					}
					string[] rows = content.Split(new string[2] { "\r\n", "\n" }, StringSplitOptions.None);
					int maxLineWidth = rows.Max((string r) => (int)GetWidthOfString(r, new Font(new FontFamily("consolas"), 10f, GraphicsUnit.Point)) + 30);
					genColWidth = ((genColWidth >= (float)maxLineWidth) ? genColWidth : ((float)maxLineWidth));
				}
				column.OriginalWidth = (int)genColWidth;
				if (column.UserDefinedWidth == 0)
				{
					column.HeaderStyle = (((int)genColWidth > 1000) ? 1000 : ((int)genColWidth)).ToString(CultureInfo.InvariantCulture);
				}
				else
				{
					column.HeaderStyle = column.UserDefinedWidth.ToString(CultureInfo.InvariantCulture);
				}
				i++;
			}
			foreach (IColumn col in Columns.ToList())
			{
				foreach (IRelation relDict in col.Table.Relations.ToList())
				{
					foreach (IColumnConnection r2 in relDict)
					{
						if (!(r2.Target is IColumn))
						{
							continue;
						}
						int key = ((IColumn)r2.Target).Table.Id;
						if (key > 0)
						{
							if (!ViewboxSession.TableObjects.Contains(key))
							{
								ViewboxSession.AddTableObject(key);
							}
							if (ViewboxSession.Columns[r2.Target.Id] == null)
							{
								ViewboxSession.SetupTableColumns(key);
							}
						}
					}
				}
			}
			isEmpty = model != null && (((model.RowsCount == 0L || model.DataTable.Rows.Count == 0) && base.Request != null && base.Request.UrlReferrer != null && !string.IsNullOrEmpty(base.Request.UrlReferrer.AbsoluteUri)) || (model.UniversalTable != null && model.UniversalTable.HideEmptyNodes) || (model.SapBalance != null && model.SapBalance.AllNodes.Count <= 1) || (model.StructuedTable != null && model.StructuedTable.NodeCount <= 1) || (model.DataTable.Rows.Count == 1 && model.DataTable.Rows[0].ItemArray.Count((object t) => t != DBNull.Value) == 0) || (model.UniversalTable != null && model.UniversalTable.TreeRoot.Children.Count == 0));
			ViewboxSession.HasOptChanged = false;
			IUserTableTransactionIdSettings usertrans = ViewboxApplication.Database.SystemDb.UserTableTransactionIdSettings[model.TableInfo, ViewboxSession.User];
			if (usertrans != null)
			{
				model.TableInfo.TransactionNumber = usertrans.TransactionId;
			}
			return model;
		}

		private float GetWidthOfString(string str, Font font)
		{
			Bitmap objBitmap = null;
			Graphics objGraphics = null;
			objBitmap = new Bitmap(500, 200);
			objGraphics = Graphics.FromImage(objBitmap);
			SizeF stringSize = objGraphics.MeasureString(str, font);
			objBitmap.Dispose();
			objGraphics.Dispose();
			return stringSize.Width;
		}

		public void UpdateIsArchiveChecked(int id, bool is_archive_checked)
		{
			ViewboxSession.CheckedTableItems[id] = is_archive_checked;
		}

		public void ClearEmptyMessage()
		{
			ViewboxSession.IsThereEmptyMessage = false;
		}

		public ActionResult GetVisibilitySaveDialog()
		{
			return PartialView("_DialogPartial", new DialogModel
			{
				DialogType = DialogModel.Type.Info,
				Title = Resources.SaveVisibility,
				Content = Resources.SaveVisibilityDialog,
				Buttons = new List<DialogModel.Button>
				{
					new DialogModel.Button
					{
						Caption = Resources.Cancel,
						Id = "cancel-save"
					},
					new DialogModel.Button
					{
						Caption = Resources.SaveForMe,
						Id = "save-for-me"
					},
					new DialogModel.Button
					{
						Caption = Resources.SaveForEveryone,
						Id = "save-for-all"
					}
				}
			});
		}

		public ActionResult UpdateColumnArray(int[] id, bool[] is_visible, bool forAll = false, bool save = false)
		{
			for (int i = 0; i < id.Length; i++)
			{
				UpdateColumn(id[i], is_visible[i], forAll, save);
			}
			return Json(false, JsonRequestBehavior.AllowGet);
		}

		public ActionResult UpdateColumn(int id, bool is_visible, bool forAll = false, bool save = false)
		{
			if (!forAll)
			{
				GetColumnVisible(id, is_visible, save);
			}
			else
			{
				IUser user = ViewboxSession.User;
				if (user.IsSuper)
				{
					foreach (IUser u2 in ViewboxApplication.Database.SystemDb.Users)
					{
						if (ViewboxSession.User == u2)
						{
							GetColumnVisible(id, is_visible, save);
							continue;
						}
						int tempId2 = id;
						if (tempId2 < 0)
						{
							IColumn col2 = ViewboxSession.Columns[tempId2];
							string tableName2 = col2.Table.Database + "." + col2.Table.TableName;
							ITableObject table2 = ViewboxApplication.Database.SystemDb.Objects[tableName2];
							tempId2 = table2.Columns[col2.Name].Id;
						}
						ViewboxSession.UpdateColumn(tempId2, is_visible, u2, save);
					}
				}
				else
				{
					foreach (IRole r in user.Roles)
					{
						foreach (IUser u in r.Users)
						{
							if (ViewboxSession.User == u)
							{
								GetColumnVisible(id, is_visible, save);
								continue;
							}
							int tempId = id;
							if (tempId < 0)
							{
								IColumn col = ViewboxSession.Columns[tempId];
								string tableName = col.Table.Database + "." + col.Table.TableName;
								ITableObject table = ViewboxApplication.Database.SystemDb.Objects[tableName];
								tempId = table.Columns[col.Name].Id;
							}
							ViewboxSession.UpdateColumn(tempId, is_visible, u, save);
						}
					}
				}
			}
			return Json(false, JsonRequestBehavior.AllowGet);
		}

		public ActionResult UpdateColumns(int id, bool is_visible = false)
		{
			if (id < 0)
			{
				foreach (IColumn item2 in ViewboxSession.Columns)
				{
					GetColumnVisible(item2.Id, is_visible);
				}
			}
			else
			{
				foreach (IColumn item in ViewboxSession.Columns.Where((IColumn c) => c.Id > 0))
				{
					GetColumnVisible(item.Id, is_visible);
				}
			}
			return Json(false, JsonRequestBehavior.AllowGet);
		}

		private void GetColumnVisible(int id, bool is_visible, bool save = false)
		{
			int originalColumnId = 0;
			ITableObject table = null;
			if (!save)
			{
				return;
			}
			IColumn column = ViewboxSession.Columns[id];
			column.IsVisible = is_visible;
			string tableName = column.Table.Database + "." + column.Table.TableName;
			try
			{
				table = ViewboxSession.TableObjects[tableName];
				if (table == null || table.Columns == null || table.Columns.Count == 0)
				{
					table = ViewboxApplication.Database.SystemDb.Objects[tableName];
				}
				table.Columns[column.Name].IsVisible = is_visible;
			}
			catch (Exception)
			{
			}
			if (id > 0)
			{
				originalColumnId = id;
				ViewboxSession.UpdateColumn(id, is_visible, null, save);
			}
			else
			{
				ViewboxDb.TableObject temp = ViewboxSession.TempTableObjects.FirstOrDefault((ViewboxDb.TableObject t) => t.Table.Id == column.Table.Id);
				if (temp != null)
				{
					IColumn col = table.Columns[column.Name];
					originalColumnId = col.Id;
					col.IsVisible = is_visible;
					if (ViewboxSession.Columns.Contains(col))
					{
						ViewboxSession.UpdateColumn(col.Id, is_visible);
					}
					col = temp.Table.Columns[id];
					col.IsVisible = is_visible;
					ViewboxSession.UpdateColumn(originalColumnId, is_visible, null, save);
				}
			}
			if (originalColumnId == 0)
			{
				return;
			}
			IEnumerable<ViewboxDb.TableObject> temps = ViewboxSession.TempTableObjects.Where((ViewboxDb.TableObject t) => t.OriginalTable.Columns.Contains(originalColumnId));
			if (temps == null)
			{
				return;
			}
			foreach (ViewboxDb.TableObject t2 in temps)
			{
				foreach (IColumn c in t2.Table.Columns)
				{
					if (ViewboxSession.Columns[originalColumnId] != null && c.Name == ViewboxSession.Columns[originalColumnId].Name)
					{
						c.IsVisible = is_visible;
					}
				}
			}
		}

		[HttpGet]
		public ActionResult UpdateColumnText(int tableId, int columnId, string newName)
		{
			int originalTableId = tableId;
			int originalColumnId = columnId;
			ITableObject obj = ((tableId < 0) ? ViewboxSession.TempTableObjects[tableId].Table : ViewboxSession.TableObjects[tableId]);
			if (tableId < 0)
			{
				string colName = obj.Columns[columnId].Name;
				originalTableId = ViewboxSession.TempTableObjects[tableId].OriginalTable.Id;
				originalColumnId = ViewboxApplication.Database.SystemDb.Columns.SingleOrDefault((IColumn c) => c.Name == colName && c.Table.Name == obj.Name).Id;
			}
			obj.Columns[columnId].SetDescription(newName, ViewboxSession.Language);
			ViewboxApplication.Database.SystemDb.Columns[originalColumnId].SetDescription(newName, ViewboxSession.Language);
			TableAndColumnSettingsModel model = TableAndColumnSettingsModel.Instance;
			if (model.UpdateColumn(originalColumnId, newName))
			{
				return Json(true, JsonRequestBehavior.AllowGet);
			}
			return Json(false, JsonRequestBehavior.AllowGet);
		}

		[HttpGet]
		public ActionResult GetNewColumnWidth(int tableId, int columnId)
		{
			ViewboxDb.TableObject obj = ViewboxSession.TempTableObjects[tableId];
			ITableObject table = ((tableId < 0) ? obj.Table : ViewboxSession.TableObjects[tableId]);
			IColumn col = table.Columns.First((IColumn item) => item.Id == columnId);
			bool isEmpty = false;
			DataGridModel model = GenerateDataGridModel(tableId, out isEmpty);
			return Json(col.OriginalWidth, JsonRequestBehavior.AllowGet);
		}

		[HttpGet]
		public ActionResult UpdateTableText(int tableId, string newName)
		{
			TableAndColumnSettingsModel model = TableAndColumnSettingsModel.Instance;
			if (model.UpdateTable(tableId, newName))
			{
				return Json(true, JsonRequestBehavior.AllowGet);
			}
			return Json(false, JsonRequestBehavior.AllowGet);
		}

		public PartialViewResult ColumnList(int id)
		{
			if (!ViewboxSession.TableObjects.Contains(id))
			{
				ViewboxSession.AddTableObject(id);
			}
			ViewboxSession.SetupTableColumns(id);
			ITableObject tobj = ViewboxSession.TableObjects[id];
			return PartialView("_ColumnListPartial", tobj);
		}

		public PartialViewResult TableObjectList(TableType type, int page = 0, string search = "")
		{
			TableObjectList model = new TableObjectList();
			model.PerPage = 25;
			model.CurrentPage = page;
			model.Type = type;
			model.Categories = ViewboxApplication.Database.SystemDb.GetCategoryCollection();
			string system = ViewboxSession.SelectedSystem.ToLower();
			foreach (ICategory cat in ViewboxApplication.Database.SystemDb.Categories)
			{
				ICategory c = cat.Clone() as ICategory;
				List<ITableObject> list = new List<ITableObject>();
				IUser user = ViewboxSession.User;
				list.AddRange(from t in cat.TableObjects
					where t.Type == model.Type && t.Database.ToLower() == system && !t.IsArchived && (string.IsNullOrEmpty(search) || t.GetDescription().Contains(search)) && ((ViewboxApplication.Database.SystemDb.UserTableObjectSettings[user, t] != null) ? ViewboxApplication.Database.SystemDb.UserTableObjectSettings[user, t].IsVisible : t.IsVisible)
					orderby t.GetDescription()
					select t);
				model.Count += list.Count;
				list = list.GetRange(model.From, Math.Min(list.Count - model.From, model.PerPage));
				foreach (ITableObject i in list)
				{
					c.TableObjects.Add(i);
				}
				model.Categories.Add(c);
			}
			return PartialView("_TableObjectListPartial", model);
		}

		private ActionResult WrongParameterType(string value, string parType)
		{
			return PartialView("_DialogPartial", new DialogModel
			{
				DialogType = DialogModel.Type.Warning,
				Title = Resources.WrongParameterType,
				Content = string.Format(Resources.WrongParameterText, value, parType),
				Buttons = new List<DialogModel.Button>
				{
					new DialogModel.Button
					{
						Caption = Resources.OK
					}
				}
			});
		}

		private ActionResult ValueOutOfRange(string value, string minVal, string maxVal)
		{
			string messageContent = string.Empty;
			messageContent = ((minVal != null && maxVal != null) ? string.Format(Resources.OutOfRange, value, minVal, maxVal) : ((minVal == null) ? string.Format(Resources.OutOfMaxRange, value, maxVal) : string.Format(Resources.OutOfMinRange, value, minVal)));
			return PartialView("_DialogPartial", new DialogModel
			{
				DialogType = DialogModel.Type.Warning,
				Title = Resources.OutOfRangeTitle,
				Content = messageContent,
				Buttons = new List<DialogModel.Button>
				{
					new DialogModel.Button
					{
						Caption = Resources.OK
					}
				}
			});
		}

		private ParamError DeleteHistory(IParameter parameter)
		{
			bool deleteTheParam = ViewboxApplication.Database.SystemDb.DeleteParameterFreeSelectionHistory(ViewboxSession.User.Id, parameter.Issue.Id);
			return ParamError.NoError;
		}

		private ParamError SaveValueHistoryFreeSelection(string value, int selectionType, ref IIssue issue, int parId)
		{
			IParameter parameter = issue.Parameters.SingleOrDefault((IParameter p) => p.Id == parId);
			if (value.Equals(Resources.LabelEnterValue))
			{
				value = string.Empty;
			}
			if (parameter.HistoryFreeSelectionValues == null || parameter.HistoryFreeSelectionValues.Count > 0)
			{
				parameter.HistoryFreeSelectionValues = new List<IHistoryParameterValueFreeSelection>();
			}
			parameter.HistoryFreeSelectionValues.Add(ViewboxApplication.Database.SystemDb.AddParameterFreeSelectionHistory(ViewboxSession.User.Id, parameter.Id, parameter.Issue.Id, value, selectionType));
			return ParamError.NoError;
		}

		private ParamError SaveValueInHistory(string value, IParameter parameter)
		{
			if (value.Equals(Resources.LabelEnterValue))
			{
				value = string.Empty;
			}
			if (parameter.HistoryValues == null)
			{
				parameter.HistoryValues = new HistoryParameterValueCollection();
			}
			bool savingTheParam = ViewboxApplication.Database.SystemDb.AddParameterHistory(parameter.HistoryValues, ViewboxSession.User.Id, parameter.Id, value);
			return ParamError.NoError;
		}

		public JsonResult GetSelectionTypeConcat()
		{
			string result = ViewboxApplication.SelectionTypeConcat;
			return Json(result, JsonRequestBehavior.AllowGet);
		}

		[ValidateInput(false)]
		public string ValidateIssueParameterInput(int id, int parameterId, string value)
		{
			try
			{
				ITableObject tableObject = ViewboxSession.TableObjects[id];
				IIssue issue = ViewboxSession.TableObjects[id] as IIssue;
				IParameter parameter = issue.Parameters[parameterId];
				if (string.IsNullOrEmpty(parameter.ColumnName) || string.IsNullOrWhiteSpace(parameter.ColumnName))
				{
					return "";
				}
				string tempParam = value.Replace(",", "");
				if (string.IsNullOrEmpty(tempParam) || tempParam == Resources.LabelEnterValue)
				{
					return "";
				}
				SqlType[] checkTypes = new SqlType[6]
				{
					SqlType.Decimal,
					SqlType.Numeric,
					SqlType.Integer,
					SqlType.DateTime,
					SqlType.Date,
					SqlType.Time
				};
				ViewboxSession.SetupTableColumns(id);
				if (checkTypes.Contains(parameter.DataType))
				{
					string minValue = (tableObject.Columns.Contains(parameter.ColumnName) ? tableObject.Columns[parameter.ColumnName].MinValue : null);
					string maxValue = (tableObject.Columns.Contains(parameter.ColumnName) ? tableObject.Columns[parameter.ColumnName].MaxValue : null);
					if (string.IsNullOrEmpty(minValue) && string.IsNullOrEmpty(maxValue))
					{
						return "";
					}
					bool minValid = true;
					bool maxValid = true;
					switch (parameter.DataType)
					{
					case SqlType.Decimal:
					case SqlType.Numeric:
					{
						double minDouble = 0.0;
						double maxDouble = 0.0;
						if (!double.TryParse(tempParam, out var valueDoubleToCheck))
						{
							return string.Format(Resources.WrongParameterText, value, parameter.DataType.ToString());
						}
						if (minValue != null)
						{
							minValid = double.TryParse(minValue, out minDouble);
						}
						if (maxValue != null)
						{
							maxValid = double.TryParse(maxValue, out maxDouble);
						}
						if (!minValid || !maxValid)
						{
							return string.Format(Resources.WrongParameterText, value, parameter.DataType.ToString());
						}
						if ((minValue != null && valueDoubleToCheck < minDouble) || (maxValue != null && valueDoubleToCheck > maxDouble))
						{
							if (minValue != null && maxValue != null)
							{
								return string.Format(Resources.OutOfRange, value, minDouble.ToString(), maxDouble.ToString());
							}
							if (minValue != null)
							{
								return string.Format(Resources.OutOfMinRange, value, minDouble.ToString());
							}
							return string.Format(Resources.OutOfMaxRange, value, maxDouble.ToString());
						}
						break;
					}
					case SqlType.Integer:
					{
						long minInt = 0L;
						long maxInt = 0L;
						if (!long.TryParse(tempParam, out var valueIntToCheck))
						{
							return string.Format(Resources.WrongParameterText, value, parameter.DataType.ToString());
						}
						if (minValue != null)
						{
							minValid = long.TryParse(minValue, out minInt);
						}
						if (maxValue != null)
						{
							maxValid = long.TryParse(maxValue, out maxInt);
						}
						if (!minValid || !maxValid)
						{
							return string.Format(Resources.WrongParameterText, value, parameter.DataType.ToString());
						}
						if ((minValue != null && valueIntToCheck < minInt) || (maxValue != null && valueIntToCheck > maxInt))
						{
							if (minValue != null && maxValue != null)
							{
								return string.Format(Resources.OutOfRange, value, minInt.ToString(), maxInt.ToString());
							}
							if (minValue != null)
							{
								return string.Format(Resources.OutOfMinRange, value, minInt.ToString());
							}
							return string.Format(Resources.OutOfMaxRange, value, maxInt.ToString());
						}
						break;
					}
					case SqlType.Date:
					{
						DateTime minDate = DateTime.MaxValue;
						DateTime maxDate = DateTime.MaxValue;
						DateTime valueDateCheck = DateTime.MaxValue;
						if (!DateTime.TryParse(tempParam, out valueDateCheck) && !DateTime.TryParseExact(tempParam, "d", CultureInfo.InvariantCulture, DateTimeStyles.None, out valueDateCheck))
						{
							return string.Format(Resources.WrongParameterText, value, parameter.DataType.ToString());
						}
						if (minValue != null)
						{
							minValid = DateTime.TryParse(minValue, out minDate);
							if (!minValid)
							{
								minValid = DateTime.TryParseExact(minValue, "d", CultureInfo.InvariantCulture, DateTimeStyles.None, out minDate);
								if (!minValid)
								{
									minValid = DateTime.TryParseExact(minValue, "yyyy.MM.dd.", CultureInfo.InvariantCulture, DateTimeStyles.None, out minDate);
								}
							}
						}
						if (maxValue != null)
						{
							maxValid = DateTime.TryParse(maxValue, out maxDate);
							if (!maxValid)
							{
								maxValid = DateTime.TryParseExact(maxValue, "d", CultureInfo.InvariantCulture, DateTimeStyles.None, out maxDate);
								if (!maxValid)
								{
									maxValid = DateTime.TryParseExact(maxValue, "yyyy.MM.dd.", CultureInfo.InvariantCulture, DateTimeStyles.None, out maxDate);
								}
							}
						}
						if (!minValid || !maxValid)
						{
							return string.Format(Resources.WrongParameterText, value, parameter.DataType.ToString());
						}
						if ((minValue != null && valueDateCheck.Ticks < minDate.Ticks) || (maxValue != null && valueDateCheck.Ticks > maxDate.Ticks))
						{
							if (minValue != null && maxValue != null)
							{
								return string.Format(Resources.OutOfRange, value, minDate.ToString("d"), maxDate.ToString("d"));
							}
							if (minValue != null)
							{
								return string.Format(Resources.OutOfMinRange, value, minDate.ToString("d"));
							}
							return string.Format(Resources.OutOfMaxRange, value, maxDate.ToString("d"));
						}
						break;
					}
					case SqlType.Time:
					case SqlType.DateTime:
					{
						DateTime minDateTime = DateTime.MaxValue;
						DateTime maxDateTime = DateTime.MaxValue;
						DateTime valueDateTimeCheck = DateTime.MaxValue;
						if (!DateTime.TryParse(tempParam, out valueDateTimeCheck))
						{
							return string.Format(Resources.WrongParameterText, value, parameter.DataType.ToString());
						}
						if (minValue != null)
						{
							minValid = DateTime.TryParse(minValue, out minDateTime);
							if (!minValid)
							{
								minValid = DateTime.TryParseExact(minValue, "u", CultureInfo.InvariantCulture, DateTimeStyles.None, out minDateTime);
								if (!minValid)
								{
									minValid = DateTime.TryParseExact(minValue, "d", CultureInfo.InvariantCulture, DateTimeStyles.None, out minDateTime);
								}
							}
						}
						if (maxValue != null)
						{
							minValid = DateTime.TryParse(minValue, out minDateTime);
							if (!minValid)
							{
								maxValid = DateTime.TryParseExact(maxValue, "u", CultureInfo.InvariantCulture, DateTimeStyles.None, out maxDateTime);
								if (!minValid)
								{
									maxValid = DateTime.TryParseExact(maxValue, "d", CultureInfo.InvariantCulture, DateTimeStyles.None, out maxDateTime);
								}
							}
						}
						if (!minValid || !maxValid)
						{
							return string.Format(Resources.WrongParameterText, value, parameter.DataType.ToString());
						}
						if ((minValue != null && valueDateTimeCheck.Ticks < minDateTime.Ticks) || (maxValue != null && valueDateTimeCheck.Ticks > maxDateTime.Ticks))
						{
							if (minValue != null && maxValue != null)
							{
								return string.Format(Resources.OutOfRange, value, minDateTime.ToString(CultureInfo.InvariantCulture), maxDateTime.ToString(CultureInfo.InvariantCulture));
							}
							return (minValue != null) ? string.Format(Resources.OutOfMinRange, value, minDateTime.ToString(CultureInfo.InvariantCulture)) : string.Format(Resources.OutOfMaxRange, value, maxDateTime.ToString(CultureInfo.InvariantCulture));
						}
						break;
					}
					}
				}
				return "";
			}
			catch (Exception ex)
			{
				using (NDC.Push(LogHelper.GetNamespaceContext()))
				{
					_log.Error(ex.Message, ex);
				}
				return ex.Message;
			}
		}

		[ValidateInput(false)]
		public ActionResult ExecuteCountTable(int id, int start = 0, int size = 0, bool documents = false)
		{
			if (documents)
			{
				return RedirectToAction("ExecuteCountTable", "Documents", new { id, start, size });
			}
			if (size != 0)
			{
				ViewboxSession.User.DisplayRowCount = size;
			}
			else
			{
				size = ViewboxSession.User.DisplayRowCount;
			}
			ITableObject obj = ((id < 0) ? ViewboxSession.TempTableObjects[id].Table : ViewboxSession.TableObjects[id]);
			DataGridModel model = new DataGridModel();
			model.TableInfo = obj;
			model.RowsPerPage = size;
			model.FromRow = start;
			ViewboxDb.TableObject tobj = ViewboxSession.TempTableObjects[id];
			if (tobj == null)
			{
				model.RowsCount = ((model.TableInfo.Type != TableType.Issue) ? ViewboxSession.GetDataCount(model.TableInfo) : ViewboxApplication.Database.TempDatabase.ComputeDataCount(model.TableInfo));
			}
			else
			{
				model.TableObject = tobj;
				if (tobj.Table != null && tobj.Table.PageSystem != null)
				{
					PageSystem ps = tobj.Table.PageSystem;
					if (ps.IsEnded)
					{
						model.RowsCount = ps.Count;
						ps.CountStep = 0L;
					}
					else
					{
						ps.CountStep = 0L;
						model.RowsCount = ViewboxApplication.Database.TempDatabase.ComputePagedDataCount(tobj.Table);
					}
				}
				else
				{
					model.RowsCount = ((tobj.Table != null) ? tobj.Table.RowCount : 0);
				}
			}
			ViewboxApplication.Database.SystemDb.SetRowCount(tobj.Table, model.RowsCount);
			ViewboxApplication.Database.SystemDb.IsCalculated = true;
			return PartialView("_NavigationControlsPartial", model);
		}

		[HttpGet]
		public ActionResult BuildDynamicClickAndClickDialog(int id, int rowNo, List<int> sourceColumnId, List<int> targetColumnId)
		{
			IIssue issue = ViewboxApplication.Database.SystemDb.Issues.SingleOrDefault((IIssue i) => i.Id == id);
			List<string> param = new List<string>();
			for (int j = 0; j < sourceColumnId.Count; j++)
			{
				string value = DataGridModel.GetClickAndClickValue(rowNo, sourceColumnId[j]);
				param.Add(value);
			}
			return ExecuteIssueForClickAndClick(id, param, targetColumnId);
		}

		[ValidateInput(false)]
		public ActionResult ExecuteIssueForClickAndClick(int id, List<string> param, List<int> itemId, List<int> columnId = null, List<string> filterValue = null)
		{
			IIssue issue = ViewboxSession.TableObjects[id] as IIssue;
			List<int> selectionType = new List<int>();
			List<string> parametersForClickNClick = new List<string>();
			List<int> itemIdsForClickNClick = new List<int>();
			foreach (IParameter p in issue.Parameters)
			{
				if (!itemId.Contains(p.Id))
				{
					parametersForClickNClick.Add("");
					itemIdsForClickNClick.Add(p.Id);
				}
				else
				{
					int j = itemId.IndexOf(p.Id);
					parametersForClickNClick.Add(param[j]);
					itemIdsForClickNClick.Add(p.Id);
				}
				selectionType.Add(0);
			}
			IFilter f = null;
			if (columnId != null && filterValue != null)
			{
				StringBuilder sfFilter = new StringBuilder();
				for (int i = 0; i < columnId.Count; i++)
				{
					sfFilter.Append($"Equal(%{columnId[i]},\"{filterValue[i]}\"),");
				}
				try
				{
					f = ViewboxSession.GetFilter(id, $"And({sfFilter.ToString()})", ViewboxSession.TempTableObjects, ViewboxSession.TableObjects);
				}
				catch (Exception)
				{
					f = null;
				}
			}
			return ExecuteIssue(id, parametersForClickNClick, itemIdsForClickNClick, selectionType, null, isClickAndClick: true);
		}

		[ValidateInput(false)]
		public ActionResult ExecuteIssue(int id, List<string> param, List<int> itemId, List<int> selectionType, AndFilter filter = null, bool isClickAndClick = false)
		{
			ViewboxSession.SaveStartLoadingTime();
			ViewboxApplication.Database.SystemDb.IsReport = false;
			List<string> displayValues = new List<string>();
			Regex regex = new Regex("^[0-9]*$");
			IIssue issue = ViewboxSession.TableObjects[id] as IIssue;
			bool hasSelectionLogic = issue != null && issue.Logic != null;
			if (issue == null)
			{
				issue = ViewboxApplication.Database.SystemDb.Issues.SingleOrDefault((IIssue t) => t.Id == id);
			}
			ViewboxSession.isCurrentDynamic = false;
			if (issue.IssueType == IssueType.StoredProcedure)
			{
				ViewboxSession.isCurrentDynamic = true;
			}
			for (int i3 = 0; i3 < itemId.Count; i3++)
			{
				if (itemId.Count == param.Count)
				{
					SaveValueInHistory(param[i3], issue.Parameters[itemId[i3]]);
				}
			}
			List<string> paramToBeFilled = new List<string>();
			int i2;
			for (i2 = 0; i2 < issue.Parameters.Count; i2++)
			{
				if (issue.Parameters.Single((IParameter x) => x.Id == itemId[i2]).IsRequired == 1 && (string.IsNullOrWhiteSpace(param[i2]) || param[i2].Contains(Resources.LabelEnterValue)))
				{
					paramToBeFilled.Add(issue.Parameters.Single((IParameter x) => x.Id == itemId[i2]).GetDescription());
				}
			}
			if (paramToBeFilled.Count > 0)
			{
				return PartialView("_DialogPartial", new DialogModel
				{
					DialogType = DialogModel.Type.Warning,
					Title = Resources.DynamicViewParamRequiredCaption,
					Content = string.Format(Resources.DynamicViewParamRequiredText, string.Join(", ", paramToBeFilled)),
					Buttons = new List<DialogModel.Button>
					{
						new DialogModel.Button
						{
							Caption = Resources.OK
						}
					}
				});
			}
			List<string> paramToBeOptionallyFilled = new List<string>();
			int n;
			for (n = 0; n < issue.Parameters.Count; n++)
			{
				if (issue.Parameters.Single((IParameter x) => x.Id == itemId[n]).IsRequired == 1 && (string.IsNullOrWhiteSpace(param[n]) || param[n].Contains(Resources.LabelEnterValue)))
				{
					paramToBeFilled.Add(issue.Parameters.Single((IParameter x) => x.Id == itemId[n]).GetDescription());
				}
				if (issue.Parameters.Single((IParameter x) => x.Id == itemId[n]).OptionallyRequired == 1 && (string.IsNullOrWhiteSpace(param[n]) || param[n].Contains(Resources.LabelEnterValue)))
				{
					paramToBeOptionallyFilled.Add(issue.Parameters.Single((IParameter x) => x.Id == itemId[n]).GetDescription());
				}
			}
			if (paramToBeOptionallyFilled.Count == issue.Parameters.Count((IParameter p) => p.OptionallyRequired == 1) && issue.Parameters.Count((IParameter p) => p.OptionallyRequired == 1) != 0)
			{
				return PartialView("_DialogPartial", new DialogModel
				{
					DialogType = DialogModel.Type.Warning,
					Title = Resources.DynamicViewParamRequiredCaption,
					Content = string.Format(Resources.DynamicViewParamRequiredText2, string.Join(", ", paramToBeOptionallyFilled)),
					Buttons = new List<DialogModel.Button>
					{
						new DialogModel.Button
						{
							Caption = Resources.OK
						}
					}
				});
			}
			if (hasSelectionLogic && !Validate(issue, param, itemId))
			{
				return PartialView("_DialogParameterLogicText", new DialogModel
				{
					DialogType = DialogModel.Type.Warning,
					Title = Resources.InvalidSelection,
					Content = issue.Logic.GetWarningText(ViewboxSession.Language.CountryCode),
					Buttons = new List<DialogModel.Button>
					{
						new DialogModel.Button
						{
							Caption = Resources.OK
						}
					}
				});
			}
			IProperty property = ViewboxApplication.FindProperty("table_order");
			if (property != null && property.Value.ToLower() == "true")
			{
				issue = ViewboxSession.Issues[id];
				if (issue == null)
				{
					issue = ViewboxSession.TableObjects[id] as IIssue;
					if (issue == null)
					{
						_log.Error("Issue not exists fo user: " + id);
						return RedirectToAction("Index", "IssueList");
					}
				}
				ViewboxApplication.Database.SystemDb.UpdateTableObjectOrder(ViewboxSession.User, TableType.Issue, issue);
			}
			string tempParam = string.Empty;
			IOptimization opt = ViewboxSession.Optimizations.LastOrDefault();
			if (issue.NeedBukrs)
			{
				IOptimization split = global::ViewboxDb.ViewboxDb.GetOptimizationForType(opt, OptimizationType.SplitTable);
				if (split != null && split.Value == null)
				{
					return PartialView("_DialogPartial", new DialogModel
					{
						DialogType = DialogModel.Type.Warning,
						Title = Resources.Canceled,
						Content = string.Format(Resources.NeedBukrs, (split.Group == null) ? "Company Code" : split.Group.GetName(), split.GetValue()),
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
			if (issue.NeedGJahr)
			{
				IOptimization sort = global::ViewboxDb.ViewboxDb.GetOptimizationForType(opt, OptimizationType.SortColumn);
				if (sort != null && sort.Value == null)
				{
					return PartialView("_DialogPartial", new DialogModel
					{
						DialogType = DialogModel.Type.Warning,
						Title = Resources.Canceled,
						Content = string.Format(Resources.NeedGJahr, (sort.Group == null) ? "Year" : sort.Group.GetName(), sort.GetValue()),
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
			object[] paramValues = new object[param.Count];
			IParameter lastParameter = null;
			int j2 = 0;
			int m;
			object[] array;
			for (m = 0; m < param.Count; m++)
			{
				IParameter par = issue.Parameters.Single((IParameter x) => x.Id == itemId[m]);
				if (lastParameter != null && par.GroupId == lastParameter.GroupId && par.GroupId != 0 && lastParameter.Id != par.Id && par.Ordinal > lastParameter.Ordinal)
				{
					j2--;
				}
				int select = 0;
				try
				{
					select = selectionType?[j2] ?? 0;
				}
				catch (Exception)
				{
					select = 0;
				}
				switch (par.DataType)
				{
				case SqlType.Integer:
				{
					if (string.IsNullOrEmpty(param[m]) || param[m] == Resources.LabelEnterValue)
					{
						paramValues[m] = "";
						displayValues.Add("");
						break;
					}
					if (select < 2)
					{
						displayValues.Add(param[m]);
						tempParam = param[m].Replace(",", "").Replace(".", "");
						if (long.TryParse(tempParam, out var toInt2))
						{
							array = paramValues;
							int num = m;
							array[num] = string.Concat(array[num], toInt2.ToString());
							break;
						}
						return PartialView("_DialogPartial", new DialogModel
						{
							DialogType = DialogModel.Type.Warning,
							Title = Resources.WrongParameterType,
							Content = string.Format(Resources.WrongParameterText, par.Name, par.DataType.ToString()),
							Buttons = new List<DialogModel.Button>
							{
								new DialogModel.Button
								{
									Caption = Resources.OK
								}
							}
						});
					}
					string[] inputs2 = param[m].Replace("\\;", "SEMICOLON").Split(';');
					for (int k4 = 0; k4 < inputs2.Count(); k4++)
					{
						inputs2[k4] = inputs2[k4].Replace("SEMICOLON", ";");
					}
					displayValues.Add(param[m]);
					foreach (string inputValue in inputs2.Where((string s) => s != string.Empty))
					{
						tempParam = inputValue.Replace(",", "").Replace(".", "");
						if (long.TryParse(tempParam, out var toInt))
						{
							array = paramValues;
							int num = m;
							array[num] = string.Concat(array[num], toInt.ToString(), ";");
							continue;
						}
						return PartialView("_DialogPartial", new DialogModel
						{
							DialogType = DialogModel.Type.Warning,
							Title = Resources.WrongParameterType,
							Content = string.Format(Resources.WrongParameterText, par.Name, par.DataType.ToString()),
							Buttons = new List<DialogModel.Button>
							{
								new DialogModel.Button
								{
									Caption = Resources.OK
								}
							}
						});
					}
					break;
				}
				case SqlType.Decimal:
				{
					if (string.IsNullOrEmpty(param[m]) || param[m] == Resources.LabelEnterValue)
					{
						paramValues[m] = "";
						displayValues.Add("");
						break;
					}
					double toDouble;
					if (select < 2)
					{
						tempParam = param[m];
						displayValues.Add(param[m]);
						if (double.TryParse(tempParam, out toDouble))
						{
							paramValues[m] = toDouble.ToString().Replace(",", ".");
							break;
						}
						return PartialView("_DialogPartial", new DialogModel
						{
							DialogType = DialogModel.Type.Warning,
							Title = Resources.WrongParameterType,
							Content = string.Format(Resources.WrongParameterText, par.Name, par.DataType.ToString()),
							Buttons = new List<DialogModel.Button>
							{
								new DialogModel.Button
								{
									Caption = Resources.OK
								}
							}
						});
					}
					string[] inputs3 = param[m].Replace("\\;", "SEMICOLON").Split(';');
					for (int k5 = 0; k5 < inputs3.Count(); k5++)
					{
						inputs3[k5] = inputs3[k5].Replace("SEMICOLON", ";");
					}
					displayValues.Add(param[m]);
					foreach (string inputValue2 in inputs3.Where((string s) => s != string.Empty))
					{
						tempParam = ((ViewboxSession.Language.CountryCode == null || !ViewboxSession.Language.CountryCode.Contains("de")) ? inputValue2.Replace(",", ".") : inputValue2.Replace(".", ""));
						if (double.TryParse(tempParam, out toDouble))
						{
							array = paramValues;
							int num = m;
							array[num] = string.Concat(array[num], toDouble.ToString().Replace(",", "."), ";");
							continue;
						}
						return PartialView("_DialogPartial", new DialogModel
						{
							DialogType = DialogModel.Type.Warning,
							Title = Resources.WrongParameterType,
							Content = string.Format(Resources.WrongParameterText, par.Name, par.DataType.ToString()),
							Buttons = new List<DialogModel.Button>
							{
								new DialogModel.Button
								{
									Caption = Resources.OK
								}
							}
						});
					}
					break;
				}
				case SqlType.Boolean:
					paramValues[m] = param[m];
					displayValues.Add(param[m]);
					break;
				case SqlType.Time:
				{
					if (string.IsNullOrEmpty(param[m]) || param[m] == Resources.LabelEnterValue)
					{
						paramValues[m] = "";
						displayValues.Add("");
						break;
					}
					displayValues.Add(param[m]);
					if (TimeSpan.TryParse(param[m], out var tempTime))
					{
						paramValues[m] = tempTime;
						break;
					}
					return PartialView("_DialogPartial", new DialogModel
					{
						DialogType = DialogModel.Type.Warning,
						Title = Resources.WrongParameterType,
						Content = string.Format(Resources.WrongParameterText, $"\"{par.GetDescription()}\"", par.DataType.ToString()),
						Buttons = new List<DialogModel.Button>
						{
							new DialogModel.Button
							{
								Caption = Resources.OK
							}
						}
					});
				}
				case SqlType.Date:
				{
					if (string.IsNullOrEmpty(param[m]) || param[m] == Resources.LabelEnterValue)
					{
						paramValues[m] = "";
						displayValues.Add("");
						break;
					}
					if (select < 2)
					{
						displayValues.Add(param[m]);
						if (DateTime.TryParse(param[m], out var valueDateTime2))
						{
							paramValues[m] = valueDateTime2.ToString("yyyy-MM-dd");
							break;
						}
						if (param[m] == "00.00.0000")
						{
							paramValues[m] = "0000-00-00";
							break;
						}
						return PartialView("_DialogPartial", new DialogModel
						{
							DialogType = DialogModel.Type.Warning,
							Title = Resources.WrongParameterType,
							Content = string.Format(Resources.WrongParameterText, par.Name, par.DataType.ToString()),
							Buttons = new List<DialogModel.Button>
							{
								new DialogModel.Button
								{
									Caption = Resources.OK
								}
							}
						});
					}
					string[] inputs4 = param[m].Replace("\\;", "SEMICOLON").Split(';');
					for (int k6 = 0; k6 < inputs4.Count(); k6++)
					{
						inputs4[k6] = inputs4[k6].Replace("SEMICOLON", ";");
					}
					displayValues.Add(param[m]);
					foreach (string inputValue3 in inputs4.Where((string s) => s != string.Empty))
					{
						if (DateTime.TryParse(inputValue3, out var valueDateTime))
						{
							array = paramValues;
							int num = m;
							array[num] = string.Concat(array[num], valueDateTime.ToString("yyyy-MM-dd"), ";");
							continue;
						}
						return PartialView("_DialogPartial", new DialogModel
						{
							DialogType = DialogModel.Type.Warning,
							Title = Resources.WrongParameterType,
							Content = string.Format(Resources.WrongParameterText, par.Name, par.DataType.ToString()),
							Buttons = new List<DialogModel.Button>
							{
								new DialogModel.Button
								{
									Caption = Resources.OK
								}
							}
						});
					}
					break;
				}
				case SqlType.DateTime:
				{
					if (string.IsNullOrEmpty(param[m]) || param[m] == Resources.LabelEnterValue)
					{
						paramValues[m] = "";
						displayValues.Add("");
						break;
					}
					if (select < 2)
					{
						displayValues.Add(param[m]);
						if (DateTime.TryParse(param[m], out var valueDateTime4))
						{
							paramValues[m] = valueDateTime4.ToString("yyyy-MM-dd HH:mm:ss");
							break;
						}
						return PartialView("_DialogPartial", new DialogModel
						{
							DialogType = DialogModel.Type.Warning,
							Title = Resources.WrongParameterType,
							Content = string.Format(Resources.WrongParameterText, par.Name, par.DataType.ToString()),
							Buttons = new List<DialogModel.Button>
							{
								new DialogModel.Button
								{
									Caption = Resources.OK
								}
							}
						});
					}
					string[] inputs5 = param[m].Replace("\\;", "SEMICOLON").Split(';');
					for (int k7 = 0; k7 < inputs5.Count(); k7++)
					{
						inputs5[k7] = inputs5[k7].Replace("SEMICOLON", ";");
					}
					displayValues.Add(param[m]);
					foreach (string inputValue4 in inputs5.Where((string s) => s != string.Empty))
					{
						if (DateTime.TryParse(inputValue4, out var valueDateTime3))
						{
							array = paramValues;
							int num = m;
							array[num] = string.Concat(array[num], valueDateTime3.ToString("yyyy-MM-dd HH:mm:ss"), ";");
							continue;
						}
						return PartialView("_DialogPartial", new DialogModel
						{
							DialogType = DialogModel.Type.Warning,
							Title = Resources.WrongParameterType,
							Content = string.Format(Resources.WrongParameterText, par.Name, par.DataType.ToString()),
							Buttons = new List<DialogModel.Button>
							{
								new DialogModel.Button
								{
									Caption = Resources.OK
								}
							}
						});
					}
					break;
				}
				case SqlType.String:
				case SqlType.Binary:
				{
					if (param[m] == Resources.LabelEnterValue || param[m] == "")
					{
						paramValues[m] = "";
						displayValues.Add("");
						break;
					}
					if (select < 2)
					{
						paramValues[m] = EscapeSpecials(param[m]);
						displayValues.Add(param[m]);
						if (par.LeadingZeros > 0 && regex.IsMatch(paramValues[m].ToString()))
						{
							paramValues[m] = paramValues[m].ToString().PadLeft(par.LeadingZeros, '0');
						}
						break;
					}
					string[] inputs = param[m].Replace("\\;", "SEMICOLON").Split(new char[1] { ';' }, StringSplitOptions.RemoveEmptyEntries);
					for (int k3 = 0; k3 < inputs.Count(); k3++)
					{
						inputs[k3] = inputs[k3].Replace("SEMICOLON", ";");
					}
					displayValues.Add(param[m]);
					for (int k2 = 0; k2 < inputs.Length; k2++)
					{
						string escapedInput = EscapeSpecials(inputs[k2]);
						if (par.LeadingZeros > 0 && regex.IsMatch(escapedInput))
						{
							array = paramValues;
							int num = m;
							array[num] = string.Concat(array[num], escapedInput.PadLeft(par.LeadingZeros, '0'), ";");
						}
						else
						{
							array = paramValues;
							int num = m;
							array[num] = string.Concat(array[num], escapedInput, ";");
						}
					}
					break;
				}
				default:
					if (param[m] == Resources.LabelEnterValue || param[m] == "")
					{
						paramValues[m] = "";
						displayValues.Add("");
					}
					else
					{
						paramValues[m] = param[m];
						displayValues.Add(param[m]);
					}
					break;
				}
				j2++;
				lastParameter = par;
			}
			SqlType[] checkTypes = new SqlType[6]
			{
				SqlType.Decimal,
				SqlType.Numeric,
				SqlType.Integer,
				SqlType.DateTime,
				SqlType.Date,
				SqlType.Time
			};
			lastParameter = null;
			j2 = 0;
			ITableObject tableObject = ViewboxSession.TableObjects[id];
			ViewboxSession.SetupTableColumns(id);
			int l;
			for (l = 0; l < param.Count; l++)
			{
				IParameter par2 = issue.Parameters.Single((IParameter x) => x.Id == itemId[l]);
				if (string.IsNullOrEmpty(par2.ColumnName) || string.IsNullOrEmpty(paramValues[l].ToString()) || !checkTypes.Contains(par2.DataType))
				{
					continue;
				}
				string minValue = (tableObject.Columns.Contains(par2.ColumnName) ? tableObject.Columns[par2.ColumnName].MinValue : null);
				string maxValue = (tableObject.Columns.Contains(par2.ColumnName) ? tableObject.Columns[par2.ColumnName].MaxValue : null);
				if (string.IsNullOrEmpty(minValue) || string.IsNullOrEmpty(maxValue))
				{
					continue;
				}
				bool minValid = true;
				bool maxValid = true;
				tempParam = paramValues[l].ToString();
				if (lastParameter != null && par2.GroupId == lastParameter.GroupId && par2.GroupId != 0 && lastParameter.Id != par2.Id && par2.Ordinal > lastParameter.Ordinal)
				{
					j2--;
				}
				switch (par2.DataType)
				{
				case SqlType.Decimal:
				case SqlType.Numeric:
				{
					if (selectionType == null || selectionType[j2] >= 2)
					{
						break;
					}
					double minDouble = 0.0;
					double maxDouble = 0.0;
					double valueDoubleToCheck = 0.0;
					if (!double.TryParse(tempParam, out valueDoubleToCheck))
					{
						return WrongParameterType(paramValues[l].ToString(), par2.DataType.ToString());
					}
					if (minValue != null)
					{
						if (ViewboxSession.Language.CountryCode != null && ViewboxSession.Language.CountryCode.Contains("de") && minValue.Contains('.'))
						{
							minValue = minValue.Replace('.', ',');
						}
						minValid = double.TryParse(minValue, out minDouble);
					}
					if (maxValue != null)
					{
						if (ViewboxSession.Language.CountryCode != null && ViewboxSession.Language.CountryCode.Contains("de") && maxValue.Contains('.'))
						{
							maxValue = maxValue.Replace('.', ',');
						}
						maxValid = double.TryParse(maxValue, out maxDouble);
					}
					if (!minValid || !maxValid)
					{
						return WrongParameterType(paramValues[l].ToString(), par2.DataType.ToString());
					}
					if ((minValue != null && valueDoubleToCheck < minDouble) || (maxValue != null && valueDoubleToCheck > maxDouble))
					{
						return ValueOutOfRange(paramValues[l].ToString(), minValue, maxValue);
					}
					break;
				}
				case SqlType.Integer:
					if (selectionType != null && selectionType[j2] < 2)
					{
						long minInt = 0L;
						long maxInt = 0L;
						long valueIntToCheck = 0L;
						if (!long.TryParse(tempParam, out valueIntToCheck))
						{
							return WrongParameterType(paramValues[l].ToString(), par2.DataType.ToString());
						}
						if (minValue != null)
						{
							minValid = long.TryParse(minValue, out minInt);
						}
						if (maxValue != null)
						{
							maxValid = long.TryParse(maxValue, out maxInt);
						}
						if (!minValid || !maxValid)
						{
							return WrongParameterType(paramValues[l].ToString(), par2.DataType.ToString());
						}
						if ((minValue != null && valueIntToCheck < minInt) || (maxValue != null && valueIntToCheck > maxInt))
						{
							return ValueOutOfRange(paramValues[l].ToString(), minValue, maxValue);
						}
					}
					break;
				case SqlType.Date:
				{
					if (selectionType == null || selectionType[j2] >= 2)
					{
						break;
					}
					DateTime minDate = DateTime.MaxValue;
					DateTime maxDate = DateTime.MaxValue;
					DateTime valueDateCheck = DateTime.MaxValue;
					if (!DateTime.TryParse(tempParam, out valueDateCheck) && !DateTime.TryParseExact(tempParam, "d", CultureInfo.InvariantCulture, DateTimeStyles.None, out valueDateCheck))
					{
						return WrongParameterType(paramValues[l].ToString(), par2.DataType.ToString());
					}
					minValid = true;
					maxValid = true;
					if (minValue != null)
					{
						minValid = DateTime.TryParse(minValue, out minDate);
						if (!minValid)
						{
							minValid = DateTime.TryParseExact(minValue, "d", CultureInfo.InvariantCulture, DateTimeStyles.None, out minDate);
							if (!minValid)
							{
								minValid = DateTime.TryParseExact(minValue, "yyyy.MM.dd.", CultureInfo.InvariantCulture, DateTimeStyles.None, out minDate);
							}
						}
					}
					if (maxValue != null)
					{
						maxValid = DateTime.TryParse(maxValue, out maxDate);
						if (!maxValid)
						{
							maxValid = DateTime.TryParseExact(maxValue, "d", CultureInfo.InvariantCulture, DateTimeStyles.None, out maxDate);
							if (!maxValid)
							{
								maxValid = DateTime.TryParseExact(maxValue, "yyyy.MM.dd.", CultureInfo.InvariantCulture, DateTimeStyles.None, out maxDate);
							}
						}
					}
					if (!minValid || !maxValid)
					{
						return WrongParameterType(paramValues[l].ToString(), par2.DataType.ToString());
					}
					if ((minValue != null && valueDateCheck.Ticks < minDate.Ticks) || (maxValue != null && valueDateCheck.Ticks > maxDate.Ticks))
					{
						return ValueOutOfRange(paramValues[l].ToString(), minValue, maxValue);
					}
					break;
				}
				case SqlType.Time:
				case SqlType.DateTime:
				{
					if (selectionType == null || selectionType[j2] >= 2)
					{
						break;
					}
					DateTime minDateTime = DateTime.MaxValue;
					DateTime maxDateTime = DateTime.MaxValue;
					DateTime valueDateTimeCheck = DateTime.MaxValue;
					if (!DateTime.TryParse(tempParam, out valueDateTimeCheck))
					{
						return WrongParameterType(paramValues[l].ToString(), par2.DataType.ToString());
					}
					minValid = true;
					maxValid = true;
					if (minValue != null)
					{
						minValid = DateTime.TryParse(minValue, out minDateTime);
						if (!minValid)
						{
							minValid = DateTime.TryParseExact(minValue, "d", CultureInfo.InvariantCulture, DateTimeStyles.None, out minDateTime);
						}
					}
					if (maxValue != null)
					{
						maxValid = DateTime.TryParse(maxValue, out maxDateTime);
						if (!maxValid)
						{
							DateTime.TryParseExact(maxValue, "d", CultureInfo.InvariantCulture, DateTimeStyles.None, out maxDateTime);
						}
					}
					if (!minValid || !maxValid)
					{
						return WrongParameterType(paramValues[l].ToString(), par2.DataType.ToString());
					}
					if ((minValue != null && valueDateTimeCheck.Ticks < minDateTime.Ticks) || (maxValue != null && valueDateTimeCheck.Ticks > maxDateTime.Ticks))
					{
						return ValueOutOfRange(paramValues[l].ToString(), minValue, maxValue);
					}
					break;
				}
				}
				lastParameter = par2;
			}
			StringBuilder filterBuilder = new StringBuilder();
			if (!isClickAndClick)
			{
				lastParameter = null;
				j2 = 0;
				ViewboxApplication.Database.SystemDb.ClearPreviousFreeSelectionParameters(issue.Id, ViewboxSession.User.Id);
				int k;
				for (k = 0; k < param.Count; k++)
				{
					IParameter p4 = issue.Parameters.Single((IParameter x) => x.Id == itemId[k]);
					string activeParam = ((param[k] == Resources.LabelEnterValue) ? "" : param[k]);
					if (lastParameter != null && p4.GroupId == lastParameter.GroupId && p4.GroupId != 0 && lastParameter.Id != p4.Id && p4.Ordinal > lastParameter.Ordinal)
					{
						j2--;
					}
					int tmpSelectionType = 0;
					try
					{
						tmpSelectionType = ((selectionType != null && j2 >= 0 && selectionType.Count != 0) ? selectionType[j2] : 0);
					}
					catch (Exception)
					{
						tmpSelectionType = 0;
					}
					if (k == 0)
					{
						ParamError pHEDelete = DeleteHistory(p4);
					}
					switch (SaveValueHistoryFreeSelection(param[k], tmpSelectionType, ref issue, p4.Id))
					{
					case ParamError.CouldNotSave:
						return PartialView("_DialogPartial", new DialogModel
						{
							DialogType = DialogModel.Type.Warning,
							Title = Resources.ExecuteIssueTitle,
							Content = Resources.ErrorSavingIssueParametersProbablyAnotherUserIsLoggedIn,
							Buttons = new List<DialogModel.Button>
							{
								new DialogModel.Button
								{
									Caption = Resources.OK
								}
							}
						});
					case ParamError.LengthTooLong:
						return PartialView("_DialogPartial", new DialogModel
						{
							DialogType = DialogModel.Type.Warning,
							Title = Resources.ExecuteIssueTitle,
							Content = Resources.ParameterLengthIsTooLong,
							Buttons = new List<DialogModel.Button>
							{
								new DialogModel.Button
								{
									Caption = Resources.OK
								}
							}
						});
					}
					lastParameter = p4;
					j2++;
				}
				int j;
				for (j = 0; j < issue.Parameters.Count; j++)
				{
					if (issue.Parameters.Single((IParameter x) => x.Id == itemId[j]).Values != null)
					{
						ViewboxApplication.Database.SystemDb.SaveParameterValueOrder(issue.Parameters.Single((IParameter x) => x.Id == itemId[j]), ViewboxSession.User, paramValues[j]);
					}
				}
			}
			IFilter f = null;
			lastParameter = null;
			j2 = 0;
			List<string> filterBuffer2 = new List<string>();
			List<string> procedureValues = new List<string>();
			if ((issue as Issue).IssueType == IssueType.StoredProcedure)
			{
				int i;
				for (i = 0; i < paramValues.Count(); i++)
				{
					IParameter p2 = issue.Parameters.Single((IParameter x) => x.Id == itemId[i]);
					procedureValues.Add("");
					if (lastParameter != null && p2.GroupId == lastParameter.GroupId && p2.GroupId != 0 && lastParameter.Id != p2.Id && p2.Ordinal > lastParameter.Ordinal)
					{
						j2--;
					}
					if (p2.FreeSelection == 1)
					{
						switch ((!isClickAndClick) ? ((selectionType != null && j2 >= 0 && selectionType.Count != 0) ? selectionType[j2] : 0) : 0)
						{
						case 0:
							if (p2.GroupId == 0)
							{
								if (paramValues[i].ToString() != "")
								{
									paramValues[i] = $"(`{p2.ColumnNameInView}` = '{paramValues[i]}')";
								}
							}
							else if (p2.Name.ToLower().Contains("von") || p2.Name.ToLower().Contains("from"))
							{
								if (paramValues[i].ToString() != "")
								{
									if (p2.UseAbsolute == 1 && (p2.DataType == SqlType.Decimal || p2.DataType == SqlType.Integer || p2.DataType == SqlType.Numeric))
									{
										filterBuffer2.Add($"(ABS(`{p2.ColumnNameInView}`) BETWEEN {paramValues[i]} AND ");
									}
									else
									{
										filterBuffer2.Add(string.Format("({0} BETWEEN '{1}' AND ", (p2.LeadingZeros == 0) ? $"`{p2.ColumnNameInView}`" : string.Format("IF(`{0}` REGEXP '^[0-9]*$', LPAD(`{0}`, {1}, '0'), `{0}`)", p2.ColumnNameInView, p2.LeadingZeros), paramValues[i]));
									}
								}
							}
							else if (paramValues[i - 1].ToString() != "" && paramValues[i].ToString() != "" && paramValues[i - 1].ToString() == paramValues[i].ToString() && paramValues[i].ToString() != "" && paramValues[i - 1].ToString() != "")
							{
								paramValues[i - 1] = $"({$"`{p2.ColumnNameInView}`"} = '{paramValues[i - 1]}')";
								paramValues[i] = "";
								filterBuffer2.Clear();
							}
							else if (paramValues[i].ToString() != "" && filterBuffer2.Count > 0)
							{
								if (p2.UseAbsolute == 1 && (p2.DataType == SqlType.Decimal || p2.DataType == SqlType.Integer || p2.DataType == SqlType.Numeric))
								{
									filterBuffer2.Add($"{paramValues[i]})");
									paramValues[i - 1] = string.Join("", filterBuffer2);
									paramValues[i] = "";
									filterBuffer2.Clear();
								}
								else
								{
									filterBuffer2.Add($"'{paramValues[i]}')");
									paramValues[i - 1] = string.Join("", filterBuffer2);
									paramValues[i] = "";
									filterBuffer2.Clear();
								}
							}
							else if (paramValues[i - 1].ToString() != "")
							{
								paramValues[i - 1] = $"({$"`{p2.ColumnNameInView}`"} = '{paramValues[i - 1]}')";
								paramValues[i] = "";
								filterBuffer2.Clear();
							}
							break;
						case 1:
							if (p2.GroupId == 0)
							{
								if (paramValues[i].ToString() != "")
								{
									paramValues[i] = $"({$"`{p2.ColumnNameInView}`"} != '{paramValues[i]}')";
								}
							}
							else if (p2.Name.ToLower().Contains("von") || p2.Name.ToLower().Contains("from"))
							{
								if (paramValues[i].ToString() != "")
								{
									filterBuffer2.Add($"({$"`{p2.ColumnNameInView}`"} NOT BETWEEN '{paramValues[i]}' AND ");
								}
							}
							else if (paramValues[i].ToString() != "" && filterBuffer2.Count > 0)
							{
								filterBuffer2.Add($"'{paramValues[i]}')");
								paramValues[i - 1] = string.Join("", filterBuffer2);
								paramValues[i] = "";
								filterBuffer2.Clear();
							}
							else if (paramValues[i - 1].ToString() != "")
							{
								filterBuffer2.Clear();
								paramValues[i - 1] = $"({$"`{p2.ColumnNameInView}`"} != '{paramValues[i - 1]}')";
							}
							break;
						case 2:
						{
							if (!(paramValues[i].ToString() != ""))
							{
								break;
							}
							string[] inputs6 = paramValues[i].ToString().Replace("\\;", "SEMICOLON").Split(';');
							for (int k8 = 0; k8 < inputs6.Count(); k8++)
							{
								inputs6[k8] = inputs6[k8].Replace("SEMICOLON", ";");
							}
							paramValues[i] = "";
							string[] array3 = inputs6;
							foreach (string item in array3)
							{
								if (item != "")
								{
									array = paramValues;
									int num = i;
									array[num] = string.Concat(array[num], $"'{item}',");
								}
							}
							paramValues[i] = paramValues[i].ToString().Remove(paramValues[i].ToString().Length - 1);
							paramValues[i] = $"({$"`{p2.ColumnNameInView}`"} IN ({paramValues[i]}))";
							break;
						}
						case 3:
						{
							if (!(paramValues[i].ToString() != ""))
							{
								break;
							}
							string[] inputs6 = paramValues[i].ToString().Replace("\\;", "SEMICOLON").Split(';');
							for (int k9 = 0; k9 < inputs6.Count(); k9++)
							{
								inputs6[k9] = inputs6[k9].Replace("SEMICOLON", ";");
							}
							paramValues[i] = "";
							string[] array2 = inputs6;
							foreach (string item2 in array2)
							{
								if (item2 != "")
								{
									array = paramValues;
									int num = i;
									array[num] = string.Concat(array[num], $"'{item2}',");
								}
							}
							paramValues[i] = paramValues[i].ToString().Remove(paramValues[i].ToString().Length - 1);
							paramValues[i] = $"({$"`{p2.ColumnNameInView}`"} NOT IN ({paramValues[i]}))";
							break;
						}
						}
					}
					lastParameter = p2;
					j2++;
				}
				Dictionary<int, StringBuilder> parameterValues = new Dictionary<int, StringBuilder>();
				int l2 = 0;
				foreach (int pid in itemId)
				{
					IParameter p3 = issue.Parameters.Single((IParameter x) => x.Id == pid);
					string value = paramValues[l2].ToString();
					if (!parameterValues.ContainsKey(pid))
					{
						parameterValues.Add(pid, new StringBuilder(value));
					}
					else if (value != string.Empty)
					{
						if (parameterValues[pid].Length > 0)
						{
							parameterValues[pid].Append(" AND ");
						}
						parameterValues[pid].Append(value);
					}
					l2++;
				}
				paramValues = ((IEnumerable<object>)parameterValues.Select(delegate(KeyValuePair<int, StringBuilder> p)
				{
					KeyValuePair<int, StringBuilder> keyValuePair = p;
					return keyValuePair.Value.ToString();
				})).ToArray();
			}
			int id2 = id;
			ITableObjectCollection tableObjects = ViewboxSession.TableObjects;
			IFullColumnCollection columns = ViewboxSession.Columns;
			object[] paramValues2 = paramValues;
			array = displayValues.ToArray();
			Transformation trans = Transformation.Create(id2, tableObjects, columns, opt, null, f, paramValues2, array, selectionType, itemId, 0, RelationType.Normal, "", "", null, originalColumnIds: true, null, multipleOptimization: false, null, trans_KeyChanged);
			ViewboxApplication.Database.SystemDb.IsReport = true;
			return PartialView("_DialogPartial", new DialogModel
			{
				DialogType = DialogModel.Type.Info,
				Key = trans.Key,
				Title = Resources.ExecuteIssueTitle,
				Content = string.Format(Resources.LongRunningDialogText, Resources.ExecuteIssueContent),
				Buttons = new List<DialogModel.Button>
				{
					new DialogModel.Button
					{
						Caption = Resources.LongRunningDialogCaption,
						Data = trans.Key
					}
				}
			});
		}

		private bool Validate(IIssue issue, List<string> param, List<int> itemId)
		{
			Dictionary<IParameter, string> parameters = new Dictionary<IParameter, string>();
			string logic = issue.Logic.Logic;
			for (int i = 0; i < itemId.Count(); i++)
			{
				parameters.Add(issue.Parameters[itemId[i]], param[i]);
			}
			foreach (KeyValuePair<IParameter, string> parameter in parameters.OrderByDescending((KeyValuePair<IParameter, string> x) => x.Key.Name.Length))
			{
				logic = ((!(parameter.Value != Resources.LabelEnterValue) || !(parameter.Value != "")) ? logic.Replace(parameter.Key.Name, "false") : logic.Replace(parameter.Key.Name, "true"));
			}
			using DatabaseBase conn = ViewboxApplication.Database.ConnectionManager.GetConnection();
			int valid = int.Parse(conn.ExecuteScalar($"SELECT {logic} ;").ToString());
			return valid == 1;
		}

		private static string RemoveEscapes(string p)
		{
			return p.Replace("\\'", "'");
		}

		private static string EscapeSpecials(string p)
		{
			return p.Replace("'", "\\'");
		}

		[ValidateInput(false)]
		[HttpGet]
		public ActionResult CancelGetIssueRecordCount(string Key)
		{
			Transformation.Find(Key)?.Cancel();
			return new JsonResult
			{
				Data = null,
				JsonRequestBehavior = JsonRequestBehavior.AllowGet
			};
		}

		public static string AppendQuerystring(string keyvalue)
		{
			return AppendQuerystring(System.Web.HttpContext.Current.Request.RawUrl, keyvalue);
		}

		public static string AppendQuerystring(string url, string keyvalue)
		{
			string dummyHost = "http://www.test.com:80/";
			if (!url.ToLower().StartsWith("http"))
			{
				url = dummyHost + url;
			}
			UriBuilder builder = new UriBuilder(url);
			string query = builder.Query;
			NameValueCollection qs = HttpUtility.ParseQueryString(query);
			string[] pts = keyvalue.Split('&');
			string[] array = pts;
			foreach (string p in array)
			{
				string[] pts2 = p.Split('=');
				qs.Set(pts2[0], pts2[1]);
			}
			StringBuilder sb = new StringBuilder();
			foreach (string key in qs.Keys)
			{
				sb.Append($"{key}={qs[key]}&");
			}
			builder.Query = sb.ToString().TrimEnd('&');
			return builder.ToString().Replace(dummyHost, string.Empty);
		}

		private DialogModel IssueReexecution(ViewboxDb.TableObject tobj)
		{
			IOptimization opt = ViewboxSession.Optimizations.LastOrDefault();
			IIssue issue = tobj.Table as IIssue;
			if (issue?.NeedBukrs ?? false)
			{
				IOptimization split = global::ViewboxDb.ViewboxDb.GetOptimizationForType(opt, OptimizationType.SplitTable);
				if (split != null && split.Value == null)
				{
					string url2 = AppendQuerystring("optid=" + tobj.Optimization.Id).Replace("?isNoResult=true&", "?").Replace("?isNoResult=true", "").Replace("&isNoResult=true", "");
					OptimizationManager.SetOptimization(tobj.Optimization.Id);
					return new DialogModel
					{
						DialogType = DialogModel.Type.Warning,
						Title = Resources.Canceled,
						Content = string.Format(Resources.NeedBukrs, (split.Group == null) ? "Company Code" : split.Group.GetName(), split.GetValue()),
						Buttons = new List<DialogModel.Button>
						{
							new DialogModel.Button
							{
								Caption = Resources.OK,
								Url = url2
							}
						}
					};
				}
			}
			if (issue?.NeedGJahr ?? false)
			{
				IOptimization sort = global::ViewboxDb.ViewboxDb.GetOptimizationForType(opt, OptimizationType.SortColumn);
				if (sort != null && sort.Value == null)
				{
					string url = AppendQuerystring("optid=" + tobj.Optimization.Id).Replace("?isNoResult=true&", "?").Replace("?isNoResult=true", "").Replace("&isNoResult=true", "");
					OptimizationManager.SetOptimization(tobj.Optimization.Id);
					return new DialogModel
					{
						DialogType = DialogModel.Type.Warning,
						Title = Resources.Canceled,
						Content = string.Format(Resources.NeedGJahr, (sort.Group == null) ? "Year" : sort.Group.GetName(), sort.GetValue()),
						Buttons = new List<DialogModel.Button>
						{
							new DialogModel.Button
							{
								Caption = Resources.OK,
								Url = url
							}
						}
					};
				}
			}
			List<int> SumColumnsIndex = ((tobj.Sum == null) ? null : new List<int>(tobj.Sum.Select((IColumn c) => c.Id)));
			int id = tobj.Table.Id;
			ITableObjectCollection tableObjects = ViewboxSession.TableObjects;
			IFullColumnCollection columns = ViewboxSession.Columns;
			SortCollection sort2 = tobj.Sort;
			IFilter filter = tobj.Filter;
			object[] paramValues = tobj.ParamValues;
			object[] displayValues = tobj.DisplayValues.ToArray();
			Transformation trans = Transformation.Create(id, tableObjects, columns, opt, sort2, filter, paramValues, displayValues, tobj.SelectionTypes, tobj.ItemId, 0, RelationType.Normal, "", "", SumColumnsIndex, originalColumnIds: true, tobj.GroupSubTotal, multipleOptimization: false, null, trans_KeyChanged);
			return new DialogModel
			{
				DialogType = DialogModel.Type.Info,
				Key = trans.Key,
				Title = Resources.ExecuteIssueTitle,
				Content = string.Format(Resources.LongRunningDialogText, Resources.ExecuteIssueContent),
				Buttons = new List<DialogModel.Button>
				{
					new DialogModel.Button
					{
						Caption = Resources.LongRunningDialogCaption,
						Data = trans.Key,
						Url = "/IssueList",
						Id = "optimization_change"
					}
				}
			};
		}

		public ActionResult Sort(int id, int tableId, SortDirection direction = SortDirection.Ascending, int scrollPosition = 0)
		{
			ViewboxSession.SaveStartLoadingTime();
			string sFilter = string.Empty;
			if (tableId < 0)
			{
				ViewboxDb.TableObject obj = ViewboxSession.TempTableObjects[tableId];
				if (obj.Filter != null)
				{
					sFilter = obj.Filter.ToOriginalString();
				}
			}
			return SortAndFilter(tableId, new SortCollection
			{
				new Sort(id, direction)
			}, sFilter, null, save: false, scrollPosition, canReturnDialogPartial: true, "", urlDecode: false, 0, "", "", null, null, null, fromSort: true);
		}

		public ActionResult Group2(int id, List<int> colIds, AggregationCollection aggs, string filter, bool saveGroup = false, string tableName = "")
		{
			IFilter f = null;
			if (!string.IsNullOrWhiteSpace(filter))
			{
				f = ViewboxSession.GetFilter(id, filter, ViewboxSession.TempTableObjects, ViewboxSession.TableObjects);
			}
			Transformation trans = ViewboxSession.CreateTransformation(id, colIds, aggs, f, saveGroup, tableName);
			return PartialView("_DialogPartial", new DialogModel
			{
				DialogType = DialogModel.Type.Info,
				Key = trans.Key,
				Title = Resources.Group,
				Content = string.Format(Resources.LongRunningDialogText, Resources.Group),
				Buttons = new List<DialogModel.Button>
				{
					new DialogModel.Button
					{
						Caption = Resources.LongRunningDialogCaption
					}
				}
			});
		}

		public ActionResult Group(int id, List<int> colIds, List<AggregationFunction> aggfunc, string filter, bool saveGroup = false, string tableName = "")
		{
			if (colIds == null || colIds.Count == 0)
			{
				return PartialView("_ExtendedDialogPartial", new DialogModel
				{
					DialogType = DialogModel.Type.Warning,
					Title = Resources.GroupFailed,
					Content = Resources.GroupFailedColIds,
					Buttons = new List<DialogModel.Button>
					{
						new DialogModel.Button
						{
							Caption = Resources.OK
						}
					}
				});
			}
			if (saveGroup && (string.IsNullOrWhiteSpace(tableName) || tableName.Trim() == Resources.InsertName.Trim()))
			{
				return PartialView("_DialogPartial", new DialogModel
				{
					DialogType = DialogModel.Type.Warning,
					Title = Resources.Group,
					Content = Resources.GroupJoinDefaultLabelError,
					Buttons = new List<DialogModel.Button>
					{
						new DialogModel.Button
						{
							Caption = Resources.OK
						}
					}
				});
			}
			IFilter f = null;
			if (!string.IsNullOrWhiteSpace(filter))
			{
				f = ViewboxSession.GetFilter(id, filter, ViewboxSession.TempTableObjects, ViewboxSession.TableObjects, decode: false, forcedForCol: true, ViewboxSession.Columns);
			}
			Transformation trans = ViewboxSession.CreateTransformation(id, colIds, aggfunc, f, saveGroup, tableName);
			return PartialView("_DialogPartial", new DialogModel
			{
				DialogType = DialogModel.Type.Info,
				Key = trans.Key,
				Title = Resources.Group,
				Content = string.Format(Resources.LongRunningDialogText, Resources.Group),
				Buttons = new List<DialogModel.Button>
				{
					new DialogModel.Button
					{
						Caption = Resources.LongRunningDialogCaption
					}
				}
			});
		}

		public ActionResult OnlySumGroup(int id, string filter, bool saveGroup = false, string tableName = "")
		{
			List<AggregationFunction> aggfunc = new List<AggregationFunction> { AggregationFunction.Sum };
			IFilter f = null;
			if (!string.IsNullOrWhiteSpace(filter))
			{
				f = ViewboxSession.GetFilter(id, filter, ViewboxSession.TempTableObjects, ViewboxSession.TableObjects);
			}
			ITableObject table = ((id < 0) ? ViewboxSession.TempTableObjects[id].Table : ViewboxSession.TableObjects[id]);
			bool hasCorrectValue = false;
			foreach (IColumn col in table.Columns)
			{
				if (col.DataType == SqlType.Decimal && !col.IsEmpty)
				{
					hasCorrectValue = true;
				}
			}
			if (!hasCorrectValue)
			{
				return PartialView("_DialogPartial", new DialogModel
				{
					DialogType = DialogModel.Type.Warning,
					Title = Resources.NoNumberInTable,
					Content = Resources.NoNumberColumn,
					Buttons = new List<DialogModel.Button>
					{
						new DialogModel.Button
						{
							Caption = Resources.OK
						}
					}
				});
			}
			Transformation trans = ViewboxSession.CreateTransformation(id, null, aggfunc, f, saveGroup, tableName);
			return PartialView("_DialogPartial", new DialogModel
			{
				DialogType = DialogModel.Type.Info,
				Key = trans.Key,
				Title = Resources.AggSum,
				Content = string.Format(Resources.LongRunningDialogText, Resources.AggSum),
				Buttons = new List<DialogModel.Button>
				{
					new DialogModel.Button
					{
						Caption = Resources.LongRunningDialogCaption
					}
				}
			});
		}

		public ActionResult Relations(int id, int joinTableId, List<JoinColumns> columns)
		{
			bool warning = columns == null;
			if (!warning)
			{
				foreach (JoinColumns column in columns)
				{
					if (column.Column1 == 0)
					{
						warning = true;
						break;
					}
					if (column.Column2 == 0)
					{
						warning = true;
						break;
					}
				}
			}
			if (!warning)
			{
				List<int> firstCols = columns.Select((JoinColumns cols) => cols.Column1).ToList();
				List<int> secondCols = columns.Select((JoinColumns cols) => cols.Column2).ToList();
				ITableObject tableobject = ViewboxSession.TableObjects[id];
				if (id < 0)
				{
					tableobject = ViewboxSession.TempTableObjects[id].Table;
				}
				if (ViewboxApplication.Database.DoesRelationExist(id, columns, tableobject))
				{
					return PartialView("_DialogPartial", new DialogModel
					{
						DialogType = DialogModel.Type.Warning,
						Title = Resources.AddingRelation,
						Content = Resources.RelationExists,
						Buttons = new List<DialogModel.Button>
						{
							new DialogModel.Button
							{
								Caption = Resources.OK
							}
						}
					});
				}
				RelationJob relationj = RelationJob.Create(id, joinTableId, columns, tableobject);
				return PartialView("_DialogPartial", new DialogModel
				{
					DialogType = DialogModel.Type.Info,
					Key = relationj.Key,
					Title = Resources.AddingRelation,
					Content = string.Format(Resources.LongRunningDialogText, Resources.AddingRelation),
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
				DialogType = DialogModel.Type.Warning,
				Title = Resources.Relations,
				Content = Resources.MissingColumnsForRelation,
				Buttons = new List<DialogModel.Button>
				{
					new DialogModel.Button
					{
						Caption = Resources.OK
					}
				}
			});
		}

		public ActionResult DeleteRelations(int id, string[] relations)
		{
			ITableObject tableobject = null;
			if (id < 0)
			{
				tableobject = ViewboxSession.TempTableObjects[id].Table;
			}
			RelationJob relj = RelationJob.Create(id, relations, tableobject);
			return PartialView("_DialogPartial", new DialogModel
			{
				DialogType = DialogModel.Type.Info,
				Key = relj.Key,
				Title = Resources.DeleteRelations,
				Content = string.Format(Resources.LongRunningDialogText, Resources.DeleteRelations),
				Buttons = new List<DialogModel.Button>
				{
					new DialogModel.Button
					{
						Caption = Resources.LongRunningDialogCaption
					}
				}
			});
		}

		public ActionResult Join(int id, int joinTableId, JoinColumnsCollection columns, string filter1, string filter2, JoinType type, bool saveJoin = false, string tableName = "")
		{
			bool warning = columns == null;
			bool warning2 = false;
			if (!warning)
			{
				foreach (JoinColumns column in columns)
				{
					if (column.Column1 == 0)
					{
						warning2 = true;
						break;
					}
					if (column.Column2 == 0)
					{
						warning2 = true;
						break;
					}
				}
			}
			if (!(warning || warning2))
			{
				if (saveJoin && (string.IsNullOrWhiteSpace(tableName) || tableName.Trim() == Resources.InsertName.Trim()))
				{
					return PartialView("_DialogPartial", new DialogModel
					{
						DialogType = DialogModel.Type.Warning,
						Title = Resources.Join,
						Content = Resources.GroupJoinDefaultLabelError,
						Buttons = new List<DialogModel.Button>
						{
							new DialogModel.Button
							{
								Caption = Resources.OK
							}
						}
					});
				}
				IFilter f1 = null;
				if (!string.IsNullOrWhiteSpace(filter1))
				{
					f1 = ViewboxSession.GetFilter(id, filter1, ViewboxSession.TempTableObjects, ViewboxSession.TableObjects, decode: false, forcedForCol: true, ViewboxSession.Columns);
				}
				IFilter f2 = null;
				if (!string.IsNullOrWhiteSpace(filter2))
				{
					f2 = ViewboxSession.GetFilter(joinTableId, filter2, ViewboxSession.TempTableObjects, ViewboxSession.TableObjects, decode: false, forcedForCol: true, ViewboxSession.Columns);
				}
				ITableObject table1 = ((id < 0) ? ViewboxSession.TempTableObjects[id].Table : ViewboxSession.TableObjects[id]);
				ITableObject table2 = ((joinTableId < 0) ? ViewboxSession.TempTableObjects[joinTableId].Table : ViewboxSession.TableObjects[joinTableId]);
				long enabledLines = ViewboxApplication.SQLJoinRowsInThousands;
				long enabledLinesForIndexing = ViewboxApplication.SQLJoinIndexRowsInThousands;
				long calulatedRowCount = 0L;
				long indexingRowCount = 0L;
				bool indexingAffects = false;
				if (!ViewboxApplication.Database.TempDatabase.PreAnalyzeJoinTable(new Join(table1, table2, columns, f1, f2, type), ViewboxSession.Optimizations.LastOrDefault(), enabledLines, enabledLinesForIndexing, out calulatedRowCount, out indexingRowCount, out indexingAffects))
				{
					return PartialView("_ExtendedDialogPartial", new DialogModel
					{
						DialogType = DialogModel.Type.Warning,
						Title = Resources.Join,
						Content = ((indexingRowCount > enabledLinesForIndexing) ? string.Format(Resources.SqlJoinIndexingTakeMuchTime, indexingRowCount, enabledLinesForIndexing) : string.Format(Resources.SqlJoinTakeMuchTime, calulatedRowCount, enabledLines)),
						Buttons = new List<DialogModel.Button>
						{
							new DialogModel.Button
							{
								Caption = Resources.OK
							}
						}
					});
				}
				Transformation trans = Transformation.Create(ViewboxSession.TableObjects, ViewboxSession.Columns, ViewboxSession.Optimizations.LastOrDefault(), id, joinTableId, columns, f1, f2, type, saveJoin, tableName);
				return PartialView("_DialogPartial", new DialogModel
				{
					DialogType = DialogModel.Type.Info,
					Title = Resources.Join,
					Key = trans.Key,
					Content = string.Format(Resources.LongRunningDialogText, Resources.Join),
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
				DialogType = DialogModel.Type.Warning,
				Title = Resources.Join,
				Content = Resources.MissingColumnsForJoin,
				Buttons = new List<DialogModel.Button>
				{
					new DialogModel.Button
					{
						Caption = Resources.OK
					}
				}
			});
		}

		public ActionResult MultipleOptimizationLimits(int id, bool multiOptimization, int[] optimizationFrom, int[] optimizationTo)
		{
			IDictionary<int, Tuple<int, int>> optimizationSelected = new Dictionary<int, Tuple<int, int>>();
			if (optimizationFrom != null && optimizationTo != null)
			{
				for (int i = 0; i < optimizationFrom.Count(); i++)
				{
					if (optimizationFrom[i] <= optimizationTo[i])
					{
						optimizationSelected.Add(new KeyValuePair<int, Tuple<int, int>>(i + 1, new Tuple<int, int>(optimizationFrom[i], optimizationTo[i])));
					}
					else
					{
						optimizationSelected.Add(new KeyValuePair<int, Tuple<int, int>>(i + 1, new Tuple<int, int>(optimizationTo[i], optimizationFrom[i])));
					}
				}
			}
			IFilter f = null;
			List<int> summaList = null;
			SortCollection sortList = null;
			SubTotalParameters groupSubTotal = null;
			object[] paramValues = null;
			List<int> itemId = null;
			List<int> selectionTypes = null;
			string[] displayValues = null;
			if (id < 0)
			{
				ViewboxDb.TableObject table = ViewboxSession.TempTableObjects[id];
				f = table.Filter;
				summaList = ((table.Sum == null) ? null : table.Sum.Select((IColumn c) => c.Id).ToList());
				sortList = table.Sort;
				groupSubTotal = table.GroupSubTotal;
				paramValues = ViewboxSession.TempTableObjects[id].ParamValues;
				itemId = ViewboxSession.TempTableObjects[id].ItemId;
				selectionTypes = ViewboxSession.TempTableObjects[id].SelectionTypes;
				displayValues = ViewboxSession.TempTableObjects[id].DisplayValues.ToArray();
			}
			ITableObjectCollection tableObjects = ViewboxSession.TableObjects;
			IFullColumnCollection columns = ViewboxSession.Columns;
			IOptimization opt = ViewboxSession.Optimizations.LastOrDefault();
			SortCollection sortList2 = sortList;
			IFilter filter = f;
			object[] paramValues2 = paramValues;
			object[] displayValues2 = displayValues;
			Transformation trans = Transformation.Create(id, tableObjects, columns, opt, sortList2, filter, paramValues2, displayValues2, selectionTypes, itemId, 0, RelationType.Normal, "", "", summaList, originalColumnIds: true, groupSubTotal, multiOptimization, optimizationSelected, trans_KeyChanged);
			return PartialView("_DialogPartial", new DialogModel
			{
				DialogType = DialogModel.Type.Info,
				Title = Resources.MultipleOptimizations,
				Key = trans.Key,
				Content = string.Format(Resources.LongRunningDialogText, Resources.MultipleOptimizations),
				Buttons = new List<DialogModel.Button>
				{
					new DialogModel.Button
					{
						Caption = Resources.LongRunningDialogCaption
					}
				}
			});
		}

		public ActionResult ActivateMultipleOptimization(int id, bool multiOptimization)
		{
			IDictionary<int, Tuple<int, int>> optimizationSelected = null;
			if (multiOptimization)
			{
				IOptimization optimization = ViewboxSession.Optimizations.LastOrDefault();
				optimizationSelected = new Dictionary<int, Tuple<int, int>>();
				IOptimization mandt = optimization.GetOptimization(OptimizationType.IndexTable);
				IOptimization bukr = optimization.GetOptimization(OptimizationType.SplitTable);
				IOptimization gjahr = optimization.GetOptimization(OptimizationType.SortColumn);
				//if (mandt != null && !mandt.Value.IsNullOrWhiteSpace())
				//{
				//	optimizationSelected.Add(optimizationSelected.Count + 1, new Tuple<int, int>(mandt.Id, mandt.Id));
				//}
				//else
				//{
				//	List<IOptimization> madts = ViewboxSession.Optimizations.Select((IOptimization x) => x.GetOptimization(OptimizationType.IndexTable)).ToList();
				//	if (madts != null && madts.FirstOrDefault() != null && madts.LastOrDefault() != null)
				//	{
				//		optimizationSelected.Add(optimizationSelected.Count + 1, new Tuple<int, int>(madts.First().Id, madts.Last().Id));
				//	}
				//}
				//if (bukr != null && !bukr.Value.IsNullOrWhiteSpace())
				//{
				//	optimizationSelected.Add(optimizationSelected.Count + 1, new Tuple<int, int>(bukr.Id, bukr.Id));
				//}
				//else
				//{
				//	IOrderedEnumerable<IOptimization> bukrs = from x in ViewboxSession.AllowedBukrs
				//		where x.Parent == mandt && !x.Value.IsNullOrWhiteSpace()
				//		orderby x.Id
				//		select x;
				//	if (bukrs != null && bukrs.FirstOrDefault() != null && bukrs.LastOrDefault() != null)
				//	{
				//		optimizationSelected.Add(optimizationSelected.Count + 1, new Tuple<int, int>(bukrs.First().Id, bukrs.Last().Id));
				//	}
				//}
				//if (gjahr != null && !gjahr.Value.IsNullOrWhiteSpace())
				//{
				//	optimizationSelected.Add(optimizationSelected.Count + 1, new Tuple<int, int>(gjahr.Id, gjahr.Id));
				//}
				//else if (bukr != null)
				//{
				//	IOrderedEnumerable<IOptimization> gjahrs2 = (bukr.Value.IsNullOrWhiteSpace() ? (from x in ViewboxSession.AllowedGjahr
				//		where ViewboxSession.AllowedBukrs.FirstOrDefault((IOptimization y) => y.Parent == mandt && !y.Value.IsNullOrWhiteSpace() && y == x.Parent) != null && !x.Value.IsNullOrWhiteSpace()
				//		orderby x.Id
				//		select x) : (from x in ViewboxSession.AllowedGjahr
				//		where x.Parent == bukr && !x.Value.IsNullOrWhiteSpace()
				//		orderby x.Id
				//		select x));
				//	if (gjahrs2 != null && gjahrs2.FirstOrDefault() != null && gjahrs2.LastOrDefault() != null)
				//	{
				//		optimizationSelected.Add(optimizationSelected.Count + 1, new Tuple<int, int>(gjahrs2.First().Id, gjahrs2.Last().Id));
				//	}
				//}
				//else if (mandt != null)
				//{
				//	IOrderedEnumerable<IOptimization> gjahrs = (mandt.Value.IsNullOrWhiteSpace() ? (from x in ViewboxSession.AllowedGjahr
				//		where ViewboxSession.AllowedMandant.FirstOrDefault((IOptimization y) => y.Parent == mandt && !y.Value.IsNullOrWhiteSpace() && y == x.Parent) != null && !x.Value.IsNullOrWhiteSpace()
				//		orderby x.Id
				//		select x) : (from x in ViewboxSession.AllowedGjahr
				//		where x.Parent == mandt && !x.Value.IsNullOrWhiteSpace()
				//		orderby x.Id
				//		select x));
				//	if (gjahrs != null && gjahrs.FirstOrDefault() != null && gjahrs.LastOrDefault() != null)
				//	{
				//		optimizationSelected.Add(optimizationSelected.Count + 1, new Tuple<int, int>(gjahrs.First().Id, gjahrs.Last().Id));
				//	}
				//}
			}
			IFilter f = null;
			List<int> summaList = null;
			SortCollection sortList = null;
			SubTotalParameters groupSubTotal = null;
			object[] paramValues = null;
			List<int> itemId = null;
			List<int> selectionTypes = null;
			string[] displayValues = null;
			if (id < 0)
			{
				ViewboxDb.TableObject table = ViewboxSession.TempTableObjects[id];
				f = table.Filter;
				summaList = ((table.Sum == null) ? null : table.Sum.Select((IColumn c) => c.Id).ToList());
				sortList = table.Sort;
				groupSubTotal = table.GroupSubTotal;
				paramValues = ViewboxSession.TempTableObjects[id].ParamValues;
				itemId = ViewboxSession.TempTableObjects[id].ItemId;
				selectionTypes = ViewboxSession.TempTableObjects[id].SelectionTypes;
				displayValues = ViewboxSession.TempTableObjects[id].DisplayValues.ToArray();
			}
			ITableObjectCollection tableObjects = ViewboxSession.TableObjects;
			IFullColumnCollection columns = ViewboxSession.Columns;
			IOptimization opt = ViewboxSession.Optimizations.LastOrDefault();
			SortCollection sortList2 = sortList;
			IFilter filter = f;
			object[] paramValues2 = paramValues;
			object[] displayValues2 = displayValues;
			Transformation trans = Transformation.Create(id, tableObjects, columns, opt, sortList2, filter, paramValues2, displayValues2, selectionTypes, itemId, 0, RelationType.Normal, "", "", summaList, originalColumnIds: true, groupSubTotal, multiOptimization, optimizationSelected, trans_KeyChanged);
			return PartialView("_DialogPartial", new DialogModel
			{
				DialogType = DialogModel.Type.Info,
				Title = Resources.MultipleOptimizations,
				Key = trans.Key,
				Content = string.Format(Resources.LongRunningDialogText, Resources.MultipleOptimizations),
				Buttons = new List<DialogModel.Button>
				{
					new DialogModel.Button
					{
						Caption = Resources.LongRunningDialogCaption
					}
				}
			});
		}

		private bool ValidateFilterSave(IFilter f)
		{
			if (f is AndFilter || f is OrFilter)
			{
				return false;
			}
			return true;
		}

		private bool ValidateFilter(IFilter f)
		{
			if (f is ColValueFilter && (f as ColValueFilter).Column == null)
			{
				return false;
			}
			if (f is ColSetFilter && (f as ColSetFilter).Column == null)
			{
				return false;
			}
			if (f is BetweenFilter && (f as BetweenFilter).Column == null)
			{
				return false;
			}
			if (f is BinaryFilter)
			{
				if ((f as BinaryFilter).Conditions.Count == 0)
				{
					return false;
				}
				foreach (IFilter cf in (f as BinaryFilter).Conditions)
				{
					if (!ValidateFilter(cf))
					{
						return false;
					}
				}
			}
			return true;
		}

		[ValidateInput(false)]
		public ActionResult SortAndFilter(int id, SortCollection sortList, string filter, DescriptionCollection descriptions, bool save = false, int scrollPosition = 0, bool canReturnDialogPartial = true, string sortListString = "", bool urlDecode = false, int relationType = 0, string relationExtInfo = "", string relationColumnExtInfo = "", List<int> summaList = null, SubTotalParameters subTotalParameters = null, List<bool> freeselection = null, bool fromSort = false)
		{
			SubTotalParameters groupSubTotal = subTotalParameters;
			bool multipleOptimization = id < 0 && ViewboxSession.TempTableObjects[id].MultiOptimization;
			IDictionary<int, Tuple<int, int>> optimizationSelected = ((id < 0) ? ViewboxSession.TempTableObjects[id].OptimizationSelected : null);
			if (filter?.Contains("%") ?? false)
			{
				string[] filters = filter.Split('%');
				string[] array = filters;
				foreach (string filt in array)
				{
					if (!filt.Contains("\""))
					{
						continue;
					}
					string tempCol = filt.Split(',')[0];
					string tempFilter = filt.Split('"')[1];
					string oldTempFilter = tempFilter;
					if (!int.TryParse(tempCol, out var columnNo))
					{
						continue;
					}
					IColumn col = ViewboxSession.Columns[columnNo];
					if (col == null)
					{
						throw new HttpException(404, "Not Found");
					}
					if (col.DataType != SqlType.Decimal && col.DataType != SqlType.Numeric)
					{
						continue;
					}
					if (ViewboxSession.Language.CountryCode != null && ViewboxSession.Language.CountryCode.Contains("de"))
					{
						if (tempFilter.Contains(".") && tempFilter.Contains(",") && tempFilter.Length > 1)
						{
							tempFilter = tempFilter.Replace(".", "");
						}
						tempFilter = tempFilter.Replace(",", ".");
					}
					filter = filter.Replace("%" + tempCol + ",\"" + oldTempFilter, "%" + tempCol + ",\"" + tempFilter);
				}
			}
			if (filter != null && (filter.Contains("AND(\"))") || filter.Contains("AND(\"),")))
			{
				filter = filter.Replace("AND(\"))", "");
				filter = filter.Replace("AND(\"),", "");
				if (filter != string.Empty)
				{
					filter = $"AND({filter},)";
				}
			}
			if (!string.IsNullOrEmpty(filter))
			{
				filter = filter.Replace(Resources.EnterValue, "");
			}
			if (filter != null && (filter.ToLower() == "or()" || filter.ToLower() == "and()"))
			{
				filter = string.Empty;
			}
			IFilter f = null;
			if (id > 0)
			{
				if (!ViewboxSession.TableObjects.Contains(id))
				{
					ViewboxSession.AddTableObject(id);
				}
				ViewboxSession.SetupTableColumns(id);
			}
			if ((filter == string.Empty || filter == null) && sortList == null && sortListString == "" && summaList == null && id < 0 && !ViewboxSession.TempTableObjects[id].IsNewGroupByOrJoinTable)
			{
				ITableObject originalTableObj = ViewboxSession.TempTableObjects[id].OriginalTable ?? ViewboxSession.TempTableObjects[id].Table;
				if (originalTableObj.Type == TableType.Table || originalTableObj.Type == TableType.View)
				{
					int originalId = ((id < 0) ? originalTableObj.Id : id);
					id = originalId;
				}
			}
			try
			{
				if (filter != null && filter != string.Empty)
				{
					string[] filterParts = filter.Split(new string[1] { ")," }, StringSplitOptions.None);
					string[] array2 = filterParts;
					foreach (string toParse in array2)
					{
						if (!toParse.Contains("Like"))
						{
							continue;
						}
						int idLimit = 0;
						string colIdStr = toParse.Replace("And(", "").Replace("Like(%", "");
						string text = colIdStr;
						foreach (char c in text)
						{
							if (c == ',')
							{
								break;
							}
							idLimit++;
						}
						if (int.TryParse(colIdStr.Substring(0, idLimit), out var colId))
						{
							ViewboxSession.Columns[colId].ExactMatchUnchecked = true;
						}
					}
				}
			}
			catch (Exception)
			{
			}
			if (urlDecode)
			{
				filter = base.Server.UrlDecode(filter);
			}
			if (!string.IsNullOrWhiteSpace(filter))
			{
				if (sortList == null)
				{
					sortList = new SortCollection();
				}
				f = ViewboxSession.GetFilter(id, filter, ViewboxSession.TempTableObjects, ViewboxSession.TableObjects, urlDecode, forcedForCol: true, ViewboxSession.Columns);
				if (f != null && (f as BinaryFilter).Conditions.Count == 0 && ViewboxSession.TempTableObjects[id] != null && ViewboxSession.TempTableObjects.LastOrDefault() != null)
				{
					f = ((id < 0) ? ViewboxSession.TempTableObjects[id].Filter : ViewboxSession.TempTableObjects.LastOrDefault().Filter);
				}
				if (f == null)
				{
					return PartialView("_DialogPartial", new DialogModel
					{
						DialogType = DialogModel.Type.Warning,
						Content = Resources.IncorrectInput,
						Buttons = new List<DialogModel.Button>
						{
							new DialogModel.Button
							{
								Caption = Resources.OK
							}
						}
					});
				}
				if (!save)
				{
					BinaryFilter binFilter = f as BinaryFilter;
					if (binFilter != null)
					{
						foreach (IFilter cf in binFilter.Conditions)
						{
							if (!ValidateFilter(cf))
							{
								return PartialView("_DialogPartial", new DialogModel
								{
									DialogType = DialogModel.Type.Warning,
									Content = Resources.IncorrectInput,
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
				}
				if (!save)
				{
					foreach (IFilter item3 in (f as BinaryFilter).Conditions)
					{
						if (item3 is ColValueFilter && (item3 as ColValueFilter).Column != null && item3 is ColValueFilter && ((item3 as ColValueFilter).Column.DataType == SqlType.Decimal || (item3 as ColValueFilter).Column.DataType == SqlType.Numeric) && (item3 as ColValueFilter).Op != Operators.Like && (item3 as ColValueFilter).Value.ToString() != "∅" && !double.TryParse((item3 as ColValueFilter).Value.ToString(), out var _))
						{
							return PartialView("_DialogPartial", new DialogModel
							{
								DialogType = DialogModel.Type.Warning,
								Content = Resources.IncorrectInput,
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
				if (save)
				{
					if (f != null)
					{
						foreach (IFilter condition in (f as BinaryFilter).Conditions)
						{
							if (!ValidateFilterSave(condition))
							{
								return PartialView("_DialogPartial", new DialogModel
								{
									DialogType = DialogModel.Type.Warning,
									Content = Resources.IncorrectInputForSave,
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
					int temp_id = id;
					if (temp_id < 0)
					{
						temp_id = ((ViewboxSession.TempTableObjects[id].BaseTableId < 0) ? ViewboxSession.TempTableObjects[id].OriginalTable.Id : ViewboxSession.TempTableObjects[id].BaseTableId);
						if (!ViewboxSession.TableObjects.Contains(temp_id))
						{
							ViewboxSession.AddTableObject(temp_id);
						}
						ViewboxSession.SetupTableColumns(temp_id);
					}
					ViewboxSession.SaveIssue(temp_id, f, descriptions, freeselection);
					if (f != null)
					{
						ViewboxSession.loadingIsVisible = false;
						return PartialView("_DialogPartial", new DialogModel
						{
							DialogType = DialogModel.Type.Info,
							Content = Resources.FilterIsTheSame + Environment.NewLine + string.Format(Resources.SaveFilterMessage, ViewboxApplication.SaveFilterIndexLimit),
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
			int obj_id = ((id < 0) ? ViewboxSession.TempTableObjects[id].OriginalTable.Id : id);
			if (groupSubTotal != null && groupSubTotal.ColumnList != null && groupSubTotal.ColumnList.Count > 0)
			{
				SqlType? type = SqlType.String;
				ITableObject table = null;
				table = ((id >= 0) ? ViewboxSession.TableObjects[id] : ViewboxSession.TempTableObjects[id].Table);
				if (table != null)
				{
					foreach (string item2 in groupSubTotal.ColumnList)
					{
						type = table.Columns.FirstOrDefault((IColumn x) => x.Name == item2)?.DataType;
					}
				}
				else
				{
					type = null;
				}
				if (!type.HasValue || (type != SqlType.Integer && type != SqlType.Decimal && type != SqlType.Numeric))
				{
					return PartialView("_DialogPartial", new DialogModel
					{
						DialogType = DialogModel.Type.Warning,
						Content = Resources.SubTotalWarning,
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
			ViewboxDb.TableObject tobj = ((id < 0) ? ViewboxSession.TempTableObjects[id] : null);
			SubTotalParameters previousGroupSubTotal = null;
			if (tobj != null)
			{
				previousGroupSubTotal = tobj.GroupSubTotal;
			}
			if (groupSubTotal == null || groupSubTotal.ColumnList == null || groupSubTotal.GroupList == null)
			{
				groupSubTotal = previousGroupSubTotal;
			}
			if ((f == null || (f is AndFilter && (f as AndFilter).Conditions.Count == 0) || (f is OrFilter && (f as OrFilter).Conditions.Count == 0)) && tobj != null && tobj.Table is IIssue && (tobj.Table as IIssue).IssueType != IssueType.StoredProcedure && (sortList == null || sortList.Count == 0) && canReturnDialogPartial && (summaList == null || summaList.Count == 0) && groupSubTotal == null && previousGroupSubTotal == null && !multipleOptimization)
			{
				string link = string.Empty;
				link = ((id < 0 && ViewboxSession.TempTableObjects[id].OriginalTable.Type == TableType.Issue) ? ((summaList != null) ? ("/DataGrid/Index/" + (ViewboxSession.TempTableObjects[id].OriginalTable as Issue).ExecutedIssueId) : ("/DataGrid/Index/" + (ViewboxSession.TempTableObjects[id].OriginalTable as Issue).OriginalId)) : ((id >= 0) ? ("/DataGrid/Index/" + obj_id) : ("/DataGrid/Index/" + ViewboxSession.TempTableObjects[id].PreviousTableId)));
				return PartialView("_DialogPartial", new DialogModel
				{
					Link = link
				});
			}
			if (!string.IsNullOrEmpty(sortListString))
			{
				if (sortList == null)
				{
					sortList = new SortCollection();
				}
				string[] array3 = sortListString.Split('#');
				foreach (string sort2 in array3)
				{
					string[] sortparts = sort2.Split('|');
					if (sortparts.Length == 2)
					{
						sortList.Add(new Sort
						{
							cid = int.Parse(sortparts[0]),
							dir = (SortDirection)int.Parse(sortparts[1])
						});
					}
				}
			}
			else if (sortList == null)
			{
				sortList = new SortCollection();
			}
			else if (sortList.Count == 1)
			{
				ViewboxDb.TableObject obj = ViewboxSession.TempTableObjects[id];
				if (fromSort && obj != null && obj.Sort != null && obj.Sort.Find((Sort item) => item.cid == sortList[0].cid && item.dir == sortList[0].dir) != null)
				{
					if (obj.Sort.Count == 1)
					{
						sortList.Clear();
					}
					else
					{
						List<Sort> sort = obj.Sort.FindAll((Sort item) => item.cid != sortList[0].cid);
						sortList.Clear();
						foreach (Sort currentSort in sort)
						{
							sortList.Add(currentSort.Clone());
						}
					}
				}
			}
			if (relationType == 1 && !string.IsNullOrEmpty(relationExtInfo))
			{
				object filterValue = null;
				AndFilter andFilter = f as AndFilter;
				if (andFilter?.Conditions.Any((IFilter w) => w is ColValueFilter) ?? false)
				{
					filterValue = (andFilter.Conditions.FirstOrDefault((IFilter w) => w is ColValueFilter) as ColValueFilter).Value;
				}
				if (filterValue != null)
				{
					Transformation bluelinetrans = Transformation.Create(relationExtInfo, filterValue.ToString(), download: true, relationColumnExtInfo);
					return PartialView("_DialogPartial", new DialogModel
					{
						DialogType = DialogModel.Type.Info,
						Title = Resources.DownloadBluelineDocuments,
						Key = bluelinetrans.Key,
						Content = string.Format(Resources.LongRunningDialogText, Resources.DownloadBluelineDocuments),
						Buttons = new List<DialogModel.Button>
						{
							new DialogModel.Button
							{
								Caption = Resources.LongRunningDialogCaption
							}
						}
					});
				}
			}
			object[] paramValues = ((id < 0) ? ViewboxSession.TempTableObjects[id].ParamValues : null);
			List<int> itemId = ((id < 0) ? ViewboxSession.TempTableObjects[id].ItemId : null);
			List<int> selectionTypes = ((id < 0) ? ViewboxSession.TempTableObjects[id].SelectionTypes : null);
			string[] displayValues = null;
			if (ViewboxSession.TempTableObjects[id] != null && ViewboxSession.TempTableObjects[id].DisplayValues != null)
			{
				displayValues = ((id < 0) ? ViewboxSession.TempTableObjects[id].DisplayValues.ToArray() : null);
			}
			int id2 = id;
			ITableObjectCollection tableObjects = ViewboxSession.TableObjects;
			IFullColumnCollection columns = ViewboxSession.Columns;
			IOptimization opt = ViewboxSession.Optimizations.LastOrDefault();
			SortCollection sortList2 = sortList;
			IFilter filter2 = f;
			object[] displayValues2 = displayValues;
			Transformation trans = Transformation.Create(id2, tableObjects, columns, opt, sortList2, filter2, paramValues, displayValues2, selectionTypes, itemId, scrollPosition, (RelationType)relationType, relationExtInfo, relationColumnExtInfo, summaList, originalColumnIds: true, groupSubTotal, multipleOptimization, optimizationSelected, trans_KeyChanged);
			string s = trans.Key;
			string title = "";
			int count = 0;
			if (sortList != null)
			{
				title += Resources.SortTitle;
				count++;
			}
			if (filter != null)
			{
				title += (string.IsNullOrWhiteSpace(title) ? "" : (" " + Resources.And + " "));
				title += Resources.FilterTitle;
				count++;
			}
			if (summaList != null)
			{
				title += (string.IsNullOrWhiteSpace(title) ? "" : (" " + Resources.And + " "));
				title += Resources.AggSum;
				count++;
			}
			if (groupSubTotal != null)
			{
				title += (string.IsNullOrWhiteSpace(title) ? "" : (" " + Resources.And + " "));
				title += Resources.SubTotalTitle;
				count++;
			}
			return PartialView("_DialogPartial", new DialogModel
			{
				DialogType = DialogModel.Type.Info,
				Title = title,
				Key = trans.Key,
				Content = string.Format((count > 1) ? Resources.LongRunningDialogTextWithPlural : Resources.LongRunningDialogText, title),
				Buttons = new List<DialogModel.Button>
				{
					new DialogModel.Button
					{
						Caption = Resources.LongRunningDialogCaption
					}
				}
			});
		}

		private void trans_KeyChanged(string key)
		{
			ViewboxSession.CurrentTransactionKey = key;
		}

		private DialogModel SortAndFilterRetransformation(int id, SortCollection sortList, IFilter filter)
		{
			object[] paramValues = ((id < 0) ? ViewboxSession.TempTableObjects[id].ParamValues : null);
			SubTotalParameters groupSubTotal = ((id < 0) ? ViewboxSession.TempTableObjects[id].GroupSubTotal : null);
			List<int> itemIds = ((id < 0) ? ViewboxSession.TempTableObjects[id].ItemId : null);
			string[] displayValues = ((id < 0) ? ViewboxSession.TempTableObjects[id].DisplayValues.ToArray() : null);
			List<int> selectionTypes = ((id < 0) ? ViewboxSession.TempTableObjects[id].SelectionTypes : null);
			List<int> SumColumnsIndex;
			if (id < 0 && ViewboxSession.TempTableObjects[id] != null && ViewboxSession.TempTableObjects[id].Sum != null)
			{
				try
				{
					SumColumnsIndex = new List<int>(ViewboxSession.TempTableObjects[id].Sum.Select((IColumn c) => c.Id));
				}
				catch (Exception)
				{
					SumColumnsIndex = null;
				}
			}
			else
			{
				SumColumnsIndex = null;
			}
			ITableObjectCollection tableObjects = ViewboxSession.TableObjects;
			IFullColumnCollection columns = ViewboxSession.Columns;
			IOptimization opt = ViewboxSession.Optimizations.LastOrDefault();
			object[] displayValues2 = displayValues;
			Transformation trans = Transformation.Create(id, tableObjects, columns, opt, sortList, filter, paramValues, displayValues2, selectionTypes, itemIds, 0, RelationType.Normal, "", "", SumColumnsIndex, originalColumnIds: false, groupSubTotal, multipleOptimization: false, null, trans_KeyChanged);
			string title = "";
			if (sortList != null)
			{
				title += Resources.SortTitle;
			}
			if (filter != null)
			{
				title += (string.IsNullOrEmpty(title) ? "" : (" " + Resources.And + " "));
				title += Resources.FilterTitle;
			}
			return new DialogModel
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
			};
		}

		public ActionResult RefreshFilterView(int id, string filter = "")
		{
			IFilter f = null;
			if (!string.IsNullOrWhiteSpace(filter))
			{
				f = ViewboxSession.GetFilter(id, filter, ViewboxSession.TempTableObjects, ViewboxSession.TableObjects, decode: false, forcedForCol: true, ViewboxSession.Columns);
			}
			return PartialView("_FilterSubTreePartial", new FilterModel(f, null));
		}

		public ActionResult Filter(int id, int column, string filter, SortCollection sortList, bool exactmatch = true, int scrollPosition = 0, bool reset = false)
		{
			string sFilter = string.Empty;
			if (!exactmatch)
			{
				if (filter == null)
				{
					filter = string.Empty;
				}
				filter = filter.Replace("\\", "\\\\").Replace("(", "\\(").Replace(")", "\\)");
				filter = filter.Replace("%", "\\%");
			}
			if (filter == "\"")
			{
				filter = "\\\"";
			}
			IColumn col = ((id >= 0) ? ViewboxSession.Columns[column] : ViewboxSession.TempTableObjects[id].Table.Columns[column]);
			if (col == null)
			{
				throw new HttpException(404, "Not Found");
			}
			if (col.DataType == SqlType.Decimal || col.DataType == SqlType.Numeric)
			{
				if (ViewboxSession.Language.CountryCode != null && ViewboxSession.Language.CountryCode.Contains("de"))
				{
					if (filter.Contains(".") && filter.Length > 1)
					{
						filter = filter.Replace(".", "");
					}
					filter = filter.Replace(",", ".");
				}
				if (exactmatch)
				{
					filter = filter.TrimStart('0');
					if (filter.Contains('.'))
					{
						filter = filter.TrimEnd('0');
					}
					if (filter.Length == 0 || filter.StartsWith("."))
					{
						filter = "0" + filter;
					}
					if (filter.EndsWith("."))
					{
						filter = filter.Substring(0, filter.Length - 1);
					}
				}
			}
			sFilter = ((!exactmatch) ? $"Like(%{column},\"%{filter}%\")" : $"Equal(%{column},\"{filter}\")");
			string columnName = string.Empty;
			if (id > 0)
			{
				columnName = ViewboxSession.Columns[column].Name;
			}
			else
			{
				columnName = ViewboxSession.TempTableObjects[id].Table.Columns[column].Name;
			}
			foreach (IColumn c2 in ViewboxSession.Columns.Where((IColumn c) => c.Name == columnName))
			{
				c2.ExactMatchUnchecked = !exactmatch;
			}
			if (id >= 0)
			{
				sFilter = ((!reset) ? $"And({sFilter},)" : "And()");
			}
			else
			{
				ViewboxDb.TableObject obj = ViewboxSession.TempTableObjects[id];
				if (obj.Filter == null)
				{
					sFilter = ((!reset) ? $"And({sFilter},)" : "And(,)");
				}
				else
				{
					string orig = obj.Filter.ToOriginalString();
					if (reset && !orig.Contains(column.ToString(CultureInfo.InvariantCulture)))
					{
						ViewboxDb.TableObject lastObj = ViewboxSession.TempTableObjects[id - 1];
						Match[] matches = new Regex("%(-{0,1}\\d+)", RegexOptions.None).Matches(orig).Cast<Match>().ToArray();
						if (lastObj != null)
						{
							column = lastObj.Table.Columns[columnName].Id;
						}
						Match[] array = matches;
						foreach (Match match in array)
						{
							if (int.TryParse(match.Value.Substring(1), out var colId))
							{
								IColumn matchedCol = ViewboxSession.Columns[colId];
								if (matchedCol != null && matchedCol.Name == ViewboxSession.Columns[column].Name && matchedCol.Table.TableName == ViewboxSession.TempTableObjects[id].Table.TableName)
								{
									column = matchedCol.Id;
									break;
								}
							}
						}
					}
					if (orig.Contains("Like"))
					{
						orig = orig.Replace("\\%", "%");
					}
					string filterToFind = string.Empty;
					if (orig.Contains($"NotEqual(%{column}"))
					{
						filterToFind = $"NotEqual(%{column}";
					}
					else if (orig.Contains($"Equal(%{column}"))
					{
						filterToFind = $"Equal(%{column}";
					}
					else if (orig.Contains($"Like(%{column}"))
					{
						filterToFind = $"Like(%{column}";
					}
					if (!string.IsNullOrEmpty(filterToFind))
					{
						int startPos = orig.IndexOf(filterToFind, StringComparison.Ordinal);
						string leftoverFilterPart = orig.Substring(startPos);
						object s = obj.Filter.GetColumnValue(col.Name);
						int endPos = CharIndexInString(leftoverFilterPart, ')', (from value in obj.Filter.GetColumnValue(col.Name).ToString()
							where value == ')'
							select value).Count() + 1);
						string replaceValue = leftoverFilterPart.Substring(0, endPos + 1);
						if (reset)
						{
							sFilter = orig.Replace(replaceValue + ",", "");
							sFilter = sFilter.Replace(replaceValue, "");
						}
						else
						{
							sFilter = orig.Replace(replaceValue, sFilter);
						}
					}
					else if (string.IsNullOrEmpty(filterToFind) && !reset)
					{
						sFilter = $"{orig.Substring(0, orig.Count() - 1)},{sFilter})";
					}
					else if (string.IsNullOrEmpty(filterToFind) && reset)
					{
						sFilter = obj.Filter.ToOriginalString();
					}
				}
				if (obj.Sort != null && obj.Sort.Count != 0)
				{
					if (sortList == null)
					{
						sortList = new SortCollection();
					}
					sortList.AddRange(obj.Sort);
				}
			}
			return SortAndFilter(id, sortList, sFilter, null, save: false, scrollPosition);
		}

		public PartialViewResult SearchColumns(int id, string search, string partial, string addedColumns = null)
		{
			DataGridModel model = new DataGridModel();
			if (addedColumns != null)
			{
				model.AlreadyAddedColumns = addedColumns.Split('&').ToList();
			}
			SortCollection sc = new SortCollection();
			if (id < 0)
			{
				model.TableObject = ViewboxSession.TempTableObjects[id];
				model.TableInfo = model.TableObject.Table;
			}
			else
			{
				model.TableInfo = ViewboxSession.TableObjects[id];
			}
			model.Search = search;
			return PartialView(partial, model);
		}

		public PartialViewResult UpdateColumnOrder(int id, int columnId, string columnOrder, bool refreshDataGrid = true)
		{
			ViewboxDb.TableObject obj = ViewboxSession.TempTableObjects[id];
			ITableObject table = ((id < 0) ? obj.Table : ViewboxSession.TableObjects[id]);
			string[] columnIds = columnOrder.Split(',');
			int ordinal = 0;
			string[] array = columnIds;
			foreach (string colId in array)
			{
				if (colId == columnId.ToString())
				{
					break;
				}
				ordinal++;
			}
			int tableId = id;
			if (id < 0)
			{
				tableId = ViewboxSession.TempTableObjects[id].OriginalTable.Id;
			}
			string data = ViewboxApplication.Database.SystemDb.GetTableColumnWidths(ViewboxSession.User.Id, tableId);
			Dictionary<string, string> columnWidthsByName = new Dictionary<string, string>();
			if (data != null)
			{
				string[] widthData = data.Split(',');
				for (int i = 0; i < table.Columns.Count; i++)
				{
					columnWidthsByName.Add(table.Columns.ElementAt(i).Name, widthData[i]);
				}
			}
			table.Columns[columnId].Ordinal = ordinal;
			ViewboxSession.Columns[columnId].Ordinal = ordinal;
			if (id < 0)
			{
				IColumn tempColumn3 = obj.Table.Columns.FirstOrDefault((IColumn t) => t.Id == columnId);
				table.Columns[tempColumn3.Name].Ordinal = ordinal;
			}
			if (data != null)
			{
				List<string> columnWidths = new List<string>();
				foreach (IColumn column in table.Columns)
				{
					columnWidths.Add(columnWidthsByName[column.Name]);
				}
				ViewboxApplication.Database.SystemDb.SaveColumnWidthSizes(tableId, string.Join(",", columnWidths.ToArray()), ViewboxSession.User);
			}
			columnOrder = string.Empty;
			if (id > 0)
			{
				ITableObject tableObject = ViewboxApplication.Database.SystemDb.Objects[table.Id];
				string[] array2 = columnIds;
				foreach (string columnItem2 in array2)
				{
					if (!string.IsNullOrWhiteSpace(columnOrder))
					{
						columnOrder += ",";
					}
					IColumn tempColumn2 = table.Columns.FirstOrDefault((IColumn x) => x.Id.ToString() == columnItem2);
					columnOrder += ColumnOrder(tableObject, tempColumn2.Name);
				}
				ViewboxApplication.Database.SystemDb.UpdateColumnOrder(ViewboxSession.User, tableObject, columnOrder);
			}
			else if (tableId != id && ViewboxSession.TableObjects[tableId] != null)
			{
				ITableObject tableObjectTemp = null;
				tableObjectTemp = ((obj.OriginalTable.Type != TableType.Issue) ? ViewboxApplication.Database.SystemDb.Objects[obj.OriginalTable.Id] : ViewboxApplication.Database.SystemDb.Objects.FirstOrDefault((ITableObject x) => x.Database == obj.OriginalTable.Database && x.TableName == obj.OriginalTable.TableName.Replace("_filter", string.Empty)));
				string tempColumnOrder = null;
				string[] array3 = columnIds;
				foreach (string columnItem in array3)
				{
					if (tempColumnOrder != null)
					{
						tempColumnOrder += ",";
						columnOrder += ",";
					}
					IColumn tempColumn = obj.Table.Columns.FirstOrDefault((IColumn t) => t.Id.ToString() == columnItem);
					tempColumnOrder += table.Columns[tempColumn.Name].Id;
					columnOrder += ColumnOrder(tableObjectTemp, tempColumn.Name);
				}
				ViewboxApplication.Database.SystemDb.UpdateColumnOrder(ViewboxSession.User, tableObjectTemp, columnOrder);
			}
			List<int> tempColOrder = new List<int>();
			if (id > 0)
			{
				tempColOrder = (from n in columnOrder.Split(',')
					select int.Parse(n)).ToList();
				if (ViewboxSession.PreviousColumnOrder.ContainsKey(tableId))
				{
					ViewboxSession.PreviousColumnOrder[tableId] = tempColOrder;
				}
				else
				{
					ViewboxSession.PreviousColumnOrder.Add(tableId, tempColOrder);
				}
			}
			else
			{
				Dictionary<int, string> otColDict = new Dictionary<int, string>();
				ITableObject ot = ViewboxSession.TempTableObjects[id].OriginalTable;
				foreach (IColumn item in ot.Columns)
				{
					otColDict.Add(item.Id, item.Name);
				}
				foreach (IColumn col in table.Columns)
				{
					int tmp = otColDict.FirstOrDefault((KeyValuePair<int, string> x) => x.Value == col.Name).Key;
					tempColOrder.Add(tmp);
				}
				if (ViewboxSession.PreviousColumnOrder.ContainsKey(tableId))
				{
					ViewboxSession.PreviousColumnOrder[tableId] = tempColOrder;
				}
				else
				{
					ViewboxSession.PreviousColumnOrder.Add(tableId, tempColOrder);
				}
			}
			if (refreshDataGrid)
			{
				DataGridModel model = new DataGridModel();
				model.TableInfo = table;
				if (obj != null)
				{
					model.TableObject = obj;
					model.DataTable = ViewboxSession.LoadDataTable(model.TableObject, model.TableObject.Optimization, 0L, 0, (obj.Table == null) ? null : obj.Table.PageSystem);
				}
				else
				{
					model.DataTable = ViewboxSession.LoadDataTable(model.TableInfo, 0L, 1);
				}
				return PartialView("_TableHeadPartial", model);
			}
			return null;
		}

		public PartialViewResult UpdateColumnWidth(int id, int columnID, int newWidth)
		{
			ViewboxDb.TableObject obj = ViewboxSession.TempTableObjects[id];
			ITableObject table = ((id < 0) ? obj.Table : ViewboxSession.TableObjects[id]);
			IColumn col = table.Columns.First((IColumn item) => item.Id == columnID);
			if (ViewboxSession.UserDefinedColumnWidths == null)
			{
				ViewboxSession.UserDefinedColumnWidths = new List<UserDefinedWidth>();
			}
			UserDefinedWidth savedWidth = ViewboxSession.UserDefinedColumnWidths.Find((UserDefinedWidth item) => item.tableName == col.Table.Name && item.colName == col.Name);
			if (savedWidth == null)
			{
				UserDefinedWidth udw = new UserDefinedWidth();
				udw.colName = col.Name;
				udw.tableName = col.Table.Name;
				udw.newWidth = newWidth;
				ViewboxSession.UserDefinedColumnWidths.Add(udw);
			}
			else
			{
				savedWidth.newWidth = newWidth;
			}
			return null;
		}

		private int CharIndexInString(string sourceString, char searchChar, int number)
		{
			int count = 0;
			for (int i = 0; i < sourceString.Length; i++)
			{
				if (sourceString[i] == searchChar)
				{
					count++;
					if (count == number)
					{
						return i;
					}
				}
			}
			return -1;
		}

		private int ColumnOrder(ITableObject table, string name)
		{
			string ids = string.Empty;
			IOrderedEnumerable<IColumn> columns = table.Columns.OrderBy((IColumn x) => x.Ordinal);
			foreach (IColumn item in columns)
			{
				if (item.Name == name)
				{
					return item.Id;
				}
			}
			return 0;
		}

		public ActionResult TransactionNumberChange(int id, string transactionNumber)
		{
			if (id < 0)
			{
				ViewboxDb.TableObject tempTableObject = ViewboxSession.TempTableObjects[id];
				id = tempTableObject.OriginalTable.Id;
			}
			ViewboxApplication.Database.SystemDb.SaveTransactionNumber(id, ViewboxSession.User.Id, transactionNumber);
			return PartialView("_DialogPartial", new DialogModel
			{
				DialogType = DialogModel.Type.Info,
				Class = "tono-change",
				Title = Resources.TransactionIdChanged,
				Content = string.Format(Resources.TransactionIdChangedTo, transactionNumber),
				Buttons = new List<DialogModel.Button>
				{
					new DialogModel.Button
					{
						Caption = Resources.OK
					}
				}
			});
		}

		public PartialViewResult ShowIssueDescriptionDialog(int id, string filter)
		{
			if (!string.IsNullOrWhiteSpace(filter))
			{
				IFilter f = ViewboxSession.GetFilter(id, filter, ViewboxSession.TempTableObjects, ViewboxSession.TableObjects, decode: false, forcedForCol: true, ViewboxSession.Columns);
				if (f == null)
				{
					return PartialView("_DialogPartial", new DialogModel
					{
						DialogType = DialogModel.Type.Warning,
						Content = Resources.IncorrectInput,
						Buttons = new List<DialogModel.Button>
						{
							new DialogModel.Button
							{
								Caption = Resources.OK
							}
						}
					});
				}
				ITableObject tobj = ((id < 0) ? ViewboxSession.TempTableObjects[id].Table : ViewboxSession.TableObjects[id]);
				List<Tuple<string, string, string, string, int, int>> inputs = new List<Tuple<string, string, string, string, int, int>>
				{
					new Tuple<string, string, string, string, int, int>(Resources.Issue, "text", "tableName", tobj.GetDescription(ViewboxSession.Language), 256, 0)
				};
				List<Tuple<string, string, string>> hidden = new List<Tuple<string, string, string>>
				{
					new Tuple<string, string, string>("countryCode", ViewboxSession.Language.CountryCode, "countryCode")
				};
				int j = 0;
				foreach (ILanguage m in ViewboxApplication.Languages)
				{
					hidden.Add(new Tuple<string, string, string>("descriptions[" + j + "].countryCode", m.CountryCode, ""));
					hidden.Add(new Tuple<string, string, string>("descriptions[" + j + "].descriptions", tobj.GetDescription(m), m.CountryCode + "_tableName"));
					j++;
				}
				int i = 1;
				bool nextHidden = false;
				foreach (Tuple<IFilter, IColumn, string, object, string> p in f.GetParameters(new List<Tuple<IFilter, IColumn, string, object, string>>()))
				{
					if (p.Item2 == null)
					{
						return PartialView("_DialogPartial", new DialogModel
						{
							DialogType = DialogModel.Type.Warning,
							Content = Resources.WrongParameter,
							Buttons = new List<DialogModel.Button>
							{
								new DialogModel.Button
								{
									Caption = Resources.OK
								}
							}
						});
					}
					string description = p.Item2.Descriptions[ViewboxSession.Language];
					if (string.IsNullOrWhiteSpace(description))
					{
						description = p.Item2.Name;
					}
					if (!nextHidden)
					{
						inputs.Add(new Tuple<string, string, string, string, int, int>(Resources.Parameter + " " + i, "text", p.Item3, description + " " + p.Item5, 256, 1));
						if (p.Item1 is BetweenFilter)
						{
							nextHidden = true;
						}
						i++;
					}
					else
					{
						hidden.Add(new Tuple<string, string, string>(p.Item3, description + " " + p.Item5, ""));
						nextHidden = false;
					}
					j = 0;
					foreach (ILanguage k in ViewboxApplication.Languages)
					{
						hidden.Add(new Tuple<string, string, string>("descriptions[" + j + "].descriptions", description + " " + p.Item5, k.CountryCode + "_" + p.Item3));
						j++;
					}
				}
				DialogModel dialog = new DialogModel
				{
					Title = Resources.UserDefinedIssueDescriptionsCaption,
					Content = ((ViewboxApplication.Languages.Count() > 1) ? Resources.UserDefinedIssueDescriptionsTextLanguages : Resources.UserDefinedIssueDescriptionsText),
					DialogType = DialogModel.Type.Info,
					Inputs = inputs,
					HiddenFields = hidden,
					Buttons = new List<DialogModel.Button>
					{
						new DialogModel.Button
						{
							Caption = Resources.Cancel,
							Data = false.ToString()
						},
						new DialogModel.Button
						{
							Caption = Resources.Submit,
							Data = true.ToString()
						}
					}
				};
				if (ViewboxApplication.Languages.Count() > 1)
				{
					dialog.Select = new List<Tuple<string, string, List<string>, List<string>, int>>
					{
						new Tuple<string, string, List<string>, List<string>, int>(Resources.LanguageSelection, "language", new List<string>(ViewboxApplication.Languages.Select((ILanguage l) => l.LanguageName)), new List<string>(ViewboxApplication.Languages.Select((ILanguage l) => l.CountryCode)), new List<ILanguage>(ViewboxApplication.Languages).IndexOf(ViewboxSession.Language))
					};
				}
				return PartialView("_ExtendedDialogPartial", dialog);
			}
			return null;
		}

		public PartialViewResult ReadExtendedData(string value, int parent, int child, string information)
		{
			List<string> result = new List<string>();
			IOptimization currentOptimization = ViewboxSession.Optimizations.LastOrDefault();
			string[] array = information.Split(',');
			foreach (string informationColumn in array)
			{
				int colId = 0;
				string tempData = string.Empty;
				if (int.TryParse(informationColumn, out colId))
				{
					tempData = ViewboxApplication.Database.IndexDatabase.GetExtendedColumnInformation(value.Trim(), parent, child, colId, currentOptimization, ViewboxSession.Language.CountryCode);
					if (!string.IsNullOrEmpty(tempData))
					{
						result.Add(tempData);
					}
					else
					{
						result.Add(Resources.NoAdditionalInformation);
					}
				}
			}
			return PartialView("_ExtendedColumnInformation", result);
		}

		public ActionResult ResetColumnOrder(int id, int start = 0, int size = 35, bool json = false, bool useEnlargeProperty = true)
		{
			bool reset = true;
			ViewboxDb.TableObject tobj = ViewboxSession.TempTableObjects[id];
			ITableObject obj = ((id < 0) ? tobj.Table : ViewboxSession.TableObjects[id]);
			int originalId = ((tobj != null && tobj.OriginalTable != null) ? tobj.OriginalTable.Id : 0);
			ITableObject orTable = ((id < 0) ? ViewboxSession.TempTableObjects[id].Table : ViewboxSession.TableObjects[id]);
			List<Tuple<int, string>> columnOrder = ViewboxApplication.Database.SystemDb.ResetColumnOrder(obj, ViewboxSession.User, orTable, originalId);
			if (obj != null)
			{
				List<IColumn> newList2 = obj.Columns.OrderBy((IColumn c) => c.Ordinal).ToList();
				int orId = 0;
				orId = ((obj.Id >= 0) ? obj.Id : tobj.BaseTableId);
				List<IColumn> newTempList = ViewboxApplication.Database.SystemDb.Objects[orId].Columns.OrderBy((IColumn c) => c.Ordinal).ToList();
				for (int j = 0; j < columnOrder.Count; j++)
				{
					Tuple<int, string> column2 = columnOrder[j];
					IColumn originalColumn2 = newList2.ElementAt(j);
					if (column2.Item2 != originalColumn2.Name)
					{
						IColumn columnFirstorDefault2 = newList2.FirstOrDefault((IColumn c) => c.Name == column2.Item2);
						if (columnFirstorDefault2 != null)
						{
							columnFirstorDefault2.Ordinal = j;
						}
					}
					IColumn originalTempColumn = newTempList.ElementAt(j);
					if (column2.Item2 != originalTempColumn.Name)
					{
						IColumn columnFirstorDefault = newTempList.FirstOrDefault((IColumn c) => c.Name == column2.Item2);
						if (columnFirstorDefault != null)
						{
							columnFirstorDefault.Ordinal = j;
						}
					}
					IColumn objColumns = obj.Columns.ElementAt(j);
					if (column2.Item2 != objColumns.Name)
					{
						IColumn columnFirstOrDefault3 = obj.Columns.FirstOrDefault((IColumn c) => c.Name == column2.Item2);
						if (columnFirstOrDefault3 != null)
						{
							columnFirstOrDefault3.Ordinal = j;
						}
					}
					if (tobj == null)
					{
						continue;
					}
					IColumn tobjColumns = tobj.Table.Columns.ElementAt(j);
					if (column2.Item2 != tobjColumns.Name)
					{
						IColumn columnFirstOrDefault2 = tobj.Table.Columns.FirstOrDefault((IColumn c) => c.Name == column2.Item2);
						if (columnFirstOrDefault2 != null)
						{
							columnFirstOrDefault2.Ordinal = j;
						}
					}
				}
			}
			else
			{
				List<IColumn> newList = ViewboxApplication.Database.SystemDb.Objects[id].Columns.OrderBy((IColumn c) => c.Ordinal).ToList();
				for (int i = 0; i < columnOrder.Count; i++)
				{
					Tuple<int, string> column = columnOrder[i];
					IColumn originalColumn = newList.ElementAt(i);
					if (column.Item2 != originalColumn.Name)
					{
						IColumn columnFirstOrDefault = newList.FirstOrDefault((IColumn c) => c.Name == column.Item2);
						if (columnFirstOrDefault != null)
						{
							columnFirstOrDefault.Ordinal = i;
						}
					}
				}
			}
			ViewboxApplication.Database.SystemDb.ResetColumnWidthsOnResetColumnOrder(obj, ViewboxSession.User, originalId);
			bool resetColumnOrder = true;
			return RedirectToAction("Index", new { id, start, useEnlargeProperty, json, size, resetColumnOrder, reset });
		}

		[ValidateInput(false)]
		public PartialViewResult ShowRelationPartial(int columnId, string description, string rowValues)
		{
			IColumn column = ViewboxSession.Columns[columnId];
			return PartialView("_RelationPartial", new RelationModel(description, rowValues.Split(new string[1] { ";;DIV;;" }, StringSplitOptions.None).ToList(), column.Table, new List<IRelation>(column.Table.Relations[column])));
		}

		public PartialViewResult ShowRelationPartialNew(int tableId, int columnId, int rowNo)
		{
			IColumn column = ViewboxSession.Columns[columnId];
			string descript = string.Empty;
			ILanguage nat = null;
			for (int i = 0; i < column.Descriptions.Count(); i++)
			{
				if (column.Descriptions.ToList()[i].Key.ToString() == ViewboxApplication.Database.SystemDb.DefaultLanguage.CountryCode)
				{
					descript = column.Descriptions.ToList()[i].Value;
					nat = ViewboxApplication.Database.SystemDb.DefaultLanguage;
				}
			}
			int size = CalculateStringSize(descript, 51);
			if (descript.Length > size)
			{
				string newDescription = descript.Substring(0, size) + "...";
				column.Descriptions[nat] = newDescription;
			}
			ViewboxDb.TableObject tobj = ViewboxSession.TempTableObjects[tableId];
			ITableObject obj = ((tableId < 0) ? tobj.Table : ViewboxSession.TableObjects[tableId]);
			return PartialView("_RelationPartialNew", new RelationModelNew(column, rowNo, new List<IRelation>(obj.Relations[column])));
		}

		private int CalculateStringSize(string tmp, int dest)
		{
			char[] narrow5 = new char[11]
			{
				'f', 'i', 'j', 'l', 't', '(', ')', '.', ',', '-',
				'I'
			};
			char[] wide25 = new char[25]
			{
				'w', 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'J',
				'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T',
				'U', 'V', 'X', 'Y', 'Z'
			};
			char[] wide26 = new char[1] { 'W' };
			int pieces = tmp.Length;
			float siz = 0f;
			for (int i = 0; i < tmp.Length; i++)
			{
				siz = (narrow5.Contains(tmp[i]) ? (siz + 0.5f) : (wide25.Contains(tmp[i]) ? (siz + 1.2f) : ((!wide26.Contains(tmp[i])) ? (siz + 1f) : (siz + 1.5f))));
				if (siz > 50f)
				{
					pieces = i;
					break;
				}
			}
			return pieces;
		}

		public JsonResult ReloadVisibleColumns(int id, bool sortierung = false)
		{
			List<string[]> columnsData = new List<string[]>();
			ITableObject table = ((id < 0) ? ViewboxSession.TempTableObjects[id].Table : ViewboxSession.TableObjects[id]);
			ViewboxDb.TableObject tempTableObject = ViewboxSession.TempTableObjects[id];
			bool sumOn = false;
			if (id < 0)
			{
				sumOn = ViewboxSession.TempTableObjects[id].Sum != null;
			}
			if (table != null)
			{
				foreach (IColumn column in table.Columns)
				{
					try
					{
						if (sortierung && tempTableObject != null && tempTableObject.Sort != null)
						{
							if (column.IsVisible && !tempTableObject.Sort.Any((Sort s) => s.cid == column.Id))
							{
								columnsData.Add(new string[4]
								{
									column.Id.ToString(),
									(column.Ordinal + 1).ToString(),
									column.GetDescription(),
									""
								});
							}
						}
						else if ((column.IsVisible || sumOn) && tempTableObject != null && tempTableObject.Sum != null && !tempTableObject.Sum.Contains(column))
						{
							columnsData.Add(new string[4]
							{
								column.Id.ToString(),
								(column.Ordinal + 1).ToString(),
								column.GetDescription(),
								""
							});
						}
						else if (column.IsVisible)
						{
							columnsData.Add(new string[4]
							{
								column.Id.ToString(),
								(column.Ordinal + 1).ToString(),
								column.GetDescription(),
								""
							});
						}
						else
						{
							columnsData.Add(new string[4]
							{
								column.Id.ToString(),
								(column.Ordinal + 1).ToString(),
								column.GetDescription(),
								" style=\"display: none;\""
							});
						}
					}
					catch (Exception)
					{
						if (column.IsVisible || sumOn)
						{
							columnsData.Add(new string[4]
							{
								column.Id.ToString(),
								(column.Ordinal + 1).ToString(),
								column.GetDescription(),
								""
							});
						}
					}
				}
				return Json(columnsData.ToArray(), JsonRequestBehavior.AllowGet);
			}
			return null;
		}

		public void GetInformationFromScripts()
		{
			GenerateTableJob job = GenerateTableJob.Create();
		}

		public PartialViewResult GenerateConfirmationDialog()
		{
			return GenerateConfirmationDialog("Do admin job", "Start an Admin job");
		}

		[ActionName("GenerateConfirmationDialogOverloaded")]
		public PartialViewResult GenerateConfirmationDialog(string dialogTitle, string dialogContent)
		{
			dialogTitle = Resources.ResourceManager.GetString(dialogTitle) ?? dialogTitle;
			dialogContent = Resources.ResourceManager.GetString(dialogContent) ?? dialogContent;
			return PartialView("_DialogPartial", new DialogModel
			{
				DialogType = DialogModel.Type.Info,
				Title = dialogTitle,
				Content = string.Format(Resources.Proceed, dialogContent),
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

		public PartialViewResult GetDataLoading()
		{
			return PartialView("_DialogPartial", new DialogModel
			{
				DialogType = DialogModel.Type.Info,
				Content = Resources.ReadingDataPleaseWait
			});
		}

		[HttpPost]
		public JsonResult SummarizeColumns(string tableName, IList<string> columnNames)
		{
			Dictionary<string, decimal> summarizedColumns = ViewboxSession.SummarizeColumns(tableName, columnNames);
			summarizedColumns.Add("Test", 56m);
			summarizedColumns.Add("Test2", 100m);
			return Json(summarizedColumns.ToArray(), JsonRequestBehavior.DenyGet);
		}

		public ActionResult ExcelFilter(int id, int columnId, List<string> filtervalues)
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("OR(");
			foreach (string filtervalue in filtervalues)
			{
				sb.Append($"Equal(%{columnId},\"{filtervalue}\"),");
			}
			sb.Append(")");
			return SortAndFilter(id, null, sb.ToString(), null);
		}

		public ActionResult SetSubToTalProperties(int id, IList<int> groupColumnId = null, IList<int> subtotalList = null, bool onlyExpectedResult = false)
		{
			IFilter filter = new AndFilter();
			List<int> summaList = null;
			SortCollection sortCollection = new SortCollection();
			SubTotalParameters groupSubTotal = null;
			if (subtotalList == null && groupColumnId != null)
			{
				return PartialView("_DialogPartial", new DialogModel
				{
					DialogType = DialogModel.Type.Warning,
					Title = Resources.Warning,
					Content = Resources.NoSelectedSubtotalColumn,
					Buttons = new List<DialogModel.Button>
					{
						new DialogModel.Button
						{
							Caption = Resources.OK
						}
					}
				});
			}
			if (groupColumnId == null && subtotalList != null)
			{
				return PartialView("_DialogPartial", new DialogModel
				{
					DialogType = DialogModel.Type.Warning,
					Title = Resources.Warning,
					Content = Resources.NoSelectedGroupColumn,
					Buttons = new List<DialogModel.Button>
					{
						new DialogModel.Button
						{
							Caption = Resources.OK
						}
					}
				});
			}
			if (id < 0)
			{
				ViewboxDb.TableObject tobj2 = ViewboxSession.TempTableObjects[id];
				filter = ViewboxSession.TempTableObjects[id].Filter;
				summaList = ((ViewboxSession.TempTableObjects[id].Sum == null) ? null : ViewboxSession.TempTableObjects[id].Sum.Select((IColumn c) => c.Id).ToList());
				sortCollection = ViewboxSession.TempTableObjects[id].Sort;
				if (groupColumnId != null && groupColumnId.Count != 0 && subtotalList != null)
				{
					List<string> subtotalNameList2 = new List<string>();
					List<string> groupColumn2 = new List<string>();
					foreach (int groupId2 in groupColumnId)
					{
						groupColumn2.Add(tobj2.Table.Columns.FirstOrDefault((IColumn x) => x.Id == groupId2).Name);
					}
					if (subtotalList != null && groupColumn2 != null)
					{
						foreach (int item2 in subtotalList)
						{
							IColumn column2 = tobj2.Table.Columns.FirstOrDefault((IColumn x) => x.Id == item2);
							if (column2 != null)
							{
								SqlType type = column2.DataType;
								if (type != SqlType.Integer && type != SqlType.Numeric && type != SqlType.Decimal)
								{
									tobj2.GroupSubTotal = null;
									return PartialView("_DialogPartial", new DialogModel
									{
										DialogType = DialogModel.Type.Warning,
										Title = Resources.NoNumberInTable,
										Content = Resources.NoNumberColumn,
										Buttons = new List<DialogModel.Button>
										{
											new DialogModel.Button
											{
												Caption = Resources.OK
											}
										}
									});
								}
								subtotalNameList2.Add(column2.Name);
							}
						}
						groupSubTotal = new SubTotalParameters
						{
							GroupList = groupColumn2,
							ColumnList = subtotalNameList2,
							OnlyExpectedResult = onlyExpectedResult
						};
					}
				}
				else
				{
					tobj2.GroupSubTotal = null;
				}
			}
			else
			{
				ITableObject tobj = ViewboxSession.TableObjects[id];
				if (groupColumnId != null && groupColumnId.Count != 0 && subtotalList != null)
				{
					List<string> subtotalNameList = new List<string>();
					List<string> groupColumn = new List<string>();
					foreach (int groupId in groupColumnId)
					{
						groupColumn.Add(tobj.Columns.FirstOrDefault((IColumn x) => x.Id == groupId).Name);
					}
					if (subtotalList != null && groupColumn != null)
					{
						foreach (int item in subtotalList)
						{
							IColumn column = tobj.Columns.FirstOrDefault((IColumn x) => x.Id == item);
							if (column != null)
							{
								SqlType type = column.DataType;
								if (type != SqlType.Integer && type != SqlType.Numeric && type != SqlType.Decimal)
								{
									return PartialView("_DialogPartial", new DialogModel
									{
										DialogType = DialogModel.Type.Warning,
										Title = Resources.NoNumberInTable,
										Content = Resources.NoNumberColumn,
										Buttons = new List<DialogModel.Button>
										{
											new DialogModel.Button
											{
												Caption = Resources.OK
											}
										}
									});
								}
								subtotalNameList.Add(column.Name);
							}
						}
						groupSubTotal = new SubTotalParameters
						{
							GroupList = groupColumn,
							ColumnList = subtotalNameList,
							OnlyExpectedResult = onlyExpectedResult
						};
					}
				}
			}
			return SortAndFilter(id, sortCollection, (filter != null) ? filter.ToOriginalString() : string.Empty, null, save: false, 0, canReturnDialogPartial: true, "", urlDecode: false, 0, "", "", summaList, groupSubTotal);
		}

		public DataGridModel SubToTalCalculation(DataGridModel model, string filter = null)
		{
			if (model.TableObject.GroupSubTotal == null || model.GroupColumn == null || model.SubtotalColumns == null)
			{
				return model;
			}
			DataTable tempDataTable = model.DataTable.Clone();
			model.subtotalRows = new List<int>();
			int index = 0;
			foreach (object row5 in model.DataTable.Rows)
			{
				DataRow dataRow = row5 as DataRow;
				if (dataRow != null)
				{
					tempDataTable.Rows.Add(dataRow.ItemArray);
				}
			}
			EnumerableDataRowGroupList<DataRow> enumerableRowCollection = new EnumerableDataRowGroupList<DataRow>(tempDataTable.Rows)
			{
				GroupColumnNameList = model.GroupColumn.Select((IColumn x) => x.Name).ToList()
			};
			Func<DataRow, string> groupFunc = enumerableRowCollection.GroupData;
			IEnumerable<IGrouping<string, DataRow>> groupByRows2 = enumerableRowCollection.GroupBy(groupFunc);
			IGrouping<string, DataRow>[] groupByRows = groupByRows2.Distinct().ToArray();
			if (groupByRows.Count() > 1 && model.FromRow == 0L && model.PageCount > 1)
			{
				int groupRowsCount3 = 0;
				model.DataTable.Rows.Clear();
				for (int m = 0; m < groupByRows.Count() - 1; m++)
				{
					DataRow rowSub3 = model.DataTable.NewRow();
					IGrouping<string, DataRow> group4 = groupByRows[m];
					foreach (DataRow row10 in group4)
					{
						model.DataTable.Rows.Add(row10.ItemArray);
						groupRowsCount3++;
						index++;
						foreach (IColumn item6 in model.SubtotalColumns)
						{
							if (decimal.TryParse(row10[item6.Name].ToString(), out var tmp3))
							{
								if (!string.IsNullOrEmpty(rowSub3[item6.Name].ToString()))
								{
									rowSub3[item6.Name] = Convert.ToDecimal(rowSub3[item6.Name]) + tmp3;
								}
								else
								{
									rowSub3[item6.Name] = tmp3;
								}
							}
							else
							{
								rowSub3[item6.Name] = tmp3;
							}
						}
					}
					model.subtotalRows.Add(index);
					model.DataTable.Rows.Add(rowSub3.ItemArray);
					index++;
				}
				for (int l = groupByRows.Count() - 1; l < groupByRows.Count(); l++)
				{
					IGrouping<string, DataRow> group5 = groupByRows[l];
					foreach (DataRow row11 in group5)
					{
						if (groupRowsCount3 < 35)
						{
							model.DataTable.Rows.Add(row11.ItemArray);
							groupRowsCount3++;
						}
					}
				}
			}
			else if (groupByRows.Count() > 1)
			{
				model.DataTable.Rows.Clear();
				int groupRowsCount2 = 0;
				for (int k = 0; k < groupByRows.Count() - 1; k++)
				{
					DataRow rowSub2 = model.DataTable.NewRow();
					IGrouping<string, DataRow> group2 = groupByRows[k];
					foreach (DataRow row8 in group2)
					{
						model.DataTable.Rows.Add(row8.ItemArray);
						groupRowsCount2++;
						index++;
					}
					if (k == 0)
					{
						foreach (IColumn item5 in model.SubtotalColumns)
						{
							try
							{
							}
							catch (ArgumentException)
							{
								DataRow rowTotalSubTemp3 = model.DataTable.NewRow();
								rowTotalSubTemp3.ItemArray = rowSub2.ItemArray.Clone() as object[];
								int oldColIdx3 = model.DataTable.Columns.IndexOf(item5.Name);
								DataTable dtClone3 = model.DataTable.Clone();
								dtClone3.Columns[oldColIdx3].DataType = typeof(string);
								foreach (DataRow row7 in model.DataTable.Rows)
								{
									dtClone3.ImportRow(row7);
								}
								model.DataTable = dtClone3;
								rowSub2 = model.DataTable.NewRow();
								rowSub2.ItemArray = rowTotalSubTemp3.ItemArray.Clone() as object[];
							}
						}
					}
					else
					{
						foreach (DataRow row6 in group2)
						{
							foreach (IColumn item4 in model.SubtotalColumns)
							{
								if (decimal.TryParse(row6[item4.Name].ToString(), out var tmp2))
								{
									if (!string.IsNullOrEmpty(rowSub2[item4.Name].ToString()))
									{
										rowSub2[item4.Name] = Convert.ToDecimal(rowSub2[item4.Name]) + tmp2;
									}
									else
									{
										rowSub2[item4.Name] = tmp2;
									}
								}
								else if (!string.IsNullOrEmpty(rowSub2[item4.Name].ToString()))
								{
									rowSub2[item4.Name] = Convert.ToDecimal(rowSub2[item4.Name]) + tmp2;
								}
								else
								{
									rowSub2[item4.Name] = tmp2;
								}
							}
						}
					}
					model.subtotalRows.Add(index);
					model.DataTable.Rows.Add(rowSub2.ItemArray);
					index++;
				}
				for (int j = groupByRows.Count() - 1; j < groupByRows.Count(); j++)
				{
					IGrouping<string, DataRow> group3 = groupByRows[j];
					foreach (DataRow row9 in group3)
					{
						if (groupRowsCount2 < 35)
						{
							model.DataTable.Rows.Add(row9.ItemArray);
							groupRowsCount2++;
						}
					}
				}
			}
			if (model.FromRow + model.RowsPerPage >= model.RowsCount)
			{
				model.DataTable.Rows.Clear();
				int groupRowsCount = 0;
				for (int i = 0; i < groupByRows.Count(); i++)
				{
					DataRow rowSub = model.DataTable.NewRow();
					IGrouping<string, DataRow> group = groupByRows[i];
					foreach (DataRow row3 in group)
					{
						model.DataTable.Rows.Add(row3.ItemArray);
						groupRowsCount++;
					}
					if (i == 0)
					{
						foreach (IColumn item2 in model.SubtotalColumns)
						{
							try
							{
							}
							catch (ArgumentException)
							{
								DataRow rowTotalSubTemp = model.DataTable.NewRow();
								rowTotalSubTemp.ItemArray = rowSub.ItemArray.Clone() as object[];
								int oldColIdx = model.DataTable.Columns.IndexOf(item2.Name);
								DataTable dtClone = model.DataTable.Clone();
								dtClone.Columns[oldColIdx].DataType = typeof(string);
								foreach (DataRow row2 in model.DataTable.Rows)
								{
									dtClone.ImportRow(row2);
								}
								model.DataTable = dtClone;
								rowSub = model.DataTable.NewRow();
								rowSub.ItemArray = rowTotalSubTemp.ItemArray.Clone() as object[];
							}
						}
					}
					else
					{
						foreach (DataRow row in group)
						{
							foreach (IColumn item in model.SubtotalColumns)
							{
								if (decimal.TryParse(row[item.Name].ToString(), out var tmp))
								{
									if (!string.IsNullOrEmpty(rowSub[item.Name].ToString()))
									{
										rowSub[item.Name] = Convert.ToDecimal(rowSub[item.Name]) + tmp;
									}
									else
									{
										rowSub[item.Name] = tmp;
									}
								}
								else if (!string.IsNullOrEmpty(rowSub[item.Name].ToString()))
								{
									rowSub[item.Name] = Convert.ToDecimal(rowSub[item.Name]) + tmp;
								}
								else
								{
									rowSub[item.Name] = tmp;
								}
							}
						}
					}
					model.subtotalRows.Add(model.DataTable.Rows.Count);
					model.DataTable.Rows.Add(rowSub.ItemArray);
				}
				DataRow rowTotalSub = model.DataTable.NewRow();
				foreach (IColumn item3 in model.SubtotalColumns)
				{
					try
					{
					}
					catch (ArgumentException)
					{
						DataRow rowTotalSubTemp2 = model.DataTable.NewRow();
						rowTotalSubTemp2.ItemArray = rowTotalSub.ItemArray.Clone() as object[];
						int oldColIdx2 = model.DataTable.Columns.IndexOf(item3.Name);
						DataTable dtClone2 = model.DataTable.Clone();
						dtClone2.Columns[oldColIdx2].DataType = typeof(string);
						foreach (DataRow row4 in model.DataTable.Rows)
						{
							dtClone2.ImportRow(row4);
						}
						model.DataTable = dtClone2;
						rowTotalSub = model.DataTable.NewRow();
						rowTotalSub.ItemArray = rowTotalSubTemp2.ItemArray.Clone() as object[];
					}
				}
				model.fulltotalRow = model.DataTable.Rows.Count;
				model.DataTable.Rows.Add(rowTotalSub.ItemArray);
			}
			return model;
		}

		public DataGridModel SubtotalGroupCalculation(DataGridModel model, string filter = null)
		{
			return model;
		}

		public PartialViewResult GetValidationDialog()
		{
			return PartialView("_DialogPartial", new DialogModel
			{
				DialogType = DialogModel.Type.Info,
				Title = Resources.ValidatingInputs,
				Content = string.Format(Resources.ValidatingInputsPleaseWait)
			});
		}

		public JsonResult OptimisationList(int level = 0, int id = 0)
		{
			IEnumerable<IOptimization> optimisationList = ViewboxSession.AllowedOptimizations.Where((IOptimization t) => t.Level == level && t.Parent.Id == id);
			List<string[]> result = new List<string[]>();
			foreach (IOptimization optimisationItem in optimisationList)
			{
				string[] item = new string[2]
				{
					optimisationItem.Id.ToString(),
					(optimisationItem.Level != 4) ? (optimisationItem.Value + " - " + optimisationItem.GetDescription()) : optimisationItem.Value
				};
				if (item[1] != null)
				{
					result.Add(item);
				}
			}
			result = result.OrderBy((string[] t) => t[1]).ToList();
			return Json(result.ToArray(), JsonRequestBehavior.AllowGet);
		}
	}
}
