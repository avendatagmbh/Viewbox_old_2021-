using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using SystemDb;
using Viewbox.Properties;
using ViewboxDb;

namespace Viewbox.Models
{
	public class IssueList : ViewboxModel, ITableObjectList, ICategoryList
	{
		private ListOptionsModel _listOptions;

		private ListOptionsModel _visibleOptions;

		public IEnumerable<IIssue> Issues { get; set; }

		public IEnumerable<IIssue> FavoriteIssues { get; internal set; }

		public IEnumerable<IIssue> SelectedIssues { get; internal set; }

		public List<SelectListItem> SelectionList { get; set; }

		public ConcreteSelectionTypeFactory Selection { get; set; }

		public int VonBisOrdinal { get; set; }

		public IIssue SelectedIssue => Issues.FirstOrDefault();

		public override string LabelCaption => Resources.Issues;

		public List<SelectListItem> UserParameterGroupList { get; set; }

		public bool IssuesCheck { get; set; }

		public bool RightsMode => base.Header.RightsMode;

		public TableType Type => TableType.Issue;

		public ICategoryCollection Categories => ViewboxSession.IssueCategories;

		public ICategory SelectedCategory { get; private set; }

		public string SearchPhrase { get; private set; }

		public IEnumerable<ITableObject> TableObjects => Issues;

		public ITableObject SelectedTableObject => SelectedIssue;

		public string LabelSearch => Resources.IssueSearch;

		public string LabelEnterValue => Resources.LabelEnterValue;

		public string SelectedSystem => ViewboxSession.SelectedSystem;

		public bool ShowEmpty { get; internal set; }

		public bool ShowHidden { get; internal set; }

		public bool ShowArchived { get; internal set; }

		public bool ShowEmptyHidden { get; internal set; }

		public TableCounts TableCount { get; internal set; }

		public int Count { get; internal set; }

		public int CurrentPage { get; internal set; }

		public int PerPage { get; internal set; }

		public int LastPage
		{
			get
			{
				if (PerPage == 0)
				{
					return 0;
				}
				return (Count % PerPage == 0) ? (Count / PerPage - 1) : (Count / PerPage);
			}
		}

		public int From => CurrentPage * PerPage;

		public int To => Math.Min(Count, From + PerPage) - 1;

		public string LabelNumberOfRows => Resources.Issues;

		public string LabelNoData => Resources.NoData;

		public int SortColumn { get; internal set; }

		public SortDirection Direction { get; internal set; }

		public ListOptionsModel ArchiveListOptions
		{
			get
			{
				return _listOptions ?? (_listOptions = new ListOptionsModel());
			}
			set
			{
				_listOptions = value;
			}
		}

		public ListOptionsModel VisibleListOptions
		{
			get
			{
				return _visibleOptions ?? (_visibleOptions = new ListOptionsModel());
			}
			set
			{
				_visibleOptions = value;
			}
		}

		public IssueList(int id = -1, string search = null, bool selectNoCategory = false)
		{
			if (!selectNoCategory)
			{
				SelectedCategory = ViewboxSession.GetIssueCategoryById(id);
			}
			SearchPhrase = search?.ToLower();
		}

		public DialogModel GetWaitDialog(string action)
		{
			return new DialogModel
			{
				Title = Resources.PleaseWait,
				Content = string.Format(Resources.ExecutingAction, action),
				DialogType = DialogModel.Type.Info
			};
		}

		public List<IIssue> GetIssueList(bool showHidden = false, int sortColumn = 0, SortDirection direction = SortDirection.Ascending, bool isPdfExport = false)
		{
			IIssueCollection issues = (isPdfExport ? ViewboxApplication.Database.SystemDb.Issues : ViewboxSession.Issues);
			List<IIssue> list = new List<IIssue>();
			if (showHidden)
			{
				list.AddRange(from i in issues
					where i.Database == SelectedSystem && (SearchPhrase == null || i.GetDescription().ToLower().Contains(SearchPhrase) || i.TableName.ToLower().Contains(SearchPhrase) || i.TransactionNumber.ToString().StartsWith(SearchPhrase)) && !i.IsVisible
					orderby i.Ordinal
					select i);
			}
			else
			{
				list.AddRange(from i in issues
					where i.Category.Id == SelectedCategory.Id && i.Database == SelectedSystem && (SearchPhrase == null || i.GetDescription().ToLower().Contains(SearchPhrase) || i.TableName.ToLower().Contains(SearchPhrase) || i.TransactionNumber.ToString().StartsWith(SearchPhrase)) && i.IsVisible
					orderby i.Ordinal
					select i);
			}
			if (sortColumn == 1)
			{
				if (direction == SortDirection.Ascending)
				{
					list = new List<IIssue>(list.OrderBy((IIssue l) => l.TransactionNumber));
				}
				if (direction == SortDirection.Descending)
				{
					list = new List<IIssue>(list.OrderByDescending((IIssue l) => l.TransactionNumber));
				}
			}
			if (sortColumn == 2)
			{
				if (direction == SortDirection.Ascending)
				{
					list = new List<IIssue>(list.OrderBy((IIssue l) => l.GetDescription()));
				}
				if (direction == SortDirection.Descending)
				{
					list = new List<IIssue>(list.OrderByDescending((IIssue l) => l.GetDescription()));
				}
			}
			return list;
		}

