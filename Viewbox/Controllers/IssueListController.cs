using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using SystemDb;
using SystemDb.Internal;
using AV.Log;
using DbAccess;
using log4net;
using Utils;
using Viewbox.Enums;
using Viewbox.Models;
using Viewbox.Models.Wertehilfe;
using Viewbox.Properties;
using ViewboxDb;

namespace Viewbox.Controllers
{
	[Authorize]
	[ViewboxFilter]
	public class IssueListController : TableObjectControllerBase
	{
		private ILog log = LogHelper.GetLogger();

		public ActionResult Index(int id = -1, string search = null, int sortColumn = 0, SortDirection direction = SortDirection.Ascending, bool json = false, int page = 0, int size = 25, bool catRefresh = false, bool showHidden = false)
		{
			IOptimization opt = ViewboxSession.Optimizations.FirstOrDefault();
			if (opt == null || ViewboxSession.IssueCount == 0 || ViewboxSession.HideIssuesButton)
			{
				return RedirectToAction("Index", "Home");
			}
			ViewboxSession.ClearSavedDocumentSettings("IssueList");
			IIssue issue = ((id >= 0) ? (ViewboxSession.TableObjects[id] as IIssue) : ((ViewboxApplication.Database.SystemDb.UserLastIssueSettingsCollection[ViewboxSession.User] != null) ? (ViewboxSession.TableObjects[ViewboxApplication.Database.SystemDb.UserLastIssueSettingsCollection[ViewboxSession.User].LastUsedIssue] as IIssue) : ViewboxSession.Issues.FirstOrDefault()));
			if (issue != null)
			{
				id = issue.Id;
			}
			if (ViewboxSession.HasOptChanged)
			{
				ViewboxSession.ClearSavedDocumentSettings("IssueList");
				if (issue != null && issue.Parameters != null && issue != null && issue.Parameters != null)
				{
					foreach (IParameter param in issue.Parameters)
					{
						if (param.HistoryValues == null)
						{
							param.HistoryValues = new HistoryParameterValueCollection();
						}
						ViewboxApplication.Database.SystemDb.AddParameterHistory(param.HistoryValues, ViewboxSession.User.Id, param.Id, "");
					}
				}
			}
			ViewboxSession.IssueOptimizationFilter.Clear();
			if (issue != null && issue.Parameters != null && issue.Parameters.Any((IParameter x) => x.OptimizationType != OptimizationType.NotSet))
			{
				foreach (IParameter param2 in issue.Parameters)
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
					foreach (IOptimization item in ViewboxSession.AllowedOptimizations)
					{
						if (item.Value != null)
						{
							if (item.Group.Type == OptimizationType.IndexTable && !allowedIndex.Contains(item.Value))
							{
								allowedIndex = allowedIndex + item.Value + ",";
							}
							else if (item.Group.Type == OptimizationType.SplitTable && !allowedSplit.Contains(item.Value))
							{
								allowedSplit = allowedSplit + item.Value + ",";
							}
							else if (item.Group.Type == OptimizationType.SortColumn && item.Parent != null && item.Parent.Group.Type == OptimizationType.SplitTable && !allowedSort.Contains(item.Parent.Value + ":" + item.Value))
							{
								allowedSort = allowedSort + item.Parent.Value + ":" + item.Value + ",";
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
			return RedirectToAction("ShowOne", new
			{
				id = id,
				isNoResult = "false"
			});
		}

		public void CheckIssues(IssueList model)
		{
			bool check = false;
			IEnumerable<ITableObject> enumerable;
			if (!(model.TableObjects is IEnumerable<IIssue>))
			{
				enumerable = model.TableObjects;
			}
			else
			{
				IEnumerable<ITableObject> selectedIssues = model.SelectedIssues;
				enumerable = selectedIssues;
			}
			IEnumerable<ITableObject> tableObjects = enumerable;
			foreach (ITableObject tableobject in tableObjects)
			{
				IIssue Issue = tableobject as IIssue;
				if (Issue != null)
				{
					model.IssuesCheck = true;
					break;
				}
			}
		}

		public ActionResult IndexList(int id = -1, string search = null, int sortColumn = 0, SortDirection direction = SortDirection.Ascending, bool json = false, int page = 0, int size = 25, bool catRefresh = false, bool showHidden = false, int? objectTypeFilter = null, OperationEnum operation = OperationEnum.Sort)
		{
			if (objectTypeFilter.HasValue)
			{
				if (ViewboxApplication.Database.SystemDb.ObjectTypeRelationsCollection.IsEmpty)
				{
					ViewboxSession.User.Settings[UserSettingsType.Report_ObjectTypeFilter] = ((objectTypeFilter.Value != -1) ? objectTypeFilter.Value.ToString() : null);
					log.InfoFormat("UserSettingsType.Report_ObjectTypeFilter value setted. New value: {0}", ViewboxSession.User.Settings[UserSettingsType.Report_ObjectTypeFilter]);
				}
				else
				{
					ViewboxSession.User.Settings[UserSettingsType.Report_ObjectTypeFilter] = null;
					ViewboxSession.User.Settings[UserSettingsType.Extended_ObjectType] = ((objectTypeFilter.Value != -1) ? objectTypeFilter.Value.ToString() : null);
				}
			}
			if (operation == OperationEnum.Filter || operation == OperationEnum.Sort)
			{
				page = 0;
				if (ViewboxSession.IssueSortAndFilterSettings != null && ViewboxSession.IssueSortAndFilterSettings.SortColumn == sortColumn && ViewboxSession.IssueSortAndFilterSettings.Sort == direction && operation == OperationEnum.Sort)
				{
					ViewboxSession.IssueSortAndFilterSettings.SortColumn = (sortColumn = 0);
				}
			}
			IssueList model = GetModel(isSingleResult: true, id, search, sortColumn, direction, page, size, showHidden, ViewboxSession.User.Settings[UserSettingsType.Report_ObjectTypeFilter].ToNullableInt(), ViewboxSession.User.Settings[UserSettingsType.Extended_ObjectType].ToNullableInt());
			return json ? ((ViewResultBase)PartialView("_ReportListPopupPartial", model)) : ((ViewResultBase)View("Index", model));
		}

		public ActionResult ColumnValueList(string search = null, SortDirection direction = SortDirection.None, bool showHidden = false)
		{
			IssueList model = GetModel(isSingleResult: true, -1, search, 0, direction, 0, int.MaxValue, showHidden);
			IEnumerable<ValueListElement> valueList = from x in ViewboxApplication.Database.SystemDb.ObjectTypesCollection
				where model.TableObjects.Any((ITableObject y) => y.Type == TableType.Issue && y.ObjectTypes.Any((KeyValuePair<string, string> z) => z.Value == GetObjectTypeTextCollection(x)))
				select new ValueListElement
				{
					Value = GetObjectTypeTextCollection(x),
					Id = x.Key
				};
			if (direction != SortDirection.None)
			{
				valueList = ((direction == SortDirection.Ascending) ? valueList.OrderBy((ValueListElement v) => v.Value) : valueList.OrderByDescending((ValueListElement v) => v.Value));
			}
			log.InfoFormat("UserSettingsType.Report_ObjectTypeFilter current value: {0}", ViewboxSession.User.Settings[UserSettingsType.Report_ObjectTypeFilter]);
			object cachedObjectTypeText = (string.IsNullOrEmpty(ViewboxSession.User.Settings[UserSettingsType.Report_ObjectTypeFilter]) ? Resources.ShowAll : (from ot in valueList
				where ot.Id == ViewboxSession.User.Settings[UserSettingsType.Report_ObjectTypeFilter].ToNullableInt().Value
				select ot.Value).SingleOrDefault());
			ColumnValueListModels columnValueListModel = new ColumnValueListModels(valueList)
			{
				Direction = direction,
				PopupTitle = Resources.FilterOnField,
				AddShowAllItem = true
			};
			base.ViewBag.ListType = "IssueColumnValueList";
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

		public ActionResult ShowOne(int id = -1, bool showHidden = false, bool catRefresh = false, bool isNoResult = true)
		{
			IOptimization opt = ViewboxSession.Optimizations.LastOrDefault();
			IIssue issue = ((id >= 0) ? (ViewboxSession.TableObjects[id] as IIssue) : ViewboxApplication.Database.SystemDb.Issues.Where((IIssue i) => i.Database == opt.FindValue(OptimizationType.System)).FirstOrDefault());
			if (ViewboxSession.HasOptChanged)
			{
				if (issue != null && issue.Parameters != null)
				{
					foreach (IParameter param in issue.Parameters)
					{
						if (param.HistoryValues == null)
						{
							param.HistoryValues = new HistoryParameterValueCollection();
						}
						ViewboxApplication.Database.SystemDb.AddParameterHistory(param.HistoryValues, ViewboxSession.User.Id, param.Id, "");
						if (param.HistoryFreeSelectionValues == null)
						{
							param.HistoryFreeSelectionValues = new List<IHistoryParameterValueFreeSelection>();
						}
					}
				}
				ViewboxSession.User.Settings[UserSettingsType.Report_ObjectTypeFilter] = "";
				ViewboxSession.IssueSortAndFilterSettings = null;
				ViewboxSession.HasOptChanged = false;
			}
			ViewboxSession.IssueOptimizationFilter.Clear();
			ViewboxSession.AllowedOpts = null;
			if (issue != null && issue.Parameters != null && issue.Parameters.Any((IParameter x) => x.OptimizationType != OptimizationType.NotSet))
			{
				foreach (IParameter param2 in issue.Parameters)
				{
					if (param2.OptimizationType != 0 && !ViewboxSession.IssueOptimizationFilter.Contains(param2.OptimizationType))
					{
						ViewboxSession.IssueOptimizationFilter.Add(param2.OptimizationType);
					}
				}
				string allowedIndex = "";
				string allowedSplit = "";
				string allowedSort = "";
				foreach (IOptimization item in ViewboxSession.AllowedOptimizations)
				{
					if (item.Value != null)
					{
						if (item.Group.Type == OptimizationType.IndexTable && !allowedIndex.Contains(item.Value))
						{
							allowedIndex = allowedIndex + item.Value + ",";
						}
						else if (item.Group.Type == OptimizationType.SplitTable && !allowedSplit.Contains(item.Value))
						{
							allowedSplit = allowedSplit + item.Value + ",";
						}
						else if (item.Group.Type == OptimizationType.SortColumn && item.Parent != null && item.Parent.Group.Type == OptimizationType.SplitTable && !allowedSort.Contains(item.Parent.Value + ":" + item.Value))
						{
							allowedSort = allowedSort + item.Parent.Value + ":" + item.Value + ",";
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
			IssueList model = GetModel(isSingleResult: true, id, null, 0, SortDirection.Ascending, 0, 25, showHidden, ViewboxSession.User.Settings[UserSettingsType.Report_ObjectTypeFilter].ToNullableInt(), ViewboxSession.User.Settings[UserSettingsType.Extended_ObjectType].ToNullableInt());
			try
			{
				if (model.SelectedIssues == null || model.SelectedIssues.Count() == 0)
				{
					int tmpId = ((id == -1 && ViewboxSession.LastIssueId != 0) ? ViewboxSession.LastIssueId : id);
					IIssue tmpIssue = model.Issues.FirstOrDefault((IIssue x) => x.Id == tmpId);
					if (isNoResult && ViewboxSession.TempTableObjects.Count > 0 && ViewboxSession.TempTableObjects.LastOrDefault().Optimization.Id != opt.Id)
					{
						OptimizationManager.CheckAfterExecution(ViewboxSession.TempTableObjects.LastOrDefault());
						tmpId = ((ViewboxSession.LastIssueId != 0) ? ViewboxSession.LastIssueId : tmpId);
						tmpIssue = model.Issues.FirstOrDefault((IIssue x) => x.Id == tmpId);
					}
					if (isNoResult)
					{
						return RedirectToAction("ShowOne", new
						{
							id = (tmpIssue?.Id ?? model.Issues.FirstOrDefault().Id),
							showHidden = showHidden.ToString().ToLower(),
							catRefresh = catRefresh.ToString().ToLower(),
							isNoResult = "true"
						});
					}
					return RedirectToAction("ShowOne", new
					{
						id = (tmpIssue?.Id ?? model.Issues.FirstOrDefault().Id),
						showHidden = showHidden.ToString().ToLower(),
						catRefresh = catRefresh.ToString().ToLower()
					});
				}
			}
			catch (Exception)
			{
			}
			if (id != -1)
			{
				ViewboxSession.LastIssueId = id;
			}
			CheckIssues(model);
			SetParametersHasIndexErrorVariableValue(issue);
			if (!ViewboxApplication.Database.SystemDb.Issues.Any((IIssue i) => i.Database.ToLower() == opt.FindValue(OptimizationType.System).ToLower()) && !ViewboxSession.HideIssuesButton && opt != null && ViewboxApplication.Database.SystemDb.Views.Any((SystemDb.IView v) => v.Database.ToLower() == opt.FindValue(OptimizationType.System).ToLower()) && !ViewboxSession.HideViewsButton)
			{
				return Redirect("/ViewList");
			}
			if (!ViewboxApplication.Database.SystemDb.Issues.Any((IIssue i) => i.Database.ToLower() == opt.FindValue(OptimizationType.System).ToLower()) && opt != null && ViewboxSession.TableCount > 0 && !ViewboxSession.HideTablesButton)
			{
				return Redirect("/TableList");
			}
			if (!ViewboxApplication.Database.SystemDb.Issues.Any((IIssue i) => i.Database.ToLower() == opt.FindValue(OptimizationType.System).ToLower()) && (opt == null || ViewboxSession.TableCount <= 0 || ViewboxSession.HideTablesButton) && (opt == null || !ViewboxApplication.Database.SystemDb.Views.Any((SystemDb.IView v) => v.Database.ToLower() == opt.FindValue(OptimizationType.System).ToLower()) || ViewboxSession.HideViewsButton))
			{
				return Redirect("/Settings");
			}
			if (!model.SelectedIssues.Any())
			{
				return Redirect("/Settings");
			}
			if (catRefresh)
			{
				return PartialView("_ShowOneTableObjectOverview", model);
			}
			return SingleResultView(model, json: false);
		}

		private void RestoreFreeSelectionParameters(IIssue issue)
		{
			using DatabaseBase db = ViewboxApplication.Database.ConnectionManager.GetConnection();
			List<HistoryParameterValueFreeSelection> history = db.DbMapping.Load<HistoryParameterValueFreeSelection>($"userId = {ViewboxSession.User.Id} AND issueId = {issue.Id}");
			foreach (HistoryParameterValueFreeSelection h in history)
			{
				IParameter param = issue.Parameters.FirstOrDefault((IParameter p) => p.Id == h.ParameterId);
				param.HistoryFreeSelectionValues.Clear();
				param.HistoryFreeSelectionValues.Add(new HistoryParameterValueFreeSelection
				{
					IssueId = h.IssueId,
					Id = h.Id,
					ParameterId = h.ParameterId,
					SelectionType = h.SelectionType,
					UserId = h.UserId,
					Value = h.Value
				});
			}
		}

		private void SetParametersHasIndexErrorVariableValue(IIssue issue)
		{
			if (issue == null || issue.Parameters == null)
			{
				return;
			}
			IProperty currentProp = ViewboxApplication.FindProperty(ViewboxSession.User, "indexerrorprop");
			bool isTrue = false;
			bool.TryParse(currentProp.Value, out isTrue);
			isTrue = isTrue && ViewboxSession.IsAvAdmin;
			IOptimization currentOpt = ViewboxSession.Optimizations.LastOrDefault();
			string language = LanguageKeyTransformer.Transformer(ViewboxSession.User.CurrentLanguage);
			int userId = ViewboxSession.User.Id;
			Parallel.ForEach(issue.Parameters, delegate(IParameter param)
			{
				if (isTrue)
				{
					if (param.HasIndexData)
					{
						IWertehilfe wertehilfe = null;
						try
						{
							wertehilfe = WertehilfeFactory.Create(param.Id, "", isExact: true, onlyCheck: true);
							wertehilfe.Optimization = currentOpt;
							wertehilfe.Language = language;
							wertehilfe.BuildValuesCollection();
						}
						catch (Exception)
						{
						}
						param.IndexErrorStyleString = ((wertehilfe != null && wertehilfe.ValueListCollection != null && wertehilfe.ValueListCollection.Count() > 0) ? ";background-color: rgba(0, 128,0 , 0.6);" : ";background-color: rgba(255, 255 ,0 , 0.6);");
					}
					else
					{
						param.IndexErrorStyleString = ";background-color:rgba(255, 0, 0, 0.6);";
					}
				}
				else
				{
					param.IndexErrorStyleString = "";
				}
			});
		}

		public PartialViewResult CategoryTabs(int id = -1, string search = null, int sortColumn = 0, SortDirection direction = SortDirection.Ascending, int page = 0, int size = 25, bool showHidden = false)
		{
			IssueList model = new IssueList(id, search, showHidden);
			model.CurrentPage = page;
			model.PerPage = size;
			List<IIssue> list = model.GetIssueList(showHidden, sortColumn, direction);
			model.SortColumn = sortColumn;
			model.Direction = direction;
			model.Count = list.Count;
			model.ShowHidden = showHidden;
			return PartialView("_CategoryTabsPartial", model);
		}

		public ActionResult Sort(int id = -1, string search = null, int sortColumn = 0, SortDirection direction = SortDirection.Ascending, bool json = false, int page = 0, int size = 25, bool catRefresh = false, bool showHidden = false)
		{
			return Index(id, search, sortColumn, direction, json, page, size, catRefresh, showHidden);
		}

		public ActionResult DeleteIssue(int id, string search = null, int sortColumn = 0, SortDirection direction = SortDirection.Ascending, bool json = false, int page = 0, int size = 25, bool catRefresh = false, bool showHidden = false)
		{
			ViewboxSession.DeleteIssue(id);
			if (ViewboxSession.IssueCount == 0)
			{
				return null;
			}
			return Index(-1, search, sortColumn, direction, json, 0, size, catRefresh, showHidden);
		}

		public ActionResult HideTableObject(int id = -1, string search = null, int sortColumn = 0, SortDirection direction = SortDirection.Ascending, bool json = false, int page = 0, int size = 25, bool catRefresh = false, bool showHidden = false)
		{
			return Index(id, search, sortColumn, direction, json, page, size, catRefresh, showHidden);
		}

		public void ChangeTableOrder(int id, int ordinal)
		{
			ITableObject tobj = ((id < 0) ? ViewboxSession.TempTableObjects[id].Table : ViewboxSession.TableObjects[id]);
			ViewboxApplication.Database.SystemDb.UpdateTableObjectOrder(ViewboxSession.User, TableType.Issue, tobj);
		}

		public void SaveFavorite(int id)
		{
			ViewboxApplication.Database.SystemDb.SaveFavoriteIssue(ViewboxSession.User, id);
		}

		public void DeleteFavorite(int id)
		{
			ViewboxApplication.Database.SystemDb.DeleteFavoriteIssue(ViewboxSession.User, id);
		}

		public PartialViewResult GetConfirmationDialog()
		{
			return PartialView("_DialogPartial", new DialogModel
			{
				DialogType = DialogModel.Type.Info,
				Title = Resources.ExecuteIssueTitle,
				Content = string.Format(Resources.Proceed, Resources.ExecuteIssueTitle),
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

		public PartialViewResult GetValidationDialog()
		{
			return PartialView("_DialogPartial", new DialogModel
			{
				DialogType = DialogModel.Type.Info,
				Title = Resources.ValidatingInputs,
				Content = string.Format(Resources.ValidatingInputsPleaseWait)
			});
		}

		public PartialViewResult DeleteConfirmationDialog()
		{
			return PartialView("_DialogPartial", new DialogModel
			{
				DialogType = DialogModel.Type.Info,
				Title = Resources.DeleteIssueTitle,
				Content = string.Format(Resources.Proceed, Resources.DeleteIssueTitle),
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

		public PartialViewResult EditIssueDialog(int id)
		{
			IIssue issue = ViewboxSession.Issues[id];
			List<Tuple<string, string, string, string, int, int>> inputs = new List<Tuple<string, string, string, string, int, int>>
			{
				new Tuple<string, string, string, string, int, int>(Resources.Issue, "text", "tableName", issue.GetDescription(ViewboxSession.Language), 256, 0)
			};
			List<Tuple<string, string, string>> hidden = new List<Tuple<string, string, string>>
			{
				new Tuple<string, string, string>("countryCode", ViewboxSession.Language.CountryCode, "countryCode")
			};
			int j = 0;
			foreach (ILanguage m in ViewboxApplication.Languages)
			{
				hidden.Add(new Tuple<string, string, string>("descriptions[" + j + "].countryCode", m.CountryCode, ""));
				hidden.Add(new Tuple<string, string, string>("descriptions[" + j + "].descriptions", issue.GetDescription(m), m.CountryCode + "_tableName"));
				j++;
			}
			int i = 1;
			foreach (IParameter p in issue.Parameters)
			{
				inputs.Add(new Tuple<string, string, string, string, int, int>(Resources.Parameter + " " + i, "text", p.Name, p.GetDescription(), 256, 0));
				j = 0;
				foreach (ILanguage k in ViewboxApplication.Languages)
				{
					hidden.Add(new Tuple<string, string, string>("descriptions[" + j + "].descriptions", p.GetDescription(k), k.CountryCode + "_" + p.Name));
					j++;
				}
				i++;
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

		public ActionResult EditIssue(int id, DescriptionCollection descriptions, string search = null, int sortColumn = 0, SortDirection direction = SortDirection.Ascending, bool json = false, int page = 0, int size = 25, bool catRefresh = false, bool showHidden = false)
		{
			ViewboxApplication.Database.EditIssue(ViewboxSession.Issues[id], ViewboxApplication.Languages, descriptions);
			return IndexList(-1, search, sortColumn, direction, json, 0, size, catRefresh, showHidden);
		}

		public PartialViewResult ShowParameterGroupSaveDialog(int issueId, string[] parameterGroup)
		{
			List<Tuple<string, string, string, string, int, int>> inputs = new List<Tuple<string, string, string, string, int, int>>
			{
				new Tuple<string, string, string, string, int, int>(Resources.Issue, "text", "parameterGroupName", "parameterGroup", 256, 0)
			};
			List<Tuple<string, string, string>> hidden = new List<Tuple<string, string, string>>
			{
				new Tuple<string, string, string>("countryCode", ViewboxSession.Language.CountryCode, "countryCode")
			};
			hidden.Add(new Tuple<string, string, string>("issueId", issueId.ToString(), ""));
			int i = 0;
			foreach (ILanguage j in ViewboxApplication.Languages)
			{
				hidden.Add(new Tuple<string, string, string>("descriptions[" + i + "].countryCode", j.CountryCode, ""));
				hidden.Add(new Tuple<string, string, string>("descriptions[" + i + "].descriptions", "parameterGroup", j.CountryCode + "_parameterGroupName"));
				i++;
			}
			i = 0;
			foreach (string par in parameterGroup)
			{
				hidden.Add(new Tuple<string, string, string>("parametergroup[" + i + "]", par, ""));
				i++;
			}
			DialogModel dialog = new DialogModel
			{
				Title = Resources.IssueParameterGroupSaveCaption,
				Content = Resources.IssueParameterGroupSaveCaption,
				FormAction = "SaveParameterGroup",
				Inputs = inputs,
				HiddenFields = hidden,
				DialogType = DialogModel.Type.Info,
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

		public ActionResult _TableCrud(int id, int tableId)
		{
			ITableCrud crud = ViewboxApplication.TableCruds.GetCrud(tableId, id);
			if (crud != null)
			{
				return PartialView("_TableCrudOuter", new TableCrudModel(crud));
			}
			return new EmptyResult();
		}

		public ActionResult _TableCrudEdit(int tableId, int rowno)
		{
			ITableCrud crud = ViewboxApplication.TableCruds.GetCrudForTable(tableId);
			if (crud != null)
			{
				return PartialView("_TableCrudOuter", new TableCrudModel(crud, rowno));
			}
			return new EmptyResult();
		}

		public ActionResult _AddCrud(TableCrudModel model)
		{
			ITableCrud crud = ViewboxApplication.TableCruds.GetCrudForTable(model.TableId);
			if (crud == null)
			{
				return new EmptyResult();
			}
			StringBuilder sql = new StringBuilder();
			if (model.SubmitButton == Resources.Submit)
			{
				DatabaseBase conn = ViewboxApplication.Database.SystemDb.ConnectionManager.GetConnection();
				try
				{
					using IDbCommand command = conn.GetDbCommand();
					command.Connection = conn.Connection;
					if (model.Rowno > -1)
					{
						sql.Append("UPDATE ").Append(conn.Enquote(crud.Table.Database, crud.Table.TableName)).Append(" SET ");
						sql.Append(string.Join(", ", model.Columns.Select((TableCrudColumnModel w) => conn.Enquote(w.Name) + " = @param_" + w.Name)));
						sql.Append(" WHERE _ROW_NO_ = ").Append(model.Rowno);
						foreach (TableCrudColumnModel column2 in model.Columns)
						{
							IDbDataParameter pValue2 = command.CreateParameter();
							pValue2.ParameterName = "@param_" + column2.Name;
							pValue2.Value = column2.Value;
							pValue2.DbType = SqlTypeHelper.SqlTypeToDbType(column2.Type);
							command.Parameters.Add(pValue2);
						}
					}
					else
					{
						sql.Append("INSERT INTO ").Append(conn.Enquote(crud.Table.Database, crud.Table.TableName)).Append(" (");
						sql.Append(string.Join(", ", model.Columns.Select((TableCrudColumnModel w) => conn.Enquote(w.Name))));
						sql.Append(" ) VALUES (");
						bool first = true;
						foreach (TableCrudColumnModel column in model.Columns)
						{
							IDbDataParameter pValue = command.CreateParameter();
							pValue.ParameterName = "@param_" + column.Name;
							pValue.Value = column.Value;
							pValue.DbType = SqlTypeHelper.SqlTypeToDbType(column.Type);
							if (!first)
							{
								sql.Append(", ");
							}
							else
							{
								first = false;
							}
							sql.Append(pValue.ParameterName);
							command.Parameters.Add(pValue);
						}
						sql.Append(" );");
					}
					command.CommandText = sql.ToString();
					command.Prepare();
					command.ExecuteNonQuery();
				}
				finally
				{
					if (conn != null)
					{
						((IDisposable)conn).Dispose();
					}
				}
				return new EmptyResult();
			}
			model.CalculateFields();
			return PartialView("_TableCrud", model);
		}

		public ActionResult _DeleteCrud(int tableId, int rowno)
		{
			ITableCrud crud = ViewboxApplication.TableCruds.GetCrudForTable(tableId);
			if (crud != null)
			{
				using DatabaseBase conn = ViewboxApplication.Database.SystemDb.ConnectionManager.GetConnection();
				StringBuilder builder = new StringBuilder();
				builder.Append("DELETE FROM ").Append(conn.Enquote(crud.Table.Database, crud.Table.TableName)).Append(" WHERE _ROW_NO_ = ")
					.Append(rowno);
				conn.ExecuteNonQuery(builder.ToString());
			}
			return RedirectToAction("Index");
		}

		[HttpGet]
		public ActionResult UpdateIssueText(int issueId, string newName)
		{
			TableAndColumnSettingsModel model = TableAndColumnSettingsModel.Instance;
			using (DatabaseBase conn = ViewboxApplication.Database.ConnectionManager.GetConnection())
			{
				if (!conn.IsOpen)
				{
					conn.Open();
				}
				using IDataReader reader = conn.ExecuteReader("SELECT id FROM table_texts WHERE `ref_id` = " + issueId);
				if (reader.Read())
				{
					int id = reader.GetInt32(0);
				}
			}
			if (model.UpdateTable(issueId, newName))
			{
				return Json(true, JsonRequestBehavior.AllowGet);
			}
			return Json(false, JsonRequestBehavior.AllowGet);
		}

		protected override void OnException(ExceptionContext filterContext)
		{
			base.OnException(filterContext);
			IOptimization opt = ViewboxSession.Optimizations.FirstOrDefault();
			if (opt == null || ViewboxSession.IssueCount == 0 || ViewboxSession.HideIssuesButton)
			{
				RedirectToAction("Index", "Home");
			}
			IIssue to = (IIssue)ViewboxSession.TableObjects.FirstOrDefault((ITableObject t) => t.Type == TableType.Issue && t.Database == opt.FindValue(OptimizationType.System));
			if (to == null)
			{
				foreach (ITableObject i in ViewboxApplication.Database.SystemDb.Objects.Where((ITableObject t) => t.Type == TableType.Issue && t.Database == opt.FindValue(OptimizationType.System)))
				{
					if (i.IsAllowedInRightsMode())
					{
						to = (Issue)i;
						break;
					}
				}
			}
			filterContext.ExceptionHandled = true;
			filterContext.Result = ((to != null) ? RedirectToAction("ShowOne", "IssueList", new
			{
				id = to.Id
			}) : RedirectToAction("ShowOne", "IssueList"));
			ViewboxSession.HasOptChanged = true;
		}

		public void CleareParamaterList()
		{
			IIssue issue = ((ViewboxApplication.Database.SystemDb.UserLastIssueSettingsCollection[ViewboxSession.User] != null) ? (ViewboxSession.TableObjects[ViewboxApplication.Database.SystemDb.UserLastIssueSettingsCollection[ViewboxSession.User].LastUsedIssue] as IIssue) : ViewboxApplication.Database.SystemDb.Issues.FirstOrDefault());
			if (issue == null || issue.Parameters == null)
			{
				return;
			}
			foreach (IParameter param in issue.Parameters)
			{
				if (param.HistoryValues == null)
				{
					param.HistoryValues = new HistoryParameterValueCollection();
				}
				ViewboxApplication.Database.SystemDb.AddParameterHistory(param.HistoryValues, ViewboxSession.User.Id, param.Id, "");
				if (param.HistoryFreeSelectionValues == null)
				{
					param.HistoryFreeSelectionValues = new List<IHistoryParameterValueFreeSelection>();
				}
			}
		}
	}
}
