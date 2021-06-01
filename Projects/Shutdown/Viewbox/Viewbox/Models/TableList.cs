using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using SystemDb;
using Viewbox.Properties;
using ViewboxDb;

namespace Viewbox.Models
{
	public class TableList : ViewboxModel, ITableObjectList, ICategoryList
	{
		private ListOptionsModel _listOptions;

		private ListOptionsModel _visibleOptions;

		public IEnumerable<ITable> Tables { get; set; }

		public ITable SelectedTable => Tables.FirstOrDefault();

		public override string LabelCaption => Resources.Tables;

		public List<SelectListItem> SelectionList { get; set; }

		public ConcreteSelectionTypeFactory Selection { get; set; }

		public int VonBisOrdinal { get; set; }

		public bool IssuesCheck { get; set; }

		public bool RightsMode => base.Header.RightsMode;

		public TableType Type => TableType.Table;

		public ICategoryCollection Categories => ViewboxSession.TableCategories;

		public ICategory SelectedCategory { get; private set; }

		public string SearchPhrase { get; private set; }

		public IEnumerable<ITableObject> TableObjects => Tables;

		public ITableObject SelectedTableObject => SelectedTable;

		public string LabelSearch => Resources.TableSearch;

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

		public string LabelNumberOfRows => Resources.Tables;

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

		public TableList(int id = -1, string search = null, bool selectNoCategory = false)
		{
			if (!selectNoCategory)
			{
				SelectedCategory = ViewboxSession.GetTableCategoryById(id);
			}
			SearchPhrase = search?.ToLower();
			ViewboxSession.SearchTablePdfExport = search;
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

		public List<ITable> GetTableList(bool showEmpty = false, bool showHidden = false, int sortColumn = 0, SortDirection direction = SortDirection.Ascending, bool isPDFexport = false, bool showEmptyHidden = false)
		{
			ITableCollection tables = (isPDFexport ? ViewboxApplication.Database.SystemDb.Tables : ViewboxSession.Tables);
			List<ITable> list = new List<ITable>();
			if (showEmptyHidden)
			{
				list.AddRange(from t in tables
					where t.Database == SelectedSystem && (SearchPhrase == null || t.GetDescription().ToLower().Contains(SearchPhrase) || t.TableName.ToLower().Contains(SearchPhrase) || t.TransactionNumber.Contains(SearchPhrase)) && t.GetRowCount() < 1 && !t.IsVisible
					orderby t.Ordinal
					select t);
			}
			else if (showEmpty)
			{
				list.AddRange(from t in tables
					where t.Database == SelectedSystem && (SearchPhrase == null || t.GetDescription().ToLower().Contains(SearchPhrase) || t.TableName.ToLower().Contains(SearchPhrase) || t.TransactionNumber.Contains(SearchPhrase)) && t.GetRowCount() < 1 && t.IsVisible
					orderby t.Ordinal
					select t);
			}
			else if (showHidden)
			{
				list.AddRange(from t in tables
					where t.Database == SelectedSystem && (SearchPhrase == null || t.GetDescription().ToLower().Contains(SearchPhrase) || t.TableName.ToLower().Contains(SearchPhrase) || t.TransactionNumber.Contains(SearchPhrase)) && t.GetRowCount() > 0 && !t.IsVisible
					orderby t.Ordinal
					select t);
			}
			else
			{
				list.AddRange(from t in tables
					where t.Category.Id == SelectedCategory.Id && t.Database == SelectedSystem && (SearchPhrase == null || t.GetDescription().ToLower().Contains(SearchPhrase) || t.TableName.ToLower().Contains(SearchPhrase) || t.TransactionNumber.Contains(SearchPhrase)) && t.GetRowCount() > 0 && t.IsVisible
					orderby t.Ordinal
					select t);
			}
			if (sortColumn == 1)
			{
				if (direction == SortDirection.Ascending)
				{
					list = new List<ITable>(list.OrderBy((ITable l) => l.TransactionNumber));
				}
				if (direction == SortDirection.Descending)
				{
					list = new List<ITable>(list.OrderByDescending((ITable l) => l.TransactionNumber));
				}
			}
			if (sortColumn == 2)
			{
				if (direction == SortDirection.Ascending)
				{
					list = new List<ITable>(list.OrderBy((ITable l) => l.GetDescription()));
				}
				if (direction == SortDirection.Descending)
				{
					list = new List<ITable>(list.OrderByDescending((ITable l) => l.GetDescription()));
				}
			}
			if (sortColumn == 3)
			{
				if (direction == SortDirection.Ascending)
				{
					list = new List<ITable>(list.OrderBy((ITable l) => l.GetRowCount()));
				}
				if (direction == SortDirection.Descending)
				{
					list = new List<ITable>(list.OrderByDescending((ITable l) => l.GetRowCount()));
				}
			}
			return list;
		}

		public List<ITable> GetListFromSessionTables(int sortColumn = 0, SortDirection direction = SortDirection.Ascending, bool showEmpty = false)
		{
			List<ITable> list = new List<ITable>();
			switch (sortColumn)
			{
			case 1:
				if (direction == SortDirection.Ascending)
				{
					list = new List<ITable>(ViewboxSession.Tables.OrderBy((ITable t) => t.TransactionNumber));
				}
				if (direction == SortDirection.Descending)
				{
					list = new List<ITable>(ViewboxSession.Tables.OrderByDescending((ITable t) => t.TransactionNumber));
				}
				break;
			case 2:
				if (direction == SortDirection.Ascending)
				{
					list = new List<ITable>(ViewboxSession.Tables.OrderBy((ITable l) => l.GetDescription()));
				}
				if (direction == SortDirection.Descending)
				{
					list = new List<ITable>(ViewboxSession.Tables.OrderByDescending((ITable l) => l.GetDescription()));
				}
				break;
			case 3:
				if (showEmpty)
				{
					list = new List<ITable>(ViewboxSession.Tables.OrderBy((ITable t) => t.Ordinal));
					break;
				}
				if (direction == SortDirection.Ascending)
				{
					list = new List<ITable>(ViewboxSession.Tables.OrderBy((ITable t) => t.GetRowCount()));
				}
				if (direction == SortDirection.Descending)
				{
					list = new List<ITable>(ViewboxSession.Tables.OrderByDescending((ITable t) => t.GetRowCount()));
				}
				break;
			default:
			{
				bool ordered = false;
				IProperty property = ViewboxApplication.FindProperty("table_order");
				if (property != null && property.Value.ToLower() == "true")
				{
					Dictionary<int, int> tableOrder = ViewboxApplication.Database.SystemDb.GetTableObjectOrder(ViewboxSession.User, TableType.Table);
					if (tableOrder != null)
					{
						ordered = true;
						list = new List<ITable>(ViewboxSession.Tables.OrderBy((ITable t) => tableOrder.ContainsKey(t.Id) ? tableOrder[t.Id] : (t.Ordinal + tableOrder.Count)));
					}
				}
				if (!ordered)
				{
					list = new List<ITable>(from t in ViewboxSession.Tables
						orderby t.GetDescription(), t.Ordinal
						select t);
				}
				break;
			}
			}
			return list;
		}
	}
}