		public List<IIssue> GetListFromSessionIssues(IEnumerable<int> exc, bool exclude, string search, ILanguage language, out int fullTablelistCount, int sortColumn = 0, SortDirection direction = SortDirection.Ascending, int page = 0, int size = 25)
		{
			List<IIssue> list = new List<IIssue>();
			IEnumerable<IIssue> temp = ViewboxSession.Issues.Where((IIssue t) => (exclude ? (!exc.Contains(t.Id)) : exc.Contains(t.Id)) && (search == null || ((t.Descriptions[language ?? ViewboxApplication.Database.SystemDb.DefaultLanguage] != null) ? t.Descriptions[language ?? ViewboxApplication.Database.SystemDb.DefaultLanguage].ToLower().Contains(search.ToLower()) : (t.TableName.ToLower().Contains(search.ToLower()) || t.GetDescription().ToLower().Contains(search.ToLower()))) || t.TransactionNumber.ToLower().Contains(search.ToLower())) && t.IsVisible);
			fullTablelistCount = temp.Count();
			switch (sortColumn)
			{
			case 1:
				if (direction == SortDirection.Ascending)
				{
					list = new List<IIssue>(temp.OrderBy((IIssue t) => t.TransactionNumber));
				}
				if (direction == SortDirection.Descending)
				{
					list = new List<IIssue>(temp.OrderByDescending((IIssue t) => t.TransactionNumber));
				}
				break;
			case 2:
				if (direction == SortDirection.Ascending)
				{
					list = new List<IIssue>(temp.OrderBy((IIssue l) => l.GetDescription()));
				}
				if (direction == SortDirection.Descending)
				{
					list = new List<IIssue>(temp.OrderByDescending((IIssue l) => l.GetDescription()));
				}
				break;
			case 3:
				if (direction == SortDirection.Ascending)
				{
					list = new List<IIssue>(temp.OrderBy((IIssue l) => l.GetObjectType()).ThenBy((IIssue t) => t.GetDescription()));
				}
				if (direction == SortDirection.Descending)
				{
					list = new List<IIssue>(temp.OrderByDescending((IIssue l) => l.GetObjectType()).ThenBy((IIssue t) => t.GetDescription()));
				}
				break;
			case 4:
				if (direction == SortDirection.Ascending)
				{
					list = new List<IIssue>(temp.OrderBy((IIssue l) => l.GetObjectType()).ThenBy((IIssue t) => t.GetDescription()));
				}
				if (direction == SortDirection.Descending)
				{
					list = new List<IIssue>(temp.OrderByDescending((IIssue l) => l.GetObjectType()).ThenBy((IIssue t) => t.GetDescription()));
				}
				break;
			default:
			{
				bool ordered = false;
				IProperty property = ViewboxApplication.FindProperty("table_order");
				if (property != null && property.Value.ToLower() == "true")
				{
					Dictionary<int, int> tableOrder = ViewboxApplication.Database.SystemDb.GetTableObjectOrder(ViewboxSession.User, TableType.Issue);
					if (tableOrder != null)
					{
						ordered = true;
						list = new List<IIssue>(temp.OrderBy((IIssue t) => tableOrder.ContainsKey(t.Id) ? tableOrder[t.Id] : (t.Ordinal + tableOrder.Count)));
					}
				}
				if (!ordered)
				{
					list = new List<IIssue>(from t in temp
						orderby t.Ordinal, t.GetDescription()
						select t);
				}
				break;
			}
			}
			return new List<IIssue>(list.Skip(page * size).Take(size));
		}

		public List<IIssue> GetIssueFromSessionIssues(int id)
		{
			return new List<IIssue>(ViewboxSession.Issues.Where((IIssue i) => i.Id == id));
		}
	}
}
